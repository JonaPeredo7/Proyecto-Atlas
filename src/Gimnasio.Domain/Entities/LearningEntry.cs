using Gimnasio.Domain.Common;

namespace Gimnasio.Domain.Entities;

public sealed class LearningEntry : Entity
{
    private LearningEntry() { }

    public LearningEntry(Guid profileId, DateOnly date, string title)
    {
        AthleteProfileId = profileId;
        Date = date;
        Title = Required(title);
    }

    public Guid AthleteProfileId { get; private set; }
    public DateOnly Date { get; private set; }
    public string Title { get; private set; } = "";
    public string Category { get; private set; } = "";
    public string Observation { get; private set; } = "";
    public string? Interpretation { get; private set; }
    public string? NextAction { get; private set; }
    public DateOnly? ReviewDueOn { get; private set; }
    public string Confidence { get; private set; } = "low";
    public string Status { get; private set; } = "open";
    public DateOnly? ReviewedOn { get; private set; }
    public string? FollowUpOutcome { get; private set; }
    public string? FollowUpObservation { get; private set; }
    public Guid? TrainingSessionId { get; private set; }
    public Guid? PersonalGoalId { get; private set; }
    public Guid? TrainingCycleId { get; private set; }
    public int Version { get; private set; } = 1;

    public void Record(DateOnly date, string title, string category, string observation, string? interpretation,
        string? nextAction, DateOnly? reviewDueOn, string confidence, string status, DateOnly? reviewedOn, string? followUpOutcome,
        string? followUpObservation, Guid? sessionId, Guid? goalId, Guid? cycleId, bool update)
    {
        var cleanAction = Clean(nextAction);
        var cleanOutcome = Clean(followUpOutcome);
        var cleanFollowUp = Clean(followUpObservation);
        var cleanStatus = Allowed(status, ["open", "applied", "archived"]);
        var hasAnyReview = reviewedOn.HasValue || cleanOutcome is not null || cleanFollowUp is not null;
        var hasCompleteReview = reviewedOn.HasValue && cleanOutcome is not null && cleanFollowUp is not null;

        if (reviewedOn < date) throw new ArgumentException("La revisión no puede ser anterior a la observación.");
        if (reviewDueOn < date) throw new ArgumentException("La fecha prevista de revisión no puede ser anterior a la observación.");
        if (hasAnyReview && !hasCompleteReview) throw new ArgumentException("El seguimiento necesita fecha, resultado y observación.");
        if (cleanStatus == "applied" && (cleanAction is null || !hasCompleteReview))
            throw new ArgumentException("Para marcar una acción como aplicada, registrá la acción y su seguimiento.");

        Date = date;
        Title = Required(title);
        Category = Required(category);
        Observation = Required(observation);
        Interpretation = Clean(interpretation);
        NextAction = cleanAction;
        ReviewDueOn = reviewDueOn;
        Confidence = Allowed(confidence, ["low", "medium", "high"]);
        Status = cleanStatus;
        ReviewedOn = reviewedOn;
        FollowUpOutcome = cleanOutcome is null ? null : Allowed(cleanOutcome, ["helpful", "neutral", "not-helpful", "inconclusive"]);
        FollowUpObservation = cleanFollowUp;
        TrainingSessionId = sessionId;
        PersonalGoalId = goalId;
        TrainingCycleId = cycleId;
        if (update) Version++;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RestoreVersion(int version) { if (version < 1) throw new ArgumentOutOfRangeException(nameof(version)); Version = version; }
    public void RestoreHistoricalState(int version, string status, DateOnly? reviewDueOn, DateOnly? reviewedOn, string? followUpOutcome, string? followUpObservation)
    {
        RestoreVersion(version);
        Status = Allowed(status, ["open", "applied", "archived"]);
        ReviewDueOn = reviewDueOn;
        ReviewedOn = reviewedOn;
        FollowUpOutcome = string.IsNullOrWhiteSpace(followUpOutcome) ? null : Allowed(followUpOutcome, ["helpful", "neutral", "not-helpful", "inconclusive"]);
        FollowUpObservation = Clean(followUpObservation);
    }
    public void Remove() { IsActive = false; UpdatedAt = DateTimeOffset.UtcNow; }
    private static string Required(string value) => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("El valor es obligatorio.") : value.Trim();
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static string Allowed(string value, string[] allowed) => allowed.Contains(value) ? value : throw new ArgumentException("El valor seleccionado no es válido.");
}
