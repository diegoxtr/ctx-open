namespace Ctx.Mcp.Mcp;

using System.ComponentModel;
using Ctx.Application;
using ModelContextProtocol.Server;

[McpServerToolType]
public static class CtxWriteTools
{
    [McpServerTool(Name = "ctx_init", ReadOnly = false, Destructive = false), Description("Initialize a CTX repository. Requires ctx-mcp --mode write.")]
    public static Task<CommandResult> InitAsync(
        ICtxApplicationService service,
        RepositoryGuard guard,
        CancellationToken cancellationToken,
        string name,
        string description = "",
        string branch = "main",
        string createdBy = "mcp-agent",
        string? repo = null)
        => service.InitAsync(
            guard.ResolveWritableInitialization(repo),
            new InitRepositoryRequest(name, description, branch, createdBy),
            cancellationToken);

    [McpServerTool(Name = "ctx_goal_add", ReadOnly = false, Destructive = false), Description("Add a CTX goal. Requires ctx-mcp --mode write.")]
    public static Task<CommandResult> AddGoalAsync(
        ICtxApplicationService service,
        RepositoryGuard guard,
        CancellationToken cancellationToken,
        [Description("Goal title.")] string title,
        string description = "",
        int priority = 100,
        string? parentGoalId = null,
        string createdBy = "mcp-agent",
        string? repo = null)
        => service.AddGoalAsync(
            guard.ResolveWritable(repo),
            new AddGoalRequest(title, description, priority, parentGoalId, createdBy),
            cancellationToken);

    [McpServerTool(Name = "ctx_goal_update", ReadOnly = false, Destructive = false), Description("Update a CTX goal. Requires ctx-mcp --mode write.")]
    public static Task<CommandResult> UpdateGoalAsync(
        ICtxApplicationService service,
        RepositoryGuard guard,
        CancellationToken cancellationToken,
        string goalId,
        string? title = null,
        string? description = null,
        int? priority = null,
        string? state = null,
        string updatedBy = "mcp-agent",
        string? repo = null)
        => service.UpdateGoalAsync(
            guard.ResolveWritable(repo),
            new UpdateGoalRequest(goalId, title, description, priority, state, updatedBy),
            cancellationToken);

    [McpServerTool(Name = "ctx_epic_add", ReadOnly = false, Destructive = false), Description("Add a CTX epic. Requires ctx-mcp --mode write.")]
    public static Task<CommandResult> AddEpicAsync(
        ICtxApplicationService service,
        RepositoryGuard guard,
        CancellationToken cancellationToken,
        [Description("Epic title.")] string title,
        string description = "",
        string[]? goalIds = null,
        string createdBy = "mcp-agent",
        string? repo = null)
        => service.AddEpicAsync(
            guard.ResolveWritable(repo),
            new AddEpicRequest(title, description, Normalize(goalIds), createdBy),
            cancellationToken);

    [McpServerTool(Name = "ctx_epic_update", ReadOnly = false, Destructive = false), Description("Update a CTX epic. Requires ctx-mcp --mode write.")]
    public static Task<CommandResult> UpdateEpicAsync(
        ICtxApplicationService service,
        RepositoryGuard guard,
        CancellationToken cancellationToken,
        string epicId,
        string? title = null,
        string? description = null,
        string? state = null,
        string[]? goalIds = null,
        string updatedBy = "mcp-agent",
        string? repo = null)
        => service.UpdateEpicAsync(
            guard.ResolveWritable(repo),
            new UpdateEpicRequest(epicId, title, description, state, goalIds is null ? null : Normalize(goalIds), updatedBy),
            cancellationToken);

