using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using UzlezzBlogs.Microservices.Shared.Configs;
using UzlezzBlogs.Microservices.Shared.Messages;

namespace UzlezzBlogs.Microservices.Shared;

public class RabbitMqService : IMessageBroker, IAsyncDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;

    public RabbitMqService(IOptions<RabbitMqConfig> config)
    {
        var factory = new ConnectionFactory()
        {
            HostName = config.Value.HostName,
            Port = config.Value.Port,
            UserName = config.Value.UserName,
            Password = config.Value.Password,
            VirtualHost = config.Value.VirtualHost
        };

        _connection = factory.CreateConnectionAsync().Result;
        _channel = _connection.CreateChannelAsync().Result;
    }

    public async Task Publish<TMessage>(TMessage message) where TMessage : IMessage
    {
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);
        await _channel.BasicPublishAsync("", TMessage.Queue, body);
    }

    public async Task Consume<TMessage>(Func<TMessage, Task> consumer) where TMessage : IMessage
    {
        var c = new AsyncEventingBasicConsumer(_channel);
        c.ReceivedAsync += async (channel, args) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(args.Body.ToArray());
                var message = JsonSerializer.Deserialize<TMessage>(json);

                await consumer(message!);
                await _channel.BasicAckAsync(args.DeliveryTag, false);
            }
            catch
            {
                await _channel.BasicNackAsync(args.DeliveryTag, false, true);
            }
        };

        await _channel.BasicConsumeAsync(TMessage.Queue, false, c);
    }

    public async ValueTask DisposeAsync()
    {
        await _channel.CloseAsync();
        await _connection.CloseAsync();
    }
}
