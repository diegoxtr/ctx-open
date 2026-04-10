namespace Ctx.Tests;

using Ctx.Core;
using Xunit;

public sealed class ContextBuilderTests
{
    [Fact]
    public void Build_SelectsRelevantStructuredArtifacts()
    {
        var context = DomainFactory.WorkingContext();
        var builder = new ContextBuilder(
            new FixedClock(new DateTimeOffset(2026, 4, 7, 12, 30, 0, TimeSpan.Zero)),
            new Sha256HashingService());

        var packet = builder.Build(context, "Prepare architecture review");

        Assert.Equal(context.Project.Id, packet.ProjectId);
        Assert.NotEmpty(packet.GoalIds);
        Assert.NotEmpty(packet.TaskIds);
        Assert.NotEmpty(packet.HypothesisIds);
        Assert.Contains(packet.Sections, section => section.Title == "Goals");
        Assert.Contains(packet.Sections, section => section.Title == "Tasks");
        Assert.True(packet.EstimatedTokens > 0);
        Assert.False(string.IsNullOrWhiteSpace(packet.Fingerprint));
    }
}
