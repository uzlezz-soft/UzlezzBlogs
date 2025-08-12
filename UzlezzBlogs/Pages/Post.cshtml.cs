using UzlezzBlogs.Core.Dto;
using UzlezzBlogs.Services;

namespace UzlezzBlogs.Pages;

public class PostModel(IPostService postService) : PageModel
{
    public required PostDetails Details { get; set; }
    public required PostComment[] Comments { get; set; }

    public async Task<IActionResult> OnGet(string post)
    {
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
