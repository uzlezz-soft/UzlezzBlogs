namespace UzlezzBlogs.Core.Dto;

public record RegisterRequest(string UserName, string Email, string Password);

public record LoginRequest(string UserName, string Password);
public record LoginResponse(string Token);

public record ConfirmEmailRequest(string UserId, string Token);

public record AvatarResponse(string Avatar, string AvatarMimeType);