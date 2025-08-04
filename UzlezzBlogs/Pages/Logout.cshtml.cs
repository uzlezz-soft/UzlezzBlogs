using UzlezzBlogs.Middleware;

namespace UzlezzBlogs.Pages;

[RequestAuth]
public class LogoutModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string ReturnUrl { get; set; } = string.Empty;

    public IActionResult OnGet()
    {
        Response.Cookies.Delete(Constants.JwtCookieName);

        return LocalRedirect(Url.IsLocalUrl(ReturnUrl) ? ReturnUrl : "/");
    }
}
