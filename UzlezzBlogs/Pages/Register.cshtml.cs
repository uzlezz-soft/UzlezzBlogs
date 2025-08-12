using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using UzlezzBlogs.Configs;
using UzlezzBlogs.Core.Dto;
using UzlezzBlogs.Services;

namespace UzlezzBlogs.Pages;

public class RegisterModel(IAuthService authService, IOptions<AuthConfig> authConfig) : PageModel
{
    [Required]
    [Display(Name = "Username")]
    [StringLength(30, MinimumLength = 3)]
    [BindProperty]
    public string UserName { get; set; } = "";

    [Required]
    [EmailAddress]
    [BindProperty]
    public string Email { get; set; } = "";

    [Required]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6)]
    [BindProperty]
    public string Password { get; set; } = "";

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    [BindProperty]
    public string ConfirmPassword { get; set; } = "";

    public IActionResult OnGet()
    {
        if (HttpContext.IsAuthorized())
        {
            return LocalRedirect("/Me");
        }
        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
            return Page();

        var result = await authService.Register(new RegisterRequest(UserName, Email, Password));
        if (!result.IsSuccessStatusCode)
        {
            if (result.StatusCode == HttpStatusCode.Conflict)
            {
                ModelState.AddModelError(nameof(UserName), "User with this name or email is already registered");
                return Page();
            }
            return StatusCode(StatusCodes.Status503ServiceUnavailable);
        }

        var options = new CookieOptions
        {
            Secure = authConfig.Value.CookieHttpsOnly,
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow + TimeSpan.FromDays(7),
            IsEssential = true
        };

        Response.Cookies.Append(Constants.JwtCookieName, result.Content!.Token, options);
        return LocalRedirect("/Me");
    }
}
