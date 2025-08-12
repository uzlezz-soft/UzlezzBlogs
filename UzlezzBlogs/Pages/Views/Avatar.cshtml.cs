using Hydro;

namespace UzlezzBlogs.Pages.Views;

public class Avatar : HydroView
{
    public required string UserName { get; set; }
    public required int Width { get; set; }
    public required int Height { get; set; }
    public required bool Border { get; set; } = false;
    public required string AddClasses { get; set; } = string.Empty;
}
