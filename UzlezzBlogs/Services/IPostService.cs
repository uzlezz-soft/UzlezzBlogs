using UzlezzBlogs.Core.Dto;

namespace UzlezzBlogs.Services;

public interface IPostService
{
    [Get("/post/list/{page}")]
    Task<IApiResponse<PostPreviewList>> GetLatestPosts(int page);
    [Get("/post/list/user/{userName}/{page}")]
    Task<IApiResponse<PostPreviewList>> GetUserPosts(string userName, int page);
    [Get("/post/list/rated/{page}")]
    Task<IApiResponse<RatedPostPreviewList>> GetRatedPosts(int page, [Authorize] string token);

    [Get("/post/details/{url}")]
    Task<IApiResponse<PostDetails>> GetPostDetails(string url);
    [Get("/post/details/{id}/comments")]
    Task<IApiResponse<PostComment[]>> GetPostComments(string id, int skip, int take);

    [Get("/post/raw/{id}")]
    Task<IApiResponse<PostContent>> GetPostContent(string id, [Authorize] string token);
    [Get("/post/preview")]
    Task<IApiResponse<string>> GetPostPreview(string content, [Authorize] string token);

    [Post("/post/rate/{postId}/{rating}")]
    Task<IApiResponse<PostRatings>> RatePost(string postId, int rating, [Authorize] string token);
    [Post("/post/comment/{postId}")]
    Task<IApiResponse<PostComment>> Comment(string postId, string content, [Authorize] string token);
}
