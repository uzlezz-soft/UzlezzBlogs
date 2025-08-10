using Microsoft.Extensions.Options;
using NotificationService.Configs;
using NotificationService.Models;
using UzlezzBlogs.Microservices.Shared.Messages;

namespace NotificationService.Services;

public class NotificationEmailFactory(IOptions<MailConfig> config) : INotificationEmailFactory
{
    private readonly string _host = config.Value.WebsiteUrl;

    public async Task<(string? Subject, string? Body)> FormEmail(Notification notification)
    {
        if (notification.Type == "comment")
        {
            var model = new Comment
            { 
                User = notification.TargetUserName!,
                AuthorAvatar = $"{_host}/avatar/{notification.Parameters["author"]}",
                AuthorName = notification.Parameters["author"]!.ToString()!,
                Content = notification.Parameters["content"]!.ToString()!,
                PostUrl = $"{_host}/post/{notification.Parameters["postUrl"]}"
            };
            var body = await RenderPage(notification.Type, model);
            var subject = $"New comment on your post `{notification.Parameters["postTitle"]}`";
            return (subject, body);
        }

        return (null, null);
    }

    private static readonly string Location = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!;
    private async Task<string> RenderPage<T>(string pageFile, T model)
    {
        string path = Path.Combine(Location, "Views", $"{pageFile}.cshtml");
        if (!File.Exists(path)) throw new FileNotFoundException($"Page file {pageFile} does not exist");

        var content = await File.ReadAllTextAsync(path);
        foreach (var field in typeof(T).GetFields())
        {
            var name = field.Name;
            var data = field.GetValue(model);

            content = content.Replace($"{{{name}}}", data!.ToString());
        }

        return content;
    }
}
