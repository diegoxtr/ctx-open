namespace Ctx.Domain;

public enum LifecycleState
{
    Draft = 0,
    Active = 1,
    Validated = 2,
    Completed = 3,
    Superseded = 4,
    Archived = 5
}

public enum TaskExecutionState
{
    Draft = 0,
    Ready = 1,
    InProgress = 2,
    Blocked = 3,
    Done = 4
}

public enum DecisionState
{
    Proposed = 0,
    Accepted = 1,
    Rejected = 2,
    Superseded = 3
}

public enum HypothesisState
{
    Proposed = 0,
    UnderEvaluation = 1,
    Supported = 2,
    Refuted = 3,
    Archived = 4
}

public enum HypothesisBranchState
{
    Active = 0,
    Weakening = 1,
    Merged = 2,
    Deprecated = 3,
    Promoted = 4
}

public enum HypothesisBranchRole
{
    Competing = 0,
    Integrative = 1,
    Dominant = 2
}

public enum HypothesisRelationType
{
    CompetesWith = 0,
    MergedInto = 1,
    Supersedes = 2,
    DerivedFrom = 3,
    BorrowsEvidenceFrom = 4
}

public enum EvidenceKind
{
    Observation = 0,
    Benchmark = 1,
    Document = 2,
    Experiment = 3,
    ProviderOutput = 4
}

public enum RunState
{
    Planned = 0,
    Running = 1,
    Completed = 2,
    Failed = 3
}

public enum ConclusionState
{
    Draft = 0,
    Accepted = 1,
    Superseded = 2
}

public enum OperationalRunbookKind
{
    Procedure = 0,
    Troubleshooting = 1,
    Policy = 2,
    Guardrail = 3
}

public enum CognitiveTriggerKind
{
    UserPrompt = 0,
    AgentPrompt = 1,
    Continuation = 2,
    RunbookTrigger = 3,
    IssueTrigger = 4
}
