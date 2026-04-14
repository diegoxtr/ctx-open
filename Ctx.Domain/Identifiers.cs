namespace Ctx.Domain;

public readonly record struct ProjectId(string Value)
{
    public static ProjectId New() => new(Guid.NewGuid().ToString("N"));
    public override string ToString() => Value;
}

public readonly record struct GoalId(string Value)
{
    public static GoalId New() => new(Guid.NewGuid().ToString("N"));
    public override string ToString() => Value;
}

public readonly record struct TaskId(string Value)
{
    public static TaskId New() => new(Guid.NewGuid().ToString("N"));
    public override string ToString() => Value;
}

public readonly record struct HypothesisId(string Value)
{
    public static HypothesisId New() => new(Guid.NewGuid().ToString("N"));
    public override string ToString() => Value;
}

public readonly record struct DecisionId(string Value)
{
    public static DecisionId New() => new(Guid.NewGuid().ToString("N"));
    public override string ToString() => Value;
}

public readonly record struct EvidenceId(string Value)
{
    public static EvidenceId New() => new(Guid.NewGuid().ToString("N"));
    public override string ToString() => Value;
}

public readonly record struct ConclusionId(string Value)
{
    public static ConclusionId New() => new(Guid.NewGuid().ToString("N"));
    public override string ToString() => Value;
}

public readonly record struct RunId(string Value)
{
    public static RunId New() => new(Guid.NewGuid().ToString("N"));
    public override string ToString() => Value;
}

public readonly record struct ContextCommitId(string Value)
{
    public static ContextCommitId New() => new(Guid.NewGuid().ToString("N"));
    public override string ToString() => Value;
}

public readonly record struct ContextPacketId(string Value)
{
    public static ContextPacketId New() => new(Guid.NewGuid().ToString("N"));
    public override string ToString() => Value;
}

public readonly record struct WorkingContextId(string Value)
{
    public static WorkingContextId New() => new(Guid.NewGuid().ToString("N"));
    public override string ToString() => Value;
}

public readonly record struct OperationalRunbookId(string Value)
{
    public static OperationalRunbookId New() => new(Guid.NewGuid().ToString("N"));
    public override string ToString() => Value;
}

public readonly record struct CognitiveTriggerId(string Value)
{
    public static CognitiveTriggerId New() => new(Guid.NewGuid().ToString("N"));
    public override string ToString() => Value;
}
