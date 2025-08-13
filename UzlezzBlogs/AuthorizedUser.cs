using System.Text;

namespace UzlezzBlogs;

public record AuthorizedUser(string Id, string Name, string Email);

public static class AuthorizedUserExtensions
{
    public static string CensoredEmail(this AuthorizedUser user)
    {
        var builder = new StringBuilder();
        var email = user.Email.AsSpan();

        var center = email.LastIndexOf('@');

        builder.Append(email[..2]);
        builder.Append("******");
        builder.Append(email.Slice(center - 1, 1));
        builder.Append('@');
        builder.Append(email.Slice(center + 1, 1));
        builder.Append("*****");
        builder.Append(email[^2..]);
        
        return builder.ToString();
    }
}