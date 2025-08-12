using UzlezzBlogs.Services;

namespace UzlezzBlogs.Pages;

[RequestAuth]
public class ConfirmEmailModel(IAuthService authService) : PageModel
{
    [BindProperty(Name = "token", SupportsGet = true)]
    public string Token { get; set; } = string.Empty;

    public async Task<IActionResult> OnGet()
    {
        var result = await authService.ConfirmEmail(Token.Replace(' ', '+'), HttpContext.GetAuthToken()!);
        return LocalRedirect(result.IsSuccessStatusCode ? "/me" : "/");
    }
}