    [McpServerTool(Name = "ctx_epic_promote", ReadOnly = false, Destructive = false), Description("Promote a CTX epic into an executable task. Requires ctx-mcp --mode write.")]
    public static Task<CommandResult> PromoteEpicAsync(
        ICtxApplicationService service,
        RepositoryGuard guard,
        CancellationToken cancellationToken,
        string epicId,
        string taskTitle,
        string? taskDescription = null,
        string? goalId = null,
        string createdBy = "mcp-agent",
        string? repo = null)
        => service.PromoteEpicAsync(
            guard.ResolveWritable(repo),
            new PromoteEpicRequest(epicId, taskTitle, taskDescription, goalId, createdBy),
            cancellationToken);

    [McpServerTool(Name = "ctx_line_open", ReadOnly = false, Destructive = false), Description("Open a tactical CTX work line under an existing parent goal. Requires ctx-mcp --mode write.")]
    public static Task<CommandResult> OpenLineAsync(
        ICtxApplicationService service,
        RepositoryGuard guard,
        CancellationToken cancellationToken,
        string parentGoalId,
        string title,
        string description = "",
        int? priority = null,
        string? taskTitle = null,
        string? taskDescription = null,
        string createdBy = "mcp-agent",
        string? repo = null)
        => service.OpenWorkLineAsync(
            guard.ResolveWritable(repo),
            new OpenWorkLineRequest(parentGoalId, title, description, priority, taskTitle, taskDescription, createdBy),
            cancellationToken);

    [McpServerTool(Name = "ctx_task_add", ReadOnly = false, Destructive = false), Description("Add a CTX task. Requires ctx-mcp --mode write.")]
    public static Task<CommandResult> AddTaskAsync(
        ICtxApplicationService service,
        RepositoryGuard guard,
        CancellationToken cancellationToken,
        string title,
        string description = "",
        string? goalId = null,
        string[]? dependsOnTaskIds = null,
        string? parentTaskId = null,
        string createdBy = "mcp-agent",
        string? repo = null)
        => service.AddTaskAsync(
            guard.ResolveWritable(repo),
            new AddTaskRequest(title, description, goalId, Normalize(dependsOnTaskIds), createdBy, parentTaskId),
            cancellationToken);

    [McpServerTool(Name = "ctx_task_update", ReadOnly = false, Destructive = false), Description("Update a CTX task title, description, state, or goal. Requires ctx-mcp --mode write.")]
    public static Task<CommandResult> UpdateTaskAsync(
        ICtxApplicationService service,
        RepositoryGuard guard,
        CancellationToken cancellationToken,
        string taskId,
        string? title = null,
        string? description = null,
        string? state = null,
        string? goalId = null,
        string updatedBy = "mcp-agent",
        string? repo = null)
        => service.UpdateTaskAsync(
            guard.ResolveWritable(repo),
            new UpdateTaskRequest(taskId, title, description, state, updatedBy, goalId),
            cancellationToken);

    [McpServerTool(Name = "ctx_hypothesis_add", ReadOnly = false, Destructive = false), Description("Add a CTX hypothesis. Requires ctx-mcp --mode write.")]
    public static Task<CommandResult> AddHypothesisAsync(
        ICtxApplicationService service,
        RepositoryGuard guard,
        CancellationToken cancellationToken,
        string statement,
        string rationale = "",
        decimal confidence = 0.5m,
        decimal impact = 0.5m,
        decimal evidenceStrength = 0.5m,
        decimal costToValidate = 0.5m,
        string? taskId = null,
        string createdBy = "mcp-agent",
        string? repo = null)
        => service.AddHypothesisAsync(
            guard.ResolveWritable(repo),
            new AddHypothesisRequest(statement, rationale, confidence, impact, evidenceStrength, costToValidate, taskId, createdBy),
            cancellationToken);

