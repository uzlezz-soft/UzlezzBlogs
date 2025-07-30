using Auth.Api.Entities;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using UzlezzBlogs.Core.Dto;

namespace Auth.Api.Interfaces;

public interface IUserService
{
    Task<string?> SignInWithPasswordAndGetJwtAsync(string userName, string password);
    Task<IEnumerable<IdentityError>?> RegisterAsync(string userName, string email, string password);
    Task SignOutAsync();
    Task<bool> ConfirmEmailAsync(string userId, string token);
    Task<Avatar?> GetAvatarAsync(string userName);
    Task<UserProfile?> GetProfileAsync(string userName);
}
