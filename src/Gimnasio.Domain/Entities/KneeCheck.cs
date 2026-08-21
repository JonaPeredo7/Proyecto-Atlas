using Gimnasio.Domain.Common;

namespace Gimnasio.Domain.Entities;

public sealed class KneeCheck : Entity
{
    private KneeCheck() { }
    public KneeCheck(Guid athleteProfileId) => AthleteProfileId = athleteProfileId;

    public Guid AthleteProfileId { get; private set; }
    public DateTimeOffset RecordedAt { get; private set; }
    public string Context { get; private set; } = string.Empty;
    public string Side { get; private set; } = string.Empty;
    public int PainNow { get; private set; }
    public int PainBest24H { get; private set; }
    public int PainWorst24H { get; private set; }
    public string Swelling { get; private set; } = string.Empty;
    public bool Instability { get; private set; }
    public bool Locking { get; private set; }
    public bool FullExtension { get; private set; }
    public int WalkingCapacity { get; private set; }
    public int StairsCapacity { get; private set; }
    public int SquatCapacity { get; private set; }
    public string? Notes { get; private set; }

    public void Record(DateTimeOffset recordedAt, string context, string side, int painNow, int painBest24H, int painWorst24H, string swelling, bool instability, bool locking, bool fullExtension, int walkingCapacity, int stairsCapacity, int squatCapacity, string? notes)
    {
        if (painBest24H > painWorst24H) throw new ArgumentException("El dolor mínimo no puede superar al máximo de las últimas 24 horas.");
        RecordedAt = recordedAt;
        Context = Required(context, nameof(context)); Side = Required(side, nameof(side));
        PainNow = Range(painNow, 0, 10, nameof(painNow)); PainBest24H = Range(painBest24H, 0, 10, nameof(painBest24H)); PainWorst24H = Range(painWorst24H, 0, 10, nameof(painWorst24H));
        Swelling = Required(swelling, nameof(swelling)); Instability = instability; Locking = locking; FullExtension = fullExtension;
        WalkingCapacity = Range(walkingCapacity, 0, 10, nameof(walkingCapacity)); StairsCapacity = Range(stairsCapacity, 0, 10, nameof(stairsCapacity)); SquatCapacity = Range(squatCapacity, 0, 10, nameof(squatCapacity));
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(); UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Remove() { IsActive = false; UpdatedAt = DateTimeOffset.UtcNow; }
    private static int Range(int value, int min, int max, string name) => value >= min && value <= max ? value : throw new ArgumentOutOfRangeException(name);
    private static string Required(string value, string name) => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("El valor es obligatorio.", name) : value.Trim();
}