    [McpServerTool(Name = "ctx_hypothesis_update", ReadOnly = false, Destructive = false), Description("Update a CTX hypothesis. Requires ctx-mcp --mode write.")]
    public static Task<CommandResult> UpdateHypothesisAsync(
        ICtxApplicationService service,
        RepositoryGuard guard,
        CancellationToken cancellationToken,
        string hypothesisId,
        string? statement = null,
        string? rationale = null,
        decimal? confidence = null,
        decimal? impact = null,
        decimal? evidenceStrength = null,
        decimal? costToValidate = null,
        string? state = null,
        string? branchState = null,
        string? branchRole = null,
        string? lineageGroupId = null,
        string updatedBy = "mcp-agent",
        string? repo = null)
        => service.UpdateHypothesisAsync(
            guard.ResolveWritable(repo),
            new UpdateHypothesisRequest(hypothesisId, statement, rationale, confidence, impact, evidenceStrength, costToValidate, state, branchState, branchRole, lineageGroupId, updatedBy),
            cancellationToken);

    [McpServerTool(Name = "ctx_evidence_add", ReadOnly = false, Destructive = false), Description("Add CTX evidence and optionally attach it to supported entities. Requires ctx-mcp --mode write.")]
    public static Task<CommandResult> AddEvidenceAsync(
        ICtxApplicationService service,
        RepositoryGuard guard,
        CancellationToken cancellationToken,
        string title,
        string summary = "",
        string source = "mcp-agent",
        string kind = "Document",
        decimal confidence = 0.5m,
        string[]? supports = null,
        string createdBy = "mcp-agent",
        string? repo = null)
        => service.AddEvidenceAsync(
            guard.ResolveWritable(repo),
            new AddEvidenceRequest(title, summary, source, kind, confidence, Normalize(supports), createdBy),
            cancellationToken);

    [McpServerTool(Name = "ctx_evidence_share", ReadOnly = false, Destructive = false), Description("Share existing CTX evidence to another hypothesis or supported entity. Requires ctx-mcp --mode write.")]
    public static Task<CommandResult> ShareEvidenceAsync(
        ICtxApplicationService service,
        RepositoryGuard guard,
        CancellationToken cancellationToken,
        string evidenceId,
        string targetReference,
        string updatedBy = "mcp-agent",
        string? repo = null)
        => service.ShareEvidenceAsync(
            guard.ResolveWritable(repo),
            new ShareEvidenceRequest(evidenceId, targetReference, updatedBy),
            cancellationToken);

    [McpServerTool(Name = "ctx_hypothesis_relate", ReadOnly = false, Destructive = false), Description("Relate two CTX hypotheses. Requires ctx-mcp --mode write.")]
    public static Task<CommandResult> RelateHypothesisAsync(
        ICtxApplicationService service,
        RepositoryGuard guard,
        CancellationToken cancellationToken,
        string hypothesisId,
        string relationType,
        string targetHypothesisId,
        string? note = null,
        string updatedBy = "mcp-agent",
        string? repo = null)
        => service.RelateHypothesisAsync(
            guard.ResolveWritable(repo),
            new RelateHypothesisRequest(hypothesisId, relationType, targetHypothesisId, note, updatedBy),
            cancellationToken);

    [McpServerTool(Name = "ctx_hypothesis_merge", ReadOnly = false, Destructive = false), Description("Merge one CTX hypothesis into another. Requires ctx-mcp --mode write.")]
    public static Task<CommandResult> MergeHypothesisAsync(
        ICtxApplicationService service,
        RepositoryGuard guard,
        CancellationToken cancellationToken,
        string sourceHypothesisId,
        string targetHypothesisId,
        string updatedBy = "mcp-agent",
        string? repo = null)
        => service.MergeHypothesisAsync(
            guard.ResolveWritable(repo),
            new MergeHypothesisRequest(sourceHypothesisId, targetHypothesisId, updatedBy),
            cancellationToken);

