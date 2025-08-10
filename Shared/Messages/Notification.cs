namespace UzlezzBlogs.Microservices.Shared.Messages;

public class Notification : IMessage
{
    public static string Queue => "Notification";

    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? Type { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = [];
}
