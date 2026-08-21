using Gimnasio.Domain.Entities;
using Gimnasio.Domain.Enums;

namespace Gimnasio.Tests.Domain;

public sealed class MeasurementTests
{
    [Fact]
    public void Metric_KeepsProtocolTargetAndDirection()
    {
        var metric = new MetricDefinition(Guid.NewGuid(), "Flexiones en 60 segundos", "Rendimiento", "repeticiones");
        metric.Configure(MetricDirection.HigherIsBetter, 40, new DateOnly(2027, 5, 15), "Máximo válido en 60 segundos", "Reglamento confirmado", "https://example.com/protocol");

        Assert.Equal(40, metric.TargetValue);
        Assert.Equal(MetricDirection.HigherIsBetter, metric.Direction);
        Assert.NotNull(metric.Protocol);
    }

    [Fact]
    public void Measurement_RecordsConditionsSeparately()
    {
        var entry = new MeasurementEntry(Guid.NewGuid(), new DateOnly(2026, 8, 13), 25);
        entry.Record(27, "Mismo horario y calentamiento", "Técnica válida");

        Assert.Equal(27, entry.Value);
        Assert.Equal("Mismo horario y calentamiento", entry.Conditions);
    }

    [Fact]
    public void Measurement_RejectsNegativeValue()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new MeasurementEntry(Guid.NewGuid(), new DateOnly(2026, 8, 13), -1));
    }
}
