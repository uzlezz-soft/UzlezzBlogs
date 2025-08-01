using UzlezzBlogs.Core.Dto;
using UzlezzBlogs.Services;

namespace UzlezzBlogs.Pages
{
    public class IndexModel(IPostService postService) : PageModel
    {
        public PostPreview[] Posts { get; set; }

        [BindProperty(SupportsGet = true, Name = "Page")]
        public int PageIndex { get; set; } = 1;

        public int TotalPages { get; set; } = 1;

        public async Task<IActionResult> OnGetAsync()
        {
            var list = await postService.GetLatestPosts(PageIndex);
            if (PageIndex > list.TotalPages && list.TotalPages > 0)
                return LocalRedirect($"/?Page={list.TotalPages}");

            Posts = list.Posts;
            TotalPages = list.TotalPages;

            return Page();
        }
    }
}
