using Gimnasio.Domain.Entities;

namespace Gimnasio.Tests.Domain;

public sealed class DataTransferOperationTests
{
    [Fact]
    public void RestoreAuditPreservesSafeTotalsAndHashes()
    {
        var source=new string('a',64);var safety=new string('b',64);
        var operation=new DataTransferOperation(Guid.NewGuid(),"restore",source,null,safety,12,8,2);
        Assert.Equal("completed",operation.Status);Assert.Equal(12,operation.Restored);Assert.Equal(8,operation.AlreadyPresent);Assert.Equal(2,operation.Conflicts);Assert.Equal(safety,operation.SafetyBackupSha256);
    }

    [Fact]
    public void AuditRejectsInvalidHashOrOperationType()
    {
        Assert.Throws<ArgumentException>(()=>new DataTransferOperation(Guid.NewGuid(),"delete","bad",null,null,0,0,0));
        Assert.Throws<ArgumentException>(()=>new DataTransferOperation(Guid.NewGuid(),"backup","bad",null,null,0,0,0));
    }
}
