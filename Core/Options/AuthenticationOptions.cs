using Microsoft.Extensions.Configuration;

namespace Core.Options;

public class AuthenticationOptions: OptionsProviderBase
{
    protected override string MainSectionName => "AuthenticationOptions";

    public AuthenticationOptions(IConfiguration configuration) : base(configuration)
    {
    }

    public int ExpirationMinutes => GetInt("ExpirationMinutes", 90);

    public string Issuer => GetString("Issuer", "issuer");
    public string Audience => GetString("Audience", "Audience");

    /// <summary>
    /// must be a long string.
    /// https://stackoverflow.com/questions/47279947/idx10603-the-algorithm-hs256-requires-the-securitykey-keysize-to-be-greater
    /// </summary>
    public string SecurityKey => GetString("SecurityKey", "401b09eab3c013d4ca54922bb802bec8fd5318192b0a75f201d8b3727429090fb337591abd3e44453b954555b7a0812e1081c39b740293f765eae731f5a65ed1");
}