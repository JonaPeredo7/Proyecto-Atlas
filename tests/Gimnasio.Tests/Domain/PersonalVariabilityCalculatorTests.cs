using Gimnasio.Domain.Services;

namespace Gimnasio.Tests.Domain;

public sealed class PersonalVariabilityCalculatorTests
{
    [Fact]
    public void CalculatesMedianCentralRangeAndCoverageWithoutClassifyingHealth()
    {
        var result=PersonalVariabilityCalculator.Calculate(new[]
        {
            new PersonalVariabilityInput(6,1,1,1,1,1),new PersonalVariabilityInput(7,2,2,2,2,2),
            new PersonalVariabilityInput(8,3,3,3,3,3),new PersonalVariabilityInput(9,4,4,4,4,4)
        });
        var sleep=result.Factors.Single(x=>x.Key=="sleep-duration");
        Assert.Equal(4,result.CheckInDays);Assert.Equal(14.3m,result.CoveragePercent);
        Assert.Equal(7.5m,sleep.Median);Assert.Equal(6.8m,sleep.LowerQuartile);Assert.Equal(8.2m,sleep.UpperQuartile);
        Assert.Contains("no definen una zona ideal",result.Disclaimer,StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void MissingOptionalValuesDoNotBecomeZeroes()
    {
        var result=PersonalVariabilityCalculator.Calculate(new[]
        {
            new PersonalVariabilityInput(null,4,4,3,2,null),new PersonalVariabilityInput(7,4,4,3,2,0)
        });
        Assert.Equal(1,result.Factors.Single(x=>x.Key=="sleep-duration").Entries);
        var pain=result.Factors.Single(x=>x.Key=="pain");Assert.Equal(1,pain.Entries);Assert.Equal(0,pain.Median);
    }
}
