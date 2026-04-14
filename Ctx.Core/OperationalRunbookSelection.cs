namespace Ctx.Core;

using Ctx.Domain;

public static class OperationalRunbookSelection
{
    public static (IReadOnlyList<OperationalRunbook> Selected, IReadOnlyList<OperationalRunbook> Available) Select(
        IReadOnlyList<OperationalRunbook> runbooks,
        string purpose,
        string? goalId,
        string? taskId,
        IReadOnlyList<Goal> selectedGoals,
        IReadOnlyList<Ctx.Domain.Task> selectedTasks)
    {
        if (runbooks.Count == 0)
        {
            return (Array.Empty<OperationalRunbook>(), Array.Empty<OperationalRunbook>());
        }

        var goalIds = new HashSet<string>(
            selectedGoals.Select(goal => goal.Id.Value)
                .Concat(selectedTasks.Where(task => task.GoalId is not null).Select(task => task.GoalId!.Value.Value))
                .Concat(string.IsNullOrWhiteSpace(goalId) ? Array.Empty<string>() : new[] { goalId! }),
            StringComparer.OrdinalIgnoreCase);
        var taskIds = new HashSet<string>(
            selectedTasks.Select(task => task.Id.Value)
                .Concat(string.IsNullOrWhiteSpace(taskId) ? Array.Empty<string>() : new[] { taskId! }),
            StringComparer.OrdinalIgnoreCase);
        var normalizedPurpose = purpose.Trim();

        var matches = runbooks
            .Where(runbook => runbook.State == LifecycleState.Active)
            .Select(runbook => new
            {
                Runbook = runbook,
                TaskMatch = runbook.TaskIds.Any(item => taskIds.Contains(item.Value)),
                GoalMatch = runbook.GoalIds.Any(item => goalIds.Contains(item.Value)),
                TriggerMatch = runbook.Triggers.Any(trigger => !string.IsNullOrWhiteSpace(trigger) && normalizedPurpose.Contains(trigger, StringComparison.OrdinalIgnoreCase)),
                HasFailureSignal = runbook.Triggers.Any(trigger => !string.IsNullOrWhiteSpace(trigger) && trigger.Contains("lock", StringComparison.OrdinalIgnoreCase) && normalizedPurpose.Contains("lock", StringComparison.OrdinalIgnoreCase))
            })
            .Where(item => item.TaskMatch || item.GoalMatch || item.TriggerMatch || (item.Runbook.GoalIds.Count == 0 && item.Runbook.TaskIds.Count == 0))
            .Select(item => new
            {
                item.Runbook,
                Score =
                    (item.TaskMatch ? 100 : 0) +
                    (item.GoalMatch ? 70 : 0) +
                    (item.TriggerMatch ? 40 : 0) +
                    (item.Runbook.Kind == OperationalRunbookKind.Guardrail ? 20 : 0) +
                    (item.Runbook.Kind == OperationalRunbookKind.Troubleshooting && !item.HasFailureSignal ? -100 : 0)
            })
            .Where(item => item.Score >= 0)
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.Runbook.Title, StringComparer.OrdinalIgnoreCase)
            .Select(item => item.Runbook)
            .ToList();

        var selected = matches.Take(2).ToArray();
        var available = matches.Skip(2).Take(3).ToArray();
        return (selected, available);
    }
}
