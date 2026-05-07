// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using Ctx.Application;
using Ctx.Infrastructure;

var runtime = Bootstrapper.Create();
var service = runtime.ApplicationService;
var repositoryPath = Directory.GetCurrentDirectory();
var argsList = args.ToList();

if (argsList.Count == 0)
{
    WriteHelp(repositoryPath);
    return 0;
}

try
{
    var stopwatch = Stopwatch.StartNew();
    CommandResult result = await DispatchAsync(argsList, service, repositoryPath);
    stopwatch.Stop();
    await RecordCommandTelemetryAsync(runtime, repositoryPath, argsList, result.Success, stopwatch.Elapsed);
    WriteResult(result, runtime.JsonOptions);
    return result.Success ? 0 : 1;
}
catch (Exception exception)
{
    await RecordCommandTelemetryAsync(runtime, repositoryPath, argsList, false, TimeSpan.Zero);
    Console.Error.WriteLine(JsonSerializer.Serialize(new
    {
        success = false,
        error = exception.Message,
        type = exception.GetType().Name
    }, runtime.JsonOptions));
    return 1;
}

static async Task<CommandResult> DispatchAsync(IReadOnlyList<string> args, ICtxApplicationService service, string repositoryPath)
{
    var cancellationToken = CancellationToken.None;
    var command = args[0].ToLowerInvariant();

    return command switch
    {
        "help" => new CommandResult(true, "CTX help.", BuildHelpText(repositoryPath)),
        "helper" => new CommandResult(true, "CTX helper.", BuildHelpText(repositoryPath)),
        "--help" => new CommandResult(true, "CTX help.", BuildHelpText(repositoryPath)),
        "-h" => new CommandResult(true, "CTX help.", BuildHelpText(repositoryPath)),
        "version" => new CommandResult(true, $"CTX {Ctx.Domain.DomainConstants.ProductVersion}", new
        {
            product = "CTX",
            version = Ctx.Domain.DomainConstants.ProductVersion,
            repositoryFormat = Ctx.Domain.DomainConstants.CurrentRepositoryVersion
        }),
        "usage" when Match(args, "usage", "summary") => await DispatchUsageSummaryAsync(service, repositoryPath, cancellationToken),
        "usage" when Match(args, "usage", "coverage") => await DispatchUsageCoverageAsync(service, repositoryPath, cancellationToken),
        "next" => await service.NextAsync(repositoryPath, cancellationToken),
        "gaps" => await service.GapsAsync(repositoryPath, cancellationToken),
        "roadmap" => await service.RoadmapAsync(repositoryPath, cancellationToken),
        "plan" => await service.PlanAsync(repositoryPath, GetOption(args, "--purpose") ?? "Plan next CTX work", GetOption(args, "--goal"), GetOption(args, "--task"), cancellationToken),
        "doctor" => await service.DoctorAsync(repositoryPath, cancellationToken),
        "audit" => await service.AuditAsync(repositoryPath, cancellationToken),
        "check" => await service.CheckAsync(repositoryPath, GetOption(args, "--task"), cancellationToken),
        "closeout" => await service.CloseoutAsync(repositoryPath, cancellationToken),
        "preflight" => await service.PreflightAsync(repositoryPath, RequireOption(args, "--operation"), GetOption(args, "--goal"), GetOption(args, "--task"), cancellationToken),
        "operational" when Match(args, "operational", "review") => await service.OperationalReviewAsync(
            repositoryPath,
            GetOption(args, "--operation"),
            int.TryParse(GetOption(args, "--threshold"), out var recurrenceThreshold) ? recurrenceThreshold : 2,
            cancellationToken),
        "graph" when Match(args, "graph", "summary") => await service.GraphSummaryAsync(repositoryPath, cancellationToken),
        "graph" when Match(args, "graph", "show") => await service.GraphShowAsync(repositoryPath, RequirePositional(args, 2, "node id"), cancellationToken),
        "graph" when Match(args, "graph", "export") => await service.ExportGraphAsync(repositoryPath, GetOption(args, "--format") ?? "json", GetOption(args, "--commit"), null, null, null, cancellationToken),
        "graph" when Match(args, "graph", "lineage") => await DispatchGraphLineageAsync(args, service, repositoryPath, cancellationToken),
        "thread" when Match(args, "thread", "reconstruct") => await DispatchThreadReconstructAsync(args, service, repositoryPath, cancellationToken),
        "bootstrap" when Match(args, "bootstrap", "map") => await service.BootstrapMapAsync(
            repositoryPath,
            new BootstrapMapRequest(
                RequireOption(args, "--from"),
                GetOption(args, "--mode") ?? "auto",
                int.TryParse(GetOption(args, "--max-files"), out var maxFiles) ? maxFiles : 12,
                Environment.UserName),
            cancellationToken),
        "bootstrap" when Match(args, "bootstrap", "apply") => await service.BootstrapApplyAsync(
            repositoryPath,
            new BootstrapApplyRequest(
                RequireOption(args, "--from"),
                GetOption(args, "--mode") ?? "auto",
                int.TryParse(GetOption(args, "--max-files"), out var applyMaxFiles) ? applyMaxFiles : 12,
                GetOption(args, "--parent-goal"),
                Environment.UserName),
            cancellationToken),
        "export" => await service.ExportAsync(repositoryPath, GetOption(args, "--output") ?? "ctx-export.json", cancellationToken),
        "import" => await service.ImportAsync(repositoryPath, RequireOption(args, "--input"), cancellationToken),

        "init" => await service.InitAsync(
            repositoryPath,
            new InitRepositoryRequest(
                GetOption(args, "--name") ?? new DirectoryInfo(repositoryPath).Name,
                GetOption(args, "--description") ?? "CTX cognitive repository",
                GetOption(args, "--branch") ?? "main",
                Environment.UserName),
            cancellationToken),

        "status" => await service.StatusAsync(repositoryPath, cancellationToken),
        "line" when Match(args, "line", "open") => await service.OpenWorkLineAsync(
            repositoryPath,
            new OpenWorkLineRequest(
                RequireOption(args, "--goal"),
                RequireOption(args, "--title"),
                GetOption(args, "--description") ?? string.Empty,
                int.TryParse(GetOption(args, "--priority"), out var linePriority) ? linePriority : null,
                GetOption(args, "--task-title"),
                GetOption(args, "--task-description"),
                Environment.UserName),
            cancellationToken),
        "runbook" when Match(args, "runbook", "add") => await service.AddOperationalRunbookAsync(
            repositoryPath,
            new AddOperationalRunbookRequest(
                RequireOption(args, "--title"),
                GetOption(args, "--kind") ?? "Procedure",
                GetMultiOption(args, "--trigger", "--triggers"),
                RequireOption(args, "--when"),
                GetMultiOption(args, "--do"),
                GetMultiOption(args, "--verify"),
                GetMultiOption(args, "--reference", "--references"),
                GetMultiOption(args, "--goal", "--goals"),
                GetMultiOption(args, "--task", "--tasks"),
                Environment.UserName,
                GetMultiOption(args, "--precondition", "--preconditions"),
                GetMultiOption(args, "--signal", "--signals", "--failure-signal", "--failure-signals"),
                GetMultiOption(args, "--escalate", "--escalation", "--escalation-boundary")),
            cancellationToken),
        "runbook" when Match(args, "runbook", "attach") => await service.AttachOperationalRunbookAsync(
            repositoryPath,
            new AttachOperationalRunbookRequest(
                RequirePositional(args, 2, "runbook id"),
                GetMultiOption(args, "--goal", "--goals"),
                GetMultiOption(args, "--task", "--tasks"),
                Environment.UserName),
            cancellationToken),
        "runbook" when Match(args, "runbook", "detach") => await service.DetachOperationalRunbookAsync(
            repositoryPath,
            new DetachOperationalRunbookRequest(
                RequirePositional(args, 2, "runbook id"),
                GetMultiOption(args, "--goal", "--goals"),
                GetMultiOption(args, "--task", "--tasks"),
                Environment.UserName),
            cancellationToken),
        "runbook" when Match(args, "runbook", "list") => await service.ListOperationalRunbooksAsync(repositoryPath, cancellationToken),
        "runbook" when Match(args, "runbook", "show") => await service.ShowOperationalRunbookAsync(repositoryPath, RequirePositional(args, 2, "runbook id"), cancellationToken),
        "prompt" when Match(args, "prompt", "list") => await service.ListPromptTimelineAsync(repositoryPath, GetOption(args, "--kind"), cancellationToken),
        "prompts" => await service.ListPromptTimelineAsync(repositoryPath, GetOption(args, "--kind"), cancellationToken),
        "trigger" when Match(args, "trigger", "add") => await service.AddCognitiveTriggerAsync(
            repositoryPath,
            new AddCognitiveTriggerRequest(
                GetOption(args, "--kind") ?? "UserPrompt",
                RequireOption(args, "--summary"),
                GetOption(args, "--text"),
                GetMultiOption(args, "--goal", "--goals"),
                GetMultiOption(args, "--task", "--tasks"),
                GetMultiOption(args, "--runbook", "--runbooks"),
                Environment.UserName),
            cancellationToken),
        "trigger" when Match(args, "trigger", "list") => await service.ListCognitiveTriggersAsync(repositoryPath, cancellationToken),
        "trigger" when Match(args, "trigger", "show") => await service.ShowCognitiveTriggerAsync(repositoryPath, RequirePositional(args, 2, "trigger id"), cancellationToken),

        "goal" when Match(args, "goal", "add") => await service.AddGoalAsync(
            repositoryPath,
            new AddGoalRequest(
                RequireOption(args, "--title"),
                GetOption(args, "--description") ?? string.Empty,
                int.TryParse(GetOption(args, "--priority"), out var priority) ? priority : 100,
                GetOption(args, "--parent"),
                Environment.UserName),
            cancellationToken),
        "goal" when Match(args, "goal", "update") => await service.UpdateGoalAsync(
            repositoryPath,
            new UpdateGoalRequest(
                RequirePositional(args, 2, "goal id"),
                GetOption(args, "--title"),
                GetOption(args, "--description"),
                int.TryParse(GetOption(args, "--priority"), out var goalPriority) ? goalPriority : null,
                GetOption(args, "--state"),
                Environment.UserName),
            cancellationToken),
        "goal" when Match(args, "goal", "list") => await service.ListArtifactsAsync(repositoryPath, "goal", cancellationToken),
        "goal" when Match(args, "goal", "show") => await service.ShowArtifactAsync(repositoryPath, "goal", RequirePositional(args, 2, "goal id"), cancellationToken),

        "epic" when Match(args, "epic", "add") => await service.AddEpicAsync(
            repositoryPath,
            new AddEpicRequest(
                RequireOption(args, "--title"),
                GetOption(args, "--description") ?? string.Empty,
                GetMultiOption(args, "--goal", "--goals"),
                Environment.UserName),
            cancellationToken),
        "epic" when Match(args, "epic", "update") => await service.UpdateEpicAsync(
            repositoryPath,
            new UpdateEpicRequest(
                RequirePositional(args, 2, "epic id"),
                GetOption(args, "--title"),
                GetOption(args, "--description"),
                GetOption(args, "--state"),
                HasOption(args, "--goal", "--goals") ? GetMultiOption(args, "--goal", "--goals") : null,
                Environment.UserName),
            cancellationToken),
        "epic" when Match(args, "epic", "promote") => await service.PromoteEpicAsync(
            repositoryPath,
            new PromoteEpicRequest(
                RequirePositional(args, 2, "epic id"),
                RequireOption(args, "--task-title"),
                GetOption(args, "--task-description") ?? GetOption(args, "--description"),
                GetOption(args, "--goal"),
                Environment.UserName),
            cancellationToken),
        "epic" when Match(args, "epic", "list") => await service.ListArtifactsAsync(repositoryPath, "epic", cancellationToken),
        "epic" when Match(args, "epic", "show") => await service.ShowArtifactAsync(repositoryPath, "epic", RequirePositional(args, 2, "epic id"), cancellationToken),

        "task" when Match(args, "task", "add") => await service.AddTaskAsync(
            repositoryPath,
            new AddTaskRequest(
                RequireOption(args, "--title"),
                GetOption(args, "--description") ?? string.Empty,
                GetOption(args, "--goal"),
                GetMultiOption(args, "--depends-on"),
                Environment.UserName,
                GetOption(args, "--parent")),
            cancellationToken),
        "task" when Match(args, "task", "update") => await service.UpdateTaskAsync(
            repositoryPath,
            new UpdateTaskRequest(
                RequirePositional(args, 2, "task id"),
                GetOption(args, "--title"),
                GetOption(args, "--description"),
                GetOption(args, "--state"),
                Environment.UserName,
                GetOption(args, "--goal")),
            cancellationToken),
        "task" when Match(args, "task", "list") => await service.ListArtifactsAsync(repositoryPath, "task", cancellationToken),
        "task" when Match(args, "task", "show") => await service.ShowArtifactAsync(repositoryPath, "task", RequirePositional(args, 2, "task id"), cancellationToken),

        "hypo" when Match(args, "hypo", "add") => await service.AddHypothesisAsync(
            repositoryPath,
            new AddHypothesisRequest(
                RequireOption(args, "--statement"),
                GetOption(args, "--rationale") ?? string.Empty,
                ParseDecimalOption(args, 0.5m, "--probability", "--confidence"),
                ParseDecimalOption(args, 0.5m, "--impact"),
                ParseDecimalOption(args, 0.5m, "--evidence-strength"),
                ParseDecimalOption(args, 0.5m, "--cost-to-validate"),
                GetOption(args, "--task"),
                Environment.UserName),
            cancellationToken),
        "hypo" when Match(args, "hypo", "update") => await service.UpdateHypothesisAsync(
            repositoryPath,
            new UpdateHypothesisRequest(
                RequirePositional(args, 2, "hypothesis id"),
                GetOption(args, "--statement"),
                GetOption(args, "--rationale"),
                TryGetDecimalOption(args, "--probability", "--confidence"),
                TryGetDecimalOption(args, "--impact"),
                TryGetDecimalOption(args, "--evidence-strength"),
                TryGetDecimalOption(args, "--cost-to-validate"),
                GetOption(args, "--state"),
                GetOption(args, "--branch-state"),
                GetOption(args, "--branch-role"),
                GetOption(args, "--lineage-group"),
                Environment.UserName),
            cancellationToken),
        "hypo" when Match(args, "hypo", "relate") => await service.RelateHypothesisAsync(
            repositoryPath,
            new RelateHypothesisRequest(
                RequirePositional(args, 2, "hypothesis id"),
                RequireOption(args, "--relation"),
                RequireOption(args, "--to"),
                GetOption(args, "--note"),
                Environment.UserName),
            cancellationToken),
        "hypo" when Match(args, "hypo", "merge") => await service.MergeHypothesisAsync(
            repositoryPath,
            new MergeHypothesisRequest(
                RequirePositional(args, 2, "source hypothesis id"),
                RequireOption(args, "--into"),
                Environment.UserName),
            cancellationToken),
        "hypo" when Match(args, "hypo", "supersede") => await service.SupersedeHypothesisAsync(
            repositoryPath,
            new SupersedeHypothesisRequest(
                RequirePositional(args, 2, "old hypothesis id"),
                RequireOption(args, "--by"),
                Environment.UserName),
            cancellationToken),
        "hypo" when Match(args, "hypo", "rank") => await service.RankHypothesesAsync(repositoryPath, cancellationToken),
        "hypo" when Match(args, "hypo", "list") => await service.ListArtifactsAsync(repositoryPath, "hypothesis", cancellationToken),
        "hypo" when Match(args, "hypo", "show") => await service.ShowArtifactAsync(repositoryPath, "hypothesis", RequirePositional(args, 2, "hypothesis id"), cancellationToken),

        "decision" when Match(args, "decision", "add") => await service.AddDecisionAsync(
            repositoryPath,
            new AddDecisionRequest(
                RequireOption(args, "--title"),
                GetOption(args, "--rationale") ?? string.Empty,
                GetOption(args, "--state") ?? "Proposed",
                GetMultiOption(args, "--hypothesis", "--hypotheses"),
                GetMultiOption(args, "--evidence"),
                Environment.UserName),
            cancellationToken),
        "decision" when Match(args, "decision", "update") => await service.UpdateDecisionAsync(
            repositoryPath,
            new UpdateDecisionRequest(
                RequirePositional(args, 2, "decision id"),
                GetOption(args, "--title"),
                GetOption(args, "--rationale"),
                GetOption(args, "--state"),
                HasOption(args, "--hypotheses", "--hypothesis") ? GetMultiOption(args, "--hypothesis", "--hypotheses") : null,
                HasOption(args, "--evidence") ? GetMultiOption(args, "--evidence") : null,
                Environment.UserName),
            cancellationToken),
        "decision" when Match(args, "decision", "list") => await service.ListArtifactsAsync(repositoryPath, "decision", cancellationToken),
        "decision" when Match(args, "decision", "show") => await service.ShowArtifactAsync(repositoryPath, "decision", RequirePositional(args, 2, "decision id"), cancellationToken),

        "evidence" when Match(args, "evidence", "add") => await service.AddEvidenceAsync(
            repositoryPath,
            new AddEvidenceRequest(
                RequireOption(args, "--title"),
                GetOption(args, "--summary") ?? string.Empty,
                GetOption(args, "--source") ?? "unspecified",
                GetOption(args, "--kind") ?? "Document",
                ParseDecimalOption(args, 0.5m, "--confidence"),
                GetMultiOption(args, "--supports"),
                Environment.UserName),
            cancellationToken),
        "evidence" when Match(args, "evidence", "share") => await service.ShareEvidenceAsync(
            repositoryPath,
            new ShareEvidenceRequest(
                RequirePositional(args, 2, "evidence id"),
                RequireOption(args, "--to"),
                Environment.UserName),
            cancellationToken),
        "evidence" when Match(args, "evidence", "list") => await service.ListArtifactsAsync(repositoryPath, "evidence", cancellationToken),
        "evidence" when Match(args, "evidence", "show") => await service.ShowArtifactAsync(repositoryPath, "evidence", RequirePositional(args, 2, "evidence id"), cancellationToken),

        "conclusion" when Match(args, "conclusion", "add") => await service.AddConclusionAsync(
            repositoryPath,
            new AddConclusionRequest(
                RequireOption(args, "--summary"),
                GetOption(args, "--state") ?? "Draft",
                GetMultiOption(args, "--decision", "--decisions"),
                GetMultiOption(args, "--evidence"),
                GetMultiOption(args, "--goal", "--goals"),
                GetMultiOption(args, "--task", "--tasks"),
                Environment.UserName),
            cancellationToken),
        "conclusion" when Match(args, "conclusion", "update") => await service.UpdateConclusionAsync(
            repositoryPath,
            new UpdateConclusionRequest(
                RequirePositional(args, 2, "conclusion id"),
                GetOption(args, "--summary"),
                GetOption(args, "--state"),
                Environment.UserName),
            cancellationToken),
        "conclusion" when Match(args, "conclusion", "list") => await service.ListArtifactsAsync(repositoryPath, "conclusion", cancellationToken),
        "conclusion" when Match(args, "conclusion", "show") => await service.ShowArtifactAsync(repositoryPath, "conclusion", RequirePositional(args, 2, "conclusion id"), cancellationToken),

        "run" when Match(args, "run", "list") => await service.ListRunsAsync(repositoryPath, cancellationToken),
        "run" when Match(args, "run", "show") => await service.ShowRunAsync(repositoryPath, RequirePositional(args, 2, "run id"), cancellationToken),
        "run" => await service.RunAsync(
            repositoryPath,
            new RunRequest(
                GetOption(args, "--provider") ?? "openai",
                GetOption(args, "--purpose") ?? "Advance the current reasoning state.",
                GetOption(args, "--model") ?? "gpt-4.1",
                GetOption(args, "--goal"),
                GetOption(args, "--task"),
                Environment.UserName),
            cancellationToken),

        "commit" => await service.CommitAsync(
            repositoryPath,
            new CommitRequest(
                RequireOption(args, "-m", "--message"),
                Environment.UserName),
            cancellationToken),

        "log" => await service.LogAsync(repositoryPath, cancellationToken),

        "diff" => await service.DiffAsync(
            repositoryPath,
            GetPositional(args, 1),
            GetPositional(args, 2),
            cancellationToken),

        "branch" => await service.BranchAsync(repositoryPath, RequirePositional(args, 1, "branch name"), cancellationToken),
        "checkout" => await service.CheckoutAsync(repositoryPath, RequirePositional(args, 1, "branch name"), cancellationToken),
        "merge" => await service.MergeAsync(repositoryPath, RequirePositional(args, 1, "source branch"), cancellationToken),

        "context" => await service.ContextAsync(
            repositoryPath,
            GetOption(args, "--purpose") ?? "Summarize current reasoning context.",
            GetOption(args, "--goal"),
            GetOption(args, "--task"),
            cancellationToken),
        "packet" when Match(args, "packet", "list") => await service.ListPacketsAsync(repositoryPath, cancellationToken),
        "packet" when Match(args, "packet", "show") => await service.ShowPacketAsync(repositoryPath, RequirePositional(args, 2, "packet id"), cancellationToken),
        "provider" when Match(args, "provider", "list") => await service.ListProvidersAsync(repositoryPath, cancellationToken),
        "metrics" when Match(args, "metrics", "show") => await service.ShowMetricsAsync(repositoryPath, cancellationToken),

        _ => throw new InvalidOperationException("Unknown command. Run `ctx` with no arguments to see help.")
    };
}

