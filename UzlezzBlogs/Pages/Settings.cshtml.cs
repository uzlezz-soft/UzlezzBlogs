using System.ComponentModel.DataAnnotations;

namespace UzlezzBlogs.Pages;

[RequestAuth]
public class SettingsModel(IAuthService authService) : PageModel
{
    public static readonly string[] AllowedAvatarExtensions = [".png", ".jpg", ".webp"];
    public static readonly string[] AllowedAvatarMimeTypes = ["image/png", "image/jpeg", "image/webp"];

    [BindProperty]
    [StringLength(3000)]
    public string Description { get; set; }

    [BindProperty]
    public IFormFile? AvatarFile { get; set; }

    public async Task<IActionResult> OnGet()
    {
        var profile = await authService.GetProfileDetails(HttpContext.GetAuthToken()!);
        if (!profile.IsSuccessStatusCode)
        {
            return profile.StatusCode == HttpStatusCode.BadRequest
                ? BadRequest() : StatusCode(StatusCodes.Status503ServiceUnavailable);
        }
        Description = profile.Content!.Description;
        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
            return Page();

        bool changedAvatar = false;
        if (AvatarFile is not null && AvatarFile.Length > 0)
        {
            if (AvatarFile.Length > 1024 * 128)
            {
                ModelState.AddModelError(nameof(AvatarFile), "File is too large. Size should not exceed 128kb");
                return Page();
            }

            var extension = Path.GetExtension(AvatarFile.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !AllowedAvatarExtensions.Contains(extension))
            {
                ModelState.AddModelError(nameof(AvatarFile), "Invalid file type.");
                return Page();
            }

            if (!AllowedAvatarMimeTypes.Contains(AvatarFile.ContentType))
            {
                ModelState.AddModelError(nameof(AvatarFile), "Invalid MIME type.");
            }

            var uploadResult = await authService.UploadAvatar(
                new StreamPart(AvatarFile.OpenReadStream(), "avatar", contentType: AvatarFile.ContentType), HttpContext.GetAuthToken()!);
            if (!uploadResult.IsSuccessStatusCode)
            {
                if (uploadResult.StatusCode == HttpStatusCode.BadRequest)
                {
                    ModelState.AddModelError(nameof(AvatarFile), "Invalid file");
                    return Page();
                }
                return StatusCode(StatusCodes.Status503ServiceUnavailable);
            }
            changedAvatar = true;
        }

        var result = await authService.EditProfile(Description, HttpContext.GetAuthToken()!);
        if (result.IsSuccessStatusCode) return LocalRedirect($"/Me{(changedAvatar ? "?ReCacheAvatar=true" : "")}");

        return result.StatusCode == HttpStatusCode.BadRequest
            ? BadRequest() : StatusCode(StatusCodes.Status503ServiceUnavailable);
    }
}
