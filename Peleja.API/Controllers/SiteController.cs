namespace Peleja.API.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NAuth.ACL.Interfaces;
using Peleja.DTO;
using Peleja.Domain.Services;

[ApiController]
[Route("api/v1/sites")]
[Authorize]
public class SiteController : ControllerBase
{
    private readonly SiteService _siteService;
    private readonly PageService _pageService;
    private readonly CommentService _commentService;
    private readonly IUserClient _userClient;
    private readonly ILogger<SiteController> _logger;

    public SiteController(
        SiteService siteService,
        PageService pageService,
        CommentService commentService,
        IUserClient userClient,
        ILogger<SiteController> logger)
    {
        _siteService = siteService;
        _pageService = pageService;
        _commentService = commentService;
        _userClient = userClient;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateSite([FromBody] SiteInsertInfo info)
    {
        try
        {
            var userSession = _userClient.GetUserInSession(HttpContext);
            if (userSession == null)
                return Unauthorized();

            var result = await _siteService.CreateAsync(userSession.UserId, info);

            return CreatedAtAction(nameof(ListSites), null, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating site");
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> ListSites(
        [FromQuery] string? cursor = null,
        [FromQuery] int pageSize = 15)
    {
        try
        {
            var userSession = _userClient.GetUserInSession(HttpContext);
            if (userSession == null)
                return Unauthorized();

            long? cursorValue = null;
            if (!string.IsNullOrEmpty(cursor) && long.TryParse(cursor, out var cv))
                cursorValue = cv;

            var result = await _siteService.ListByUserIdAsync(userSession.UserId, cursorValue, pageSize);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing sites");
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPut("{siteId}")]
    public async Task<IActionResult> UpdateSite(long siteId, [FromBody] SiteUpdateInfo info)
    {
        try
        {
            var userSession = _userClient.GetUserInSession(HttpContext);
            if (userSession == null)
                return Unauthorized();

            var result = await _siteService.UpdateAsync(siteId, userSession.UserId, info);

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
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating site {SiteId}", siteId);
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet("{siteId}/pages")]
    public async Task<IActionResult> ListPages(
        long siteId,
        [FromQuery] string? cursor = null,
        [FromQuery] int pageSize = 15)
    {
        try
        {
            var userSession = _userClient.GetUserInSession(HttpContext);
            if (userSession == null)
                return Unauthorized();

            long? cursorValue = null;
            if (!string.IsNullOrEmpty(cursor) && long.TryParse(cursor, out var cv))
                cursorValue = cv;

            var result = await _pageService.GetBySiteIdAsync(siteId, userSession.UserId, cursorValue, pageSize);

            return Ok(result);
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
            _logger.LogError(ex, "Error listing pages for site {SiteId}", siteId);
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet("{siteId}/pages/{pageId}/comments")]
    public async Task<IActionResult> ListCommentsByPage(
        long siteId,
        long pageId,
        [FromQuery] string sortBy = "recent",
        [FromQuery] string? cursor = null,
        [FromQuery] int pageSize = 15)
    {
        try
        {
            var userSession = _userClient.GetUserInSession(HttpContext);
            if (userSession == null)
                return Unauthorized();

            long? cursorValue = null;
            if (!string.IsNullOrEmpty(cursor) && long.TryParse(cursor.Split('_').Last(), out var cv))
                cursorValue = cv;

            var result = await _commentService.GetByPageIdAuthenticatedAsync(
                siteId, pageId, userSession.UserId, sortBy, cursorValue, pageSize);

            return Ok(result);
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
            _logger.LogError(ex, "Error listing comments for site {SiteId} page {PageId}", siteId, pageId);
            return StatusCode(500, ex.Message);
        }
    }
}
