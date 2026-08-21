using Gimnasio.Domain.Services;

namespace Gimnasio.Tests.Domain;

public sealed class PersonalPeriodComparisonCalculatorTests
{
    [Fact]
    public void ComparesRecentMedianAgainstPreviousCentralRange()
    {
        var baseline=Enumerable.Range(1,7).Select(x=>new PersonalVariabilityInput(7,4,4,x,2,1));
        var recent=Enumerable.Range(0,3).Select(_=>new PersonalVariabilityInput(6,3,3,9,3,2));
        var result=PersonalPeriodComparisonCalculator.Calculate(recent,baseline);
        var fatigue=result.Factors.Single(x=>x.Key=="fatigue");
        Assert.Equal("above",fatigue.Position);Assert.Equal(5m,fatigue.Delta);Assert.Equal(6,result.ComparableFactors);
        Assert.Contains("no un umbral clínico",result.Disclaimer,StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DoesNotClassifyWhenEitherPeriodHasTooFewValues()
    {
        var baseline=Enumerable.Range(0,6).Select(_=>new PersonalVariabilityInput(7,4,4,3,2,null));
        var recent=Enumerable.Range(0,2).Select(_=>new PersonalVariabilityInput(6,3,3,5,3,null));
        var result=PersonalPeriodComparisonCalculator.Calculate(recent,baseline);
        Assert.All(result.Factors,x=>Assert.Equal("insufficient",x.Position));Assert.Equal(0,result.ComparableFactors);
    }
}
