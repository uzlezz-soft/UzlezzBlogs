using System.ComponentModel.DataAnnotations;
using UzlezzBlogs.Core.Dto;

namespace UzlezzBlogs.Pages;

[RequestAuth]
public class CreateModel(IPostService postService) : PageModel
{
    [Required]
    [StringLength(255, MinimumLength = 10)]
    [BindProperty]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(500, MinimumLength = 0)]
    [BindProperty]
    public string Description { get; set; } = string.Empty;

    [Required]
    [StringLength(int.MaxValue, MinimumLength = 100)]
    [BindProperty]
    public string Markdown { get; set; } = string.Empty;

    public IActionResult OnGet()
    {
        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
            return Page();

        var request = new PostCreateRequest(Title, Description, Markdown);
        var result = await postService.CreatePost(request, HttpContext.GetAuthToken()!);
        if (!result.IsSuccessStatusCode) return StatusCode(StatusCodes.Status503ServiceUnavailable);

        return LocalRedirect($"/post/{result.Content!.Url}");
    }
}
