namespace NotificationService.Services;

public interface IMailService
{
    public Task SendEmail(string recipient, string recipientName, string subject, string body, bool isHtml = true);
}
