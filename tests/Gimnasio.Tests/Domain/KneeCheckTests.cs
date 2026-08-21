using Gimnasio.Domain.Entities;
namespace Gimnasio.Tests.Domain;
public sealed class KneeCheckTests
{
    [Fact] public void RecordStoresFunctionalDomains(){var x=new KneeCheck(Guid.NewGuid());x.Record(DateTimeOffset.UtcNow,"24 h posteriores","izquierdo",3,1,5,"leve",false,false,true,8,6,5,null);Assert.Equal(5,x.PainWorst24H);Assert.Equal(6,x.StairsCapacity);}
    [Fact] public void RecordRejectsBestPainAboveWorst(){var x=new KneeCheck(Guid.NewGuid());Assert.Throws<ArgumentException>(()=>x.Record(DateTimeOffset.UtcNow,"control","derecho",4,6,3,"ninguna",false,false,true,10,10,10,null));}
}
