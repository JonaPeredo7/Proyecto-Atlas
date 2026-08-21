using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Gimnasio.Application.Reports;
using Gimnasio.Domain.Entities;
using Gimnasio.Domain.Enums;
using Gimnasio.Domain.Services;
using Gimnasio.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Gimnasio.Infrastructure.Reports;

internal sealed class ReportService(GimnasioDbContext db) : IReportService
{
    public async Task<ProfessionalReportDto> GetProfessionalAsync(Guid userId, DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        if (to < from) throw new ArgumentException("La fecha final no puede ser anterior a la inicial.");
        if (to.DayNumber - from.DayNumber > 92) throw new ArgumentException("El informe admite hasta 93 días.");
        var profile = await db.AthleteProfiles.AsNoTracking().SingleOrDefaultAsync(x => x.UserId == userId, ct)
            ?? throw new InvalidOperationException("Primero debe inicializarse el perfil Atlas.");

        var sessions = await db.TrainingSessions.AsNoTracking().Where(x => x.AthleteProfileId == profile.Id && x.Date >= from && x.Date <= to && x.Status == TrainingSessionStatus.Completed).ToListAsync(ct);
        var activities = await db.DailyActivities.AsNoTracking().Where(x => x.AthleteProfileId == profile.Id && x.Date >= from && x.Date <= to && x.IsActive).ToListAsync(ct);
        var checks = await db.DailyCheckIns.AsNoTracking().Where(x => x.AthleteProfileId == profile.Id && x.Date >= from && x.Date <= to).ToListAsync(ct);
        var startDateTime = from.ToDateTime(TimeOnly.MinValue);
        var endDateTime = to.AddDays(1).ToDateTime(TimeOnly.MinValue);
        var startTime = new DateTimeOffset(startDateTime, TimeZoneInfo.Local.GetUtcOffset(startDateTime));
        var endTime = new DateTimeOffset(endDateTime, TimeZoneInfo.Local.GetUtcOffset(endDateTime));
        var knee = await db.KneeChecks.AsNoTracking().Where(x => x.AthleteProfileId == profile.Id && x.IsActive && x.RecordedAt >= startTime && x.RecordedAt < endTime).OrderByDescending(x => x.RecordedAt).Take(20).ToListAsync(ct);
        var goals = await db.PersonalGoals.AsNoTracking().Where(x => x.AthleteProfileId == profile.Id && x.IsActive && x.Status != "archived").ToListAsync(ct);
        var metrics = await db.MetricDefinitions.AsNoTracking().Where(x => x.AthleteProfileId == profile.Id && x.IsActive).ToListAsync(ct);
        var metricIds = metrics.Select(x => x.Id).ToArray();
        var allEntries = await db.MeasurementEntries.AsNoTracking().Where(x => metricIds.Contains(x.MetricDefinitionId) && x.IsActive && x.Date <= to).OrderBy(x => x.Date).ToListAsync(ct);
        var learning = await db.LearningEntries.AsNoTracking().Where(x => x.AthleteProfileId == profile.Id && x.IsActive && x.Date >= from && x.Date <= to).OrderByDescending(x => x.Date).Take(12).ToListAsync(ct);

        var days = to.DayNumber - from.DayNumber + 1;
        var weeks = Enumerable.Range(0, (int)Math.Ceiling(days / 7m)).Select(i =>
        {
            var start = from.AddDays(i * 7);
            var end = start.AddDays(6) > to ? to : start.AddDays(6);
            var weeklySessions = sessions.Where(x => x.Date >= start && x.Date <= end).ToArray();
            var weeklyActivities = activities.Where(x => x.Date >= start && x.Date <= end).ToArray();
            var trainingLoad = weeklySessions.Sum(x => (x.ActualDurationMinutes ?? 0) * (x.SessionRpe ?? 0));
            var externalLoad = weeklyActivities.Sum(x => x.InternalLoad);
            var work = WorkPlanComparisonCalculator.Calculate(weeklyActivities.Where(x => x.PlannedDurationMinutes.HasValue).Select(x => new WorkPlanEntry(x.PlannedDurationMinutes!.Value, x.DurationMinutes)));
            var workContext = WorkContextSummaryCalculator.Calculate(weeklyActivities.Where(x => x.ActivityType.Contains("trabajo", StringComparison.OrdinalIgnoreCase)).Select(x => new WorkContextEntry(x.Date, x.WorkDemands, x.BreakMinutes, x.UnusualConditions)));
            return new ReportWeekDto(start, end, trainingLoad, externalLoad, trainingLoad + externalLoad, weeklySessions.Length, work.RecordedDays, work.PlannedMinutes, work.ActualMinutes, work.ExtraMinutes, workContext.ContextRecordedDays, workContext.TotalBreakMinutes, workContext.UnusualDays);
        }).ToArray();

        var reportGoals = goals.Select(goal =>
        {
            var values = goal.MetricDefinitionId.HasValue ? allEntries.Where(x => x.MetricDefinitionId == goal.MetricDefinitionId).ToArray() : [];
            var latest = values.LastOrDefault();
            var baseline = goal.BaselineValue ?? values.FirstOrDefault()?.Value;
            decimal? progress = null;
            if (baseline.HasValue && goal.TargetValue.HasValue && latest is not null && baseline != goal.TargetValue)
                progress = Math.Round(Math.Clamp((latest.Value - baseline.Value) / (goal.TargetValue.Value - baseline.Value) * 100m, 0, 100), 1);
            return new ReportGoalDto(goal.Title, goal.Category, goal.Status, goal.BaselineValue, goal.TargetValue, goal.Unit, latest?.Value, latest?.Date, progress);
        }).ToArray();

        var reportMetrics = metrics.Select(metric =>
        {
            var values = allEntries.Where(x => x.MetricDefinitionId == metric.Id && x.Date >= from).ToArray();
            if (values.Length < 2) return null;
            var first = values.First();
            var latest = values.Last();
            return new ReportMetricDto(metric.Name, metric.Category, metric.Unit, metric.Direction.ToString(), first.Value, first.Date, latest.Value, latest.Date, latest.Value - first.Value, values.Length);
        }).Where(x => x is not null).Cast<ReportMetricDto>().ToArray();

        var reportKnee = knee.Select(x =>
        {
            var reasons = KneeReasons(x);
            var state = x.Locking || x.Instability || !x.FullExtension || x.Swelling == "alta" || x.PainWorst24H >= 7 ? "attention" : reasons.Count > 0 ? "observe" : "stable";
            return new ReportKneeDto(x.RecordedAt, x.Context, x.Side, x.PainNow, x.PainWorst24H, x.Swelling, x.Instability, x.Locking, x.FullExtension, (x.WalkingCapacity + x.StairsCapacity + x.SquatCapacity) / 3, state, reasons);
        }).ToArray();
        var trainingTotal = sessions.Sum(x => (x.ActualDurationMinutes ?? 0) * (x.SessionRpe ?? 0));
        var externalTotal = activities.Sum(x => x.InternalLoad);
        var workTotal = WorkPlanComparisonCalculator.Calculate(activities.Where(x => x.PlannedDurationMinutes.HasValue).Select(x => new WorkPlanEntry(x.PlannedDurationMinutes!.Value, x.DurationMinutes)));
        var workContextTotal = WorkContextSummaryCalculator.Calculate(activities.Where(x => x.ActivityType.Contains("trabajo", StringComparison.OrdinalIgnoreCase)).Select(x => new WorkContextEntry(x.Date, x.WorkDemands, x.BreakMinutes, x.UnusualConditions)));
        var coverage = sessions.Select(x => x.Date).Concat(activities.Select(x => x.Date)).Concat(checks.Select(x => x.Date)).Concat(knee.Select(x => DateOnly.FromDateTime(x.RecordedAt.LocalDateTime))).Distinct().Count();

        return new(DateTimeOffset.Now, from, to,
            new(profile.DisplayName, profile.HeightCm, profile.ReferenceWeightKg, profile.PrimaryGoal, profile.TargetDate, profile.AffectedKnee),
            new(sessions.Count, sessions.Sum(x => x.ActualDurationMinutes ?? 0), trainingTotal, activities.Sum(x => x.DurationMinutes), externalTotal, trainingTotal + externalTotal, checks.Count, Avg(checks.Select(x => (int?)x.SleepQuality)), Avg(checks.Select(x => (int?)x.Energy)), Avg(checks.Select(x => (int?)x.Fatigue)), Avg(checks.Select(x => (int?)x.Stress)), Avg(checks.Select(x => x.PainIntensity)), reportKnee.Count(x => x.State == "attention")),
            new(workTotal.RecordedDays, workTotal.PlannedMinutes, workTotal.ActualMinutes, workTotal.DifferenceMinutes, workTotal.ExtraMinutes, workTotal.ShorterMinutes, weeks.Count(x => x.WorkRecordedDays > 0), workContextTotal.ContextRecordedDays, workContextTotal.TotalBreakMinutes, workContextTotal.UnusualDays),
            weeks, reportGoals, reportKnee, reportMetrics,
            learning.Select(x => new ReportLearningDto(x.Date, x.Title, x.Observation, x.Interpretation, x.NextAction, x.Confidence, x.Status)).ToArray(), coverage,
            "Informe descriptivo generado con datos personales y autorreportados. No constituye diagnóstico, indicación médica ni autorización para entrenar.");
    }

