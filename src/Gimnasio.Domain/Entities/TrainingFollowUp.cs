using Gimnasio.Domain.Common;

namespace Gimnasio.Domain.Entities;

public sealed class TrainingFollowUp : Entity
{
    private TrainingFollowUp() { }
    public TrainingFollowUp(Guid trainingSessionId) => TrainingSessionId = trainingSessionId;

    public Guid TrainingSessionId { get; private set; }
    public DateTimeOffset RecordedAt { get; private set; }
    public int Recovery { get; private set; }
    public int? PainIntensity { get; private set; }
    public string? PainLocation { get; private set; }
    public string? Stiffness { get; private set; }
    public string? Swelling { get; private set; }
    public bool Instability { get; private set; }
    public bool Locking { get; private set; }
    public string? Notes { get; private set; }

    public void Record(int recovery, int? painIntensity, string? painLocation, string? stiffness, string? swelling, bool instability, bool locking, string? notes)
    {
        if (recovery is < 1 or > 5) throw new ArgumentOutOfRangeException(nameof(recovery));
        if (painIntensity is < 0 or > 10) throw new ArgumentOutOfRangeException(nameof(painIntensity));
        Recovery = recovery;
        PainIntensity = painIntensity;
        PainLocation = Clean(painLocation);
        Stiffness = Clean(stiffness);
        Swelling = Clean(swelling);
        Instability = instability;
        Locking = locking;
        Notes = Clean(notes);
        RecordedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RestoreRecordedAt(DateTimeOffset recordedAt)
    {
        if (recordedAt == default) throw new ArgumentException("La fecha del seguimiento es obligatoria.", nameof(recordedAt));
        RecordedAt = recordedAt;
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
