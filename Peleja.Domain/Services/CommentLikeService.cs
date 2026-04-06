namespace Peleja.Domain.Services;

using AutoMapper;
using Peleja.Domain.Models;
using Peleja.DTO;
using Peleja.Infra.Interfaces.Repositories;

public class CommentLikeService
{
    private readonly ICommentLikeRepository<CommentLikeModel> _commentLikeRepository;
    private readonly ICommentRepository<CommentModel> _commentRepository;
    private readonly IMapper _mapper;

    public CommentLikeService(
        ICommentLikeRepository<CommentLikeModel> commentLikeRepository,
        ICommentRepository<CommentModel> commentRepository,
        IMapper mapper)
    {
        _commentLikeRepository = commentLikeRepository;
        _commentRepository = commentRepository;
        _mapper = mapper;
    }

    public async Task<CommentLikeResult> ToggleLikeAsync(long commentId, long userId)
    {
        var comment = await _commentRepository.GetByIdAsync(commentId);
        if (comment == null)
            throw new KeyNotFoundException("Comment not found");

        var existingLike = await _commentLikeRepository.GetAsync(commentId, userId);

        if (existingLike != null)
        {
            await _commentLikeRepository.DeleteAsync(existingLike);
            comment.DecrementLikeCount();
            await _commentRepository.UpdateAsync(comment);

            var result = _mapper.Map<CommentLikeResult>(comment);
            result.IsLikedByUser = false;
            return result;
        }
        else
        {
            var like = CommentLikeModel.Create(commentId, userId);
            await _commentLikeRepository.CreateAsync(like);
            comment.IncrementLikeCount();
            await _commentRepository.UpdateAsync(comment);

            var result = _mapper.Map<CommentLikeResult>(comment);
            result.IsLikedByUser = true;
            return result;
        }
    }
}