    public async Task<CreatedReportShareDto> CreateShareAsync(Guid userId, CreateReportShareRequest request, CancellationToken ct = default)
    {
        if (!request.Consent) throw new ArgumentException("Debés confirmar el consentimiento antes de compartir.");
        if (request.ExpiresInDays is < 1 or > 30) throw new ArgumentException("El enlace puede durar entre 1 y 30 días.");
        var profile = await db.AthleteProfiles.SingleOrDefaultAsync(x => x.UserId == userId, ct)
            ?? throw new InvalidOperationException("Primero debe inicializarse el perfil Atlas.");
        var report = await GetProfessionalAsync(userId, request.From, request.To, ct);
        report = report with
        {
            KneeChecks = request.IncludeKnee ? report.KneeChecks : [],
            Learning = request.IncludeLearning ? report.Learning : []
        };
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
        var share = new ReportShare(profile.Id, Hash(token), JsonSerializer.Serialize(report), request.From, request.To, DateTimeOffset.UtcNow.AddDays(request.ExpiresInDays), request.IncludeKnee, request.IncludeLearning, request.RecipientLabel);
        db.ReportShares.Add(share);
        await db.SaveChangesAsync(ct);
        return new(share.Id, token, share.CreatedAt, share.ExpiresAt, share.RecipientLabel, share.IncludeKnee, share.IncludeLearning);
    }

