using Post.Api.Entities;
using UzlezzBlogs.Core.Dto;

namespace Post.Api.Mapping;

public static class CommentMapping
{
    public static PostComment ToPostComment(this Comment comment)
        => new(comment.UserName, comment.Content, comment.CreatedDate);
}