static bool Match(IReadOnlyList<string> args, string first, string second)
    => args.Count >= 2 && args[0].Equals(first, StringComparison.OrdinalIgnoreCase) && args[1].Equals(second, StringComparison.OrdinalIgnoreCase);

static bool HasOption(IReadOnlyList<string> args, params string[] names)
    => args.Any(arg => names.Contains(arg, StringComparer.OrdinalIgnoreCase));

static string? GetOption(IReadOnlyList<string> args, params string[] names)
{
    for (var i = 0; i < args.Count; i++)
    {
        if (!names.Contains(args[i], StringComparer.OrdinalIgnoreCase))
        {
            continue;
        }

        if (i + 1 >= args.Count)
        {
            throw new InvalidOperationException($"Missing value for option '{args[i]}'.");
        }

        return args[i + 1];
    }

    return null;
}

static string RequireOption(IReadOnlyList<string> args, params string[] names)
    => GetOption(args, names) ?? throw new InvalidOperationException($"Missing required option '{names[0]}'.");

static IReadOnlyList<string> GetMultiOption(IReadOnlyList<string> args, params string[] names)
{
    var value = GetOption(args, names);
    if (string.IsNullOrWhiteSpace(value))
    {
        return Array.Empty<string>();
    }

    return value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
}

static decimal? TryGetDecimalOption(IReadOnlyList<string> args, params string[] names)
{
    var value = GetOption(args, names);
    return TryParseDecimal(value, out var parsed) ? parsed : null;
}

