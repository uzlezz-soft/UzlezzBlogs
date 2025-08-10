using System.Text.Json;
using UzlezzBlogs.Microservices.Shared;
using UzlezzBlogs.Microservices.Shared.Messages;

namespace NotificationService.Services;

public class NotificationBackgroundService(IMessageBroker messageBroker, ILogger<NotificationBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await messageBroker.Consume<Notification>(async message =>
        {
            logger.LogInformation("Notification {Notification}", JsonSerializer.Serialize(messageBroker));
            await Task.Yield();
        });
    }
}
