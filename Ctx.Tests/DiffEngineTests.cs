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

    [Fact]
    public void Diff_DoesNotReportModifiedEntitiesForEquivalentListInstances()
    {
        var current = DomainFactory.WorkingContext();
        var previousContext = current with
        {
            Goals = current.Goals.Select(goal => goal with { TaskIds = goal.TaskIds.ToArray() }).ToArray(),
            Tasks = current.Tasks.Select(task => task with { DependsOnTaskIds = task.DependsOnTaskIds.ToArray(), HypothesisIds = task.HypothesisIds.ToArray() }).ToArray(),
            Hypotheses = current.Hypotheses.Select(hypothesis => hypothesis with { TaskIds = hypothesis.TaskIds.ToArray(), EvidenceIds = hypothesis.EvidenceIds.ToArray() }).ToArray(),
            Evidence = current.Evidence.Select(evidence => evidence with { Supports = evidence.Supports.ToArray() }).ToArray(),
            Decisions = current.Decisions.Select(decision => decision with { HypothesisIds = decision.HypothesisIds.ToArray(), EvidenceIds = decision.EvidenceIds.ToArray() }).ToArray(),
            Conclusions = current.Conclusions.Select(conclusion => conclusion with { DecisionIds = conclusion.DecisionIds.ToArray(), EvidenceIds = conclusion.EvidenceIds.ToArray(), GoalIds = conclusion.GoalIds.ToArray(), TaskIds = conclusion.TaskIds.ToArray() }).ToArray()
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

        Assert.Empty(diff.Tasks);
        Assert.Empty(diff.Hypotheses);
        Assert.Empty(diff.Evidence);
        Assert.Empty(diff.Decisions);
        Assert.Empty(diff.Conclusions);
    }
}
