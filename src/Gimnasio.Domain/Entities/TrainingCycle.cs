using Gimnasio.Domain.Common;
namespace Gimnasio.Domain.Entities;
public sealed class TrainingCycle:Entity
{
    private TrainingCycle(){}public TrainingCycle(Guid profileId,string name){AthleteProfileId=profileId;Name=Required(name);}
    public Guid AthleteProfileId{get;private set;}public string Name{get;private set;}="";public DateOnly StartDate{get;private set;}public DateOnly EndDate{get;private set;}public string Focus{get;private set;}="";public int PlannedSessionsPerWeek{get;private set;}public string Status{get;private set;}="planned";public string? Notes{get;private set;}public int Version{get;private set;}=1;
    public void Configure(string name,DateOnly start,DateOnly end,string focus,int sessions,string status,string? notes,bool isUpdate){if(end<start)throw new ArgumentException("El ciclo no puede terminar antes de comenzar.");if(sessions is<0 or>14)throw new ArgumentOutOfRangeException(nameof(sessions));Name=Required(name);StartDate=start;EndDate=end;Focus=Required(focus);PlannedSessionsPerWeek=sessions;Status=Allowed(status,["planned","active","completed","paused"]);Notes=Clean(notes);if(isUpdate)Version++;UpdatedAt=DateTimeOffset.UtcNow;}
    public void RestoreVersion(int version)=>Version=version>0?version:throw new ArgumentOutOfRangeException(nameof(version));
    private static string Required(string v)=>string.IsNullOrWhiteSpace(v)?throw new ArgumentException("El valor es obligatorio."):v.Trim();private static string? Clean(string? v)=>string.IsNullOrWhiteSpace(v)?null:v.Trim();private static string Allowed(string v,string[] values)=>values.Contains(v)?v:throw new ArgumentException("Estado no válido.");
}
