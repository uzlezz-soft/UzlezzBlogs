namespace UzlezzBlogs.Microservices.Shared.Messages;

public class Notification : IMessage
{
    public static string Queue => "Notification";

    public string? TargetUserName { get; set; }
    public string? TargetEmail { get; set; }
    public string? Type { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = [];
}
