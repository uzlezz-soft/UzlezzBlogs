using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using UzlezzBlogs.Configs;
using UzlezzBlogs.Core.Dto;

namespace UzlezzBlogs.Pages;

public class LoginModel(IAuthService authService, IOptions<AuthConfig> authConfig) : PageModel
{
    [Required]
    [Display(Name = "Username")]
    [StringLength(30, MinimumLength = 3)]
    [BindProperty]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6)]
    [BindProperty]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember me")]
    [BindProperty]
    public bool RememberMe { get; set; }

    [ValidateNever]
    [BindProperty]
    public string ReturnUrl { get; set; } = string.Empty;

    public IActionResult OnGet([FromQuery(Name = "ReturnUrl")] string? returnUrl = null)
    {
        if (HttpContext.GetAuthToken() is not null)
        {
            return LocalRedirect(Url.IsLocalUrl(ReturnUrl) ? ReturnUrl : "/");
        }

        ReturnUrl = returnUrl ?? string.Empty;
        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
            return Page();

        var result = await authService.Login(new LoginRequest(UserName, Password));
        if (!result.IsSuccessStatusCode)
        {
            if (result.StatusCode == HttpStatusCode.BadRequest)
            {
                ModelState.AddModelError(nameof(UserName), "Username or password is invalid");
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
        return LocalRedirect(Url.IsLocalUrl(ReturnUrl) ? ReturnUrl : "/");
    }
}
