using System.IdentityModel.Tokens.Jwt;
using Confluent.Kafka;
using Core.Extensions;
using Core.Kafka;
using Core.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Core;

public static class ServiceCollections
{
    public static void AddCoreBase(this IServiceCollection services, IConfiguration cfg)
    {
        services.AddSingleton(cfg);
        var authOptions = new AuthenticationOptions(cfg);
        services.AddSingleton(authOptions);
        services.AddSingleton<IKafkaOptions, KafkaOptions>();
        
        services.AddSingleton<KafkaClientHandle>();
        services.AddSingleton<KafkaDependentProducer<Null, string>>();
        services.AddSingleton<KafkaDependentProducer<string, long>>();
        services.AddSingleton<KafkaDependentProducer<string, string>>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = authOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = authOptions.Audience,
                    ValidateLifetime = true,
                    IssuerSigningKey = authOptions.SecurityKey.GetSymmetricSecurityKey(),
                    ValidateIssuerSigningKey = true,
                };
            });

        services.AddSingleton<JwtSecurityTokenHandler>();
    }
}