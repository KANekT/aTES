using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Reflection;
using System.Text;
using Dapper;

namespace Core;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    readonly IDbConnection _connection;

    protected GenericRepository(DapperContext context)
    {
        _connection = context.CreateConnection();
    }

    public async Task<bool> Add(T entity, CancellationToken ctx)
    {
        _connection.Open();
        
        var rowsEffected = 0;
        try
        {
            var (schemaName, tableName) = GetSchemaTableName();
            var columns    = GetColumns(excludeKey: true);
            var properties = GetPropertyNames(excludeKey: true);
            var query      = $"INSERT INTO {schemaName}.{tableName} ({columns}) VALUES ({properties})";

            rowsEffected = await _connection.ExecuteAsync(query, entity);
        }
        catch (Exception ex)
        {
            // ignored
        }
        
        _connection.Close();
        return rowsEffected > 0;
    }

    public async Task<bool> Delete(T entity, CancellationToken ctx)
    {
        _connection.Open();
        
        var rowsEffected = 0;
        try
        {
            var (schemaName, tableName) = GetSchemaTableName();
            var keyColumn   = GetKeyColumnName();
            var keyProperty = GetKeyPropertyName();
            var query       = $"DELETE FROM {schemaName}.{tableName} WHERE {keyColumn} = @{keyProperty}";

            rowsEffected = await _connection.ExecuteAsync(query, entity);
        }
        catch (Exception ex)
        {
            // ignored
        }

        _connection.Close();
        return rowsEffected > 0;
    }

    public async Task<IEnumerable<T>> GetAll(CancellationToken ctx)
    {
        _connection.Open();
        
        IEnumerable<T> result = Array.Empty<T>();
        try
        {
            var (schemaName, tableName) = GetSchemaTableName();
            var query     = $"SELECT * FROM {schemaName}.{tableName}";

            result = await _connection.QueryAsync<T>(query);
        }
        catch (Exception ex)
        {
            // ignored
        }

        _connection.Close();
        return result;
    }

    public async Task<T?> GetByKey(long id, CancellationToken ctx)
    {
        _connection.Open();
        
        IEnumerable<T> result = Array.Empty<T>();
        try
        {
            var (schemaName, tableName) = GetSchemaTableName();
            var keyColumn = GetKeyColumnName();
            var query     = $"SELECT * FROM {schemaName}.{tableName} WHERE {keyColumn} = '{id}'";

            result = await _connection.QueryAsync<T>(query);
        }
        catch (Exception ex)
        {
            // ignored
        }

        _connection.Close();
        return result.FirstOrDefault();
    }

    public async Task<T?> GetByPublicId(string id, CancellationToken ctx)
    {
        _connection.Open();
        
        IEnumerable<T> result = Array.Empty<T>();
        try
        {
            var (schemaName, tableName) = GetSchemaTableName();
            var keyColumn = "Ulid";
            var query     = $"SELECT * FROM {schemaName}.{tableName} WHERE \"{keyColumn}\" = '{id}'";

            result = await _connection.QueryAsync<T>(query);
        }
        catch (Exception ex)
        {
            // ignored
        }

        _connection.Close();
        return result.FirstOrDefault();
    }
    
    public async Task<bool> Update(T entity, CancellationToken ctx)
    {
        _connection.Open();
        
        var rowsEffected = 0;
        try
        {
            var (schemaName, tableName) = GetSchemaTableName();
            var keyColumn   = GetKeyColumnName();
            var keyProperty = GetKeyPropertyName();

            var query = new StringBuilder();
            query.Append($"UPDATE {schemaName}.{tableName} SET ");

            foreach (var property in GetProperties(true))
            {
                var columnAttr = property.GetCustomAttribute<ColumnAttribute>();

                if (columnAttr == null)
                {
                    continue;
                }
                
                var propertyName = property.Name;
                var columnName   = columnAttr.Name;

                query.Append($"\"{columnName}\" = @{propertyName},");
            }

            query.Remove(query.Length - 1, 1);

            query.Append($" WHERE \"{keyColumn}\" = @{keyProperty}");

            rowsEffected = await _connection.ExecuteAsync(query.ToString(), entity);
        }
        catch (Exception ex)
        {
            // ignored
        }

        _connection.Close();
        return rowsEffected > 0;
    }

    private Tuple<string, string> GetSchemaTableName()
    {
        var type = typeof(T);
        var tableAttr = type.GetCustomAttribute<TableAttribute>();
        var schemaName = "public";
        var tableName = type.Name + "s";
        if (tableAttr == null)
            return new Tuple<string, string>($"\"{schemaName}\"", $"\"{tableName}\"");
        
        schemaName = tableAttr.Schema ?? "public";
        tableName = tableAttr.Name;

        return new Tuple<string, string> ($"\"{schemaName}\"", $"\"{tableName}\"");
    }
    
    private string GetSchemaName()
    {
        var type = typeof(T);
        var tableAttr = type.GetCustomAttribute<TableAttribute>();
        var schemaName = tableAttr == null ? type.Name + "s" : tableAttr.Schema ?? type.Name + "s";
        return $"\"{schemaName}\"";
    }
    
    private string GetTableName()
    {
        var type = typeof(T);
        var tableAttr = type.GetCustomAttribute<TableAttribute>();
        if (tableAttr == null)
            return type.Name + "s";
        var tableName = tableAttr.Name;
        return $"\"{tableName}\"";
    }

    private static string? GetKeyColumnName()
    {
        var properties = typeof(T).GetProperties();

        foreach (var property in properties)
        {
            var keyAttributes = property.GetCustomAttributes(typeof(KeyAttribute), true);

            if (keyAttributes.Length <= 0) continue;
            var columnAttributes = property.GetCustomAttributes(typeof(ColumnAttribute), true);

            if (columnAttributes.Length <= 0) return property.Name;
            var columnAttribute = (ColumnAttribute)columnAttributes[0];
            return columnAttribute.Name;
        }

        return null;
    }


    private string GetColumns(bool excludeKey = false)
    {
        var type = typeof(T);
        var columns = string.Join(", ", type.GetProperties()
            .Where(p => !excludeKey || !p.IsDefined(typeof(KeyAttribute)))
            .Select(p =>
            {
                var columnAttr = p.GetCustomAttribute<ColumnAttribute>();
                var columnName= columnAttr != null ? columnAttr.Name : p.Name;
                return $"\"{columnName}\"";
            }));

        return columns;
    }

    private string GetPropertyNames(bool excludeKey = false)
    {
        var properties = typeof(T).GetProperties()
            .Where(p => !excludeKey || p.GetCustomAttribute<KeyAttribute>() == null);

        var values = string.Join(", ", properties.Select(p => $"@{p.Name}"));

        return values;
    }

    private IEnumerable<PropertyInfo> GetProperties(bool excludeKey = false)
    {
        var properties = typeof(T).GetProperties()
            .Where(p => !excludeKey || p.GetCustomAttribute<KeyAttribute>() == null);

        return properties;
    }

    private string? GetKeyPropertyName()
    {
        var properties = typeof(T).GetProperties()
            .Where(p => p.GetCustomAttribute<KeyAttribute>() != null).ToArray();

        return properties.Any() ? properties.FirstOrDefault()?.Name : null;
    }
}