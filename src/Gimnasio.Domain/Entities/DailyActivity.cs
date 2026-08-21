using Gimnasio.Domain.Common;

namespace Gimnasio.Domain.Entities;

public sealed class DailyActivity : Entity
{
    private DailyActivity() { }

    public DailyActivity(Guid athleteProfileId, DateOnly date, string activityType)
    {
        AthleteProfileId = athleteProfileId;
        Date = date;
        ActivityType = Required(activityType, nameof(activityType));
    }

    public Guid AthleteProfileId { get; private set; }
    public DateOnly Date { get; private set; }
    public string ActivityType { get; private set; } = string.Empty;
    public int DurationMinutes { get; private set; }
    public int Rpe { get; private set; }
    public decimal? DistanceKm { get; private set; }
    public string? Notes { get; private set; }
    public int? PlannedDurationMinutes { get; private set; }
    public string? PlannedSource { get; private set; }
    public string? WorkDemands { get; private set; }
    public int? BreakMinutes { get; private set; }
    public string? UnusualConditions { get; private set; }
    public int InternalLoad => DurationMinutes * Rpe;

    public void Record(DateOnly date, string activityType, int durationMinutes, int rpe, decimal? distanceKm, string? notes)
    {
        Date = date;
        ActivityType = Required(activityType, nameof(activityType));
        DurationMinutes = Range(durationMinutes, 1, 960, nameof(durationMinutes));
        Rpe = Range(rpe, 1, 10, nameof(rpe));
        DistanceKm = distanceKm is null or >= 0 and <= 500 ? distanceKm : throw new ArgumentOutOfRangeException(nameof(distanceKm));
        Notes = Clean(notes);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void AttachPlanSnapshot(int? plannedDurationMinutes, string? plannedSource)
    {
        PlannedDurationMinutes = plannedDurationMinutes is null ? null : Range(plannedDurationMinutes.Value, 1, 960, nameof(plannedDurationMinutes));
        PlannedSource = PlannedDurationMinutes.HasValue ? Clean(plannedSource) : null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void AttachWorkContext(string? workDemands, int? breakMinutes, string? unusualConditions)
    {
        if (!ActivityType.Contains("trabajo", StringComparison.OrdinalIgnoreCase))
        {
            WorkDemands = null;
            BreakMinutes = null;
            UnusualConditions = null;
            return;
        }

        WorkDemands = Clean(workDemands, 300, nameof(workDemands));
        BreakMinutes = breakMinutes is null ? null : Range(breakMinutes.Value, 0, DurationMinutes, nameof(breakMinutes));
        UnusualConditions = Clean(unusualConditions, 400, nameof(unusualConditions));
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Remove()
    {
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static string Required(string value, string name) =>
        string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("El tipo de actividad es obligatorio.", name) : value.Trim();
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static string? Clean(string? value, int maxLength, string name)
    {
        var result = Clean(value);
        return result is null || result.Length <= maxLength ? result : throw new ArgumentException($"El texto admite hasta {maxLength} caracteres.", name);
    }
    private static int Range(int value, int min, int max, string name) =>
        value >= min && value <= max ? value : throw new ArgumentOutOfRangeException(name);
}
