using Gimnasio.Application.Measurements;
using Gimnasio.Domain.Entities;
using Gimnasio.Domain.Enums;
using Gimnasio.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Gimnasio.Infrastructure.Measurements;

internal sealed class MeasurementService(GimnasioDbContext dbContext) : IMeasurementService
{
    public async Task<MeasurementOverviewDto> GetAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var profile = await Profile(userId, cancellationToken);
        var metrics = await dbContext.MetricDefinitions.AsNoTracking()
            .Where(x => x.AthleteProfileId == profile.Id && x.IsActive)
            .OrderBy(x => x.Category).ThenBy(x => x.Name).ToListAsync(cancellationToken);
        var ids = metrics.Select(x => x.Id).ToArray();
        var entries = await dbContext.MeasurementEntries.AsNoTracking()
            .Where(x => ids.Contains(x.MetricDefinitionId) && x.IsActive)
            .OrderBy(x => x.Date).ToListAsync(cancellationToken);
        var result = metrics.Select(x => Map(x, entries.Where(e => e.MetricDefinitionId == x.Id))).ToArray();
        return new(result, entries.Count, metrics.Count(x => x.TargetValue.HasValue));
    }

    public async Task<MetricDto> CreateMetricAsync(Guid userId, SaveMetricRequest request, CancellationToken cancellationToken = default)
    {
        var profile = await Profile(userId, cancellationToken);
        var name = request.Name.Trim();
        if (await dbContext.MetricDefinitions.AnyAsync(x => x.AthleteProfileId == profile.Id && x.Name == name && x.IsActive, cancellationToken))
            throw new InvalidOperationException("Ya existe un indicador con ese nombre.");
        if (!Enum.TryParse<MetricDirection>(request.Direction, true, out var direction))
            throw new ArgumentException("La dirección del indicador no es válida.", nameof(request.Direction));
        if (!string.IsNullOrWhiteSpace(request.SourceUrl) && !Uri.TryCreate(request.SourceUrl, UriKind.Absolute, out _))
            throw new ArgumentException("El enlace de la fuente no es válido.", nameof(request.SourceUrl));
        var metric = new MetricDefinition(profile.Id, name, request.Category, request.Unit);
        metric.Configure(direction, request.TargetValue, request.TargetDate, request.Protocol, request.SourceTitle, request.SourceUrl);
        dbContext.MetricDefinitions.Add(metric);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(metric, []);
    }

    public async Task<MetricDto?> AddEntryAsync(Guid userId, Guid metricId, SaveMeasurementEntryRequest request, CancellationToken cancellationToken = default)
    {
        var metric = await dbContext.MetricDefinitions.SingleOrDefaultAsync(x => x.Id == metricId && x.IsActive && dbContext.AthleteProfiles.Any(p => p.Id == x.AthleteProfileId && p.UserId == userId), cancellationToken);
        if (metric is null) return null;
        var entry = await dbContext.MeasurementEntries.SingleOrDefaultAsync(x => x.MetricDefinitionId == metricId && x.Date == request.Date, cancellationToken);
        if (entry is null) { entry = new MeasurementEntry(metricId, request.Date, request.Value); dbContext.MeasurementEntries.Add(entry); }
        entry.Record(request.Value, request.Conditions, request.Notes);
        await dbContext.SaveChangesAsync(cancellationToken);
        var entries = await dbContext.MeasurementEntries.AsNoTracking().Where(x => x.MetricDefinitionId == metricId && x.IsActive).OrderBy(x => x.Date).ToListAsync(cancellationToken);
        return Map(metric, entries);
    }

    private async Task<AthleteProfile> Profile(Guid userId, CancellationToken cancellationToken) =>
        await dbContext.AthleteProfiles.SingleOrDefaultAsync(x => x.UserId == userId, cancellationToken)
        ?? throw new InvalidOperationException("Primero debe inicializarse el perfil Atlas.");

    private static MetricDto Map(MetricDefinition metric, IEnumerable<MeasurementEntry> source)
    {
        var entries = source.OrderBy(x => x.Date).ToArray();
        var latest = entries.LastOrDefault();
        decimal? best = entries.Length == 0 ? null : metric.Direction switch { MetricDirection.HigherIsBetter => entries.Max(x => x.Value), MetricDirection.LowerIsBetter => entries.Min(x => x.Value), _ => latest!.Value };
        decimal? change = entries.Length < 2 ? null : latest!.Value - entries[0].Value;
        decimal? remaining = latest is null || metric.TargetValue is null ? null : metric.Direction switch
        {
            MetricDirection.HigherIsBetter => Math.Max(0, metric.TargetValue.Value - latest.Value),
            MetricDirection.LowerIsBetter => Math.Max(0, latest.Value - metric.TargetValue.Value),
            _ => Math.Abs(metric.TargetValue.Value - latest.Value)
        };
        return new(metric.Id, metric.Name, metric.Category, metric.Unit, metric.Direction.ToString(), metric.TargetValue, metric.TargetDate, metric.Protocol, metric.SourceTitle, metric.SourceUrl,
            latest?.Value, latest?.Date, best, change, remaining, entries.OrderByDescending(x => x.Date).Select(x => new MeasurementEntryDto(x.Id, x.Date, x.Value, x.Conditions, x.Notes)).ToArray());
    }
}
