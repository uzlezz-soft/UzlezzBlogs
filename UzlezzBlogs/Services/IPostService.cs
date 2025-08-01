using Refit;
using UzlezzBlogs.Core.Dto;

namespace UzlezzBlogs.Services;

public interface IPostService
{
    [Get("/post/list/{page}")]
    Task<PostPreviewList> GetLatestPosts(int page);
    [Get("/post/list/user/{userName}/{page}")]
    Task<PostPreviewList> GetUserPosts(string userName, int page);

    [Get("/post/details/{url}")]
    Task<PostDetails> GetPostDetails(string url);

    [Get("/post/details/{id}/comments")]
    Task<PostComment[]> GetPostComments(string id, int skip, int take);
}
