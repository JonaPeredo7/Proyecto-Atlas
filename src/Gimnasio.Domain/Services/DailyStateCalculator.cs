namespace Gimnasio.Domain.Services;

public sealed record DailyStateInput(int? SleepMinutes,int SleepQuality,int Energy,int Fatigue,int Stress,int? PainIntensity,bool NeedsAttention);
public sealed record DailyStateFactor(string Key,string Label,decimal Current,decimal Baseline,decimal Delta,decimal VisualThreshold,string Unit,string Trend,string Basis);
public sealed record DailyStateAssessment(string Status,string Label,string Summary,int BaselineDays,IReadOnlyCollection<DailyStateFactor>Factors);

public static class DailyStateCalculator
{
    public static DailyStateAssessment Calculate(DailyStateInput? today,IReadOnlyCollection<DailyStateInput> recent)
    {
        if(today is null)return new("incomplete","Falta el check-in","Completá el autorreporte antes de comparar el día con tu línea personal.",recent.Count,[]);
        if(recent.Count<3)return new("recorded","Registro disponible","Todavía faltan días comparables para formar una línea personal estable.",recent.Count,[]);

        var factors=new List<DailyStateFactor>();
        Add(factors,"sleep-quality","Calidad del sueño",today.SleepQuality,recent.Average(x=>(decimal)x.SleepQuality),1,"/ 5",true);
        Add(factors,"energy","Energía",today.Energy,recent.Average(x=>(decimal)x.Energy),1,"/ 5",true);
        Add(factors,"fatigue","Fatiga",today.Fatigue,recent.Average(x=>(decimal)x.Fatigue),2,"/ 10",false);
        Add(factors,"stress","Estrés",today.Stress,recent.Average(x=>(decimal)x.Stress),1,"/ 5",false);
        Add(factors,"pain","Dolor",today.PainIntensity??0,recent.Average(x=>(decimal)(x.PainIntensity??0)),2,"/ 10",false);
        var recentSleep=recent.Where(x=>x.SleepMinutes.HasValue).Select(x=>x.SleepMinutes!.Value).ToArray();
        if(today.SleepMinutes.HasValue&&recentSleep.Length>=3)Add(factors,"sleep-duration","Sueño",today.SleepMinutes.Value/60m,(decimal)recentSleep.Average()/60m,1," h",true);

        if(today.NeedsAttention)return new("attention","Revisar síntomas","El autorreporte contiene señales que requieren seguimiento. Atlas no determina su causa ni indica si debés entrenar.",recent.Count,factors);
        var unfavorable=factors.Count(x=>x.Trend=="worse");
        return unfavorable>=2
            ?new("observe","Contexto más exigente","Dos o más señales están menos favorables que tu referencia reciente. Revisá el contexto y la carga prevista sin sacar conclusiones clínicas.",recent.Count,factors)
            :new("stable","Contexto similar","La mayoría de las señales se mantiene cerca de tu referencia reciente. Es una comparación descriptiva, no una evaluación médica.",recent.Count,factors);
    }

    private static void Add(List<DailyStateFactor> factors,string key,string label,decimal current,decimal baseline,decimal threshold,string unit,bool higherIsBetter)
    {
        current=Math.Round(current,1);baseline=Math.Round(baseline,1);var delta=Math.Round(current-baseline,1);
        var trend=Math.Abs(delta)<threshold?"similar":higherIsBetter?(delta>0?"better":"worse"):(delta<0?"better":"worse");
        factors.Add(new(key,label,current,baseline,delta,threshold,unit,trend,"Regla visual de Atlas; no es un umbral clínico."));
    }
}
