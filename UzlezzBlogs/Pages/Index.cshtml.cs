using UzlezzBlogs.Core.Dto;
using UzlezzBlogs.Services;

namespace UzlezzBlogs.Pages
{
    public class IndexModel(IPostService postService) : PageModel
    {
        public required PostPreview[] Posts { get; set; }

        [BindProperty(SupportsGet = true, Name = "Page")]
        public int PageIndex { get; set; } = 1;

        public int TotalPages { get; set; } = 1;

        public async Task<IActionResult> OnGetAsync()
        {
            var result = await postService.GetLatestPosts(PageIndex);
            if (!result.IsSuccessStatusCode) return StatusCode(StatusCodes.Status503ServiceUnavailable);

            var list = result.Content!;
            if (PageIndex > list.TotalPages && list.TotalPages > 0)
                return LocalRedirect($"/?page={list.TotalPages}");

            Posts = list.Posts;
            TotalPages = list.TotalPages;

            return Page();
        }
    }
}
