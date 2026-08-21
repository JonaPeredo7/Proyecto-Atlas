namespace Gimnasio.Application.Measurements;

public interface IMeasurementService
{
    Task<MeasurementOverviewDto> GetAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<MetricDto> CreateMetricAsync(Guid userId, SaveMetricRequest request, CancellationToken cancellationToken = default);
    Task<MetricDto?> AddEntryAsync(Guid userId, Guid metricId, SaveMeasurementEntryRequest request, CancellationToken cancellationToken = default);
}

public sealed record MeasurementOverviewDto(IReadOnlyCollection<MetricDto> Metrics, int TotalEntries, int MetricsWithTargets);
public sealed record MetricDto(Guid Id, string Name, string Category, string Unit, string Direction, decimal? TargetValue, DateOnly? TargetDate, string? Protocol, string? SourceTitle, string? SourceUrl, decimal? LatestValue, DateOnly? LatestDate, decimal? BestValue, decimal? ChangeFromFirst, decimal? RemainingToTarget, IReadOnlyCollection<MeasurementEntryDto> Entries);
public sealed record MeasurementEntryDto(Guid Id, DateOnly Date, decimal Value, string? Conditions, string? Notes);
public sealed record SaveMetricRequest(string Name, string Category, string Unit, string Direction, decimal? TargetValue, DateOnly? TargetDate, string? Protocol, string? SourceTitle, string? SourceUrl);
public sealed record SaveMeasurementEntryRequest(DateOnly Date, decimal Value, string? Conditions, string? Notes);
