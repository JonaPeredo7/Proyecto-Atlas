using Gimnasio.Application.Atlas;
using Gimnasio.Domain.Entities;
using Gimnasio.Domain.Enums;
using Gimnasio.Domain.Services;
using Gimnasio.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Gimnasio.Infrastructure.Atlas;

internal sealed class AtlasService(GimnasioDbContext dbContext) : IAtlasService
{
    public async Task<AtlasOverviewDto> GetOverviewAsync(
        Guid userId,
        string displayName,
        CancellationToken cancellationToken = default)
    {
        var profile = await GetOrCreateProfileAsync(userId, displayName, cancellationToken);
        var facts = await dbContext.ProfileFacts.AsNoTracking()
            .Where(item => item.AthleteProfileId == profile.Id && item.IsActive)
            .OrderBy(item => item.Category)
            .ThenBy(item => item.Label)
            .ToListAsync(cancellationToken);
        var since = DateOnly.FromDateTime(DateTime.Today.AddDays(-6));
        var checkIns = await dbContext.DailyCheckIns.AsNoTracking()
            .Where(item => item.AthleteProfileId == profile.Id && item.Date >= since)
            .OrderByDescending(item => item.Date)
            .ToListAsync(cancellationToken);
        var evidence = await dbContext.EvidenceReferences.AsNoTracking()
            .Where(item => item.IsActive)
            .GroupBy(item => item.Status)
            .Select(group => new { Status = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.Status, item => item.Count, cancellationToken);

        var today = DateOnly.FromDateTime(DateTime.Today);
        var todaySessions = await dbContext.TrainingSessions.AsNoTracking()
            .Where(item => item.AthleteProfileId == profile.Id && item.Date == today && item.IsActive)
            .OrderBy(item => item.CreatedAt)
            .ToListAsync(cancellationToken);
        var todayActivities = await dbContext.DailyActivities.AsNoTracking()
            .Where(item => item.AthleteProfileId == profile.Id && item.Date == today && item.IsActive)
            .OrderBy(item => item.CreatedAt)
            .ToListAsync(cancellationToken);
        var isoDay = today.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)today.DayOfWeek;
        var todaySchedule = await dbContext.RecurringScheduleBlocks.AsNoTracking()
            .Where(item => item.AthleteProfileId == profile.Id && item.IsActive && item.DayOfWeek == isoDay && item.EffectiveFrom <= today && (!item.EffectiveTo.HasValue || item.EffectiveTo >= today))
            .OrderBy(item => item.StartTime).ToListAsync(cancellationToken);
        var latestKneeCheck = await dbContext.KneeChecks.AsNoTracking().Where(item => item.AthleteProfileId == profile.Id && item.IsActive).OrderByDescending(item => item.RecordedAt).FirstOrDefaultAsync(cancellationToken);
        var hasActiveGoal = await dbContext.PersonalGoals.AsNoTracking().AnyAsync(item => item.AthleteProfileId == profile.Id && item.IsActive && item.Status == "active", cancellationToken);
        var hasCurrentCycle = await dbContext.TrainingCycles.AsNoTracking().AnyAsync(item => item.AthleteProfileId == profile.Id && item.IsActive && item.Status == "active" && item.StartDate <= today && item.EndDate >= today, cancellationToken);
        var latestLearningDate = await dbContext.LearningEntries.AsNoTracking().Where(item=>item.AthleteProfileId==profile.Id&&item.IsActive).OrderByDescending(item=>item.Date).Select(item=>(DateOnly?)item.Date).FirstOrDefaultAsync(cancellationToken);
        var openLearningItems = await dbContext.LearningEntries.AsNoTracking()
            .Where(item => item.AthleteProfileId == profile.Id && item.IsActive && item.Status == "open" && item.NextAction != null)
            .OrderBy(item => item.Date).Select(item => new { item.Date, item.ReviewDueOn }).ToListAsync(cancellationToken);
        var dueLearningActions = openLearningItems.Count(item => item.ReviewDueOn.HasValue && item.ReviewDueOn.Value <= today);
        var completedSessionIds = await dbContext.TrainingSessions.AsNoTracking()
            .Where(item => item.AthleteProfileId == profile.Id && item.Status == TrainingSessionStatus.Completed &&
                item.CompletedAt != null && item.CompletedAt <= DateTimeOffset.UtcNow.AddHours(-20))
            .Select(item => item.Id)
            .ToListAsync(cancellationToken);
        var followedSessionIds = await dbContext.TrainingFollowUps.AsNoTracking()
            .Where(item => completedSessionIds.Contains(item.TrainingSessionId))
            .Select(item => item.TrainingSessionId)
            .ToListAsync(cancellationToken);
        var pendingFollowUps = completedSessionIds.Except(followedSessionIds).Count();
        var metrics = await dbContext.MetricDefinitions.AsNoTracking()
            .Where(item => item.AthleteProfileId == profile.Id && item.IsActive)
            .Select(item => new { item.Id, HasEntries = dbContext.MeasurementEntries.Any(entry => entry.MetricDefinitionId == item.Id && entry.IsActive) })
            .ToListAsync(cancellationToken);
        var todayCheckIn = checkIns.SingleOrDefault(item => item.Date == today);
        var state=DailyStateCalculator.Calculate(todayCheckIn is null?null:StateInput(todayCheckIn),checkIns.Where(x=>x.Date<today).Select(StateInput).ToArray());
        var planContext=DailyPlanContextCalculator.Calculate(state.Status,todaySessions.Select(x=>new DailyPlanInput(x.PlannedDurationMinutes,x.TargetRpe,x.Status)).ToArray());
        var todayDecision=await dbContext.DailyPlanDecisions.AsNoTracking().SingleOrDefaultAsync(x=>x.AthleteProfileId==profile.Id&&x.Date==today,cancellationToken);
        var actions = BuildDailyActions(todayCheckIn, todaySessions, todayActivities, latestKneeCheck, hasActiveGoal, hasCurrentCycle, latestLearningDate, openLearningItems.Count, dueLearningActions, openLearningItems.FirstOrDefault()?.Date ?? default, pendingFollowUps, metrics.Count, metrics.Count(item => !item.HasEntries),todayDecision is not null);
        var trainingLoadToday = todaySessions.Where(item => item.Status == TrainingSessionStatus.Completed).Sum(item => (item.ActualDurationMinutes ?? 0) * (item.SessionRpe ?? 0));
        var externalLoadToday = todayActivities.Sum(item => item.InternalLoad);
        var scheduledMinutes = todaySchedule.Where(x => x.StartTime.HasValue && x.EndTime.HasValue).Sum(x => (int)(x.EndTime!.Value - x.StartTime!.Value).TotalMinutes);
        var exactBlocks = todaySchedule.Where(x => x.StartTime.HasValue && x.EndTime.HasValue).ToArray();
        var recurringConflict = exactBlocks.SelectMany((item,index)=>exactBlocks.Skip(index+1).Where(other=>item.StartTime<other.EndTime&&other.StartTime<item.EndTime)).Any();
        var sessionIntervals = todaySessions.Where(x=>x.Status!=TrainingSessionStatus.Cancelled&&x.PlannedStartTime.HasValue&&x.PlannedDurationMinutes.HasValue).ToArray();
        var sessionConflict = sessionIntervals.SelectMany((item,index)=>sessionIntervals.Skip(index+1).Where(other=>item.PlannedStartTime<End(other)&&other.PlannedStartTime<End(item))).Any();
        var commitmentConflict = sessionIntervals.Any(item=>exactBlocks.Any(block=>!SameActivity(item,block)&&item.PlannedStartTime<block.EndTime&&block.StartTime<End(item)));
        int? daysToTarget = profile.TargetDate.HasValue ? profile.TargetDate.Value.DayNumber - today.DayNumber : null;
        return new AtlasOverviewDto(
            ToDto(profile),
            facts.Select(ToDto).ToArray(),
            todayCheckIn is null ? null : ToDto(todayCheckIn),
            checkIns.Select(ToDto).ToArray(),
            new EvidenceSummaryDto(
                evidence.GetValueOrDefault("draft"),
                evidence.GetValueOrDefault("in-review"),
                evidence.GetValueOrDefault("informative"),
                evidence.GetValueOrDefault("operational")),
            new DailyHubDto(
                actions,
                new DailyStateDto(state.Status,state.Label,state.Summary,state.BaselineDays,state.Factors.Select(x=>new DailyStateFactorDto(x.Key,x.Label,x.Current,x.Baseline,x.Delta,x.VisualThreshold,x.Unit,x.Trend,x.Basis)).ToArray(),"Comparación descriptiva con autorreportes recientes. Dos señales menos favorables activan 'Observar' como regla interna; no diagnostica, prescribe ni autoriza entrenamiento."),
                new DailyPlanContextDto(planContext.Status,planContext.Label,planContext.Summary,planContext.SessionCount,planContext.PlannedMinutes,planContext.PlannedLoad,planContext.IncompleteSessions,planContext.HasInProgress,"La carga prevista es una estimación. Sólo la carga realizada se incorpora al seguimiento."),
                todayDecision is null?null:ToDto(todayDecision),
                todaySessions.Select(item => new TodayTrainingDto(item.Id, item.Name, item.ActivityType, item.Status.ToString(), item.PlannedStartTime, item.PlannedDurationMinutes, item.TargetRpe)).ToArray(),
                todaySchedule.Select(item=>new TodayScheduleDto(item.Id,item.Name,item.Category,item.TimeWindow,item.StartTime,item.EndTime,item.Notes)).ToArray(),
                scheduledMinutes,
                recurringConflict||sessionConflict||commitmentConflict,
                pendingFollowUps,
                openLearningItems.Count,
                dueLearningActions,
                openLearningItems.Count == 0 ? null : openLearningItems[0].Date,
                metrics.Count,
                metrics.Count(item => !item.HasEntries),
                daysToTarget,
                todayActivities.Select(ToDto).ToArray(),
                trainingLoadToday,
                externalLoadToday,
                trainingLoadToday + externalLoadToday));
    }

