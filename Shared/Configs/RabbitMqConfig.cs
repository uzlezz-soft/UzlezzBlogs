namespace UzlezzBlogs.Microservices.Shared.Configs;

public class RabbitMqConfig
{
    public static readonly string RabbitMq = "RabbitMq";

    public required string HostName { get; set; }
    public required int Port { get; set; }
    public required string UserName { get; set; }
    public required string Password { get; set; }
    public required string VirtualHost { get; set; }
}
