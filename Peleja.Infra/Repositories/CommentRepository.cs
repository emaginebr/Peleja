namespace Peleja.Infra.Repositories;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Peleja.Domain.Models;
using Peleja.Infra.Context;
using Peleja.Infra.Interfaces.Repositories;

public class CommentRepository : ICommentRepository<CommentModel>
{
    private readonly PelejaContext _context;
    private readonly IMapper _mapper;

    public CommentRepository(PelejaContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<CommentModel>> GetByPageIdAsync(long pageId, string sortBy, long? cursor, int pageSize)
    {
        var query = _context.Comments
            .IgnoreQueryFilters()
            .Where(c => c.PageId == pageId
                        && c.ParentCommentId == null
                        && !c.IsDeleted)
            .Include(c => c.Replies.Where(r => !r.IsDeleted))
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
        else
        {
            if (cursor.HasValue)
            {
                query = query.Where(c => c.CommentId < cursor.Value);
            }

            query = query.OrderByDescending(c => c.CommentId);
        }

        var entities = await query
            .Take(pageSize + 1)
            .ToListAsync();

        return _mapper.Map<List<CommentModel>>(entities);
    }

    public async Task<CommentModel?> GetByIdAsync(long commentId)
    {
        var entity = await _context.Comments
            .FirstOrDefaultAsync(c => c.CommentId == commentId);

        return entity != null ? _mapper.Map<CommentModel>(entity) : null;
    }

    public async Task<CommentModel> CreateAsync(CommentModel comment)
    {
        var entity = _mapper.Map<Context.Comment>(comment);
        _context.Comments.Add(entity);
        await _context.SaveChangesAsync();
        return _mapper.Map<CommentModel>(entity);
    }

    public async Task<CommentModel> UpdateAsync(CommentModel comment)
    {
        var entity = await _context.Comments
            .FirstOrDefaultAsync(c => c.CommentId == comment.CommentId);

        if (entity == null)
            throw new KeyNotFoundException("Comment not found");

        _mapper.Map(comment, entity);
        await _context.SaveChangesAsync();
        return _mapper.Map<CommentModel>(entity);
    }

    public async Task<int> GetRepliesCountAsync(long parentCommentId)
    {
        return await _context.Comments
            .CountAsync(c => c.ParentCommentId == parentCommentId);
    }

    public async Task<List<CommentModel>> GetRepliesAsync(long parentCommentId)
    {
        var entities = await _context.Comments
            .Where(c => c.ParentCommentId == parentCommentId)
            .OrderBy(c => c.CommentId)
            .AsNoTracking()
            .ToListAsync();

        return _mapper.Map<List<CommentModel>>(entities);
    }

    public async Task<bool> ExistsAsync(long commentId)
    {
        return await _context.Comments
            .AnyAsync(c => c.CommentId == commentId);
    }
}
