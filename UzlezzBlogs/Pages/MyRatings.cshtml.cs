using UzlezzBlogs.Core.Dto;

namespace UzlezzBlogs.Pages;

[RequestAuth]
public class MyRatingsModel(IPostService postService) : PageModel
{
    public required RatedPostPreview[] Posts { get; set; }

    [BindProperty(SupportsGet = true, Name = "p")]
    public int PageIndex { get; set; } = 1;

    public int TotalPages { get; set; } = 1;

    public async Task<IActionResult> OnGetAsync()
    {
        var result = await postService.GetRatedPosts(PageIndex, HttpContext.GetAuthToken()!);
        if (!result.IsSuccessStatusCode) return StatusCode(StatusCodes.Status503ServiceUnavailable);

        var list = result.Content!;
        if (PageIndex > list.TotalPages && list.TotalPages > 0)
            return LocalRedirect($"/MyRatings?p={list.TotalPages}");
        
        Posts = list.Posts;
        TotalPages = list.TotalPages;

        return Page();
    }
}
