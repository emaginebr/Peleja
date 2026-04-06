namespace Peleja.API.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Peleja.Domain.Services;

[ApiController]
[Route("api/v1/giphy")]
public class GiphyController : ControllerBase
{
    private readonly GiphyService _giphyService;

    public GiphyController(GiphyService giphyService)
    {
        _giphyService = giphyService;
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
                return BadRequest("O parâmetro q é obrigatório");

            var result = await _giphyService.SearchAsync(q, limit, offset);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (HttpRequestException)
        {
            return Problem(
                detail: "Serviço de GIFs temporariamente indisponível. Tente novamente em alguns instantes.",
                statusCode: 503);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
