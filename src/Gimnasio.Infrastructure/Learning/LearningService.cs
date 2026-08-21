using Gimnasio.Application.Learning;
using Gimnasio.Domain.Entities;
using Gimnasio.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Gimnasio.Infrastructure.Learning;

internal sealed class LearningService(GimnasioDbContext db) : ILearningService
{
    public async Task<LearningOverviewDto> GetAsync(Guid userId, CancellationToken ct = default)
    {
        var profileId = await Profile(userId, ct);
        var entries = await db.LearningEntries.AsNoTracking()
            .Where(x => x.AthleteProfileId == profileId && x.IsActive)
            .OrderByDescending(x => x.Date).ThenByDescending(x => x.CreatedAt).Take(80).ToListAsync(ct);
        var sessions = await db.TrainingSessions.AsNoTracking().Where(x => x.AthleteProfileId == profileId)
            .OrderByDescending(x => x.Date).Take(30).ToListAsync(ct);
        var goals = await db.PersonalGoals.AsNoTracking().Where(x => x.AthleteProfileId == profileId && x.IsActive).ToListAsync(ct);
        var cycles = await db.TrainingCycles.AsNoTracking().Where(x => x.AthleteProfileId == profileId && x.IsActive).ToListAsync(ct);

        return new(
            entries.Select(x => Map(x, sessions, goals, cycles)).ToArray(),
            sessions.Select(x => new LearningOptionDto(x.Id, x.Name, $"{x.Date} · {x.ActivityType}")).ToArray(),
            goals.Select(x => new LearningOptionDto(x.Id, x.Title, x.Category)).ToArray(),
            cycles.Select(x => new LearningOptionDto(x.Id, x.Name, x.Focus)).ToArray(),
            entries.Count(x => x.Status == "open" && !string.IsNullOrWhiteSpace(x.NextAction)),
            entries.Count(x => x.ReviewedOn.HasValue),
            entries.Count(x => x.Date >= DateOnly.FromDateTime(DateTime.Today.AddDays(-29))));
    }

    public async Task<LearningEntryDto> SaveAsync(Guid userId, Guid? id, SaveLearningEntryRequest request, CancellationToken ct = default)
    {
        var profileId = await Profile(userId, ct);
        await Validate(profileId, request, ct);
        var update = id.HasValue;
        LearningEntry entry;
        if (update)
            entry = await db.LearningEntries.SingleOrDefaultAsync(x => x.Id == id && x.AthleteProfileId == profileId && x.IsActive, ct) ?? throw new KeyNotFoundException();
        else
        {
            entry = new(profileId, request.Date, request.Title);
            db.LearningEntries.Add(entry);
        }

        entry.Record(request.Date, request.Title, request.Category, request.Observation, request.Interpretation,
            request.NextAction, request.ReviewDueOn, request.Confidence, request.Status, request.ReviewedOn, request.FollowUpOutcome,
            request.FollowUpObservation, request.TrainingSessionId, request.PersonalGoalId, request.TrainingCycleId, update);
        db.PlanChanges.Add(new(profileId, "learning", entry.Id, entry.Version, request.ChangeReason, $"{entry.Title} · {entry.Status}"));
        await db.SaveChangesAsync(ct);

        var sessions = await db.TrainingSessions.AsNoTracking().Where(x => x.Id == entry.TrainingSessionId).ToListAsync(ct);
        var goals = await db.PersonalGoals.AsNoTracking().Where(x => x.Id == entry.PersonalGoalId).ToListAsync(ct);
        var cycles = await db.TrainingCycles.AsNoTracking().Where(x => x.Id == entry.TrainingCycleId).ToListAsync(ct);
        return Map(entry, sessions, goals, cycles);
    }

    public async Task<bool> DeleteAsync(Guid userId, Guid id, CancellationToken ct = default)
    {
        var profileId = await Profile(userId, ct);
        var entry = await db.LearningEntries.SingleOrDefaultAsync(x => x.Id == id && x.AthleteProfileId == profileId && x.IsActive, ct);
        if (entry is null) return false;
        entry.Remove();
        await db.SaveChangesAsync(ct);
        return true;
    }

    private async Task Validate(Guid profileId, SaveLearningEntryRequest request, CancellationToken ct)
    {
        if (request.TrainingSessionId.HasValue && !await db.TrainingSessions.AnyAsync(x => x.Id == request.TrainingSessionId && x.AthleteProfileId == profileId, ct))
            throw new ArgumentException("La sesión no pertenece al perfil.");
        if (request.PersonalGoalId.HasValue && !await db.PersonalGoals.AnyAsync(x => x.Id == request.PersonalGoalId && x.AthleteProfileId == profileId && x.IsActive, ct))
            throw new ArgumentException("El objetivo no pertenece al perfil.");
        if (request.TrainingCycleId.HasValue && !await db.TrainingCycles.AnyAsync(x => x.Id == request.TrainingCycleId && x.AthleteProfileId == profileId && x.IsActive, ct))
            throw new ArgumentException("El ciclo no pertenece al perfil.");
    }

    private async Task<Guid> Profile(Guid userId, CancellationToken ct) =>
        (await db.AthleteProfiles.AsNoTracking().SingleOrDefaultAsync(x => x.UserId == userId, ct))?.Id
        ?? throw new InvalidOperationException("Primero debe inicializarse el perfil Atlas.");

    private static LearningEntryDto Map(LearningEntry entry, IEnumerable<TrainingSession> sessions,
        IEnumerable<PersonalGoal> goals, IEnumerable<TrainingCycle> cycles) => new(
            entry.Id, entry.Date, entry.Title, entry.Category, entry.Observation, entry.Interpretation, entry.NextAction, entry.ReviewDueOn,
            entry.Confidence, entry.Status, entry.ReviewedOn, entry.FollowUpOutcome, entry.FollowUpObservation,
            entry.TrainingSessionId, sessions.FirstOrDefault(x => x.Id == entry.TrainingSessionId)?.Name,
            entry.PersonalGoalId, goals.FirstOrDefault(x => x.Id == entry.PersonalGoalId)?.Title,
            entry.TrainingCycleId, cycles.FirstOrDefault(x => x.Id == entry.TrainingCycleId)?.Name, entry.Version);
}
