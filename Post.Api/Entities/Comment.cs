using System.ComponentModel.DataAnnotations.Schema;

namespace Post.Api.Entities;

[Table("Comments")]
public class Comment
{
    public int Id { get; set; }
    public required int PostId { get; set; }
    public virtual BlogPost Post { get; set; }
    public required string UserId { get; set; }
    public required string Content { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
