using BlijvenLeren.App.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlijvenLeren.App.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Comment> Comments => Set<Comment>();

    public DbSet<LearningResource> LearningResources => Set<LearningResource>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LearningResource>(entity =>
        {
            entity.ToTable("learning_resources");
            entity.HasKey(resource => resource.Id);

            entity.Property(resource => resource.Title)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(resource => resource.Description)
                .HasMaxLength(2000)
                .IsRequired();

            entity.Property(resource => resource.Url)
                .HasMaxLength(2048)
                .IsRequired();

            entity.Property(resource => resource.CreatedUtc)
                .IsRequired();

            entity.HasMany(resource => resource.Comments)
                .WithOne(comment => comment.LearningResource)
                .HasForeignKey(comment => comment.LearningResourceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.ToTable("comments");
            entity.HasKey(comment => comment.Id);

            entity.Property(comment => comment.AuthorDisplayName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(comment => comment.Body)
                .HasMaxLength(2000)
                .IsRequired();

            entity.Property(comment => comment.AuthorType)
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();

            entity.Property(comment => comment.Status)
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();

            entity.Property(comment => comment.CreatedUtc)
                .IsRequired();

            entity.HasIndex(comment => new { comment.LearningResourceId, comment.Status });
        });
    }
}
