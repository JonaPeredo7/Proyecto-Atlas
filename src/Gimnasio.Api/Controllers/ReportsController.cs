using System.Security.Claims;
using Gimnasio.Application.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gimnasio.Api.Controllers;

[ApiController, Route("api/reports"), Authorize, ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class ReportsController(IReportService service) : ControllerBase
{
    [HttpGet("professional")]
    public async Task<ActionResult<ProfessionalReportDto>> Professional([FromQuery] DateOnly from, [FromQuery] DateOnly to, CancellationToken ct)
    {
        try { return Ok(await service.GetProfessionalAsync(UserId(), from, to, ct)); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost("shares")]
    public async Task<ActionResult<CreatedReportShareDto>> CreateShare(CreateReportShareRequest request, CancellationToken ct)
    {
        try { return Ok(await service.CreateShareAsync(UserId(), request, ct)); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpGet("shares")]
    public async Task<ActionResult<IReadOnlyCollection<ReportShareDto>>> Shares(CancellationToken ct) =>
        Ok(await service.ListSharesAsync(UserId(), ct));

    [HttpDelete("shares/{id:guid}")]
    public async Task<IActionResult> Revoke(Guid id, CancellationToken ct)
    {
        try { await service.RevokeShareAsync(UserId(), id, ct); return NoContent(); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [AllowAnonymous, HttpGet("shared/{token}")]
    public async Task<ActionResult<SharedProfessionalReportDto>> Shared(string token, CancellationToken ct)
    {
        var shared = await service.GetSharedAsync(token, ct);
        return shared is null ? NotFound(new { message = "El enlace no existe, venció o fue revocado." }) : Ok(shared);
    }

    [AllowAnonymous, HttpPost("shared/{token}/feedback")]
    public async Task<ActionResult<CreatedReportFeedbackDto>> SubmitFeedback(string token, CreateReportFeedbackRequest request, CancellationToken ct)
    {
        try { return Ok(await service.SubmitFeedbackAsync(token, request, ct)); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    [HttpGet("feedback")]
    public async Task<ActionResult<IReadOnlyCollection<ReportFeedbackDto>>> Feedback(CancellationToken ct) =>
        Ok(await service.ListFeedbackAsync(UserId(), ct));

    [HttpPut("feedback/{id:guid}")]
    public async Task<IActionResult> ReviewFeedback(Guid id, ReviewReportFeedbackRequest request, CancellationToken ct)
    {
        try { await service.ReviewFeedbackAsync(UserId(), id, request, ct); return NoContent(); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    private Guid UserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
