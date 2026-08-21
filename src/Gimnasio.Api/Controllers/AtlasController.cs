using System.Security.Claims;
using Gimnasio.Application.Atlas;
using Gimnasio.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

namespace Gimnasio.Api.Controllers;

[ApiController]
[Route("api/atlas")]
[Authorize]
public sealed class AtlasController(
    IAtlasService atlasService,
    UserManager<ApplicationUser> userManager) : ControllerBase
{
    [HttpGet("overview")]
    public async Task<ActionResult<AtlasOverviewDto>> GetOverview(CancellationToken cancellationToken)
    {
        var currentUser = await userManager.GetUserAsync(User);
        var displayName = currentUser is null
            ? User.Identity?.Name ?? "Atleta"
            : $"{currentUser.FirstName} {currentUser.LastName}".Trim();

        return Ok(await atlasService.GetOverviewAsync(UserId, displayName, cancellationToken));
    }

    [HttpPut("profile")]
    public async Task<ActionResult<AtlasProfileDto>> UpdateProfile(
        UpdateAtlasProfileRequest request,
        CancellationToken cancellationToken) =>
        Ok(await atlasService.UpdateProfileAsync(UserId, request, cancellationToken));

    [HttpPut("check-ins/today")]
    public async Task<ActionResult<DailyCheckInDto>> SaveCheckIn(
        SaveDailyCheckInRequest request,
        CancellationToken cancellationToken) =>
        Ok(await atlasService.SaveCheckInAsync(UserId, request, cancellationToken));

    [HttpPut("decisions/today")]
    public async Task<ActionResult<DailyPlanDecisionDto>> SaveDailyDecision(SaveDailyPlanDecisionRequest request,CancellationToken cancellationToken)
    {
        try{return Ok(await atlasService.SaveDailyDecisionAsync(UserId,request,cancellationToken));}
        catch(ArgumentException ex){return BadRequest(new{message=ex.Message});}
    }

    [HttpPost("daily-activities")]
    public async Task<ActionResult<DailyActivityDto>> CreateDailyActivity(SaveDailyActivityRequest request, CancellationToken cancellationToken)
    {
        try { return Ok(await atlasService.SaveDailyActivityAsync(UserId, null, request, cancellationToken)); }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException) { return Conflict(new { message = ex.Message }); }
    }

    [HttpPut("daily-activities/{activityId:guid}")]
    public async Task<ActionResult<DailyActivityDto>> UpdateDailyActivity(Guid activityId, SaveDailyActivityRequest request, CancellationToken cancellationToken)
    {
        try { return Ok(await atlasService.SaveDailyActivityAsync(UserId, activityId, request, cancellationToken)); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException) { return Conflict(new { message = ex.Message }); }
    }

    [HttpDelete("daily-activities/{activityId:guid}")]
    public async Task<IActionResult> DeleteDailyActivity(Guid activityId, CancellationToken cancellationToken) =>
        await atlasService.DeleteDailyActivityAsync(UserId, activityId, cancellationToken) ? NoContent() : NotFound();

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