    public async Task<IReadOnlyCollection<ReportShareDto>> ListSharesAsync(Guid userId, CancellationToken ct = default)
    {
        var profileId = await db.AthleteProfiles.Where(x => x.UserId == userId).Select(x => (Guid?)x.Id).SingleOrDefaultAsync(ct);
        if (!profileId.HasValue) return [];
        var shares = await db.ReportShares.AsNoTracking().Where(x => x.AthleteProfileId == profileId).OrderByDescending(x => x.CreatedAt).Take(30).ToArrayAsync(ct);
        var now = DateTimeOffset.UtcNow;
        return shares.Select(x => new ReportShareDto(x.Id, x.From, x.To, x.CreatedAt, x.ExpiresAt, x.RevokedAt, x.RecipientLabel, x.IncludeKnee, x.IncludeLearning, !x.IsActive || x.RevokedAt is not null ? "revoked" : x.ExpiresAt <= now ? "expired" : "active")).ToArray();
    }

    public async Task RevokeShareAsync(Guid userId, Guid shareId, CancellationToken ct = default)
    {
        var profileId = await db.AthleteProfiles.Where(x => x.UserId == userId).Select(x => (Guid?)x.Id).SingleOrDefaultAsync(ct);
        var share = profileId.HasValue ? await db.ReportShares.SingleOrDefaultAsync(x => x.Id == shareId && x.AthleteProfileId == profileId, ct) : null;
        if (share is null) throw new KeyNotFoundException("El enlace no existe.");
        share.Revoke();
        await db.SaveChangesAsync(ct);
    }

