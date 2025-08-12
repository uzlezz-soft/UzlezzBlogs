using Microsoft.AspNetCore.Http.Features;

namespace UzlezzBlogs.Middleware;

public class TokenMiddleware(ITokenValidatorService tokenValidator) : IMiddleware
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
                context.Response.Redirect($"/login?returnUrl={context.Request.Path}");
                return;
            }
            await next(context);
            return;
        }
        context.Items[Constants.Token] = cookie;

        var user = await tokenValidator.ValidateAsync(cookie);
        if (user is null)
        {
            context.Items.Remove(Constants.Token);
            context.Response.Cookies.Delete(Constants.JwtCookieName);

            if (attribute is not null)
            {
                context.Response.Redirect($"/login?returnUrl={context.Request.Path}");
                return;
            }
            await next(context);
            return;
        }
        context.Items[Constants.AuthorizedUser] = user;

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
