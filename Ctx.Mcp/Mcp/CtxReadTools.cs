namespace Ctx.Mcp.Mcp;

using System.ComponentModel;
using Ctx.Application;
using ModelContextProtocol.Server;

[McpServerToolType]
public static class CtxReadTools
{
    [McpServerTool(Name = "ctx_status", ReadOnly = true, Destructive = false), Description("Read CTX status for the configured repository.")]
    public static Task<CommandResult> StatusAsync(
        ICtxApplicationService service,
        RepositoryGuard guard,
        CancellationToken cancellationToken,
        [Description("Optional repository path. Prefer configuring one MCP server entry per repository.")] string? repo = null)
        => service.StatusAsync(guard.Resolve(repo), cancellationToken);

    [McpServerTool(Name = "ctx_audit", ReadOnly = true, Destructive = false), Description("Run CTX consistency audit for the configured repository.")]
    public static Task<CommandResult> AuditAsync(
        ICtxApplicationService service,
        RepositoryGuard guard,
        CancellationToken cancellationToken,
        string? repo = null)
        => service.AuditAsync(guard.Resolve(repo), cancellationToken);

    [McpServerTool(Name = "ctx_next", ReadOnly = true, Destructive = false), Description("Ask CTX for the next recommended cognitive step.")]
    public static Task<CommandResult> NextAsync(
        ICtxApplicationService service,
        RepositoryGuard guard,
        CancellationToken cancellationToken,
        string? repo = null)
        => service.NextAsync(guard.Resolve(repo), cancellationToken);

    [McpServerTool(Name = "ctx_context", ReadOnly = true, Destructive = false), Description("Build a compact CTX context packet for a purpose, goal, or task.")]
    public static Task<CommandResult> ContextAsync(
        ICtxApplicationService service,
        RepositoryGuard guard,
        CancellationToken cancellationToken,
        [Description("Purpose for the context packet.")] string purpose = "mcp-context",
        string? goalId = null,
        string? taskId = null,
        string? repo = null)
        => service.ContextAsync(guard.Resolve(repo), purpose, goalId, taskId, cancellationToken);

    [McpServerTool(Name = "ctx_graph_summary", ReadOnly = true, Destructive = false), Description("Read the CTX graph summary.")]
    public static Task<CommandResult> GraphSummaryAsync(
        ICtxApplicationService service,
        RepositoryGuard guard,
        CancellationToken cancellationToken,
        string? repo = null)
        => service.GraphSummaryAsync(guard.Resolve(repo), cancellationToken);

    [McpServerTool(Name = "ctx_graph_show", ReadOnly = true, Destructive = false), Description("Show one CTX graph node by id.")]
    public static Task<CommandResult> GraphShowAsync(
        ICtxApplicationService service,
        RepositoryGuard guard,
        CancellationToken cancellationToken,
        [Description("Graph node id, for example Task:<id> or Hypothesis:<id>.")] string nodeId,
        string? repo = null)
        => service.GraphShowAsync(guard.Resolve(repo), nodeId, cancellationToken);

    [McpServerTool(Name = "ctx_thread_reconstruct", ReadOnly = true, Destructive = false), Description("Reconstruct a CTX cognitive thread.")]
    public static Task<CommandResult> ThreadReconstructAsync(
        ICtxApplicationService service,
        RepositoryGuard guard,
        CancellationToken cancellationToken,
        [Description("Task id to reconstruct.")] string taskId,
        [Description("Output format: json or markdown.")] string format = "json",
        string? repo = null)
        => service.ThreadReconstructAsync(guard.Resolve(repo), "task", taskId, format, cancellationToken);

    [McpServerTool(Name = "ctx_preflight", ReadOnly = true, Destructive = false), Description("Run CTX preflight guidance for an operation.")]
    public static Task<CommandResult> PreflightAsync(
        ICtxApplicationService service,
        RepositoryGuard guard,
        CancellationToken cancellationToken,
        [Description("Operation name, for example git-closeout or recover-index-lock.")] string operation,
        string? goalId = null,
        string? taskId = null,
        string? repo = null)
        => service.PreflightAsync(guard.Resolve(repo), operation, goalId, taskId, cancellationToken);

    [McpServerTool(Name = "ctx_check", ReadOnly = true, Destructive = false), Description("Check whether a CTX task thread has enough closure artifacts for a cognitive commit.")]
    public static Task<CommandResult> CheckAsync(
        ICtxApplicationService service,
        RepositoryGuard guard,
        CancellationToken cancellationToken,
        [Description("Optional task id. When omitted, CTX selects the active task.")] string? taskId = null,
        string? repo = null)
        => service.CheckAsync(guard.Resolve(repo), taskId, cancellationToken);

    [McpServerTool(Name = "ctx_closeout", ReadOnly = true, Destructive = false), Description("Review pending cognitive changes before a CTX or Git closeout.")]
    public static Task<CommandResult> CloseoutAsync(
        ICtxApplicationService service,
        RepositoryGuard guard,
        CancellationToken cancellationToken,
        string? repo = null)
        => service.CloseoutAsync(guard.Resolve(repo), cancellationToken);
}
