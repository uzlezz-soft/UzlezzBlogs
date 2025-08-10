namespace UzlezzBlogs.Microservices.Shared.Messages;

public interface IMessage
{
    static abstract string Queue { get; }
}
