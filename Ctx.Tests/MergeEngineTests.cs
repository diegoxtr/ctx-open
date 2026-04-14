namespace Ctx.Tests;

using Ctx.Core;
using Ctx.Domain;
using Xunit;

public sealed class MergeEngineTests
{
    [Fact]
    public void Merge_ReportsCognitiveConflictsForDivergentEntities()
    {
        var current = DomainFactory.WorkingContext();
        var divergentTask = current.Tasks[0] with { Description = "Current branch description", State = TaskExecutionState.InProgress };
        current = current with { Tasks = new[] { divergentTask } };

        var sourceSnapshot = current with
        {
            Tasks = new[] { divergentTask with { Description = "Incoming branch description", State = TaskExecutionState.Blocked } }
        };

        var sourceCommit = new ContextCommit(
            ContextCommitId.New(),
            "feature",
            "feature work",
            Array.Empty<ContextCommitId>(),
            new DateTimeOffset(2026, 4, 7, 15, 0, 0, TimeSpan.Zero),
            "hash",
            new ContextDiff(null, null, Array.Empty<ContextDiffChange>(), Array.Empty<ContextDiffChange>(), Array.Empty<ContextDiffChange>(), Array.Empty<ContextDiffChange>(), Array.Empty<ContextDiffChange>(), Array.Empty<ContextDiffChange>(), Array.Empty<CognitiveConflict>(), string.Empty),
            new RepositorySnapshot(sourceSnapshot, Array.Empty<OperationalRunbook>()),
            sourceSnapshot.Trace);

        var result = new MergeEngine().Merge(new RepositorySnapshot(current, Array.Empty<OperationalRunbook>()), sourceCommit);

        Assert.False(result.AutoMerged);
        Assert.Contains(result.Conflicts, conflict => conflict.EntityType == nameof(Ctx.Domain.Task));
    }
}
