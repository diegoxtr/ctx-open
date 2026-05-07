namespace Ctx.Mcp.Mcp;

using System.ComponentModel;
using Ctx.Application;
using Ctx.Domain;
using ModelContextProtocol.Server;

[McpServerToolType]
public static class CtxReadTools
{
    [McpServerTool(Name = "ctx_version", ReadOnly = true, Destructive = false), Description("Read the CTX product and repository format version.")]
    public static CommandResult Version()
        => new(
            true,
            $"CTX {DomainConstants.ProductVersion}",
            new
            {
                product = "CTX",
                version = DomainConstants.ProductVersion,
                repositoryFormat = DomainConstants.CurrentRepositoryVersion
            });

    [McpServerTool(Name = "ctx_doctor", ReadOnly = true, Destructive = false), Description("Run CTX technical diagnostics for the configured repository.")]
    public static Task<CommandResult> DoctorAsync(
        ICtxApplicationService service,
        RepositoryGuard guard,
        CancellationToken cancellationToken,
        string? repo = null)
        => service.DoctorAsync(guard.Resolve(repo), cancellationToken);

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

    [McpServerTool(Name = "ctx_gaps", ReadOnly = true, Destructive = false), Description("Build a read-only CTX gaps summary for unresolved planning gaps, blocked work, and deferred candidates.")]
    public static Task<CommandResult> GapsAsync(
        ICtxApplicationService service,
        RepositoryGuard guard,
        CancellationToken cancellationToken,
        string? repo = null)
        => service.GapsAsync(guard.Resolve(repo), cancellationToken);

    [McpServerTool(Name = "ctx_roadmap", ReadOnly = true, Destructive = false), Description("Build a read-only CTX roadmap view for future planning lanes and parked work.")]
    public static Task<CommandResult> RoadmapAsync(
        ICtxApplicationService service,
        RepositoryGuard guard,
        CancellationToken cancellationToken,
        string? repo = null)
        => service.RoadmapAsync(guard.Resolve(repo), cancellationToken);

