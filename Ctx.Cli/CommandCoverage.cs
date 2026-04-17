namespace Ctx.Cli;

using Ctx.Domain;

public static class CommandCoverage
{
    private static readonly string[] KnownCommands =
    [
        "version",
        "doctor",
        "audit",
        "graph summary",
        "graph show",
        "graph export",
        "graph lineage",
        "thread reconstruct",
        "export",
        "import",
        "init",
        "status",
        "line open",
        "goal add",
        "goal list",
        "goal show",
        "task add",
        "task update",
        "task list",
        "task show",
        "hypo add",
        "hypo update",
        "hypo relate",
        "hypo merge",
        "hypo supersede",
        "hypo rank",
        "hypo list",
        "hypo show",
        "decision add",
        "decision list",
        "decision show",
        "evidence add",
        "evidence share",
        "evidence list",
        "evidence show",
        "conclusion add",
        "conclusion list",
        "conclusion show",
        "run",
        "run list",
        "run show",
        "commit",
        "log",
        "diff",
        "usage summary",
        "usage coverage",
        "branch",
        "checkout",
        "merge",
        "context",
        "packet list",
        "packet show",
        "provider list",
        "metrics show"
    ];

    public static IReadOnlyList<string> GetKnownCommandNames() => KnownCommands;

    public static CommandCoverageReport Build(MetricsSnapshot snapshot)
    {
        var usedCommands = snapshot.CommandUsage
            .Select(item => item.Command)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(item => item, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var unusedCommands = KnownCommands
            .Where(item => !usedCommands.Contains(item, StringComparer.OrdinalIgnoreCase))
            .OrderBy(item => item, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var coveragePercentage = KnownCommands.Length == 0
            ? 0m
            : Math.Round((decimal)usedCommands.Length / KnownCommands.Length * 100m, 2);

        return new CommandCoverageReport(
            KnownCommands.Length,
            usedCommands.Length,
            unusedCommands.Length,
            coveragePercentage,
            usedCommands,
            unusedCommands);
    }
}

public sealed record CommandCoverageReport(
    int TotalKnownCommands,
    int UsedCommandCount,
    int UnusedCommandCount,
    decimal CoveragePercentage,
    IReadOnlyList<string> UsedCommands,
    IReadOnlyList<string> UnusedCommands);
