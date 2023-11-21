using Domain;
using Microsoft.EntityFrameworkCore;

namespace Persistence;

public class DataContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Plan> Plans { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(
            "host=localhost;port=5432;database=TelegramWeatherBotUsers;username=postgres;password=525252;");
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.Entity<User>()
            .HasOne(u => u.Plan)
            .WithMany(p => p.Users)
            .HasForeignKey(u => u.PlanId);

        builder.Entity<User>()
            .HasOne(u => u.Notifications)
            .WithMany(p => p.Users)
            .HasForeignKey(u => u.NotificationsId);

    }
}