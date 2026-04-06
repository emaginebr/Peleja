namespace Peleja.Infra.Context;

using Microsoft.EntityFrameworkCore;
using Peleja.Domain.Enums;
using Peleja.Domain.Models;

public class PelejaContext : DbContext
{
    public PelejaContext(DbContextOptions<PelejaContext> options) : base(options)
    {
    }

    public DbSet<Tenant> Tenants { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Comment> Comments { get; set; } = null!;
    public DbSet<CommentLike> CommentLikes { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── Tenant ──────────────────────────────────────────────
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.ToTable("tenants");

            entity.HasKey(e => e.TenantId)
                .HasName("tenants_pkey");

            entity.Property(e => e.TenantId)
                .HasColumnName("tenant_id")
                .UseIdentityAlwaysColumn();

            entity.Property(e => e.Name)
                .HasColumnName("name")
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(e => e.Slug)
                .HasColumnName("slug")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.NauthApiUrl)
                .HasColumnName("nauth_api_url")
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(e => e.NauthApiKey)
                .HasColumnName("nauth_api_key")
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true);

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp without time zone");

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("timestamp without time zone");

            entity.HasIndex(e => e.Slug)
                .IsUnique()
                .HasDatabaseName("ix_tenants_slug");
        });

        // ── User ────────────────────────────────────────────────
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");

            entity.HasKey(e => e.UserId)
                .HasName("users_pkey");

            entity.Property(e => e.UserId)
                .HasColumnName("user_id")
                .UseIdentityAlwaysColumn();

            entity.Property(e => e.TenantId)
                .HasColumnName("tenant_id");

            entity.Property(e => e.NauthUserId)
                .HasColumnName("nauth_user_id")
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(e => e.DisplayName)
                .HasColumnName("display_name")
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(e => e.AvatarUrl)
                .HasColumnName("avatar_url")
                .HasMaxLength(1000);

            entity.Property(e => e.Role)
                .HasColumnName("role")
                .HasDefaultValue(UserRole.User);

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp without time zone");

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("timestamp without time zone");

            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.Users)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_tenants_users");

            entity.HasIndex(e => new { e.TenantId, e.NauthUserId })
                .IsUnique()
                .HasDatabaseName("ix_users_tenant_nauth");
        });

        // ── Comment ─────────────────────────────────────────────
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.ToTable("comments");

            entity.HasKey(e => e.CommentId)
                .HasName("comments_pkey");

            entity.Property(e => e.CommentId)
                .HasColumnName("comment_id")
                .UseIdentityAlwaysColumn();

            entity.Property(e => e.TenantId)
                .HasColumnName("tenant_id");

            entity.Property(e => e.UserId)
                .HasColumnName("user_id");

            entity.Property(e => e.ParentCommentId)
                .HasColumnName("parent_comment_id");

            entity.Property(e => e.PageUrl)
                .HasColumnName("page_url")
                .HasMaxLength(2000)
                .IsRequired();

            entity.Property(e => e.Content)
                .HasColumnName("content")
                .HasMaxLength(5000)
                .IsRequired();

            entity.Property(e => e.GifUrl)
                .HasColumnName("gif_url")
                .HasMaxLength(2000);

            entity.Property(e => e.LikeCount)
                .HasColumnName("like_count")
                .HasDefaultValue(0);

            entity.Property(e => e.IsEdited)
                .HasColumnName("is_edited")
                .HasDefaultValue(false);

            entity.Property(e => e.IsDeleted)
                .HasColumnName("is_deleted")
                .HasDefaultValue(false);

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp without time zone");

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("timestamp without time zone");

            entity.Property(e => e.DeletedAt)
                .HasColumnName("deleted_at")
                .HasColumnType("timestamp without time zone");

            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.Comments)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_tenants_comments");

            entity.HasOne(e => e.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_users_comments");

            entity.HasOne(e => e.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(e => e.ParentCommentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_comments_comments");

            // Global query filter for soft delete
            entity.HasQueryFilter(c => !c.IsDeleted);

            // Indexes
            entity.HasIndex(e => new { e.TenantId, e.PageUrl, e.IsDeleted })
                .HasDatabaseName("ix_comments_page_url");

            entity.HasIndex(e => new { e.TenantId, e.PageUrl, e.LikeCount, e.CommentId })
                .IsDescending(false, false, true, true)
                .HasFilter("is_deleted = false AND parent_comment_id IS NULL")
                .HasDatabaseName("ix_comments_popular");

            entity.HasIndex(e => new { e.TenantId, e.PageUrl, e.CommentId })
                .IsDescending(false, false, true)
                .HasFilter("is_deleted = false AND parent_comment_id IS NULL")
                .HasDatabaseName("ix_comments_recent");

            entity.HasIndex(e => e.ParentCommentId)
                .HasFilter("parent_comment_id IS NOT NULL")
                .HasDatabaseName("ix_comments_parent");
        });

        // ── CommentLike ─────────────────────────────────────────
        modelBuilder.Entity<CommentLike>(entity =>
        {
            entity.ToTable("comment_likes");

            entity.HasKey(e => e.CommentLikeId)
                .HasName("comment_likes_pkey");

            entity.Property(e => e.CommentLikeId)
                .HasColumnName("comment_like_id")
                .UseIdentityAlwaysColumn();

            entity.Property(e => e.CommentId)
                .HasColumnName("comment_id");

            entity.Property(e => e.UserId)
                .HasColumnName("user_id");

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp without time zone");

            entity.HasOne(e => e.Comment)
                .WithMany(c => c.CommentLikes)
                .HasForeignKey(e => e.CommentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_comments_comment_likes");

            entity.HasOne(e => e.User)
                .WithMany(u => u.CommentLikes)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_users_comment_likes");

            entity.HasIndex(e => new { e.CommentId, e.UserId })
                .IsUnique()
                .HasDatabaseName("ix_comment_likes_unique");
        });
    }
}
