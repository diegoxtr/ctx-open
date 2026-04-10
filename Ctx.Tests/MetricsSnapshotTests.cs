namespace Ctx.Tests;

using Ctx.Domain;

public sealed class MetricsSnapshotTests
{
    [Fact]
    public void RecordCommandUsage_CreatesAndAccumulatesPerCommandMetrics()
    {
        var snapshot = new MetricsSnapshot(0, 0, 0m, 0, 0, TimeSpan.Zero);

        snapshot = snapshot.RecordCommandUsage("status", true, TimeSpan.FromMilliseconds(120), new DateTimeOffset(2026, 4, 9, 4, 40, 0, TimeSpan.Zero));
        snapshot = snapshot.RecordCommandUsage("status", false, TimeSpan.FromMilliseconds(80), new DateTimeOffset(2026, 4, 9, 4, 41, 0, TimeSpan.Zero));

        Assert.Equal(2, snapshot.TotalCommandInvocations);
        var usage = Assert.Single(snapshot.CommandUsage);
        Assert.Equal("status", usage.Command);
        Assert.Equal(2, usage.TotalInvocations);
        Assert.Equal(1, usage.SuccessfulInvocations);
        Assert.Equal(1, usage.FailedInvocations);
        Assert.Equal(TimeSpan.FromMilliseconds(200), usage.TotalExecutionTime);
        Assert.Equal("failure", usage.LastOutcome);
        Assert.Equal(new DateTimeOffset(2026, 4, 9, 4, 41, 0, TimeSpan.Zero), usage.LastInvokedAtUtc);
    }
}