static decimal ParseDecimalOption(IReadOnlyList<string> args, decimal defaultValue, params string[] names)
{
    var value = GetOption(args, names);
    return TryParseDecimal(value, out var parsed) ? parsed : defaultValue;
}

static bool TryParseDecimal(string? value, out decimal parsed)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        parsed = default;
        return false;
    }

    return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out parsed)
        || decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out parsed);
}

static string? GetPositional(IReadOnlyList<string> args, int index)
{
    var filtered = args.Where(item => !item.StartsWith('-')).ToArray();
    return filtered.Length > index ? filtered[index] : null;
}

static string RequirePositional(IReadOnlyList<string> args, int index, string label)
    => GetPositional(args, index) ?? throw new InvalidOperationException($"Missing required {label}.");

static Task<CommandResult> DispatchGraphLineageAsync(
    IReadOnlyList<string> args,
    ICtxApplicationService service,
    string repositoryPath,
    CancellationToken cancellationToken)
{
    var hypothesisId = GetOption(args, "--hypothesis");
    var decisionId = GetOption(args, "--decision");

    var supplied = new[]
    {
        (Type: "goal", Id: GetOption(args, "--goal")),
        (Type: "conclusion", Id: GetOption(args, "--conclusion")),
        (Type: "hypothesis", Id: hypothesisId),
        (Type: "decision", Id: decisionId),
        (Type: "task", Id: GetOption(args, "--task"))
    }
    .Where(item => !string.IsNullOrWhiteSpace(item.Id))
    .ToArray();

    if (supplied.Length != 1)
    {
        throw new InvalidOperationException("Graph lineage requires exactly one focus option: --goal <id>, --task <id>, --hypothesis <id>, --decision <id> or --conclusion <id>.");
    }

    return service.GraphLineageAsync(
        repositoryPath,
        supplied[0].Type,
        supplied[0].Id!,
        GetOption(args, "--format") ?? "json",
        GetOption(args, "--output"),
        cancellationToken);
}

