namespace Ctx.Domain;

using System.Text.Json.Serialization;

public record Project(
    ProjectId Id,
    string Name,
    string Description,
    string DefaultBranch,
    LifecycleState State,
    Traceability Trace) : CognitiveEntity<ProjectId>(Id, Trace);

public record Goal(
    GoalId Id,
    GoalId? ParentGoalId,
    string Title,
    string Description,
    int Priority,
    LifecycleState State,
    Traceability Trace,
    IReadOnlyList<TaskId> TaskIds) : CognitiveEntity<GoalId>(Id, Trace);

public record Task(
    TaskId Id,
    GoalId? GoalId,
    string Title,
    string Description,
    TaskExecutionState State,
    Traceability Trace,
    IReadOnlyList<TaskId> DependsOnTaskIds,
    IReadOnlyList<HypothesisId> HypothesisIds,
    TaskId? ParentTaskId = null) : CognitiveEntity<TaskId>(Id, Trace);

public record HypothesisRelation(
    HypothesisRelationType RelationType,
    HypothesisId TargetHypothesisId,
    string? Note = null);

public record Hypothesis(
    HypothesisId Id,
    string Statement,
    string Rationale,
    decimal Confidence,
    decimal Impact,
    decimal EvidenceStrength,
    decimal CostToValidate,
    HypothesisState State,
    Traceability Trace,
    IReadOnlyList<TaskId> TaskIds,
    IReadOnlyList<EvidenceId> EvidenceIds,
    HypothesisBranchState? BranchState = null,
    HypothesisBranchRole? BranchRole = null,
    string? LineageGroupId = null,
    IReadOnlyList<HypothesisId>? ParentHypothesisIds = null,
    HypothesisId? MergedIntoHypothesisId = null,
    IReadOnlyList<HypothesisId>? SupersedesHypothesisIds = null,
    IReadOnlyList<HypothesisRelation>? Relations = null) : CognitiveEntity<HypothesisId>(Id, Trace)
{
    public decimal Probability => Confidence;
    public decimal Score => HypothesisScoring.Calculate(Probability, Impact, EvidenceStrength, CostToValidate);
}

public record Decision(
    DecisionId Id,
    string Title,
    string Rationale,
    DecisionState State,
    Traceability Trace,
    IReadOnlyList<HypothesisId> HypothesisIds,
    IReadOnlyList<EvidenceId> EvidenceIds) : CognitiveEntity<DecisionId>(Id, Trace);

public record Evidence(
    EvidenceId Id,
    string Title,
    string Summary,
    string Source,
    EvidenceKind Kind,
    decimal Confidence,
    LifecycleState State,
    Traceability Trace,
    IReadOnlyList<EntityReference> Supports) : CognitiveEntity<EvidenceId>(Id, Trace);

public record Conclusion(
    ConclusionId Id,
    string Summary,
    ConclusionState State,
    Traceability Trace,
    IReadOnlyList<DecisionId> DecisionIds,
    IReadOnlyList<EvidenceId> EvidenceIds,
    IReadOnlyList<GoalId> GoalIds,
    IReadOnlyList<TaskId> TaskIds) : CognitiveEntity<ConclusionId>(Id, Trace);

[method: JsonConstructor]
public record OperationalRunbook(
    OperationalRunbookId Id,
    string Title,
    OperationalRunbookKind Kind,
    IReadOnlyList<string> Triggers,
    string WhenToUse,
    IReadOnlyList<string> Do,
    IReadOnlyList<string> Verify,
    IReadOnlyList<string> References,
    IReadOnlyList<string> Preconditions,
    IReadOnlyList<string> FailureSignals,
    IReadOnlyList<string> EscalationBoundary,
    IReadOnlyList<GoalId> GoalIds,
    IReadOnlyList<TaskId> TaskIds,
    LifecycleState State,
    Traceability Trace) : CognitiveEntity<OperationalRunbookId>(Id, Trace)
{
    public OperationalRunbook(
        OperationalRunbookId id,
        string title,
        OperationalRunbookKind kind,
        IReadOnlyList<string> triggers,
        string whenToUse,
        IReadOnlyList<string> @do,
        IReadOnlyList<string> verify,
        IReadOnlyList<string> references,
        IReadOnlyList<GoalId> goalIds,
        IReadOnlyList<TaskId> taskIds,
        LifecycleState state,
        Traceability trace)
        : this(
            id,
            title,
            kind,
            triggers,
            whenToUse,
            @do,
            verify,
            references,
            Array.Empty<string>(),
            Array.Empty<string>(),
            Array.Empty<string>(),
            goalIds,
            taskIds,
            state,
            trace)
    {
    }
}

