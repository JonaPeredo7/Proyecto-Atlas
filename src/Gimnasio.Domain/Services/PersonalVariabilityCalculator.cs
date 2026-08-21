namespace Gimnasio.Domain.Services;

public sealed record PersonalVariabilityInput(decimal? SleepHours,decimal? SleepQuality,decimal? Energy,decimal? Fatigue,decimal? Stress,decimal? Pain);
public sealed record PersonalVariabilityFactor(string Key,string Label,string Unit,int Entries,decimal Median,decimal LowerQuartile,decimal UpperQuartile,decimal Minimum,decimal Maximum);
public sealed record PersonalVariabilityProfile(int WindowDays,int CheckInDays,decimal CoveragePercent,IReadOnlyCollection<PersonalVariabilityFactor>Factors,string Disclaimer);

public static class PersonalVariabilityCalculator
{
    public static PersonalVariabilityProfile Calculate(IEnumerable<PersonalVariabilityInput> source,int windowDays=28)
    {
        if(windowDays<1)throw new ArgumentOutOfRangeException(nameof(windowDays));
        var entries=source.ToArray();var factors=new List<PersonalVariabilityFactor>();
        Add(factors,"sleep-duration","Duración del sueño"," h",entries.Select(x=>x.SleepHours));
        Add(factors,"sleep-quality","Calidad del sueño"," / 5",entries.Select(x=>x.SleepQuality));
        Add(factors,"energy","Energía"," / 5",entries.Select(x=>x.Energy));
        Add(factors,"fatigue","Fatiga"," / 10",entries.Select(x=>x.Fatigue));
        Add(factors,"stress","Estrés"," / 5",entries.Select(x=>x.Stress));
        Add(factors,"pain","Dolor"," / 10",entries.Select(x=>x.Pain));
        return new(windowDays,entries.Length,Math.Round(entries.Length*100m/windowDays,1),factors,"Mediana y rango central del 50% de tus registros. Describen variabilidad observada; no definen una zona ideal, normalidad clínica ni aptitud para entrenar.");
    }

    private static void Add(List<PersonalVariabilityFactor> result,string key,string label,string unit,IEnumerable<decimal?> source)
    {
        var values=source.Where(x=>x.HasValue).Select(x=>x!.Value).OrderBy(x=>x).ToArray();
        if(values.Length==0)return;
        result.Add(new(key,label,unit,values.Length,Percentile(values,.5m),Percentile(values,.25m),Percentile(values,.75m),values[0],values[^1]));
    }

    private static decimal Percentile(IReadOnlyList<decimal> values,decimal percentile)
    {
        if(values.Count==1)return Math.Round(values[0],1);
        var position=(values.Count-1)*percentile;var lower=(int)Math.Floor(position);var upper=(int)Math.Ceiling(position);
        var value=values[lower]+(values[upper]-values[lower])*(position-lower);
        return Math.Round(value,1);
    }
}
