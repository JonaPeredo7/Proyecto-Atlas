using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using Gimnasio.Application.DataExport;
using Gimnasio.Domain.Entities;
using Gimnasio.Domain.Enums;
using Gimnasio.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Gimnasio.Infrastructure.DataExport;

internal sealed class DataExportService(GimnasioDbContext db) : IDataExportService
{
    public async Task<PersonalDataSummaryDto> GetSummaryAsync(Guid userId, CancellationToken ct = default)
    {
        var profileId = await ProfileId(userId, ct);
        var sessionIds = await db.TrainingSessions.Where(x => x.AthleteProfileId == profileId).Select(x => x.Id).ToArrayAsync(ct);
        var metricIds = await db.MetricDefinitions.Where(x => x.AthleteProfileId == profileId).Select(x => x.Id).ToArrayAsync(ct);
        var shareIds = await db.ReportShares.Where(x => x.AthleteProfileId == profileId).Select(x => x.Id).ToArrayAsync(ct);
        var timestamps = new List<DateTimeOffset?>
        {
            await db.AthleteProfiles.Where(x=>x.Id==profileId).Select(x=>(DateTimeOffset?)x.CreatedAt).SingleAsync(ct),
            await db.DailyCheckIns.Where(x=>x.AthleteProfileId==profileId).MinAsync(x=>(DateTimeOffset?)x.CreatedAt,ct),
            await db.TrainingSessions.Where(x=>x.AthleteProfileId==profileId).MinAsync(x=>(DateTimeOffset?)x.CreatedAt,ct),
            await db.MeasurementEntries.Where(x=>metricIds.Contains(x.MetricDefinitionId)).MinAsync(x=>(DateTimeOffset?)x.CreatedAt,ct),
            await db.LearningEntries.Where(x=>x.AthleteProfileId==profileId).MinAsync(x=>(DateTimeOffset?)x.CreatedAt,ct)
            ,await db.RecurringScheduleBlocks.Where(x=>x.AthleteProfileId==profileId).MinAsync(x=>(DateTimeOffset?)x.CreatedAt,ct)
        };
        var latest = new List<DateTimeOffset?>
        {
            await db.AthleteProfiles.Where(x=>x.Id==profileId).Select(x=>(DateTimeOffset?)(x.UpdatedAt??x.CreatedAt)).SingleAsync(ct),
            await db.DailyCheckIns.Where(x=>x.AthleteProfileId==profileId).MaxAsync(x=>(DateTimeOffset?)(x.UpdatedAt??x.CreatedAt),ct),
            await db.TrainingSessions.Where(x=>x.AthleteProfileId==profileId).MaxAsync(x=>(DateTimeOffset?)(x.UpdatedAt??x.CreatedAt),ct),
            await db.MeasurementEntries.Where(x=>metricIds.Contains(x.MetricDefinitionId)).MaxAsync(x=>(DateTimeOffset?)(x.UpdatedAt??x.CreatedAt),ct),
            await db.LearningEntries.Where(x=>x.AthleteProfileId==profileId).MaxAsync(x=>(DateTimeOffset?)(x.UpdatedAt??x.CreatedAt),ct)
            ,await db.RecurringScheduleBlocks.Where(x=>x.AthleteProfileId==profileId).MaxAsync(x=>(DateTimeOffset?)(x.UpdatedAt??x.CreatedAt),ct)
        };
        return new(
            await db.ProfileFacts.CountAsync(x=>x.AthleteProfileId==profileId,ct),
            await db.DailyCheckIns.CountAsync(x=>x.AthleteProfileId==profileId,ct),
            await db.DailyActivities.CountAsync(x=>x.AthleteProfileId==profileId,ct),
            await db.DailyPlanDecisions.CountAsync(x=>x.AthleteProfileId==profileId,ct),
            await db.RecurringScheduleBlocks.CountAsync(x=>x.AthleteProfileId==profileId,ct),
            await db.KneeChecks.CountAsync(x=>x.AthleteProfileId==profileId,ct),
            sessionIds.Length,
            await db.TrainingExercises.CountAsync(x=>sessionIds.Contains(x.TrainingSessionId),ct),
            await db.TrainingFollowUps.CountAsync(x=>sessionIds.Contains(x.TrainingSessionId),ct),
            await db.PersonalGoals.CountAsync(x=>x.AthleteProfileId==profileId,ct),
            await db.TrainingCycles.CountAsync(x=>x.AthleteProfileId==profileId,ct),
            await db.PlanChanges.CountAsync(x=>x.AthleteProfileId==profileId,ct),
            metricIds.Length,
            await db.MeasurementEntries.CountAsync(x=>metricIds.Contains(x.MetricDefinitionId),ct),
            await db.LearningEntries.CountAsync(x=>x.AthleteProfileId==profileId,ct),
            shareIds.Length,
            await db.ReportFeedback.CountAsync(x=>shareIds.Contains(x.ReportShareId),ct),
            timestamps.Where(x=>x.HasValue).Min(),latest.Where(x=>x.HasValue).Max());
    }

