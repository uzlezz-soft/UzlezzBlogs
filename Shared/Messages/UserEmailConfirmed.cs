namespace UzlezzBlogs.Microservices.Shared.Messages;

public class UserEmailConfirmed : IMessage
{
    public static string Queue => nameof(UserEmailConfirmed);

    public required string UserName { get; set; }
    public required string Email { get; set; }
}
