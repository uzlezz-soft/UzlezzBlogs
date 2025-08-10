using UzlezzBlogs.Microservices.Shared.Messages;

namespace NotificationService.Services;

public interface INotificationEmailFactory
{
    public Task<(string? Subject, string? Body)> FormEmail(Notification notification);
}
