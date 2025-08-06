namespace UzlezzBlogs.Services;

public interface ITokenValidatorService
{
    public Task<AuthorizedUser?> ValidateAsync(string token);
}
