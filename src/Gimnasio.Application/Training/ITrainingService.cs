namespace Gimnasio.Application.Training;

public interface ITrainingService
{
    Task<TrainingOverviewDto> GetOverviewAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<TrainingSessionDto> CreateSessionAsync(Guid userId, SaveTrainingSessionRequest request, CancellationToken cancellationToken = default);
    Task<TrainingSessionDto?> UpdateSessionAsync(Guid userId, Guid sessionId, SaveTrainingSessionRequest request, CancellationToken cancellationToken = default);
    Task<TrainingSessionDto?> DuplicateSessionAsync(Guid userId, Guid sessionId, DuplicateTrainingSessionRequest request, CancellationToken cancellationToken = default);
    Task<CopyTrainingWeekResultDto> CopyWeekAsync(Guid userId, CopyTrainingWeekRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ScheduleBlockDto>> GetScheduleAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ScheduleBlockDto>> AddScheduleBlocksAsync(Guid userId, SaveScheduleBlockRequest request, CancellationToken cancellationToken = default);
    Task<bool> RemoveScheduleBlockAsync(Guid userId, Guid blockId, CancellationToken cancellationToken = default);
    Task<TrainingSessionDto?> AddExerciseAsync(Guid userId, Guid sessionId, AddTrainingExerciseRequest request, CancellationToken cancellationToken = default);
    Task<TrainingSessionDto?> UpdateExerciseAsync(Guid userId, Guid sessionId, Guid exerciseId, AddTrainingExerciseRequest request, CancellationToken cancellationToken = default);
    Task<TrainingSessionDto?> StartSessionAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken = default);
    Task<TrainingSessionDto?> RecordExerciseAsync(Guid userId, Guid sessionId, Guid exerciseId, RecordTrainingExerciseRequest request, CancellationToken cancellationToken = default);
    Task<TrainingSessionDto?> CompleteSessionAsync(Guid userId, Guid sessionId, CompleteTrainingSessionRequest request, CancellationToken cancellationToken = default);
    Task<TrainingFollowUpDto?> SaveFollowUpAsync(Guid userId, Guid sessionId, SaveTrainingFollowUpRequest request, CancellationToken cancellationToken = default);
    Task<TrainingCalendarDto> GetCalendarAsync(Guid userId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
}

public sealed record TrainingCalendarDto(DateOnly From, DateOnly To, IReadOnlyCollection<TrainingCalendarDayDto> Days, CalendarSummaryDto Summary);
public sealed record TrainingCalendarDayDto(DateOnly Date, bool HasCheckIn, int? Energy, int? Fatigue, int? Pain, int PlannedMinutes, int ActualMinutes, int InternalLoad, int ExternalLoad, int TotalLoad, int DailyActivities, IReadOnlyCollection<CalendarSessionDto> Sessions, IReadOnlyCollection<CalendarLearningReviewDto> LearningReviews, IReadOnlyCollection<ScheduleBlockDto> ScheduleBlocks, bool HasScheduleConflict);
public sealed record CalendarSessionDto(Guid Id, string Name, string ActivityType, string Status, TimeOnly? PlannedStartTime, TimeOnly? PlannedEndTime, int? PlannedDurationMinutes, int? TargetRpe, int? ActualDurationMinutes, int? SessionRpe, bool FollowUpPending);
public sealed record CalendarLearningReviewDto(Guid Id, string Title, string NextAction, string Status, DateOnly ReviewDueOn, DateOnly? ReviewedOn, bool IsDue);
public sealed record CalendarSummaryDto(int PlannedSessions, int CompletedSessions, int PlannedMinutes, int ActualMinutes, int InternalLoad, int ExternalLoad, int TotalLoad, int CheckInDays, int ScheduledLearningReviews, int DueLearningReviews);

public sealed record TrainingOverviewDto(
    IReadOnlyCollection<TrainingSessionDto> Upcoming,
    IReadOnlyCollection<TrainingSessionDto> Recent,
    IReadOnlyCollection<TrainingSessionDto> PendingFollowUps,
    TrainingMetricsDto Metrics,
    IReadOnlyCollection<TrainingPlanOptionDto> Goals,
    IReadOnlyCollection<TrainingPlanOptionDto> Cycles);

public sealed record TrainingPlanOptionDto(Guid Id, string Name, string Detail);

public sealed record TrainingMetricsDto(int SessionsLast7Days, int MinutesLast7Days, int LoadLast7Days, int PendingFollowUps);

public sealed record TrainingSessionDto(
    Guid Id,
    DateOnly Date,
    string Name,
    string ActivityType,
    TimeOnly? PlannedStartTime,
    int? PlannedDurationMinutes,
    int? TargetRpe,
    string? Goal,
    string? Notes,
    Guid? PersonalGoalId,
    string? PersonalGoalTitle,
    Guid? TrainingCycleId,
    string? TrainingCycleName,
    int Version,
    string Status,
    int? ActualDurationMinutes,
    int? SessionRpe,
    int? InternalLoad,
    string? CompletionNotes,
    DateTimeOffset? CompletedAt,
    bool FollowUpDue,
    TrainingFollowUpDto? FollowUp,
    IReadOnlyCollection<TrainingExerciseDto> Exercises);

public sealed record TrainingExerciseDto(
    Guid Id,
    int Order,
    string Name,
    string? Category,
    int PlannedSets,
    string PlannedRepetitions,
    decimal? PlannedLoadKg,
    int? RestSeconds,
    string? Notes,
    int? CompletedSets,
    string? ActualRepetitions,
    decimal? ActualLoadKg,
    int? ExerciseRpe,
    bool IsCompleted);

public sealed record TrainingFollowUpDto(
    Guid Id,
    DateTimeOffset RecordedAt,
    int Recovery,
    int? PainIntensity,
    string? PainLocation,
    string? Stiffness,
    string? Swelling,
    bool Instability,
    bool Locking,
    string? Notes,
    bool NeedsAttention);

public sealed record SaveTrainingSessionRequest(
    DateOnly Date,
    string Name,
    string ActivityType,
    TimeOnly? PlannedStartTime,
    int? PlannedDurationMinutes,
    int? TargetRpe,
    string? Goal,
    string? Notes,
    Guid? PersonalGoalId,
    Guid? TrainingCycleId,
    string ChangeReason);

public sealed record DuplicateTrainingSessionRequest(DateOnly Date, string? Name, string ChangeReason);
public sealed record CopyTrainingWeekRequest(DateOnly SourceWeekStart, DateOnly TargetWeekStart, string ChangeReason);
public sealed record CopyTrainingWeekResultDto(int CopiedSessions, int SkippedSessions, IReadOnlyCollection<TrainingSessionDto> Sessions);
public sealed record SaveScheduleBlockRequest(string Name, string Category, IReadOnlyCollection<int> DaysOfWeek, string TimeWindow, TimeOnly? StartTime, TimeOnly? EndTime, DateOnly EffectiveFrom, DateOnly? EffectiveTo, string? Notes);
public sealed record ScheduleBlockDto(Guid Id, int DayOfWeek, string Name, string Category, string TimeWindow, TimeOnly? StartTime, TimeOnly? EndTime, DateOnly EffectiveFrom, DateOnly? EffectiveTo, string? Notes);

public sealed record AddTrainingExerciseRequest(
    string Name,
    string? Category,
    int Sets,
    string Repetitions,
    decimal? LoadKg,
    int? RestSeconds,
    string? Notes,
    string ChangeReason);

public sealed record RecordTrainingExerciseRequest(int CompletedSets, string? ActualRepetitions, decimal? ActualLoadKg, int? ExerciseRpe);
public sealed record CompleteTrainingSessionRequest(int ActualDurationMinutes, int SessionRpe, string? CompletionNotes);
public sealed record SaveTrainingFollowUpRequest(int Recovery, int? PainIntensity, string? PainLocation, string? Stiffness, string? Swelling, bool Instability, bool Locking, string? Notes);
