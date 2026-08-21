namespace Gimnasio.Application.Atlas;

public interface IAtlasService
{
    Task<AtlasOverviewDto> GetOverviewAsync(Guid userId, string displayName, CancellationToken cancellationToken = default);
    Task<AtlasProfileDto> UpdateProfileAsync(Guid userId, UpdateAtlasProfileRequest request, CancellationToken cancellationToken = default);
    Task<DailyCheckInDto> SaveCheckInAsync(Guid userId, SaveDailyCheckInRequest request, CancellationToken cancellationToken = default);
    Task<DailyActivityDto> SaveDailyActivityAsync(Guid userId, Guid? activityId, SaveDailyActivityRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteDailyActivityAsync(Guid userId, Guid activityId, CancellationToken cancellationToken = default);
    Task<DailyPlanDecisionDto> SaveDailyDecisionAsync(Guid userId,SaveDailyPlanDecisionRequest request,CancellationToken cancellationToken=default);
}

public sealed record AtlasOverviewDto(
    AtlasProfileDto Profile,
    IReadOnlyCollection<ProfileFactDto> Facts,
    DailyCheckInDto? Today,
    IReadOnlyCollection<DailyCheckInDto> RecentCheckIns,
    EvidenceSummaryDto Evidence,
    DailyHubDto Hub);

public sealed record DailyHubDto(
    IReadOnlyCollection<DailyActionDto> Actions,
    DailyStateDto State,
    DailyPlanContextDto PlanContext,
    DailyPlanDecisionDto? Decision,
    IReadOnlyCollection<TodayTrainingDto> TodaySessions,
    IReadOnlyCollection<TodayScheduleDto> TodaySchedule,
    int ScheduledMinutes,
    bool HasScheduleConflict,
    int PendingFollowUps,
    int OpenLearningActions,
    int DueLearningActions,
    DateOnly? OldestOpenLearningDate,
    int ActiveMetrics,
    int MetricsWithoutEntries,
    int? DaysToPrimaryTarget,
    IReadOnlyCollection<DailyActivityDto> TodayActivities,
    int TrainingLoadToday,
    int ExternalLoadToday,
    int TotalLoadToday);

public sealed record DailyActionDto(string Kind, string Title, string Detail, string Route, string State);
public sealed record DailyStateDto(string Status,string Label,string Summary,int BaselineDays,IReadOnlyCollection<DailyStateFactorDto>Factors,string Disclaimer);
public sealed record DailyStateFactorDto(string Key,string Label,decimal Current,decimal Baseline,decimal Delta,decimal VisualThreshold,string Unit,string Trend,string Basis);
public sealed record DailyPlanContextDto(string Status,string Label,string Summary,int SessionCount,int PlannedMinutes,int PlannedLoad,int IncompleteSessions,bool HasInProgress,string Disclaimer);
public sealed record DailyPlanDecisionDto(Guid Id,DateOnly Date,string Decision,string Reason,string ContextStatus,int PlannedLoadSnapshot,int Version,DateTimeOffset UpdatedAt);
public sealed record SaveDailyPlanDecisionRequest(string Decision,string Reason);
public sealed record TodayTrainingDto(Guid Id, string Name, string ActivityType, string Status, TimeOnly? PlannedStartTime, int? PlannedDurationMinutes, int? TargetRpe);
public sealed record TodayScheduleDto(Guid Id,string Name,string Category,string TimeWindow,TimeOnly?StartTime,TimeOnly?EndTime,string?Notes);
public sealed record DailyActivityDto(Guid Id, DateOnly Date, string ActivityType, int DurationMinutes, int Rpe, decimal? DistanceKm, string? Notes, int InternalLoad, int? PlannedDurationMinutes, string? PlannedSource, int? DurationVarianceMinutes, string? WorkDemands, int? BreakMinutes, string? UnusualConditions);

public sealed record AtlasProfileDto(
    Guid Id,
    string DisplayName,
    decimal? HeightCm,
    decimal? ReferenceWeightKg,
    string? PrimaryGoal,
    DateOnly? TargetDate,
    string? DominantHand,
    string? DominantLeg,
    string? AffectedKnee);

public sealed record ProfileFactDto(
    Guid Id,
    string Category,
    string Label,
    string? Value,
    string Status,
    string StatusLabel,
    string SourceTitle,
    string? Notes);

public sealed record DailyCheckInDto(
    Guid Id,
    DateOnly Date,
    int? SleepMinutes,
    int SleepQuality,
    int Energy,
    int Fatigue,
    int Stress,
    string? PainLocation,
    string? PainSide,
    int? PainIntensity,
    string? Stiffness,
    string? Swelling,
    bool Instability,
    bool Locking,
    int ExpectedWorkLoad,
    decimal? PlannedCyclingKm,
    string? PlannedActivity,
    string? Notes,
    bool NeedsAttention);

public sealed record EvidenceSummaryDto(int Draft, int InReview, int Informative, int Operational);

public sealed record UpdateAtlasProfileRequest(
    string DisplayName,
    decimal? HeightCm,
    decimal? ReferenceWeightKg,
    string? PrimaryGoal,
    DateOnly? TargetDate,
    string? DominantHand,
    string? DominantLeg,
    string? AffectedKnee);

public sealed record SaveDailyCheckInRequest(
    DateOnly Date,
    int? SleepMinutes,
    int SleepQuality,
    int Energy,
    int Fatigue,
    int Stress,
    string? PainLocation,
    string? PainSide,
    int? PainIntensity,
    string? Stiffness,
    string? Swelling,
    bool Instability,
    bool Locking,
    int ExpectedWorkLoad,
    decimal? PlannedCyclingKm,
    string? PlannedActivity,
    string? Notes);

public sealed record SaveDailyActivityRequest(DateOnly Date, string ActivityType, int DurationMinutes, int Rpe, decimal? DistanceKm, string? Notes, int? PlannedDurationMinutes, string? PlannedSource, string? WorkDemands, int? BreakMinutes, string? UnusualConditions);
