namespace Peleja.Infra.Context;

using Microsoft.EntityFrameworkCore;

public class PelejaContext : DbContext
{
    public PelejaContext(DbContextOptions<PelejaContext> options) : base(options)
    {
    }

    public DbSet<Page> Pages { get; set; } = null!;
    public DbSet<Comment> Comments { get; set; } = null!;
    public DbSet<CommentLike> CommentLikes { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── Page ────────────────────────────────────────────────
        modelBuilder.Entity<Page>(entity =>
        {
            entity.ToTable("pages");

            entity.HasKey(e => e.PageId)
                .HasName("pages_pkey");

            entity.Property(e => e.PageId)
                .HasColumnName("page_id")
                .UseIdentityAlwaysColumn();

            entity.Property(e => e.UserId)
                .HasColumnName("user_id");

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

            entity.HasIndex(e => e.PageUrl)
                .IsUnique()
                .HasDatabaseName("ix_pages_page_url");
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
                .HasConstraintName("fk_pages_comments");

            entity.HasOne(e => e.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(e => e.ParentCommentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_comments_comments");

            entity.HasQueryFilter(c => !c.IsDeleted);

            entity.HasIndex(e => new { e.PageId, e.IsDeleted })
                .HasDatabaseName("ix_comments_page");

            entity.HasIndex(e => new { e.PageId, e.LikeCount, e.CommentId })
                .IsDescending(false, true, true)
                .HasFilter("is_deleted = false AND parent_comment_id IS NULL")
                .HasDatabaseName("ix_comments_popular");

            entity.HasIndex(e => new { e.PageId, e.CommentId })
                .IsDescending(false, true)
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

            entity.HasIndex(e => new { e.CommentId, e.UserId })
                .IsUnique()
                .HasDatabaseName("ix_comment_likes_unique");
        });
    }
}