static Task<CommandResult> DispatchThreadReconstructAsync(
    IReadOnlyList<string> args,
    ICtxApplicationService service,
    string repositoryPath,
    CancellationToken cancellationToken)
{
    var supplied = new[]
    {
        (Type: "task", Id: GetOption(args, "--task"))
    }
    .Where(item => !string.IsNullOrWhiteSpace(item.Id))
    .ToArray();

    if (supplied.Length != 1)
    {
        throw new InvalidOperationException("Thread reconstruction currently requires exactly one focus option: --task <id>.");
    }

    return service.ThreadReconstructAsync(
        repositoryPath,
        supplied[0].Type,
        supplied[0].Id!,
        GetOption(args, "--format") ?? "json",
        cancellationToken);
}

static async Task<CommandResult> DispatchUsageSummaryAsync(
    ICtxApplicationService service,
    string repositoryPath,
    CancellationToken cancellationToken)
{
    var metricsResult = await service.ShowMetricsAsync(repositoryPath, cancellationToken);
    if (!metricsResult.Success)
    {
        return metricsResult;
    }

    var metrics = (Ctx.Domain.MetricsSnapshot)metricsResult.Data!;
    var commands = metrics.CommandUsage
        .OrderByDescending(item => item.TotalInvocations)
        .ThenBy(item => item.Command, StringComparer.OrdinalIgnoreCase)
        .Select(item => new
        {
            command = item.Command,
            totalInvocations = item.TotalInvocations,
            successfulInvocations = item.SuccessfulInvocations,
            failedInvocations = item.FailedInvocations,
            totalExecutionTimeMs = Math.Round(item.TotalExecutionTime.TotalMilliseconds, 2),
            averageExecutionTimeMs = item.TotalInvocations == 0
                ? 0d
                : Math.Round(item.TotalExecutionTime.TotalMilliseconds / item.TotalInvocations, 2),
            item.LastInvokedAtUtc,
            item.LastOutcome
        })
        .ToArray();

    var coverage = Ctx.Cli.CommandCoverage.Build(metrics);

    return new CommandResult(true, "Command usage summary generated.", new
    {
        totalCommandInvocations = metrics.TotalCommandInvocations,
        uniqueCommands = metrics.CommandUsage.Count,
        commands,
        usedCommandCount = coverage.UsedCommandCount,
        unusedCommandCount = coverage.UnusedCommandCount,
        coveragePercentage = coverage.CoveragePercentage,
        unusedCommands = coverage.UnusedCommands
    });
}

