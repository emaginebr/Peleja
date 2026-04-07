namespace Peleja.Domain.Services;

using AutoMapper;
using Microsoft.Extensions.Logging;
using Peleja.Domain.Models;
using Peleja.DTO;
using Peleja.Infra.Interfaces.Repositories;

public class CommentService
{
    private readonly ICommentRepository<CommentModel> _commentRepository;
    private readonly ICommentLikeRepository<CommentLikeModel> _commentLikeRepository;
    private readonly IPageRepository<PageModel> _pageRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CommentService> _logger;

    public CommentService(
        ICommentRepository<CommentModel> commentRepository,
        ICommentLikeRepository<CommentLikeModel> commentLikeRepository,
        IPageRepository<PageModel> pageRepository,
        IMapper mapper,
        ILogger<CommentService> logger)
    {
        _commentRepository = commentRepository;
        _commentLikeRepository = commentLikeRepository;
        _pageRepository = pageRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PaginatedResult<CommentResult>> GetByPageUrlAsync(
        long siteId, string pageUrl, string sortBy, long? cursor, int pageSize, long? currentUserId)
    {
        pageSize = Math.Clamp(pageSize, 1, 50);

        var page = await _pageRepository.GetByUrlAndSiteIdAsync(siteId, pageUrl);
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

    public async Task<CommentResult> CreateAsync(long siteId, long userId, string? userName, string? userImageUrl, CommentInsertInfo info)
    {
        if (string.IsNullOrWhiteSpace(info.Content))
            throw new ArgumentException("Content is required");
        if (info.Content.Length > 2000)
            throw new ArgumentException("Content must not exceed 2000 characters");
        if (string.IsNullOrWhiteSpace(info.PageUrl))
            throw new ArgumentException("Page URL is required");
        if (info.PageUrl.Length > 2000)
            throw new ArgumentException("Page URL must not exceed 2000 characters");
        if (info.GifUrl != null && info.GifUrl.Length > 500)
            throw new ArgumentException("GIF URL must not exceed 500 characters");

        var page = await _pageRepository.GetByUrlAndSiteIdAsync(siteId, info.PageUrl);
        if (page == null)
        {
            page = PageModel.Create(siteId, info.PageUrl);
            page = await _pageRepository.CreateAsync(page);
        }

        if (info.ParentCommentId.HasValue)
        {
            var parent = await _commentRepository.GetByIdAsync(info.ParentCommentId.Value);
            if (parent == null)
                throw new KeyNotFoundException("Parent comment not found");
            if (parent.IsReply())
                throw new ArgumentException("Cannot reply to a reply");
            if (parent.PageId != page.PageId)
                throw new ArgumentException("Parent comment belongs to a different page");
        }

        var comment = CommentModel.Create(page.PageId, userId, userName, userImageUrl, info.Content, info.GifUrl, info.ParentCommentId);

        var created = await _commentRepository.CreateAsync(comment);
        return await MapToResult(created, userId);
    }

    public async Task<CommentResult> UpdateAsync(long commentId, long userId, CommentUpdateInfo info)
    {
        var comment = await _commentRepository.GetByIdAsync(commentId);
        if (comment == null)
            throw new KeyNotFoundException("Comment not found");
        if (!comment.IsOwnedBy(userId))
            throw new UnauthorizedAccessException("Only the author can edit this comment");

        if (string.IsNullOrWhiteSpace(info.Content))
            throw new ArgumentException("Content is required");
        if (info.Content.Length > 2000)
            throw new ArgumentException("Content must not exceed 2000 characters");
        if (info.GifUrl != null && info.GifUrl.Length > 500)
            throw new ArgumentException("GIF URL must not exceed 500 characters");

        comment.Update(info.Content, info.GifUrl);

        var updated = await _commentRepository.UpdateAsync(comment);
        return await MapToResult(updated, userId);
    }

    public async Task<PaginatedResult<CommentResult>> GetByPageIdAuthenticatedAsync(
        long siteId, long pageId, long userId, string sortBy, long? cursor, int pageSize)
    {
        pageSize = Math.Clamp(pageSize, 1, 50);

        // Validate page belongs to site
        var page = await _pageRepository.GetByIdAndSiteIdAsync(pageId, siteId);
        if (page == null)
            throw new KeyNotFoundException("Page not found");

        var comments = await _commentRepository.GetByPageIdAsync(page.PageId, sortBy, cursor, pageSize);

        var hasMore = comments.Count > pageSize;
        if (hasMore)
            comments = comments.Take(pageSize).ToList();

        var items = new List<CommentResult>();
        foreach (var comment in comments)
        {
            items.Add(await MapToResult(comment, userId));
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

    public async Task DeleteAsync(long commentId, long userId, bool isAdmin, long siteAdminUserId = 0)
    {
        var comment = await _commentRepository.GetByIdAsync(commentId);
        if (comment == null)
            throw new KeyNotFoundException("Comment not found");

        bool isSiteAdmin = siteAdminUserId > 0 && siteAdminUserId == userId;

        if (!comment.IsOwnedBy(userId) && !isAdmin && !isSiteAdmin)
            throw new UnauthorizedAccessException("You do not have permission to delete this comment");

        comment.Delete();
        await _commentRepository.UpdateAsync(comment);
    }

    private async Task<CommentResult> MapToResult(CommentModel comment, long? currentUserId)
    {
        var result = _mapper.Map<CommentResult>(comment);

        if (comment.IsDeleted)
        {
            result.Content = "[Comment removed]";
            result.GifUrl = null;
            result.LikeCount = 0;
            result.UserId = 0;
            result.UserName = null;
            result.UserImageUrl = null;
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
