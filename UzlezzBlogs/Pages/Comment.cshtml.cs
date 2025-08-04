using UzlezzBlogs.Middleware;
using UzlezzBlogs.Services;

namespace UzlezzBlogs.Pages;

[RequestAuth]
public class CommentModel(IPostService postService) : PageModel
{
    [BindProperty]
    public string Content { get; set; } = string.Empty;

    public async Task<IActionResult> OnPost(string postId)
    {
        var comment = await postService.Comment(postId, Content, HttpContext.GetAuthToken()!);
        if (!comment.IsSuccessStatusCode)
        {
            return comment.StatusCode switch
            {
                HttpStatusCode.BadRequest => BadRequest(),
                HttpStatusCode.NotFound => NotFound(),
                _ => StatusCode(StatusCodes.Status503ServiceUnavailable)
            };
        }

        return new JsonResult(comment.Content!);
    }
}
