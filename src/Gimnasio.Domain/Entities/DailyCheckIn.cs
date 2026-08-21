using Gimnasio.Domain.Common;

namespace Gimnasio.Domain.Entities;

public sealed class DailyCheckIn : Entity
{
    private DailyCheckIn() { }

    public DailyCheckIn(Guid athleteProfileId, DateOnly date)
    {
        AthleteProfileId = athleteProfileId;
        Date = date;
    }

    public Guid AthleteProfileId { get; private set; }
    public DateOnly Date { get; private set; }
    public int? SleepMinutes { get; private set; }
    public int SleepQuality { get; private set; }
    public int Energy { get; private set; }
    public int Fatigue { get; private set; }
    public int Stress { get; private set; }
    public string? PainLocation { get; private set; }
    public string? PainSide { get; private set; }
    public int? PainIntensity { get; private set; }
    public string? Stiffness { get; private set; }
    public string? Swelling { get; private set; }
    public bool Instability { get; private set; }
    public bool Locking { get; private set; }
    public int ExpectedWorkLoad { get; private set; }
    public decimal? PlannedCyclingKm { get; private set; }
    public string? PlannedActivity { get; private set; }
    public string? Notes { get; private set; }

    public void Record(
        int? sleepMinutes,
        int sleepQuality,
        int energy,
        int fatigue,
        int stress,
        string? painLocation,
        string? painSide,
        int? painIntensity,
        string? stiffness,
        string? swelling,
        bool instability,
        bool locking,
        int expectedWorkLoad,
        decimal? plannedCyclingKm,
        string? plannedActivity,
        string? notes)
    {
        SleepMinutes = Range(sleepMinutes, 0, 1440, nameof(sleepMinutes));
        SleepQuality = Range(sleepQuality, 1, 5, nameof(sleepQuality));
        Energy = Range(energy, 1, 5, nameof(energy));
        Fatigue = Range(fatigue, 0, 10, nameof(fatigue));
        Stress = Range(stress, 1, 5, nameof(stress));
        PainIntensity = Range(painIntensity, 0, 10, nameof(painIntensity));
        ExpectedWorkLoad = Range(expectedWorkLoad, 0, 10, nameof(expectedWorkLoad));
        PlannedCyclingKm = plannedCyclingKm is null || plannedCyclingKm is >= 0 and <= 500
            ? plannedCyclingKm
            : throw new ArgumentOutOfRangeException(nameof(plannedCyclingKm));
        PainLocation = Clean(painLocation);
        PainSide = Clean(painSide);
        Stiffness = Clean(stiffness);
        Swelling = Clean(swelling);
        Instability = instability;
        Locking = locking;
        PlannedActivity = Clean(plannedActivity);
        Notes = Clean(notes);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static int Range(int value, int min, int max, string name) =>
        value >= min && value <= max ? value : throw new ArgumentOutOfRangeException(name);

    private static int? Range(int? value, int min, int max, string name) =>
        value is null ? null : Range(value.Value, min, max, name);

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
