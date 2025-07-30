namespace UzlezzBlogs.Core.Dto;

public record RegisterRequest(string UserName, string Email, string Password);

public record LoginRequest(string UserName, string Password);
public record LoginResponse(string Token);

public record Avatar(string AvatarData, string AvatarMimeType);

public record UserProfile(string UserName, string DescriptionHtml);