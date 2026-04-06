namespace Peleja.API.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Peleja.API.Middleware;
using Peleja.Domain.Models.DTOs;
using Peleja.Domain.Services;

[ApiController]
[Route("api/v1/comments")]
public class CommentController : ControllerBase
{
    private readonly CommentService _commentService;
    private readonly TenantContext _tenantContext;

    public CommentController(CommentService commentService, TenantContext tenantContext)
    {
        _commentService = commentService;
        _tenantContext = tenantContext;
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
            if (User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (long.TryParse(userIdClaim, out var uid))
                    currentUserId = uid;
            }

            long? cursorValue = null;
            if (!string.IsNullOrEmpty(cursor) && long.TryParse(cursor.Split('_').Last(), out var cv))
                cursorValue = cv;

            var result = await _commentService.GetByPageUrlAsync(
                _tenantContext.TenantId, pageUrl, sortBy, cursorValue, pageSize, currentUserId);

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
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized();

            var result = await _commentService.CreateAsync(_tenantContext.TenantId, userId, info);

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
            var userId = GetCurrentUserId();
            var result = await _commentService.UpdateAsync(commentId, userId, info);

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
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            await _commentService.DeleteAsync(commentId, userId, userRole);

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

    private long GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("UserId")?.Value;
        return long.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    private int GetCurrentUserRole()
    {
        var roleClaim = User.FindFirst("Role")?.Value;
        return int.TryParse(roleClaim, out var role) ? role : 1;
    }
}
