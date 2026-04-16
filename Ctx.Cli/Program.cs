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
    return 1;
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
        "doctor" => await service.DoctorAsync(repositoryPath, cancellationToken),
        "audit" => await service.AuditAsync(repositoryPath, cancellationToken),
        "check" => await service.CheckAsync(repositoryPath, GetOption(args, "--task"), cancellationToken),
        "closeout" => await service.CloseoutAsync(repositoryPath, cancellationToken),
        "preflight" => await service.PreflightAsync(repositoryPath, RequireOption(args, "--operation"), GetOption(args, "--goal"), GetOption(args, "--task"), cancellationToken),
        "graph" when Match(args, "graph", "summary") => await service.GraphSummaryAsync(repositoryPath, cancellationToken),
        "graph" when Match(args, "graph", "show") => await service.GraphShowAsync(repositoryPath, RequirePositional(args, 2, "node id"), cancellationToken),
        "graph" when Match(args, "graph", "export") => await service.ExportGraphAsync(repositoryPath, GetOption(args, "--format") ?? "json", GetOption(args, "--commit"), cancellationToken),
        "graph" when Match(args, "graph", "lineage") => await DispatchGraphLineageAsync(args, service, repositoryPath, cancellationToken),
        "thread" when Match(args, "thread", "reconstruct") => await DispatchThreadReconstructAsync(args, service, repositoryPath, cancellationToken),
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
        "runbook" when Match(args, "runbook", "list") => await service.ListOperationalRunbooksAsync(repositoryPath, cancellationToken),
        "runbook" when Match(args, "runbook", "show") => await service.ShowOperationalRunbookAsync(repositoryPath, RequirePositional(args, 2, "runbook id"), cancellationToken),
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
        "goal" when Match(args, "goal", "list") => await service.ListArtifactsAsync(repositoryPath, "goal", cancellationToken),
        "goal" when Match(args, "goal", "show") => await service.ShowArtifactAsync(repositoryPath, "goal", RequirePositional(args, 2, "goal id"), cancellationToken),

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
                Environment.UserName),
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

    return $$"""
CTX - Cognitive Version Control System

Commands:
  ctx version
  ctx doctor
  ctx audit
  ctx check [--task <taskId>]
  ctx closeout
  ctx preflight --operation <git-closeout|publish-local|viewer-validation|recover-index-lock> [--goal <goalId>] [--task <taskId>]
  ctx graph summary
  ctx graph show <nodeId>
  ctx graph export [--format json|mermaid] [--commit <commitId>]
  ctx graph lineage --goal <id>
  ctx graph lineage --conclusion <id>
  ctx graph lineage --hypothesis <id>
  ctx graph lineage --decision <id>
  ctx graph lineage --task <id>
  ctx thread reconstruct --task <id> [--format json|markdown]
  ctx export [--output <path>]
    ctx import --input <path>
    ctx init --name <project> [--description <text>] [--branch <name>]
  ctx status
  ctx line open --goal <goalId> --title <text> [--description <text>] [--priority <n>] [--task-title <text>] [--task-description <text>]
  ctx runbook add --title <text> [--kind <Procedure|Troubleshooting|Policy|Guardrail>] --when <text> [--trigger <a,b>] [--do <a,b>] [--verify <a,b>] [--reference <a,b>] [--precondition <a,b>] [--signal <a,b>] [--escalate <a,b>] [--goal <id,id>] [--task <id,id>]
  ctx runbook list
  ctx runbook show <runbookId>
  ctx trigger add --summary <text> [--kind <UserPrompt|AgentPrompt|Continuation|RunbookTrigger|IssueTrigger>] [--text <text>] [--goal <id,id>] [--task <id,id>] [--runbook <id,id>]
  ctx trigger list
  ctx trigger show <triggerId>
  ctx goal add --title <text> [--description <text>] [--priority <n>] [--parent <goalId>]
  ctx goal list
  ctx goal show <goalId>
  ctx task add --title <text> [--description <text>] [--goal <goalId>] [--depends-on <taskId,taskId>]
  ctx task update <taskId> [--title <text>] [--description <text>] [--state <Draft|Ready|InProgress|Blocked|Done>]
  ctx task list
  ctx task show <taskId>
  ctx hypo add --statement <text> [--rationale <text>] [--confidence <0-1>|--probability <0-1>] [--impact <0-1>] [--evidence-strength <0-1>] [--cost-to-validate <0-1>] [--task <taskId>]
  ctx hypo update <hypothesisId> [--statement <text>] [--rationale <text>] [--confidence <0-1>|--probability <0-1>] [--impact <0-1>] [--evidence-strength <0-1>] [--cost-to-validate <0-1>] [--state <Proposed|UnderEvaluation|Supported|Refuted|Archived>]
  ctx hypo rank
  ctx hypo list
  ctx hypo show <hypothesisId>
  ctx decision add --title <text> [--rationale <text>] [--state <state>] [--hypotheses <id,id>] [--evidence <id,id>]
  ctx decision list
  ctx decision show <decisionId>
  ctx evidence add --title <text> [--summary <text>] [--source <text>] [--kind <kind>] [--confidence <0-1>] [--supports <type:id,type:id>]
  ctx evidence list
  ctx evidence show <evidenceId>
  ctx conclusion add --summary <text> [--state <state>] [--decisions <id,id>] [--evidence <id,id>] [--goals <id,id>] [--tasks <id,id>]
  ctx conclusion update <conclusionId> [--summary <text>] [--state <Draft|Accepted|Superseded>]
  ctx conclusion list
  ctx conclusion show <conclusionId>
  ctx run [--provider openai|anthropic] [--model <model>] [--purpose <text>] [--goal <goalId>] [--task <taskId>]
  ctx run list
  ctx run show <runId>
  ctx commit -m <message>
  ctx log
  ctx diff [fromCommitId] [toCommitId]
  ctx next
  ctx usage summary
  ctx usage coverage
  ctx branch <name>
  ctx checkout <name>
  ctx merge <sourceBranch>
  ctx context [--purpose <text>] [--goal <goalId>] [--task <taskId>]
  ctx packet list
  ctx packet show <packetId>
  ctx provider list
  ctx metrics show

{{projectContext}}
""";
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