static async Task<CommandResult> DispatchUsageCoverageAsync(
    ICtxApplicationService service,
    string repositoryPath,
    CancellationToken cancellationToken)
{
    var metricsResult = await service.ShowMetricsAsync(repositoryPath, cancellationToken);
    if (!metricsResult.Success)
    {
        return metricsResult;
    }

    var metrics = (Ctx.Domain.MetricsSnapshot)metricsResult.Data!;
    var coverage = Ctx.Cli.CommandCoverage.Build(metrics);

    return new CommandResult(true, "Command usage coverage generated.", new
    {
        totalKnownCommands = coverage.TotalKnownCommands,
        usedCommandCount = coverage.UsedCommandCount,
        unusedCommandCount = coverage.UnusedCommandCount,
        coveragePercentage = coverage.CoveragePercentage,
        usedCommands = coverage.UsedCommands,
        unusedCommands = coverage.UnusedCommands
    });
}

static void WriteResult(CommandResult result, JsonSerializerOptions options)
{
    if (result.Data is string text)
    {
        Console.WriteLine(text);
        return;
    }

    Console.WriteLine(JsonSerializer.Serialize(new
    {
        success = result.Success,
        message = result.Message,
        data = result.Data
    }, options));
}

static void WriteHelp(string repositoryPath)
{
    Console.WriteLine(BuildHelpText(repositoryPath));
}

