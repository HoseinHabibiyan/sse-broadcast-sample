using Microsoft.EntityFrameworkCore;
using SSEBroadcastSample.Models;

namespace SSEBroadcastSample.Context;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<NotificationTarget> NotificationTargets { get; set; }
    public DbSet<NotificationDelivery> NotificationDeliveries { get; set; }


    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        modelBuilder.Entity<User>()
            .HasData(
                new User
                {
                    Id = 1,
                    UserName = "Alice"
                },
                new User
                {
                    Id = 2,
                    UserName = "Bob"
                },
                new User
                {
                    Id = 3,
                    UserName = "Charlie"
                },
                new User
                {
                    Id = 4,
                    UserName = "David"
                },
                new User
                {
                    Id = 5,
                    UserName = "Emma"
                }
            );


        modelBuilder.Entity<Notification>()
            .HasData(
                new Notification
                {
                    Id = 1,
                    Message = "Welcome",
                    ExpireDate = new DateTime(2026, 12, 31),
                }
            );


        modelBuilder.Entity<NotificationTarget>()
            .HasData(
                new NotificationTarget
                {
                    Id = 1,
                    NotificationId = 1,
                    TargetType = NotificationTargetType.All,
                }
            );
    }
}