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

    [Get("/auth/confirmEmail")]
    Task<IApiResponse> ConfirmEmail([Query] string token, [Authorize] string authToken);
    [Post("/auth/resendEmailConfirmation")]
    Task<IApiResponse> ResendEmailConfirmation([Authorize] string authToken);

    [Get("/auth/profile")]
    Task<IApiResponse<UserProfileDetails>> GetProfileDetails([Authorize] string token);
    [Post("/auth/editProfile")]
    Task<IApiResponse> EditProfile([Query] string description, [Authorize] string token);
    [Multipart]
    [Post("/auth/uploadAvatar")]
    Task<IApiResponse> UploadAvatar([AliasAs("file")] StreamPart stream, [Authorize] string token);
}