public record CognitiveTrigger(
    CognitiveTriggerId Id,
    CognitiveTriggerKind Kind,
    string Summary,
    string? Text,
    string Fingerprint,
    IReadOnlyList<GoalId> GoalIds,
    IReadOnlyList<TaskId> TaskIds,
    IReadOnlyList<OperationalRunbookId> OperationalRunbookIds,
    LifecycleState State,
    Traceability Trace) : CognitiveEntity<CognitiveTriggerId>(Id, Trace);

public record RunArtifact(
    string ArtifactType,
    string Title,
    string Content,
    IReadOnlyList<EntityReference> References);

public record Run(
    RunId Id,
    string Provider,
    string Model,
    RunState State,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    ContextPacketId PacketId,
    TokenUsage Usage,
    string PromptFingerprint,
    string Summary,
    IReadOnlyList<RunArtifact> Artifacts,
    Traceability Trace) : CognitiveEntity<RunId>(Id, Trace);

public record ContextPacket(
    ContextPacketId Id,
    ProjectId ProjectId,
    DateTimeOffset CreatedAtUtc,
    string Purpose,
    string Fingerprint,
    int EstimatedTokens,
    IReadOnlyList<GoalId> GoalIds,
    IReadOnlyList<TaskId> TaskIds,
    IReadOnlyList<HypothesisId> HypothesisIds,
    IReadOnlyList<DecisionId> DecisionIds,
    IReadOnlyList<EvidenceId> EvidenceIds,
    IReadOnlyList<ConclusionId> ConclusionIds,
    IReadOnlyList<OperationalRunbookId>? RunbookIds,
    IReadOnlyList<CognitiveTriggerId>? TriggerIds,
    IReadOnlyList<ContentSection> Sections);

public record ContextGraph(
    Project Project,
    IReadOnlyList<Goal> Goals,
    IReadOnlyList<Task> Tasks,
    IReadOnlyList<Hypothesis> Hypotheses,
    IReadOnlyList<Decision> Decisions,
    IReadOnlyList<Evidence> Evidence,
    IReadOnlyList<Conclusion> Conclusions,
    IReadOnlyList<Run> Runs);

public record WorkingContext(
    WorkingContextId Id,
    string RepositoryVersion,
    string CurrentBranch,
    ContextCommitId? HeadCommitId,
    bool Dirty,
    Project Project,
    IReadOnlyList<Goal> Goals,
    IReadOnlyList<Task> Tasks,
    IReadOnlyList<Hypothesis> Hypotheses,
    IReadOnlyList<Decision> Decisions,
    IReadOnlyList<Evidence> Evidence,
    IReadOnlyList<Conclusion> Conclusions,
    IReadOnlyList<Run> Runs,
    Traceability Trace) : CognitiveEntity<WorkingContextId>(Id, Trace)
{
    public ContextGraph ToGraph() => new(
        Project,
        Goals,
        Tasks,
        Hypotheses,
        Decisions,
        Evidence,
        Conclusions,
        Runs);
}

[method: JsonConstructor]
public record RepositorySnapshot(
    WorkingContext WorkingContext,
    IReadOnlyList<OperationalRunbook> Runbooks,
    IReadOnlyList<CognitiveTrigger> Triggers)
{
    public RepositorySnapshot(WorkingContext workingContext, IReadOnlyList<OperationalRunbook> runbooks)
        : this(workingContext, runbooks, Array.Empty<CognitiveTrigger>())
    {
    }
}

public record ContextDiffChange(
    string ChangeType,
    string EntityType,
    string EntityId,
    string Summary);

public record CognitiveConflict(
    string EntityType,
    string EntityId,
    string ConflictType,
    string CurrentSummary,
    string IncomingSummary);