    [McpServerTool(Name = "ctx_hypothesis_supersede", ReadOnly = false, Destructive = false), Description("Supersede one CTX hypothesis with another. Requires ctx-mcp --mode write.")]
    public static Task<CommandResult> SupersedeHypothesisAsync(
        ICtxApplicationService service,
        RepositoryGuard guard,
        CancellationToken cancellationToken,
        string oldHypothesisId,
        string newHypothesisId,
        string updatedBy = "mcp-agent",
        string? repo = null)
        => service.SupersedeHypothesisAsync(
            guard.ResolveWritable(repo),
            new SupersedeHypothesisRequest(oldHypothesisId, newHypothesisId, updatedBy),
            cancellationToken);

    [McpServerTool(Name = "ctx_decision_add", ReadOnly = false, Destructive = false), Description("Add a CTX decision. Requires ctx-mcp --mode write.")]
    public static Task<CommandResult> AddDecisionAsync(
        ICtxApplicationService service,
        RepositoryGuard guard,
        CancellationToken cancellationToken,
        string title,
        string rationale = "",
        string state = "Proposed",
        string[]? hypothesisIds = null,
        string[]? evidenceIds = null,
        string createdBy = "mcp-agent",
        string? repo = null)
        => service.AddDecisionAsync(
            guard.ResolveWritable(repo),
            new AddDecisionRequest(title, rationale, state, Normalize(hypothesisIds), Normalize(evidenceIds), createdBy),
            cancellationToken);

    [McpServerTool(Name = "ctx_decision_update", ReadOnly = false, Destructive = false), Description("Update a CTX decision. Requires ctx-mcp --mode write.")]
    public static Task<CommandResult> UpdateDecisionAsync(
        ICtxApplicationService service,
        RepositoryGuard guard,
        CancellationToken cancellationToken,
        string decisionId,
        string? title = null,
        string? rationale = null,
        string? state = null,
        string[]? hypothesisIds = null,
        string[]? evidenceIds = null,
        string updatedBy = "mcp-agent",
        string? repo = null)
        => service.UpdateDecisionAsync(
            guard.ResolveWritable(repo),
            new UpdateDecisionRequest(decisionId, title, rationale, state, hypothesisIds, evidenceIds, updatedBy),
            cancellationToken);

    [McpServerTool(Name = "ctx_conclusion_add", ReadOnly = false, Destructive = false), Description("Add a CTX conclusion. Requires ctx-mcp --mode write.")]
    public static Task<CommandResult> AddConclusionAsync(
        ICtxApplicationService service,
        RepositoryGuard guard,
        CancellationToken cancellationToken,
        string summary,
        string state = "Draft",
        string[]? decisionIds = null,
        string[]? evidenceIds = null,
        string[]? goalIds = null,
        string[]? taskIds = null,
        string createdBy = "mcp-agent",
        string? repo = null)
        => service.AddConclusionAsync(
            guard.ResolveWritable(repo),
            new AddConclusionRequest(summary, state, Normalize(decisionIds), Normalize(evidenceIds), Normalize(goalIds), Normalize(taskIds), createdBy),
            cancellationToken);

    [McpServerTool(Name = "ctx_conclusion_update", ReadOnly = false, Destructive = false), Description("Update a CTX conclusion. Requires ctx-mcp --mode write.")]
    public static Task<CommandResult> UpdateConclusionAsync(
        ICtxApplicationService service,
        RepositoryGuard guard,
        CancellationToken cancellationToken,
        string conclusionId,
        string? summary = null,
        string? state = null,
        string updatedBy = "mcp-agent",
        string? repo = null)
        => service.UpdateConclusionAsync(
            guard.ResolveWritable(repo),
            new UpdateConclusionRequest(conclusionId, summary, state, updatedBy),
            cancellationToken);

    [McpServerTool(Name = "ctx_commit", ReadOnly = false, Destructive = false), Description("Create a durable CTX cognitive commit. Requires ctx-mcp --mode write.")]
    public static Task<CommandResult> CommitAsync(
        ICtxApplicationService service,
        RepositoryGuard guard,
        CancellationToken cancellationToken,
        string message,
        string createdBy = "mcp-agent",
        string? repo = null)
        => service.CommitAsync(
            guard.ResolveWritable(repo),
            new CommitRequest(message, createdBy),
            cancellationToken);

