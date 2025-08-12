namespace UzlezzBlogs.Pages;

[RequestAuth]
public class MeModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public bool? ReCacheAvatar { get; set; }

    public void OnGet()
    {
    }
}
