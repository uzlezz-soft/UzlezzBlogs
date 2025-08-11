using Microsoft.Extensions.Options;
using NotificationService.Configs;
using NotificationService.Models;
using System.Reflection;
using UzlezzBlogs.Microservices.Shared.Messages;

namespace NotificationService.Services;

public class MailMessageFactory(IOptions<MailConfig> config) : IMailMessageFactory
{
    private readonly Dictionary<string, Func<MailConfig, Notification, Task<(string, string)>>> _handlers = new()
    {
        ["confirm_email"] = async (config, notification) =>
        {
            var body = await RenderPage("ConfirmEmail", new ConfirmEmail
            {
                User = notification.TargetUserName!,
                Link = config.Link($"confirmEmail?token={notification.Parameters["token"]}")
            });
            return ("Email confirmation", body);
        },
        ["comment"] = async (config, notification) =>
        {
            var model = new Comment
            {
                User = notification.TargetUserName!,
                AuthorAvatar = $"{config.WebsiteUrl}/avatar/{notification.Parameters["author"]}",
                AuthorName = notification.Parameters["author"]!.ToString()!,
                Content = notification.Parameters["content"]!.ToString()!,
                PostUrl = $"{config.WebsiteUrl}/post/{notification.Parameters["postUrl"]}"
            };
            var body = await RenderPage("Comment", model);
            var subject = $"New comment on your post `{notification.Parameters["postTitle"]}`";
            return (subject, body);
        }
    };

    public async Task<(string? Subject, string? Body)> FormMessage(Notification notification)
    {
        if (notification.Type is not null && _handlers.TryGetValue(notification.Type, out var handler))
        {
            return await handler.Invoke(config.Value, notification);
        }
        return (null, null);
    }

    private static readonly string Location = Path.Combine(
        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
        "Views");
    private static async Task<string> RenderPage<T>(string pageFile, T model)
    {
        string path = Path.Combine(Location, $"{pageFile}.cshtml");
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
