using UzlezzBlogs.Core.Dto;

namespace Post.Api.Interfaces;

public interface IPostService
{
    Task<(PostPreview[] posts, int totalPages)> GetPagedPostsAsync(int page);
    Task<(RatedPostPreview[] posts, int totalPages)> GetPagedUserRatedPostsAsync(string userId, int page);
    Task<(PostPreview[] posts, int totalPages)> GetUserPostsAsync(string userName, int page);
    Task<PostContent?> GetPostContentByIdAsync(string id, string userId);
    Task<PostComment[]?> GetPostCommentsAsync(string id, int skip, int take);
    Task<PostDetails?> GetPostWithDetailsAsync(string postUrl, string? requestingUserId);
    Task<PostRatings?> RatePostAsync(string postId, string userId, bool isUpvote);
    Task<PostComment?> AddCommentAsync(string postId, string userId, string userName, string markdownContent);
    Task<PostPreview> CreatePostAsync(string title, string description, string content, string userId, string userName);
    Task<bool> EditPostAsync(string userId, string id, string description, string content);
    Task<int> GetPostCountAsync();
    string PreviewHtml(string content);
    Task<(PostPreview[] posts, int totalPages)> SearchPostsAsync(string query, int page);
}
