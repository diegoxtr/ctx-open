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
