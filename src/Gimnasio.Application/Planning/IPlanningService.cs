namespace Gimnasio.Application.Planning;
public interface IPlanningService{Task<PlanningOverviewDto>GetAsync(Guid userId,CancellationToken ct=default);Task<GoalDto>SaveGoalAsync(Guid userId,Guid? id,SaveGoalRequest request,CancellationToken ct=default);Task<CycleDto>SaveCycleAsync(Guid userId,Guid? id,SaveCycleRequest request,CancellationToken ct=default);}
public sealed record PlanningOverviewDto(IReadOnlyCollection<GoalDto>Goals,IReadOnlyCollection<CycleDto>Cycles,IReadOnlyCollection<PlanChangeDto>Changes,IReadOnlyCollection<MetricOptionDto>Metrics);
public sealed record MetricOptionDto(Guid Id,string Name,string Unit,string Direction);
public sealed record GoalDto(Guid Id,string Title,string Category,decimal?BaselineValue,decimal?TargetValue,string?Unit,DateOnly StartDate,DateOnly?TargetDate,string Status,string?Rationale,int Version,Guid?MetricDefinitionId,string?MetricName,decimal?LatestValue,DateOnly?LatestDate,decimal?ProgressPercent);
public sealed record CycleDto(Guid Id,string Name,DateOnly StartDate,DateOnly EndDate,string Focus,int PlannedSessionsPerWeek,string Status,string?Notes,int Version,int ExpectedSessions,int CompletedSessions,decimal?AdherencePercent);
public sealed record PlanChangeDto(Guid Id,string EntityType,Guid EntityId,int Version,string Reason,string Summary,DateTimeOffset ChangedAt);
public sealed record SaveGoalRequest(string Title,string Category,decimal?BaselineValue,decimal?TargetValue,string?Unit,DateOnly StartDate,DateOnly?TargetDate,string Status,string?Rationale,string ChangeReason,Guid?MetricDefinitionId);
public sealed record SaveCycleRequest(string Name,DateOnly StartDate,DateOnly EndDate,string Focus,int PlannedSessionsPerWeek,string Status,string?Notes,string ChangeReason);
