using System.Security.Claims;
using Gimnasio.Application.Measurements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gimnasio.Api.Controllers;

[ApiController]
[Route("api/measurements")]
[Authorize]
public sealed class MeasurementsController(IMeasurementService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<MeasurementOverviewDto>> Get(CancellationToken cancellationToken) => Ok(await service.GetAsync(UserId, cancellationToken));

    [HttpPost("metrics")]
    public async Task<ActionResult<MetricDto>> Create(SaveMetricRequest request, CancellationToken cancellationToken)
    { try { return Ok(await service.CreateMetricAsync(UserId, request, cancellationToken)); } catch (Exception ex) when (ex is InvalidOperationException or ArgumentException) { return Conflict(new { message = ex.Message }); } }

    [HttpPut("metrics/{metricId:guid}/entries")]
    public async Task<ActionResult<MetricDto>> AddEntry(Guid metricId, SaveMeasurementEntryRequest request, CancellationToken cancellationToken)
    { try { var result = await service.AddEntryAsync(UserId, metricId, request, cancellationToken); return result is null ? NotFound() : Ok(result); } catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); } }

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
