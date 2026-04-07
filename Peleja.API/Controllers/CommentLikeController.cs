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
    private readonly ILogger<CommentLikeController> _logger;

    public CommentLikeController(CommentLikeService commentLikeService, IUserClient userClient, ILogger<CommentLikeController> logger)
    {
        _commentLikeService = commentLikeService;
        _userClient = userClient;
        _logger = logger;
    }

    [HttpPost("{commentId}/like")]
    [Authorize]
    public async Task<IActionResult> ToggleLike(long commentId)
    {
        try
        {
            if (HttpContext.Items.TryGetValue("SiteAllowsWrite", out var allows) && allows is bool val && !val)
                return StatusCode(403, "Site is not accepting modifications");

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
            _logger.LogError(ex, "Error toggling like for comment {CommentId}", commentId);
            return StatusCode(500, ex.Message);
        }
    }
}