    public async Task<SharedProfessionalReportDto?> GetSharedAsync(string token, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(token)) return null;
        var now = DateTimeOffset.UtcNow;
        var hash = Hash(token);
        var share = await db.ReportShares.AsNoTracking().SingleOrDefaultAsync(x => x.TokenHash == hash && x.IsActive && x.RevokedAt == null && x.ExpiresAt > now, ct);
        if (share is null) return null;
        var report = JsonSerializer.Deserialize<ProfessionalReportDto>(share.SnapshotJson);
        return report is null ? null : new(report, share.CreatedAt, share.ExpiresAt, share.RecipientLabel);
    }

    public async Task<CreatedReportFeedbackDto> SubmitFeedbackAsync(string token, CreateReportFeedbackRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(token)) throw new KeyNotFoundException("El enlace no está disponible.");
        var now = DateTimeOffset.UtcNow;
        var hash = Hash(token);
        var share = await db.ReportShares.SingleOrDefaultAsync(x => x.TokenHash == hash && x.IsActive && x.RevokedAt == null && x.ExpiresAt > now, ct)
            ?? throw new KeyNotFoundException("El enlace no existe, venció o fue revocado.");
        if (await db.ReportFeedback.CountAsync(x => x.ReportShareId == share.Id, ct) >= 20)
            throw new InvalidOperationException("Este enlace alcanzó el límite de aportes.");
        var feedback = new ReportFeedback(share.Id, request.AuthorName, request.Kind, request.Section, request.Message);
        db.ReportFeedback.Add(feedback);
        await db.SaveChangesAsync(ct);
        return new(feedback.Id, feedback.CreatedAt);
    }

    public async Task<IReadOnlyCollection<ReportFeedbackDto>> ListFeedbackAsync(Guid userId, CancellationToken ct = default)
    {
        var profileId = await db.AthleteProfiles.Where(x => x.UserId == userId).Select(x => (Guid?)x.Id).SingleOrDefaultAsync(ct);
        if (!profileId.HasValue) return [];
        var shares = await db.ReportShares.AsNoTracking().Where(x => x.AthleteProfileId == profileId).ToArrayAsync(ct);
        var shareIds = shares.Select(x => x.Id).ToArray();
        var feedback = await db.ReportFeedback.AsNoTracking().Where(x => shareIds.Contains(x.ReportShareId)).OrderByDescending(x => x.CreatedAt).Take(100).ToArrayAsync(ct);
        var byId = shares.ToDictionary(x => x.Id);
        return feedback.Select(x =>
        {
            var share = byId[x.ReportShareId];
            return new ReportFeedbackDto(x.Id, x.ReportShareId, x.AuthorName, x.Kind, x.Section, x.Message, x.Status, x.DecisionNote, x.CreatedAt, x.ReviewedAt, share.RecipientLabel, share.From, share.To);
        }).ToArray();
    }

    public async Task ReviewFeedbackAsync(Guid userId, Guid feedbackId, ReviewReportFeedbackRequest request, CancellationToken ct = default)
    {
        var profileId = await db.AthleteProfiles.Where(x => x.UserId == userId).Select(x => (Guid?)x.Id).SingleOrDefaultAsync(ct);
        var feedback = profileId.HasValue
            ? await db.ReportFeedback.SingleOrDefaultAsync(x => x.Id == feedbackId && db.ReportShares.Any(s => s.Id == x.ReportShareId && s.AthleteProfileId == profileId), ct)
            : null;
        if (feedback is null) throw new KeyNotFoundException("El aporte no existe.");
        feedback.Review(request.Status, request.DecisionNote);
        await db.SaveChangesAsync(ct);
    }

    private static string Hash(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))).ToLowerInvariant();
    private static decimal? Avg(IEnumerable<int?> values)
    {
        var present = values.Where(x => x.HasValue).Select(x => x!.Value).ToArray();
        return present.Length == 0 ? null : Math.Round((decimal)present.Average(), 1);
    }
    private static List<string> KneeReasons(KneeCheck check)
    {
        var reasons = new List<string>();
        if (check.Locking) reasons.Add("Bloqueo referido");
        if (check.Instability) reasons.Add("Inestabilidad referida");
        if (!check.FullExtension) reasons.Add("Sin extensión completa referida");
        if (check.Swelling is "moderada" or "alta") reasons.Add($"Inflamación {check.Swelling}");
        if (check.PainWorst24H >= 7) reasons.Add("Dolor máximo alto en 24 h");
        return reasons;
    }
}
