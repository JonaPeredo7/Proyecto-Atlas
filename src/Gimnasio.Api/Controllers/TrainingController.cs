using System.Security.Claims;
using Gimnasio.Application.Training;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gimnasio.Api.Controllers;

[ApiController]
[Route("api/training")]
[Authorize]
public sealed class TrainingController(ITrainingService trainingService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<TrainingOverviewDto>> Get(CancellationToken ct) => Ok(await trainingService.GetOverviewAsync(UserId, ct));

    [HttpGet("calendar")]
    public async Task<ActionResult<TrainingCalendarDto>> Calendar([FromQuery] DateOnly from, [FromQuery] DateOnly to, CancellationToken ct)
    {
        try { return Ok(await trainingService.GetCalendarAsync(UserId, from, to, ct)); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost("sessions")]
    public async Task<ActionResult<TrainingSessionDto>> Create(SaveTrainingSessionRequest request, CancellationToken ct) =>
        await Execute(() => trainingService.CreateSessionAsync(UserId, request, ct));

    [HttpPut("sessions/{sessionId:guid}")]
    public async Task<ActionResult<TrainingSessionDto>> Update(Guid sessionId, SaveTrainingSessionRequest request, CancellationToken ct) =>
        await ExecuteNullable(() => trainingService.UpdateSessionAsync(UserId, sessionId, request, ct));

    [HttpPost("sessions/{sessionId:guid}/duplicate")]
    public async Task<ActionResult<TrainingSessionDto>> Duplicate(Guid sessionId, DuplicateTrainingSessionRequest request, CancellationToken ct) =>
        await ExecuteNullable(() => trainingService.DuplicateSessionAsync(UserId, sessionId, request, ct));

    [HttpPost("weeks/copy")]
    public async Task<ActionResult<CopyTrainingWeekResultDto>> CopyWeek(CopyTrainingWeekRequest request, CancellationToken ct)
    {
        try { return Ok(await trainingService.CopyWeekAsync(UserId, request, ct)); }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException) { return Conflict(new { message = ex.Message }); }
    }

    [HttpGet("schedule")]
    public async Task<ActionResult<IReadOnlyCollection<ScheduleBlockDto>>> Schedule(CancellationToken ct) => Ok(await trainingService.GetScheduleAsync(UserId, ct));

    [HttpPost("schedule")]
    public async Task<ActionResult<IReadOnlyCollection<ScheduleBlockDto>>> AddSchedule(SaveScheduleBlockRequest request, CancellationToken ct)
    {
        try { return Ok(await trainingService.AddScheduleBlocksAsync(UserId, request, ct)); }
        catch (ArgumentException ex) { return Conflict(new { message = ex.Message }); }
    }

    [HttpDelete("schedule/{blockId:guid}")]
    public async Task<IActionResult> RemoveSchedule(Guid blockId, CancellationToken ct) => await trainingService.RemoveScheduleBlockAsync(UserId, blockId, ct) ? NoContent() : NotFound();

    [HttpPost("sessions/{sessionId:guid}/exercises")]
    public async Task<ActionResult<TrainingSessionDto>> AddExercise(Guid sessionId, AddTrainingExerciseRequest request, CancellationToken ct) =>
        await ExecuteNullable(() => trainingService.AddExerciseAsync(UserId, sessionId, request, ct));

    [HttpPut("sessions/{sessionId:guid}/exercises/{exerciseId:guid}")]
    public async Task<ActionResult<TrainingSessionDto>> UpdateExercise(Guid sessionId, Guid exerciseId, AddTrainingExerciseRequest request, CancellationToken ct) =>
        await ExecuteNullable(() => trainingService.UpdateExerciseAsync(UserId, sessionId, exerciseId, request, ct));

    [HttpPost("sessions/{sessionId:guid}/start")]
    public async Task<ActionResult<TrainingSessionDto>> Start(Guid sessionId, CancellationToken ct) =>
        await ExecuteNullable(() => trainingService.StartSessionAsync(UserId, sessionId, ct));

    [HttpPut("sessions/{sessionId:guid}/exercises/{exerciseId:guid}/result")]
    public async Task<ActionResult<TrainingSessionDto>> RecordExercise(Guid sessionId, Guid exerciseId, RecordTrainingExerciseRequest request, CancellationToken ct) =>
        await ExecuteNullable(() => trainingService.RecordExerciseAsync(UserId, sessionId, exerciseId, request, ct));

    [HttpPost("sessions/{sessionId:guid}/complete")]
    public async Task<ActionResult<TrainingSessionDto>> Complete(Guid sessionId, CompleteTrainingSessionRequest request, CancellationToken ct) =>
        await ExecuteNullable(() => trainingService.CompleteSessionAsync(UserId, sessionId, request, ct));

    [HttpPut("sessions/{sessionId:guid}/follow-up")]
    public async Task<ActionResult<TrainingFollowUpDto>> FollowUp(Guid sessionId, SaveTrainingFollowUpRequest request, CancellationToken ct)
    {
        try { var result = await trainingService.SaveFollowUpAsync(UserId, sessionId, request, ct); return result is null ? NotFound() : Ok(result); }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException) { return Conflict(new { message = ex.Message }); }
    }

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private async Task<ActionResult<TrainingSessionDto>> Execute(Func<Task<TrainingSessionDto>> action)
    { try { return Ok(await action()); } catch (Exception ex) when (ex is InvalidOperationException or ArgumentException) { return Conflict(new { message = ex.Message }); } }
    private async Task<ActionResult<TrainingSessionDto>> ExecuteNullable(Func<Task<TrainingSessionDto?>> action)
    { try { var result = await action(); return result is null ? NotFound() : Ok(result); } catch (Exception ex) when (ex is InvalidOperationException or ArgumentException) { return Conflict(new { message = ex.Message }); } }
}
