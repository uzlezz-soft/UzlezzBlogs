using UzlezzBlogs.Core.Dto;
using UzlezzBlogs.Services;

namespace UzlezzBlogs.Pages;

public class ProfileModel(IPostService postService, IAuthService authService) : PageModel
{
    [BindProperty(SupportsGet = true, Name = "Page")]
    public int PageIndex { get; set; } = 1;

    public UserProfile Profile { get; set; }
    public PostPreview[] Posts { get; set; }
    public int TotalPages { get; set; } = 1;

    public async Task<IActionResult> OnGet(string userName)
    {
        Profile = await authService.GetProfile(userName);

        var list = await postService.GetUserPosts(userName, PageIndex);
        if (PageIndex > list.TotalPages && list.TotalPages > 0)
            return LocalRedirect($"/profile/{userName}?page={list.TotalPages}");

        Posts = list.Posts;
        TotalPages = list.TotalPages;

        return Page();
    }
}
