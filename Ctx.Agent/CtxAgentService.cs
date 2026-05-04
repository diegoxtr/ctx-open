namespace Ctx.Agent;

using System.Collections.Concurrent;
using Ctx.Application;

public sealed class CtxAgentService : ICtxAgentService
{
    private readonly ICtxApplicationService _applicationService;
    private readonly Func<DateTimeOffset> _utcNow;
    private readonly ConcurrentDictionary<string, AgentSessionStateRecord> _sessions = new(StringComparer.Ordinal);

    public CtxAgentService(ICtxApplicationService applicationService, Func<DateTimeOffset>? utcNow = null)
    {
        _applicationService = applicationService;
        _utcNow = utcNow ?? (() => DateTimeOffset.UtcNow);
    }

    public async Task<AgentSessionSummary> StartSessionAsync(StartAgentSessionRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.RepositoryPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Purpose);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.CreatedBy);

        var planning = await BuildPlanAsync(
            request.RepositoryPath,
            request.Purpose,
            request.GoalId,
            request.TaskId,
            cancellationToken);
        var now = _utcNow();
        var state = new AgentSessionStateRecord(
            Guid.NewGuid().ToString("N"),
            request.RepositoryPath,
            request.Mode,
            AgentSessionState.Active,
            request.CreatedBy,
            now,
            now,
            request.Purpose,
            request.GoalId,
            request.TaskId,
            planning.Context.Id.Value,
            Array.Empty<string>());

        _sessions[state.SessionId] = state;

        return ToSummary(state, planning);
    }

    public async Task<AgentTurnPlan> PlanTurnAsync(PlanAgentTurnRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Prompt);
        var state = GetSession(request.SessionId, request.RepositoryPath);
        EnsureActive(state);

        var purpose = string.IsNullOrWhiteSpace(request.Purpose)
            ? $"Agent turn: {request.Prompt}"
            : request.Purpose;
        var goalId = request.GoalId ?? state.ActiveGoalId;
        var taskId = request.TaskId ?? state.ActiveTaskId;
        var planning = await BuildPlanAsync(request.RepositoryPath, purpose, goalId, taskId, cancellationToken);
        state = state with
        {
            UpdatedAtUtc = _utcNow(),
            ActiveGoalId = goalId,
            ActiveTaskId = taskId,
            LastPlanPacketId = planning.Context.Id.Value
        };
        _sessions[state.SessionId] = state;

        return new AgentTurnPlan(
            ToSummary(state),
            request.Prompt,
            planning,
            BuildTurnGuidance(state, mutated: false));
    }

    public async Task<AgentTurnResult> ReceivePromptAsync(ReceiveAgentPromptRequest request, CancellationToken cancellationToken)
    {
        if (request.PersistTrigger)
        {
            var state = GetSession(request.SessionId, request.RepositoryPath);
            if (state.Mode == AgentSessionMode.ReadOnly)
            {
                throw new InvalidOperationException("Read-only agent sessions cannot persist cognitive triggers.");
            }

            throw new NotSupportedException("Persisting prompt triggers is reserved for the controlled cognitive-write phase.");
        }

        var plan = await PlanTurnAsync(
            new PlanAgentTurnRequest(
                request.RepositoryPath,
                request.SessionId,
                request.Prompt,
                request.Purpose,
                request.GoalId,
                request.TaskId),
            cancellationToken);

        return new AgentTurnResult(
            plan.Session,
            request.Prompt,
            plan,
            Mutated: false,
            plan.Session.LinkedTriggerIds,
            BuildTurnGuidance(GetSession(request.SessionId, request.RepositoryPath), mutated: false));
    }

    public async Task<AgentCloseoutSummary> CloseWorkBlockAsync(CloseAgentWorkBlockRequest request, CancellationToken cancellationToken)
    {
        var state = GetSession(request.SessionId, request.RepositoryPath);
        EnsureActive(state);

        var taskId = request.TaskId ?? state.ActiveTaskId;
        var check = await _applicationService.CheckAsync(request.RepositoryPath, taskId, cancellationToken);
        var closeout = await _applicationService.CloseoutAsync(request.RepositoryPath, cancellationToken);
        state = state with
        {
            State = AgentSessionState.Closing,
            UpdatedAtUtc = _utcNow(),
            ActiveTaskId = taskId
        };
        _sessions[state.SessionId] = state;

        return new AgentCloseoutSummary(
            ToSummary(state),
            check,
            closeout,
            new[]
            {
                "Review check and closeout output before creating a CTX commit.",
                "Closeout is advisory in read-only agent sessions; it does not write artifacts."
            });
    }

    public async Task<AgentHandoffPacket> PrepareHandoffAsync(PrepareAgentHandoffRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Purpose);
        var state = GetSession(request.SessionId, request.RepositoryPath);
        EnsureActiveOrClosing(state);

        var context = await _applicationService.ContextAsync(
            request.RepositoryPath,
            request.Purpose,
            state.ActiveGoalId,
            state.ActiveTaskId,
            cancellationToken);
        state = state with
        {
            State = AgentSessionState.Closed,
            UpdatedAtUtc = _utcNow()
        };
        _sessions[state.SessionId] = state;

        return new AgentHandoffPacket(
            ToSummary(state),
            context,
            new[]
            {
                "Use this handoff packet as the next session anchor.",
                "Structured CTX artifacts remain canonical; do not depend on raw chat transcript."
            });
    }

    private async Task<PlanningSummary> BuildPlanAsync(string repositoryPath, string purpose, string? goalId, string? taskId, CancellationToken cancellationToken)
    {
        var result = await _applicationService.PlanAsync(repositoryPath, purpose, goalId, taskId, cancellationToken);
        if (!result.Success)
        {
            throw new InvalidOperationException(result.Message);
        }

        return result.Data as PlanningSummary
            ?? throw new InvalidOperationException("Application service returned an unexpected planning payload.");
    }

    private AgentSessionStateRecord GetSession(string sessionId, string repositoryPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);
        if (!_sessions.TryGetValue(sessionId, out var state))
        {
            throw new InvalidOperationException($"Agent session '{sessionId}' was not found.");
        }

        if (!string.Equals(Path.GetFullPath(state.RepositoryPath), Path.GetFullPath(repositoryPath), StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Agent session repository does not match the requested repository.");
        }

        return state;
    }

    private static void EnsureActive(AgentSessionStateRecord state)
    {
        if (state.State != AgentSessionState.Active)
        {
            throw new InvalidOperationException($"Agent session '{state.SessionId}' is {state.State}.");
        }
    }

    private static void EnsureActiveOrClosing(AgentSessionStateRecord state)
    {
        if (state.State is not AgentSessionState.Active and not AgentSessionState.Closing)
        {
            throw new InvalidOperationException($"Agent session '{state.SessionId}' is {state.State}.");
        }
    }

    private static IReadOnlyList<string> BuildTurnGuidance(AgentSessionStateRecord state, bool mutated)
    {
        var guidance = new List<string>
        {
            "Use the returned planning packet as the authoritative next-turn context.",
            "Keep raw chat secondary to structured CTX artifacts."
        };

        if (state.Mode == AgentSessionMode.ReadOnly)
        {
            guidance.Add("This session is read-only; prompt turns must not mutate CTX state.");
        }

        if (!mutated)
        {
            guidance.Add("No cognitive artifacts were written by this turn.");
        }

        return guidance;
    }

    private static AgentSessionSummary ToSummary(AgentSessionStateRecord state, PlanningSummary? initialPlan = null)
    {
        return new AgentSessionSummary(
            state.SessionId,
            state.RepositoryPath,
            state.Mode,
            state.State,
            state.CreatedBy,
            state.CreatedAtUtc,
            state.UpdatedAtUtc,
            state.Purpose,
            state.ActiveGoalId,
            state.ActiveTaskId,
            state.LastPlanPacketId,
            state.LinkedTriggerIds,
            initialPlan);
    }

    private sealed record AgentSessionStateRecord(
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
        IReadOnlyList<string> LinkedTriggerIds);
}
