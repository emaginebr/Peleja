namespace Peleja.Infra.Context;

using Microsoft.EntityFrameworkCore;
using Peleja.Domain.Enums;

public class PelejaContext : DbContext
{
    public PelejaContext(DbContextOptions<PelejaContext> options) : base(options)
    {
    }

    public DbSet<Site> Sites { get; set; } = null!;
    public DbSet<Page> Pages { get; set; } = null!;
    public DbSet<Comment> Comments { get; set; } = null!;
    public DbSet<CommentLike> CommentLikes { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── Site ────────────────────────────────────────────────
        modelBuilder.Entity<Site>(entity =>
        {
            entity.ToTable("peleja_sites");

            entity.HasKey(e => e.SiteId)
                .HasName("peleja_sites_pkey");

            entity.Property(e => e.SiteId)
                .HasColumnName("site_id")
                .UseIdentityAlwaysColumn();

            entity.Property(e => e.ClientId)
                .HasColumnName("client_id")
                .HasMaxLength(32)
                .IsRequired();

            entity.Property(e => e.SiteUrl)
                .HasColumnName("site_url")
                .HasMaxLength(2000)
                .IsRequired();

            entity.Property(e => e.Tenant)
                .HasColumnName("tenant")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.UserId)
                .HasColumnName("user_id");

            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasDefaultValue(SiteStatus.Active);

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp without time zone");

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("timestamp without time zone");

            entity.HasIndex(e => e.ClientId)
                .IsUnique()
                .HasDatabaseName("ix_peleja_sites_client_id");

            entity.HasIndex(e => e.SiteUrl)
                .IsUnique()
                .HasDatabaseName("ix_peleja_sites_site_url");

            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("ix_peleja_sites_user_id");
        });

        // ── Page ────────────────────────────────────────────────
        modelBuilder.Entity<Page>(entity =>
        {
            entity.ToTable("peleja_pages");

            entity.HasKey(e => e.PageId)
                .HasName("peleja_pages_pkey");

            entity.Property(e => e.PageId)
                .HasColumnName("page_id")
                .UseIdentityAlwaysColumn();

            entity.Property(e => e.SiteId)
                .HasColumnName("site_id");

            entity.Property(e => e.PageUrl)
                .HasColumnName("page_url")
                .HasMaxLength(2000)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp without time zone");

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("timestamp without time zone");

            entity.HasIndex(e => new { e.SiteId, e.PageUrl })
                .IsUnique()
                .HasDatabaseName("ix_peleja_pages_site_page_url");
        });

        // ── Comment ─────────────────────────────────────────────
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.ToTable("peleja_comments");

            entity.HasKey(e => e.CommentId)
                .HasName("peleja_comments_pkey");

            entity.Property(e => e.CommentId)
                .HasColumnName("comment_id")
                .UseIdentityAlwaysColumn();

            entity.Property(e => e.PageId)
                .HasColumnName("page_id");

            entity.Property(e => e.UserId)
                .HasColumnName("user_id");

            entity.Property(e => e.ParentCommentId)
                .HasColumnName("parent_comment_id");

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

            entity.HasOne(e => e.Page)
                .WithMany(p => p.Comments)
                .HasForeignKey(e => e.PageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_peleja_pages_comments");

            entity.HasOne(e => e.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(e => e.ParentCommentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_peleja_comments_comments");

            entity.HasQueryFilter(c => !c.IsDeleted);

            entity.HasIndex(e => new { e.PageId, e.IsDeleted })
                .HasDatabaseName("ix_peleja_comments_page");

            entity.HasIndex(e => new { e.PageId, e.LikeCount, e.CommentId })
                .IsDescending(false, true, true)
                .HasFilter("is_deleted = false AND parent_comment_id IS NULL")
                .HasDatabaseName("ix_peleja_comments_popular");

            entity.HasIndex(e => new { e.PageId, e.CommentId })
                .IsDescending(false, true)
                .HasFilter("is_deleted = false AND parent_comment_id IS NULL")
                .HasDatabaseName("ix_peleja_comments_recent");

            entity.HasIndex(e => e.ParentCommentId)
                .HasFilter("parent_comment_id IS NOT NULL")
                .HasDatabaseName("ix_peleja_comments_parent");
        });

        // ── CommentLike ─────────────────────────────────────────
        modelBuilder.Entity<CommentLike>(entity =>
        {
            entity.ToTable("peleja_comment_likes");

            entity.HasKey(e => e.CommentLikeId)
                .HasName("peleja_comment_likes_pkey");

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
                .HasConstraintName("fk_peleja_comments_comment_likes");

            entity.HasIndex(e => new { e.CommentId, e.UserId })
                .IsUnique()
                .HasDatabaseName("ix_peleja_comment_likes_unique");
        });
    }
}
