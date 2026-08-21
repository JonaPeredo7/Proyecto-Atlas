namespace Gimnasio.Application.Health;

public interface IHealthService
{
    Task<KneeOverviewDto> GetKneeOverviewAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<KneeCheckDto> SaveKneeCheckAsync(Guid userId, Guid? checkId, SaveKneeCheckRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteKneeCheckAsync(Guid userId, Guid checkId, CancellationToken cancellationToken = default);
}

public sealed record KneeOverviewDto(IReadOnlyCollection<KneeCheckDto> Checks, KneeTrendDto Trend);
public sealed record KneeTrendDto(int TotalChecks, int? LatestPain, int? PreviousPain, int? LatestFunction, string State, IReadOnlyCollection<string> Reasons);
public sealed record KneeCheckDto(Guid Id, DateTimeOffset RecordedAt, string Context, string Side, int PainNow, int PainBest24H, int PainWorst24H, string Swelling, bool Instability, bool Locking, bool FullExtension, int WalkingCapacity, int StairsCapacity, int SquatCapacity, string? Notes, string State, IReadOnlyCollection<string> Reasons);
public sealed record SaveKneeCheckRequest(DateTimeOffset RecordedAt, string Context, string Side, int PainNow, int PainBest24H, int PainWorst24H, string Swelling, bool Instability, bool Locking, bool FullExtension, int WalkingCapacity, int StairsCapacity, int SquatCapacity, string? Notes);
