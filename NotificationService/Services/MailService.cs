using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using NotificationService.Configs;

namespace NotificationService.Services;

public class MailService(IOptions<MailConfig> config, ILogger<MailService> logger) : IMailService
{
    private readonly MailConfig _config = config.Value;
    private SmtpClient? _client;

    private async Task ReconnectIfNecessary()
    {
        if (_client is not null && _client.IsConnected) return;

        _client = new SmtpClient();
        await _client.ConnectAsync(_config.SmtpHost, _config.SmtpPort, _config.SmtpUseSsl);
        await _client.AuthenticateAsync(_config.SmtpLogin, _config.SmtpPassword);
    }

    public async Task SendEmail(string recipient, string recipientName, string subject, string body, bool isHtml = true)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("No Reply", _config.SmtpLogin));
        message.To.Add(new MailboxAddress(recipientName, recipient));
        message.Subject = subject;
        message.Body = new TextPart(isHtml ? MimeKit.Text.TextFormat.Html : MimeKit.Text.TextFormat.Plain)
        {
            Text = body
        };

        await ReconnectIfNecessary();

        try
        {
            await _client!.SendAsync(message);
        }
        catch (SmtpCommandException exception)
        {
            logger.LogError("Unable to send email to {Recipient}({UserName}): {ErrorMessage}",
                recipient, recipientName, exception.Message);
        }
    }
}
