using Gimnasio.Application.Health;
using Gimnasio.Domain.Entities;
using Gimnasio.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Gimnasio.Infrastructure.Health;

internal sealed class HealthService(GimnasioDbContext dbContext) : IHealthService
{
    public async Task<KneeOverviewDto> GetKneeOverviewAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var profileId = await ProfileId(userId, cancellationToken);
        var checks = await dbContext.KneeChecks.AsNoTracking().Where(x => x.AthleteProfileId == profileId && x.IsActive).OrderByDescending(x => x.RecordedAt).Take(60).ToListAsync(cancellationToken);
        var dtos = checks.Select(Map).ToArray();
        var latest = dtos.FirstOrDefault(); var previous = dtos.Skip(1).FirstOrDefault();
        return new(dtos, new(dtos.Length, latest?.PainNow, previous?.PainNow, latest is null ? null : Function(latest), latest?.State ?? "no-data", latest?.Reasons ?? []));
    }

    public async Task<KneeCheckDto> SaveKneeCheckAsync(Guid userId, Guid? checkId, SaveKneeCheckRequest request, CancellationToken cancellationToken = default)
    {
        var profileId = await ProfileId(userId, cancellationToken);
        KneeCheck check;
        if (checkId.HasValue) check = await dbContext.KneeChecks.SingleOrDefaultAsync(x => x.Id == checkId && x.AthleteProfileId == profileId && x.IsActive, cancellationToken) ?? throw new KeyNotFoundException();
        else { check = new KneeCheck(profileId); dbContext.KneeChecks.Add(check); }
        check.Record(request.RecordedAt, request.Context, request.Side, request.PainNow, request.PainBest24H, request.PainWorst24H, request.Swelling, request.Instability, request.Locking, request.FullExtension, request.WalkingCapacity, request.StairsCapacity, request.SquatCapacity, request.Notes);
        await dbContext.SaveChangesAsync(cancellationToken); return Map(check);
    }

    public async Task<bool> DeleteKneeCheckAsync(Guid userId, Guid checkId, CancellationToken cancellationToken = default)
    {
        var profileId = await ProfileId(userId, cancellationToken);
        var check = await dbContext.KneeChecks.SingleOrDefaultAsync(x => x.Id == checkId && x.AthleteProfileId == profileId && x.IsActive, cancellationToken);
        if (check is null) return false; check.Remove(); await dbContext.SaveChangesAsync(cancellationToken); return true;
    }

    private async Task<Guid> ProfileId(Guid userId, CancellationToken ct) => (await dbContext.AthleteProfiles.AsNoTracking().SingleOrDefaultAsync(x => x.UserId == userId, ct))?.Id ?? throw new InvalidOperationException("Primero debe inicializarse el perfil Atlas.");
    private static int Function(KneeCheckDto x) => (x.WalkingCapacity + x.StairsCapacity + x.SquatCapacity) / 3;
    private static KneeCheckDto Map(KneeCheck x)
    {
        var reasons = new List<string>();
        if (x.Locking) reasons.Add("Se registró bloqueo de rodilla.");
        if (x.Instability) reasons.Add("Se registró sensación de inestabilidad o falseo.");
        if (!x.FullExtension) reasons.Add("No se alcanza la extensión completa referida.");
        if (x.Swelling is "moderada" or "alta") reasons.Add($"Inflamación {x.Swelling} referida.");
        if (x.PainWorst24H >= 7) reasons.Add("Dolor máximo alto en las últimas 24 horas.");
        var function = (x.WalkingCapacity + x.StairsCapacity + x.SquatCapacity) / 3;
        if (function <= 4) reasons.Add("Capacidad funcional autorreportada baja.");
        var state = x.Locking || x.Instability || !x.FullExtension || x.Swelling is "alta" || x.PainWorst24H >= 7 ? "attention" : reasons.Count > 0 || x.PainWorst24H >= 5 || x.Swelling == "moderada" ? "observe" : "stable";
        return new(x.Id, x.RecordedAt, x.Context, x.Side, x.PainNow, x.PainBest24H, x.PainWorst24H, x.Swelling, x.Instability, x.Locking, x.FullExtension, x.WalkingCapacity, x.StairsCapacity, x.SquatCapacity, x.Notes, state, reasons);
    }
}
