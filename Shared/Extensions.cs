using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using UzlezzBlogs.Core.Configs;
using UzlezzBlogs.Microservices.Shared.Configs;

namespace UzlezzBlogs.Microservices.Shared;

public static class Extensions
{
    public static IServiceCollection ConfigureMessageBroker(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqConfig>(configuration.GetSection(RabbitMqConfig.RabbitMq));
        services.AddSingleton<IMessageBroker, RabbitMqService>();
        return services;
    }

    public static IServiceCollection ConfigureDatabaseContext<T>(this IServiceCollection services, string connectionString)
        where T : DbContext
    {
        services.AddDbContextPool<T>(options =>
        {
            options.UseNpgsql(connectionString);
            options.ConfigureWarnings(w => w.Throw(RelationalEventId.MultipleCollectionIncludeWarning));
        });
        return services;
    }

    public static IServiceCollection AddJwtAuth(this IServiceCollection services, ConfigurationManager configuration)
    {
        services.Configure<JwtConfig>(configuration.GetSection(JwtConfig.Jwt));
        var config = configuration.GetSection(JwtConfig.Jwt).Get<JwtConfig>()!;

        var key = Encoding.UTF8.GetBytes(config.Key);
        services.AddAuthentication(opt =>
        {
            opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
            .AddJwtBearer(opt =>
            {
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = config.Issuer,
                    ValidAudience = config.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                };
            });
        services.AddAuthorization();
        return services;
    }
}
