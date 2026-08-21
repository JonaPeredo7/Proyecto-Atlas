using Gimnasio.Domain.Common;
using Gimnasio.Domain.Enums;

namespace Gimnasio.Domain.Entities;

public sealed class MetricDefinition : Entity
{
    private MetricDefinition() { }

    public MetricDefinition(Guid athleteProfileId, string name, string category, string unit)
    {
        AthleteProfileId = athleteProfileId;
        Name = Required(name, nameof(name));
        Category = Required(category, nameof(category));
        Unit = Required(unit, nameof(unit));
    }

    public Guid AthleteProfileId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public string Unit { get; private set; } = string.Empty;
    public MetricDirection Direction { get; private set; }
    public decimal? TargetValue { get; private set; }
    public DateOnly? TargetDate { get; private set; }
    public string? Protocol { get; private set; }
    public string? SourceTitle { get; private set; }
    public string? SourceUrl { get; private set; }

    public void Configure(MetricDirection direction, decimal? targetValue, DateOnly? targetDate, string? protocol, string? sourceTitle, string? sourceUrl)
    {
        if (targetValue is < 0 or > 1000000) throw new ArgumentOutOfRangeException(nameof(targetValue));
        Name = Required(Name, nameof(Name));
        Direction = direction;
        TargetValue = targetValue;
        TargetDate = targetDate;
        Protocol = Clean(protocol);
        SourceTitle = Clean(sourceTitle);
        SourceUrl = Clean(sourceUrl);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static string Required(string value, string name) => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("El valor es obligatorio.", name) : value.Trim();
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
