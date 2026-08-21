using Gimnasio.Domain.Enums;

namespace Gimnasio.Domain.Services;

public sealed record DailyPlanInput(int? PlannedDurationMinutes,int? TargetRpe,TrainingSessionStatus Status);
public sealed record DailyPlanContext(string Status,string Label,string Summary,int SessionCount,int PlannedMinutes,int PlannedLoad,int IncompleteSessions,bool HasInProgress);

public static class DailyPlanContextCalculator
{
    public static DailyPlanContext Calculate(string personalState,IReadOnlyCollection<DailyPlanInput> source)
    {
        var sessions=source.Where(x=>x.Status!=TrainingSessionStatus.Cancelled).ToArray();
        if(sessions.Length==0)return new("none","Sin sesión planificada","No hay entrenamiento previsto para hoy. Puede ser descanso intencional o un día todavía sin planificar.",0,0,0,0,false);
        var incomplete=sessions.Count(x=>!x.PlannedDurationMinutes.HasValue||!x.TargetRpe.HasValue);
        var minutes=sessions.Sum(x=>x.PlannedDurationMinutes??0);
        var load=sessions.Sum(x=>(x.PlannedDurationMinutes??0)*(x.TargetRpe??0));
        var inProgress=sessions.Any(x=>x.Status==TrainingSessionStatus.InProgress);
        if(inProgress)return new("active","Entrenamiento en curso","La sesión ya fue iniciada. Atlas conserva el contexto para interpretarlo junto con el resultado final.",sessions.Length,minutes,load,incomplete,true);
        if(personalState=="attention")return new("attention","Revisión previa necesaria","Hay señales de atención en el autorreporte y un plan previsto. Revisá ambos antes de decidir; Atlas no modifica ni autoriza la sesión.",sessions.Length,minutes,load,incomplete,false);
        if(incomplete>0)return new("incomplete","Completar la prescripción",$"{incomplete} sesión{(incomplete==1?" no tiene":"es no tienen")} duración o RPE objetivo completos, por lo que la carga prevista es parcial.",sessions.Length,minutes,load,incomplete,false);
        if(personalState=="observe")return new("observe","Plan y contexto para revisar","El plan está completo y el contexto diario presenta cambios respecto de tu referencia. La decisión de mantenerlo o corregirlo debe ser explícita.",sessions.Length,minutes,load,0,false);
        return new("planned","Plan disponible","La prescripción de hoy está completa. La carga mostrada es una estimación de duración × RPE objetivo, no una garantía del resultado.",sessions.Length,minutes,load,0,false);
    }
}
