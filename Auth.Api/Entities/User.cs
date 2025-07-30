using Microsoft.AspNetCore.Identity;

namespace Auth.Api.Entities;
public class User : IdentityUser
{
    public string Avatar { get; set; } = Constants.DefaultAvatar;
    public string AvatarMimeType { get; set; } = Constants.DefaultAvatarMimeType;
    public string DescriptionMarkdown { get; set; } = "No description";
    public string Description { get; set; } = "<p>No description</p>";
}
