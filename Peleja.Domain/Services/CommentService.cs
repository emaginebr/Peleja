namespace Peleja.Domain.Services;

using AutoMapper;
using Peleja.Domain.Models;
using Peleja.DTO;
using Peleja.Domain.Interfaces.Repositories;

public class CommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly ICommentLikeRepository _commentLikeRepository;
    private readonly IPageRepository _pageRepository;
    private readonly IMapper _mapper;

    public CommentService(
        ICommentRepository commentRepository,
        ICommentLikeRepository commentLikeRepository,
        IPageRepository pageRepository,
        IMapper mapper)
    {
        _commentRepository = commentRepository;
        _commentLikeRepository = commentLikeRepository;
        _pageRepository = pageRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<CommentResult>> GetByPageUrlAsync(
        string pageUrl, string sortBy, long? cursor, int pageSize, long? currentUserId)
    {
        pageSize = Math.Clamp(pageSize, 1, 50);

        var page = await _pageRepository.GetByUrlAsync(pageUrl);
        if (page == null)
            return new PaginatedResult<CommentResult> { Items = new(), HasMore = false };

        var comments = await _commentRepository.GetByPageIdAsync(page.PageId, sortBy, cursor, pageSize);

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

    public async Task<CommentResult> CreateAsync(long userId, CommentInsertInfo info)
    {
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

        var page = await _pageRepository.GetByUrlAsync(info.PageUrl);
        if (page == null)
        {
            page = PageModel.Create(userId, info.PageUrl);
            page = await _pageRepository.CreateAsync(page);
        }

        if (info.ParentCommentId.HasValue)
        {
            var parent = await _commentRepository.GetByIdAsync(info.ParentCommentId.Value);
            if (parent == null)
                throw new KeyNotFoundException("Comentário pai não encontrado");
            if (parent.IsReply())
                throw new ArgumentException("Não é possível responder a uma resposta");
            if (parent.PageId != page.PageId)
                throw new ArgumentException("Comentário pai pertence a outra página");
        }

        var comment = CommentModel.Create(page.PageId, userId, info.Content, info.GifUrl, info.ParentCommentId);

        var created = await _commentRepository.CreateAsync(comment);
        return await MapToResult(created, userId);
    }

    public async Task<CommentResult> UpdateAsync(long commentId, long userId, CommentUpdateInfo info)
    {
        var comment = await _commentRepository.GetByIdAsync(commentId);
        if (comment == null)
            throw new KeyNotFoundException("Comentário não encontrado");
        if (!comment.IsOwnedBy(userId))
            throw new UnauthorizedAccessException("Apenas o autor pode editar o comentário");

        if (string.IsNullOrWhiteSpace(info.Content))
            throw new ArgumentException("O conteúdo é obrigatório");
        if (info.Content.Length > 2000)
            throw new ArgumentException("O conteúdo deve ter no máximo 2000 caracteres");
        if (info.GifUrl != null && info.GifUrl.Length > 500)
            throw new ArgumentException("A URL do GIF deve ter no máximo 500 caracteres");

        comment.Update(info.Content, info.GifUrl);

        var updated = await _commentRepository.UpdateAsync(comment);
        return await MapToResult(updated, userId);
    }

    public async Task DeleteAsync(long commentId, long userId, bool isAdmin)
    {
        var comment = await _commentRepository.GetByIdAsync(commentId);
        if (comment == null)
            throw new KeyNotFoundException("Comentário não encontrado");

        if (!comment.IsOwnedBy(userId) && !isAdmin)
            throw new UnauthorizedAccessException("Sem permissão para excluir este comentário");

        comment.Delete();
        await _commentRepository.UpdateAsync(comment);
    }

    private async Task<CommentResult> MapToResult(CommentModel comment, long? currentUserId)
    {
        var result = _mapper.Map<CommentResult>(comment);

        if (comment.IsDeleted)
        {
            result.Content = "[Comentário removido]";
            result.GifUrl = null;
            result.LikeCount = 0;
            result.UserId = 0;
            result.IsLikedByUser = false;
        }
        else if (currentUserId.HasValue)
        {
            result.IsLikedByUser = await _commentLikeRepository.ExistsAsync(comment.CommentId, currentUserId.Value);
        }

        if (comment.Replies != null && comment.Replies.Count > 0)
        {
            result.Replies = new List<CommentResult>();
            foreach (var reply in comment.Replies.OrderBy(r => r.CreatedAt))
            {
                result.Replies.Add(await MapToResult(reply, currentUserId));
            }
        }

        return result;
    }
}
