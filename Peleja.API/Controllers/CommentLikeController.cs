namespace Peleja.API.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NAuth.ACL.Interfaces;
using Peleja.Domain.Services;

[ApiController]
[Route("api/v1/comments")]
public class CommentLikeController : ControllerBase
{
    private readonly CommentLikeService _commentLikeService;
    private readonly IUserClient _userClient;

    public CommentLikeController(CommentLikeService commentLikeService, IUserClient userClient)
    {
        _commentLikeService = commentLikeService;
        _userClient = userClient;
    }

    [HttpPost("{commentId}/like")]
    [Authorize]
    public async Task<IActionResult> ToggleLike(long commentId)
    {
        try
        {
            var userSession = _userClient.GetUserInSession(HttpContext);
            if (userSession == null)
                return Unauthorized();

            var result = await _commentLikeService.ToggleLikeAsync(commentId, userSession.UserId);

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
