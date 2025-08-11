using NotificationService;
using NotificationService.Configs;
using NotificationService.Services;
using UzlezzBlogs.Microservices.Shared;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((ctx, services) =>
    {
        services.ConfigureDatabaseContext<NotificationDbContext>(ctx.Configuration.GetConnectionString("DefaultConnection")!);

        services.Configure<MailConfig>(ctx.Configuration.GetSection(MailConfig.Mail));
        services.AddScoped<IMailService, MailService>();

        services.ConfigureMessageBroker(ctx.Configuration);

        services.AddSingleton<IMailMessageFactory, MailMessageFactory>();
        services.AddHostedService<NotificationBackgroundService>();
        services.AddHostedService<MailConfirmationBackgroundService>();
    });

var app = builder.Build();

await app.RunAsync();
