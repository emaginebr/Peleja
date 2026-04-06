namespace Peleja.Domain.Services;

using Peleja.Domain.Models;
using Peleja.Domain.Models.DTOs;
using Peleja.Domain.Interfaces.Repositories;

public class CommentLikeService
{
    private readonly ICommentLikeRepository _commentLikeRepository;
    private readonly ICommentRepository _commentRepository;

    public CommentLikeService(
        ICommentLikeRepository commentLikeRepository,
        ICommentRepository commentRepository)
    {
        _commentLikeRepository = commentLikeRepository;
        _commentRepository = commentRepository;
    }

    public async Task<CommentLikeResult> ToggleLikeAsync(long commentId, long userId)
    {
        var comment = await _commentRepository.GetByIdAsync(commentId);
        if (comment == null)
            throw new KeyNotFoundException("Comentário não encontrado");

        var existingLike = await _commentLikeRepository.GetAsync(commentId, userId);

        if (existingLike != null)
        {
            // Unlike - remove
            await _commentLikeRepository.DeleteAsync(existingLike);
            comment.LikeCount = Math.Max(0, comment.LikeCount - 1);
            await _commentRepository.UpdateAsync(comment);

            return new CommentLikeResult
            {
                CommentId = commentId,
                LikeCount = comment.LikeCount,
                IsLikedByUser = false
            };
        }
        else
        {
            // Like - add
            var like = new CommentLike
            {
                CommentId = commentId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            await _commentLikeRepository.CreateAsync(like);
            comment.LikeCount += 1;
            await _commentRepository.UpdateAsync(comment);

            return new CommentLikeResult
            {
                CommentId = commentId,
                LikeCount = comment.LikeCount,
                IsLikedByUser = true
            };
        }
    }
}
