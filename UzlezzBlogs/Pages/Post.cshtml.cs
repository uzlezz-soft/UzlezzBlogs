using UzlezzBlogs.Core.Dto;

namespace UzlezzBlogs.Pages;

public class PostModel(IPostService postService) : PageModel
{
    public required PostDetails Details { get; set; }
    public required PostComment[] Comments { get; set; }

    [BindProperty(Name = "cl", SupportsGet = true)]
    public int ClearLocalStorage { get; set; } = 0;

    public async Task<IActionResult> OnGet(string post)
    {
        if (ClearLocalStorage == 1)
        {
            Details = new PostDetails(string.Empty, string.Empty, string.Empty, post, string.Empty, DateTime.UtcNow,
                string.Empty, 0, 0, null, 0, 0);
            return Page();
        }

        var details = await postService.GetPostDetails(post,
            HttpContext.IsAuthorized() ? HttpContext.GetAuthToken() : null);
        if (!details.IsSuccessStatusCode)
            return details.StatusCode == HttpStatusCode.NotFound
                ? NotFound()
                : StatusCode(StatusCodes.Status503ServiceUnavailable);
        Details = details.Content!;

        if (Details.CommentCount > 0)
        {
            var comments = await postService.GetPostComments(Details.Id, 0, Details.CommentCount);
            if (comments.IsSuccessStatusCode)
            {
                Comments = comments.Content!;
            }
        }
        else Comments = [];

        return Page();
    }
}