    public async Task<DailyPlanDecisionDto> SaveDailyDecisionAsync(Guid userId,SaveDailyPlanDecisionRequest request,CancellationToken cancellationToken=default)
    {
        var profile=await GetOrCreateProfileAsync(userId,"Atleta",cancellationToken);var overview=await GetOverviewAsync(userId,profile.DisplayName,cancellationToken);var today=DateOnly.FromDateTime(DateTime.Today);var entity=await dbContext.DailyPlanDecisions.SingleOrDefaultAsync(x=>x.AthleteProfileId==profile.Id&&x.Date==today,cancellationToken);var update=entity is not null;entity??=new DailyPlanDecision(profile.Id,today);if(!update)dbContext.DailyPlanDecisions.Add(entity);entity.Record(request.Decision,request.Reason,overview.Hub.State.Status,overview.Hub.PlanContext.PlannedLoad,update);await dbContext.SaveChangesAsync(cancellationToken);return ToDto(entity);
    }

    public async Task<AtlasProfileDto> UpdateProfileAsync(
        Guid userId,
        UpdateAtlasProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        var profile = await GetOrCreateProfileAsync(userId, request.DisplayName, cancellationToken);
        profile.Update(
            request.DisplayName,
            request.HeightCm,
            request.ReferenceWeightKg,
            request.PrimaryGoal,
            request.TargetDate,
            request.DominantHand,
            request.DominantLeg,
            request.AffectedKnee);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToDto(profile);
    }

