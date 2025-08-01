namespace Post.Api.Entities;

public class PostRating
{
    public required string PostId { get; set; }
    public virtual BlogPost Post { get; set; }

    public required string UserId { get; set; }

    public required bool IsUpvote { get; set; }
}
