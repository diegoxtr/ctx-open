namespace Ctx.Agent;

using Ctx.Application;

public enum AgentSessionMode
{
    ReadOnly,
    CognitiveWrite,
    Execution
}

public enum AgentSessionState
{
    Active,
    WaitingForPermission,
    Closing,
    Closed
}

public record StartAgentSessionRequest(
    string RepositoryPath,
    string Purpose,
    string CreatedBy,
    AgentSessionMode Mode = AgentSessionMode.ReadOnly,
    string? GoalId = null,
    string? TaskId = null);

public record PlanAgentTurnRequest(
    string RepositoryPath,
    string SessionId,
    string Prompt,
    string? Purpose = null,
    string? GoalId = null,
    string? TaskId = null);

public record ReceiveAgentPromptRequest(
    string RepositoryPath,
    string SessionId,
    string Prompt,
    string? Purpose = null,
    string? GoalId = null,
    string? TaskId = null,
    bool PersistTrigger = false);

public record CloseAgentWorkBlockRequest(
    string RepositoryPath,
    string SessionId,
    string? TaskId = null);

public record PrepareAgentHandoffRequest(
    string RepositoryPath,
    string SessionId,
    string Purpose);

public record AgentSessionSummary(
    string SessionId,
    string RepositoryPath,
    AgentSessionMode Mode,
    AgentSessionState State,
    string CreatedBy,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    string Purpose,
    string? ActiveGoalId,
    string? ActiveTaskId,
    string? LastPlanPacketId,
    IReadOnlyList<string> LinkedTriggerIds,
    PlanningSummary? InitialPlan);

public record AgentTurnPlan(
    AgentSessionSummary Session,
    string Prompt,
    PlanningSummary Planning,
    IReadOnlyList<string> Guidance);

public record AgentTurnResult(
    AgentSessionSummary Session,
    string Prompt,
    AgentTurnPlan Plan,
    bool Mutated,
    IReadOnlyList<string> LinkedTriggerIds,
    IReadOnlyList<string> Guidance);

public record AgentCloseoutSummary(
    AgentSessionSummary Session,
    CommandResult Check,
    CommandResult Closeout,
    IReadOnlyList<string> Guidance);

public record AgentHandoffPacket(
    AgentSessionSummary Session,
    CommandResult Context,
    IReadOnlyList<string> Guidance);

public interface ICtxAgentService
{
    Task<AgentSessionSummary> StartSessionAsync(StartAgentSessionRequest request, CancellationToken cancellationToken);
    Task<AgentTurnPlan> PlanTurnAsync(PlanAgentTurnRequest request, CancellationToken cancellationToken);
    Task<AgentTurnResult> ReceivePromptAsync(ReceiveAgentPromptRequest request, CancellationToken cancellationToken);
    Task<AgentCloseoutSummary> CloseWorkBlockAsync(CloseAgentWorkBlockRequest request, CancellationToken cancellationToken);
    Task<AgentHandoffPacket> PrepareHandoffAsync(PrepareAgentHandoffRequest request, CancellationToken cancellationToken);
}
