using System.ComponentModel.DataAnnotations;
using UzlezzBlogs.Services;

namespace UzlezzBlogs.Pages;

[RequestAuth]
public class PreviewModel(IPostService postService) : PageModel
{
    [Required]
    [BindProperty(SupportsGet = true)]
    public string Markdown { get; set; } = string.Empty;

    public async Task<IActionResult> OnGet()
    {
        var result = await postService.GetPostPreview(Markdown, HttpContext.GetAuthToken()!);
        if (!result.IsSuccessStatusCode) return StatusCode(StatusCodes.Status503ServiceUnavailable);
        return Content(result.Content!);
    }
}
