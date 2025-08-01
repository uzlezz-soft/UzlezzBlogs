using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Post.Api.Configs;
using Post.Api.Entities;
using Post.Api.Interfaces;
using Post.Api.Mapping;
using UzlezzBlogs.Core.Dto;

namespace Post.Api.Services;

public class PostService : IPostService
{
    private readonly PostDbContext _context;
    private readonly MainPageConfig _config;
    private readonly IPostUrlGenerator _urlGenerator;
    private readonly IHtmlGenerator _htmlGenerator;
    private readonly ILogger<PostService> _logger;

    public PostService(
        PostDbContext context,
        IOptions<MainPageConfig> config,
        IPostUrlGenerator urlGenerator,
        IHtmlGenerator htmlGenerator,
        ILogger<PostService> logger)
    {
        _context = context;
        _config = config.Value;
        _urlGenerator = urlGenerator;
        _htmlGenerator = htmlGenerator;
        _logger = logger;
    }

    public async Task<PostComment?> AddCommentAsync(string postId, string userId, string userName, string markdownContent)
    {
        var post = await _context.Posts
            .Include(p => p.Comments)
            .FirstOrDefaultAsync(p => p.Id == postId) ?? throw new Exception("Post not found");
        var comment = new Comment
        {
            PostId = post.Id,
            UserId = userId,
            UserName = userName,
            Content = _htmlGenerator.GenerateHtmlForComment(markdownContent)
        };

        post.Comments.Add(comment);
        _context.Update(post);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserName} commented on post {PostId}: {PostTitle}",
            userName, post.Id, post.Title);
        // TODO: send notification

        return comment.ToPostComment();
    }

    public async Task<PostPreview> CreatePostAsync(string title, string description, string content, string userId, string userName)
    {
        var totalPosts = await _context.Posts.CountAsync();
        title = title.Trim();
        var url = _urlGenerator.GenerateUrl(title, totalPosts + 1);

        var post = new BlogPost
        {
            Id = Guid.NewGuid().ToString(),
            Title = title,
            Description = description.Trim(),
            Content = content,
            HtmlContent = _htmlGenerator.GenerateHtml(content), 
            Url = url,
            ViewCount = 0,
            UserId = userId,
            UserName = userName
        };

        await _context.Posts.AddAsync(post);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserName} created post {PostId}: {PostTitle}", userId, post.Id, post.Title);
        // TODO: send notification

        return post.ToPostPreview();
    }

    public async Task<bool> EditPostAsync(string userId, string id, string description, string content)
    {
        var post = await _context.Posts
            .Where(post => post.Id == id && post.UserId == userId)
            .FirstOrDefaultAsync();
        if (post is null) return false;

        post.Description = description;
        post.Content = content;
        post.HtmlContent = _htmlGenerator.GenerateHtml(content);
        _context.Update(post);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserName} edited post {PostId}", userId, post.Id);
        return true;
    }

    public string PreviewHtml(string content) => _htmlGenerator.GenerateHtml(content);

    public async Task<(PostPreview[] posts, int totalPages)> GetPagedPostsAsync(int page)
    {
        var count = await _context.Posts.CountAsync();
        var totalPages = (count + _config.PostsPerPage - 1) / _config.PostsPerPage;
        page = Math.Max(1, Math.Min(page, totalPages));

        var posts = await _context.Posts
            .OrderByDescending(p => p.CreatedDate)
            .Skip((page - 1) * _config.PostsPerPage)
            .Take(_config.PostsPerPage)
            .Select(post => post.ToPostPreview())
            .ToArrayAsync();

        return (posts, totalPages);
    }

    public async Task<(RatedPostPreview[] posts, int totalPages)> GetPagedUserRatedPostsAsync(string userId, int page)
    {
        var query = _context.Posts
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
        return _context.Posts
            .Where(p => p.Id == id && p.UserId == userId)
            .Select(x => x.ToPostContent())
            .FirstOrDefaultAsync();
    }

    public Task<int> GetPostCountAsync()
    {
        return _context.Posts.CountAsync();
    }

    public async Task<PostDetails?> GetPostWithDetailsAsync(string postUrl, string? requestingUserId)
    {
        var post = await _context.Posts
            .Include(p => p.Ratings)
            .Include(p => p.Comments)
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Url == postUrl);
        if (post is null) return null;

        post.ViewCount++;
        _context.Update(post);
        await _context.SaveChangesAsync();

        return post.ToPostDetails(requestingUserId);
    }

    public async Task<(PostPreview[] posts, int totalPages)> GetUserPostsAsync(string userName, int page)
    {
        var query = _context.Posts
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
        var post = await _context.Posts
            .Include(p => p.Ratings)
            .AsNoTracking()
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

        _context.Update(post);
        await _context.SaveChangesAsync();

        var upvotes = post.Ratings.Count(r => r.IsUpvote);
        var downvotes = post.Ratings.Count - upvotes;

        return new PostRatings(upvotes, downvotes, post.ViewCount);
    }

    public async Task<PostComment[]?> GetPostCommentsAsync(string id, int skip, int take)
    {
        var post = await _context.Posts
            .Include(p => p.Comments)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (post is null) return null;

        return post.Comments
            .Skip(skip)
            .Take(take)
            .Select(x => x.ToPostComment())
            .ToArray();
    }
}
