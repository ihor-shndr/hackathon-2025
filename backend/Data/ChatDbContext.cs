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
    public DbSet<Group> Groups { get; set; }
    public DbSet<GroupMember> GroupMembers { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<MessageReaction> MessageReactions { get; set; }

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

        // Configure Group entity
        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Foreign key relationship with owner
            entity.HasOne(e => e.Owner)
                  .WithMany()
                  .HasForeignKey(e => e.OwnerId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Property configurations
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Index for active groups
            entity.HasIndex(e => e.IsActive);
        });

        // Configure GroupMember entity
        modelBuilder.Entity<GroupMember>(entity =>
        {
            // Composite primary key
            entity.HasKey(e => new { e.GroupId, e.UserId });

            // Foreign key relationships
            entity.HasOne(e => e.Group)
                  .WithMany(g => g.Members)
                  .HasForeignKey(e => e.GroupId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Property configurations
            entity.Property(e => e.JoinedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Indexes for performance
            entity.HasIndex(e => new { e.GroupId, e.IsActive });
            entity.HasIndex(e => new { e.UserId, e.IsActive });
        });

        // Configure Message entity
        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Foreign key relationships
            entity.HasOne(e => e.Sender)
                  .WithMany()
                  .HasForeignKey(e => e.SenderId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Group)
                  .WithMany(g => g.Messages)
                  .HasForeignKey(e => e.GroupId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .IsRequired(false);

            entity.HasOne(e => e.Recipient)
                  .WithMany()
                  .HasForeignKey(e => e.RecipientId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .IsRequired(false);

            // Property configurations
            entity.Property(e => e.Content).HasMaxLength(4000).IsRequired();
            entity.Property(e => e.AttachmentUrl).HasMaxLength(500);
            entity.Property(e => e.SentAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Indexes for performance
            entity.HasIndex(e => new { e.GroupId, e.SentAt });
            entity.HasIndex(e => new { e.SenderId, e.RecipientId, e.SentAt });
            entity.HasIndex(e => e.SentAt);
            entity.HasIndex(e => e.IsDeleted);
        });

        // Configure MessageReaction entity
        modelBuilder.Entity<MessageReaction>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Foreign key relationships
            entity.HasOne(e => e.Message)
                  .WithMany(m => m.Reactions)
                  .HasForeignKey(e => e.MessageId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Property configurations
            entity.Property(e => e.Emoji).HasMaxLength(10).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Unique constraint: one reaction per user per message per emoji
            entity.HasIndex(e => new { e.MessageId, e.UserId, e.Emoji }).IsUnique();

            // Index for performance
            entity.HasIndex(e => e.MessageId);
        });
    }
}
