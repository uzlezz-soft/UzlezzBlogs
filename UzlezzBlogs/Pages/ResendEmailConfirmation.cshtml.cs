using Microsoft.AspNetCore.RateLimiting;

namespace UzlezzBlogs.Pages;

[RequestAuth]
[EnableRateLimiting("resend_email_confirmation")]
public class ResendEmailConfirmationModel(
    IAuthService authService,
    ILogger<ResendEmailConfirmationModel> logger) : PageModel
{
    public async Task<IActionResult> OnPost()
    {
        logger.LogInformation("User {User} requested email confirmation", HttpContext.GetUser()!.Name);

        var result = await authService.ResendEmailConfirmation(HttpContext.GetAuthToken()!);
        return result.IsSuccessStatusCode ? StatusCode(StatusCodes.Status200OK)
            : result.StatusCode == HttpStatusCode.Unauthorized 
            ? Unauthorized() : StatusCode(StatusCodes.Status503ServiceUnavailable);
    }
}
