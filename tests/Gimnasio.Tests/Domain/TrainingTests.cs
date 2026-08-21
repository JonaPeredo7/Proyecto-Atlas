using Gimnasio.Domain.Entities;
using Gimnasio.Domain.Enums;

namespace Gimnasio.Tests.Domain;

public sealed class TrainingTests
{
    [Fact]
    public void Session_RecordsPlanAndCompletion()
    {
        var session = new TrainingSession(Guid.NewGuid(), new DateOnly(2026, 8, 13), "Fuerza", "Fuerza");
        session.Configure(60, 6, "Técnica", null);
        session.Start();
        session.Complete(55, 7, "Buena tolerancia");

        Assert.Equal(TrainingSessionStatus.Completed, session.Status);
        Assert.Equal(55, session.ActualDurationMinutes);
        Assert.Equal(7, session.SessionRpe);
    }

    [Fact]
    public void Session_RejectsInvalidRpe()
    {
        var session = new TrainingSession(Guid.NewGuid(), new DateOnly(2026, 8, 13), "Fuerza", "Fuerza");
        Assert.Throws<ArgumentOutOfRangeException>(() => session.Configure(60, 11, null, null));
    }

    [Fact]
    public void Session_CanKeepOptionalPlannedStartTime()
    {
        var session = new TrainingSession(Guid.NewGuid(), new DateOnly(2026, 9, 2), "Gimnasio", "Fuerza");
        session.Schedule(new TimeOnly(9, 0));
        Assert.Equal(new TimeOnly(9, 0), session.PlannedStartTime);
    }

    [Fact]
    public void Session_CanLinkPlanAndVersionPrescription()
    {
        var session = new TrainingSession(Guid.NewGuid(), new DateOnly(2026, 8, 13), "Fuerza", "Fuerza");
        var goalId = Guid.NewGuid(); var cycleId = Guid.NewGuid();
        session.LinkPlan(goalId, cycleId, false);
        session.MarkPrescriptionChange();
        Assert.Equal(goalId, session.PersonalGoalId); Assert.Equal(cycleId, session.TrainingCycleId); Assert.Equal(2, session.Version);
    }

    [Fact]
    public void Session_RejectsPlanCorrectionAfterStarting()
    {
        var session = new TrainingSession(Guid.NewGuid(), new DateOnly(2026, 8, 13), "Fuerza", "Fuerza"); session.Start();
        Assert.Throws<InvalidOperationException>(() => session.Revise(new DateOnly(2026, 8, 14), "Otro", "Cardio"));
    }

    [Fact]
    public void Exercise_KeepsPlanSeparateFromResult()
    {
        var exercise = new TrainingExercise(Guid.NewGuid(), 1, "Sentadilla");
        exercise.Prescribe("Fuerza", 4, "8", 50, 120, null);
        exercise.Record(3, "8, 8, 7", 50, 8);

        Assert.Equal(4, exercise.PlannedSets);
        Assert.Equal(3, exercise.CompletedSets);
        Assert.True(exercise.IsCompleted);
    }

    [Fact]
    public void Exercise_CopiedPrescription_StartsWithoutResults()
    {
        var source = new TrainingExercise(Guid.NewGuid(), 1, "Sentadilla");
        source.Prescribe("Fuerza", 4, "8", 50, 120, "Técnica controlada");
        source.Record(3, "8, 8, 7", 50, 8);
        var copy = new TrainingExercise(Guid.NewGuid(), source.Order, source.Name);
        copy.Prescribe(source.Category, source.PlannedSets, source.PlannedRepetitions, source.PlannedLoadKg, source.RestSeconds, source.Notes);

        Assert.Equal(source.PlannedSets, copy.PlannedSets);
        Assert.Null(copy.CompletedSets);
        Assert.Null(copy.ExerciseRpe);
        Assert.False(copy.IsCompleted);
    }

    [Fact]
    public void RecurringSchedule_RequiresValidExactTimes()
    {
        var block = new RecurringScheduleBlock(Guid.NewGuid(), 2, "Taekwondo", "training");
        block.Configure("exact", new TimeOnly(19, 0), new TimeOnly(20, 30), new DateOnly(2026, 8, 15), null, null);
        Assert.Equal(new TimeOnly(20, 30), block.EndTime);
        Assert.Throws<ArgumentException>(() => block.Configure("exact", new TimeOnly(20, 0), new TimeOnly(19, 0), new DateOnly(2026, 8, 15), null, null));
    }

    [Fact]
    public void FollowUp_RecordsPostTrainingSymptoms()
    {
        var followUp = new TrainingFollowUp(Guid.NewGuid());
        followUp.Record(3, 2, "Rodilla izquierda", "leve", "ninguna", false, false, "Sin aumento relevante");

        Assert.Equal(2, followUp.PainIntensity);
        Assert.False(followUp.Instability);
        Assert.NotEqual(default, followUp.RecordedAt);
    }
}
