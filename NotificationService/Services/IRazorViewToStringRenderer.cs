namespace NotificationService.Services;

public interface IRazorViewToStringRenderer
{
    Task<string> RenderViewAsync<TModel>(string viewName, TModel model);
}
