using Gimnasio.Domain.Common;

namespace Gimnasio.Domain.Entities;

public sealed class MeasurementEntry : Entity
{
    private MeasurementEntry() { }

    public MeasurementEntry(Guid metricDefinitionId, DateOnly date, decimal value)
    {
        MetricDefinitionId = metricDefinitionId;
        Date = date;
        SetValue(value);
    }

    public Guid MetricDefinitionId { get; private set; }
    public DateOnly Date { get; private set; }
    public decimal Value { get; private set; }
    public string? Conditions { get; private set; }
    public string? Notes { get; private set; }

    public void Record(decimal value, string? conditions, string? notes)
    {
        SetValue(value);
        Conditions = Clean(conditions);
        Notes = Clean(notes);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private void SetValue(decimal value) => Value = value is >= 0 and <= 1000000 ? value : throw new ArgumentOutOfRangeException(nameof(value));
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
