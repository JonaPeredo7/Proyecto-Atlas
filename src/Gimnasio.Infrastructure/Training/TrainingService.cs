using Gimnasio.Application.Training;
using Gimnasio.Domain.Entities;
using Gimnasio.Domain.Enums;
using Gimnasio.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Gimnasio.Infrastructure.Training;

internal sealed class TrainingService(GimnasioDbContext dbContext) : ITrainingService
{
    public async Task<TrainingCalendarDto> GetCalendarAsync(Guid userId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        if (to < from) throw new ArgumentException("La fecha final no puede ser anterior a la inicial.");
        if (to.DayNumber - from.DayNumber > 62) throw new ArgumentException("La agenda admite períodos de hasta 63 días.");
        var profile = await Profile(userId, cancellationToken);
        var sessions = await dbContext.TrainingSessions.AsNoTracking()
            .Where(x => x.AthleteProfileId == profile.Id && x.IsActive && x.Date >= from && x.Date <= to)
            .OrderBy(x => x.Date).ThenBy(x => x.CreatedAt).ToListAsync(cancellationToken);
        var ids = sessions.Select(x => x.Id).ToArray();
        var followUps = await dbContext.TrainingFollowUps.AsNoTracking().Where(x => ids.Contains(x.TrainingSessionId)).Select(x => x.TrainingSessionId).ToListAsync(cancellationToken);
        var checkIns = await dbContext.DailyCheckIns.AsNoTracking()
            .Where(x => x.AthleteProfileId == profile.Id && x.Date >= from && x.Date <= to)
            .ToDictionaryAsync(x => x.Date, cancellationToken);
        var dailyActivities = await dbContext.DailyActivities.AsNoTracking()
            .Where(x => x.AthleteProfileId == profile.Id && x.IsActive && x.Date >= from && x.Date <= to)
            .OrderBy(x => x.Date).ToListAsync(cancellationToken);
        var learningReviews = await dbContext.LearningEntries.AsNoTracking()
            .Where(x => x.AthleteProfileId == profile.Id && x.IsActive && x.ReviewDueOn.HasValue && x.ReviewDueOn.Value >= from && x.ReviewDueOn.Value <= to)
            .OrderBy(x => x.ReviewDueOn).ThenBy(x => x.CreatedAt).ToListAsync(cancellationToken);
        var schedule = await dbContext.RecurringScheduleBlocks.AsNoTracking()
            .Where(x => x.AthleteProfileId == profile.Id && x.IsActive && x.EffectiveFrom <= to && (!x.EffectiveTo.HasValue || x.EffectiveTo >= from))
            .OrderBy(x => x.DayOfWeek).ThenBy(x => x.StartTime).ToListAsync(cancellationToken);
        var today = DateOnly.FromDateTime(DateTime.Today);
        var days = Enumerable.Range(0, to.DayNumber - from.DayNumber + 1).Select(offset => from.AddDays(offset)).Select(date =>
        {
            var items = sessions.Where(x => x.Date == date).ToArray();
            var activities = dailyActivities.Where(x => x.Date == date).ToArray();
            var reviews = learningReviews.Where(x => x.ReviewDueOn == date)
                .Select(x => new CalendarLearningReviewDto(x.Id, x.Title, x.NextAction ?? "Sin próxima acción", x.Status, x.ReviewDueOn!.Value, x.ReviewedOn, x.Status == "open" && x.ReviewDueOn <= today)).ToArray();
            var blocks = schedule.Where(x => x.DayOfWeek == IsoDay(date) && x.EffectiveFrom <= date && (!x.EffectiveTo.HasValue || x.EffectiveTo >= date)).Select(Map).ToArray();
            var exactBlocks = blocks.Where(x => x.TimeWindow == "exact" && x.StartTime.HasValue && x.EndTime.HasValue).ToArray();
            var sessionIntervals = items.Where(x => x.Status != TrainingSessionStatus.Cancelled && x.PlannedStartTime.HasValue && PlannedEnd(x).HasValue).ToArray();
            var recurringConflict = exactBlocks.SelectMany((item, index) => exactBlocks.Skip(index + 1).Where(other => item.StartTime < other.EndTime && other.StartTime < item.EndTime)).Any();
            var sessionConflict = sessionIntervals.SelectMany((item, index) => sessionIntervals.Skip(index + 1).Where(other => item.PlannedStartTime < PlannedEnd(other) && other.PlannedStartTime < PlannedEnd(item))).Any();
            var commitmentConflict = sessionIntervals.Any(item => exactBlocks.Any(block => !SameActivity(item, block) && item.PlannedStartTime < block.EndTime && block.StartTime < PlannedEnd(item)));
            var conflict = recurringConflict || sessionConflict || commitmentConflict;
            checkIns.TryGetValue(date, out var check);
            var mapped = items.Select(x => new CalendarSessionDto(x.Id, x.Name, x.ActivityType, x.Status.ToString(), x.PlannedStartTime, PlannedEnd(x), x.PlannedDurationMinutes, x.TargetRpe, x.ActualDurationMinutes, x.SessionRpe,
                x.Status == TrainingSessionStatus.Completed && x.CompletedAt <= DateTimeOffset.UtcNow.AddHours(-20) && !followUps.Contains(x.Id))).ToArray();
            var trainingLoad = items.Sum(x => (x.ActualDurationMinutes ?? 0) * (x.SessionRpe ?? 0));
            var externalLoad = activities.Sum(x => x.InternalLoad);
            return new TrainingCalendarDayDto(date, check is not null, check?.Energy, check?.Fatigue, check?.PainIntensity,
                items.Sum(x => x.PlannedDurationMinutes ?? 0), items.Sum(x => x.ActualDurationMinutes ?? 0), trainingLoad, externalLoad, trainingLoad + externalLoad, activities.Length, mapped, reviews, blocks, conflict);
        }).ToArray();
        var totalTrainingLoad = sessions.Sum(x => (x.ActualDurationMinutes ?? 0) * (x.SessionRpe ?? 0));
        var totalExternalLoad = dailyActivities.Sum(x => x.InternalLoad);
        return new(from, to, days, new CalendarSummaryDto(sessions.Count(x => x.Status != TrainingSessionStatus.Cancelled), sessions.Count(x => x.Status == TrainingSessionStatus.Completed),
            sessions.Sum(x => x.PlannedDurationMinutes ?? 0), sessions.Sum(x => x.ActualDurationMinutes ?? 0), totalTrainingLoad, totalExternalLoad, totalTrainingLoad + totalExternalLoad, checkIns.Count,
            learningReviews.Count, learningReviews.Count(x => x.Status == "open" && x.ReviewDueOn <= today)));
    }

