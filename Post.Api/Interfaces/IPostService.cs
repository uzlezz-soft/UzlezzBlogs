using Post.Api.Entities;

namespace Post.Api.Interfaces;

public interface IPostService
{
    Task<(BlogPost[] posts, int totalPages)> GetPagedPostsAsync(int page);
    Task<(BlogPost[] posts, int totalPages)> GetPagedUserRatedPostsAsync(string userId, int page);
    Task<(BlogPost[] posts, int totalPages)> GetUserPostsAsync(string userName, int page);
    Task<BlogPost?> GetPostByIdAsync(int id);
    Task<BlogPost?> GetPostWithDetailsAsync(string postUrl);
    Task<(int upvotes, int downvotes, int views)> RatePostAsync(int postId, string userId, bool isUpvote);
    Task<Comment> AddCommentAsync(int postId, string userId, string markdownContent, string host);
    Task<BlogPost> CreatePostAsync(string title, string description, string content, string userId);
    Task<BlogPost?> EditPostAsync(string userId, int id, string description, string content);
    Task<int> GetPostCountAsync();
    string GeneratePostUrl(string title, int nextPostNumber);
    string GeneratePostContentHtml(string content);
}
