using Gimnasio.Domain.Services;

namespace Gimnasio.Tests.Domain;

public sealed class DailyStateCalculatorTests
{
    private static readonly DailyStateInput[] Baseline=
    [
        new(420,4,4,3,2,0,false),new(450,4,4,3,2,1,false),new(430,4,4,4,2,0,false),new(440,4,4,3,3,0,false)
    ];

    [Fact]
    public void SeveralUnfavorableChangesAreDescribedAsObserve()
    {
        var result=DailyStateCalculator.Calculate(new(330,2,2,7,4,3,false),Baseline);
        Assert.Equal("observe",result.Status);Assert.True(result.Factors.Count(x=>x.Trend=="worse")>=2);Assert.Equal(4,result.BaselineDays);
        Assert.All(result.Factors,x=>{Assert.True(x.VisualThreshold>0);Assert.Contains("no es un umbral clínico",x.Basis,StringComparison.OrdinalIgnoreCase);});
    }

    [Fact]
    public void SymptomFlagsTakePriorityWithoutGivingMedicalClearance()
    {
        var result=DailyStateCalculator.Calculate(new(420,4,4,3,2,6,true),Baseline);
        Assert.Equal("attention",result.Status);Assert.Contains("ni indica",result.Summary,StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AShortHistoryDoesNotPretendToBeABaseline()
    {
        var result=DailyStateCalculator.Calculate(new(420,4,4,3,2,0,false),Baseline.Take(2).ToArray());
        Assert.Equal("recorded",result.Status);Assert.Empty(result.Factors);
    }

    [Fact]
    public void VisualThresholdIsExposedAndBoundaryCountsAsAChange()
    {
        var result=DailyStateCalculator.Calculate(new(380,3,4,3,2,0,false),Baseline);
        var sleepQuality=result.Factors.Single(x=>x.Key=="sleep-quality");
        Assert.Equal(1,sleepQuality.VisualThreshold);Assert.Equal("worse",sleepQuality.Trend);
    }
}
