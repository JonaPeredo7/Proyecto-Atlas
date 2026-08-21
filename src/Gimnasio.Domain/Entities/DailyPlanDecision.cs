using Gimnasio.Domain.Common;

namespace Gimnasio.Domain.Entities;

public sealed class DailyPlanDecision : Entity
{
    private static readonly string[] Decisions=["as-planned","adjusted","recovery","professional-review"];
    private static readonly string[] Contexts=["incomplete","recorded","stable","observe","attention"];
    private DailyPlanDecision() { }
    public DailyPlanDecision(Guid profileId,DateOnly date){if(profileId==Guid.Empty)throw new ArgumentException("El perfil es obligatorio.");AthleteProfileId=profileId;Date=date;}

    public Guid AthleteProfileId{get;private set;}
    public DateOnly Date{get;private set;}
    public string Decision{get;private set;}="as-planned";
    public string Reason{get;private set;}="";
    public string ContextStatus{get;private set;}="incomplete";
    public int PlannedLoadSnapshot{get;private set;}
    public int Version{get;private set;}=1;

    public void Record(string decision,string reason,string contextStatus,int plannedLoad,bool update)
    {
        Decision=Allowed(decision,Decisions);Reason=Required(reason,1000);ContextStatus=Allowed(contextStatus,Contexts);if(plannedLoad<0)throw new ArgumentOutOfRangeException(nameof(plannedLoad));PlannedLoadSnapshot=plannedLoad;if(update)Version++;UpdatedAt=DateTimeOffset.UtcNow;
    }
    public void RestoreVersion(int version){if(version<1)throw new ArgumentOutOfRangeException(nameof(version));Version=version;}
    private static string Allowed(string value,string[] allowed)=>allowed.Contains(value)?value:throw new ArgumentException("La opción seleccionada no es válida.");
    private static string Required(string value,int max)=>string.IsNullOrWhiteSpace(value)?throw new ArgumentException("El motivo es obligatorio."):value.Trim().Length<=max?value.Trim():throw new ArgumentException($"El texto admite hasta {max} caracteres.");
}
