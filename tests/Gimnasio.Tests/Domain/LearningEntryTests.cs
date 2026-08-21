using Gimnasio.Domain.Entities;

namespace Gimnasio.Tests.Domain;

public sealed class LearningEntryTests
{
    [Fact]
    public void EntrySeparatesObservationFromInterpretation()
    {
        var entry = new LearningEntry(Guid.NewGuid(), new(2026, 8, 13), "Sentadilla");
        entry.Record(new(2026, 8, 13), "Sentadilla", "Técnica", "Completé 3 series sin dolor",
            "La profundidad podría ser tolerable", "Repetir con la misma carga", null, "medium", "open",
            null, null, null, null, null, null, false);
        Assert.Equal("Completé 3 series sin dolor", entry.Observation);
        Assert.Equal("medium", entry.Confidence);
    }

    [Fact]
    public void EntryRejectsEmptyObservation()
    {
        var entry = new LearningEntry(Guid.NewGuid(), new(2026, 8, 13), "Prueba");
        Assert.Throws<ArgumentException>(() => entry.Record(new(2026, 8, 13), "Prueba", "Técnica", "",
            null, null, null, "low", "open", null, null, null, null, null, null, false));
    }

    [Fact]
    public void AppliedActionRequiresCompleteFollowUp()
    {
        var entry = new LearningEntry(Guid.NewGuid(), new(2026, 8, 13), "Carga");
        Assert.Throws<ArgumentException>(() => entry.Record(new(2026, 8, 13), "Carga", "Entrenamiento", "Bajé la carga",
            null, "Repetir la carga", null, "low", "applied", new(2026, 8, 14), null, null, null, null, null, false));
    }

    [Fact]
    public void FollowUpPreservesObservationWithoutClaimingCausality()
    {
        var entry = new LearningEntry(Guid.NewGuid(), new(2026, 8, 13), "Carga");
        entry.Record(new(2026, 8, 13), "Carga", "Entrenamiento", "Bajé la carga", null, "Repetir la carga", null,
            "low", "applied", new(2026, 8, 14), "helpful", "El dolor fue menor al día siguiente", null, null, null, false);
        Assert.Equal("helpful", entry.FollowUpOutcome);
        Assert.Equal(new DateOnly(2026, 8, 14), entry.ReviewedOn);
        Assert.Equal("El dolor fue menor al día siguiente", entry.FollowUpObservation);
    }

    [Fact]
    public void ReviewCannotPrecedeOriginalObservation()
    {
        var entry = new LearningEntry(Guid.NewGuid(), new(2026, 8, 13), "Carga");
        Assert.Throws<ArgumentException>(() => entry.Record(new(2026, 8, 13), "Carga", "Entrenamiento", "Observación",
            null, "Acción", null, "low", "applied", new(2026, 8, 12), "neutral", "Sin cambios", null, null, null, false));
    }

    [Fact]
    public void PlannedReviewCannotPrecedeOriginalObservation()
    {
        var entry = new LearningEntry(Guid.NewGuid(), new(2026, 8, 13), "Carga");
        Assert.Throws<ArgumentException>(() => entry.Record(new(2026, 8, 13), "Carga", "Entrenamiento", "Observación",
            null, "Acción", new(2026, 8, 12), "low", "open", null, null, null, null, null, null, false));
    }

    [Fact]
    public void RestoredEntryPreservesItsHistoricalVersion()
    {
        var entry = new LearningEntry(Guid.NewGuid(), new(2026, 8, 13), "Técnica");
        entry.RestoreVersion(6);
        Assert.Equal(6, entry.Version);
        Assert.Throws<ArgumentOutOfRangeException>(() => entry.RestoreVersion(0));
    }
}
