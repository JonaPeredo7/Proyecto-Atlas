using Gimnasio.Domain.Services;

namespace Gimnasio.Tests.Domain;

public sealed class LearningPatternCalculatorTests
{
    [Fact]
    public void RequiresAtLeastThreeObservationsPerCategory()
    {
        var result = LearningPatternCalculator.Calculate([
            new("Entrenamiento", "helpful"), new("Entrenamiento", "helpful")]);
        Assert.Empty(result);
    }

    [Fact]
    public void MarksRepeatedResultOnlyWithThreeObservationsAndSeventyPercent()
    {
        var result = LearningPatternCalculator.Calculate([
            new("Entrenamiento", "helpful"), new("Entrenamiento", "helpful"), new("Entrenamiento", "helpful"), new("Entrenamiento", "neutral")]).Single();
        Assert.Equal("repeated", result.State);
        Assert.Equal(75m, result.SharePercent);
        Assert.Equal(3, result.OutcomeCount);
    }

    [Fact]
    public void KeepsSplitResultsAsMixed()
    {
        var result = LearningPatternCalculator.Calculate([
            new("Recuperación", "helpful"), new("Recuperación", "helpful"), new("Recuperación", "neutral")]).Single();
        Assert.Equal("mixed", result.State);
        Assert.Equal(66.7m, result.SharePercent);
    }
}
