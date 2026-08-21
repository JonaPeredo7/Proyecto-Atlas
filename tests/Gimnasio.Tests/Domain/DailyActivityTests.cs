using Gimnasio.Domain.Entities;

namespace Gimnasio.Tests.Domain;

public sealed class DailyActivityTests
{
    [Fact]
    public void Activity_KeepsPlannedDurationSeparateFromActualDuration()
    {
        var activity = new DailyActivity(Guid.NewGuid(), new DateOnly(2026, 8, 17), "Trabajo físico");
        activity.Record(new DateOnly(2026, 8, 17), "Trabajo físico", 480, 5, null, null);
        activity.AttachPlanSnapshot(420, "Trabajo 13:00–20:00");
        Assert.Equal(480, activity.DurationMinutes);
        Assert.Equal(420, activity.PlannedDurationMinutes);
    }

    [Fact]
    public void Record_CalculatesInternalLoad()
    {
        var activity = new DailyActivity(Guid.NewGuid(), new DateOnly(2026, 8, 13), "Trabajo");
        activity.Record(new DateOnly(2026, 8, 13), "Trabajo", 180, 4, null, "Limpieza y escaleras");

        Assert.Equal(720, activity.InternalLoad);
        Assert.Equal("Trabajo", activity.ActivityType);
    }

    [Fact]
    public void WorkContext_KeepsDemandBreaksAndUnusualConditions()
    {
        var activity = new DailyActivity(Guid.NewGuid(), new DateOnly(2026, 8, 17), "Trabajo físico");
        activity.Record(new DateOnly(2026, 8, 17), "Trabajo físico", 480, 5, null, null);

        activity.AttachWorkContext("Escaleras, caminata y traslado de cargas", 30, "Se agregó un edificio no previsto");

        Assert.Equal(30, activity.BreakMinutes);
        Assert.Contains("Escaleras", activity.WorkDemands);
        Assert.NotNull(activity.UnusualConditions);
    }

    [Fact]
    public void WorkContext_IsClearedForNonWorkActivity()
    {
        var activity = new DailyActivity(Guid.NewGuid(), new DateOnly(2026, 8, 17), "Bicicleta");
        activity.Record(new DateOnly(2026, 8, 17), "Bicicleta", 30, 4, 8, null);

        activity.AttachWorkContext("Traslado de cargas", 10, "Condición inusual");

        Assert.Null(activity.WorkDemands);
        Assert.Null(activity.BreakMinutes);
        Assert.Null(activity.UnusualConditions);
    }

    [Theory]
    [InlineData(0, 5)]
    [InlineData(60, 0)]
    [InlineData(60, 11)]
    public void Record_RejectsInvalidDurationOrRpe(int duration, int rpe)
    {
        var activity = new DailyActivity(Guid.NewGuid(), new DateOnly(2026, 8, 13), "Bicicleta");
        Assert.Throws<ArgumentOutOfRangeException>(() => activity.Record(new DateOnly(2026, 8, 13), "Bicicleta", duration, rpe, 5, null));
    }
}
