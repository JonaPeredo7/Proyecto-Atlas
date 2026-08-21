using Gimnasio.Domain.Entities;
using Gimnasio.Domain.Enums;

namespace Gimnasio.Tests.Domain;

public sealed class RestoreMetadataTests
{
    [Fact]
    public void ImportedEntityPreservesIdentityAndLifecycleMetadata()
    {
        var id=Guid.NewGuid();var created=new DateTimeOffset(2025,1,2,3,4,5,TimeSpan.Zero);var updated=created.AddDays(3);var activity=new DailyActivity(Guid.NewGuid(),new(2025,1,2),"Bicicleta");activity.Record(new(2025,1,2),"Bicicleta",30,4,8,null);
        activity.RestoreMetadata(id,created,updated,false);
        Assert.Equal(id,activity.Id);Assert.Equal(created,activity.CreatedAt);Assert.Equal(updated,activity.UpdatedAt);Assert.False(activity.IsActive);
    }

    [Fact]
    public void ImportedEntityRejectsEmptyIdentity()
    {
        var activity=new DailyActivity(Guid.NewGuid(),new(2025,1,2),"Trabajo");
        Assert.Throws<ArgumentException>(()=>activity.RestoreMetadata(Guid.Empty,DateTimeOffset.UtcNow,null,true));
    }

    [Fact]
    public void VersionedPlanningEntitiesPreserveTheirHistoricalVersion()
    {
        var goal=new PersonalGoal(Guid.NewGuid(),"Mejorar fuerza");goal.RestoreVersion(4);
        var cycle=new TrainingCycle(Guid.NewGuid(),"Base");cycle.RestoreVersion(3);
        Assert.Equal(4,goal.Version);Assert.Equal(3,cycle.Version);
        Assert.Throws<ArgumentOutOfRangeException>(()=>goal.RestoreVersion(0));
    }

    [Fact]
    public void CompletedTrainingSessionPreservesItsHistoricalState()
    {
        var completedAt=new DateTimeOffset(2025,2,3,18,30,0,TimeSpan.Zero);var session=new TrainingSession(Guid.NewGuid(),new(2025,2,3),"Fuerza A","Fuerza");
        session.RestoreState(TrainingSessionStatus.Completed,5,62,8,"Buena técnica",completedAt);
        Assert.Equal(TrainingSessionStatus.Completed,session.Status);Assert.Equal(5,session.Version);Assert.Equal(62,session.ActualDurationMinutes);Assert.Equal(8,session.SessionRpe);Assert.Equal(completedAt,session.CompletedAt);
    }

    [Fact]
    public void TrainingFollowUpPreservesItsOriginalRecordedDate()
    {
        var recordedAt=new DateTimeOffset(2025,2,4,9,15,0,TimeSpan.Zero);var followUp=new TrainingFollowUp(Guid.NewGuid());followUp.Record(4,2,"Rodilla derecha","Leve","Ninguna",false,false,null);
        followUp.RestoreRecordedAt(recordedAt);
        Assert.Equal(recordedAt,followUp.RecordedAt);
    }
}
