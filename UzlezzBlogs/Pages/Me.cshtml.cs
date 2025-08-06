using UzlezzBlogs.Middleware;

namespace UzlezzBlogs.Pages;

[RequestAuth]
public class MeModel : PageModel
{
    public void OnGet()
    {
    }
}
