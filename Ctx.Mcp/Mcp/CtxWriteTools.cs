namespace Ctx.Mcp.Mcp;

using System.ComponentModel;
using Ctx.Application;
using ModelContextProtocol.Server;

[McpServerToolType]
public static class CtxWriteTools
{
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

    private static IReadOnlyList<string> Normalize(string[]? values)
        => values is null ? Array.Empty<string>() : values.Where(value => !string.IsNullOrWhiteSpace(value)).ToArray();
}
