namespace Gimnasio.Domain.Services;

public sealed record WorkPlanEntry(int PlannedMinutes, int ActualMinutes);
public sealed record WorkPlanComparison(int RecordedDays, int PlannedMinutes, int ActualMinutes, int DifferenceMinutes, int ExtraMinutes, int ShorterMinutes, int DaysWithExtra);
public sealed record WorkPlanTrend(bool HasComparison, decimal? ActualMinutesPerDayChange, decimal? ExtraMinutesPerDayChange);

public static class WorkPlanComparisonCalculator
{
    public static WorkPlanComparison Calculate(IEnumerable<WorkPlanEntry> entries)
    {
        var values = entries.ToArray();
        var planned = values.Sum(x => x.PlannedMinutes);
        var actual = values.Sum(x => x.ActualMinutes);
        return new(values.Length, planned, actual, actual - planned,
            values.Sum(x => Math.Max(0, x.ActualMinutes - x.PlannedMinutes)),
            values.Sum(x => Math.Max(0, x.PlannedMinutes - x.ActualMinutes)),
            values.Count(x => x.ActualMinutes > x.PlannedMinutes));
    }

    public static WorkPlanTrend Compare(WorkPlanComparison current, WorkPlanComparison previous)
    {
        if (current.RecordedDays == 0 || previous.RecordedDays == 0)
            return new(false, null, null);

        var actualChange = Math.Round(
            current.ActualMinutes / (decimal)current.RecordedDays - previous.ActualMinutes / (decimal)previous.RecordedDays, 1);
        var extraChange = Math.Round(
            current.ExtraMinutes / (decimal)current.RecordedDays - previous.ExtraMinutes / (decimal)previous.RecordedDays, 1);
        return new(true, actualChange, extraChange);
    }
}
