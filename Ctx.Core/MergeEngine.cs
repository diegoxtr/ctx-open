namespace Ctx.Core;

using Ctx.Application;
using Ctx.Domain;

public sealed class MergeEngine : IMergeEngine
{
    public MergeResult Merge(WorkingContext current, ContextCommit sourceCommit)
    {
        var source = sourceCommit.Snapshot;
        var conflicts = new List<CognitiveConflict>();

        conflicts.AddRange(FindConflicts(current.Tasks, source.Tasks, item => item.Id.Value, item => $"{item.Title} [{item.State}]"));
        conflicts.AddRange(FindConflicts(current.Hypotheses, source.Hypotheses, item => item.Id.Value, item => $"{item.Statement} [{item.State}]"));
        conflicts.AddRange(FindConflicts(current.Decisions, source.Decisions, item => item.Id.Value, item => $"{item.Title} [{item.State}]"));
        conflicts.AddRange(FindConflicts(current.Evidence, source.Evidence, item => item.Id.Value, item => $"{item.Title} [{item.Kind}]"));
        conflicts.AddRange(FindConflicts(current.Conclusions, source.Conclusions, item => item.Id.Value, item => $"{item.Summary} [{item.State}]"));

        var merged = current with
        {
            Dirty = true,
            Goals = MergeById(current.Goals, source.Goals, item => item.Id.Value),
            Tasks = MergeById(current.Tasks, source.Tasks, item => item.Id.Value),
            Hypotheses = MergeById(current.Hypotheses, source.Hypotheses, item => item.Id.Value),
            Decisions = MergeById(current.Decisions, source.Decisions, item => item.Id.Value),
            Evidence = MergeById(current.Evidence, source.Evidence, item => item.Id.Value),
            Conclusions = MergeById(current.Conclusions, source.Conclusions, item => item.Id.Value),
            Runs = MergeById(current.Runs, source.Runs, item => item.Id.Value)
        };

        return new MergeResult(
            merged,
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
