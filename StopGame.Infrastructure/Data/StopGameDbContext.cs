using Microsoft.EntityFrameworkCore;
using StopGame.Domain.Entities;

namespace StopGame.Infrastructure.Data;

public class StopGameDbContext : DbContext
{
    public StopGameDbContext(DbContextOptions<StopGameDbContext> options) : base(options)
    {
    }

    public DbSet<Topic> Topics { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Topic configuration
        modelBuilder.Entity<Topic>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Name).IsRequired().HasMaxLength(100);
            entity.Property(t => t.IsDefault).IsRequired();
            entity.Property(t => t.CreatedAt).IsRequired();
            
            entity.HasIndex(t => t.Name).IsUnique();
        });

        // Seed default topics
        var defaultTopics = Topic.GetDefaultTopics();
        modelBuilder.Entity<Topic>().HasData(defaultTopics);
    }
}