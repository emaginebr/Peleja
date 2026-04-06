namespace Peleja.API.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Peleja.Domain.Services;

[ApiController]
[Route("api/v1/comments")]
public class CommentLikeController : ControllerBase
{
    private readonly CommentLikeService _commentLikeService;

    public CommentLikeController(CommentLikeService commentLikeService)
    {
        _commentLikeService = commentLikeService;
    }

    [HttpPost("{commentId}/like")]
    [Authorize]
    public async Task<IActionResult> ToggleLike(long commentId)
    {
        try
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (!long.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var result = await _commentLikeService.ToggleLikeAsync(commentId, userId);

            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
