namespace Peleja.Infra.Repositories;

using Microsoft.EntityFrameworkCore;
using Peleja.Domain.Models;
using Peleja.Infra.Context;
using Peleja.Infra.Interfaces.Repositories;

public class CommentLikeRepository : ICommentLikeRepository
{
    private readonly PelejaContext _context;

    public CommentLikeRepository(PelejaContext context)
    {
        _context = context;
    }

    public async Task<CommentLike?> GetAsync(long commentId, long userId)
    {
        return await _context.CommentLikes
            .FirstOrDefaultAsync(cl => cl.CommentId == commentId && cl.UserId == userId);
    }

    public async Task<CommentLike> CreateAsync(CommentLike commentLike)
    {
        _context.CommentLikes.Add(commentLike);
        await _context.SaveChangesAsync();
        return commentLike;
    }

    public async Task DeleteAsync(CommentLike commentLike)
    {
        _context.CommentLikes.Remove(commentLike);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(long commentId, long userId)
    {
        return await _context.CommentLikes
            .AnyAsync(cl => cl.CommentId == commentId && cl.UserId == userId);
    }
}
