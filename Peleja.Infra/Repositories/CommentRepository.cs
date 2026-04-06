namespace Peleja.Infra.Repositories;

using Microsoft.EntityFrameworkCore;
using Peleja.Domain.Models;
using Peleja.Infra.Context;
using Peleja.Infra.Interfaces.Repositories;

public class CommentRepository : ICommentRepository
{
    private readonly PelejaContext _context;

    public CommentRepository(PelejaContext context)
    {
        _context = context;
    }

    public async Task<List<Comment>> GetByPageUrlAsync(long tenantId, string pageUrl, string sortBy, long? cursor, int pageSize)
    {
        // Use IgnoreQueryFilters to include soft-deleted replies (they will be marked as deleted)
        var query = _context.Comments
            .IgnoreQueryFilters()
            .Where(c => c.TenantId == tenantId
                        && c.PageUrl == pageUrl
                        && c.ParentCommentId == null
                        && !c.IsDeleted)
            .Include(c => c.User)
            .Include(c => c.Replies.Where(r => !r.IsDeleted))
                .ThenInclude(r => r.User)
            .AsNoTracking();

        if (sortBy == "popular")
        {
            if (cursor.HasValue)
            {
                var cursorComment = await _context.Comments
                    .AsNoTracking()
                    .IgnoreQueryFilters()
                    .Where(c => c.CommentId == cursor.Value)
                    .Select(c => new { c.LikeCount, c.CommentId })
                    .FirstOrDefaultAsync();

                if (cursorComment != null)
                {
                    query = query.Where(c =>
                        c.LikeCount < cursorComment.LikeCount ||
                        (c.LikeCount == cursorComment.LikeCount && c.CommentId < cursorComment.CommentId));
                }
            }

            query = query
                .OrderByDescending(c => c.LikeCount)
                .ThenByDescending(c => c.CommentId);
        }
        else // "recent"
        {
            if (cursor.HasValue)
            {
                query = query.Where(c => c.CommentId < cursor.Value);
            }

            query = query.OrderByDescending(c => c.CommentId);
        }

        return await query
            .Take(pageSize + 1)
            .ToListAsync();
    }

    public async Task<Comment?> GetByIdAsync(long commentId)
    {
        return await _context.Comments
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.CommentId == commentId);
    }

    public async Task<Comment> CreateAsync(Comment comment)
    {
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();
        return comment;
    }

    public async Task<Comment> UpdateAsync(Comment comment)
    {
        _context.Comments.Update(comment);
        await _context.SaveChangesAsync();
        return comment;
    }

    public async Task<int> GetRepliesCountAsync(long parentCommentId)
    {
        return await _context.Comments
            .CountAsync(c => c.ParentCommentId == parentCommentId);
    }

    public async Task<List<Comment>> GetRepliesAsync(long parentCommentId)
    {
        return await _context.Comments
            .Include(c => c.User)
            .Where(c => c.ParentCommentId == parentCommentId)
            .OrderBy(c => c.CommentId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(long commentId)
    {
        return await _context.Comments
            .AnyAsync(c => c.CommentId == commentId);
    }
}
