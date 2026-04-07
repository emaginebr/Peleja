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
    private readonly IUserClient _userClient;
    private readonly ILogger<SiteController> _logger;

    public SiteController(SiteService siteService, IUserClient userClient, ILogger<SiteController> logger)
    {
        _siteService = siteService;
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
    public async Task<IActionResult> ListSites()
    {
        try
        {
            var userSession = _userClient.GetUserInSession(HttpContext);
            if (userSession == null)
                return Unauthorized();

            var result = await _siteService.ListByUserIdAsync(userSession.UserId);

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
}
