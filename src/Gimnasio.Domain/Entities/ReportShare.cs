using Gimnasio.Domain.Common;

namespace Gimnasio.Domain.Entities;

public sealed class ReportShare : Entity
{
    private ReportShare() { }

    public ReportShare(
        Guid athleteProfileId,
        string tokenHash,
        string snapshotJson,
        DateOnly from,
        DateOnly to,
        DateTimeOffset expiresAt,
        bool includeKnee,
        bool includeLearning,
        string? recipientLabel)
    {
        if (expiresAt <= DateTimeOffset.UtcNow) throw new ArgumentException("El vencimiento debe ser futuro.");
        AthleteProfileId = athleteProfileId;
        TokenHash = Required(tokenHash);
        SnapshotJson = Required(snapshotJson);
        From = from;
        To = to;
        ExpiresAt = expiresAt;
        IncludeKnee = includeKnee;
        IncludeLearning = includeLearning;
        RecipientLabel = Clean(recipientLabel);
        ConsentGrantedAt = DateTimeOffset.UtcNow;
    }

    public Guid AthleteProfileId { get; private set; }
    public string TokenHash { get; private set; } = "";
    public string SnapshotJson { get; private set; } = "";
    public DateOnly From { get; private set; }
    public DateOnly To { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset ConsentGrantedAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public bool IncludeKnee { get; private set; }
    public bool IncludeLearning { get; private set; }
    public string? RecipientLabel { get; private set; }

    public bool IsAvailable(DateTimeOffset now) => IsActive && RevokedAt is null && ExpiresAt > now;

    public static ReportShare RestoreHistorical(
        Guid athleteProfileId,
        string archivedTokenHash,
        string snapshotJson,
        DateOnly from,
        DateOnly to,
        DateTimeOffset expiresAt,
        DateTimeOffset consentGrantedAt,
        DateTimeOffset? revokedAt,
        bool includeKnee,
        bool includeLearning,
        string? recipientLabel)
    {
        if (athleteProfileId == Guid.Empty) throw new ArgumentException("El perfil es obligatorio.", nameof(athleteProfileId));
        if (to < from) throw new ArgumentException("La fecha final no puede ser anterior a la inicial.");
        if (consentGrantedAt == default) throw new ArgumentException("La fecha de consentimiento es obligatoria.", nameof(consentGrantedAt));
        return new ReportShare
        {
            AthleteProfileId = athleteProfileId,
            TokenHash = Required(archivedTokenHash),
            SnapshotJson = Required(snapshotJson),
            From = from,
            To = to,
            ExpiresAt = expiresAt,
            ConsentGrantedAt = consentGrantedAt,
            RevokedAt = revokedAt ?? DateTimeOffset.UtcNow,
            IncludeKnee = includeKnee,
            IncludeLearning = includeLearning,
            RecipientLabel = Clean(recipientLabel),
            IsActive = false
        };
    }

    public void Revoke()
    {
        if (!IsActive) return;
        IsActive = false;
        RevokedAt = DateTimeOffset.UtcNow;
        UpdatedAt = RevokedAt;
    }

    private static string Required(string value) =>
        string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("El valor es obligatorio.") : value.Trim();

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
