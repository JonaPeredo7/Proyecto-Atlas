using Gimnasio.Domain.Entities;

namespace Gimnasio.Tests.Domain;

public sealed class AtlasProfileTests
{
    [Fact]
    public void AthleteProfile_Update_SavesAndNormalizesPersonalData()
    {
        var profile = new AthleteProfile(Guid.NewGuid(), "  Jonathan  ");

        profile.Update(
            "  Jonathan Atlas  ",
            180,
            65,
            "  Mejorar el rendimiento  ",
            new DateOnly(2027, 5, 15),
            "Derecha",
            "Derecha",
            "Izquierda");

        Assert.Equal("Jonathan Atlas", profile.DisplayName);
        Assert.Equal(180, profile.HeightCm);
        Assert.Equal(65, profile.ReferenceWeightKg);
        Assert.Equal("Mejorar el rendimiento", profile.PrimaryGoal);
    }

    [Fact]
    public void AthleteProfile_Update_RejectsImplausibleHeight()
    {
        var profile = new AthleteProfile(Guid.NewGuid(), "Jonathan");

        Assert.Throws<ArgumentOutOfRangeException>(() => profile.Update(
            "Jonathan",
            50,
            65,
            null,
            null,
            null,
            null,
            null));
    }

    [Fact]
    public void DailyCheckIn_Record_SavesReadinessAndPainData()
    {
        var checkIn = new DailyCheckIn(Guid.NewGuid(), new DateOnly(2026, 8, 12));

        checkIn.Record(
            450,
            4,
            4,
            3,
            2,
            "  Rodilla  ",
            "Izquierda",
            2,
            "Leve",
            "Ninguna",
            false,
            false,
            6,
            15,
            "Fuerza y bicicleta",
            "Sin cambios relevantes");

        Assert.Equal(450, checkIn.SleepMinutes);
        Assert.Equal("Rodilla", checkIn.PainLocation);
        Assert.Equal(2, checkIn.PainIntensity);
        Assert.False(checkIn.Instability);
    }

    [Fact]
    public void DailyCheckIn_Record_RejectsPainOutsideScale()
    {
        var checkIn = new DailyCheckIn(Guid.NewGuid(), new DateOnly(2026, 8, 12));

        Assert.Throws<ArgumentOutOfRangeException>(() => checkIn.Record(
            450,
            4,
            4,
            3,
            2,
            "Rodilla",
            "Izquierda",
            11,
            null,
            null,
            false,
            false,
            6,
            null,
            null,
            null));
    }
}
