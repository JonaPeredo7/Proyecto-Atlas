using Gimnasio.Domain.Common;

namespace Gimnasio.Domain.Entities;

public sealed class AthleteProfile : Entity
{
    private AthleteProfile() { }

    public AthleteProfile(Guid userId, string displayName)
    {
        UserId = userId;
        DisplayName = Require(displayName, nameof(displayName));
    }

    public Guid UserId { get; private set; }
    public string DisplayName { get; private set; } = string.Empty;
    public decimal? HeightCm { get; private set; }
    public decimal? ReferenceWeightKg { get; private set; }
    public string? PrimaryGoal { get; private set; }
    public DateOnly? TargetDate { get; private set; }
    public string? DominantHand { get; private set; }
    public string? DominantLeg { get; private set; }
    public string? AffectedKnee { get; private set; }

    public void Update(
        string displayName,
        decimal? heightCm,
        decimal? referenceWeightKg,
        string? primaryGoal,
        DateOnly? targetDate,
        string? dominantHand,
        string? dominantLeg,
        string? affectedKnee)
    {
        DisplayName = Require(displayName, nameof(displayName));
        HeightCm = InRange(heightCm, 80, 260, nameof(heightCm));
        ReferenceWeightKg = InRange(referenceWeightKg, 20, 400, nameof(referenceWeightKg));
        PrimaryGoal = Clean(primaryGoal);
        TargetDate = targetDate;
        DominantHand = Clean(dominantHand);
        DominantLeg = Clean(dominantLeg);
        AffectedKnee = Clean(affectedKnee);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static string Require(string value, string name) =>
        string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("El valor es obligatorio.", name) : value.Trim();

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static decimal? InRange(decimal? value, decimal min, decimal max, string name) =>
        value is null || value >= min && value <= max
            ? value
            : throw new ArgumentOutOfRangeException(name, $"El valor debe estar entre {min} y {max}.");
}
