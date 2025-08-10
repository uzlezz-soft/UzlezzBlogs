using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace NotificationService.Services;

public class RazorViewToStringRenderer(
    IRazorViewEngine viewEngine,
    ITempDataProvider tempDataProvider,
    IServiceProvider serviceProvider) : IRazorViewToStringRenderer
{
    public async Task<string> RenderViewAsync<TModel>(string pageName, TModel model)
    {
        var actionContext = new ActionContext(
            new DefaultHttpContext { RequestServices = serviceProvider },
            new RouteData(),
            new PageActionDescriptor()
        );

        var viewResult = viewEngine.GetView(executingFilePath: null, $"Views/{pageName}", isMainPage: true);
        if (!viewResult.Success)
            throw new InvalidOperationException($"Razor View {pageName} not found");

        var viewDictionary = new ViewDataDictionary<TModel>(
            metadataProvider: new EmptyModelMetadataProvider(),
            modelState: new ModelStateDictionary())
        {
            Model = model
        };

        await using var sw = new StringWriter();
        var viewContext = new ViewContext(
            actionContext,
            viewResult.View,
            viewDictionary,
            new TempDataDictionary(actionContext.HttpContext, tempDataProvider),
            sw,
            new HtmlHelperOptions()
        );

        await viewResult.View.RenderAsync(viewContext);
        return sw.ToString();
    }
}
