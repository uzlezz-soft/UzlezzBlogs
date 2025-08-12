using Hydro.Configuration;
using UzlezzBlogs.Configs;
using UzlezzBlogs.Core.Configs;
using UzlezzBlogs.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ApiConfig>(builder.Configuration.GetSection(ApiConfig.Api));
builder.Services.Configure<AuthConfig>(builder.Configuration.GetSection(AuthConfig.Auth));
builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection(JwtConfig.Jwt));

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.Lax;
});

builder.Services.AddSingleton<ITokenValidatorService, TokenValidatorService>();
builder.Services.AddTokenMiddleware();

builder.Services.AddHydro();

builder.Services.AddRazorPages();

builder.Services.AddRefitClient<IPostService>()
    .ConfigureHttpClient(c =>
    {
        c.BaseAddress = new Uri(builder.Configuration["Api:LocalGateway"]!);
    });
builder.Services.AddRefitClient<IAuthService>()
    .ConfigureHttpClient(c =>
    {
        c.BaseAddress = new Uri(builder.Configuration["Api:LocalGateway"]!);
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseStaticFiles(new StaticFileOptions()
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=2592000");
    }
});

app.UseRouting();
app.UseTokenMiddleware();

app.MapRazorPages();

app.UseHydro(builder.Environment);

app.Run();
