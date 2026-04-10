namespace Ctx.Tests;

using Ctx.Cli;
using Ctx.Domain;

public sealed class CommandCoverageTests
{
    [Fact]
    public void Build_ReportsUsedAndUnusedCommands()
    {
        var snapshot = new MetricsSnapshot(0, 0, 0m, 0, 0, TimeSpan.Zero)
            .RecordCommandUsage("status", true, TimeSpan.FromMilliseconds(20), DateTimeOffset.UtcNow)
            .RecordCommandUsage("task add", true, TimeSpan.FromMilliseconds(30), DateTimeOffset.UtcNow)
            .RecordCommandUsage("usage summary", true, TimeSpan.FromMilliseconds(10), DateTimeOffset.UtcNow);

        var report = CommandCoverage.Build(snapshot);

        Assert.Equal(CommandCoverage.GetKnownCommandNames().Count, report.TotalKnownCommands);
        Assert.Equal(3, report.UsedCommandCount);
        Assert.Contains("status", report.UsedCommands);
        Assert.Contains("task add", report.UsedCommands);
        Assert.Contains("usage summary", report.UsedCommands);
        Assert.DoesNotContain("status", report.UnusedCommands);
        Assert.Contains("usage coverage", report.UnusedCommands);
        Assert.True(report.CoveragePercentage > 0);
    }
}
