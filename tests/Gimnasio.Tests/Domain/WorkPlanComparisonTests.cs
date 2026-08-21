using Gimnasio.Domain.Services;

namespace Gimnasio.Tests.Domain;

public sealed class WorkPlanComparisonTests
{
    [Fact]
    public void CalculatesExtraAndShorterTimeSeparately()
    {
        var result = WorkPlanComparisonCalculator.Calculate([new(420,480),new(360,330),new(390,390)]);
        Assert.Equal(30,result.DifferenceMinutes);
        Assert.Equal(60,result.ExtraMinutes);
        Assert.Equal(30,result.ShorterMinutes);
        Assert.Equal(1,result.DaysWithExtra);
    }

    [Fact]
    public void EmptyPeriodReturnsZeroCoverage()
    {
        var result=WorkPlanComparisonCalculator.Calculate([]);
        Assert.Equal(0,result.RecordedDays);
        Assert.Equal(0,result.DifferenceMinutes);
    }

    [Fact]
    public void ComparesDailyAveragesWhenPeriodsHaveDifferentCoverage()
    {
        var current = WorkPlanComparisonCalculator.Calculate([new(420, 480), new(420, 450)]);
        var previous = WorkPlanComparisonCalculator.Calculate([new(420, 420)]);

        var trend = WorkPlanComparisonCalculator.Compare(current, previous);

        Assert.True(trend.HasComparison);
        Assert.Equal(45m, trend.ActualMinutesPerDayChange);
        Assert.Equal(45m, trend.ExtraMinutesPerDayChange);
    }

    [Fact]
    public void TrendIsUnavailableWithoutCoverageInBothPeriods()
    {
        var current = WorkPlanComparisonCalculator.Calculate([new(420, 450)]);
        var previous = WorkPlanComparisonCalculator.Calculate([]);

        var trend = WorkPlanComparisonCalculator.Compare(current, previous);

        Assert.False(trend.HasComparison);
        Assert.Null(trend.ActualMinutesPerDayChange);
    }
}
