using UzlezzBlogs.Core.Dto;
using UzlezzBlogs.Middleware;
using UzlezzBlogs.Services;

namespace UzlezzBlogs.Pages;

[RequestAuth(Required = false)]
public class PostModel(IPostService postService) : PageModel
{
    public PostDetails Details { get; set; }
    public PostComment[] Comments { get; set; }

    public async Task<IActionResult> OnGet(string post)
    {
        var details = await postService.GetPostDetails(post);
        if (!details.IsSuccessStatusCode)
            return details.StatusCode == HttpStatusCode.NotFound
                ? NotFound()
                : StatusCode(StatusCodes.Status503ServiceUnavailable);
        Details = details.Content!;

        var comments = await postService.GetPostComments(Details.Id, 0, Details.CommentCount);
        if (comments.IsSuccessStatusCode)
        {
            Comments = comments.Content!;
        }

        return Page();
    }
}
