using Gimnasio.Domain.Common;
namespace Gimnasio.Domain.Entities;
public sealed class PlanChange:Entity
{
    private PlanChange(){}public PlanChange(Guid profileId,string entityType,Guid entityId,int version,string reason,string summary){AthleteProfileId=profileId;EntityType=Required(entityType);EntityId=entityId;Version=version;Reason=Required(reason);Summary=Required(summary);}
    public Guid AthleteProfileId{get;private set;}public string EntityType{get;private set;}="";public Guid EntityId{get;private set;}public int Version{get;private set;}public string Reason{get;private set;}="";public string Summary{get;private set;}="";
    private static string Required(string v)=>string.IsNullOrWhiteSpace(v)?throw new ArgumentException("El motivo del cambio es obligatorio."):v.Trim();
}
