using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UzlezzBlogs.Core.Configs;

namespace UzlezzBlogs.Middleware;

public class TokenMiddleware(IOptions<JwtConfig> config) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var endpoint = context.Features.Get<IEndpointFeature>()?.Endpoint;
        var attribute = endpoint?.Metadata.GetMetadata<RequestAuthAttribute>();

        var cookie = context.Request.Cookies[Constants.JwtCookieName];
        if (cookie is null)
        {
            if (attribute is not null)
            {
                context.Response.StatusCode = 401;
                return;
            }
            await next(context);
            return;
        }
        context.Items[Constants.Token] = cookie;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.Value.Key));

        var result = await new JwtSecurityTokenHandler().ValidateTokenAsync(cookie, new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = config.Value.Issuer,
            ValidAudience = config.Value.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ClockSkew = TimeSpan.Zero
        });
        if (!result.IsValid)
        {
            context.Items.Remove(Constants.Token);
            context.Response.Cookies.Delete(Constants.JwtCookieName);

            if (attribute is not null)
            {
                context.Response.StatusCode = 401;
                return;
            }
            await next(context);
            return;
        }

        var id = result.ClaimsIdentity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
        var name = result.ClaimsIdentity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
        var email = result.ClaimsIdentity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;

        if (id is null || name is null || email is null)
        {
            context.Items.Remove(Constants.Token);
            context.Response.Cookies.Delete(Constants.JwtCookieName);

            if (attribute is not null)
            {
                context.Response.StatusCode = 401;
                return;
            }
        }
        else
        {
            context.Items[Constants.AuthorizedUser] = new AuthorizedUser(id!, name!, email!);
        }

        await next(context);
    }
}

public static class TokenMiddlewareExtensions
{
    public static IApplicationBuilder UseTokenMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TokenMiddleware>();
    }

    public static IServiceCollection AddTokenMiddleware(this IServiceCollection collection)
    {
        collection.AddTransient<TokenMiddleware>();
        return collection;
    }

    public static string? GetAuthToken(this HttpContext context)
        => context.Items[Constants.Token] as string;

    public static bool IsAuthorized(this HttpContext context)
        => context.Items[Constants.Token] is not null;

    public static AuthorizedUser? GetUser(this HttpContext context)
        => context.Items[Constants.AuthorizedUser] as AuthorizedUser;
}
