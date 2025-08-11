using Auth.Api.Entities;
using Auth.Api.Interfaces;
using Auth.Api.Mapping;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UzlezzBlogs.Core.Dto;
using UzlezzBlogs.Core.Configs;
using UzlezzBlogs.Microservices.Shared;
using UzlezzBlogs.Microservices.Shared.Messages;

namespace Auth.Api.Services;

public class UserService(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    IOptions<JwtConfig> config,
    AuthDbContext context,
    IMessageBroker messageBroker,
    ILogger<UserService> logger) : IUserService
{
    private readonly JwtConfig _config = config.Value;

    public async Task<bool> ConfirmEmailAsync(string userId, string token)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return false;
        var result = await userManager.ConfirmEmailAsync(user, token);
        if (!result.Succeeded) return false;

        logger.LogInformation("{UserName} confirmed email", user.UserName);
        await messageBroker.Publish(new UserEmailConfirmed
        {
            UserName = user.UserName!,
            Email = user.Email!
        });

        return true;
    }

    public async Task<Avatar?> GetAvatarAsync(string userName)
    {
        userName = userManager.NormalizeName(userName);

        return await context.Users
            .Where(u => u.NormalizedUserName == userName)
            .Select(u => new Avatar(u.Avatar, u.AvatarMimeType))
            .FirstOrDefaultAsync();
    }

    public async Task<UserProfile?> GetProfileAsync(string userName)
    {
        userName = userManager.NormalizeName(userName);

        return await context.Users
            .Where(u => u.NormalizedUserName == userName)
            .Select(u => u.ToProfile())
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<IdentityError>?> RegisterAsync(string userName, string email, string password)
    {
        var user = new User() { Email = email, UserName = userName };
        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded) return result.Errors;

        logger.LogInformation("User registered: {UserName}", user.UserName);
        await messageBroker.Publish(new Notification
        {
            TargetUserName = user.UserName,
            TargetEmail = user.Email,
            Type = "confirm_email",
            Parameters = new()
            {
                ["token"] = await userManager.GenerateEmailConfirmationTokenAsync(user)
            }
        });

        return null;
    }

    public async Task<string?> SignInWithPasswordAndGetJwtAsync(string userName, string password)
    {
        var result = await signInManager.PasswordSignInAsync(userName, password, false, false);
        if (!result.Succeeded) return null;

        var user = await userManager.FindByNameAsync(userName);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user!.Id),
            new Claim(ClaimTypes.Name, user!.UserName!),
            new Claim(ClaimTypes.Email, user!.Email!),
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config.Issuer,
            audience: _config.Audience,
            claims: claims,
            expires: DateTime.Now.AddDays(_config.LifetimeDays),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task SignOutAsync()
    {
        // TODO: maybe add user's token to db list of invalid tokens
        // But how would other microservices check it?
        // Message broker notification?
        await signInManager.SignOutAsync();
    }
}
