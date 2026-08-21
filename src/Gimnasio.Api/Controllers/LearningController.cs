using System.Security.Claims;using Gimnasio.Application.Learning;using Microsoft.AspNetCore.Authorization;using Microsoft.AspNetCore.Mvc;
namespace Gimnasio.Api.Controllers;
[ApiController,Route("api/learning"),Authorize]public sealed class LearningController(ILearningService service):ControllerBase
{
 [HttpGet]public async Task<ActionResult<LearningOverviewDto>>Get(CancellationToken ct)=>Ok(await service.GetAsync(UserId,ct));[HttpPost]public async Task<ActionResult<LearningEntryDto>>Create(SaveLearningEntryRequest r,CancellationToken ct)=>await Save(null,r,ct);[HttpPut("{id:guid}")]public async Task<ActionResult<LearningEntryDto>>Update(Guid id,SaveLearningEntryRequest r,CancellationToken ct)=>await Save(id,r,ct);[HttpDelete("{id:guid}")]public async Task<IActionResult>Delete(Guid id,CancellationToken ct)=>await service.DeleteAsync(UserId,id,ct)?NoContent():NotFound();
 private async Task<ActionResult<LearningEntryDto>>Save(Guid?id,SaveLearningEntryRequest r,CancellationToken ct){try{return Ok(await service.SaveAsync(UserId,id,r,ct));}catch(KeyNotFoundException){return NotFound();}catch(Exception e)when(e is ArgumentException or InvalidOperationException){return BadRequest(new{message=e.Message});}}private Guid UserId=>Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
