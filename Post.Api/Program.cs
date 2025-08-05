using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Post.Api;
using Post.Api.Configs;
using Post.Api.Interfaces;
using Post.Api.Services;
using System.Security.Claims;
using UzlezzBlogs.Core.Dto;
using UzlezzBlogs.Microservices.Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.ConfigureDatabaseContext<PostDbContext>(builder.Configuration.GetConnectionString("DefaultConnection")!);
builder.Services.AddJwtAuth(builder.Configuration);

builder.Services.Configure<MainPageConfig>(builder.Configuration.GetSection(MainPageConfig.MainPage));
builder.Services.Configure<PostUrlConfig>(builder.Configuration.GetSection(PostUrlConfig.PostUrl));

builder.Services.AddTransient<IPostUrlGenerator, PostUrlGenerator>();
builder.Services.AddTransient<IHtmlGenerator, MarkdigHtmlGenerator>();
builder.Services.AddScoped<IPostService, PostService>();

builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(b => b.Expire(TimeSpan.FromMinutes(1)));
    options.AddPolicy("profile", b => b.Expire(TimeSpan.FromMinutes(5)));
    options.AddPolicy("details", b => b.Expire(TimeSpan.FromSeconds(30)));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseOutputCache();

app.MapGet("/list/{page}", async (IPostService postService, [FromRoute] int page = 1) =>
{
    var (posts, totalPages) = await postService.GetPagedPostsAsync(page);
    return Results.Ok(new PostPreviewList(posts, totalPages));
})
    .CacheOutput();

app.MapGet("/list/user/{userName}/{page}", async (IPostService postService,
    [FromRoute] string userName, [FromRoute] int page = 1) =>
{
    var (posts, totalPages) = await postService.GetUserPostsAsync(userName, page);
    return Results.Ok(new PostPreviewList(posts, totalPages));
})
    .CacheOutput("profile");

app.MapGet("/list/rated/{page}", [Authorize] async (IPostService postService, HttpContext context, [FromRoute] int page = 1) =>
{
    var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var (posts, totalPages) = await postService.GetPagedUserRatedPostsAsync(userId!, page);
    return Results.Ok(new RatedPostPreviewList(posts, totalPages));
});

app.MapGet("/details/{url}", async (IPostService postService, HttpContext context, [FromRoute] string url) =>
{
    var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    var post = await postService.GetPostWithDetailsAsync(url, userId);
    if (post is null) return Results.NotFound();
    return Results.Ok(post);
})
    .CacheOutput("details");

app.MapGet("/details/{id}/comments", async (IPostService postService, [FromRoute] string id,
    [FromQuery] int skip = 0, [FromQuery] int take = 50) =>
{
    var comments = await postService.GetPostCommentsAsync(id, skip, take);
    return comments is null ? Results.BadRequest() : Results.Ok(comments);
});

app.MapGet("/raw/{id}", [Authorize] async (IPostService postService, HttpContext context, [FromRoute] string id) =>
{
    var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var post = await postService.GetPostContentByIdAsync(id, userId!);
    return post is null ? Results.BadRequest() : Results.Ok(post);
});

app.MapGet("/preview", [Authorize] (IPostService postService, [FromQuery] string content) =>
{
    return Results.Ok(postService.PreviewHtml(content));
});

app.MapPost("/rate/{postId}/{rating}", [Authorize] async (IPostService postService, HttpContext context,
    [FromRoute] string postId, [FromRoute] int rating) =>
{
    var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    var result = await postService.RatePostAsync(postId, userId!, rating == 1);
    return result is null ? Results.BadRequest() : Results.Ok(result);
});

app.MapPost("/comment/{postId}", [Authorize] async (IPostService postService, HttpContext context,
    [FromRoute] string postId, [FromQuery] string content) =>
{
    var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var userName = context.User.FindFirst(ClaimTypes.Name)?.Value;

    try
    {
        var comment = await postService.AddCommentAsync(postId, userId!, userName!, content);
        return comment is null ? Results.BadRequest() : Results.Ok(comment);
    }
    catch
    {
        return Results.NotFound();
    }
});

app.MapPost("/create", [Authorize] async (IPostService postService, HttpContext context, [FromBody] PostCreateRequest request) =>
{
    var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var userName = context.User.FindFirst(ClaimTypes.Name)?.Value;

    var post = await postService.CreatePostAsync(request.Title, request.Description, request.Content, userId!, userName!);
    return Results.Ok(post);
});

app.MapPost("/edit", [Authorize] async (IPostService postService, HttpContext context, [FromBody] PostEditRequest request) =>
{
    var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    return await postService.EditPostAsync(userId!, request.Id, request.Description, request.Content)
        ? Results.Ok() : Results.BadRequest();
});

app.Run();
