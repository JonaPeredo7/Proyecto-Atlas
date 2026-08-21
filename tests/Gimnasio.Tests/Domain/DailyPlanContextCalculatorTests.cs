using Gimnasio.Domain.Enums;
using Gimnasio.Domain.Services;

namespace Gimnasio.Tests.Domain;

public sealed class DailyPlanContextCalculatorTests
{
    [Fact]
    public void CompletePlanExposesItsEstimatedLoad()
    {
        var result=DailyPlanContextCalculator.Calculate("stable",[new(60,7,TrainingSessionStatus.Planned)]);
        Assert.Equal("planned",result.Status);Assert.Equal(60,result.PlannedMinutes);Assert.Equal(420,result.PlannedLoad);Assert.Equal(0,result.IncompleteSessions);
    }

    [Fact]
    public void AttentionNeverBecomesAutomaticClearance()
    {
        var result=DailyPlanContextCalculator.Calculate("attention",[new(45,6,TrainingSessionStatus.Planned)]);
        Assert.Equal("attention",result.Status);Assert.Contains("no modifica ni autoriza",result.Summary,StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void MissingPrescriptionDataIsExplicit()
    {
        var result=DailyPlanContextCalculator.Calculate("stable",[new(null,6,TrainingSessionStatus.Planned)]);
        Assert.Equal("incomplete",result.Status);Assert.Equal(1,result.IncompleteSessions);Assert.Equal(0,result.PlannedLoad);
    }
}
