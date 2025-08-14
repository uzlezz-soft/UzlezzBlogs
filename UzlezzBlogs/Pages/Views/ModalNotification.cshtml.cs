using Hydro;

namespace UzlezzBlogs.Pages.Views;

public class ModalNotification : HydroView
{
    public required string Name { get; set; }
    public required string Title { get; set; }
    public required bool Immediate { get; set; } = false;
}