static string BuildHelpText(string repositoryPath)
{
    var projectRoot = ResolveProjectRoot(repositoryPath, AppContext.BaseDirectory);
    var projectContext = BuildProjectContextText(projectRoot);
    var cliCommandsDoc = Path.Combine(projectRoot, "docs", "CLI_COMMANDS.md");
    var helpState = BuildHelpState(repositoryPath, projectRoot);
    var branchLine = !string.IsNullOrWhiteSpace(helpState.Branch) ? $"  Branch: {helpState.Branch}" : string.Empty;
    var dirtyLine = helpState.Dirty.HasValue ? $"  Dirty: {helpState.Dirty.Value}" : string.Empty;
    var openTasksLine = helpState.OpenTaskCount.HasValue ? $"  Open tasks: {helpState.OpenTaskCount.Value}" : string.Empty;
    var followupLine = !string.IsNullOrWhiteSpace(helpState.FollowupCommand)
        ? $"  Follow-up: {helpState.FollowupCommand}"
        : string.Empty;

    return $$"""
CTX - Cognitive Version Control System by Diego Mariano Verrastro

Current State:
  {{helpState.Label}}
  Meaning: {{helpState.Meaning}}
{{branchLine}}
{{dirtyLine}}
{{openTasksLine}}

Next Command:
  {{helpState.NextCommand}}
{{followupLine}}

Operate From Plan:
  ctx plan --purpose "<current intent>"
  If a task is known: ctx plan --task <taskId> --purpose "<current work>"
  Read data.runbookSuggestions first. That is the effective playbook list for the turn.
  Apply returned runbooks by checking Preconditions, following Do, validating Verify, and stopping at EscalationBoundary.

State Machine:
  - No CTX repository:
      ctx init --name "<project>"
  - Existing repo + open work:
      ctx plan --purpose "<current intent>"
  - Existing repo + pending cognitive delta:
      ctx closeout
  - Existing repo + durable block ready:
      ctx commit -m "<message>"
  - Existing repo + no open work:
      ctx next, then ctx gaps or ctx roadmap if no executable work exists

{{projectContext}}

Core Commands:
  ctx status            inspect current cognitive state
  ctx audit             consistency check before continuing
  ctx next              CTX-prioritized next step
  ctx check             closure and runbook check for a task
  ctx context           focused context packet for a goal or task
  ctx plan              planning packet: state, next, context, runbooks, guidance
  ctx gaps              read-only unresolved planning gaps
  ctx roadmap           read-only future planning lanes
  ctx epic add|list|promote
                        durable future planning that stays out of ctx next until promoted
  ctx prompt list       prompt/trigger timeline ordered by creation date
  ctx runbook list      list operational playbooks
  ctx closeout          review what still separates working state from HEAD
  ctx commit -m "..."   durable cognitive snapshot
  ctx helper            show this operator guide again

Common Surfaces:
  ctx bootstrap map|apply ...
  ctx graph summary|show|export|lineage ...
  ctx thread reconstruct ...
  ctx preflight --operation <...>
  ctx operational review --operation <...> [--threshold 2]

MCP Agent Surface:
  ctx_plan              first MCP call for planning packets
  ctx_gaps              MCP read-only planning gaps
  ctx_roadmap           MCP read-only future planning lanes
  ctx_preflight         MCP runbook-aware operation guidance
  ctx_epic_*            MCP epic list/show/add/update/promote parity

Full Command Reference:
  {{cliCommandsDoc}}

Branch-like hypothesis reminder:
  Hypothesis branch semantics live inside hypothesis lineage first.
  Use branch-state, branch-role, relations, merge, supersede, and evidence share to preserve competing interpretations.
  Do not treat these as repository branches yet.
""";
}

