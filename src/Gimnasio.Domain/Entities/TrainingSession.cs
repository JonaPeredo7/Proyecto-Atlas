using Gimnasio.Domain.Common;
using Gimnasio.Domain.Enums;

namespace Gimnasio.Domain.Entities;

public sealed class TrainingSession : Entity
{
    private TrainingSession() { }

    public TrainingSession(Guid athleteProfileId, DateOnly date, string name, string activityType)
    {
        AthleteProfileId = athleteProfileId;
        Date = date;
        Name = Required(name, nameof(name));
        ActivityType = Required(activityType, nameof(activityType));
    }

    public Guid AthleteProfileId { get; private set; }
    public Guid? PersonalGoalId { get; private set; }
    public Guid? TrainingCycleId { get; private set; }
    public int Version { get; private set; } = 1;
    public DateOnly Date { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string ActivityType { get; private set; } = string.Empty;
    public TimeOnly? PlannedStartTime { get; private set; }
    public int? PlannedDurationMinutes { get; private set; }
    public int? TargetRpe { get; private set; }
    public string? Goal { get; private set; }
    public string? Notes { get; private set; }
    public TrainingSessionStatus Status { get; private set; } = TrainingSessionStatus.Planned;
    public int? ActualDurationMinutes { get; private set; }
    public int? SessionRpe { get; private set; }
    public string? CompletionNotes { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    public void Configure(int? plannedDurationMinutes, int? targetRpe, string? goal, string? notes)
    {
        if (Status is TrainingSessionStatus.Completed or TrainingSessionStatus.Cancelled)
            throw new InvalidOperationException("La sesión ya está cerrada.");

        PlannedDurationMinutes = Range(plannedDurationMinutes, 1, 600, nameof(plannedDurationMinutes));
        TargetRpe = Range(targetRpe, 1, 10, nameof(targetRpe));
        Goal = Clean(goal);
        Notes = Clean(notes);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Schedule(TimeOnly? plannedStartTime)
    {
        if (Status is TrainingSessionStatus.Completed or TrainingSessionStatus.Cancelled) throw new InvalidOperationException("La sesión ya está cerrada.");
        PlannedStartTime = plannedStartTime;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void LinkPlan(Guid? personalGoalId, Guid? trainingCycleId, bool newVersion)
    {
        if (Status is TrainingSessionStatus.Completed or TrainingSessionStatus.Cancelled) throw new InvalidOperationException("La sesión ya está cerrada.");
        PersonalGoalId = personalGoalId; TrainingCycleId = trainingCycleId; if (newVersion) Version++; UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Revise(DateOnly date, string name, string activityType)
    {
        if (Status != TrainingSessionStatus.Planned) throw new InvalidOperationException("Los datos planificados sólo pueden corregirse antes de iniciar la sesión.");
        Date = date; Name = Required(name, nameof(name)); ActivityType = Required(activityType, nameof(activityType)); UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkPrescriptionChange() { if (Status is TrainingSessionStatus.Completed or TrainingSessionStatus.Cancelled) throw new InvalidOperationException("La sesión ya está cerrada."); Version++; UpdatedAt = DateTimeOffset.UtcNow; }

    public void Start()
    {
        if (Status != TrainingSessionStatus.Planned)
            throw new InvalidOperationException("Sólo se puede iniciar una sesión planificada.");
        Status = TrainingSessionStatus.InProgress;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Complete(int actualDurationMinutes, int sessionRpe, string? completionNotes)
    {
        if (Status is TrainingSessionStatus.Completed or TrainingSessionStatus.Cancelled)
            throw new InvalidOperationException("La sesión ya está cerrada.");

        ActualDurationMinutes = Range(actualDurationMinutes, 1, 600, nameof(actualDurationMinutes));
        SessionRpe = Range(sessionRpe, 1, 10, nameof(sessionRpe));
        CompletionNotes = Clean(completionNotes);
        Status = TrainingSessionStatus.Completed;
        CompletedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RestoreState(TrainingSessionStatus status, int version, int? actualDurationMinutes, int? sessionRpe, string? completionNotes, DateTimeOffset? completedAt)
    {
        if (!Enum.IsDefined(status)) throw new ArgumentOutOfRangeException(nameof(status));
        if (version < 1) throw new ArgumentOutOfRangeException(nameof(version));
        actualDurationMinutes = Range(actualDurationMinutes, 1, 600, nameof(actualDurationMinutes));
        sessionRpe = Range(sessionRpe, 1, 10, nameof(sessionRpe));
        if (status == TrainingSessionStatus.Completed && (actualDurationMinutes is null || sessionRpe is null || completedAt is null))
            throw new ArgumentException("Una sesión completada debe conservar duración, esfuerzo y fecha de cierre.");
        if (status != TrainingSessionStatus.Completed && completedAt is not null)
            throw new ArgumentException("Sólo una sesión completada puede tener fecha de cierre.");

        Status = status;
        Version = version;
        ActualDurationMinutes = actualDurationMinutes;
        SessionRpe = sessionRpe;
        CompletionNotes = Clean(completionNotes);
        CompletedAt = completedAt;
    }

    private static string Required(string value, string name) =>
        string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("El valor es obligatorio.", name) : value.Trim();
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static int? Range(int? value, int min, int max, string name) =>
        value is null || value >= min && value <= max ? value : throw new ArgumentOutOfRangeException(name);
    private static int Range(int value, int min, int max, string name) =>
        value >= min && value <= max ? value : throw new ArgumentOutOfRangeException(name);
}
