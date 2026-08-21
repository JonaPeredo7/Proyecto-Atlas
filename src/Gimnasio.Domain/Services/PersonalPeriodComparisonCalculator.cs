namespace Gimnasio.Domain.Services;

public sealed record PersonalPeriodComparisonFactor(string Key,string Label,string Unit,int RecentEntries,int BaselineEntries,decimal RecentMedian,decimal BaselineMedian,decimal BaselineLowerQuartile,decimal BaselineUpperQuartile,decimal Delta,string Position);
public sealed record PersonalPeriodComparison(int RecentDays,int BaselineDays,int RecentCheckIns,int BaselineCheckIns,decimal RecentCoveragePercent,decimal BaselineCoveragePercent,int ComparableFactors,IReadOnlyCollection<PersonalPeriodComparisonFactor>Factors,string Disclaimer);

public static class PersonalPeriodComparisonCalculator
{
    private const int MinimumRecentEntries=3;
    private const int MinimumBaselineEntries=7;

    public static PersonalPeriodComparison Calculate(IEnumerable<PersonalVariabilityInput> recentSource,IEnumerable<PersonalVariabilityInput> baselineSource,int recentDays=7,int baselineDays=21)
    {
        if(recentDays<1)throw new ArgumentOutOfRangeException(nameof(recentDays));
        if(baselineDays<1)throw new ArgumentOutOfRangeException(nameof(baselineDays));
        var recent=recentSource.ToArray();var baseline=baselineSource.ToArray();var factors=new List<PersonalPeriodComparisonFactor>();
        Add(factors,"sleep-duration","Duración del sueño"," h",recent.Select(x=>x.SleepHours),baseline.Select(x=>x.SleepHours));
        Add(factors,"sleep-quality","Calidad del sueño"," / 5",recent.Select(x=>x.SleepQuality),baseline.Select(x=>x.SleepQuality));
        Add(factors,"energy","Energía"," / 5",recent.Select(x=>x.Energy),baseline.Select(x=>x.Energy));
        Add(factors,"fatigue","Fatiga"," / 10",recent.Select(x=>x.Fatigue),baseline.Select(x=>x.Fatigue));
        Add(factors,"stress","Estrés"," / 5",recent.Select(x=>x.Stress),baseline.Select(x=>x.Stress));
        Add(factors,"pain","Dolor"," / 10",recent.Select(x=>x.Pain),baseline.Select(x=>x.Pain));
        return new(recentDays,baselineDays,recent.Length,baseline.Length,Math.Round(recent.Length*100m/recentDays,1),Math.Round(baseline.Length*100m/baselineDays,1),factors.Count(x=>x.Position!="insufficient"),factors,"La posición compara la mediana reciente con el 50% central de los 21 días anteriores. Se requieren 3 registros recientes y 7 previos por indicador. Es una regla descriptiva de cobertura, no un umbral clínico ni una atribución causal.");
    }

    private static void Add(List<PersonalPeriodComparisonFactor> result,string key,string label,string unit,IEnumerable<decimal?> recentSource,IEnumerable<decimal?> baselineSource)
    {
        var recent=Values(recentSource);var baseline=Values(baselineSource);if(recent.Length==0||baseline.Length==0)return;
        var recentMedian=Percentile(recent,.5m);var baselineMedian=Percentile(baseline,.5m);var lower=Percentile(baseline,.25m);var upper=Percentile(baseline,.75m);
        var position=recent.Length<MinimumRecentEntries||baseline.Length<MinimumBaselineEntries?"insufficient":recentMedian<lower?"below":recentMedian>upper?"above":"within";
        result.Add(new(key,label,unit,recent.Length,baseline.Length,recentMedian,baselineMedian,lower,upper,Math.Round(recentMedian-baselineMedian,1),position));
    }

    private static decimal[] Values(IEnumerable<decimal?> source)=>source.Where(x=>x.HasValue).Select(x=>x!.Value).OrderBy(x=>x).ToArray();
    private static decimal Percentile(IReadOnlyList<decimal> values,decimal percentile)
    {
        if(values.Count==1)return Math.Round(values[0],1);
        var position=(values.Count-1)*percentile;var lower=(int)Math.Floor(position);var upper=(int)Math.Ceiling(position);
        return Math.Round(values[lower]+(values[upper]-values[lower])*(position-lower),1);
    }
}
