namespace Gimnasio.Domain.Services;

public sealed record LearningPatternInput(string Category, string Outcome);
public sealed record LearningPatternResult(string Category, int Entries, string DominantOutcome, int OutcomeCount, decimal SharePercent, string State);

public static class LearningPatternCalculator
{
    public static IReadOnlyCollection<LearningPatternResult> Calculate(IEnumerable<LearningPatternInput> source)
    {
        return source
            .Where(x => !string.IsNullOrWhiteSpace(x.Category) && !string.IsNullOrWhiteSpace(x.Outcome))
            .GroupBy(x => x.Category.Trim(), StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() >= 3)
            .Select(group =>
            {
                var dominant = group.GroupBy(x => x.Outcome).OrderByDescending(x => x.Count()).ThenBy(x => x.Key).First();
                var entries = group.Count();
                var share = Math.Round(dominant.Count() * 100m / entries, 1);
                var state = dominant.Count() >= 3 && share >= 70m ? "repeated" : "mixed";
                return new LearningPatternResult(group.Key, entries, dominant.Key, dominant.Count(), share, state);
            })
            .OrderByDescending(x => x.Entries)
            .ThenBy(x => x.Category)
            .ToArray();
    }
}
