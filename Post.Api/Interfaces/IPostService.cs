using Post.Api.Entities;
using UzlezzBlogs.Core.Dto;

namespace Post.Api.Interfaces;

public interface IPostService
{
    Task<(PostPreview[] posts, int totalPages)> GetPagedPostsAsync(int page);
    Task<(RatedPostPreview[] posts, int totalPages)> GetPagedUserRatedPostsAsync(string userId, int page);
    Task<(PostPreview[] posts, int totalPages)> GetUserPostsAsync(string userName, int page);
    Task<PostContent?> GetPostContentByIdAsync(int id, string userId);
    Task<PostComment[]?> GetPostCommentsAsync(int id, int skip, int take);
    Task<PostDetails?> GetPostWithDetailsAsync(string postUrl, string? requestingUserId);
    Task<PostRatings?> RatePostAsync(int postId, string userId, bool isUpvote);
    Task<PostComment?> AddCommentAsync(int postId, string userId, string userName, string markdownContent);
    Task<PostPreview> CreatePostAsync(string title, string description, string content, string userId, string userName);
    Task<bool> EditPostAsync(string userId, int id, string description, string content);
    Task<int> GetPostCountAsync();
    string PreviewHtml(string content);
}
