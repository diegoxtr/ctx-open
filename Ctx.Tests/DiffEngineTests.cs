namespace Ctx.Tests;

using Ctx.Core;
using Ctx.Domain;
using Xunit;

public sealed class DiffEngineTests
{
    [Fact]
    public void Diff_ReportsAddedAndModifiedEntities()
    {
        var current = DomainFactory.WorkingContext();
        var previousContext = current with
        {
            Tasks = Array.Empty<Ctx.Domain.Task>(),
            Hypotheses = Array.Empty<Hypothesis>()
        };

        var previousCommit = new ContextCommit(
            ContextCommitId.New(),
            "main",
            "previous",
            Array.Empty<ContextCommitId>(),
            new DateTimeOffset(2026, 4, 7, 11, 0, 0, TimeSpan.Zero),
            "hash",
            new ContextDiff(null, null, Array.Empty<ContextDiffChange>(), Array.Empty<ContextDiffChange>(), Array.Empty<ContextDiffChange>(), Array.Empty<ContextDiffChange>(), Array.Empty<ContextDiffChange>(), Array.Empty<ContextDiffChange>(), Array.Empty<CognitiveConflict>(), string.Empty),
            new RepositorySnapshot(previousContext, Array.Empty<OperationalRunbook>()),
            previousContext.Trace);

        var diff = new DiffEngine().Diff(previousCommit, new RepositorySnapshot(current, Array.Empty<OperationalRunbook>()));

        Assert.Contains(diff.Tasks, change => change.ChangeType == "Added");
        Assert.Contains(diff.Hypotheses, change => change.ChangeType == "Added");
        Assert.Equal(previousCommit.Id, diff.FromCommitId);
    }
}
