using System.Text.Json;

namespace Core.Extensions;

public static class JsonHelper
{
    public static T? EncodeNull<T>(this string json)
    {
        T? obj = default(T);
        try
        {
            if (!string.IsNullOrEmpty(json))
            {
                obj = JsonSerializer.Deserialize<T>(json, JsonDeserializeOptions);
            }
        }
        catch
        {
            // ignored
        }

        return obj;
    }

    public static T Encode<T>(this string json) where T : new()
    {
        T obj = new T();
        try
        {
            if (!string.IsNullOrEmpty(json))
            {
                var des = JsonSerializer.Deserialize<T>(json, JsonDeserializeOptions);
                if (des != null)
                {
                    obj = des;
                }
            }
        }
        catch
        {
            // ignored
        }

        return obj;
    }

    public static bool TryEncode<T>(this string json, out T obj) where T : new()
    {
        obj = new T();
        try
        {
            if (!string.IsNullOrEmpty(json))
            {
                var des = JsonSerializer.Deserialize<T>(json, JsonDeserializeOptions);
                if (des != null)
                {
                    obj = des;
                }
            }
        }
        catch
        {
            return false;
        }

        return true;
    }

    public static T[] EncodeArray<T>(this string json)
    {
        var obj = Array.Empty<T>();
        try
        {
            if (!string.IsNullOrEmpty(json))
            {
                var arr = JsonSerializer.Deserialize<T[]>(json, JsonDeserializeOptions);
                if (arr != null)
                {
                    obj = arr;
                }
            }
        }
        catch
        {
            // ignored
        }

        return obj;
    }

    public static string Decode<T>(this T? data)
    {
        return data != null
            ? JsonSerializer.Serialize(data,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
            : string.Empty;
    }

    public static string? DecodeNull<T>(this T? data)
    {
        return data != null
            ? JsonSerializer.Serialize(data,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
            : null;
    }

    public static bool IsValid<T>(this string json)
    {
        try
        {
            if (!string.IsNullOrEmpty(json))
            {
                JsonSerializer.Deserialize<T>(json, JsonDeserializeOptions);
                return true;
            }
        }
        catch
        {
            return false;
        }

        return false;
    }

    private static readonly JsonSerializerOptions JsonDeserializeOptions = new()
    {
        //Converters = { new BoolConverter(), new BoolNullConverter(), new IntConverter(), new IntNullConverter() },
        PropertyNameCaseInsensitive = true
    };
}
