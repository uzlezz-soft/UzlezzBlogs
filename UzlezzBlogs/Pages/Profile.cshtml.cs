using UzlezzBlogs.Core.Dto;

namespace UzlezzBlogs.Pages;

public class ProfileModel(IPostService postService, IAuthService authService) : PageModel
{
    [BindProperty(SupportsGet = true, Name = "Page")]
    public int PageIndex { get; set; } = 1;

    public required UserProfile Profile { get; set; }
    public required PostPreview[] Posts { get; set; }
    public int TotalPages { get; set; } = 1;

    public async Task<IActionResult> OnGet(string userName)
    {
        var profile = await authService.GetProfile(userName);
        if (!profile.IsSuccessStatusCode)
            return profile.StatusCode == HttpStatusCode.NotFound ? NotFound() : StatusCode(StatusCodes.Status503ServiceUnavailable);

        Profile = profile.Content!;

        var result = await postService.GetUserPosts(userName, PageIndex);
        if (!result.IsSuccessStatusCode) return StatusCode(StatusCodes.Status503ServiceUnavailable);
        var list = result.Content!;

        if (PageIndex > list.TotalPages && list.TotalPages > 0)
            return LocalRedirect($"/profile/{userName}?page={list.TotalPages}");

        Posts = list.Posts;
        TotalPages = list.TotalPages;

        return Page();
    }
}
