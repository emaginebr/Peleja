namespace Peleja.Infra.Repositories;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Peleja.Domain.Models;
using Peleja.Infra.Context;
using Peleja.Infra.Interfaces.Repositories;

public class CommentLikeRepository : ICommentLikeRepository<CommentLikeModel>
{
    private readonly PelejaContext _context;
    private readonly IMapper _mapper;

    public CommentLikeRepository(PelejaContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<CommentLikeModel?> GetAsync(long commentId, long userId)
    {
        var entity = await _context.CommentLikes
            .FirstOrDefaultAsync(cl => cl.CommentId == commentId && cl.UserId == userId);

        return entity != null ? _mapper.Map<CommentLikeModel>(entity) : null;
    }

    public async Task<CommentLikeModel> CreateAsync(CommentLikeModel commentLike)
    {
        var entity = _mapper.Map<Context.CommentLike>(commentLike);
        _context.CommentLikes.Add(entity);
        await _context.SaveChangesAsync();
        return _mapper.Map<CommentLikeModel>(entity);
    }

    public async Task DeleteAsync(CommentLikeModel commentLike)
    {
        var entity = await _context.CommentLikes
            .FirstOrDefaultAsync(cl => cl.CommentLikeId == commentLike.CommentLikeId);

        if (entity != null)
        {
            _context.CommentLikes.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(long commentId, long userId)
    {
        return await _context.CommentLikes
            .AnyAsync(cl => cl.CommentId == commentId && cl.UserId == userId);
    }
}
