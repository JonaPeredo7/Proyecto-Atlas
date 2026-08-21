using System.Text.Json;
using Gimnasio.Application.Reports;

namespace Gimnasio.Tests.Domain;

public sealed class ProfessionalReportCompatibilityTests
{
    [Fact]
    public void LegacySnapshotWithoutWorkContextRemainsReadable()
    {
        var legacy = new
        {
            GeneratedAt = DateTimeOffset.UtcNow,
            From = new DateOnly(2026, 7, 1),
            To = new DateOnly(2026, 7, 7),
            Profile = new ReportProfileDto("Jonathan", null, null, null, null, null),
            Summary = new ReportSummaryDto(1, 60, 300, 120, 480, 780, 2, null, null, null, null, null, 0),
            Weeks = new[] { new { From = new DateOnly(2026, 7, 1), To = new DateOnly(2026, 7, 7), TrainingLoad = 300, ExternalLoad = 480, TotalLoad = 780, Sessions = 1 } },
            Goals = Array.Empty<ReportGoalDto>(),
            KneeChecks = Array.Empty<ReportKneeDto>(),
            Metrics = Array.Empty<ReportMetricDto>(),
            Learning = Array.Empty<ReportLearningDto>(),
            DataCoverageDays = 2,
            Disclaimer = "Informe histórico"
        };

        var report = JsonSerializer.Deserialize<ProfessionalReportDto>(JsonSerializer.Serialize(legacy));

        Assert.NotNull(report);
        Assert.Null(report.Work);
        Assert.Equal(0, report.Weeks.Single().WorkRecordedDays);
    }
}
