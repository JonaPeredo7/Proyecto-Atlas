using Gimnasio.Domain.Services;

namespace Gimnasio.Tests.Domain;

public sealed class WorkContextSummaryTests
{
    [Fact]
    public void SummarizesContextWithoutInterpretingIt()
    {
        var result = WorkContextSummaryCalculator.Calculate([
            new(new(2026, 8, 10), "Escaleras y cargas", 30, null),
            new(new(2026, 8, 11), "Escaleras y cargas", 20, "Se agregó otro edificio"),
            new(new(2026, 8, 12), null, null, null)
        ]);

        Assert.Equal(2, result.ContextRecordedDays);
        Assert.Equal(50, result.TotalBreakMinutes);
        Assert.Equal(1, result.UnusualDays);
        Assert.Single(result.Demands);
        Assert.Single(result.UnusualConditions);
    }

    [Fact]
    public void EmptyContextReturnsZeroCoverage()
    {
        var result = WorkContextSummaryCalculator.Calculate([]);

        Assert.Equal(0, result.ContextRecordedDays);
        Assert.Empty(result.Demands);
    }
}
