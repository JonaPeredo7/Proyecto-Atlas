using System.Security.Claims;
using Gimnasio.Application.Health;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gimnasio.Api.Controllers;

[ApiController, Route("api/health"), Authorize]
public sealed class HealthController(IHealthService service) : ControllerBase
{
    [HttpGet("knee")] public async Task<ActionResult<KneeOverviewDto>> Get(CancellationToken ct) => Ok(await service.GetKneeOverviewAsync(UserId, ct));
    [HttpPost("knee/checks")] public async Task<ActionResult<KneeCheckDto>> Create(SaveKneeCheckRequest request, CancellationToken ct) => await Save(null, request, ct);
    [HttpPut("knee/checks/{id:guid}")] public async Task<ActionResult<KneeCheckDto>> Update(Guid id, SaveKneeCheckRequest request, CancellationToken ct) => await Save(id, request, ct);
    [HttpDelete("knee/checks/{id:guid}")] public async Task<IActionResult> Delete(Guid id, CancellationToken ct) => await service.DeleteKneeCheckAsync(UserId, id, ct) ? NoContent() : NotFound();
    private async Task<ActionResult<KneeCheckDto>> Save(Guid? id, SaveKneeCheckRequest request, CancellationToken ct) { try { return Ok(await service.SaveKneeCheckAsync(UserId, id, request, ct)); } catch (KeyNotFoundException) { return NotFound(); } catch (Exception ex) when (ex is ArgumentException or InvalidOperationException) { return BadRequest(new { message = ex.Message }); } }
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
