using UzlezzBlogs.Services;

namespace UzlezzBlogs
{
    public class UserController : Controller
    {
        private IAuthService _userService;

        public UserController(IAuthService userService)
        {
            _userService = userService;
        }

        [HttpGet("/Avatar/{userName}")]
        [ResponseCache(Duration = 86400, Location =ResponseCacheLocation.Client)]
        public async Task<IActionResult> Avatar([FromRoute] string userName)
        {
            var avatar = await _userService.GetAvatar(userName);
            return File(Convert.FromBase64String(avatar.AvatarData), avatar.AvatarMimeType);
        }
    }
}