static (string Label, string Meaning, string NextCommand, string? FollowupCommand, string? Branch, bool? Dirty, int? OpenTaskCount) BuildHelpState(string repositoryPath, string projectRoot)
{
    var ctxFolder = Path.Combine(repositoryPath, Ctx.Domain.DomainConstants.RepositoryFolderName);
    if (!Directory.Exists(ctxFolder))
    {
        var projectName = new DirectoryInfo(projectRoot).Name;
        return (
            "No CTX repository detected",
            "This folder does not have a .ctx workspace yet. Start by initializing the cognitive repository before creating goals, tasks, or hypotheses.",
            $"ctx init --name \"{projectName}\"",
            "If source material already exists, continue with: ctx bootstrap map --from <path>",
            null,
            null,
            null);
    }

    var workingPath = Path.Combine(ctxFolder, "working", "working-context.json");
    if (!File.Exists(workingPath))
    {
        return (
            "CTX repository detected but working state is missing",
            "The repository has .ctx but the working context file is missing or incomplete. Inspect repository health before continuing.",
            "ctx doctor",
            "Then run: ctx audit",
            null,
            null,
            null);
    }

    try
    {
        using var document = JsonDocument.Parse(File.ReadAllText(workingPath));
        var root = document.RootElement;
        var branch = root.TryGetProperty("currentBranch", out var branchProperty) ? branchProperty.GetString() : null;
        var dirty = root.TryGetProperty("dirty", out var dirtyProperty) && dirtyProperty.ValueKind is JsonValueKind.True or JsonValueKind.False
            ? dirtyProperty.GetBoolean()
            : false;

        var openTaskCount = 0;
        if (root.TryGetProperty("tasks", out var tasksProperty) && tasksProperty.ValueKind == JsonValueKind.Array)
        {
            foreach (var task in tasksProperty.EnumerateArray())
            {
                if (!task.TryGetProperty("state", out var stateProperty))
                {
                    continue;
                }

                var isDone = stateProperty.ValueKind switch
                {
                    JsonValueKind.Number => stateProperty.TryGetInt32(out var stateNumber) && stateNumber == 4,
                    JsonValueKind.String => string.Equals(stateProperty.GetString(), "Done", StringComparison.OrdinalIgnoreCase),
                    _ => false
                };

                if (!isDone)
                {
                    openTaskCount += 1;
                }
            }
        }

        if (dirty)
        {
            return (
                "Existing CTX repository with pending cognitive changes",
                "Working context contains unsnapshotted cognitive delta. Review the pending block before deciding whether it is ready for a durable commit.",
                "ctx closeout",
                "Then inspect: ctx status",
                branch,
                dirty,
                openTaskCount);
        }

        if (openTaskCount > 0)
        {
            return (
                "Existing CTX repository with open work",
                "There are active tasks or draft/ready/in-progress lines in this workspace. Continue from CTX instead of guessing from chat.",
                "ctx next",
                "Then inspect: ctx check --task <taskId>",
                branch,
                dirty,
                openTaskCount);
        }

        return (
            "Existing CTX repository with no open work",
            "No open tasks are currently active. Ask CTX for the strongest remaining gap or confirm that the workspace is fully closed.",
            "ctx next",
            "Then inspect: ctx audit",
            branch,
            dirty,
            openTaskCount);
    }
    catch
    {
        return (
            "CTX repository detected but state could not be read",
            "The helper could not parse the current working context. Inspect repository health before continuing.",
            "ctx doctor",
            "Then run: ctx audit",
            null,
            null,
            null);
    }
}

