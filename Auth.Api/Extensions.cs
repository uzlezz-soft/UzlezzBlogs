using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Auth.Api;

public static class Extensions
{
    public static IServiceCollection ConfigureIdentity<TUser, TEntityFrameworkStore>(this IServiceCollection services)
        where TUser : IdentityUser
        where TEntityFrameworkStore : IdentityDbContext<TUser>
    {
        services.AddIdentity<TUser, IdentityRole>(options =>
        {
            options.SignIn.RequireConfirmedAccount = false;
            options.User.RequireUniqueEmail = true;
            options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_-";
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireDigit = false;
        })
            .AddEntityFrameworkStores<TEntityFrameworkStore>()
            .AddTokenProvider<DataProtectorTokenProvider<TUser>>(TokenOptions.DefaultProvider);
        return services;
    }
}
