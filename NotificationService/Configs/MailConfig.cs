namespace NotificationService.Configs;

public class MailConfig
{
    public const string Mail = "Mail";

    public required string SmtpHost { get; set; }
    public required int SmtpPort { get; set; }
    public required bool SmtpUseSsl { get; set; }
    public required string SmtpLogin { get; set; }
    public required string SmtpPassword { get; set; }

    public required string WebsiteUrl { get; set; }

    public string Link(string link) => $"{WebsiteUrl}/{link}";
}
