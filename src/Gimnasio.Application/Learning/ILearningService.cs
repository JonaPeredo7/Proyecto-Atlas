namespace Gimnasio.Application.Learning;

public interface ILearningService
{
    Task<LearningOverviewDto> GetAsync(Guid userId, CancellationToken ct = default);
    Task<LearningEntryDto> SaveAsync(Guid userId, Guid? id, SaveLearningEntryRequest request, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid userId, Guid id, CancellationToken ct = default);
}

public sealed record LearningOverviewDto(IReadOnlyCollection<LearningEntryDto> Entries, IReadOnlyCollection<LearningOptionDto> Sessions,
    IReadOnlyCollection<LearningOptionDto> Goals, IReadOnlyCollection<LearningOptionDto> Cycles, int OpenActions,
    int ReviewedActions, int EntriesLast30Days);
public sealed record LearningOptionDto(Guid Id, string Name, string Detail);
public sealed record LearningEntryDto(Guid Id, DateOnly Date, string Title, string Category, string Observation,
    string? Interpretation, string? NextAction, DateOnly? ReviewDueOn, string Confidence, string Status, DateOnly? ReviewedOn,
    string? FollowUpOutcome, string? FollowUpObservation, Guid? TrainingSessionId, string? TrainingSessionName,
    Guid? PersonalGoalId, string? PersonalGoalName, Guid? TrainingCycleId, string? TrainingCycleName, int Version);
public sealed record SaveLearningEntryRequest(DateOnly Date, string Title, string Category, string Observation,
    string? Interpretation, string? NextAction, DateOnly? ReviewDueOn, string Confidence, string Status, DateOnly? ReviewedOn,
    string? FollowUpOutcome, string? FollowUpObservation, Guid? TrainingSessionId, Guid? PersonalGoalId,
    Guid? TrainingCycleId, string ChangeReason);
