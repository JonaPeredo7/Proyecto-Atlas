using Gimnasio.Domain.Entities;

namespace Gimnasio.Tests.Domain;

public sealed class ReportShareTests
{
    [Fact]
    public void ShareIsAvailableUntilItIsRevoked()
    {
        var share = new ReportShare(Guid.NewGuid(), "hash", "{}", new(2026, 8, 1), new(2026, 8, 13), DateTimeOffset.UtcNow.AddDays(7), true, false, "Kinesiólogo");
        Assert.True(share.IsAvailable(DateTimeOffset.UtcNow));
        share.Revoke();
        Assert.False(share.IsAvailable(DateTimeOffset.UtcNow));
        Assert.NotNull(share.RevokedAt);
    }

    [Fact]
    public void ShareRejectsPastExpiration()
    {
        Assert.Throws<ArgumentException>(() => new ReportShare(Guid.NewGuid(), "hash", "{}", new(2026, 8, 1), new(2026, 8, 13), DateTimeOffset.UtcNow.AddMinutes(-1), true, true, null));
    }

    [Fact]
    public void RestoredShareIsAlwaysHistoricalAndUnavailable()
    {
        var consent=new DateTimeOffset(2025,5,1,10,0,0,TimeSpan.Zero);
        var share=ReportShare.RestoreHistorical(Guid.NewGuid(),new string('a',64),"{}",new(2025,4,1),new(2025,4,30),consent.AddDays(7),consent,null,true,true,"Profesional");
        Assert.Equal(consent,share.ConsentGrantedAt);Assert.NotNull(share.RevokedAt);Assert.False(share.IsActive);Assert.False(share.IsAvailable(DateTimeOffset.UtcNow));
    }
}
