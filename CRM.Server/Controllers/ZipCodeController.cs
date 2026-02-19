using CRM.Application.Queries.ZipCode;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ZipCodeController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ZipCodeController> _logger;

    public ZipCodeController(IMediator mediator, ILogger<ZipCodeController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet("{zipCode}")]
    public async Task<IActionResult> GetAddressByZipCode(string zipCode)
    {
        _logger.LogInformation("Fetching address for zip code: {ZipCode}", zipCode);

        var query = new GetZipCodeInfoQuery(zipCode);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Zip code not found: {ZipCode}", zipCode);
            return NotFound(new { error = result.Error });
        }

        return Ok(result.Data);
    }
}
