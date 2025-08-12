using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Post.Api.Configs;
using Post.Api.Entities;
using Post.Api.Interfaces;
using Post.Api.Mapping;
using UzlezzBlogs.Core.Dto;
using UzlezzBlogs.Microservices.Shared;
using UzlezzBlogs.Microservices.Shared.Messages;

namespace Post.Api.Services;

public class PostService(
    PostDbContext context,
    IOptions<MainPageConfig> config,
    IPostUrlGenerator urlGenerator,
    IHtmlGenerator htmlGenerator,
    ILogger<PostService> logger,
    IMemoryCache memoryCache,
    IMessageBroker messageBroker) : IPostService
{
    private readonly MainPageConfig _config = config.Value;

    public async Task<PostComment?> AddCommentAsync(string postId, string userId, string userName, string markdownContent)
    {
        var post = await context.Posts
            .Include(p => p.Comments)
            .FirstOrDefaultAsync(p => p.Id == postId) ?? throw new Exception("Post not found");
        var comment = new Comment
        {
            PostId = post.Id,
            UserId = userId,
            UserName = userName,
            Content = htmlGenerator.GenerateHtmlForComment(markdownContent)
        };

        post.Comments.Add(comment);
        context.Update(post);
        await context.SaveChangesAsync();

        logger.LogInformation("User {UserName} commented on post {PostId}: {PostTitle}",
            userName, post.Id, post.Title);
        if (post.UserName != comment.UserName)
        {
            await messageBroker.Publish(new Notification
            {
                TargetUserName = post.UserName,
                Type = "comment",
                Parameters = new()
                {
                    ["postTitle"] = post.Title,
                    ["postUrl"] = post.Url,
                    ["author"] = userName,
                    ["content"] = comment.Content
                }
            });
        }

        return comment.ToPostComment();
    }

    public async Task<PostPreview> CreatePostAsync(string title, string description, string content, string userId, string userName)
    {
        var totalPosts = await context.Posts.CountAsync();
        title = title.Trim();
        var url = urlGenerator.GenerateUrl(title, totalPosts + 1);

        var post = new BlogPost
        {
            Id = Guid.NewGuid().ToString(),
            Title = title,
            Description = description.Trim(),
            Content = content,
            HtmlContent = htmlGenerator.GenerateHtml(content),
            Url = url,
            ViewCount = 0,
            UserId = userId,
            UserName = userName
        };

        await context.Posts.AddAsync(post);
        await context.SaveChangesAsync();

        logger.LogInformation("User {UserName} created post {PostId}: {PostTitle}", userId, post.Id, post.Title);
        await messageBroker.Publish(new Notification
        {
            Type = "post",
            Parameters = new()
            {
                ["title"] = post.Title,
                ["description"] = post.Description,
                ["author"] = post.UserName,
                ["url"] = post.Url
            }
        });

        return post.ToPostPreview();
    }

    public async Task<bool> EditPostAsync(string userId, string id, string description, string content)
    {
        var post = await context.Posts
            .Where(post => post.Id == id && post.UserId == userId)
            .FirstOrDefaultAsync();
        if (post is null) return false;

        post.Description = description;
        post.Content = content;
        post.HtmlContent = htmlGenerator.GenerateHtml(content);
        context.Update(post);
        await context.SaveChangesAsync();

        logger.LogInformation("User {UserName} edited post {PostId}", userId, post.Id);
        return true;
    }

    public string PreviewHtml(string content) => htmlGenerator.GenerateHtml(content);

    public async Task<(PostPreview[] posts, int totalPages)> GetPagedPostsAsync(int page)
    {
        var count = await context.Posts.CountAsync();
        var totalPages = (count + _config.PostsPerPage - 1) / _config.PostsPerPage;
        page = Math.Max(1, Math.Min(page, totalPages));

        var posts = await context.Posts
            .OrderByDescending(p => p.CreatedDate)
            .Skip((page - 1) * _config.PostsPerPage)
            .Take(_config.PostsPerPage)
            .Select(post => post.ToPostPreview())
            .ToArrayAsync();

        return (posts, totalPages);
    }

    public async Task<(RatedPostPreview[] posts, int totalPages)> GetPagedUserRatedPostsAsync(string userId, int page)
    {
        var query = context.Posts
            .Include(p => p.Ratings)
            .Where(p => p.Ratings.Any(r => r.UserId == userId));

        var count = query.Count();
        var totalPages = (count + _config.PostsPerPage - 1) / _config.PostsPerPage;
        page = Math.Max(1, Math.Min(page, totalPages));

        var posts = await query
            .OrderByDescending(p => p.CreatedDate)
            .Skip((page - 1) * _config.PostsPerPage)
            .Take(_config.PostsPerPage)
            .Select(post => post.ToRatedPostPreview(userId))
            .ToArrayAsync();

        return (posts, totalPages);
    }

    public Task<PostContent?> GetPostContentByIdAsync(string id, string userId)
    {
        return context.Posts
            .Where(p => p.Id == id && p.UserId == userId)
            .Select(x => x.ToPostContent())
            .FirstOrDefaultAsync();
    }

    public Task<int> GetPostCountAsync()
    {
        return context.Posts.CountAsync();
    }

    public async Task<PostDetails?> GetPostWithDetailsAsync(string postUrl, string? requestingUserId)
    {
        var post = await context.Posts
            .Include(p => p.Ratings)
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Url == postUrl);
        if (post is null) return null;

        post.ViewCount++;
        context.Update(post);
        await context.SaveChangesAsync();

        var commentCount = await context.Posts
            .Include(x => x.Comments)
            .Select(x => new { x.Id, x.Comments.Count })
            .FirstAsync(x => x.Id == post.Id);

        return post.ToPostDetails(commentCount.Count, requestingUserId);
    }

    public async Task<(PostPreview[] posts, int totalPages)> GetUserPostsAsync(string userName, int page)
    {
        var query = context.Posts
            .Where(x => EF.Functions.ILike(x.UserName, userName));

        var count = await query.CountAsync();
        var totalPages = (count + _config.PostsPerPage - 1) / _config.PostsPerPage;
        page = Math.Max(1, Math.Min(page, totalPages));

        var posts = await query
            .OrderByDescending(post => post.CreatedDate)
            .Skip((page - 1) * _config.PostsPerPage)
            .Take(_config.PostsPerPage)
            .Select(post => post.ToPostPreview())
            .ToArrayAsync();

        return (posts, totalPages);
    }

    public async Task<PostRatings?> RatePostAsync(string postId, string userId, bool isUpvote)
    {
        var post = await context.Posts
            .Include(p => p.Ratings)
            .FirstOrDefaultAsync(p => p.Id == postId);
        if (post == null) return null;

        var rating = post.Ratings.FirstOrDefault(r => r.UserId == userId);

        if (rating != null)
        {
            if ((isUpvote && rating.IsUpvote) || (!isUpvote && !rating.IsUpvote))
            {
                post.Ratings.Remove(rating);
            }
            else
            {
                rating.IsUpvote = isUpvote;
            }
        }
        else
        {
            rating = new PostRating { PostId = post.Id, UserId = userId, IsUpvote = isUpvote };
            post.Ratings.Add(rating);
        }

        context.Update(post);
        await context.SaveChangesAsync();

        var upvotes = post.Ratings.Count(r => r.IsUpvote);
        var downvotes = post.Ratings.Count - upvotes;

        return new PostRatings(upvotes, downvotes, post.ViewCount);
    }

    public async Task<PostComment[]?> GetPostCommentsAsync(string id, int skip, int take)
    {
        var post = await context.Posts
            .Include(p => p.Comments)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (post is null) return null;

        return post.Comments
            .Skip(skip)
            .Take(take)
            .Select(x => x.ToPostComment())
            .ToArray();
    }

    private async Task<PostPreview[]?> SearchPostsByKeyword(string keyword) =>
        await memoryCache.GetOrCreateAsync($"search:{keyword}", async entry =>
        {
            entry.SetSlidingExpiration(TimeSpan.FromMinutes(5));
            return await context.Posts
                .Where(x => EF.Functions.ILike(x.Title, $"%{keyword}%"))
                .Select(x => x.ToPostPreview())
                .ToArrayAsync();
        });

    private static readonly int MaxSearchKeywords = 16;
    public async Task<(PostPreview[] posts, int totalPages)> SearchPostsAsync(string query, int page)
    {
        HashSet<PostPreview> allPosts = new();

        var keywords = query.Split(' ', MaxSearchKeywords,
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(x => x.ToLower());
        foreach (var keyword in keywords)
        {
            var keywordPosts = await SearchPostsByKeyword(keyword);
            if (keywordPosts is null) continue;

            foreach (var post in keywordPosts)
            {
                allPosts.Add(post);
            }
        }

        var count = allPosts.Count;
        var totalPages = (count + _config.PostsPerPage - 1) / _config.PostsPerPage;
        page = Math.Max(1, Math.Min(page, totalPages));

        var posts = allPosts
            .OrderByDescending(post => post.CreatedDate)
            .Skip((page - 1) * _config.PostsPerPage)
            .Take(_config.PostsPerPage)
            .ToArray();

        return (posts, totalPages);
    }
}
