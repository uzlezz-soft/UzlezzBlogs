namespace UzlezzBlogs.Middleware;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RequestAuthAttribute : Attribute
{
    public bool Required = true;
}
