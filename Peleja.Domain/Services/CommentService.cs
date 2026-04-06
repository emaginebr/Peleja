namespace Peleja.Domain.Services;

using Peleja.Domain.Models;
using Peleja.Domain.Models.DTOs;
using Peleja.Domain.Interfaces.Repositories;

public class CommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly ICommentLikeRepository _commentLikeRepository;
    private readonly IUserRepository _userRepository;

    public CommentService(
        ICommentRepository commentRepository,
        ICommentLikeRepository commentLikeRepository,
        IUserRepository userRepository)
    {
        _commentRepository = commentRepository;
        _commentLikeRepository = commentLikeRepository;
        _userRepository = userRepository;
    }

    public async Task<PaginatedResult<CommentResult>> GetByPageUrlAsync(
        long tenantId, string pageUrl, string sortBy, long? cursor, int pageSize, long? currentUserId)
    {
        // pageSize clamped to 1-50, default 15
        pageSize = Math.Clamp(pageSize, 1, 50);

        var comments = await _commentRepository.GetByPageUrlAsync(tenantId, pageUrl, sortBy, cursor, pageSize);

        var hasMore = comments.Count > pageSize;
        if (hasMore)
            comments = comments.Take(pageSize).ToList();

        var items = new List<CommentResult>();
        foreach (var comment in comments)
        {
            items.Add(await MapToResult(comment, currentUserId));
        }

        string? nextCursor = null;
        if (hasMore && items.Count > 0)
        {
            var lastItem = comments.Last();
            // For "popular" sort, cursor encodes like_count and comment_id
            // For "recent" sort, cursor is just comment_id
            nextCursor = sortBy == "popular"
                ? $"{lastItem.LikeCount}_{lastItem.CommentId}"
                : lastItem.CommentId.ToString();
        }

        return new PaginatedResult<CommentResult>
        {
            Items = items,
            NextCursor = nextCursor,
            HasMore = hasMore
        };
    }

    public async Task<CommentResult> CreateAsync(long tenantId, long userId, CommentInsertInfo info)
    {
        // Validate content
        if (string.IsNullOrWhiteSpace(info.Content))
            throw new ArgumentException("O conteúdo é obrigatório");
        if (info.Content.Length > 2000)
            throw new ArgumentException("O conteúdo deve ter no máximo 2000 caracteres");
        if (string.IsNullOrWhiteSpace(info.PageUrl))
            throw new ArgumentException("A URL da página é obrigatória");
        if (info.PageUrl.Length > 2000)
            throw new ArgumentException("A URL da página deve ter no máximo 2000 caracteres");
        if (info.GifUrl != null && info.GifUrl.Length > 500)
            throw new ArgumentException("A URL do GIF deve ter no máximo 500 caracteres");

        // Validate parentCommentId if provided
        if (info.ParentCommentId.HasValue)
        {
            var parent = await _commentRepository.GetByIdAsync(info.ParentCommentId.Value);
            if (parent == null)
                throw new KeyNotFoundException("Comentário pai não encontrado");
            if (parent.ParentCommentId != null)
                throw new ArgumentException("Não é possível responder a uma resposta");
            if (parent.TenantId != tenantId)
                throw new ArgumentException("Comentário pai pertence a outro tenant");
            if (parent.PageUrl != info.PageUrl)
                throw new ArgumentException("Comentário pai pertence a outra página");
        }

        var comment = new Comment
        {
            TenantId = tenantId,
            UserId = userId,
            ParentCommentId = info.ParentCommentId,
            PageUrl = info.PageUrl,
            Content = info.Content,
            GifUrl = info.GifUrl,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _commentRepository.CreateAsync(comment);
        return await MapToResult(created, userId);
    }

    public async Task<CommentResult> UpdateAsync(long commentId, long userId, CommentUpdateInfo info)
    {
        var comment = await _commentRepository.GetByIdAsync(commentId);
        if (comment == null)
            throw new KeyNotFoundException("Comentário não encontrado");
        if (comment.UserId != userId)
            throw new UnauthorizedAccessException("Apenas o autor pode editar o comentário");

        if (string.IsNullOrWhiteSpace(info.Content))
            throw new ArgumentException("O conteúdo é obrigatório");
        if (info.Content.Length > 2000)
            throw new ArgumentException("O conteúdo deve ter no máximo 2000 caracteres");
        if (info.GifUrl != null && info.GifUrl.Length > 500)
            throw new ArgumentException("A URL do GIF deve ter no máximo 500 caracteres");

        comment.Content = info.Content;
        comment.GifUrl = info.GifUrl;
        comment.IsEdited = true;
        comment.UpdatedAt = DateTime.UtcNow;

        var updated = await _commentRepository.UpdateAsync(comment);
        return await MapToResult(updated, userId);
    }

    public async Task DeleteAsync(long commentId, long userId, int userRole)
    {
        var comment = await _commentRepository.GetByIdAsync(commentId);
        if (comment == null)
            throw new KeyNotFoundException("Comentário não encontrado");

        // Check permission: author or moderator
        bool isAuthor = comment.UserId == userId;
        bool isModerator = userRole == (int)Enums.UserRole.Moderator;

        if (!isAuthor && !isModerator)
            throw new UnauthorizedAccessException("Sem permissão para excluir este comentário");

        comment.IsDeleted = true;
        comment.DeletedAt = DateTime.UtcNow;
        await _commentRepository.UpdateAsync(comment);
    }

    private async Task<CommentResult> MapToResult(Comment comment, long? currentUserId)
    {
        bool isLikedByUser = false;
        if (currentUserId.HasValue && !comment.IsDeleted)
        {
            isLikedByUser = await _commentLikeRepository.ExistsAsync(comment.CommentId, currentUserId.Value);
        }

        AuthorInfo? author = null;
        if (!comment.IsDeleted && comment.User != null)
        {
            author = new AuthorInfo
            {
                UserId = comment.User.UserId,
                DisplayName = comment.User.DisplayName,
                AvatarUrl = comment.User.AvatarUrl
            };
        }

        var result = new CommentResult
        {
            CommentId = comment.CommentId,
            Content = comment.IsDeleted ? "[Comentário removido]" : comment.Content,
            GifUrl = comment.IsDeleted ? null : comment.GifUrl,
            PageUrl = comment.PageUrl,
            IsEdited = comment.IsEdited,
            IsDeleted = comment.IsDeleted,
            LikeCount = comment.IsDeleted ? 0 : comment.LikeCount,
            IsLikedByUser = isLikedByUser,
            CreatedAt = comment.CreatedAt,
            ParentCommentId = comment.ParentCommentId,
            Author = author,
            Replies = new List<CommentResult>()
        };

        // Map replies if present (only for root comments)
        if (comment.Replies != null && comment.Replies.Count > 0)
        {
            foreach (var reply in comment.Replies.OrderBy(r => r.CreatedAt))
            {
                result.Replies.Add(await MapToResult(reply, currentUserId));
            }
        }

        return result;
    }
}
