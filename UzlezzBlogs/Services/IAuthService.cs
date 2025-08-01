using UzlezzBlogs.Core.Dto;

namespace UzlezzBlogs.Services;

public interface IAuthService
{
    [Get("/auth/avatar/{userName}")]
    Task<Avatar> GetAvatar(string userName);

    [Get("/auth/profile/{userName}")]
    Task<UserProfile> GetProfile(string userName);
}
