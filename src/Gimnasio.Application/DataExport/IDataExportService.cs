namespace Gimnasio.Application.DataExport;

public interface IDataExportService
{
    Task<PersonalDataSummaryDto> GetSummaryAsync(Guid userId, CancellationToken ct = default);
    Task<PersonalDataExportFileDto> ExportAsync(Guid userId, CancellationToken ct = default);
    Task<RestorePreviewDto> PreviewRestoreAsync(Guid userId, byte[] content, CancellationToken ct = default);
    Task<RestoreApplyDto> RestoreMissingCoreAsync(Guid userId,byte[] content,string expectedSha256,string safetyBackupSha256,string confirmation,CancellationToken ct=default);
    Task<IReadOnlyCollection<DataTransferOperationDto>> GetOperationsAsync(Guid userId,CancellationToken ct=default);
}

public sealed record PersonalDataSummaryDto(
    int ProfileFacts,
    int CheckIns,
    int DailyActivities,
    int DailyDecisions,
    int RecurringSchedules,
    int KneeChecks,
    int TrainingSessions,
    int TrainingExercises,
    int FollowUps,
    int Goals,
    int Cycles,
    int PlanChanges,
    int Metrics,
    int Measurements,
    int LearningEntries,
    int SharedReports,
    int ProfessionalFeedback,
    DateTimeOffset? OldestRecord,
    DateTimeOffset? LatestRecord);

public sealed record PersonalDataExportFileDto(byte[] Content,string FileName,string Sha256,DateTimeOffset ExportedAt);
public sealed record RestoreModulePreviewDto(string Key,string Label,int BackupRecords,int CurrentRecords,int AlreadyPresent,int Missing);
public sealed record RestorePreviewDto(string Application,string FormatVersion,DateTimeOffset ExportedAt,string Sha256,long FileBytes,bool SameAccount,int TotalBackupRecords,int TotalMissing,IReadOnlyCollection<RestoreModulePreviewDto>Modules,IReadOnlyCollection<string>Warnings,string Status);
public sealed record RestoreApplyModuleDto(string Key,string Label,int Restored,int AlreadyPresent,int Conflicts);
public sealed record RestoreApplyDto(DateTimeOffset AppliedAt,string SourceSha256,string SafetyBackupSha256,int Restored,int AlreadyPresent,int Conflicts,IReadOnlyCollection<RestoreApplyModuleDto>Modules,IReadOnlyCollection<string>DeferredModules);
public sealed record DataTransferOperationDto(Guid Id,string OperationType,string Status,string Sha256,string?SafetyBackupSha256,string?FileName,int Restored,int AlreadyPresent,int Conflicts,DateTimeOffset CreatedAt);
