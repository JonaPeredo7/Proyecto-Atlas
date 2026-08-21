using System.Security.Claims;
using Gimnasio.Application.DataExport;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gimnasio.Api.Controllers;

[ApiController,Route("api/my-data"),Authorize,ResponseCache(NoStore=true,Location=ResponseCacheLocation.None)]
public sealed class DataExportController(IDataExportService service):ControllerBase
{
    [HttpGet("summary")]
    public async Task<ActionResult<PersonalDataSummaryDto>>Summary(CancellationToken ct)=>Ok(await service.GetSummaryAsync(UserId,ct));

    [HttpGet("operations")]
    public async Task<ActionResult<IReadOnlyCollection<DataTransferOperationDto>>>Operations(CancellationToken ct)=>Ok(await service.GetOperationsAsync(UserId,ct));

    [HttpGet("export")]
    public async Task<IActionResult>Export(CancellationToken ct)
    {
        var file=await service.ExportAsync(UserId,ct);
        Response.Headers.Append("X-Atlas-SHA256",file.Sha256);
        Response.Headers.Append("X-Atlas-Format-Version","1.0");
        return File(file.Content,"application/json",file.FileName);
    }

    [HttpPost("restore/preview"),RequestSizeLimit(20_000_000)]
    public async Task<ActionResult<RestorePreviewDto>>PreviewRestore(IFormFile file,CancellationToken ct)
    {
        if(file.Length==0)return BadRequest(new{message="El archivo está vacío."});
        if(file.Length>20_000_000)return BadRequest(new{message="El respaldo supera el límite de 20 MB."});
        await using var stream=new MemoryStream();await file.CopyToAsync(stream,ct);
        try{return Ok(await service.PreviewRestoreAsync(UserId,stream.ToArray(),ct));}
        catch(ArgumentException ex){return BadRequest(new{message=ex.Message});}
    }

    [HttpPost("restore/apply-core"),RequestSizeLimit(20_000_000)]
    public async Task<ActionResult<RestoreApplyDto>>ApplyCoreRestore(IFormFile file,[FromForm]string expectedSha256,[FromForm]string safetyBackupSha256,[FromForm]string confirmation,CancellationToken ct)
    {
        if(file.Length==0||file.Length>20_000_000)return BadRequest(new{message="El archivo no es válido para restaurar."});
        await using var stream=new MemoryStream();await file.CopyToAsync(stream,ct);
        try{return Ok(await service.RestoreMissingCoreAsync(UserId,stream.ToArray(),expectedSha256,safetyBackupSha256,confirmation,ct));}
        catch(ArgumentException ex){return BadRequest(new{message=ex.Message});}
        catch(Exception){return Conflict(new{message="La restauración se canceló por completo. No se aplicó ningún cambio."});}
    }

    private Guid UserId=>Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
