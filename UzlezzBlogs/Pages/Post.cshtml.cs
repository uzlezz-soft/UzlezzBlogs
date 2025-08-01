using UzlezzBlogs.Core.Dto;
using UzlezzBlogs.Services;

namespace UzlezzBlogs.Pages;

public class PostModel(IPostService postService) : PageModel
{
    public PostDetails Details { get; set; }
    public PostComment[] Comments { get; set; }

    public async Task<IActionResult> OnGet(string post)
    {
        Details = await postService.GetPostDetails(post);
        Comments = await postService.GetPostComments(Details.Id, 0, Details.CommentCount);

        return Page();
    }
}
