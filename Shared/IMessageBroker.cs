using UzlezzBlogs.Microservices.Shared.Messages;

namespace UzlezzBlogs.Microservices.Shared;

public interface IMessageBroker
{
    public Task Publish<TMessage>(TMessage message) where TMessage : IMessage;
    public Task Consume<TMessage>(Func<TMessage, Task> consumer) where TMessage : IMessage;
}
