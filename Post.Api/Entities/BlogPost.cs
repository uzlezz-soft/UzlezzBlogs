using System.ComponentModel.DataAnnotations.Schema;

namespace Post.Api.Entities;

[Table("Posts")]
public class BlogPost
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Url { get; set; }
    public required string Content { get; set; }
    public required string HtmlContent { get; set; }
    public required string Description { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public int ViewCount { get; set; } = 0;

    public required string UserId { get; set; }
    public required string UserName { get; set; }

    public ICollection<PostRating> Ratings { get; } = [];
    public ICollection<Comment> Comments { get; } = [];
}
