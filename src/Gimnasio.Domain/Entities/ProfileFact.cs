using Gimnasio.Domain.Common;
using Gimnasio.Domain.Enums;

namespace Gimnasio.Domain.Entities;

public sealed class ProfileFact : Entity
{
    private ProfileFact() { }

    public ProfileFact(
        Guid athleteProfileId,
        string category,
        string label,
        string? value,
        ProfileFactStatus status,
        string sourceTitle,
        string? notes = null)
    {
        AthleteProfileId = athleteProfileId;
        Category = category.Trim();
        Label = label.Trim();
        Value = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        Status = status;
        SourceTitle = sourceTitle.Trim();
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }

    public Guid AthleteProfileId { get; private set; }
    public string Category { get; private set; } = string.Empty;
    public string Label { get; private set; } = string.Empty;
    public string? Value { get; private set; }
    public ProfileFactStatus Status { get; private set; }
    public string SourceTitle { get; private set; } = string.Empty;
    public string? Notes { get; private set; }
}
