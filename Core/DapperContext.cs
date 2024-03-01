using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Core;

public class DapperContext
{
    private readonly string _connectionString;
    
    public DapperContext(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("postgres") ?? throw new NotImplementedException("not found postgres");
    }
    
    public IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);
}