static string BuildProjectContextText(string projectRoot)
{
    var templatePath = Path.Combine(projectRoot, "prompts", "CTX_HELPER_PROMPT.md");
    var fallback = """
Project Context:
  We are continuing from previous context. Re-anchor on CTX before planning from chat.
  CTX local install root: C:\ctx (if you exported the binaries there)
  Local viewer URL: http://127.0.0.1:5271
  Active project root: {{projectRoot}}

Read These First:
  {{viewerGuide}}
  {{agentPrompt}}
  {{autonomousProtocol}}

Operating Reminder:
  Analyze those files and use them for everything in this project.
  Align the existing Playbook/runbook guidance before drifting into ad-hoc operation.
  If CTX already knows what's next, continue from CTX instead of waiting for chat.
""";

    var viewerGuide = Path.Combine(projectRoot, "docs", "CTX_VIEWER_GUIDE.md");
    var agentPrompt = Path.Combine(projectRoot, "prompts", "CTX_AGENT_PROMPT.md");
    var autonomousProtocol = Path.Combine(projectRoot, "docs", "CTX_AUTONOMOUS_OPERATION_PROTOCOL.md");
    var content = File.Exists(templatePath)
        ? File.ReadAllText(templatePath)
        : fallback;

    return content
        .Replace("{{projectRoot}}", projectRoot, StringComparison.Ordinal)
        .Replace("{{viewerGuide}}", viewerGuide, StringComparison.Ordinal)
        .Replace("{{agentPrompt}}", agentPrompt, StringComparison.Ordinal)
        .Replace("{{autonomousProtocol}}", autonomousProtocol, StringComparison.Ordinal)
        .Replace("{{viewerUrl}}", "http://127.0.0.1:5271", StringComparison.Ordinal)
        .Replace("{{ctxRoot}}", @"C:\ctx", StringComparison.Ordinal);
}

static string ResolveProjectRoot(params string[] candidatePaths)
{
    foreach (var candidatePath in candidatePaths.Where(path => !string.IsNullOrWhiteSpace(path)))
    {
        var root = TryResolveProjectRoot(candidatePath);
        if (!string.IsNullOrWhiteSpace(root))
        {
            return root;
        }
    }

    return candidatePaths.FirstOrDefault(path => !string.IsNullOrWhiteSpace(path)) ?? Directory.GetCurrentDirectory();
}

static string? TryResolveProjectRoot(string candidatePath)
{
    var current = new DirectoryInfo(File.Exists(candidatePath) ? Path.GetDirectoryName(candidatePath)! : candidatePath);

    while (current is not null)
    {
        if (Directory.Exists(Path.Combine(current.FullName, ".git"))
            || File.Exists(Path.Combine(current.FullName, "Ctx.sln"))
            || File.Exists(Path.Combine(current.FullName, "ctx-install.json"))
            || File.Exists(Path.Combine(current.FullName, "prompts", "CTX_HELPER_PROMPT.md")))
        {
            return current.FullName;
        }

        current = current.Parent;
    }

    return null;
}

static async Task RecordCommandTelemetryAsync(
    CtxRuntime runtime,
    string repositoryPath,
    IReadOnlyList<string> args,
    bool success,
    TimeSpan duration)
{
    if (!Directory.Exists(Path.Combine(repositoryPath, Ctx.Domain.DomainConstants.RepositoryFolderName)))
    {
        return;
    }

    try
    {
        var snapshot = await runtime.MetricsRepository.LoadAsync(repositoryPath, CancellationToken.None);
        var updated = snapshot.RecordCommandUsage(BuildCommandTelemetryName(args), success, duration, DateTimeOffset.UtcNow);
        await runtime.MetricsRepository.SaveAsync(repositoryPath, updated, CancellationToken.None);
    }
    catch
    {
        // Telemetry is best-effort and must not break normal CLI operation.
    }
}

static string BuildCommandTelemetryName(IReadOnlyList<string> args)
{
    var positionals = args.Where(item => !item.StartsWith('-')).ToArray();
    if (positionals.Length == 0)
    {
        return "help";
    }

    if (positionals.Length == 1)
    {
        return positionals[0].ToLowerInvariant();
    }

    return positionals[0].ToLowerInvariant() switch
    {
        "goal" or "task" or "hypo" or "decision" or "evidence" or "conclusion" or "graph" or "thread" or "run" or "packet" or "provider" or "metrics" or "usage"
            => $"{positionals[0].ToLowerInvariant()} {positionals[1].ToLowerInvariant()}",
        _ => positionals[0].ToLowerInvariant()
    };
}