    public async Task<TrainingOverviewDto> GetOverviewAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var profile = await Profile(userId, cancellationToken);
        var from = DateOnly.FromDateTime(DateTime.Today.AddDays(-14));
        var sessions = await dbContext.TrainingSessions.AsNoTracking()
            .Where(x => x.AthleteProfileId == profile.Id && x.Date >= from)
            .OrderByDescending(x => x.Date)
            .ToListAsync(cancellationToken);
        var ids = sessions.Select(x => x.Id).ToArray();
        var exercises = await dbContext.TrainingExercises.AsNoTracking().Where(x => ids.Contains(x.TrainingSessionId)).OrderBy(x => x.Order).ToListAsync(cancellationToken);
        var followUps = await dbContext.TrainingFollowUps.AsNoTracking().Where(x => ids.Contains(x.TrainingSessionId)).ToDictionaryAsync(x => x.TrainingSessionId, cancellationToken);
        var goals = await dbContext.PersonalGoals.AsNoTracking().Where(x => x.AthleteProfileId == profile.Id && x.IsActive).ToListAsync(cancellationToken);
        var cycles = await dbContext.TrainingCycles.AsNoTracking().Where(x => x.AthleteProfileId == profile.Id && x.IsActive).ToListAsync(cancellationToken);
        var dtos = sessions.Select(x => Map(x, exercises.Where(e => e.TrainingSessionId == x.Id), followUps.GetValueOrDefault(x.Id), goals.FirstOrDefault(g=>g.Id==x.PersonalGoalId)?.Title, cycles.FirstOrDefault(c=>c.Id==x.TrainingCycleId)?.Name)).ToArray();
        var today = DateOnly.FromDateTime(DateTime.Today);
        var week = today.AddDays(-6);
        var completed = dtos.Where(x => x.Status == nameof(TrainingSessionStatus.Completed) && x.Date >= week).ToArray();
        return new TrainingOverviewDto(
            dtos.Where(x => x.Date >= today && x.Status is nameof(TrainingSessionStatus.Planned) or nameof(TrainingSessionStatus.InProgress)).OrderBy(x => x.Date).ToArray(),
            dtos.Where(x => x.Status == nameof(TrainingSessionStatus.Completed)).Take(10).ToArray(),
            dtos.Where(x => x.FollowUpDue && x.FollowUp is null).ToArray(),
            new TrainingMetricsDto(completed.Length, completed.Sum(x => x.ActualDurationMinutes ?? 0), completed.Sum(x => x.InternalLoad ?? 0), dtos.Count(x => x.FollowUpDue && x.FollowUp is null)),
            goals.Where(x=>x.Status=="active").Select(x=>new TrainingPlanOptionDto(x.Id,x.Title,$"{x.Category} · v{x.Version}")).ToArray(),
            cycles.Where(x=>x.Status is "active" or "planned").Select(x=>new TrainingPlanOptionDto(x.Id,x.Name,$"{x.Focus} · v{x.Version}")).ToArray());
    }

    public async Task<TrainingSessionDto> CreateSessionAsync(Guid userId, SaveTrainingSessionRequest request, CancellationToken cancellationToken = default)
    {
        var profile = await Profile(userId, cancellationToken);
        var session = new TrainingSession(profile.Id, request.Date, request.Name, request.ActivityType);
        await ValidateLinks(profile.Id, request.PersonalGoalId, request.TrainingCycleId, cancellationToken);
        session.Configure(request.PlannedDurationMinutes, request.TargetRpe, request.Goal, request.Notes);
        session.Schedule(request.PlannedStartTime);
        session.LinkPlan(request.PersonalGoalId, request.TrainingCycleId, false);
        dbContext.TrainingSessions.Add(session);
        dbContext.PlanChanges.Add(new PlanChange(profile.Id,"session",session.Id,session.Version,request.ChangeReason,$"{session.Name} · {session.Date}"));
        await dbContext.SaveChangesAsync(cancellationToken);
        return await MapLoaded(session, cancellationToken);
    }

    public async Task<TrainingSessionDto?> UpdateSessionAsync(Guid userId, Guid sessionId, SaveTrainingSessionRequest request, CancellationToken cancellationToken = default)
    {
        var session=await OwnedSession(userId,sessionId,cancellationToken);if(session is null)return null;
        await ValidateLinks(session.AthleteProfileId,request.PersonalGoalId,request.TrainingCycleId,cancellationToken);
        session.Revise(request.Date,request.Name,request.ActivityType);session.Configure(request.PlannedDurationMinutes,request.TargetRpe,request.Goal,request.Notes);session.Schedule(request.PlannedStartTime);session.LinkPlan(request.PersonalGoalId,request.TrainingCycleId,true);
        dbContext.PlanChanges.Add(new PlanChange(session.AthleteProfileId,"session",session.Id,session.Version,request.ChangeReason,$"{session.Name} · prescripción actualizada"));
        await dbContext.SaveChangesAsync(cancellationToken);return await MapLoaded(session,cancellationToken);
    }

    public async Task<TrainingSessionDto?> DuplicateSessionAsync(Guid userId, Guid sessionId, DuplicateTrainingSessionRequest request, CancellationToken cancellationToken = default)
    {
        var source = await OwnedSession(userId, sessionId, cancellationToken);
        if (source is null) return null;
        if (string.IsNullOrWhiteSpace(request.ChangeReason)) throw new ArgumentException("Debe indicarse el motivo de la copia.");

        var sourceExercises = await dbContext.TrainingExercises.AsNoTracking()
            .Where(x => x.TrainingSessionId == source.Id)
            .OrderBy(x => x.Order)
            .ToListAsync(cancellationToken);
        var copy = CreatePrescriptionCopy(source, sourceExercises, request.Date, request.Name);

        dbContext.PlanChanges.Add(new PlanChange(copy.AthleteProfileId, "session", copy.Id, copy.Version, request.ChangeReason, $"{copy.Name} · duplicada desde {source.Name} · {copy.Date}"));
        await dbContext.SaveChangesAsync(cancellationToken);
        return await MapLoaded(copy, cancellationToken);
    }

    public async Task<CopyTrainingWeekResultDto> CopyWeekAsync(Guid userId, CopyTrainingWeekRequest request, CancellationToken cancellationToken = default)
    {
        if (request.SourceWeekStart == request.TargetWeekStart) throw new ArgumentException("La semana de destino debe ser diferente de la semana de origen.");
        if (string.IsNullOrWhiteSpace(request.ChangeReason)) throw new ArgumentException("Debe indicarse el motivo de la copia semanal.");
        var profile = await Profile(userId, cancellationToken);
        var sourceEnd = request.SourceWeekStart.AddDays(6);
        var targetEnd = request.TargetWeekStart.AddDays(6);
        var sources = await dbContext.TrainingSessions.AsNoTracking()
            .Where(x => x.AthleteProfileId == profile.Id && x.IsActive && x.Status != TrainingSessionStatus.Cancelled && x.Date >= request.SourceWeekStart && x.Date <= sourceEnd)
            .OrderBy(x => x.Date).ThenBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
        if (sources.Count == 0) throw new InvalidOperationException("La semana de origen no contiene sesiones para copiar.");

        var sourceIds = sources.Select(x => x.Id).ToArray();
        var exercises = await dbContext.TrainingExercises.AsNoTracking()
            .Where(x => sourceIds.Contains(x.TrainingSessionId) && x.IsActive)
            .OrderBy(x => x.Order)
            .ToListAsync(cancellationToken);
        var existingTargets = await dbContext.TrainingSessions.AsNoTracking()
            .Where(x => x.AthleteProfileId == profile.Id && x.IsActive && x.Date >= request.TargetWeekStart && x.Date <= targetEnd)
            .ToListAsync(cancellationToken);
        var copies = new List<TrainingSession>();
        var skipped = 0;

        foreach (var source in sources)
        {
            var targetDate = request.TargetWeekStart.AddDays(source.Date.DayNumber - request.SourceWeekStart.DayNumber);
            if (existingTargets.Any(x => x.Date == targetDate && x.Name.Equals(source.Name, StringComparison.OrdinalIgnoreCase) && x.ActivityType.Equals(source.ActivityType, StringComparison.OrdinalIgnoreCase)))
            {
                skipped++;
                continue;
            }

            var copy = CreatePrescriptionCopy(source, exercises.Where(x => x.TrainingSessionId == source.Id), targetDate);
            copies.Add(copy);
            dbContext.PlanChanges.Add(new PlanChange(profile.Id, "session", copy.Id, copy.Version, request.ChangeReason, $"{copy.Name} · semana copiada desde {source.Date} hacia {targetDate}"));
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        var mapped = new List<TrainingSessionDto>();
        foreach (var copy in copies) mapped.Add(await MapLoaded(copy, cancellationToken));
        return new CopyTrainingWeekResultDto(copies.Count, skipped, mapped);
    }

    public async Task<IReadOnlyCollection<ScheduleBlockDto>> GetScheduleAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var profile = await Profile(userId, cancellationToken);
        return await dbContext.RecurringScheduleBlocks.AsNoTracking().Where(x => x.AthleteProfileId == profile.Id && x.IsActive)
            .OrderBy(x => x.DayOfWeek).ThenBy(x => x.StartTime).Select(x => new ScheduleBlockDto(x.Id, x.DayOfWeek, x.Name, x.Category, x.TimeWindow, x.StartTime, x.EndTime, x.EffectiveFrom, x.EffectiveTo, x.Notes)).ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<ScheduleBlockDto>> AddScheduleBlocksAsync(Guid userId, SaveScheduleBlockRequest request, CancellationToken cancellationToken = default)
    {
        var profile = await Profile(userId, cancellationToken);
        var days = request.DaysOfWeek.Distinct().OrderBy(x => x).ToArray();
        if (days.Length == 0 || days.Any(x => x is < 1 or > 7)) throw new ArgumentException("Seleccioná al menos un día válido.");
        var existing = await dbContext.RecurringScheduleBlocks.Where(x => x.AthleteProfileId == profile.Id && x.IsActive && x.Name == request.Name && x.EffectiveFrom == request.EffectiveFrom && days.Contains(x.DayOfWeek)).ToListAsync(cancellationToken);
        var result = new List<RecurringScheduleBlock>(existing);
        foreach (var block in existing) block.Configure(request.TimeWindow, request.StartTime, request.EndTime, request.EffectiveFrom, request.EffectiveTo, request.Notes);
        foreach (var day in days.Where(day => existing.All(x => x.DayOfWeek != day)))
        {
            var block = new RecurringScheduleBlock(profile.Id, day, request.Name, request.Category);
            block.Configure(request.TimeWindow, request.StartTime, request.EndTime, request.EffectiveFrom, request.EffectiveTo, request.Notes);
            dbContext.RecurringScheduleBlocks.Add(block); result.Add(block);
        }
        await dbContext.SaveChangesAsync(cancellationToken);
        return result.OrderBy(x => x.DayOfWeek).Select(Map).ToArray();
    }

    public async Task<bool> RemoveScheduleBlockAsync(Guid userId, Guid blockId, CancellationToken cancellationToken = default)
    {
        var block = await dbContext.RecurringScheduleBlocks.SingleOrDefaultAsync(x => x.Id == blockId && x.IsActive && dbContext.AthleteProfiles.Any(p => p.Id == x.AthleteProfileId && p.UserId == userId), cancellationToken);
        if (block is null) return false;
        block.Deactivate(); await dbContext.SaveChangesAsync(cancellationToken); return true;
    }

    public async Task<TrainingSessionDto?> AddExerciseAsync(Guid userId, Guid sessionId, AddTrainingExerciseRequest request, CancellationToken cancellationToken = default)
    {
        var session = await OwnedSession(userId, sessionId, cancellationToken);
        if (session is null) return null;
        if (session.Status is TrainingSessionStatus.Completed or TrainingSessionStatus.Cancelled) throw new InvalidOperationException("La sesión ya está cerrada.");
        var order = await dbContext.TrainingExercises.CountAsync(x => x.TrainingSessionId == sessionId, cancellationToken) + 1;
        var exercise = new TrainingExercise(sessionId, order, request.Name);
        exercise.Prescribe(request.Category, request.Sets, request.Repetitions, request.LoadKg, request.RestSeconds, request.Notes);
        dbContext.TrainingExercises.Add(exercise);
        session.MarkPrescriptionChange();
        dbContext.PlanChanges.Add(new PlanChange(session.AthleteProfileId,"exercise",exercise.Id,session.Version,request.ChangeReason,$"{session.Name} · agregado {exercise.Name}"));
        await dbContext.SaveChangesAsync(cancellationToken);
        return await MapLoaded(session, cancellationToken);
    }

    public async Task<TrainingSessionDto?> UpdateExerciseAsync(Guid userId, Guid sessionId, Guid exerciseId, AddTrainingExerciseRequest request, CancellationToken cancellationToken = default)
    {
        var session=await OwnedSession(userId,sessionId,cancellationToken);if(session is null)return null;
        var exercise=await dbContext.TrainingExercises.SingleOrDefaultAsync(x=>x.Id==exerciseId&&x.TrainingSessionId==sessionId,cancellationToken)??throw new InvalidOperationException("El ejercicio no pertenece a la sesión.");
        if(session.Status!=TrainingSessionStatus.Planned)throw new InvalidOperationException("La prescripción sólo puede corregirse antes de iniciar la sesión.");
        exercise.Rename(request.Name);exercise.Prescribe(request.Category,request.Sets,request.Repetitions,request.LoadKg,request.RestSeconds,request.Notes);session.MarkPrescriptionChange();
        dbContext.PlanChanges.Add(new PlanChange(session.AthleteProfileId,"exercise",exercise.Id,session.Version,request.ChangeReason,$"{session.Name} · actualizado {exercise.Name}"));
        await dbContext.SaveChangesAsync(cancellationToken);return await MapLoaded(session,cancellationToken);
    }

    public async Task<TrainingSessionDto?> StartSessionAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken = default)
    {
        var session = await OwnedSession(userId, sessionId, cancellationToken);
        if (session is null) return null;
        session.Start();
        await dbContext.SaveChangesAsync(cancellationToken);
        return await MapLoaded(session, cancellationToken);
    }

    public async Task<TrainingSessionDto?> RecordExerciseAsync(Guid userId, Guid sessionId, Guid exerciseId, RecordTrainingExerciseRequest request, CancellationToken cancellationToken = default)
    {
        var session = await OwnedSession(userId, sessionId, cancellationToken);
        if (session is null) return null;
        var exercise = await dbContext.TrainingExercises.SingleOrDefaultAsync(x => x.Id == exerciseId && x.TrainingSessionId == sessionId, cancellationToken)
            ?? throw new InvalidOperationException("El ejercicio no pertenece a la sesión.");
        exercise.Record(request.CompletedSets, request.ActualRepetitions, request.ActualLoadKg, request.ExerciseRpe);
        await dbContext.SaveChangesAsync(cancellationToken);
        return await MapLoaded(session, cancellationToken);
    }

    public async Task<TrainingSessionDto?> CompleteSessionAsync(Guid userId, Guid sessionId, CompleteTrainingSessionRequest request, CancellationToken cancellationToken = default)
    {
        var session = await OwnedSession(userId, sessionId, cancellationToken);
        if (session is null) return null;
        if (session.Status == TrainingSessionStatus.Completed) return await MapLoaded(session, cancellationToken);
        session.Complete(request.ActualDurationMinutes, request.SessionRpe, request.CompletionNotes);
        await dbContext.SaveChangesAsync(cancellationToken);
        return await MapLoaded(session, cancellationToken);
    }

    public async Task<TrainingFollowUpDto?> SaveFollowUpAsync(Guid userId, Guid sessionId, SaveTrainingFollowUpRequest request, CancellationToken cancellationToken = default)
    {
        var session = await OwnedSession(userId, sessionId, cancellationToken);
        if (session is null) return null;
        if (session.Status != TrainingSessionStatus.Completed) throw new InvalidOperationException("Primero debe completarse la sesión.");
        var followUp = await dbContext.TrainingFollowUps.SingleOrDefaultAsync(x => x.TrainingSessionId == sessionId, cancellationToken);
        if (followUp is null) { followUp = new TrainingFollowUp(sessionId); dbContext.TrainingFollowUps.Add(followUp); }
        followUp.Record(request.Recovery, request.PainIntensity, request.PainLocation, request.Stiffness, request.Swelling, request.Instability, request.Locking, request.Notes);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(followUp);
    }

    private async Task<AthleteProfile> Profile(Guid userId, CancellationToken ct) =>
        await dbContext.AthleteProfiles.SingleOrDefaultAsync(x => x.UserId == userId, ct)
        ?? throw new InvalidOperationException("Primero debe inicializarse el perfil Atlas desde la pantalla Hoy.");

    private async Task<TrainingSession?> OwnedSession(Guid userId, Guid sessionId, CancellationToken ct) =>
        await dbContext.TrainingSessions.SingleOrDefaultAsync(x => x.Id == sessionId && dbContext.AthleteProfiles.Any(p => p.Id == x.AthleteProfileId && p.UserId == userId), ct);

    private TrainingSession CreatePrescriptionCopy(TrainingSession source, IEnumerable<TrainingExercise> sourceExercises, DateOnly date, string? name = null)
    {
        var copy = new TrainingSession(source.AthleteProfileId, date, string.IsNullOrWhiteSpace(name) ? source.Name : name, source.ActivityType);
        copy.Configure(source.PlannedDurationMinutes, source.TargetRpe, source.Goal, source.Notes);
        copy.Schedule(source.PlannedStartTime);
        copy.LinkPlan(source.PersonalGoalId, source.TrainingCycleId, false);
        dbContext.TrainingSessions.Add(copy);
        foreach (var planned in sourceExercises)
        {
            var exercise = new TrainingExercise(copy.Id, planned.Order, planned.Name);
            exercise.Prescribe(planned.Category, planned.PlannedSets, planned.PlannedRepetitions, planned.PlannedLoadKg, planned.RestSeconds, planned.Notes);
            dbContext.TrainingExercises.Add(exercise);
        }
        return copy;
    }

    private async Task<TrainingSessionDto> MapLoaded(TrainingSession session, CancellationToken ct)
    {
        var exercises = await dbContext.TrainingExercises.AsNoTracking().Where(x => x.TrainingSessionId == session.Id).OrderBy(x => x.Order).ToListAsync(ct);
        var followUp = await dbContext.TrainingFollowUps.AsNoTracking().SingleOrDefaultAsync(x => x.TrainingSessionId == session.Id, ct);
        var goal= session.PersonalGoalId.HasValue ? await dbContext.PersonalGoals.AsNoTracking().Where(x=>x.Id==session.PersonalGoalId).Select(x=>x.Title).SingleOrDefaultAsync(ct):null;
        var cycle= session.TrainingCycleId.HasValue ? await dbContext.TrainingCycles.AsNoTracking().Where(x=>x.Id==session.TrainingCycleId).Select(x=>x.Name).SingleOrDefaultAsync(ct):null;
        return Map(session, exercises, followUp,goal,cycle);
    }

    private async Task ValidateLinks(Guid profileId,Guid? goalId,Guid? cycleId,CancellationToken ct){if(goalId.HasValue&&!await dbContext.PersonalGoals.AnyAsync(x=>x.Id==goalId&&x.AthleteProfileId==profileId&&x.IsActive,ct))throw new ArgumentException("El objetivo no pertenece al perfil.");if(cycleId.HasValue&&!await dbContext.TrainingCycles.AnyAsync(x=>x.Id==cycleId&&x.AthleteProfileId==profileId&&x.IsActive,ct))throw new ArgumentException("El ciclo no pertenece al perfil.");}

    private static TrainingSessionDto Map(TrainingSession s, IEnumerable<TrainingExercise> exercises, TrainingFollowUp? followUp,string? goalTitle=null,string? cycleName=null)
    {
        var due = s.Status == TrainingSessionStatus.Completed && s.CompletedAt.HasValue && s.CompletedAt.Value <= DateTimeOffset.UtcNow.AddHours(-20);
        return new(s.Id, s.Date, s.Name, s.ActivityType, s.PlannedStartTime, s.PlannedDurationMinutes, s.TargetRpe, s.Goal, s.Notes,s.PersonalGoalId,goalTitle,s.TrainingCycleId,cycleName,s.Version, s.Status.ToString(), s.ActualDurationMinutes, s.SessionRpe,
            s.ActualDurationMinutes * s.SessionRpe, s.CompletionNotes, s.CompletedAt, due, followUp is null ? null : Map(followUp), exercises.Select(Map).ToArray());
    }
    private static TrainingExerciseDto Map(TrainingExercise x) => new(x.Id, x.Order, x.Name, x.Category, x.PlannedSets, x.PlannedRepetitions, x.PlannedLoadKg, x.RestSeconds, x.Notes, x.CompletedSets, x.ActualRepetitions, x.ActualLoadKg, x.ExerciseRpe, x.IsCompleted);
    private static TrainingFollowUpDto Map(TrainingFollowUp x) => new(x.Id, x.RecordedAt, x.Recovery, x.PainIntensity, x.PainLocation, x.Stiffness, x.Swelling, x.Instability, x.Locking, x.Notes,
        x.Instability || x.Locking || x.PainIntensity >= 5 || x.Swelling is "moderada" or "alta");
    private static ScheduleBlockDto Map(RecurringScheduleBlock x) => new(x.Id, x.DayOfWeek, x.Name, x.Category, x.TimeWindow, x.StartTime, x.EndTime, x.EffectiveFrom, x.EffectiveTo, x.Notes);
    private static int IsoDay(DateOnly date) => date.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)date.DayOfWeek;
    private static TimeOnly? PlannedEnd(TrainingSession session) => session.PlannedStartTime.HasValue && session.PlannedDurationMinutes.HasValue ? session.PlannedStartTime.Value.AddMinutes(session.PlannedDurationMinutes.Value) : null;
    private static bool SameActivity(TrainingSession session, ScheduleBlockDto block) => block.Category == "training" && (session.ActivityType.Equals(block.Name, StringComparison.OrdinalIgnoreCase) || session.Name.Contains(block.Name, StringComparison.OrdinalIgnoreCase) || block.Name.Contains(session.ActivityType, StringComparison.OrdinalIgnoreCase));
}
