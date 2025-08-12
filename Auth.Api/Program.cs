using Auth.Api;
using Auth.Api.Entities;
using Auth.Api.Interfaces;
using Auth.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UzlezzBlogs.Core.Dto;
using UzlezzBlogs.Microservices.Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureMessageBroker(builder.Configuration);

builder.Services.ConfigureDatabaseContext<AuthDbContext>(builder.Configuration.GetConnectionString("DefaultConnection")!);
builder.Services.ConfigureIdentity<User, AuthDbContext>();
builder.Services.AddJwtAuth(builder.Configuration);

builder.Services.AddSingleton<IDescriptionHtmlGenerator, MarkdigDescriptionHtmlGenerator>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(b => b.Expire(TimeSpan.FromMinutes(1)));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/register", async ([FromBody] RegisterRequest dto, IUserService userService) =>
{
    var errors = await userService.RegisterAsync(dto.UserName, dto.Email, dto.Password);
    if (errors is not null)
        return Results.Conflict();

    var token = await userService.SignInWithPasswordAndGetJwtAsync(dto.UserName, dto.Password);
    return Results.Ok(new LoginResponse(token!));
});

app.MapPost("/login", async ([FromBody] LoginRequest dto, IUserService userService) =>
{
    var token = await userService.SignInWithPasswordAndGetJwtAsync(dto.UserName, dto.Password);
    return token is not null ? Results.Ok(new LoginResponse(token)) : Results.BadRequest();
});

app.MapGet("/avatar/{userName}", async ([FromRoute] string userName, IUserService userService) =>
{
    var avatar = await userService.GetAvatarAsync(userName);
    return avatar is null ? Results.NotFound() : Results.Ok(avatar);
});

app.MapGet("/confirmEmail", [Authorize] async (IUserService userService, HttpContext context, [FromQuery] string token) =>
{
    var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    var result = await userService.ConfirmEmailAsync(userId!, token);
    return result ? Results.Ok() : Results.BadRequest();
});

app.MapGet("/profile/{userName}", async (IUserService userService, [FromRoute] string userName) =>
{
    var profile = await userService.GetProfileAsync(userName);
    return profile is null ? Results.NotFound() : Results.Ok(profile);
})
    .CacheOutput();

app.MapGet("/profile", [Authorize] async (IUserService userService, HttpContext context) =>
{
    var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var profile = await userService.GetProfileDetailsAsync(userId!);
    return profile is null ? Results.BadRequest() : Results.Ok(profile);
});

app.MapPost("/editProfile", [Authorize] async (IUserService userService, HttpContext context, [FromQuery] string description) =>
{
    var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var result = await userService.EditProfile(userId!, description);
    return result ? Results.Ok() : Results.BadRequest();
});

app.MapPost("/uploadAvatar", [Authorize] async (IUserService userService, HttpContext context, IFormFile file) =>
{
    var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    
    await userService.UploadAvatar(userId!, file.OpenReadStream(), file.ContentType);
    return Results.Ok();
})
    .DisableAntiforgery();

app.Run();
