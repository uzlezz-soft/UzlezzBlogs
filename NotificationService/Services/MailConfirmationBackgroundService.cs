using NotificationService.Entities;
using UzlezzBlogs.Microservices.Shared;
using UzlezzBlogs.Microservices.Shared.Messages;

namespace NotificationService.Services;

public class MailConfirmationBackgroundService(
    IMessageBroker messageBroker,
    IServiceScopeFactory serviceScopeFactory,
    ILogger<MailConfirmationBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await messageBroker.Consume<UserEmailConfirmed>(async message =>
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
        }, stoppingToken);
    }
}
