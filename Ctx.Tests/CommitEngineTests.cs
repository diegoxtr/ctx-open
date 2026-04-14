namespace Ctx.Tests;

using Ctx.Core;
using Ctx.Domain;
using Xunit;

public sealed class CommitEngineTests
{
    [Fact]
    public void CreateCommit_ProducesImmutableSnapshotWithDiff()
    {
        var clock = new FixedClock(new DateTimeOffset(2026, 4, 7, 13, 0, 0, TimeSpan.Zero));
        var commitEngine = new CommitEngine(clock, new Sha256HashingService(), new DefaultJsonSerializer(), new DiffEngine());
        var context = DomainFactory.WorkingContext();
        var runbooks = new[]
        {
            new OperationalRunbook(
                OperationalRunbookId.New(),
                "Local publish",
                OperationalRunbookKind.Procedure,
                new[] { "publish-local" },
                "Use when refreshing local installs.",
                new[] { "Run scripts/publish-local.ps1" },
                new[] { "Installed viewer responds" },
                Array.Empty<string>(),
                Array.Empty<GoalId>(),
                Array.Empty<TaskId>(),
                LifecycleState.Active,
                DomainFactory.Trace)
        };

        var commit = commitEngine.CreateCommit(context, runbooks, null, "seed graph", "tester");

        Assert.Equal("seed graph", commit.Message);
        Assert.False(commit.Snapshot.WorkingContext.Dirty);
        Assert.NotNull(commit.Snapshot.WorkingContext.HeadCommitId);
        Assert.Equal(commit.Id, commit.Snapshot.WorkingContext.HeadCommitId);
        Assert.Single(commit.Snapshot.Runbooks);
        Assert.False(string.IsNullOrWhiteSpace(commit.SnapshotHash));
        Assert.NotEmpty(commit.Diff.Tasks);
        Assert.NotEmpty(commit.Diff.Runbooks);
    }

    [Fact]
    public void CreateCommit_IncludesOptionalModelIdentityWhenConfigured()
    {
        var previousModelName = Environment.GetEnvironmentVariable("CTX_MODEL_NAME");
        var previousModelVersion = Environment.GetEnvironmentVariable("CTX_MODEL_VERSION");
        Environment.SetEnvironmentVariable("CTX_MODEL_NAME", "codex");
        Environment.SetEnvironmentVariable("CTX_MODEL_VERSION", "test-build");

        try
        {
            var clock = new FixedClock(new DateTimeOffset(2026, 4, 7, 13, 30, 0, TimeSpan.Zero));
            var commitEngine = new CommitEngine(clock, new Sha256HashingService(), new DefaultJsonSerializer(), new DiffEngine());
            var context = DomainFactory.WorkingContext();
            var runbooks = Array.Empty<OperationalRunbook>();

            var commit = commitEngine.CreateCommit(context, runbooks, null, "seed graph", "tester");

            Assert.Equal("codex", commit.Trace.ModelName);
            Assert.Equal("test-build", commit.Trace.ModelVersion);
        }
        finally
        {
            Environment.SetEnvironmentVariable("CTX_MODEL_NAME", previousModelName);
            Environment.SetEnvironmentVariable("CTX_MODEL_VERSION", previousModelVersion);
        }
    }
}
