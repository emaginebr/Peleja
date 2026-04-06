namespace Peleja.API.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Peleja.Domain.Services;

[ApiController]
[Route("api/v1/giphy")]
public class GiphyController : ControllerBase
{
    private readonly GiphyService _giphyService;
    private readonly ILogger<GiphyController> _logger;

    public GiphyController(GiphyService giphyService, ILogger<GiphyController> logger)
    {
        _giphyService = giphyService;
        _logger = logger;
    }

    [HttpGet("search")]
    [Authorize]
    public async Task<IActionResult> Search(
        [FromQuery] string? q,
        [FromQuery] int limit = 20,
        [FromQuery] int offset = 0)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest("The q parameter is required");

            var result = await _giphyService.SearchAsync(q, limit, offset);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Giphy API unavailable");
            return Problem(
                detail: "GIF service temporarily unavailable. Please try again later.",
                statusCode: 503);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching Giphy for query={Query}", q);
            return StatusCode(500, ex.Message);
        }
    }
}