    public async Task<PersonalDataExportFileDto> ExportAsync(Guid userId, CancellationToken ct = default)
    {
        var profileId = await ProfileId(userId, ct);
        var account = await db.Users.AsNoTracking().Where(x=>x.Id==userId).Select(x=>new{x.Id,x.Email,x.FirstName,x.LastName}).SingleAsync(ct);
        var profile = await db.AthleteProfiles.AsNoTracking().SingleAsync(x=>x.Id==profileId,ct);
        var sessions = await db.TrainingSessions.AsNoTracking().Where(x=>x.AthleteProfileId==profileId).OrderBy(x=>x.Date).ToArrayAsync(ct);
        var sessionIds = sessions.Select(x=>x.Id).ToArray();
        var metrics = await db.MetricDefinitions.AsNoTracking().Where(x=>x.AthleteProfileId==profileId).ToArrayAsync(ct);
        var metricIds = metrics.Select(x=>x.Id).ToArray();
        var shares = await db.ReportShares.AsNoTracking().Where(x=>x.AthleteProfileId==profileId).OrderBy(x=>x.CreatedAt).ToArrayAsync(ct);
        var shareIds = shares.Select(x=>x.Id).ToArray();
        var summary = await GetSummaryAsync(userId,ct);
        var exportedAt = DateTimeOffset.UtcNow;
        var bundle = new
        {
            application="Proyecto Atlas",
            formatVersion="1.0",
            exportedAt,
            scope="Copia personal portable. No contiene contraseña, cookies ni códigos secretos de enlaces.",
            manifest=summary,
            account,
            athleteProfile=profile,
            profileFacts=await db.ProfileFacts.AsNoTracking().Where(x=>x.AthleteProfileId==profileId).OrderBy(x=>x.Category).ThenBy(x=>x.Label).ToArrayAsync(ct),
            checkIns=await db.DailyCheckIns.AsNoTracking().Where(x=>x.AthleteProfileId==profileId).OrderBy(x=>x.Date).ToArrayAsync(ct),
            dailyActivities=await db.DailyActivities.AsNoTracking().Where(x=>x.AthleteProfileId==profileId).OrderBy(x=>x.Date).ToArrayAsync(ct),
            dailyDecisions=await db.DailyPlanDecisions.AsNoTracking().Where(x=>x.AthleteProfileId==profileId).OrderBy(x=>x.Date).ToArrayAsync(ct),
            recurringSchedules=await db.RecurringScheduleBlocks.AsNoTracking().Where(x=>x.AthleteProfileId==profileId).OrderBy(x=>x.DayOfWeek).ThenBy(x=>x.StartTime).ToArrayAsync(ct),
            kneeChecks=await db.KneeChecks.AsNoTracking().Where(x=>x.AthleteProfileId==profileId).OrderBy(x=>x.RecordedAt).ToArrayAsync(ct),
            goals=await db.PersonalGoals.AsNoTracking().Where(x=>x.AthleteProfileId==profileId).OrderBy(x=>x.CreatedAt).ToArrayAsync(ct),
            cycles=await db.TrainingCycles.AsNoTracking().Where(x=>x.AthleteProfileId==profileId).OrderBy(x=>x.StartDate).ToArrayAsync(ct),
            planChanges=await db.PlanChanges.AsNoTracking().Where(x=>x.AthleteProfileId==profileId).OrderBy(x=>x.CreatedAt).ToArrayAsync(ct),
            trainingSessions=sessions,
            trainingExercises=await db.TrainingExercises.AsNoTracking().Where(x=>sessionIds.Contains(x.TrainingSessionId)).OrderBy(x=>x.TrainingSessionId).ThenBy(x=>x.Order).ToArrayAsync(ct),
            trainingFollowUps=await db.TrainingFollowUps.AsNoTracking().Where(x=>sessionIds.Contains(x.TrainingSessionId)).OrderBy(x=>x.RecordedAt).ToArrayAsync(ct),
            metricDefinitions=metrics,
            measurements=await db.MeasurementEntries.AsNoTracking().Where(x=>metricIds.Contains(x.MetricDefinitionId)).OrderBy(x=>x.Date).ToArrayAsync(ct),
            learningEntries=await db.LearningEntries.AsNoTracking().Where(x=>x.AthleteProfileId==profileId).OrderBy(x=>x.Date).ToArrayAsync(ct),
            sharedReports=shares.Select(x=>new{x.Id,x.From,x.To,x.ExpiresAt,x.ConsentGrantedAt,x.RevokedAt,x.IncludeKnee,x.IncludeLearning,x.RecipientLabel,x.CreatedAt,x.UpdatedAt,x.IsActive,snapshot=JsonSerializer.Deserialize<JsonElement>(x.SnapshotJson)}),
            professionalFeedback=await db.ReportFeedback.AsNoTracking().Where(x=>shareIds.Contains(x.ReportShareId)).OrderBy(x=>x.CreatedAt).ToArrayAsync(ct)
        };
        var options=new JsonSerializerOptions(JsonSerializerDefaults.Web){WriteIndented=true};options.Converters.Add(new JsonStringEnumConverter());
        var content=JsonSerializer.SerializeToUtf8Bytes(bundle,options);
        var sha=Convert.ToHexString(SHA256.HashData(content)).ToLowerInvariant();
        var fileName=$"proyecto-atlas-respaldo-{exportedAt:yyyy-MM-dd-HHmm}.json";
        db.DataTransferOperations.Add(new DataTransferOperation(profileId,"backup",sha,fileName,null,0,0,0));
        await db.SaveChangesAsync(ct);
        return new(content,fileName,sha,exportedAt);
    }

    public async Task<IReadOnlyCollection<DataTransferOperationDto>> GetOperationsAsync(Guid userId,CancellationToken ct=default)
    {
        var profileId=await ProfileId(userId,ct);
        return await db.DataTransferOperations.AsNoTracking().Where(x=>x.AthleteProfileId==profileId).OrderByDescending(x=>x.CreatedAt).Take(30).Select(x=>new DataTransferOperationDto(x.Id,x.OperationType,x.Status,x.Sha256,x.SafetyBackupSha256,x.FileName,x.Restored,x.AlreadyPresent,x.Conflicts,x.CreatedAt)).ToArrayAsync(ct);
    }

