using System.Security.Claims;using Gimnasio.Application.Planning;using Microsoft.AspNetCore.Authorization;using Microsoft.AspNetCore.Mvc;
namespace Gimnasio.Api.Controllers;
[ApiController,Route("api/planning"),Authorize]public sealed class PlanningController(IPlanningService service):ControllerBase
{
 [HttpGet]public async Task<ActionResult<PlanningOverviewDto>>Get(CancellationToken ct)=>Ok(await service.GetAsync(UserId,ct));
 [HttpPost("goals")]public async Task<ActionResult<GoalDto>>CreateGoal(SaveGoalRequest r,CancellationToken ct)=>await Goal(null,r,ct);[HttpPut("goals/{id:guid}")]public async Task<ActionResult<GoalDto>>UpdateGoal(Guid id,SaveGoalRequest r,CancellationToken ct)=>await Goal(id,r,ct);
 [HttpPost("cycles")]public async Task<ActionResult<CycleDto>>CreateCycle(SaveCycleRequest r,CancellationToken ct)=>await Cycle(null,r,ct);[HttpPut("cycles/{id:guid}")]public async Task<ActionResult<CycleDto>>UpdateCycle(Guid id,SaveCycleRequest r,CancellationToken ct)=>await Cycle(id,r,ct);
 private async Task<ActionResult<GoalDto>>Goal(Guid?id,SaveGoalRequest r,CancellationToken ct){try{return Ok(await service.SaveGoalAsync(UserId,id,r,ct));}catch(KeyNotFoundException){return NotFound();}catch(Exception e)when(e is ArgumentException or InvalidOperationException){return BadRequest(new{message=e.Message});}}
 private async Task<ActionResult<CycleDto>>Cycle(Guid?id,SaveCycleRequest r,CancellationToken ct){try{return Ok(await service.SaveCycleAsync(UserId,id,r,ct));}catch(KeyNotFoundException){return NotFound();}catch(Exception e)when(e is ArgumentException or InvalidOperationException){return BadRequest(new{message=e.Message});}}
 private Guid UserId=>Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
