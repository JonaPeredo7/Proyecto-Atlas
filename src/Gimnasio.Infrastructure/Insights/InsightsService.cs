using Gimnasio.Application.Insights;
using Gimnasio.Domain.Enums;
using Gimnasio.Domain.Services;
using Gimnasio.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Gimnasio.Infrastructure.Insights;

internal sealed class InsightsService(GimnasioDbContext dbContext) : IInsightsService
{
    public async Task<LongTermReviewDto> GetLongTermAsync(Guid userId,int weeks,CancellationToken cancellationToken=default)
    {
        if(weeks is not(4 or 8 or 12))throw new ArgumentException("La evaluación admite períodos de 4, 8 o 12 semanas.");
        var profile=await dbContext.AthleteProfiles.AsNoTracking().SingleOrDefaultAsync(x=>x.UserId==userId,cancellationToken)??throw new InvalidOperationException("Primero debe inicializarse el perfil Atlas.");
        var today=DateOnly.FromDateTime(DateTime.Today);var from=today.AddDays(-(weeks*7-1));
        var sessions=await dbContext.TrainingSessions.AsNoTracking().Where(x=>x.AthleteProfileId==profile.Id&&x.Date>=from&&x.Date<=today&&x.Status==TrainingSessionStatus.Completed).ToListAsync(cancellationToken);
        var activities=await dbContext.DailyActivities.AsNoTracking().Where(x=>x.AthleteProfileId==profile.Id&&x.Date>=from&&x.Date<=today&&x.IsActive).ToListAsync(cancellationToken);
        var checks=await dbContext.DailyCheckIns.AsNoTracking().Where(x=>x.AthleteProfileId==profile.Id&&x.Date>=from&&x.Date<=today).ToListAsync(cancellationToken);
        var fromTime=new DateTimeOffset(from.ToDateTime(TimeOnly.MinValue),TimeZoneInfo.Local.GetUtcOffset(from.ToDateTime(TimeOnly.MinValue)));
        var knee=await dbContext.KneeChecks.AsNoTracking().Where(x=>x.AthleteProfileId==profile.Id&&x.IsActive&&x.RecordedAt>=fromTime).ToListAsync(cancellationToken);
        var timeline=Enumerable.Range(0,weeks).Select(index=>{var start=from.AddDays(index*7);var end=start.AddDays(6);var s=sessions.Where(x=>x.Date>=start&&x.Date<=end).ToArray();var a=activities.Where(x=>x.Date>=start&&x.Date<=end).ToArray();var c=checks.Where(x=>x.Date>=start&&x.Date<=end).ToArray();var k=knee.Where(x=>{var d=DateOnly.FromDateTime(x.RecordedAt.LocalDateTime);return d>=start&&d<=end;}).ToArray();var training=s.Sum(x=>(x.ActualDurationMinutes??0)*(x.SessionRpe??0));var external=a.Sum(x=>x.InternalLoad);var work=WorkPlanComparisonCalculator.Calculate(a.Where(x=>x.PlannedDurationMinutes.HasValue).Select(x=>new WorkPlanEntry(x.PlannedDurationMinutes!.Value,x.DurationMinutes)));var context=WorkContextSummaryCalculator.Calculate(a.Where(x=>x.ActivityType.Contains("trabajo",StringComparison.OrdinalIgnoreCase)).Select(x=>new WorkContextEntry(x.Date,x.WorkDemands,x.BreakMinutes,x.UnusualConditions)));return new LongTermWeekDto(start,end,training,external,training+external,s.Length,s.Sum(x=>x.ActualDurationMinutes??0),a.Sum(x=>x.DurationMinutes),c.Length,Average(c.Select(x=>(int?)x.Fatigue)),Average(c.Select(x=>x.PainIntensity)),k.Count(IsKneeAttention),work.RecordedDays,work.PlannedMinutes,work.ActualMinutes,work.DifferenceMinutes,work.ExtraMinutes,context.ContextRecordedDays,context.TotalBreakMinutes,context.UnusualDays);}).ToArray();
        var half=weeks/2;var first=timeline.Take(half).ToArray();var second=timeline.Skip(half).ToArray();var firstLoad=first.Sum(x=>x.TotalLoad);var secondLoad=second.Sum(x=>x.TotalLoad);decimal?loadChange=firstLoad==0?null:Math.Round((secondLoad-firstLoad)*100m/firstLoad,1);
        var metricDefinitions=await dbContext.MetricDefinitions.AsNoTracking().Where(x=>x.AthleteProfileId==profile.Id&&x.IsActive).ToListAsync(cancellationToken);var metricIds=metricDefinitions.Select(x=>x.Id).ToArray();var measurementEntries=await dbContext.MeasurementEntries.AsNoTracking().Where(x=>metricIds.Contains(x.MetricDefinitionId)&&x.IsActive&&x.Date>=from&&x.Date<=today).OrderBy(x=>x.Date).ToListAsync(cancellationToken);
        var metricEvolution=metricDefinitions.Select(metric=>{var values=measurementEntries.Where(x=>x.MetricDefinitionId==metric.Id).ToArray();if(values.Length<2)return null;var firstValue=values.First();var latest=values.Last();return new MetricEvolutionDto(metric.Id,metric.Name,metric.Unit,metric.Direction.ToString(),firstValue.Value,firstValue.Date,latest.Value,latest.Date,latest.Value-firstValue.Value,values.Length);}).Where(x=>x is not null).Cast<MetricEvolutionDto>().ToArray();
        var coverage=sessions.Select(x=>x.Date).Concat(activities.Select(x=>x.Date)).Concat(checks.Select(x=>x.Date)).Concat(knee.Select(x=>DateOnly.FromDateTime(x.RecordedAt.LocalDateTime))).Distinct().Count();var signals=new List<InsightSignalDto>();
        if(coverage<weeks*3)signals.Add(new("info","Cobertura longitudinal limitada",$"Hay datos en {coverage} de {weeks*7} días.","Las comparaciones describen únicamente los registros disponibles."));
        if(loadChange is>25)signals.Add(new("observe","La segunda mitad tuvo más carga",$"La carga total aumentó {loadChange}% entre ambas mitades.","Suma entrenamiento y actividad cotidiana; no predice lesión."));else if(loadChange is<-25)signals.Add(new("info","La segunda mitad tuvo menos carga",$"La carga total descendió {Math.Abs(loadChange.Value)}% entre ambas mitades.","Comparación descriptiva del período seleccionado."));
        var periodWork=WorkPlanComparisonCalculator.Calculate(activities.Where(x=>x.PlannedDurationMinutes.HasValue).Select(x=>new WorkPlanEntry(x.PlannedDurationMinutes!.Value,x.DurationMinutes)));
        var periodWorkContext=WorkContextSummaryCalculator.Calculate(activities.Where(x=>x.ActivityType.Contains("trabajo",StringComparison.OrdinalIgnoreCase)).Select(x=>new WorkContextEntry(x.Date,x.WorkDemands,x.BreakMinutes,x.UnusualConditions)));
        if(periodWork.ExtraMinutes>0)signals.Add(new("info","El período incluye trabajo adicional",$"Se acumularon {periodWork.ExtraMinutes} minutos por encima de lo previsto en {periodWork.DaysWithExtra} jornada(s).","Usa únicamente jornadas con una duración prevista guardada; no atribuye efectos físicos a ese tiempo."));
        if(timeline.Any(x=>x.KneeAttentionChecks>0))signals.Add(new("attention","Hubo controles funcionales de atención","Al menos una semana contiene señales funcionales relevantes de rodilla.","Revisá fechas y evolución; Atlas no determina la causa."));
        if(signals.Count==0)signals.Add(new("neutral","Sin cambios destacados","No se activaron señales descriptivas con los datos disponibles.","Esto no equivale a autorización médica ni ausencia de riesgo."));
        var totalLoad=timeline.Sum(x=>x.TotalLoad);return new(weeks,from,today,timeline,new(totalLoad,Math.Round(totalLoad/(decimal)weeks,1),sessions.Count,sessions.Sum(x=>x.ActualDurationMinutes??0),activities.Sum(x=>x.DurationMinutes),loadChange,Delta(Average(second.Select(x=>x.AverageFatigue.HasValue?(int?)Math.Round(x.AverageFatigue.Value):null)),Average(first.Select(x=>x.AverageFatigue.HasValue?(int?)Math.Round(x.AverageFatigue.Value):null))),Delta(Average(second.Select(x=>x.AveragePain.HasValue?(int?)Math.Round(x.AveragePain.Value):null)),Average(first.Select(x=>x.AveragePain.HasValue?(int?)Math.Round(x.AveragePain.Value):null)))),new(periodWork.RecordedDays,periodWork.PlannedMinutes,periodWork.ActualMinutes,periodWork.DifferenceMinutes,periodWork.ExtraMinutes,periodWork.ShorterMinutes,timeline.Count(x=>x.WorkRecordedDays>0),periodWorkContext.ContextRecordedDays,periodWorkContext.TotalBreakMinutes,periodWorkContext.UnusualDays),metricEvolution,signals,coverage);
    }

