using Microsoft.EntityFrameworkCore;
using MyChat.Entities;

namespace MyChat.Data;

public class ChatDbContext : DbContext
{
    public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Contact> Contacts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.Property(e => e.Username).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Password).HasMaxLength(255).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Configure Contact entity
        modelBuilder.Entity<Contact>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Foreign key relationships
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ContactUser)
                  .WithMany()
                  .HasForeignKey(e => e.ContactUserId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Indexes for performance
            entity.HasIndex(e => new { e.UserId, e.ContactUserId }).IsUnique();
            entity.HasIndex(e => new { e.UserId, e.Status });
            entity.HasIndex(e => new { e.ContactUserId, e.Status });

            // Property configurations
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Message).HasMaxLength(500);

        });
    }
}
