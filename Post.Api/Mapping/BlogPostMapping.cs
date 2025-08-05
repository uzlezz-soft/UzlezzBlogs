using Post.Api.Entities;
using UzlezzBlogs.Core.Dto;

namespace Post.Api.Mapping;

public static class BlogPostMapping
{
    public static PostPreview ToPostPreview(this BlogPost post) =>
        new PostPreview(post.Title, post.Description, post.Url, post.CreatedDate, post.UserName);

    public static RatedPostPreview ToRatedPostPreview(this BlogPost post, string userId) =>
        new RatedPostPreview(post.Title, post.Description, post.Url, post.CreatedDate,
            post.Ratings.FirstOrDefault(x => x.UserId == userId)!.IsUpvote, post.UserName);

    public static PostContent ToPostContent(this BlogPost post) =>
        new PostContent(post.Title, post.Description, post.Content, post.HtmlContent, post.UserName);

    public static PostDetails ToPostDetails(this BlogPost post, int commentCount, string? requestingUserId) =>
        new PostDetails(post.Id, post.Title, post.Url, post.HtmlContent, post.CreatedDate, post.UserName, post.ViewCount,
            commentCount, requestingUserId is null ? null : post.Ratings.FirstOrDefault(x => x.UserId == requestingUserId)?.IsUpvote,
            post.Ratings.Count(x => x.IsUpvote), post.Ratings.Count(x => !x.IsUpvote));
}
