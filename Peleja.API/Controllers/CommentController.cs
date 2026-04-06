namespace Peleja.API.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NAuth.ACL.Interfaces;
using Peleja.DTO;
using Peleja.Domain.Services;

[ApiController]
[Route("api/v1/comments")]
public class CommentController : ControllerBase
{
    private readonly CommentService _commentService;
    private readonly IUserClient _userClient;

    public CommentController(CommentService commentService, IUserClient userClient)
    {
        _commentService = commentService;
        _userClient = userClient;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetComments(
        [FromQuery] string pageUrl,
        [FromQuery] string sortBy = "recent",
        [FromQuery] string? cursor = null,
        [FromQuery] int pageSize = 15)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(pageUrl))
                return BadRequest("O parâmetro pageUrl é obrigatório");

            long? currentUserId = null;
            var userSession = _userClient.GetUserInSession(HttpContext);
            if (userSession != null)
                currentUserId = userSession.UserId;

            long? cursorValue = null;
            if (!string.IsNullOrEmpty(cursor) && long.TryParse(cursor.Split('_').Last(), out var cv))
                cursorValue = cv;

            var result = await _commentService.GetByPageUrlAsync(
                pageUrl, sortBy, cursorValue, pageSize, currentUserId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateComment([FromBody] CommentInsertInfo info)
    {
        try
        {
            var userSession = _userClient.GetUserInSession(HttpContext);
            if (userSession == null)
                return Unauthorized();

            var result = await _commentService.CreateAsync(userSession.UserId, info);

            return CreatedAtAction(nameof(GetComments), new { pageUrl = info.PageUrl }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPut("{commentId}")]
    [Authorize]
    public async Task<IActionResult> UpdateComment(long commentId, [FromBody] CommentUpdateInfo info)
    {
        try
        {
            var userSession = _userClient.GetUserInSession(HttpContext);
            if (userSession == null)
                return Unauthorized();

            var result = await _commentService.UpdateAsync(commentId, userSession.UserId, info);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpDelete("{commentId}")]
    [Authorize]
    public async Task<IActionResult> DeleteComment(long commentId)
    {
        try
        {
            var userSession = _userClient.GetUserInSession(HttpContext);
            if (userSession == null)
                return Unauthorized();

            await _commentService.DeleteAsync(commentId, userSession.UserId, userSession.IsAdmin);

            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
