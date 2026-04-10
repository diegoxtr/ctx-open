namespace Ctx.Tests;

using Ctx.Domain;

internal static class DomainFactory
{
    private static readonly Traceability Trace = new(
        "tester",
        new DateTimeOffset(2026, 4, 7, 12, 0, 0, TimeSpan.Zero),
        null,
        null,
        new[] { "test" },
        Array.Empty<string>());

    public static WorkingContext WorkingContext()
    {
        var project = new Project(ProjectId.New(), "CTX", "Cognitive repository", "main", LifecycleState.Active, Trace);
        var goal = new Goal(GoalId.New(), null, "Ship core", "Deliver foundation", 10, LifecycleState.Active, Trace, Array.Empty<TaskId>());
        var task = new Ctx.Domain.Task(TaskId.New(), goal.Id, "Implement commit engine", "Commit snapshots", TaskExecutionState.Ready, Trace, Array.Empty<TaskId>(), Array.Empty<HypothesisId>());
        goal = goal with { TaskIds = new[] { task.Id } };
        var hypothesis = new Hypothesis(HypothesisId.New(), "Structured commits reduce repetition", "Traceability reduces waste", 0.8m, 0.7m, 0.6m, 0.3m, HypothesisState.Proposed, Trace, new[] { task.Id }, Array.Empty<EvidenceId>());
        task = task with { HypothesisIds = new[] { hypothesis.Id } };

        return new WorkingContext(
            WorkingContextId.New(),
            DomainConstants.CurrentRepositoryVersion,
            "main",
            null,
            true,
            project,
            new[] { goal },
            new[] { task },
            new[] { hypothesis },
            Array.Empty<Decision>(),
            Array.Empty<Evidence>(),
            Array.Empty<Conclusion>(),
            Array.Empty<Run>(),
            Trace);
    }
}
