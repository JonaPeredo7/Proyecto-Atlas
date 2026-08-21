namespace Gimnasio.Domain.Services;

public sealed record WorkContextEntry(DateOnly Date, string? Demands, int? BreakMinutes, string? UnusualConditions);
public sealed record WorkContextNote(DateOnly Date, string Text);
public sealed record WorkContextSummary(int ContextRecordedDays, int TotalBreakMinutes, int UnusualDays, IReadOnlyCollection<string> Demands, IReadOnlyCollection<WorkContextNote> UnusualConditions);

public static class WorkContextSummaryCalculator
{
    public static WorkContextSummary Calculate(IEnumerable<WorkContextEntry> entries)
    {
        var values = entries.ToArray();
        var contextDays = values.Count(x => !string.IsNullOrWhiteSpace(x.Demands) || x.BreakMinutes.HasValue || !string.IsNullOrWhiteSpace(x.UnusualConditions));
        var demands = values.Select(x => x.Demands?.Trim()).Where(x => !string.IsNullOrWhiteSpace(x)).Cast<string>().Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        var unusual = values.Where(x => !string.IsNullOrWhiteSpace(x.UnusualConditions)).Select(x => new WorkContextNote(x.Date, x.UnusualConditions!.Trim())).ToArray();
        return new(contextDays, values.Sum(x => x.BreakMinutes ?? 0), unusual.Select(x => x.Date).Distinct().Count(), demands, unusual);
    }
}
