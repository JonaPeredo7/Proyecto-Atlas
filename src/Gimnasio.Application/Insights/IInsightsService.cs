namespace Gimnasio.Application.Insights;

public interface IInsightsService
{
    Task<InsightsOverviewDto> GetAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<WeeklyReportDto> GetWeeklyAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<LongTermReviewDto> GetLongTermAsync(Guid userId,int weeks,CancellationToken cancellationToken = default);
}

public sealed record LongTermReviewDto(int Weeks,DateOnly From,DateOnly To,IReadOnlyCollection<LongTermWeekDto>Timeline,LongTermSummaryDto Summary,LongTermWorkDto Work,IReadOnlyCollection<MetricEvolutionDto>Metrics,IReadOnlyCollection<InsightSignalDto>Signals,int DataCoverageDays);
public sealed record LongTermWeekDto(DateOnly From,DateOnly To,int TrainingLoad,int ExternalLoad,int TotalLoad,int Sessions,int TrainingMinutes,int ActivityMinutes,int CheckIns,decimal?AverageFatigue,decimal?AveragePain,int KneeAttentionChecks,int WorkRecordedDays,int WorkPlannedMinutes,int WorkActualMinutes,int WorkDifferenceMinutes,int WorkExtraMinutes,int WorkContextRecordedDays,int WorkBreakMinutes,int WorkUnusualDays);
public sealed record LongTermSummaryDto(int TotalLoad,decimal AverageWeeklyLoad,int Sessions,int TrainingMinutes,int ActivityMinutes,decimal?LoadChangePercent,decimal?FatigueChange,decimal?PainChange);
public sealed record LongTermWorkDto(int RecordedDays,int PlannedMinutes,int ActualMinutes,int DifferenceMinutes,int ExtraMinutes,int ShorterMinutes,int WeeksWithData,int ContextRecordedDays,int BreakMinutes,int UnusualDays);
public sealed record MetricEvolutionDto(Guid Id,string Name,string Unit,string Direction,decimal FirstValue,DateOnly FirstDate,decimal LatestValue,DateOnly LatestDate,decimal Change,int Entries);

public sealed record WeeklyReportDto(DateOnly From, DateOnly To, WeeklyTotalsDto Current, WeeklyTotalsDto Previous, WeeklyComparisonDto Comparison, WeeklyWorkDto Work, IReadOnlyCollection<WeeklyDayDto> Days, IReadOnlyCollection<InsightSignalDto> Signals, int DataCoverage, WeeklyCycleDto? ActiveCycle, WeeklyReviewDto Review);
public sealed record WeeklyWorkDto(WorkPeriodDto Current,WorkPeriodDto Previous,WorkPeriodComparisonDto Comparison,WorkContextDto Context);
public sealed record WorkPeriodDto(int RecordedDays,int PlannedMinutes,int ActualMinutes,int DifferenceMinutes,int ExtraMinutes,int ShorterMinutes,int DaysWithExtra);
public sealed record WorkPeriodComparisonDto(bool HasComparison,decimal? ActualMinutesPerDayChange,decimal? ExtraMinutesPerDayChange);
public sealed record WorkContextDto(int ContextRecordedDays,int TotalBreakMinutes,int UnusualDays,IReadOnlyCollection<string> Demands,IReadOnlyCollection<WorkContextNoteDto> UnusualConditions);
public sealed record WorkContextNoteDto(DateOnly Date,string Text);
public sealed record WeeklyCycleDto(Guid Id,string Name,string Focus,int ExpectedSessions,int CompletedSessions,decimal?AdherencePercent,DateOnly EndDate);
public sealed record WeeklyReviewDto(int Decisions, int OpenLearningActions, int AppliedLearningActions, int DueLearningActions, bool HasReflection, Guid? ReflectionId);
public sealed record WeeklyTotalsDto(int TrainingLoad, int ExternalLoad, int TotalLoad, int Sessions, int TrainingMinutes, int ActivityMinutes, int CheckIns, decimal? AverageFatigue, decimal? AveragePain, int KneeChecks);
public sealed record WeeklyComparisonDto(decimal? TotalLoadPercent, decimal? FatigueChange, decimal? PainChange);
public sealed record WeeklyDayDto(DateOnly Date, int TrainingLoad, int ExternalLoad, int TotalLoad, int Sessions, int Activities, bool HasCheckIn, int? Fatigue, int? Pain, string KneeState);

public sealed record InsightsOverviewDto(
    IReadOnlyCollection<DailyTrendDto> Days,
    LoadComparisonDto Load,
    WellbeingSummaryDto Wellbeing,
    PersonalVariabilityDto Variability,
    PersonalPeriodComparisonDto PeriodComparison,
    IReadOnlyCollection<InsightSignalDto> Signals,
    IReadOnlyCollection<DecisionOutcomeDto> Decisions,
    LearningTrendDto Learning,
    IReadOnlyCollection<InsightEvidenceDto> Evidence,
    int DataDays,
    bool HasEnoughData);

public sealed record DailyTrendDto(DateOnly Date, int InternalLoad, int? SleepQuality, int? Energy, int? Fatigue, int? Stress, int? Pain, int? Recovery);
public sealed record LoadComparisonDto(int Current7Days, int Previous7Days, decimal? ChangePercent, int SessionsCurrent7Days, int MinutesCurrent7Days);
public sealed record WellbeingSummaryDto(decimal? AverageSleepQuality, decimal? AverageEnergy, decimal? AverageFatigue, decimal? AverageStress, decimal? AveragePain, decimal? AverageRecovery);
public sealed record PersonalVariabilityDto(int WindowDays,int CheckInDays,decimal CoveragePercent,IReadOnlyCollection<PersonalVariabilityFactorDto>Factors,string Disclaimer);
public sealed record PersonalVariabilityFactorDto(string Key,string Label,string Unit,int Entries,decimal Median,decimal LowerQuartile,decimal UpperQuartile,decimal Minimum,decimal Maximum);
public sealed record PersonalPeriodComparisonDto(int RecentDays,int BaselineDays,int RecentCheckIns,int BaselineCheckIns,decimal RecentCoveragePercent,decimal BaselineCoveragePercent,int ComparableFactors,IReadOnlyCollection<PersonalPeriodComparisonFactorDto>Factors,string Disclaimer);
public sealed record PersonalPeriodComparisonFactorDto(string Key,string Label,string Unit,int RecentEntries,int BaselineEntries,decimal RecentMedian,decimal BaselineMedian,decimal BaselineLowerQuartile,decimal BaselineUpperQuartile,decimal Delta,string Position);
public sealed record InsightSignalDto(string Kind, string Title, string Detail, string Basis);
public sealed record InsightEvidenceDto(string Topic, string Title, string PermanentId, string SourceUrl, string Use, string Limitation);
public sealed record DecisionOutcomeDto(DateOnly Date,string Decision,string Reason,string ContextStatus,int PlannedLoad,int TrainingLoad,int ExternalLoad,int TotalLoad,int?Pain,int?Fatigue,bool HasPostSessionAttention,int Version);
public sealed record LearningTrendDto(int ReviewedActions,int Helpful,int Neutral,int NotHelpful,int Inconclusive,int OpenActions,int DueActions,IReadOnlyCollection<LearningCategoryDto>Categories,IReadOnlyCollection<LearningPatternDto>Patterns,string Disclaimer);
public sealed record LearningCategoryDto(string Category,int ReviewedActions);
public sealed record LearningPatternDto(string Category,int Entries,string DominantOutcome,int OutcomeCount,decimal SharePercent,string State);