[method: JsonConstructor]
public record ContextDiff(
    ContextCommitId? FromCommitId,
    ContextCommitId? ToCommitId,
    IReadOnlyList<ContextDiffChange> Decisions,
    IReadOnlyList<ContextDiffChange> Hypotheses,
    IReadOnlyList<ContextDiffChange> Evidence,
    IReadOnlyList<ContextDiffChange> Tasks,
    IReadOnlyList<ContextDiffChange> Conclusions,
    IReadOnlyList<ContextDiffChange> Runbooks,
    IReadOnlyList<ContextDiffChange> Triggers,
    IReadOnlyList<CognitiveConflict> Conflicts,
    string Summary)
{
    public ContextDiff(
        ContextCommitId? fromCommitId,
        ContextCommitId? toCommitId,
        IReadOnlyList<ContextDiffChange> decisions,
        IReadOnlyList<ContextDiffChange> hypotheses,
        IReadOnlyList<ContextDiffChange> evidence,
        IReadOnlyList<ContextDiffChange> tasks,
        IReadOnlyList<ContextDiffChange> conclusions,
        IReadOnlyList<ContextDiffChange> runbooks,
        IReadOnlyList<CognitiveConflict> conflicts,
        string summary)
        : this(fromCommitId, toCommitId, decisions, hypotheses, evidence, tasks, conclusions, runbooks, Array.Empty<ContextDiffChange>(), conflicts, summary)
    {
    }
}

public record MergeResult(
    RepositorySnapshot MergedSnapshot,
    IReadOnlyList<CognitiveConflict> Conflicts,
    bool AutoMerged,
    string Summary);

public record ContextCommit(
    ContextCommitId Id,
    string Branch,
    string Message,
    IReadOnlyList<ContextCommitId> ParentIds,
    DateTimeOffset CreatedAtUtc,
    string SnapshotHash,
    ContextDiff Diff,
    RepositorySnapshot Snapshot,
    Traceability Trace) : CognitiveEntity<ContextCommitId>(Id, Trace);

public record ProviderConfiguration(
    string Name,
    string DefaultModel,
    string Endpoint,
    bool Enabled);

public record RepositoryConfig(
    string DefaultProvider,
    IReadOnlyList<ProviderConfiguration> Providers,
    int PacketTokenLimit,
    bool TrackMetrics);

public record RepositoryVersion(string CurrentVersion, DateTimeOffset InitializedAtUtc);

public record HeadReference(string Branch, ContextCommitId? CommitId);

public record BranchReference(string Name, ContextCommitId? CommitId, DateTimeOffset UpdatedAtUtc);

public sealed record CommandUsageMetrics(
    string Command,
    int TotalInvocations,
    int SuccessfulInvocations,
    int FailedInvocations,
    TimeSpan TotalExecutionTime,
    DateTimeOffset? LastInvokedAtUtc,
    string LastOutcome);

public sealed record MetricsSnapshot
{
    public MetricsSnapshot(
        int totalRuns,
        int totalTokens,
        decimal totalAcuCost,
        int repeatedIterations,
        int avoidedRedundancyCount,
        TimeSpan totalExecutionTime,
        int totalCommandInvocations = 0,
        IReadOnlyList<CommandUsageMetrics>? commandUsage = null)
    {
        TotalRuns = totalRuns;
        TotalTokens = totalTokens;
        TotalAcuCost = totalAcuCost;
        RepeatedIterations = repeatedIterations;
        AvoidedRedundancyCount = avoidedRedundancyCount;
        TotalExecutionTime = totalExecutionTime;
        TotalCommandInvocations = totalCommandInvocations;
        CommandUsage = commandUsage ?? Array.Empty<CommandUsageMetrics>();
    }

    public int TotalRuns { get; init; }
    public int TotalTokens { get; init; }
    public decimal TotalAcuCost { get; init; }
    public int RepeatedIterations { get; init; }
    public int AvoidedRedundancyCount { get; init; }
    public TimeSpan TotalExecutionTime { get; init; }
    public int TotalCommandInvocations { get; init; }
    public IReadOnlyList<CommandUsageMetrics> CommandUsage { get; init; }

