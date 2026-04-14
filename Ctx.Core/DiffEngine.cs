namespace Ctx.Core;

using Ctx.Application;
using Ctx.Domain;

public sealed class DiffEngine : IDiffEngine
{
    public ContextDiff Diff(ContextCommit? previous, RepositorySnapshot current)
    {
        var previousSnapshot = previous?.Snapshot;
        var previousWorkingContext = previousSnapshot?.WorkingContext;
        var currentWorkingContext = current.WorkingContext;

        var decisions = DiffEntities(previousWorkingContext?.Decisions ?? Array.Empty<Decision>(), currentWorkingContext.Decisions, decision => decision.Id.Value, decision => $"{decision.Title} [{decision.State}]");
        var hypotheses = DiffEntities(previousWorkingContext?.Hypotheses ?? Array.Empty<Hypothesis>(), currentWorkingContext.Hypotheses, item => item.Id.Value, item => $"{item.Statement} [{item.State}]");
        var evidence = DiffEntities(previousWorkingContext?.Evidence ?? Array.Empty<Evidence>(), currentWorkingContext.Evidence, item => item.Id.Value, item => $"{item.Title} [{item.Kind}]");
        var tasks = DiffEntities(previousWorkingContext?.Tasks ?? Array.Empty<Ctx.Domain.Task>(), currentWorkingContext.Tasks, item => item.Id.Value, item => $"{item.Title} [{item.State}]");
        var conclusions = DiffEntities(previousWorkingContext?.Conclusions ?? Array.Empty<Conclusion>(), currentWorkingContext.Conclusions, item => item.Id.Value, item => $"{item.Summary} [{item.State}]");
        var runbooks = DiffEntities(previousSnapshot?.Runbooks ?? Array.Empty<OperationalRunbook>(), current.Runbooks, item => item.Id.Value, item => $"{item.Title} [{item.Kind}]");
        var triggers = DiffEntities(previousSnapshot?.Triggers ?? Array.Empty<CognitiveTrigger>(), current.Triggers, item => item.Id.Value, item => $"{item.Kind}:{item.Summary}");
        var summary = $"decisions:{decisions.Count} hypotheses:{hypotheses.Count} evidence:{evidence.Count} tasks:{tasks.Count} conclusions:{conclusions.Count} runbooks:{runbooks.Count} triggers:{triggers.Count}";

        return new ContextDiff(previous?.Id, currentWorkingContext.HeadCommitId, decisions, hypotheses, evidence, tasks, conclusions, runbooks, triggers, Array.Empty<CognitiveConflict>(), summary);
    }

    private static IReadOnlyList<ContextDiffChange> DiffEntities<T>(
        IEnumerable<T> previous,
        IEnumerable<T> current,
        Func<T, string> idSelector,
        Func<T, string> summarySelector)
    {
        var previousMap = previous.ToDictionary(idSelector, item => item);
        var currentMap = current.ToDictionary(idSelector, item => item);
        var changes = new List<ContextDiffChange>();

        foreach (var pair in currentMap)
        {
            if (!previousMap.TryGetValue(pair.Key, out var previousItem))
            {
                changes.Add(new("Added", typeof(T).Name, pair.Key, summarySelector(pair.Value)));
                continue;
            }

            if (!Equals(previousItem, pair.Value))
            {
                changes.Add(new("Modified", typeof(T).Name, pair.Key, summarySelector(pair.Value)));
            }
        }

        foreach (var pair in previousMap)
        {
            if (!currentMap.ContainsKey(pair.Key))
            {
                changes.Add(new("Removed", typeof(T).Name, pair.Key, summarySelector(pair.Value)));
            }
        }

        return changes;
    }
}
