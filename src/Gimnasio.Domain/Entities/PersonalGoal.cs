using Gimnasio.Domain.Common;
namespace Gimnasio.Domain.Entities;
public sealed class PersonalGoal:Entity
{
    private PersonalGoal(){} public PersonalGoal(Guid profileId,string title){AthleteProfileId=profileId;Title=Required(title);}
    public Guid AthleteProfileId{get;private set;} public Guid? MetricDefinitionId{get;private set;} public string Title{get;private set;}="";public string Category{get;private set;}="";public decimal? BaselineValue{get;private set;}public decimal? TargetValue{get;private set;}public string? Unit{get;private set;}public DateOnly StartDate{get;private set;}public DateOnly? TargetDate{get;private set;}public string Status{get;private set;}="active";public string? Rationale{get;private set;}public int Version{get;private set;}=1;
    public void Configure(string title,string category,decimal? baseline,decimal? target,string? unit,DateOnly start,DateOnly? targetDate,string status,string? rationale,bool isUpdate){if(targetDate<start)throw new ArgumentException("La fecha objetivo no puede ser anterior al inicio.");Title=Required(title);Category=Required(category);BaselineValue=baseline;TargetValue=target;Unit=Clean(unit);StartDate=start;TargetDate=targetDate;Status=Allowed(status,["active","paused","completed"]);Rationale=Clean(rationale);if(isUpdate)Version++;UpdatedAt=DateTimeOffset.UtcNow;}
    public void LinkMetric(Guid? metricDefinitionId)=>MetricDefinitionId=metricDefinitionId;
    public void RestoreVersion(int version)=>Version=version>0?version:throw new ArgumentOutOfRangeException(nameof(version));
    private static string Required(string v)=>string.IsNullOrWhiteSpace(v)?throw new ArgumentException("El valor es obligatorio."):v.Trim();private static string? Clean(string? v)=>string.IsNullOrWhiteSpace(v)?null:v.Trim();private static string Allowed(string v,string[] values)=>values.Contains(v)?v:throw new ArgumentException("Estado no válido.");
}
