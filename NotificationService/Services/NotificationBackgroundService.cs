using Microsoft.EntityFrameworkCore;
using NotificationService.Entities;
using System.Text.Json;
using UzlezzBlogs.Microservices.Shared;
using UzlezzBlogs.Microservices.Shared.Messages;

namespace NotificationService.Services;

public class NotificationBackgroundService(IMessageBroker messageBroker,
    IServiceScopeFactory serviceScopeFactory,
    IMailService mailService,
    INotificationEmailFactory emailFactory,
    ILogger<NotificationBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await messageBroker.Consume<Notification>(SendNotification, stoppingToken);
        await messageBroker.Consume<UserEmailConfirmed>(OnEmailConfirmed, stoppingToken);
    }

    private async Task SendNotification(Notification notification)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();

        ConfirmedUser? user;
        if (notification.TargetEmail is not null)
        {
            user = new ConfirmedUser
            {
                UserName = notification.TargetUserName ?? notification.TargetEmail.Split("@").FirstOrDefault() ?? string.Empty,
                Email = notification.TargetEmail
            };
        }
        else
        {
            user = await context.Users.FirstOrDefaultAsync(x => x.UserName == notification.TargetUserName);
            if (user is null)
            {
                logger.LogError("Unable to send notification to {UserName}: email not confirmed", notification.TargetUserName);
                return;
            }
        }

        try
        {
            var (subject, body) = await emailFactory.FormEmail(notification);
            if (subject is null || body is null)
            {
                logger.LogError("Unable to send notification to {UserName}: unknown notification type {NotificationType}",
                    notification.TargetUserName, notification.Type);
                return;
            }

            await mailService.SendEmail(user.Email, user.UserName, subject!, body!);
        }
        catch (KeyNotFoundException)
        {
            logger.LogError("Bad notification payload: {NotificationPayload}",
                JsonSerializer.Serialize(notification.Parameters));
        }
        catch (FileNotFoundException e)
        {
            logger.LogError("Bad notification type: {NotificationType}, caused exception: {Exception}",
                notification.Type, e.Message);
        }
    }

    private async Task OnEmailConfirmed(UserEmailConfirmed message)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();

        var user = new ConfirmedUser
        {
            UserName = message.UserName,
            Email = message.Email
        };
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
        logger.LogInformation("User {UserName} confirmed email", user.UserName);
    }
}
