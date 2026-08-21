using Gimnasio.Domain.Entities;

namespace Gimnasio.Tests.Domain;

public sealed class DailyPlanDecisionTests
{
    [Fact]
    public void DecisionPreservesContextAndVersionsCorrections()
    {
        var decision=new DailyPlanDecision(Guid.NewGuid(),new(2026,8,14));decision.Record("as-planned","El contexto es similar.","stable",360,false);decision.Record("adjusted","Corregí el volumen después de revisar el plan.","observe",300,true);
        Assert.Equal("adjusted",decision.Decision);Assert.Equal("observe",decision.ContextStatus);Assert.Equal(300,decision.PlannedLoadSnapshot);Assert.Equal(2,decision.Version);
    }

    [Fact]
    public void DecisionRequiresAnExplicitReason()
    {
        var decision=new DailyPlanDecision(Guid.NewGuid(),new(2026,8,14));Assert.Throws<ArgumentException>(()=>decision.Record("recovery","","stable",0,false));
    }
}
