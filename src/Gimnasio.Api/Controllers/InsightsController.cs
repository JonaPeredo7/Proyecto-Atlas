using System.Security.Claims;
using Gimnasio.Application.Insights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gimnasio.Api.Controllers;

[ApiController]
[Route("api/insights")]
[Authorize]
public sealed class InsightsController(IInsightsService insightsService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<InsightsOverviewDto>> Get(CancellationToken cancellationToken) =>
        Ok(await insightsService.GetAsync(Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!), cancellationToken));

    [HttpGet("weekly")]
    public async Task<ActionResult<WeeklyReportDto>> Weekly(CancellationToken cancellationToken) =>
        Ok(await insightsService.GetWeeklyAsync(Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!), cancellationToken));

    [HttpGet("long-term")]
    public async Task<ActionResult<LongTermReviewDto>> LongTerm([FromQuery]int weeks=8,CancellationToken cancellationToken=default)
    {try{return Ok(await insightsService.GetLongTermAsync(Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),weeks,cancellationToken));}catch(ArgumentException ex){return BadRequest(new{message=ex.Message});}}
}
