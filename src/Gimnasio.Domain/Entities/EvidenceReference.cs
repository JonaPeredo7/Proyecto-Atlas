using Gimnasio.Domain.Common;
using Gimnasio.Domain.Enums;

namespace Gimnasio.Domain.Entities;

public sealed class EvidenceReference : Entity
{
    private EvidenceReference() { }

    public string Topic { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string? PermanentId { get; private set; }
    public string? SourceUrl { get; private set; }
    public EvidenceLevel Level { get; private set; }
    public string Status { get; private set; } = "draft";
    public DateOnly? PublishedOn { get; private set; }
    public DateOnly? NextReviewOn { get; private set; }
    public string? Applicability { get; private set; }
    public string? Limitations { get; private set; }
}