    public async Task<DailyCheckInDto> SaveCheckInAsync(
        Guid userId,
        SaveDailyCheckInRequest request,
        CancellationToken cancellationToken = default)
    {
        var profile = await dbContext.AthleteProfiles
            .SingleOrDefaultAsync(item => item.UserId == userId, cancellationToken)
            ?? throw new InvalidOperationException("Primero debe inicializarse el perfil Atlas.");
        var checkIn = await dbContext.DailyCheckIns
            .SingleOrDefaultAsync(item => item.AthleteProfileId == profile.Id && item.Date == request.Date, cancellationToken);
        if (checkIn is null)
        {
            checkIn = new DailyCheckIn(profile.Id, request.Date);
            dbContext.DailyCheckIns.Add(checkIn);
        }

        checkIn.Record(
            request.SleepMinutes,
            request.SleepQuality,
            request.Energy,
            request.Fatigue,
            request.Stress,
            request.PainLocation,
            request.PainSide,
            request.PainIntensity,
            request.Stiffness,
            request.Swelling,
            request.Instability,
            request.Locking,
            request.ExpectedWorkLoad,
            request.PlannedCyclingKm,
            request.PlannedActivity,
            request.Notes);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToDto(checkIn);
    }

    public async Task<DailyActivityDto> SaveDailyActivityAsync(
        Guid userId,
        Guid? activityId,
        SaveDailyActivityRequest request,
        CancellationToken cancellationToken = default)
    {
        var profile = await dbContext.AthleteProfiles.SingleOrDefaultAsync(item => item.UserId == userId, cancellationToken)
            ?? throw new InvalidOperationException("Primero debe inicializarse el perfil Atlas.");
        DailyActivity activity;
        if (activityId.HasValue)
        {
            activity = await dbContext.DailyActivities.SingleOrDefaultAsync(item => item.Id == activityId && item.AthleteProfileId == profile.Id && item.IsActive, cancellationToken)
                ?? throw new KeyNotFoundException("No se encontró la actividad.");
        }
        else
        {
            activity = new DailyActivity(profile.Id, request.Date, request.ActivityType);
            dbContext.DailyActivities.Add(activity);
        }
        activity.Record(request.Date, request.ActivityType, request.DurationMinutes, request.Rpe, request.DistanceKm, request.Notes);
        activity.AttachPlanSnapshot(request.PlannedDurationMinutes, request.PlannedSource);
        activity.AttachWorkContext(request.WorkDemands, request.BreakMinutes, request.UnusualConditions);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToDto(activity);
    }

