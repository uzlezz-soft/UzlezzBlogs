using Hydro;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace UzlezzBlogs.Pages.Views;

public class Pagination : HydroView
{
    [HtmlAttributeNotBound]
    public int MaxPagesToShow { get; set; } = 5;

    public int PageIndex { get; set; }
    public int TotalPages { get; set; }
    public Func<int, string> GenerateUrl { get; set; } = (page) => $"/{page}";
}
