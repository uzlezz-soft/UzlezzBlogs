using NotificationService.Services;
using Microsoft.AspNetCore.Mvc.Razor;
using UzlezzBlogs.Microservices.Shared;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((ctx, services) =>
    {
        services.AddRazorPages();
        services.Configure<RazorViewEngineOptions>(_ => {});
        services.ConfigureMessageBroker(ctx.Configuration);

        services.AddSingleton<IRazorViewToStringRenderer, RazorViewToStringRenderer>();
        services.AddHostedService<NotificationBackgroundService>();
    });

var app = builder.Build();

await app.RunAsync();
