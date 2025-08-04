using UzlezzBlogs.Core.Dto;

namespace UzlezzBlogs.Services;

public interface IAuthService
{
    [Post("/auth/register")]
    Task<IApiResponse<LoginResponse>> Register([Body] RegisterRequest request);
    [Post("/auth/login")]
    Task<IApiResponse<LoginResponse>> Login([Body] LoginRequest request);

    [Get("/auth/avatar/{userName}")]
    Task<IApiResponse<Avatar>> GetAvatar(string userName);

    [Get("/auth/profile/{userName}")]
    Task<IApiResponse<UserProfile>> GetProfile(string userName);
}
