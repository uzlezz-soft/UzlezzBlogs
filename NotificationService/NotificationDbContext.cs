using Microsoft.EntityFrameworkCore;
using NotificationService.Entities;

namespace NotificationService;

public class NotificationDbContext : DbContext
{
    public DbSet<ConfirmedUser> Users { get; set; }

    public NotificationDbContext(DbContextOptions<NotificationDbContext> options)
        : base(options)
    {
        Database.EnsureCreated();
    }
}
