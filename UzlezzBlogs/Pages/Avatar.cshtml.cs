namespace UzlezzBlogs.Pages;

[ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Client)]
public class AvatarModel(IAuthService authService) : PageModel
{
    public async Task<IActionResult> OnGet(string userName)
    {
        var avatar = await authService.GetAvatar(userName);
        return avatar.IsSuccessStatusCode
            ? File(Convert.FromBase64String(avatar.Content!.AvatarData), avatar.Content!.AvatarMimeType)
            : NotFound();
    }
}