    [McpServerTool(Name = "ctx_runbook_add", ReadOnly = false, Destructive = false), Description("Add a CTX operational runbook. Requires ctx-mcp --mode write.")]
    public static Task<CommandResult> AddRunbookAsync(
        ICtxApplicationService service,
        RepositoryGuard guard,
        CancellationToken cancellationToken,
        string title,
        string kind,
        string whenToUse,
        string[]? triggers = null,
        string[]? steps = null,
        string[]? verify = null,
        string[]? references = null,
        string[]? goalIds = null,
        string[]? taskIds = null,
        string[]? preconditions = null,
        string[]? failureSignals = null,
        string[]? escalationBoundary = null,
        string createdBy = "mcp-agent",
        string? repo = null)
        => service.AddOperationalRunbookAsync(
            guard.ResolveWritable(repo),
            new AddOperationalRunbookRequest(
                title,
                kind,
                Normalize(triggers),
                whenToUse,
                Normalize(steps),
                Normalize(verify),
                Normalize(references),
                Normalize(goalIds),
                Normalize(taskIds),
                createdBy,
                Normalize(preconditions),
                Normalize(failureSignals),
                Normalize(escalationBoundary)),
            cancellationToken);

    [McpServerTool(Name = "ctx_trigger_add", ReadOnly = false, Destructive = false), Description("Add a CTX cognitive trigger. Requires ctx-mcp --mode write.")]
    public static Task<CommandResult> AddTriggerAsync(
        ICtxApplicationService service,
        RepositoryGuard guard,
        CancellationToken cancellationToken,
        string kind,
        string summary,
        string? text = null,
        string[]? goalIds = null,
        string[]? taskIds = null,
        string[]? runbookIds = null,
        string createdBy = "mcp-agent",
        string? repo = null)
        => service.AddCognitiveTriggerAsync(
            guard.ResolveWritable(repo),
            new AddCognitiveTriggerRequest(kind, summary, text, Normalize(goalIds), Normalize(taskIds), Normalize(runbookIds), createdBy),
            cancellationToken);

    [McpServerTool(Name = "ctx_bootstrap_map", ReadOnly = true, Destructive = false), Description("Build a provisional bootstrap map from existing source material.")]
    public static Task<CommandResult> BootstrapMapAsync(
        ICtxApplicationService service,
        RepositoryGuard guard,
        CancellationToken cancellationToken,
        [Description("Source file or directory path to map.")] string from,
        string mode = "auto",
        int maxFiles = 8,
        string requestedBy = "mcp-agent",
        string? repo = null)
        => service.BootstrapMapAsync(
            guard.Resolve(repo),
            new BootstrapMapRequest(from, mode, maxFiles, requestedBy),
            cancellationToken);

    [McpServerTool(Name = "ctx_bootstrap_apply", ReadOnly = false, Destructive = false), Description("Apply the strongest provisional bootstrap line into CTX. Requires ctx-mcp --mode write.")]
    public static Task<CommandResult> BootstrapApplyAsync(
        ICtxApplicationService service,
        RepositoryGuard guard,
        CancellationToken cancellationToken,
        [Description("Source file or directory path to apply from.")] string from,
        string mode = "auto",
        int maxFiles = 8,
        string? parentGoalId = null,
        string requestedBy = "mcp-agent",
        string? repo = null)
        => service.BootstrapApplyAsync(
            guard.ResolveWritable(repo),
            new BootstrapApplyRequest(from, mode, maxFiles, parentGoalId, requestedBy),
            cancellationToken);

    private static IReadOnlyList<string> Normalize(string[]? values)
        => values is null ? Array.Empty<string>() : values.Where(value => !string.IsNullOrWhiteSpace(value)).ToArray();
}
