using Microsoft.AspNetCore.Http.Features;

namespace UzlezzBlogs.Middleware;

public class TokenMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.Features.Get<IEndpointFeature>()?.Endpoint;
        var attribute = endpoint?.Metadata.GetMetadata<RequestAuthAttribute>();
        if (attribute is not null)
        {
            var cookie = context.Request.Cookies[Constants.JwtCookieName];

            if (cookie is null && attribute.Required)
            {
                context.Response.StatusCode = 401;
                return;
            }

            context.Items[Constants.Token] = cookie;
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

    public static string? GetAuthToken(this HttpContext context)
        => context.Items[Constants.Token] as string;

    public static bool IsAuthorized(this HttpContext context)
        => context.Items[Constants.Token] is not null;
}
