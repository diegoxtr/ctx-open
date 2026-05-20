namespace Ctx.Cli;

using Ctx.Domain;

public static class CommandCoverage
{
    private static readonly string[] KnownCommands =
    [
        "version",
        "update",
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
        "next",
        "gaps",
        "roadmap",
        "check",
        "closeout",
        "preflight",
        "operational review",
        "line open",
        "runbook add",
        "runbook update",
        "runbook attach",
        "runbook detach",
        "runbook list",
        "runbook show",
        "prompt list",
        "trigger add",
        "trigger list",
        "trigger show",
        "goal add",
        "goal update",
        "goal list",
        "goal show",
        "epic add",
        "epic update",
        "epic promote",
        "epic list",
        "epic show",
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
        "decision update",
        "decision list",
        "decision show",
        "evidence add",
        "evidence share",
        "evidence list",
        "evidence show",
        "conclusion add",
        "conclusion update",
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
        "plan",
        "packet list",
        "packet show",
        "provider list",
        "metrics show",
        "bootstrap map",
        "bootstrap apply"
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
