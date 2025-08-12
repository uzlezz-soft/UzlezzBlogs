using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using UzlezzBlogs.Core.Dto;
using UzlezzBlogs.Services;

namespace UzlezzBlogs.Pages;

[RequestAuth]
public class EditModel(IPostService postService) : PageModel
{
    [BindProperty]
    [ValidateNever]
    public required string Id { get; set; }

    [ValidateNever]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(500, MinimumLength = 0)]
    [BindProperty]
    public string Description { get; set; } = string.Empty;

    [Required]
    [StringLength(int.MaxValue, MinimumLength = 100)]
    [BindProperty]
    public string Markdown { get; set; } = string.Empty;

    public required string ContentHtml { get; set; }

    [BindProperty(Name = "Url")]
    [ValidateNever]
    public required string PostUrl { get; set; }

    public async Task<IActionResult> OnGet(string id)
    {
        Id = id;
        var result = await postService.GetPostContent(id, HttpContext.GetAuthToken()!);
        if (!result.IsSuccessStatusCode)
        {
            if (result.StatusCode == HttpStatusCode.BadRequest)
                return LocalRedirect("/");
            return StatusCode(StatusCodes.Status503ServiceUnavailable);
        }

        Title = result.Content!.Title;
        Description = result.Content!.Description;
        Markdown = result.Content!.Content;
        ContentHtml = result.Content!.HtmlContent;
        PostUrl = result.Content!.Url;
        return Page();
    }

    public async Task<IActionResult> OnPost(string id)
    {
        if (!ModelState.IsValid)
            return Page();

        var request = new PostEditRequest(Id, Description, Markdown);
        var result = await postService.EditPost(request, HttpContext.GetAuthToken()!);
        if (!result.IsSuccessStatusCode)
        {
            if (result.StatusCode == HttpStatusCode.BadRequest)
                return LocalRedirect("/");
            return StatusCode(StatusCodes.Status503ServiceUnavailable);
        }

        return LocalRedirect($"/post/{PostUrl}");
    }
}
