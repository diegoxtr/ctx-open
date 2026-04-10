namespace Ctx.Core;

using Ctx.Application;
using Ctx.Domain;

public sealed class DiffEngine : IDiffEngine
{
    public ContextDiff Diff(ContextCommit? previous, WorkingContext current)
    {
        var previousSnapshot = previous?.Snapshot;

        var decisions = DiffEntities(previousSnapshot?.Decisions ?? Array.Empty<Decision>(), current.Decisions, decision => decision.Id.Value, decision => $"{decision.Title} [{decision.State}]");
        var hypotheses = DiffEntities(previousSnapshot?.Hypotheses ?? Array.Empty<Hypothesis>(), current.Hypotheses, item => item.Id.Value, item => $"{item.Statement} [{item.State}]");
        var evidence = DiffEntities(previousSnapshot?.Evidence ?? Array.Empty<Evidence>(), current.Evidence, item => item.Id.Value, item => $"{item.Title} [{item.Kind}]");
        var tasks = DiffEntities(previousSnapshot?.Tasks ?? Array.Empty<Ctx.Domain.Task>(), current.Tasks, item => item.Id.Value, item => $"{item.Title} [{item.State}]");
        var conclusions = DiffEntities(previousSnapshot?.Conclusions ?? Array.Empty<Conclusion>(), current.Conclusions, item => item.Id.Value, item => $"{item.Summary} [{item.State}]");
        var summary = $"decisions:{decisions.Count} hypotheses:{hypotheses.Count} evidence:{evidence.Count} tasks:{tasks.Count} conclusions:{conclusions.Count}";

        return new ContextDiff(previous?.Id, current.HeadCommitId, decisions, hypotheses, evidence, tasks, conclusions, Array.Empty<CognitiveConflict>(), summary);
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