    private static bool IsKneeAttention(Gimnasio.Domain.Entities.KneeCheck x)=>x.Locking||x.Instability||!x.FullExtension||x.Swelling=="alta"||x.PainWorst24H>=7;

    public async Task<WeeklyReportDto> GetWeeklyAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var profile = await dbContext.AthleteProfiles.AsNoTracking().SingleOrDefaultAsync(x => x.UserId == userId, cancellationToken) ?? throw new InvalidOperationException("Primero debe inicializarse el perfil Atlas.");
        var today = DateOnly.FromDateTime(DateTime.Today); var currentFrom = today.AddDays(-6); var previousFrom = today.AddDays(-13);
        var sessions = await dbContext.TrainingSessions.AsNoTracking().Where(x => x.AthleteProfileId == profile.Id && x.Date >= previousFrom && x.Status == TrainingSessionStatus.Completed).ToListAsync(cancellationToken);
        var activities = await dbContext.DailyActivities.AsNoTracking().Where(x => x.AthleteProfileId == profile.Id && x.Date >= previousFrom && x.IsActive).ToListAsync(cancellationToken);
        var checks = await dbContext.DailyCheckIns.AsNoTracking().Where(x => x.AthleteProfileId == profile.Id && x.Date >= previousFrom).ToListAsync(cancellationToken);
        var knee = await dbContext.KneeChecks.AsNoTracking().Where(x => x.AthleteProfileId == profile.Id && x.IsActive && x.RecordedAt >= previousFrom.ToDateTime(TimeOnly.MinValue)).ToListAsync(cancellationToken);
        var decisions = await dbContext.DailyPlanDecisions.AsNoTracking().Where(x => x.AthleteProfileId == profile.Id && x.IsActive && x.Date >= currentFrom && x.Date <= today).CountAsync(cancellationToken);
        var learning = await dbContext.LearningEntries.AsNoTracking().Where(x => x.AthleteProfileId == profile.Id && x.IsActive).ToListAsync(cancellationToken);
        WeeklyTotalsDto Totals(DateOnly from, DateOnly to)
        {
            var s=sessions.Where(x=>x.Date>=from&&x.Date<=to).ToArray();var a=activities.Where(x=>x.Date>=from&&x.Date<=to).ToArray();var c=checks.Where(x=>x.Date>=from&&x.Date<=to).ToArray();var k=knee.Where(x=>DateOnly.FromDateTime(x.RecordedAt.LocalDateTime)>=from&&DateOnly.FromDateTime(x.RecordedAt.LocalDateTime)<=to).ToArray();
            return new(s.Sum(x=>(x.ActualDurationMinutes??0)*(x.SessionRpe??0)),a.Sum(x=>x.InternalLoad),s.Sum(x=>(x.ActualDurationMinutes??0)*(x.SessionRpe??0))+a.Sum(x=>x.InternalLoad),s.Length,s.Sum(x=>x.ActualDurationMinutes??0),a.Sum(x=>x.DurationMinutes),c.Length,Average(c.Select(x=>(int?)x.Fatigue)),Average(c.Select(x=>x.PainIntensity)),k.Length);
        }
        var current=Totals(currentFrom,today);var previous=Totals(previousFrom,currentFrom.AddDays(-1));
        WorkPlanComparison WorkCalculation(DateOnly from,DateOnly to)=>WorkPlanComparisonCalculator.Calculate(activities.Where(x=>x.Date>=from&&x.Date<=to&&x.PlannedDurationMinutes.HasValue).Select(x=>new WorkPlanEntry(x.PlannedDurationMinutes!.Value,x.DurationMinutes)));
        WorkPeriodDto WorkDto(WorkPlanComparison result)=>new(result.RecordedDays,result.PlannedMinutes,result.ActualMinutes,result.DifferenceMinutes,result.ExtraMinutes,result.ShorterMinutes,result.DaysWithExtra);
        var currentWork=WorkCalculation(currentFrom,today);var previousWork=WorkCalculation(previousFrom,currentFrom.AddDays(-1));var workTrend=WorkPlanComparisonCalculator.Compare(currentWork,previousWork);
        var workContext=WorkContextSummaryCalculator.Calculate(activities.Where(x=>x.Date>=currentFrom&&x.Date<=today&&x.ActivityType.Contains("trabajo",StringComparison.OrdinalIgnoreCase)).Select(x=>new WorkContextEntry(x.Date,x.WorkDemands,x.BreakMinutes,x.UnusualConditions)));
        var work=new WeeklyWorkDto(WorkDto(currentWork),WorkDto(previousWork),new(workTrend.HasComparison,workTrend.ActualMinutesPerDayChange,workTrend.ExtraMinutesPerDayChange),new(workContext.ContextRecordedDays,workContext.TotalBreakMinutes,workContext.UnusualDays,workContext.Demands,workContext.UnusualConditions.Select(x=>new WorkContextNoteDto(x.Date,x.Text)).ToArray()));
        var days=Enumerable.Range(0,7).Select(i=>currentFrom.AddDays(i)).Select(date=>{var s=sessions.Where(x=>x.Date==date).ToArray();var a=activities.Where(x=>x.Date==date).ToArray();var c=checks.SingleOrDefault(x=>x.Date==date);var k=knee.Where(x=>DateOnly.FromDateTime(x.RecordedAt.LocalDateTime)==date).OrderByDescending(x=>x.RecordedAt).FirstOrDefault();var tl=s.Sum(x=>(x.ActualDurationMinutes??0)*(x.SessionRpe??0));var el=a.Sum(x=>x.InternalLoad);return new WeeklyDayDto(date,tl,el,tl+el,s.Length,a.Length,c is not null,c?.Fatigue,c?.PainIntensity,KneeState(k));}).ToArray();
        decimal? percent=previous.TotalLoad==0?null:Math.Round((current.TotalLoad-previous.TotalLoad)*100m/previous.TotalLoad,1);
        var signals=new List<InsightSignalDto>();
        if(current.CheckIns<4)signals.Add(new("info","Cobertura diaria incompleta",$"Hay {current.CheckIns} check-ins en 7 días.","Con pocos días, los promedios describen sólo los registros disponibles."));
        if(percent is >25)signals.Add(new("observe","Aumentó la carga total",$"La carga registrada subió {percent}% frente a la semana anterior.","Incluye entrenamiento y actividad cotidiana; no estima riesgo de lesión."));
        if(work.Current.ExtraMinutes>0)signals.Add(new("info","Hubo tiempo laboral adicional",$"Se registraron {work.Current.ExtraMinutes} minutos por encima de lo previsto en {work.Current.DaysWithExtra} día(s).","Comparación entre la duración prevista guardada y la duración real informada; no demuestra efectos sobre fatiga o rendimiento."));
        if(days.Any(x=>x.KneeState=="attention"))signals.Add(new("attention","Hubo señales funcionales de rodilla","Al menos un control semanal registró una señal de atención.","Revisá la evolución y considerá consulta profesional si persiste o empeora."));
        if(signals.Count==0)signals.Add(new("neutral","Sin cambios destacados","Los datos disponibles no activaron una señal descriptiva semanal.","No equivale a autorización médica para entrenar."));
        var cycle=await dbContext.TrainingCycles.AsNoTracking().Where(x=>x.AthleteProfileId==profile.Id&&x.IsActive&&x.Status=="active"&&x.StartDate<=today&&x.EndDate>=today).OrderByDescending(x=>x.StartDate).FirstOrDefaultAsync(cancellationToken);
        WeeklyCycleDto? cycleDto=null;if(cycle is not null){var elapsed=today.DayNumber-cycle.StartDate.DayNumber+1;var expected=(int)Math.Ceiling(elapsed/7m*cycle.PlannedSessionsPerWeek);var done=await dbContext.TrainingSessions.AsNoTracking().CountAsync(x=>x.TrainingCycleId==cycle.Id&&x.Status==TrainingSessionStatus.Completed&&x.Date>=cycle.StartDate&&x.Date<=today,cancellationToken);cycleDto=new(cycle.Id,cycle.Name,cycle.Focus,expected,done,expected==0?null:Math.Round(done*100m/expected,1),cycle.EndDate);}
        var reflection=learning.Where(x=>x.Category=="Revisión semanal"&&x.Date==today).OrderByDescending(x=>x.CreatedAt).FirstOrDefault();
        var review=new WeeklyReviewDto(decisions,learning.Count(x=>x.Status=="open"&&!string.IsNullOrWhiteSpace(x.NextAction)),learning.Count(x=>x.Status=="applied"&&x.ReviewedOn>=currentFrom&&x.ReviewedOn<=today),learning.Count(x=>x.Status=="open"&&x.ReviewDueOn.HasValue&&x.ReviewDueOn<=today),reflection is not null,reflection?.Id);
        return new(currentFrom,today,current,previous,new(percent,Delta(current.AverageFatigue,previous.AverageFatigue),Delta(current.AveragePain,previous.AveragePain)),work,days,signals,days.Count(x=>x.HasCheckIn||x.TotalLoad>0||x.KneeState!="none"),cycleDto,review);
    }

    public async Task<InsightsOverviewDto> GetAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var profile = await dbContext.AthleteProfiles.AsNoTracking().SingleOrDefaultAsync(x => x.UserId == userId, cancellationToken)
            ?? throw new InvalidOperationException("Primero debe inicializarse el perfil Atlas.");
        var today = DateOnly.FromDateTime(DateTime.Today);
        var from = today.AddDays(-27);
        var sessions = await dbContext.TrainingSessions.AsNoTracking()
            .Where(x => x.AthleteProfileId == profile.Id && x.Date >= from && x.Status == TrainingSessionStatus.Completed)
            .ToListAsync(cancellationToken);
        var checkIns = await dbContext.DailyCheckIns.AsNoTracking()
            .Where(x => x.AthleteProfileId == profile.Id && x.Date >= from).ToListAsync(cancellationToken);
        var activities=await dbContext.DailyActivities.AsNoTracking().Where(x=>x.AthleteProfileId==profile.Id&&x.Date>=from&&x.IsActive).ToListAsync(cancellationToken);
        var decisions=await dbContext.DailyPlanDecisions.AsNoTracking().Where(x=>x.AthleteProfileId==profile.Id&&x.Date>=from&&x.IsActive).OrderByDescending(x=>x.Date).ToListAsync(cancellationToken);
        var learningEntries=await dbContext.LearningEntries.AsNoTracking().Where(x=>x.AthleteProfileId==profile.Id&&x.IsActive).ToListAsync(cancellationToken);
        var sessionIds = sessions.Select(x => x.Id).ToArray();
        var followUps = await dbContext.TrainingFollowUps.AsNoTracking()
            .Where(x => sessionIds.Contains(x.TrainingSessionId)).ToListAsync(cancellationToken);

        var days = Enumerable.Range(0, 28).Select(offset => from.AddDays(offset)).Select(date =>
        {
            var dailySessions = sessions.Where(x => x.Date == date).ToArray();
            var check = checkIns.SingleOrDefault(x => x.Date == date);
            var dailyFollowUps = followUps.Where(x => dailySessions.Any(s => s.Id == x.TrainingSessionId)).ToArray();
            return new DailyTrendDto(date,
                dailySessions.Sum(x => (x.ActualDurationMinutes ?? 0) * (x.SessionRpe ?? 0)),
                check?.SleepQuality, check?.Energy, check?.Fatigue, check?.Stress, check?.PainIntensity,
                dailyFollowUps.Length == 0 ? null : (int)Math.Round(dailyFollowUps.Average(x => x.Recovery)));
        }).ToArray();

        var currentStart = today.AddDays(-6);
        var previousStart = today.AddDays(-13);
        var current = sessions.Where(x => x.Date >= currentStart).ToArray();
        var previous = sessions.Where(x => x.Date >= previousStart && x.Date < currentStart).ToArray();
        var currentLoad = Load(current);
        var previousLoad = Load(previous);
        decimal? change = previousLoad == 0 ? null : Math.Round((currentLoad - previousLoad) * 100m / previousLoad, 1);
        var recentChecks = checkIns.Where(x => x.Date >= currentStart).ToArray();
        var recentFollowUps = followUps.Where(x => sessions.Any(s => s.Id == x.TrainingSessionId && s.Date >= currentStart)).ToArray();
        var wellbeing = new WellbeingSummaryDto(
            Average(recentChecks.Select(x => (int?)x.SleepQuality)), Average(recentChecks.Select(x => (int?)x.Energy)),
            Average(recentChecks.Select(x => (int?)x.Fatigue)), Average(recentChecks.Select(x => (int?)x.Stress)),
            Average(recentChecks.Select(x => x.PainIntensity)), Average(recentFollowUps.Select(x => (int?)x.Recovery)));
        var variability=PersonalVariabilityCalculator.Calculate(checkIns.Select(x=>new PersonalVariabilityInput(x.SleepMinutes.HasValue?x.SleepMinutes.Value/60m:null,x.SleepQuality,x.Energy,x.Fatigue,x.Stress,x.PainIntensity)));
        var variabilityDto=new PersonalVariabilityDto(variability.WindowDays,variability.CheckInDays,variability.CoveragePercent,variability.Factors.Select(x=>new PersonalVariabilityFactorDto(x.Key,x.Label,x.Unit,x.Entries,x.Median,x.LowerQuartile,x.UpperQuartile,x.Minimum,x.Maximum)).ToArray(),variability.Disclaimer);
        PersonalVariabilityInput VariabilityInput(Gimnasio.Domain.Entities.DailyCheckIn x)=>new(x.SleepMinutes.HasValue?x.SleepMinutes.Value/60m:null,x.SleepQuality,x.Energy,x.Fatigue,x.Stress,x.PainIntensity);
        var periodComparison=PersonalPeriodComparisonCalculator.Calculate(recentChecks.Select(VariabilityInput),checkIns.Where(x=>x.Date<currentStart).Select(VariabilityInput));
        var periodComparisonDto=new PersonalPeriodComparisonDto(periodComparison.RecentDays,periodComparison.BaselineDays,periodComparison.RecentCheckIns,periodComparison.BaselineCheckIns,periodComparison.RecentCoveragePercent,periodComparison.BaselineCoveragePercent,periodComparison.ComparableFactors,periodComparison.Factors.Select(x=>new PersonalPeriodComparisonFactorDto(x.Key,x.Label,x.Unit,x.RecentEntries,x.BaselineEntries,x.RecentMedian,x.BaselineMedian,x.BaselineLowerQuartile,x.BaselineUpperQuartile,x.Delta,x.Position)).ToArray(),periodComparison.Disclaimer);
        var dataDays = days.Count(x => x.InternalLoad > 0 || x.SleepQuality.HasValue || x.Recovery.HasValue);
        var signals = BuildSignals(currentLoad, previousLoad, change, recentChecks.Length, recentFollowUps);
        var decisionOutcomes=decisions.Select(decision=>{var dailySessions=sessions.Where(x=>x.Date==decision.Date).ToArray();var training=Load(dailySessions);var external=activities.Where(x=>x.Date==decision.Date).Sum(x=>x.InternalLoad);var check=checkIns.SingleOrDefault(x=>x.Date==decision.Date);var post=followUps.Where(x=>dailySessions.Any(s=>s.Id==x.TrainingSessionId)).Any(x=>x.Instability||x.Locking||x.PainIntensity>=5||x.Swelling is "moderada" or "alta");return new DecisionOutcomeDto(decision.Date,decision.Decision,decision.Reason,decision.ContextStatus,decision.PlannedLoadSnapshot,training,external,training+external,check?.PainIntensity,check?.Fatigue,post,decision.Version);}).ToArray();
        var reviewedLearning=learningEntries.Where(x=>x.ReviewedOn.HasValue&&x.ReviewedOn.Value>=from&&x.ReviewedOn.Value<=today).ToArray();
        var learningPatterns=LearningPatternCalculator.Calculate(reviewedLearning.Where(x=>x.FollowUpOutcome is not null).Select(x=>new LearningPatternInput(x.Category,x.FollowUpOutcome!)));
        var learningTrend=new LearningTrendDto(reviewedLearning.Length,reviewedLearning.Count(x=>x.FollowUpOutcome=="helpful"),reviewedLearning.Count(x=>x.FollowUpOutcome=="neutral"),reviewedLearning.Count(x=>x.FollowUpOutcome=="not-helpful"),reviewedLearning.Count(x=>x.FollowUpOutcome=="inconclusive"),learningEntries.Count(x=>x.Status=="open"&&!string.IsNullOrWhiteSpace(x.NextAction)),learningEntries.Count(x=>x.Status=="open"&&x.ReviewDueOn.HasValue&&x.ReviewDueOn<=today),reviewedLearning.GroupBy(x=>x.Category).OrderByDescending(x=>x.Count()).Select(x=>new LearningCategoryDto(x.Key,x.Count())).ToArray(),learningPatterns.Select(x=>new LearningPatternDto(x.Category,x.Entries,x.DominantOutcome,x.OutcomeCount,x.SharePercent,x.State)).ToArray(),"Resultados autorreportados y descriptivos. Una asociación temporal no demuestra que la acción haya causado el cambio observado.");
        var evidence = await dbContext.EvidenceReferences.AsNoTracking().Where(x => x.IsActive && (x.Topic == "Carga interna" || x.Topic == "Carga laboral" || x.Topic == "Bienestar" || x.Topic == "Sueño" || x.Topic == "Predicción de lesión"))
            .OrderBy(x => x.Topic).Select(x => new InsightEvidenceDto(x.Topic, x.Title, x.PermanentId ?? "", x.SourceUrl ?? "", x.Applicability ?? "", x.Limitations ?? "")).ToListAsync(cancellationToken);
        return new(days, new(currentLoad, previousLoad, change, current.Length, current.Sum(x => x.ActualDurationMinutes ?? 0)), wellbeing, variabilityDto, periodComparisonDto, signals, decisionOutcomes, learningTrend, evidence, dataDays, dataDays >= 7);
    }

    private static int Load(IEnumerable<Gimnasio.Domain.Entities.TrainingSession> sessions) => sessions.Sum(x => (x.ActualDurationMinutes ?? 0) * (x.SessionRpe ?? 0));
    private static decimal? Average(IEnumerable<int?> values) { var data = values.Where(x => x.HasValue).Select(x => x!.Value).ToArray(); return data.Length == 0 ? null : (decimal)Math.Round(data.Average(), 1); }
    private static decimal? Delta(decimal? current, decimal? previous) => current.HasValue && previous.HasValue ? Math.Round(current.Value - previous.Value, 1) : null;
    private static string KneeState(Gimnasio.Domain.Entities.KneeCheck? x) => x is null ? "none" : x.Locking || x.Instability || !x.FullExtension || x.Swelling == "alta" || x.PainWorst24H >= 7 ? "attention" : x.Swelling == "moderada" || x.PainWorst24H >= 5 || (x.WalkingCapacity+x.StairsCapacity+x.SquatCapacity)/3<=4 ? "observe" : "stable";
    private static IReadOnlyCollection<InsightSignalDto> BuildSignals(int currentLoad, int previousLoad, decimal? change, int checkCount, IReadOnlyCollection<Gimnasio.Domain.Entities.TrainingFollowUp> followUps)
    {
        var result = new List<InsightSignalDto>();
        if (checkCount < 3) result.Add(new("info", "Faltan registros de contexto", "Completá varios check-ins para interpretar la carga junto con sueño, energía, fatiga y estrés.", "Menos de 3 check-ins en los últimos 7 días."));
        if (change is > 25) result.Add(new("observe", "La carga semanal aumentó", $"La carga interna registrada es {change}% mayor que la semana anterior.", "Comparación descriptiva de dos períodos de 7 días; no predice lesión."));
        if (change is < -25) result.Add(new("info", "La carga semanal descendió", $"La carga interna registrada es {Math.Abs(change.Value)}% menor que la semana anterior.", "Comparación descriptiva de dos períodos de 7 días."));
        if (followUps.Any(x => x.Instability || x.Locking || x.PainIntensity >= 5 || x.Swelling is "moderada" or "alta")) result.Add(new("attention", "Hay síntomas posteriores relevantes", "Al menos una respuesta posterior registró dolor alto, inflamación, falseo o bloqueo.", "Autorreporte posterior; si persiste, empeora o preocupa, corresponde evaluación profesional."));
        if (result.Count == 0) result.Add(new("neutral", "Sin cambios destacables con los datos disponibles", "Atlas seguirá comparando carga y respuesta con tu propia historia.", "Esto no equivale a una autorización médica para entrenar."));
        return result;
    }
}