    [McpServerTool(Name = "ctx_plan", ReadOnly = true, Destructive = false), Description("Build a compact CTX planning packet with status, next recommendation, context, runbooks, and guidance.")]
    public static Task<CommandResult> PlanAsync(
        ICtxApplicationService service,
        RepositoryGuard guard,
        CancellationToken cancellationToken,
        [Description("Purpose for the planning packet.")] string purpose = "mcp-plan",
        string? goalId = null,
        string? taskId = null,
        string? repo = null)
        => service.PlanAsync(guard.Resolve(repo), purpose, goalId, taskId, cancellationToken);

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
        [Description("Operation name, for example git-closeout, recover-index-lock, or a custom runbook trigger such as docs-freeze.")] string operation,
        string? goalId = null,
        string? taskId = null,
        string? repo = null)
        => service.PreflightAsync(guard.Resolve(repo), operation, goalId, taskId, cancellationToken);

    [McpServerTool(Name = "ctx_operational_review", ReadOnly = true, Destructive = false), Description("Review repeated operational issues and recommend runbook updates when a recurrence threshold is reached.")]
    public static Task<CommandResult> OperationalReviewAsync(
        ICtxApplicationService service,
        RepositoryGuard guard,
        CancellationToken cancellationToken,
        [Description("Optional operation filter, for example git-closeout, publish-local, or a custom runbook trigger.")] string? operation = null,
        [Description("Minimum occurrences before an issue is promoted to runbook-review guidance.")] int threshold = 2,
        string? repo = null)
        => service.OperationalReviewAsync(guard.Resolve(repo), operation, threshold, cancellationToken);

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

    [McpServerTool(Name = "ctx_log", ReadOnly = true, Destructive = false), Description("Read CTX commit history for the current branch.")]
    public static Task<CommandResult> LogAsync(
        ICtxApplicationService service,
        RepositoryGuard guard,
        CancellationToken cancellationToken,
        string? repo = null)
        => service.LogAsync(guard.Resolve(repo), cancellationToken);

    [McpServerTool(Name = "ctx_diff", ReadOnly = true, Destructive = false), Description("Calculate a CTX cognitive diff.")]
    public static Task<CommandResult> DiffAsync(
        ICtxApplicationService service,
        RepositoryGuard guard,
        CancellationToken cancellationToken,
        string? fromCommitId = null,
        string? toCommitId = null,
        string? repo = null)
        => service.DiffAsync(guard.Resolve(repo), fromCommitId, toCommitId, cancellationToken);

    [McpServerTool(Name = "ctx_graph_export", ReadOnly = true, Destructive = false), Description("Export the CTX cognitive graph as json or mermaid.")]
    public static Task<CommandResult> GraphExportAsync(
        ICtxApplicationService service,
        RepositoryGuard guard,
        CancellationToken cancellationToken,
        string format = "json",
        string? commitId = null,
        string? mode = null,
        string? focusNodeId = null,
        int? depth = null,
        string? repo = null)
        => service.ExportGraphAsync(guard.Resolve(repo), format, commitId, mode, focusNodeId, depth, cancellationToken);

    [McpServerTool(Name = "ctx_graph_lineage", ReadOnly = true, Destructive = false), Description("Read a focused CTX lineage graph.")]
    public static Task<CommandResult> GraphLineageAsync(
        ICtxApplicationService service,
        RepositoryGuard guard,
        CancellationToken cancellationToken,
        [Description("Focus type: goal, task, hypothesis, decision, or conclusion.")] string focusType,
        [Description("Focus entity id.")] string focusId,
        string format = "json",
        string? outputPath = null,
        string? repo = null)
        => service.GraphLineageAsync(guard.Resolve(repo), focusType, focusId, format, outputPath, cancellationToken);

    [McpServerTool(Name = "ctx_artifact_list", ReadOnly = true, Destructive = false), Description("List CTX artifacts by type: goal, epic, task, hypothesis, evidence, decision, or conclusion.")]
    public static Task<CommandResult> ArtifactListAsync(
        ICtxApplicationService service,
        RepositoryGuard guard,
        CancellationToken cancellationToken,
        string artifactType,
        string? repo = null)
        => service.ListArtifactsAsync(guard.Resolve(repo), artifactType, cancellationToken);

    [McpServerTool(Name = "ctx_artifact_show", ReadOnly = true, Destructive = false), Description("Show one CTX artifact by type and id.")]
    public static Task<CommandResult> ArtifactShowAsync(
        ICtxApplicationService service,
        RepositoryGuard guard,
        CancellationToken cancellationToken,
        string artifactType,
        string artifactId,
        string? repo = null)
        => service.ShowArtifactAsync(guard.Resolve(repo), artifactType, artifactId, cancellationToken);

    [McpServerTool(Name = "ctx_goal_list", ReadOnly = true, Destructive = false), Description("List CTX goals.")]
    public static Task<CommandResult> GoalListAsync(ICtxApplicationService service, RepositoryGuard guard, CancellationToken cancellationToken, string? repo = null)
        => service.ListArtifactsAsync(guard.Resolve(repo), "goal", cancellationToken);

    [McpServerTool(Name = "ctx_goal_show", ReadOnly = true, Destructive = false), Description("Show one CTX goal.")]
    public static Task<CommandResult> GoalShowAsync(ICtxApplicationService service, RepositoryGuard guard, CancellationToken cancellationToken, string goalId, string? repo = null)
        => service.ShowArtifactAsync(guard.Resolve(repo), "goal", goalId, cancellationToken);

    [McpServerTool(Name = "ctx_epic_list", ReadOnly = true, Destructive = false), Description("List CTX epics.")]
    public static Task<CommandResult> EpicListAsync(ICtxApplicationService service, RepositoryGuard guard, CancellationToken cancellationToken, string? repo = null)
        => service.ListArtifactsAsync(guard.Resolve(repo), "epic", cancellationToken);

    [McpServerTool(Name = "ctx_epic_show", ReadOnly = true, Destructive = false), Description("Show one CTX epic.")]
    public static Task<CommandResult> EpicShowAsync(ICtxApplicationService service, RepositoryGuard guard, CancellationToken cancellationToken, string epicId, string? repo = null)
        => service.ShowArtifactAsync(guard.Resolve(repo), "epic", epicId, cancellationToken);

    [McpServerTool(Name = "ctx_task_list", ReadOnly = true, Destructive = false), Description("List CTX tasks.")]
    public static Task<CommandResult> TaskListAsync(ICtxApplicationService service, RepositoryGuard guard, CancellationToken cancellationToken, string? repo = null)
        => service.ListArtifactsAsync(guard.Resolve(repo), "task", cancellationToken);

    [McpServerTool(Name = "ctx_task_show", ReadOnly = true, Destructive = false), Description("Show one CTX task.")]
    public static Task<CommandResult> TaskShowAsync(ICtxApplicationService service, RepositoryGuard guard, CancellationToken cancellationToken, string taskId, string? repo = null)
        => service.ShowArtifactAsync(guard.Resolve(repo), "task", taskId, cancellationToken);

    [McpServerTool(Name = "ctx_hypothesis_list", ReadOnly = true, Destructive = false), Description("List CTX hypotheses.")]
    public static Task<CommandResult> HypothesisListAsync(ICtxApplicationService service, RepositoryGuard guard, CancellationToken cancellationToken, string? repo = null)
        => service.ListArtifactsAsync(guard.Resolve(repo), "hypothesis", cancellationToken);

    [McpServerTool(Name = "ctx_hypothesis_show", ReadOnly = true, Destructive = false), Description("Show one CTX hypothesis.")]
    public static Task<CommandResult> HypothesisShowAsync(ICtxApplicationService service, RepositoryGuard guard, CancellationToken cancellationToken, string hypothesisId, string? repo = null)
        => service.ShowArtifactAsync(guard.Resolve(repo), "hypothesis", hypothesisId, cancellationToken);

    [McpServerTool(Name = "ctx_hypothesis_rank", ReadOnly = true, Destructive = false), Description("Rank CTX hypotheses by score.")]
    public static Task<CommandResult> HypothesisRankAsync(ICtxApplicationService service, RepositoryGuard guard, CancellationToken cancellationToken, string? repo = null)
        => service.RankHypothesesAsync(guard.Resolve(repo), cancellationToken);

    [McpServerTool(Name = "ctx_evidence_list", ReadOnly = true, Destructive = false), Description("List CTX evidence.")]
    public static Task<CommandResult> EvidenceListAsync(ICtxApplicationService service, RepositoryGuard guard, CancellationToken cancellationToken, string? repo = null)
        => service.ListArtifactsAsync(guard.Resolve(repo), "evidence", cancellationToken);

    [McpServerTool(Name = "ctx_evidence_show", ReadOnly = true, Destructive = false), Description("Show one CTX evidence item.")]
    public static Task<CommandResult> EvidenceShowAsync(ICtxApplicationService service, RepositoryGuard guard, CancellationToken cancellationToken, string evidenceId, string? repo = null)
        => service.ShowArtifactAsync(guard.Resolve(repo), "evidence", evidenceId, cancellationToken);

    [McpServerTool(Name = "ctx_decision_list", ReadOnly = true, Destructive = false), Description("List CTX decisions.")]
    public static Task<CommandResult> DecisionListAsync(ICtxApplicationService service, RepositoryGuard guard, CancellationToken cancellationToken, string? repo = null)
        => service.ListArtifactsAsync(guard.Resolve(repo), "decision", cancellationToken);

    [McpServerTool(Name = "ctx_decision_show", ReadOnly = true, Destructive = false), Description("Show one CTX decision.")]
    public static Task<CommandResult> DecisionShowAsync(ICtxApplicationService service, RepositoryGuard guard, CancellationToken cancellationToken, string decisionId, string? repo = null)
        => service.ShowArtifactAsync(guard.Resolve(repo), "decision", decisionId, cancellationToken);

    [McpServerTool(Name = "ctx_conclusion_list", ReadOnly = true, Destructive = false), Description("List CTX conclusions.")]
    public static Task<CommandResult> ConclusionListAsync(ICtxApplicationService service, RepositoryGuard guard, CancellationToken cancellationToken, string? repo = null)
        => service.ListArtifactsAsync(guard.Resolve(repo), "conclusion", cancellationToken);

    [McpServerTool(Name = "ctx_conclusion_show", ReadOnly = true, Destructive = false), Description("Show one CTX conclusion.")]
    public static Task<CommandResult> ConclusionShowAsync(ICtxApplicationService service, RepositoryGuard guard, CancellationToken cancellationToken, string conclusionId, string? repo = null)
        => service.ShowArtifactAsync(guard.Resolve(repo), "conclusion", conclusionId, cancellationToken);

    [McpServerTool(Name = "ctx_runbook_list", ReadOnly = true, Destructive = false), Description("List CTX operational runbooks.")]
    public static Task<CommandResult> RunbookListAsync(ICtxApplicationService service, RepositoryGuard guard, CancellationToken cancellationToken, string? repo = null)
        => service.ListOperationalRunbooksAsync(guard.Resolve(repo), cancellationToken);

    [McpServerTool(Name = "ctx_runbook_show", ReadOnly = true, Destructive = false), Description("Show one CTX operational runbook.")]
    public static Task<CommandResult> RunbookShowAsync(ICtxApplicationService service, RepositoryGuard guard, CancellationToken cancellationToken, string runbookId, string? repo = null)
        => service.ShowOperationalRunbookAsync(guard.Resolve(repo), runbookId, cancellationToken);

    [McpServerTool(Name = "ctx_trigger_list", ReadOnly = true, Destructive = false), Description("List CTX cognitive triggers.")]
    public static Task<CommandResult> TriggerListAsync(ICtxApplicationService service, RepositoryGuard guard, CancellationToken cancellationToken, string? repo = null)
        => service.ListCognitiveTriggersAsync(guard.Resolve(repo), cancellationToken);

    [McpServerTool(Name = "ctx_trigger_show", ReadOnly = true, Destructive = false), Description("Show one CTX cognitive trigger.")]
    public static Task<CommandResult> TriggerShowAsync(ICtxApplicationService service, RepositoryGuard guard, CancellationToken cancellationToken, string triggerId, string? repo = null)
        => service.ShowCognitiveTriggerAsync(guard.Resolve(repo), triggerId, cancellationToken);
}
