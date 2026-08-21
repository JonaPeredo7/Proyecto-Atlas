using Gimnasio.Domain.Entities;

namespace Gimnasio.Tests.Domain;

public sealed class ReportFeedbackTests
{
    [Fact]
    public void ProposalRemainsPendingUntilOwnerReviewsIt()
    {
        var feedback = new ReportFeedback(Guid.NewGuid(), "Dra. Pérez", "proposal", "knee", "Revisar la progresión de carga.");
        Assert.Equal("pending", feedback.Status);
        feedback.Review("incorporated", "Se incorporará en la próxima revisión del plan.");
        Assert.Equal("incorporated", feedback.Status);
        Assert.NotNull(feedback.ReviewedAt);
    }

    [Fact]
    public void FeedbackRejectsUnsupportedDecision()
    {
        var feedback = new ReportFeedback(Guid.NewGuid(), "Profesional", "comment", "general", "Observación descriptiva.");
        Assert.Throws<ArgumentException>(() => feedback.Review("automatic-change", null));
    }

    [Fact]
    public void RestoredFeedbackPreservesTheOriginalDecisionDate()
    {
        var reviewedAt=new DateTimeOffset(2025,5,3,14,20,0,TimeSpan.Zero);var feedback=new ReportFeedback(Guid.NewGuid(),"Kinesióloga","proposal","knee","Mantener la progresión.");
        feedback.RestoreReview("incorporated","Aplicado al plan.",reviewedAt);
        Assert.Equal("incorporated",feedback.Status);Assert.Equal(reviewedAt,feedback.ReviewedAt);Assert.Equal("Aplicado al plan.",feedback.DecisionNote);
    }
}
