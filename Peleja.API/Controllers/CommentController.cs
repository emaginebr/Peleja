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
    private readonly ILogger<CommentController> _logger;

    public CommentController(CommentService commentService, IUserClient userClient, ILogger<CommentController> logger)
    {
        _commentService = commentService;
        _userClient = userClient;
        _logger = logger;
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
                return BadRequest("The pageUrl parameter is required");

            var siteId = GetSiteId();
            if (siteId == 0)
                return BadRequest("The X-Client-Id header is required");

            if (!SiteAllowsWrite() && false) { } // read is always allowed if not blocked (middleware handles blocked)

            long? currentUserId = null;
            var userSession = _userClient.GetUserInSession(HttpContext);
            if (userSession != null)
                currentUserId = userSession.UserId;

            long? cursorValue = null;
            if (!string.IsNullOrEmpty(cursor) && long.TryParse(cursor.Split('_').Last(), out var cv))
                cursorValue = cv;

            var result = await _commentService.GetByPageUrlAsync(
                siteId, pageUrl, sortBy, cursorValue, pageSize, currentUserId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting comments for pageUrl={PageUrl}", pageUrl);
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateComment([FromBody] CommentInsertInfo info)
    {
        try
        {
            var siteId = GetSiteId();
            if (siteId == 0)
                return BadRequest("The X-Client-Id header is required");
            if (!SiteAllowsWrite())
                return StatusCode(403, "Site is not accepting new comments");

            var userSession = _userClient.GetUserInSession(HttpContext);
            if (userSession == null)
                return Unauthorized();

            var result = await _commentService.CreateAsync(siteId, userSession.UserId, info);

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
            _logger.LogError(ex, "Error creating comment");
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPut("{commentId}")]
    [Authorize]
    public async Task<IActionResult> UpdateComment(long commentId, [FromBody] CommentUpdateInfo info)
    {
        try
        {
            if (!SiteAllowsWrite())
                return StatusCode(403, "Site is not accepting modifications");

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
            _logger.LogError(ex, "Error updating comment {CommentId}", commentId);
            return StatusCode(500, ex.Message);
        }
    }

    [HttpDelete("{commentId}")]
    [Authorize]
    public async Task<IActionResult> DeleteComment(long commentId)
    {
        try
        {
            if (!SiteAllowsWrite())
                return StatusCode(403, "Site is not accepting modifications");

            var userSession = _userClient.GetUserInSession(HttpContext);
            if (userSession == null)
                return Unauthorized();

            var siteAdminUserId = GetSiteAdminUserId();
            await _commentService.DeleteAsync(commentId, userSession.UserId, userSession.IsAdmin, siteAdminUserId);

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
            _logger.LogError(ex, "Error deleting comment {CommentId}", commentId);
            return StatusCode(500, ex.Message);
        }
    }

    private long GetSiteId()
    {
        if (HttpContext.Items.TryGetValue("SiteId", out var siteId) && siteId is long id)
            return id;
        return 0;
    }

    private long GetSiteAdminUserId()
    {
        if (HttpContext.Items.TryGetValue("SiteUserId", out var userId) && userId is long id)
            return id;
        return 0;
    }

    private bool SiteAllowsWrite()
    {
        if (HttpContext.Items.TryGetValue("SiteAllowsWrite", out var allows) && allows is bool val)
            return val;
        return false;
    }
}
