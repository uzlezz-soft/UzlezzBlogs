using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using NotificationService.Configs;

namespace NotificationService.Services;

public class MailService(IOptions<MailConfig> config, ILogger<MailService> logger) : IMailService
{
    private readonly MailConfig _config = config.Value;

    public async Task SendEmail(string recipient, string recipientName, string subject,
        string body, bool isHtml = true, CancellationToken cancellationToken = default)
    {
        using var client = new SmtpClient
        {
            Timeout = 10000
        };

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("No Reply", _config.SmtpLogin));
        message.To.Add(new MailboxAddress(recipientName, recipient));
        message.Subject = subject;
        message.Body = new TextPart(isHtml ? MimeKit.Text.TextFormat.Html : MimeKit.Text.TextFormat.Plain)
        {
            Text = body
        };

        try
        {
            await client.ConnectAsync(_config.SmtpHost, _config.SmtpPort, _config.SmtpUseSsl, cancellationToken);
            await client.AuthenticateAsync(_config.SmtpLogin, _config.SmtpPassword, cancellationToken);
            await client.SendAsync(message, cancellationToken);
        }
        catch (SmtpCommandException exception)
        {
            logger.LogError("Unable to send email to {Recipient}({UserName}): {ErrorMessage}",
                recipient, recipientName, exception.Message);

            if (exception.ErrorCode == SmtpErrorCode.UnexpectedStatusCode)
                throw;
        }
        finally
        {
            if (client.IsConnected)
                await client.DisconnectAsync(true, cancellationToken);
        }
    }
}
