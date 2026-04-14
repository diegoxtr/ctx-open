namespace Ctx.Core;

using Ctx.Application;
using Ctx.Domain;

public sealed class MergeEngine : IMergeEngine
{
    public MergeResult Merge(RepositorySnapshot current, ContextCommit sourceCommit)
    {
        var source = sourceCommit.Snapshot;
        var currentWorking = current.WorkingContext;
        var sourceWorking = source.WorkingContext;
        var conflicts = new List<CognitiveConflict>();

        conflicts.AddRange(FindConflicts(currentWorking.Tasks, sourceWorking.Tasks, item => item.Id.Value, item => $"{item.Title} [{item.State}]"));
        conflicts.AddRange(FindConflicts(currentWorking.Hypotheses, sourceWorking.Hypotheses, item => item.Id.Value, item => $"{item.Statement} [{item.State}]"));
        conflicts.AddRange(FindConflicts(currentWorking.Decisions, sourceWorking.Decisions, item => item.Id.Value, item => $"{item.Title} [{item.State}]"));
        conflicts.AddRange(FindConflicts(currentWorking.Evidence, sourceWorking.Evidence, item => item.Id.Value, item => $"{item.Title} [{item.Kind}]"));
        conflicts.AddRange(FindConflicts(currentWorking.Conclusions, sourceWorking.Conclusions, item => item.Id.Value, item => $"{item.Summary} [{item.State}]"));
        conflicts.AddRange(FindConflicts(current.Runbooks, source.Runbooks, item => item.Id.Value, item => $"{item.Title} [{item.Kind}]"));
        conflicts.AddRange(FindConflicts(current.Triggers, source.Triggers, item => item.Id.Value, item => $"{item.Kind}:{item.Summary}"));

        var mergedWorking = currentWorking with
        {
            Dirty = true,
            Goals = MergeById(currentWorking.Goals, sourceWorking.Goals, item => item.Id.Value),
            Tasks = MergeById(currentWorking.Tasks, sourceWorking.Tasks, item => item.Id.Value),
            Hypotheses = MergeById(currentWorking.Hypotheses, sourceWorking.Hypotheses, item => item.Id.Value),
            Decisions = MergeById(currentWorking.Decisions, sourceWorking.Decisions, item => item.Id.Value),
            Evidence = MergeById(currentWorking.Evidence, sourceWorking.Evidence, item => item.Id.Value),
            Conclusions = MergeById(currentWorking.Conclusions, sourceWorking.Conclusions, item => item.Id.Value),
            Runs = MergeById(currentWorking.Runs, sourceWorking.Runs, item => item.Id.Value)
        };
        var mergedSnapshot = new RepositorySnapshot(
            mergedWorking,
            MergeById(current.Runbooks, source.Runbooks, item => item.Id.Value),
            MergeById(current.Triggers, source.Triggers, item => item.Id.Value));

        return new MergeResult(
            mergedSnapshot,
            conflicts,
            conflicts.Count == 0,
            conflicts.Count == 0
                ? $"Merged branch snapshot with no cognitive conflicts."
                : $"Merged branch snapshot with {conflicts.Count} cognitive conflicts requiring review.");
    }

    private static IReadOnlyList<CognitiveConflict> FindConflicts<T>(
        IEnumerable<T> current,
        IEnumerable<T> incoming,
        Func<T, string> idSelector,
        Func<T, string> summarySelector)
    {
        var currentMap = current.ToDictionary(idSelector, item => item);
        var incomingMap = incoming.ToDictionary(idSelector, item => item);
        var conflicts = new List<CognitiveConflict>();

        foreach (var pair in incomingMap)
        {
            if (!currentMap.TryGetValue(pair.Key, out var currentItem))
            {
                continue;
            }

            if (!Equals(currentItem, pair.Value))
            {
                conflicts.Add(new(
                    typeof(T).Name,
                    pair.Key,
                    "DivergentChange",
                    summarySelector(currentItem),
                    summarySelector(pair.Value)));
            }
        }

        return conflicts;
    }

    private static IReadOnlyList<T> MergeById<T>(IEnumerable<T> current, IEnumerable<T> incoming, Func<T, string> idSelector)
        => current.Concat(incoming).GroupBy(idSelector).Select(group => group.Last()).ToArray();
}
