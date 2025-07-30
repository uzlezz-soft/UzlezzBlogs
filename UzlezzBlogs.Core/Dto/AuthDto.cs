namespace UzlezzBlogs.Core.Dto;

public record RegisterRequest(string UserName, string Email, string Password);
public record RegisterResponse(string? Token, ICollection<string>? Errors);

public record LoginRequest(string UserName, string Password);
public record LoginResponse(string Token);

public record ConfirmEmailRequest(string UserId, string Token);