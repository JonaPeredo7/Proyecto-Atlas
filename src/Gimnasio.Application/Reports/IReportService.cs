namespace Gimnasio.Application.Reports;
public interface IReportService
{
    Task<ProfessionalReportDto> GetProfessionalAsync(Guid userId, DateOnly from, DateOnly to, CancellationToken ct = default);
    Task<CreatedReportShareDto> CreateShareAsync(Guid userId, CreateReportShareRequest request, CancellationToken ct = default);
    Task<IReadOnlyCollection<ReportShareDto>> ListSharesAsync(Guid userId, CancellationToken ct = default);
    Task RevokeShareAsync(Guid userId, Guid shareId, CancellationToken ct = default);
    Task<SharedProfessionalReportDto?> GetSharedAsync(string token, CancellationToken ct = default);
    Task<CreatedReportFeedbackDto> SubmitFeedbackAsync(string token, CreateReportFeedbackRequest request, CancellationToken ct = default);
    Task<IReadOnlyCollection<ReportFeedbackDto>> ListFeedbackAsync(Guid userId, CancellationToken ct = default);
    Task ReviewFeedbackAsync(Guid userId, Guid feedbackId, ReviewReportFeedbackRequest request, CancellationToken ct = default);
}
public sealed record CreateReportShareRequest(DateOnly From,DateOnly To,int ExpiresInDays,bool IncludeKnee,bool IncludeLearning,string? RecipientLabel,bool Consent);
public sealed record CreatedReportShareDto(Guid Id,string Token,DateTimeOffset CreatedAt,DateTimeOffset ExpiresAt,string? RecipientLabel,bool IncludeKnee,bool IncludeLearning);
public sealed record ReportShareDto(Guid Id,DateOnly From,DateOnly To,DateTimeOffset CreatedAt,DateTimeOffset ExpiresAt,DateTimeOffset? RevokedAt,string? RecipientLabel,bool IncludeKnee,bool IncludeLearning,string Status);
public sealed record SharedProfessionalReportDto(ProfessionalReportDto Report,DateTimeOffset SharedAt,DateTimeOffset ExpiresAt,string? RecipientLabel);
public sealed record CreateReportFeedbackRequest(string AuthorName,string Kind,string Section,string Message);
public sealed record CreatedReportFeedbackDto(Guid Id,DateTimeOffset CreatedAt);
public sealed record ReviewReportFeedbackRequest(string Status,string? DecisionNote);
public sealed record ReportFeedbackDto(Guid Id,Guid ReportShareId,string AuthorName,string Kind,string Section,string Message,string Status,string? DecisionNote,DateTimeOffset CreatedAt,DateTimeOffset? ReviewedAt,string? ShareLabel,DateOnly ReportFrom,DateOnly ReportTo);
public sealed record ProfessionalReportDto(DateTimeOffset GeneratedAt,DateOnly From,DateOnly To,ReportProfileDto Profile,ReportSummaryDto Summary,ReportWorkDto? Work,IReadOnlyCollection<ReportWeekDto>Weeks,IReadOnlyCollection<ReportGoalDto>Goals,IReadOnlyCollection<ReportKneeDto>KneeChecks,IReadOnlyCollection<ReportMetricDto>Metrics,IReadOnlyCollection<ReportLearningDto>Learning,int DataCoverageDays,string Disclaimer);
public sealed record ReportProfileDto(string DisplayName,decimal?HeightCm,decimal?ReferenceWeightKg,string?PrimaryGoal,DateOnly?TargetDate,string?AffectedKnee);
public sealed record ReportSummaryDto(int Sessions,int TrainingMinutes,int TrainingLoad,int ExternalMinutes,int ExternalLoad,int TotalLoad,int CheckIns,decimal?AverageSleepQuality,decimal?AverageEnergy,decimal?AverageFatigue,decimal?AverageStress,decimal?AveragePain,int KneeAttentionChecks);
public sealed record ReportWorkDto(int RecordedDays,int PlannedMinutes,int ActualMinutes,int DifferenceMinutes,int ExtraMinutes,int ShorterMinutes,int WeeksWithData,int ContextRecordedDays,int BreakMinutes,int UnusualDays);
public sealed record ReportWeekDto(DateOnly From,DateOnly To,int TrainingLoad,int ExternalLoad,int TotalLoad,int Sessions,int WorkRecordedDays,int WorkPlannedMinutes,int WorkActualMinutes,int WorkExtraMinutes,int WorkContextRecordedDays,int WorkBreakMinutes,int WorkUnusualDays);
public sealed record ReportGoalDto(string Title,string Category,string Status,decimal?BaselineValue,decimal?TargetValue,string?Unit,decimal?LatestValue,DateOnly?LatestDate,decimal?ProgressPercent);
public sealed record ReportKneeDto(DateTimeOffset RecordedAt,string Context,string Side,int PainNow,int PainWorst24H,string Swelling,bool Instability,bool Locking,bool FullExtension,int Function,string State,IReadOnlyCollection<string>Reasons);
public sealed record ReportMetricDto(string Name,string Category,string Unit,string Direction,decimal FirstValue,DateOnly FirstDate,decimal LatestValue,DateOnly LatestDate,decimal Change,int Entries);
public sealed record ReportLearningDto(DateOnly Date,string Title,string Observation,string?Interpretation,string?NextAction,string Confidence,string Status);
