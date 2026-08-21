using Gimnasio.Domain.Common;

namespace Gimnasio.Domain.Entities;

public sealed class ReportFeedback : Entity
{
    private static readonly string[] Kinds = ["comment", "proposal"];
    private static readonly string[] Sections = ["general", "activity", "goals", "knee", "learning"];
    private static readonly string[] Decisions = ["reviewed", "incorporated", "dismissed"];
    private ReportFeedback() { }

    public ReportFeedback(Guid reportShareId, string authorName, string kind, string section, string message)
    {
        ReportShareId = reportShareId;
        AuthorName = Required(authorName, 120);
        Kind = Allowed(kind, Kinds);
        Section = Allowed(section, Sections);
        Message = Required(message, 1600);
    }

    public Guid ReportShareId { get; private set; }
    public string AuthorName { get; private set; } = "";
    public string Kind { get; private set; } = "comment";
    public string Section { get; private set; } = "general";
    public string Message { get; private set; } = "";
    public string Status { get; private set; } = "pending";
    public string? DecisionNote { get; private set; }
    public DateTimeOffset? ReviewedAt { get; private set; }

    public void Review(string status, string? decisionNote)
    {
        Status = Allowed(status, Decisions);
        DecisionNote = Clean(decisionNote, 1000);
        ReviewedAt = DateTimeOffset.UtcNow;
        UpdatedAt = ReviewedAt;
    }

    public void RestoreReview(string status, string? decisionNote, DateTimeOffset? reviewedAt)
    {
        if (status == "pending")
        {
            if (reviewedAt is not null || !string.IsNullOrWhiteSpace(decisionNote))
                throw new ArgumentException("Un aporte pendiente no puede contener una decisión final.");
            Status = status;
            DecisionNote = null;
            ReviewedAt = null;
            return;
        }

        Status = Allowed(status, Decisions);
        if (reviewedAt is null) throw new ArgumentException("La decisión debe conservar su fecha de revisión.", nameof(reviewedAt));
        DecisionNote = Clean(decisionNote, 1000);
        ReviewedAt = reviewedAt;
    }

    private static string Required(string value, int max) =>
        string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("El valor es obligatorio.") : value.Trim().Length > max ? throw new ArgumentException($"El texto admite hasta {max} caracteres.") : value.Trim();
    private static string Allowed(string value, string[] allowed) => allowed.Contains(value) ? value : throw new ArgumentException("El valor seleccionado no es válido.");
    private static string? Clean(string? value, int max) => string.IsNullOrWhiteSpace(value) ? null : value.Trim().Length > max ? throw new ArgumentException($"El texto admite hasta {max} caracteres.") : value.Trim();
}
