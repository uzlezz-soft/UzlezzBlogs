using Hydro;
using UzlezzBlogs.Core.Dto;

namespace UzlezzBlogs.Pages.Views;

public class RatedPostPreviewCard : HydroView
{
    public required RatedPostPreview Post { get; set; }
}