    public MetricsSnapshot RecordCommandUsage(string command, bool success, TimeSpan duration, DateTimeOffset invokedAtUtc)
    {
        var normalizedCommand = command.Trim();
        var existing = CommandUsage.SingleOrDefault(item => item.Command.Equals(normalizedCommand, StringComparison.OrdinalIgnoreCase));
        var updatedEntry = existing is null
            ? new CommandUsageMetrics(
                normalizedCommand,
                1,
                success ? 1 : 0,
                success ? 0 : 1,
                duration,
                invokedAtUtc,
                success ? "success" : "failure")
            : existing with
            {
                TotalInvocations = existing.TotalInvocations + 1,
                SuccessfulInvocations = existing.SuccessfulInvocations + (success ? 1 : 0),
                FailedInvocations = existing.FailedInvocations + (success ? 0 : 1),
                TotalExecutionTime = existing.TotalExecutionTime + duration,
                LastInvokedAtUtc = invokedAtUtc,
                LastOutcome = success ? "success" : "failure"
            };

        var commandUsage = CommandUsage
            .Where(item => existing is null || !item.Command.Equals(existing.Command, StringComparison.OrdinalIgnoreCase))
            .Append(updatedEntry)
            .OrderBy(item => item.Command, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return this with
        {
            TotalCommandInvocations = TotalCommandInvocations + 1,
            CommandUsage = commandUsage
        };
    }
}

public record DoctorCheck(
    string Name,
    string Status,
    string Detail);

public record DoctorReport(
    string ProductVersion,
    string WorkingDirectory,
    bool RepositoryDetected,
    IReadOnlyList<DoctorCheck> Checks);

public record AuditIssue(
    string Severity,
    string IssueType,
    string EntityType,
    string EntityId,
    string Detail,
    string SuggestedAction);

public record AuditReport(
    string Branch,
    string? HeadCommitId,
    decimal ConsistencyScore,
    IReadOnlyList<AuditIssue> Issues,
    IReadOnlyDictionary<string, int> Summary);

public record RepositoryExport(
    string ProductVersion,
    RepositoryVersion RepositoryVersion,
    RepositoryConfig Config,
    HeadReference Head,
    RepositorySnapshot Snapshot,
    MetricsSnapshot Metrics,
    IReadOnlyList<BranchReference> Branches,
    IReadOnlyList<ContextCommit> Commits);

public record CognitiveGraphNode(
    string Id,
    string Type,
    string Label,
    string State,
    IReadOnlyDictionary<string, string> Metadata);

public record CognitiveGraphEdge(
    string From,
    string To,
    string Relationship,
    IReadOnlyDictionary<string, string> Metadata);

public record CognitiveGraphExport(
    IReadOnlyList<CognitiveGraphNode> Nodes,
    IReadOnlyList<CognitiveGraphEdge> Edges,
    IReadOnlyDictionary<string, string> Metadata);

public record CognitiveGraphLineage(
    string FocusNodeId,
    string FocusType,
    CognitiveGraphExport Graph);

public record CognitiveThreadFocus(
    string EntityType,
    string EntityId,
    string Label,
    string State);

public record CognitiveThreadStep(
    int Order,
    string Relationship,
    string EntityType,
    string EntityId,
    string Summary,
    string State);

public record CognitiveThreadTimelineEvent(
    string CommitId,
    string Branch,
    DateTimeOffset CreatedAtUtc,
    string ChangeType,
    string EntityType,
    string EntityId,
    string Summary);

public record CognitiveThreadBranchContext(
    string Branch,
    string? HeadCommitId,
    IReadOnlyList<string> RelatedCommitIds);

public record CognitiveThreadGap(
    string GapType,
    string EntityType,
    string EntityId,
    string Detail);

public record ContextThread(
    CognitiveThreadFocus Focus,
    IReadOnlyList<CognitiveThreadStep> SemanticThread,
    IReadOnlyList<CognitiveThreadTimelineEvent> Timeline,
    CognitiveThreadBranchContext BranchContext,
    IReadOnlyList<string> OpenQuestions,
    IReadOnlyList<CognitiveThreadGap> Gaps);

public record NextWorkCandidate(
    string CandidateType,
    string EntityId,
    string Title,
    string State,
    decimal Score,
    IReadOnlyDictionary<string, string> Factors);

public static class DomainConstants
{
    public const string RepositoryFolderName = ".ctx";
    public const string CurrentRepositoryVersion = "1.0";
    public const string ProductVersion = "1.0.4";
}

public static class HypothesisScoring
{
    public static decimal Calculate(decimal probability, decimal impact, decimal evidenceStrength, decimal costToValidate)
    {
        var score = (Normalize(probability) * 0.35m)
            + (Normalize(impact) * 0.35m)
            + (Normalize(evidenceStrength) * 0.20m)
            - (Normalize(costToValidate) * 0.10m);

        return decimal.Round(score, 4, MidpointRounding.AwayFromZero);
    }

    private static decimal Normalize(decimal value)
        => decimal.Clamp(value, 0m, 1m);
}