    public async Task<RestorePreviewDto> PreviewRestoreAsync(Guid userId, byte[] content, CancellationToken ct = default)
    {
        if (content.Length == 0) throw new ArgumentException("El archivo está vacío.");
        JsonDocument document;
        try { document = JsonDocument.Parse(content, new JsonDocumentOptions { MaxDepth = 48 }); }
        catch (JsonException) { throw new ArgumentException("El archivo no contiene un JSON válido."); }
        using (document)
        {
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object) throw new ArgumentException("La estructura del respaldo no es válida.");
            var application = Text(root, "application") ?? "";
            var version = Text(root, "formatVersion") ?? "";
            if (application != "Proyecto Atlas") throw new ArgumentException("El archivo no pertenece a Proyecto Atlas.");
            if (version != "1.0") throw new ArgumentException($"La versión {version} todavía no es compatible con esta restauración.");
            if (!root.TryGetProperty("exportedAt", out var exportedElement) || !exportedElement.TryGetDateTimeOffset(out var exportedAt)) throw new ArgumentException("Falta la fecha válida de exportación.");
            var backupUserId = root.TryGetProperty("account", out var account) && account.ValueKind==JsonValueKind.Object && account.TryGetProperty("id", out var accountId) && accountId.TryGetGuid(out var parsedUserId) ? parsedUserId : (Guid?)null;
            var warnings = new List<string>();
            var sameAccount = backupUserId == userId;
            if (!backupUserId.HasValue) warnings.Add("El respaldo no identifica la cuenta de origen.");
            else if (!sameAccount) warnings.Add("El respaldo pertenece a otra cuenta. Una restauración futura requerirá remapeo explícito del perfil.");

            var profileId = await ProfileId(userId, ct);
            var sessionIds = await db.TrainingSessions.Where(x=>x.AthleteProfileId==profileId).Select(x=>x.Id).ToArrayAsync(ct);
            var metricIds = await db.MetricDefinitions.Where(x=>x.AthleteProfileId==profileId).Select(x=>x.Id).ToArrayAsync(ct);
            var shareIds = await db.ReportShares.Where(x=>x.AthleteProfileId==profileId).Select(x=>x.Id).ToArrayAsync(ct);
            var decisionBackupIds=root.TryGetProperty("dailyDecisions",out _)?BackupIds(root,"dailyDecisions",warnings):[];
            var scheduleBackupIds=root.TryGetProperty("recurringSchedules",out _)?BackupIds(root,"recurringSchedules",warnings):[];
            var existing = new (string Key,string Label,HashSet<Guid> Ids)[]
            {
                ("profileFacts","Hechos del perfil",(await db.ProfileFacts.Where(x=>x.AthleteProfileId==profileId).Select(x=>x.Id).ToArrayAsync(ct)).ToHashSet()),
                ("checkIns","Check-ins",(await db.DailyCheckIns.Where(x=>x.AthleteProfileId==profileId).Select(x=>x.Id).ToArrayAsync(ct)).ToHashSet()),
                ("dailyActivities","Actividades cotidianas",(await db.DailyActivities.Where(x=>x.AthleteProfileId==profileId).Select(x=>x.Id).ToArrayAsync(ct)).ToHashSet()),
                ("dailyDecisions","Decisiones diarias",(await db.DailyPlanDecisions.Where(x=>x.AthleteProfileId==profileId).Select(x=>x.Id).ToArrayAsync(ct)).ToHashSet()),
                ("recurringSchedules","Horarios recurrentes",(await db.RecurringScheduleBlocks.Where(x=>x.AthleteProfileId==profileId).Select(x=>x.Id).ToArrayAsync(ct)).ToHashSet()),
                ("kneeChecks","Controles de rodilla",(await db.KneeChecks.Where(x=>x.AthleteProfileId==profileId).Select(x=>x.Id).ToArrayAsync(ct)).ToHashSet()),
                ("goals","Objetivos",(await db.PersonalGoals.Where(x=>x.AthleteProfileId==profileId).Select(x=>x.Id).ToArrayAsync(ct)).ToHashSet()),
                ("cycles","Ciclos",(await db.TrainingCycles.Where(x=>x.AthleteProfileId==profileId).Select(x=>x.Id).ToArrayAsync(ct)).ToHashSet()),
                ("planChanges","Cambios del plan",(await db.PlanChanges.Where(x=>x.AthleteProfileId==profileId).Select(x=>x.Id).ToArrayAsync(ct)).ToHashSet()),
                ("trainingSessions","Sesiones",sessionIds.ToHashSet()),
                ("trainingExercises","Ejercicios",(await db.TrainingExercises.Where(x=>sessionIds.Contains(x.TrainingSessionId)).Select(x=>x.Id).ToArrayAsync(ct)).ToHashSet()),
                ("trainingFollowUps","Respuestas posteriores",(await db.TrainingFollowUps.Where(x=>sessionIds.Contains(x.TrainingSessionId)).Select(x=>x.Id).ToArrayAsync(ct)).ToHashSet()),
                ("metricDefinitions","Indicadores",metricIds.ToHashSet()),
                ("measurements","Mediciones",(await db.MeasurementEntries.Where(x=>metricIds.Contains(x.MetricDefinitionId)).Select(x=>x.Id).ToArrayAsync(ct)).ToHashSet()),
                ("learningEntries","Bitácora",(await db.LearningEntries.Where(x=>x.AthleteProfileId==profileId).Select(x=>x.Id).ToArrayAsync(ct)).ToHashSet()),
                ("sharedReports","Informes compartidos",shareIds.ToHashSet()),
                ("professionalFeedback","Aportes profesionales",(await db.ReportFeedback.Where(x=>shareIds.Contains(x.ReportShareId)).Select(x=>x.Id).ToArrayAsync(ct)).ToHashSet())
            };
            var modules = new List<RestoreModulePreviewDto>();
            foreach (var module in existing)
            {
                var backupIds = module.Key=="dailyDecisions"?decisionBackupIds:module.Key=="recurringSchedules"?scheduleBackupIds:BackupIds(root,module.Key,warnings);
                var present = backupIds.Count(module.Ids.Contains);
                modules.Add(new(module.Key,module.Label,backupIds.Count,module.Ids.Count,present,backupIds.Count-present));
            }
            var totalBackup = modules.Sum(x=>x.BackupRecords);
            var totalMissing = modules.Sum(x=>x.Missing);
            if (root.TryGetProperty("manifest",out var manifest)&&manifest.ValueKind==JsonValueKind.Object)
            {
                var expected = new Dictionary<string,string>{{"profileFacts","profileFacts"},{"checkIns","checkIns"},{"dailyActivities","dailyActivities"},{"dailyDecisions","dailyDecisions"},{"recurringSchedules","recurringSchedules"},{"kneeChecks","kneeChecks"},{"trainingSessions","trainingSessions"},{"trainingExercises","trainingExercises"},{"trainingFollowUps","followUps"},{"goals","goals"},{"cycles","cycles"},{"planChanges","planChanges"},{"metricDefinitions","metrics"},{"measurements","measurements"},{"learningEntries","learningEntries"},{"sharedReports","sharedReports"},{"professionalFeedback","professionalFeedback"}};
                foreach(var item in modules){if(expected.TryGetValue(item.Key,out var manifestKey)&&manifest.TryGetProperty(manifestKey,out var count)&&count.TryGetInt32(out var declared)&&declared!=item.BackupRecords)warnings.Add($"El inventario declarado de {item.Label} no coincide con el contenido real.");}
            }
            else warnings.Add("El archivo no contiene el inventario original.");
            var sha = Convert.ToHexString(SHA256.HashData(content)).ToLowerInvariant();
            var status = totalMissing == 0 ? "nothing_to_restore" : warnings.Count == 0 ? "compatible" : "review_required";
            return new(application,version,exportedAt,sha,content.LongLength,sameAccount,totalBackup,totalMissing,modules,warnings,status);
        }
    }

    public async Task<RestoreApplyDto> RestoreMissingCoreAsync(Guid userId,byte[] content,string expectedSha256,string safetyBackupSha256,string confirmation,CancellationToken ct=default)
    {
        if(confirmation!="RESTAURAR FALTANTES")throw new ArgumentException("La frase de confirmación no coincide.");
        if(string.IsNullOrWhiteSpace(safetyBackupSha256)||safetyBackupSha256.Length!=64||!safetyBackupSha256.All(Uri.IsHexDigit))throw new ArgumentException("Primero debe prepararse un respaldo de seguridad válido.");
        var actualSha=Convert.ToHexString(SHA256.HashData(content)).ToLowerInvariant();
        if(!string.Equals(actualSha,expectedSha256,StringComparison.OrdinalIgnoreCase))throw new ArgumentException("El archivo cambió después del análisis. Volvé a analizarlo.");
        var preview=await PreviewRestoreAsync(userId,content,ct);
        if(!preview.SameAccount)throw new ArgumentException("Esta etapa sólo restaura respaldos de la misma cuenta.");
        if(preview.Warnings.Count>0)throw new ArgumentException("El respaldo contiene advertencias y no puede aplicarse. Revisá el análisis.");
        using var document=JsonDocument.Parse(content);var root=document.RootElement;var profileId=await ProfileId(userId,ct);
        var profileFacts=Read<ProfileFactBackup>(root,"profileFacts");var checkIns=Read<CheckInBackup>(root,"checkIns");var activities=Read<ActivityBackup>(root,"dailyActivities");var decisions=ReadOptional<DecisionBackup>(root,"dailyDecisions");var schedules=ReadOptional<ScheduleBackup>(root,"recurringSchedules");var kneeChecks=Read<KneeBackup>(root,"kneeChecks");var metrics=Read<MetricBackup>(root,"metricDefinitions");var measurements=Read<MeasurementBackup>(root,"measurements");var goals=Read<GoalBackup>(root,"goals");var cycles=Read<CycleBackup>(root,"cycles");var sessions=Read<SessionBackup>(root,"trainingSessions");var exercises=Read<ExerciseBackup>(root,"trainingExercises");var followUps=Read<FollowUpBackup>(root,"trainingFollowUps");var learningEntries=Read<LearningBackup>(root,"learningEntries");var planChanges=Read<PlanChangeBackup>(root,"planChanges");var sharedReports=Read<SharedReportBackup>(root,"sharedReports");var professionalFeedback=Read<FeedbackBackup>(root,"professionalFeedback");
        await using var transaction=await db.Database.BeginTransactionAsync(ct);
        try
        {
            var results=new List<RestoreApplyModuleDto>();
            var factIds=(await db.ProfileFacts.Where(x=>x.AthleteProfileId==profileId).Select(x=>x.Id).ToArrayAsync(ct)).ToHashSet();var factKeys=(await db.ProfileFacts.Where(x=>x.AthleteProfileId==profileId).Select(x=>new{x.Category,x.Label}).ToArrayAsync(ct)).Select(x=>$"{x.Category}\u001f{x.Label}").ToHashSet(StringComparer.OrdinalIgnoreCase);var factRestored=0;var factPresent=0;var factConflicts=0;
            foreach(var item in profileFacts){if(factIds.Contains(item.Id)){factPresent++;continue;}if(!factKeys.Add($"{item.Category}\u001f{item.Label}")){factConflicts++;continue;}var entity=new ProfileFact(profileId,item.Category,item.Label,item.Value,item.Status,item.SourceTitle,item.Notes);entity.RestoreMetadata(item.Id,item.CreatedAt,item.UpdatedAt,item.IsActive);db.ProfileFacts.Add(entity);factRestored++;}
            results.Add(new("profileFacts","Hechos del perfil",factRestored,factPresent,factConflicts));

            var checkIds=(await db.DailyCheckIns.Where(x=>x.AthleteProfileId==profileId).Select(x=>x.Id).ToArrayAsync(ct)).ToHashSet();var checkDates=(await db.DailyCheckIns.Where(x=>x.AthleteProfileId==profileId).Select(x=>x.Date).ToArrayAsync(ct)).ToHashSet();var checkRestored=0;var checkPresent=0;var checkConflicts=0;
            foreach(var item in checkIns){if(checkIds.Contains(item.Id)){checkPresent++;continue;}if(!checkDates.Add(item.Date)){checkConflicts++;continue;}var entity=new DailyCheckIn(profileId,item.Date);entity.Record(item.SleepMinutes,item.SleepQuality,item.Energy,item.Fatigue,item.Stress,item.PainLocation,item.PainSide,item.PainIntensity,item.Stiffness,item.Swelling,item.Instability,item.Locking,item.ExpectedWorkLoad,item.PlannedCyclingKm,item.PlannedActivity,item.Notes);entity.RestoreMetadata(item.Id,item.CreatedAt,item.UpdatedAt,item.IsActive);db.DailyCheckIns.Add(entity);checkRestored++;}
            results.Add(new("checkIns","Check-ins",checkRestored,checkPresent,checkConflicts));

            var activityIds=(await db.DailyActivities.Where(x=>x.AthleteProfileId==profileId).Select(x=>x.Id).ToArrayAsync(ct)).ToHashSet();var activityRestored=0;var activityPresent=0;
            foreach(var item in activities){if(activityIds.Contains(item.Id)){activityPresent++;continue;}var entity=new DailyActivity(profileId,item.Date,item.ActivityType);entity.Record(item.Date,item.ActivityType,item.DurationMinutes,item.Rpe,item.DistanceKm,item.Notes);entity.AttachPlanSnapshot(item.PlannedDurationMinutes,item.PlannedSource);entity.AttachWorkContext(item.WorkDemands,item.BreakMinutes,item.UnusualConditions);entity.RestoreMetadata(item.Id,item.CreatedAt,item.UpdatedAt,item.IsActive);db.DailyActivities.Add(entity);activityRestored++;}
            results.Add(new("dailyActivities","Actividades cotidianas",activityRestored,activityPresent,0));

            var decisionIds=(await db.DailyPlanDecisions.Where(x=>x.AthleteProfileId==profileId).Select(x=>x.Id).ToArrayAsync(ct)).ToHashSet();var decisionDates=(await db.DailyPlanDecisions.Where(x=>x.AthleteProfileId==profileId).Select(x=>x.Date).ToArrayAsync(ct)).ToHashSet();var decisionRestored=0;var decisionPresent=0;var decisionConflicts=0;
            foreach(var item in decisions){if(decisionIds.Contains(item.Id)){decisionPresent++;continue;}if(!decisionDates.Add(item.Date)){decisionConflicts++;continue;}var entity=new DailyPlanDecision(profileId,item.Date);entity.Record(item.Decision,item.Reason,item.ContextStatus,item.PlannedLoadSnapshot,false);entity.RestoreVersion(item.Version);entity.RestoreMetadata(item.Id,item.CreatedAt,item.UpdatedAt,item.IsActive);db.DailyPlanDecisions.Add(entity);decisionIds.Add(item.Id);decisionRestored++;}
            results.Add(new("dailyDecisions","Decisiones diarias",decisionRestored,decisionPresent,decisionConflicts));

            var scheduleIds=(await db.RecurringScheduleBlocks.Where(x=>x.AthleteProfileId==profileId).Select(x=>x.Id).ToArrayAsync(ct)).ToHashSet();var scheduleKeys=(await db.RecurringScheduleBlocks.Where(x=>x.AthleteProfileId==profileId).Select(x=>new{x.DayOfWeek,x.Name,x.EffectiveFrom}).ToArrayAsync(ct)).Select(x=>$"{x.DayOfWeek}|{x.EffectiveFrom:yyyy-MM-dd}|{x.Name}").ToHashSet(StringComparer.OrdinalIgnoreCase);var scheduleRestored=0;var schedulePresent=0;var scheduleConflicts=0;
            foreach(var item in schedules){if(scheduleIds.Contains(item.Id)){schedulePresent++;continue;}if(!scheduleKeys.Add($"{item.DayOfWeek}|{item.EffectiveFrom:yyyy-MM-dd}|{item.Name}")){scheduleConflicts++;continue;}var entity=new RecurringScheduleBlock(profileId,item.DayOfWeek,item.Name,item.Category);entity.Configure(item.TimeWindow,item.StartTime,item.EndTime,item.EffectiveFrom,item.EffectiveTo,item.Notes);entity.RestoreMetadata(item.Id,item.CreatedAt,item.UpdatedAt,item.IsActive);db.RecurringScheduleBlocks.Add(entity);scheduleRestored++;}
            results.Add(new("recurringSchedules","Horarios recurrentes",scheduleRestored,schedulePresent,scheduleConflicts));

            var kneeIds=(await db.KneeChecks.Where(x=>x.AthleteProfileId==profileId).Select(x=>x.Id).ToArrayAsync(ct)).ToHashSet();var kneeRestored=0;var kneePresent=0;
            foreach(var item in kneeChecks){if(kneeIds.Contains(item.Id)){kneePresent++;continue;}var entity=new KneeCheck(profileId);entity.Record(item.RecordedAt,item.Context,item.Side,item.PainNow,item.PainBest24H,item.PainWorst24H,item.Swelling,item.Instability,item.Locking,item.FullExtension,item.WalkingCapacity,item.StairsCapacity,item.SquatCapacity,item.Notes);entity.RestoreMetadata(item.Id,item.CreatedAt,item.UpdatedAt,item.IsActive);db.KneeChecks.Add(entity);kneeRestored++;}
            results.Add(new("kneeChecks","Controles de rodilla",kneeRestored,kneePresent,0));

            var currentMetrics=await db.MetricDefinitions.Where(x=>x.AthleteProfileId==profileId).ToArrayAsync(ct);var metricIds=currentMetrics.Select(x=>x.Id).ToHashSet();var metricByName=currentMetrics.GroupBy(x=>x.Name,StringComparer.OrdinalIgnoreCase).ToDictionary(x=>x.Key,x=>x.First().Id,StringComparer.OrdinalIgnoreCase);var metricMap=new Dictionary<Guid,Guid>();var metricRestored=0;var metricPresent=0;var metricConflicts=0;
            foreach(var item in metrics){if(metricIds.Contains(item.Id)){metricMap[item.Id]=item.Id;metricPresent++;continue;}if(metricByName.TryGetValue(item.Name,out var equivalent)){metricMap[item.Id]=equivalent;metricConflicts++;continue;}var entity=new MetricDefinition(profileId,item.Name,item.Category,item.Unit);entity.Configure(item.Direction,item.TargetValue,item.TargetDate,item.Protocol,item.SourceTitle,item.SourceUrl);entity.RestoreMetadata(item.Id,item.CreatedAt,item.UpdatedAt,item.IsActive);db.MetricDefinitions.Add(entity);metricIds.Add(item.Id);metricByName[item.Name]=item.Id;metricMap[item.Id]=item.Id;metricRestored++;}
            results.Add(new("metricDefinitions","Indicadores",metricRestored,metricPresent,metricConflicts));

            var currentMeasurements=await db.MeasurementEntries.Where(x=>metricIds.Contains(x.MetricDefinitionId)).ToArrayAsync(ct);var measurementIds=currentMeasurements.Select(x=>x.Id).ToHashSet();var measurementKeys=currentMeasurements.Select(x=>$"{x.MetricDefinitionId:N}|{x.Date:yyyy-MM-dd}").ToHashSet();var measurementRestored=0;var measurementPresent=0;var measurementConflicts=0;
            foreach(var item in measurements){if(measurementIds.Contains(item.Id)){measurementPresent++;continue;}if(!metricMap.TryGetValue(item.MetricDefinitionId,out var metricId)){measurementConflicts++;continue;}if(!measurementKeys.Add($"{metricId:N}|{item.Date:yyyy-MM-dd}")){measurementConflicts++;continue;}var entity=new MeasurementEntry(metricId,item.Date,item.Value);entity.Record(item.Value,item.Conditions,item.Notes);entity.RestoreMetadata(item.Id,item.CreatedAt,item.UpdatedAt,item.IsActive);db.MeasurementEntries.Add(entity);measurementIds.Add(item.Id);measurementRestored++;}
            results.Add(new("measurements","Mediciones",measurementRestored,measurementPresent,measurementConflicts));

            var currentGoals=await db.PersonalGoals.Where(x=>x.AthleteProfileId==profileId).ToArrayAsync(ct);var goalIds=currentGoals.Select(x=>x.Id).ToHashSet();var goalByKey=currentGoals.GroupBy(x=>$"{x.Category}\u001f{x.Title}",StringComparer.OrdinalIgnoreCase).ToDictionary(x=>x.Key,x=>x.First().Id,StringComparer.OrdinalIgnoreCase);var goalMap=new Dictionary<Guid,Guid>();var goalRestored=0;var goalPresent=0;var goalConflicts=0;
            foreach(var item in goals){if(goalIds.Contains(item.Id)){goalMap[item.Id]=item.Id;goalPresent++;continue;}var key=$"{item.Category}\u001f{item.Title}";if(goalByKey.TryGetValue(key,out var equivalent)){goalMap[item.Id]=equivalent;goalConflicts++;continue;}Guid?metricId=null;if(item.MetricDefinitionId.HasValue){if(!metricMap.TryGetValue(item.MetricDefinitionId.Value,out var mapped)){goalConflicts++;continue;}metricId=mapped;}var entity=new PersonalGoal(profileId,item.Title);entity.Configure(item.Title,item.Category,item.BaselineValue,item.TargetValue,item.Unit,item.StartDate,item.TargetDate,item.Status,item.Rationale,false);entity.LinkMetric(metricId);entity.RestoreVersion(item.Version);entity.RestoreMetadata(item.Id,item.CreatedAt,item.UpdatedAt,item.IsActive);db.PersonalGoals.Add(entity);goalIds.Add(item.Id);goalByKey[key]=item.Id;goalMap[item.Id]=item.Id;goalRestored++;}
            results.Add(new("goals","Objetivos",goalRestored,goalPresent,goalConflicts));

            var currentCycles=await db.TrainingCycles.Where(x=>x.AthleteProfileId==profileId).ToArrayAsync(ct);var cycleIds=currentCycles.Select(x=>x.Id).ToHashSet();var cycleByKey=currentCycles.GroupBy(x=>$"{x.Name}\u001f{x.StartDate:yyyy-MM-dd}\u001f{x.EndDate:yyyy-MM-dd}",StringComparer.OrdinalIgnoreCase).ToDictionary(x=>x.Key,x=>x.First().Id,StringComparer.OrdinalIgnoreCase);var cycleMap=new Dictionary<Guid,Guid>();var cycleRestored=0;var cyclePresent=0;var cycleConflicts=0;
            foreach(var item in cycles){if(cycleIds.Contains(item.Id)){cycleMap[item.Id]=item.Id;cyclePresent++;continue;}var key=$"{item.Name}\u001f{item.StartDate:yyyy-MM-dd}\u001f{item.EndDate:yyyy-MM-dd}";if(cycleByKey.TryGetValue(key,out var equivalent)){cycleMap[item.Id]=equivalent;cycleConflicts++;continue;}var entity=new TrainingCycle(profileId,item.Name);entity.Configure(item.Name,item.StartDate,item.EndDate,item.Focus,item.PlannedSessionsPerWeek,item.Status,item.Notes,false);entity.RestoreVersion(item.Version);entity.RestoreMetadata(item.Id,item.CreatedAt,item.UpdatedAt,item.IsActive);db.TrainingCycles.Add(entity);cycleIds.Add(item.Id);cycleByKey[key]=item.Id;cycleMap[item.Id]=item.Id;cycleRestored++;}
            results.Add(new("cycles","Ciclos",cycleRestored,cyclePresent,cycleConflicts));

            var currentSessions=await db.TrainingSessions.Where(x=>x.AthleteProfileId==profileId).ToArrayAsync(ct);var sessionIds=currentSessions.Select(x=>x.Id).ToHashSet();var sessionByKey=currentSessions.GroupBy(x=>$"{x.Date:yyyy-MM-dd}\u001f{x.ActivityType}\u001f{x.Name}",StringComparer.OrdinalIgnoreCase).ToDictionary(x=>x.Key,x=>x.First().Id,StringComparer.OrdinalIgnoreCase);var sessionMap=new Dictionary<Guid,Guid>();var sessionRestored=0;var sessionPresent=0;var sessionConflicts=0;
            foreach(var item in sessions){if(sessionIds.Contains(item.Id)){sessionMap[item.Id]=item.Id;sessionPresent++;continue;}var key=$"{item.Date:yyyy-MM-dd}\u001f{item.ActivityType}\u001f{item.Name}";if(sessionByKey.TryGetValue(key,out var equivalent)){sessionMap[item.Id]=equivalent;sessionConflicts++;continue;}Guid?goalId=null;Guid?cycleId=null;if(item.PersonalGoalId.HasValue){if(!goalMap.TryGetValue(item.PersonalGoalId.Value,out var mapped)){sessionConflicts++;continue;}goalId=mapped;}if(item.TrainingCycleId.HasValue){if(!cycleMap.TryGetValue(item.TrainingCycleId.Value,out var mapped)){sessionConflicts++;continue;}cycleId=mapped;}var entity=new TrainingSession(profileId,item.Date,item.Name,item.ActivityType);entity.Configure(item.PlannedDurationMinutes,item.TargetRpe,item.Goal,item.Notes);entity.Schedule(item.PlannedStartTime);entity.LinkPlan(goalId,cycleId,false);entity.RestoreState(item.Status,item.Version,item.ActualDurationMinutes,item.SessionRpe,item.CompletionNotes,item.CompletedAt);entity.RestoreMetadata(item.Id,item.CreatedAt,item.UpdatedAt,item.IsActive);db.TrainingSessions.Add(entity);sessionIds.Add(item.Id);sessionByKey[key]=item.Id;sessionMap[item.Id]=item.Id;sessionRestored++;}
            results.Add(new("trainingSessions","Sesiones",sessionRestored,sessionPresent,sessionConflicts));

            var currentExercises=await db.TrainingExercises.Where(x=>sessionIds.Contains(x.TrainingSessionId)).ToArrayAsync(ct);var exerciseIds=currentExercises.Select(x=>x.Id).ToHashSet();var exerciseBySlot=currentExercises.ToDictionary(x=>$"{x.TrainingSessionId:N}|{x.Order}",x=>x.Id);var exerciseMap=new Dictionary<Guid,Guid>();var exerciseRestored=0;var exercisePresent=0;var exerciseConflicts=0;
            foreach(var item in exercises){if(exerciseIds.Contains(item.Id)){exerciseMap[item.Id]=item.Id;exercisePresent++;continue;}if(!sessionMap.TryGetValue(item.TrainingSessionId,out var sessionId)){exerciseConflicts++;continue;}var slot=$"{sessionId:N}|{item.Order}";if(exerciseBySlot.TryGetValue(slot,out var equivalent)){exerciseMap[item.Id]=equivalent;exerciseConflicts++;continue;}var entity=new TrainingExercise(sessionId,item.Order,item.Name);entity.Prescribe(item.Category,item.PlannedSets,item.PlannedRepetitions,item.PlannedLoadKg,item.RestSeconds,item.Notes);if(item.CompletedSets.HasValue)entity.Record(item.CompletedSets.Value,item.ActualRepetitions,item.ActualLoadKg,item.ExerciseRpe);if(entity.IsCompleted!=item.IsCompleted)throw new ArgumentException($"El resultado del ejercicio {item.Name} es inconsistente.");entity.RestoreMetadata(item.Id,item.CreatedAt,item.UpdatedAt,item.IsActive);db.TrainingExercises.Add(entity);exerciseIds.Add(item.Id);exerciseBySlot[slot]=item.Id;exerciseMap[item.Id]=item.Id;exerciseRestored++;}
            results.Add(new("trainingExercises","Ejercicios",exerciseRestored,exercisePresent,exerciseConflicts));

            var currentFollowUps=await db.TrainingFollowUps.Where(x=>sessionIds.Contains(x.TrainingSessionId)).ToArrayAsync(ct);var followUpIds=currentFollowUps.Select(x=>x.Id).ToHashSet();var followUpBySession=currentFollowUps.ToDictionary(x=>x.TrainingSessionId,x=>x.Id);var followUpRestored=0;var followUpPresent=0;var followUpConflicts=0;
            foreach(var item in followUps){if(followUpIds.Contains(item.Id)){followUpPresent++;continue;}if(!sessionMap.TryGetValue(item.TrainingSessionId,out var sessionId)){followUpConflicts++;continue;}if(followUpBySession.ContainsKey(sessionId)){followUpConflicts++;continue;}var entity=new TrainingFollowUp(sessionId);entity.Record(item.Recovery,item.PainIntensity,item.PainLocation,item.Stiffness,item.Swelling,item.Instability,item.Locking,item.Notes);entity.RestoreRecordedAt(item.RecordedAt);entity.RestoreMetadata(item.Id,item.CreatedAt,item.UpdatedAt,item.IsActive);db.TrainingFollowUps.Add(entity);followUpIds.Add(item.Id);followUpBySession[sessionId]=item.Id;followUpRestored++;}
            results.Add(new("trainingFollowUps","Respuestas posteriores",followUpRestored,followUpPresent,followUpConflicts));

            var currentLearning=await db.LearningEntries.Where(x=>x.AthleteProfileId==profileId).ToArrayAsync(ct);var learningIds=currentLearning.Select(x=>x.Id).ToHashSet();var learningByKey=currentLearning.GroupBy(x=>$"{x.Date:yyyy-MM-dd}\u001f{x.Category}\u001f{x.Title}",StringComparer.OrdinalIgnoreCase).ToDictionary(x=>x.Key,x=>x.First().Id,StringComparer.OrdinalIgnoreCase);var learningMap=new Dictionary<Guid,Guid>();var learningRestored=0;var learningPresent=0;var learningConflicts=0;
            foreach(var item in learningEntries){if(learningIds.Contains(item.Id)){learningMap[item.Id]=item.Id;learningPresent++;continue;}var key=$"{item.Date:yyyy-MM-dd}\u001f{item.Category}\u001f{item.Title}";if(learningByKey.TryGetValue(key,out var equivalent)){learningMap[item.Id]=equivalent;learningConflicts++;continue;}Guid?sessionId=null;Guid?goalId=null;Guid?cycleId=null;if(item.TrainingSessionId.HasValue){if(!sessionMap.TryGetValue(item.TrainingSessionId.Value,out var mapped)){learningConflicts++;continue;}sessionId=mapped;}if(item.PersonalGoalId.HasValue){if(!goalMap.TryGetValue(item.PersonalGoalId.Value,out var mapped)){learningConflicts++;continue;}goalId=mapped;}if(item.TrainingCycleId.HasValue){if(!cycleMap.TryGetValue(item.TrainingCycleId.Value,out var mapped)){learningConflicts++;continue;}cycleId=mapped;}var entity=new LearningEntry(profileId,item.Date,item.Title);entity.Record(item.Date,item.Title,item.Category,item.Observation,item.Interpretation,item.NextAction,null,item.Confidence,"open",null,null,null,sessionId,goalId,cycleId,false);entity.RestoreHistoricalState(item.Version,item.Status,item.ReviewDueOn,item.ReviewedOn,item.FollowUpOutcome,item.FollowUpObservation);entity.RestoreMetadata(item.Id,item.CreatedAt,item.UpdatedAt,item.IsActive);db.LearningEntries.Add(entity);learningIds.Add(item.Id);learningByKey[key]=item.Id;learningMap[item.Id]=item.Id;learningRestored++;}
            results.Add(new("learningEntries","Bitácora",learningRestored,learningPresent,learningConflicts));

            var changeIds=(await db.PlanChanges.Where(x=>x.AthleteProfileId==profileId).Select(x=>x.Id).ToArrayAsync(ct)).ToHashSet();var changeRestored=0;var changePresent=0;var changeConflicts=0;
            foreach(var item in planChanges){if(changeIds.Contains(item.Id)){changePresent++;continue;}var map=item.EntityType.ToLowerInvariant() switch{"goal"=>goalMap,"cycle"=>cycleMap,"session"=>sessionMap,"exercise"=>exerciseMap,"learning"=>learningMap,_=>null};if(map is null||!map.TryGetValue(item.EntityId,out var entityId)){changeConflicts++;continue;}var entity=new PlanChange(profileId,item.EntityType,entityId,item.Version,item.Reason,item.Summary);entity.RestoreMetadata(item.Id,item.CreatedAt,item.UpdatedAt,item.IsActive);db.PlanChanges.Add(entity);changeIds.Add(item.Id);changeRestored++;}
            results.Add(new("planChanges","Cambios del plan",changeRestored,changePresent,changeConflicts));

            var currentShares=await db.ReportShares.Where(x=>x.AthleteProfileId==profileId).ToArrayAsync(ct);var shareIds=currentShares.Select(x=>x.Id).ToHashSet();var shareMap=new Dictionary<Guid,Guid>();var shareRestored=0;var sharePresent=0;var shareConflicts=0;
            foreach(var item in sharedReports){if(shareIds.Contains(item.Id)){shareMap[item.Id]=item.Id;sharePresent++;continue;}if(item.Snapshot.ValueKind!=JsonValueKind.Object){shareConflicts++;continue;}var archivedHash=Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();var entity=ReportShare.RestoreHistorical(profileId,archivedHash,item.Snapshot.GetRawText(),item.From,item.To,item.ExpiresAt,item.ConsentGrantedAt,item.RevokedAt,item.IncludeKnee,item.IncludeLearning,item.RecipientLabel);var restoredUpdatedAt=item.RevokedAt is null?entity.RevokedAt:item.UpdatedAt;entity.RestoreMetadata(item.Id,item.CreatedAt,restoredUpdatedAt,false);db.ReportShares.Add(entity);shareIds.Add(item.Id);shareMap[item.Id]=item.Id;shareRestored++;}
            results.Add(new("sharedReports","Informes compartidos",shareRestored,sharePresent,shareConflicts));

            var currentFeedback=await db.ReportFeedback.Where(x=>shareIds.Contains(x.ReportShareId)).ToArrayAsync(ct);var feedbackIds=currentFeedback.Select(x=>x.Id).ToHashSet();var feedbackKeys=currentFeedback.Select(x=>$"{x.ReportShareId:N}\u001f{x.CreatedAt:O}\u001f{x.AuthorName}\u001f{x.Message}").ToHashSet(StringComparer.OrdinalIgnoreCase);var feedbackRestored=0;var feedbackPresent=0;var feedbackConflicts=0;
            foreach(var item in professionalFeedback){if(feedbackIds.Contains(item.Id)){feedbackPresent++;continue;}if(!shareMap.TryGetValue(item.ReportShareId,out var shareId)){feedbackConflicts++;continue;}var key=$"{shareId:N}\u001f{item.CreatedAt:O}\u001f{item.AuthorName}\u001f{item.Message}";if(!feedbackKeys.Add(key)){feedbackConflicts++;continue;}var entity=new ReportFeedback(shareId,item.AuthorName,item.Kind,item.Section,item.Message);entity.RestoreReview(item.Status,item.DecisionNote,item.ReviewedAt);entity.RestoreMetadata(item.Id,item.CreatedAt,item.UpdatedAt,item.IsActive);db.ReportFeedback.Add(entity);feedbackIds.Add(item.Id);feedbackRestored++;}
            results.Add(new("professionalFeedback","Aportes profesionales",feedbackRestored,feedbackPresent,feedbackConflicts));

            var totalRestored=results.Sum(x=>x.Restored);var totalPresent=results.Sum(x=>x.AlreadyPresent);var totalConflicts=results.Sum(x=>x.Conflicts);
            db.DataTransferOperations.Add(new DataTransferOperation(profileId,"restore",actualSha,null,safetyBackupSha256,totalRestored,totalPresent,totalConflicts));
            await db.SaveChangesAsync(ct);await transaction.CommitAsync(ct);
            var supported=results.Select(x=>x.Key).ToHashSet();var deferred=preview.Modules.Where(x=>x.Missing>0&&!supported.Contains(x.Key)).Select(x=>x.Label).ToArray();
            return new(DateTimeOffset.UtcNow,actualSha,safetyBackupSha256,totalRestored,totalPresent,totalConflicts,results,deferred);
        }
        catch{await transaction.RollbackAsync(ct);throw;}
    }

    private static T[]Read<T>(JsonElement root,string property){if(!root.TryGetProperty(property,out var value)||value.ValueKind!=JsonValueKind.Array)throw new ArgumentException($"Falta la colección {property}.");try{return JsonSerializer.Deserialize<T[]>(value.GetRawText(),RestoreJsonOptions)??[];}catch(JsonException){throw new ArgumentException($"La colección {property} contiene datos incompatibles.");}}
    private static T[]ReadOptional<T>(JsonElement root,string property)=>root.TryGetProperty(property,out _)?Read<T>(root,property):[];
    private static readonly JsonSerializerOptions RestoreJsonOptions=CreateRestoreOptions();
    private static JsonSerializerOptions CreateRestoreOptions(){var options=new JsonSerializerOptions(JsonSerializerDefaults.Web);options.Converters.Add(new JsonStringEnumConverter());return options;}
    private sealed record ProfileFactBackup(Guid Id,DateTimeOffset CreatedAt,DateTimeOffset?UpdatedAt,bool IsActive,string Category,string Label,string?Value,ProfileFactStatus Status,string SourceTitle,string?Notes);
    private sealed record CheckInBackup(Guid Id,DateTimeOffset CreatedAt,DateTimeOffset?UpdatedAt,bool IsActive,DateOnly Date,int?SleepMinutes,int SleepQuality,int Energy,int Fatigue,int Stress,string?PainLocation,string?PainSide,int?PainIntensity,string?Stiffness,string?Swelling,bool Instability,bool Locking,int ExpectedWorkLoad,decimal?PlannedCyclingKm,string?PlannedActivity,string?Notes);
    private sealed record ActivityBackup(Guid Id,DateTimeOffset CreatedAt,DateTimeOffset?UpdatedAt,bool IsActive,DateOnly Date,string ActivityType,int DurationMinutes,int Rpe,decimal?DistanceKm,string?Notes,int?PlannedDurationMinutes,string?PlannedSource,string?WorkDemands,int?BreakMinutes,string?UnusualConditions);
    private sealed record DecisionBackup(Guid Id,DateTimeOffset CreatedAt,DateTimeOffset?UpdatedAt,bool IsActive,DateOnly Date,string Decision,string Reason,string ContextStatus,int PlannedLoadSnapshot,int Version);
    private sealed record ScheduleBackup(Guid Id,DateTimeOffset CreatedAt,DateTimeOffset?UpdatedAt,bool IsActive,int DayOfWeek,string Name,string Category,string TimeWindow,TimeOnly?StartTime,TimeOnly?EndTime,DateOnly EffectiveFrom,DateOnly?EffectiveTo,string?Notes);
    private sealed record KneeBackup(Guid Id,DateTimeOffset CreatedAt,DateTimeOffset?UpdatedAt,bool IsActive,DateTimeOffset RecordedAt,string Context,string Side,int PainNow,int PainBest24H,int PainWorst24H,string Swelling,bool Instability,bool Locking,bool FullExtension,int WalkingCapacity,int StairsCapacity,int SquatCapacity,string?Notes);
    private sealed record MetricBackup(Guid Id,DateTimeOffset CreatedAt,DateTimeOffset?UpdatedAt,bool IsActive,string Name,string Category,string Unit,MetricDirection Direction,decimal?TargetValue,DateOnly?TargetDate,string?Protocol,string?SourceTitle,string?SourceUrl);
    private sealed record MeasurementBackup(Guid Id,DateTimeOffset CreatedAt,DateTimeOffset?UpdatedAt,bool IsActive,Guid MetricDefinitionId,DateOnly Date,decimal Value,string?Conditions,string?Notes);
    private sealed record GoalBackup(Guid Id,DateTimeOffset CreatedAt,DateTimeOffset?UpdatedAt,bool IsActive,Guid?MetricDefinitionId,string Title,string Category,decimal?BaselineValue,decimal?TargetValue,string?Unit,DateOnly StartDate,DateOnly?TargetDate,string Status,string?Rationale,int Version);
    private sealed record CycleBackup(Guid Id,DateTimeOffset CreatedAt,DateTimeOffset?UpdatedAt,bool IsActive,string Name,DateOnly StartDate,DateOnly EndDate,string Focus,int PlannedSessionsPerWeek,string Status,string?Notes,int Version);
    private sealed record SessionBackup(Guid Id,DateTimeOffset CreatedAt,DateTimeOffset?UpdatedAt,bool IsActive,Guid?PersonalGoalId,Guid?TrainingCycleId,int Version,DateOnly Date,string Name,string ActivityType,TimeOnly?PlannedStartTime,int?PlannedDurationMinutes,int?TargetRpe,string?Goal,string?Notes,TrainingSessionStatus Status,int?ActualDurationMinutes,int?SessionRpe,string?CompletionNotes,DateTimeOffset?CompletedAt);
    private sealed record ExerciseBackup(Guid Id,DateTimeOffset CreatedAt,DateTimeOffset?UpdatedAt,bool IsActive,Guid TrainingSessionId,int Order,string Name,string?Category,int PlannedSets,string PlannedRepetitions,decimal?PlannedLoadKg,int?RestSeconds,string?Notes,int?CompletedSets,string?ActualRepetitions,decimal?ActualLoadKg,int?ExerciseRpe,bool IsCompleted);
    private sealed record FollowUpBackup(Guid Id,DateTimeOffset CreatedAt,DateTimeOffset?UpdatedAt,bool IsActive,Guid TrainingSessionId,DateTimeOffset RecordedAt,int Recovery,int?PainIntensity,string?PainLocation,string?Stiffness,string?Swelling,bool Instability,bool Locking,string?Notes);
    private sealed record LearningBackup(Guid Id,DateTimeOffset CreatedAt,DateTimeOffset?UpdatedAt,bool IsActive,DateOnly Date,string Title,string Category,string Observation,string?Interpretation,string?NextAction,DateOnly?ReviewDueOn,string Confidence,string Status,DateOnly?ReviewedOn,string?FollowUpOutcome,string?FollowUpObservation,Guid?TrainingSessionId,Guid?PersonalGoalId,Guid?TrainingCycleId,int Version);
    private sealed record PlanChangeBackup(Guid Id,DateTimeOffset CreatedAt,DateTimeOffset?UpdatedAt,bool IsActive,string EntityType,Guid EntityId,int Version,string Reason,string Summary);
    private sealed record SharedReportBackup(Guid Id,DateTimeOffset CreatedAt,DateTimeOffset?UpdatedAt,bool IsActive,DateOnly From,DateOnly To,DateTimeOffset ExpiresAt,DateTimeOffset ConsentGrantedAt,DateTimeOffset?RevokedAt,bool IncludeKnee,bool IncludeLearning,string?RecipientLabel,JsonElement Snapshot);
    private sealed record FeedbackBackup(Guid Id,DateTimeOffset CreatedAt,DateTimeOffset?UpdatedAt,bool IsActive,Guid ReportShareId,string AuthorName,string Kind,string Section,string Message,string Status,string?DecisionNote,DateTimeOffset?ReviewedAt);

    private static string? Text(JsonElement root,string name)=>root.TryGetProperty(name,out var value)&&value.ValueKind==JsonValueKind.String?value.GetString():null;
    private static HashSet<Guid> BackupIds(JsonElement root,string key,List<string>warnings)
    {
        if(!root.TryGetProperty(key,out var array)||array.ValueKind!=JsonValueKind.Array){warnings.Add($"Falta la colección {key}.");return[];}
        var ids=new HashSet<Guid>();var invalid=0;var duplicates=0;
        foreach(var item in array.EnumerateArray()){if(item.ValueKind!=JsonValueKind.Object||!item.TryGetProperty("id",out var id)||!id.TryGetGuid(out var parsed)){invalid++;continue;}if(!ids.Add(parsed))duplicates++;}
        if(invalid>0)warnings.Add($"{key}: {invalid} registros no tienen un identificador válido.");if(duplicates>0)warnings.Add($"{key}: {duplicates} identificadores están duplicados.");return ids;
    }

    private async Task<Guid> ProfileId(Guid userId,CancellationToken ct)=>await db.AthleteProfiles.Where(x=>x.UserId==userId).Select(x=>(Guid?)x.Id).SingleOrDefaultAsync(ct)??throw new InvalidOperationException("Primero debe inicializarse el perfil Atlas.");
}