    public async Task<bool> DeleteDailyActivityAsync(Guid userId, Guid activityId, CancellationToken cancellationToken = default)
    {
        var activity = await dbContext.DailyActivities.SingleOrDefaultAsync(item => item.Id == activityId && item.IsActive &&
            dbContext.AthleteProfiles.Any(profile => profile.Id == item.AthleteProfileId && profile.UserId == userId), cancellationToken);
        if (activity is null) return false;
        activity.Remove();
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<AthleteProfile> GetOrCreateProfileAsync(
        Guid userId,
        string displayName,
        CancellationToken cancellationToken)
    {
        var profile = await dbContext.AthleteProfiles
            .SingleOrDefaultAsync(item => item.UserId == userId, cancellationToken);
        if (profile is not null)
        {
            return profile;
        }

        profile = new AthleteProfile(userId, displayName);
        profile.Update(
            displayName,
            180,
            65,
            "Desarrollar un perfil atlético híbrido y llegar preparado a la evaluación policial de 2027.",
            new DateOnly(2027, 5, 15),
            null,
            null,
            null);
        dbContext.AthleteProfiles.Add(profile);
        AddInitialFacts(profile.Id);
        await dbContext.SaveChangesAsync(cancellationToken);
        return profile;
    }

    private void AddInitialFacts(Guid profileId)
    {
        const string source = "Manual Biomecánico Personal — Perfil del Atleta, versión 1.0";
        dbContext.ProfileFacts.AddRange(
            new ProfileFact(profileId, "Contexto", "Trabajo físicamente activo", "Limpieza, caminata, tiempo de pie, escaleras y traslado de cargas", ProfileFactStatus.SelfReported, source),
            new ProfileFact(profileId, "Contexto", "Bicicleta cotidiana", "Aproximadamente 5 km diarios como transporte", ProfileFactStatus.SelfReported, source),
            new ProfileFact(profileId, "Actividad", "Taekwondo", "Inicio reciente", ProfileFactStatus.Confirmed, source),
            new ProfileFact(profileId, "Historia deportiva", "Fútbol", "Experiencia como arquero y jugador de campo", ProfileFactStatus.SelfReported, source),
            new ProfileFact(profileId, "Salud", "Antecedente de LCA", "Rotura referida hace aproximadamente 8-9 años", ProfileFactStatus.SelfReported, source, "Faltan lateralidad, informes y evaluación actual."),
            new ProfileFact(profileId, "Salud", "Antecedente meniscal", "Lesión posterior al evento de LCA", ProfileFactStatus.SelfReported, source, "Faltan menisco afectado, patrón e informe de imagen."),
            new ProfileFact(profileId, "Salud", "Hiperlaxitud", "Generalizada según autorreporte", ProfileFactStatus.SelfReported, source, "Pendiente de evaluación profesional."),
            new ProfileFact(profileId, "Pendientes", "Rodilla afectada", null, ProfileFactStatus.Pending, source),
            new ProfileFact(profileId, "Pendientes", "Reglamento policial 2027", null, ProfileFactStatus.Pending, source));
    }

    private static AtlasProfileDto ToDto(AthleteProfile profile) => new(
        profile.Id,
        profile.DisplayName,
        profile.HeightCm,
        profile.ReferenceWeightKg,
        profile.PrimaryGoal,
        profile.TargetDate,
        profile.DominantHand,
        profile.DominantLeg,
        profile.AffectedKnee);

    private static ProfileFactDto ToDto(ProfileFact fact) => new(
        fact.Id,
        fact.Category,
        fact.Label,
        fact.Value,
        fact.Status.ToString(),
        fact.Status switch
        {
            ProfileFactStatus.Confirmed => "Confirmado",
            ProfileFactStatus.SelfReported => "Autorreportado",
            ProfileFactStatus.WorkingHypothesis => "Hipótesis",
            ProfileFactStatus.Pending => "Pendiente",
            ProfileFactStatus.ProfessionalValidated => "Validado profesionalmente",
            _ => fact.Status.ToString()
        },
        fact.SourceTitle,
        fact.Notes);

    private static DailyCheckInDto ToDto(DailyCheckIn item) => new(
        item.Id,
        item.Date,
        item.SleepMinutes,
        item.SleepQuality,
        item.Energy,
        item.Fatigue,
        item.Stress,
        item.PainLocation,
        item.PainSide,
        item.PainIntensity,
        item.Stiffness,
        item.Swelling,
        item.Instability,
        item.Locking,
        item.ExpectedWorkLoad,
        item.PlannedCyclingKm,
        item.PlannedActivity,
        item.Notes,
        item.Instability || item.Locking || item.PainIntensity >= 5 || item.Swelling is "moderada" or "alta");

    private static DailyActivityDto ToDto(DailyActivity item) => new(item.Id, item.Date, item.ActivityType, item.DurationMinutes, item.Rpe, item.DistanceKm, item.Notes, item.InternalLoad,item.PlannedDurationMinutes,item.PlannedSource,item.PlannedDurationMinutes.HasValue?item.DurationMinutes-item.PlannedDurationMinutes.Value:null,item.WorkDemands,item.BreakMinutes,item.UnusualConditions);
    private static DailyPlanDecisionDto ToDto(DailyPlanDecision item)=>new(item.Id,item.Date,item.Decision,item.Reason,item.ContextStatus,item.PlannedLoadSnapshot,item.Version,item.UpdatedAt??item.CreatedAt);
    private static TimeOnly? End(TrainingSession session)=>session.PlannedStartTime.HasValue&&session.PlannedDurationMinutes.HasValue?session.PlannedStartTime.Value.AddMinutes(session.PlannedDurationMinutes.Value):null;
    private static bool SameActivity(TrainingSession session,RecurringScheduleBlock block)=>block.Category=="training"&&(session.ActivityType.Equals(block.Name,StringComparison.OrdinalIgnoreCase)||session.Name.Contains(block.Name,StringComparison.OrdinalIgnoreCase)||block.Name.Contains(session.ActivityType,StringComparison.OrdinalIgnoreCase));
    private static DailyStateInput StateInput(DailyCheckIn item)=>new(item.SleepMinutes,item.SleepQuality,item.Energy,item.Fatigue,item.Stress,item.PainIntensity,item.Instability||item.Locking||item.PainIntensity>=5||item.Swelling is "moderada" or "alta");

    private static IReadOnlyCollection<DailyActionDto> BuildDailyActions(
        DailyCheckIn? checkIn,
        IReadOnlyCollection<TrainingSession> todaySessions,
        IReadOnlyCollection<DailyActivity> todayActivities,
        KneeCheck? latestKneeCheck,
        bool hasActiveGoal,
        bool hasCurrentCycle,
        DateOnly? latestLearningDate,
        int openLearningActions,
        int dueLearningActions,
        DateOnly oldestOpenLearningDate,
        int pendingFollowUps,
        int metricCount,
        int metricsWithoutEntries,
        bool hasDecision)
    {
        var actions = new List<DailyActionDto>();
        actions.Add(checkIn is null
            ? new("check-in", "Completar el check-in", "Registrá sueño, energía, fatiga, estrés y síntomas antes de interpretar el día.", "/", "pending")
            : new("check-in", "Check-in completado", checkIn.Instability || checkIn.Locking || checkIn.PainIntensity >= 5 ? "Hay síntomas que merecen seguimiento." : "El contexto diario ya quedó registrado.", "/", "done"));

        foreach (var session in todaySessions.Where(item => item.Status != TrainingSessionStatus.Completed))
            actions.Add(new("training", session.Status == TrainingSessionStatus.InProgress ? "Continuar entrenamiento" : $"Entrenamiento: {session.Name}",
                $"{session.ActivityType} · {session.PlannedDurationMinutes?.ToString() ?? "—"} min · RPE objetivo {session.TargetRpe?.ToString() ?? "—"}.", "/entrenamiento", session.Status == TrainingSessionStatus.InProgress ? "active" : "pending"));

        if (pendingFollowUps > 0)
            actions.Add(new("follow-up", "Registrar respuesta posterior", $"Tenés {pendingFollowUps} control{(pendingFollowUps == 1 ? "" : "es")} de 24 horas pendiente{(pendingFollowUps == 1 ? "" : "s")}.", "/respuesta-24h", "attention"));
        if (openLearningActions > 0)
        {
            var daysOpen = Math.Max(0, DateOnly.FromDateTime(DateTime.Today).DayNumber - oldestOpenLearningDate.DayNumber);
            var detail = dueLearningActions > 0
                ? $"{dueLearningActions} de {openLearningActions} acción{(openLearningActions == 1 ? "" : "es")} tiene{(dueLearningActions == 1 ? "" : "n")} revisión prevista para hoy o una fecha anterior."
                : $"Hay {openLearningActions} acción{(openLearningActions == 1 ? "" : "es")} abierta{(openLearningActions == 1 ? "" : "s")}. La más antigua se registró hace {daysOpen} día{(daysOpen == 1 ? "" : "s")}.";
            actions.Add(new("learning-follow-up", openLearningActions == 1 ? "Revisar una acción de la bitácora" : "Revisar acciones de la bitácora", detail, "/bitacora?filter=open", "pending"));
        }
        if (metricCount == 0)
            actions.Add(new("measurement", "Definir el primer indicador", "Creá una medición corporal o prueba física con unidad y protocolo.", "/mediciones", "optional"));
        else if (metricsWithoutEntries > 0)
            actions.Add(new("measurement", "Completar mediciones iniciales", $"{metricsWithoutEntries} indicador{(metricsWithoutEntries == 1 ? "" : "es")} todavía no {(metricsWithoutEntries == 1 ? "tiene" : "tienen")} un valor de base.", "/mediciones", "optional"));
        if (todaySessions.Count == 0)
            actions.Add(new("planning", "Planificar el entrenamiento", "No hay una sesión programada para hoy. También puede ser un día de descanso intencional.", "/entrenamiento", "optional"));
        if (todayActivities.Count == 0)
            actions.Add(new("daily-load", "Registrar actividad cotidiana", "Sumá trabajo físico, bicicleta u otra actividad realizada para completar la carga del día.", "/#carga-diaria", "optional"));
        if(checkIn is not null)actions.Add(hasDecision?new("decision","Decisión del día registrada","Atlas conservará tu elección junto con el contexto y la carga prevista.","/#decision-dia","done"):new("decision","Registrar la decisión del día","Después de revisar contexto y plan, dejá asentado qué elegís hacer y por qué.","/#decision-dia","pending"));
        if (latestKneeCheck is null || latestKneeCheck.RecordedAt < DateTimeOffset.UtcNow.AddHours(-48))
            actions.Add(new("knee", "Actualizar respuesta de la rodilla", "Registrá síntomas y función para construir la evolución de 0 a 48 horas.", "/rodilla", "optional"));
        else if (latestKneeCheck.Locking || latestKneeCheck.Instability || !latestKneeCheck.FullExtension || latestKneeCheck.PainWorst24H >= 7 || latestKneeCheck.Swelling == "alta")
            actions.Add(new("knee", "Revisar señales de rodilla", "El último control contiene señales que merecen seguimiento y posible consulta profesional.", "/rodilla", "attention"));
        if (DateTime.Today.DayOfWeek is DayOfWeek.Sunday or DayOfWeek.Monday)
            actions.Add(new("weekly", "Revisar la semana", "Compará carga total, bienestar y función con los siete días anteriores.", "/resumen-semanal", "optional"));
        if (!hasActiveGoal || !hasCurrentCycle)
            actions.Add(new("plan", "Completar el plan maestro", !hasActiveGoal ? "Definí al menos un objetivo activo y medible." : "Creá o activá el ciclo de trabajo actual.", "/plan", "optional"));
        if (openLearningActions == 0 && (!latestLearningDate.HasValue || latestLearningDate.Value < DateOnly.FromDateTime(DateTime.Today.AddDays(-14))))
            actions.Add(new("learning", "Registrar un aprendizaje", "Separá una observación reciente, tu interpretación provisional y la próxima acción.", "/bitacora", "optional"));
        return actions;
    }
}
