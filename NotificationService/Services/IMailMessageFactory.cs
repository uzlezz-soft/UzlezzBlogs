using UzlezzBlogs.Microservices.Shared.Messages;

namespace NotificationService.Services;

public interface IMailMessageFactory
{
    public Task<(string? Subject, string? Body)> FormMessage(Notification notification);
}
