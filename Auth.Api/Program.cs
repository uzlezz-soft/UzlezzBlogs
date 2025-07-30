using Auth.Api;
using Auth.Api.Entities;
using Auth.Api.Interfaces;
using Auth.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UzlezzBlogs.Core.Dto;
using UzlezzBlogs.Microservices.Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.ConfigureDatabaseContext<AuthDbContext>(builder.Configuration.GetConnectionString("DefaultConnection")!);
builder.Services.ConfigureIdentity<User, AuthDbContext>();
builder.Services.AddJwtAuth(builder.Configuration);

builder.Services.AddScoped<IUserService, UserService>();

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
        return Results.BadRequest(new RegisterResponse(null, errors.Select(x => x.Description).ToArray()));

    return Results.Ok(new RegisterResponse(await userService.SignInWithPasswordAndGetJwtAsync(dto.UserName, dto.Password), null));
});

app.MapPost("/login", async ([FromBody] LoginRequest dto, IUserService userService) =>
{
    var token = await userService.SignInWithPasswordAndGetJwtAsync(dto.UserName, dto.Password);
    return token is not null ? Results.Ok(new LoginResponse(token)) : Results.BadRequest();
});

app.Run();
