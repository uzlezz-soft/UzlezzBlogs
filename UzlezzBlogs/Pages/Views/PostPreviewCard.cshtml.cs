using Hydro;
using UzlezzBlogs.Core.Dto;

namespace UzlezzBlogs.Pages.Views;

public class PostPreviewCard : HydroView
{
    public required PostPreview Post { get; set; }
}
