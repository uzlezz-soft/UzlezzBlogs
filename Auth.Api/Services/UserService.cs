using Auth.Api.Entities;
using Auth.Api.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
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

    public Task<bool> ConfirmEmailAsync(User user, string token, string host)
    {
        throw new NotImplementedException();
    }

    public Task<User?> GetUserAsync(ClaimsPrincipal claims)
    {
        throw new NotImplementedException();
    }

    public Task<User?> GetUserByIdAsync(string userId) => _userManager.FindByIdAsync(userId);

    public Task<User?> GetUserByNameAsync(string userName) => _userManager.FindByNameAsync(userName);

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
        await _signInManager.SignOutAsync();
    }
}
