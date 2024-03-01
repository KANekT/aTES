using Core.Extensions;
using Microsoft.Extensions.Configuration;

namespace Core.Options;

public abstract class OptionsProviderBase
{
    private readonly IConfiguration _configuration;

    protected abstract string MainSectionName { get; }

    protected OptionsProviderBase(IConfiguration configuration)
    {
        _configuration = configuration.ThrowIfNull();
    }

    public string GetConnectionString(string name) => _configuration.GetConnectionString(name) ??
                                                      throw new ArgumentException("Connection String cannot be null");

    protected IConfigurationSection GetSection(string sectionName)
    {
        var path = MainSectionName.IsNullOrEmpty() ? sectionName : $"{MainSectionName}:{sectionName}";
        return _configuration.GetSection(path);
    }

    protected string? GetString(string sectionName)
    {
        var path = MainSectionName.IsNullOrEmpty() ? sectionName : $"{MainSectionName}:{sectionName}";
        return _configuration.GetSection(path)?.Value;
    }

    protected string GetString(string sectionName, string defValue)
    {
        var val = GetString(sectionName);
        return string.IsNullOrEmpty(val) ? defValue : val;
    }

    protected bool GetBool(string sectionName, bool defaultValue = false)
    {
        if (GetString(sectionName) != null && bool.TryParse(GetString(sectionName), out var result))
        {
            return result;
        }

        return defaultValue;
    }

    protected int GetInt(string sectionName, int defaultVal)
    {
        var val = GetString(sectionName);
        if (val != null && int.TryParse(val, out var result))
        {
            return result;
        }

        return defaultVal;
    }

    protected int? GetIntNull(string sectionName)
    {
        var val = GetString(sectionName);
        if (val != null && int.TryParse(val, out var result))
        {
            return result;
        }

        return null;
    }

    protected int GetIntThrowIfNull(string sectionName)
    {
        var val = GetString(sectionName).ThrowIfNull();
        return int.TryParse(val, out var result) ? result : 0;
    }

    protected Guid GetGuid(string sectionName, Guid defaultVal)
    {
        var val = GetString(sectionName);
        if (val != null && Guid.TryParse(val, out var result))
        {
            return result;
        }

        return defaultVal;
    }

    protected Guid? GetGuidNull(string sectionName)
    {
        var val = GetString(sectionName);
        if (val != null && Guid.TryParse(val, out var result))
        {
            return result;
        }

        return null;
    }

    protected TEnum GetEnum<TEnum>(string sectionName, TEnum defaultVal) where TEnum : struct, Enum
    {
        return Enum.TryParse(GetString(sectionName), true, out TEnum val) ? val : defaultVal;
    }

    protected string[] GetStrings(string sectionName)
    {
        var path = MainSectionName.IsNullOrEmpty() ? sectionName : $"{MainSectionName}:{sectionName}";
        var strings = _configuration.GetSection(path);

        return strings.Get<string[]>() ?? Array.Empty<string>();
    }
}