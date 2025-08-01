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
using UzlezzBlogs.Microservices.Shared.Configs;

namespace Auth.Api.Services;

public class UserService : IUserService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly JwtConfig _config;
    private readonly AuthDbContext _context;
    private readonly ILogger<UserService> _logger;

    public UserService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IOptions<JwtConfig> config,
        AuthDbContext context,
        ILogger<UserService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _config = config.Value;
        _context = context;
        _logger = logger;
    }

    public async Task<bool> ConfirmEmailAsync(string userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return false;
        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (!result.Succeeded) return false;

        _logger.LogInformation("{UserName} confirmed email {Email}", user.UserName, user.Email);
        // TODO: send confirmation email

        return true;
    }

    public async Task<Avatar?> GetAvatarAsync(string userName)
    {
        userName = _userManager.NormalizeName(userName);

        return await _context.Users
            .Where(u => u.NormalizedUserName == userName)
            .Select(u => new Avatar(u.Avatar, u.AvatarMimeType))
            .FirstOrDefaultAsync();
    }

    public async Task<UserProfile?> GetProfileAsync(string userName)
    {
        userName = _userManager.NormalizeName(userName);

        return await _context.Users
            .Where(u => u.NormalizedUserName == userName)
            .Select(u => u.ToProfile())
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<IdentityError>?> RegisterAsync(string userName, string email, string password)
    {
        var user = new User() { Email = email, UserName = userName };
        var result = await _userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            _logger.LogInformation("User registered: {UserName} {Email}", user.UserName, user.Email);
            // TODO: send confirmation email

            return null;
        }
        return result.Errors;
    }

    public async Task<string?> SignInWithPasswordAndGetJwtAsync(string userName, string password)
    {
        var result = await _signInManager.PasswordSignInAsync(userName, password, false, false);
        if (!result.Succeeded) return null;

        var user = await _userManager.FindByNameAsync(userName);

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
        await _signInManager.SignOutAsync();
    }
}
