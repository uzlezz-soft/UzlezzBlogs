namespace UzlezzBlogs.Pages.Shared;

public class PaginationModel
{
    public int PageIndex { get; set; }
    public int TotalPages { get; set; }

    public Func<int, string> GenerateUrl { get; set; } = page => $"/?Page={page}";
}
