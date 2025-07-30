using Microsoft.EntityFrameworkCore;
using Post.Api.Entities;

namespace Post.Api;

public class PostDbContext : DbContext
{
    public DbSet<BlogPost> Posts { get; set; }

    public PostDbContext(DbContextOptions<PostDbContext> options)
        : base(options)
    {
        Database.EnsureCreated();
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<PostRating>()
            .HasKey(r => new { r.PostId, r.UserId });
    }
}
