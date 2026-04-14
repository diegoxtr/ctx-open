namespace Ctx.Tests;

using Ctx.Core;
using Xunit;

public sealed class ContextBuilderTests
{
    [Fact]
    public void Build_SelectsRelevantStructuredArtifacts()
    {
        var context = DomainFactory.WorkingContext();
        var runbook = new Ctx.Domain.OperationalRunbook(
            Ctx.Domain.OperationalRunbookId.New(),
            "Architecture review policy",
            Ctx.Domain.OperationalRunbookKind.Policy,
            new[] { "architecture" },
            "Use when preparing architecture reviews.",
            new[] { "Check the active goal and task lineage." },
            new[] { "Packet includes the policy." },
            Array.Empty<string>(),
            context.Goals.Select(goal => goal.Id).ToArray(),
            Array.Empty<Ctx.Domain.TaskId>(),
            Ctx.Domain.LifecycleState.Active,
            DomainFactory.Trace);
        var builder = new ContextBuilder(
            new FixedClock(new DateTimeOffset(2026, 4, 7, 12, 30, 0, TimeSpan.Zero)),
            new Sha256HashingService());

        var packet = builder.Build(context, new[] { runbook }, "Prepare architecture review");

        Assert.Equal(context.Project.Id, packet.ProjectId);
        Assert.NotEmpty(packet.GoalIds);
        Assert.NotEmpty(packet.TaskIds);
        Assert.NotEmpty(packet.HypothesisIds);
        Assert.NotNull(packet.RunbookIds);
        Assert.Single(packet.RunbookIds!);
        Assert.Contains(packet.Sections, section => section.Title == "Goals");
        Assert.Contains(packet.Sections, section => section.Title == "Tasks");
        Assert.Contains(packet.Sections, section => section.Title == "Operational Runbooks");
        Assert.True(packet.EstimatedTokens > 0);
        Assert.False(string.IsNullOrWhiteSpace(packet.Fingerprint));
    }
}
