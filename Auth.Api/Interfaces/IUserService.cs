using Auth.Api.Entities;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Auth.Api.Interfaces;

public interface IUserService
{
    Task<string?> SignInWithPasswordAndGetJwtAsync(string userName, string password);
    Task<IEnumerable<IdentityError>?> RegisterAsync(string userName, string email, string password);
    Task SignOutAsync();
    Task<bool> ConfirmEmailAsync(User user, string token, string host);
    Task<User?> GetUserAsync(ClaimsPrincipal claims);
    Task<User?> GetUserByNameAsync(string userName);
    Task<User?> GetUserByIdAsync(string userId);
}
