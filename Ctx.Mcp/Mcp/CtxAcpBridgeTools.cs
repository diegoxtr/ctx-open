namespace Ctx.Mcp.Mcp;

using System.ComponentModel;
using Ctx.Agent;
using Ctx.Application;
using Ctx.Domain;
using ModelContextProtocol.Server;

[McpServerToolType]
public static class CtxAcpBridgeTools
{
    [McpServerTool(Name = "ctx_acp_session_smoke", ReadOnly = true, Destructive = false), Description("Run a read-only ACP-style session smoke flow through the internal CTX Agent layer.")]
    public static async Task<CommandResult> AcpSessionSmokeAsync(
        ICtxAgentService agentService,
        RepositoryGuard guard,
        CancellationToken cancellationToken,
        [Description("Prompt text to send through the ACP-style session turn.")] string prompt = "What is the next safe planning step from CTX?",
        [Description("Purpose label for the created ACP-style session.")] string purpose = "mcp-acp-smoke",
        string? goalId = null,
        string? taskId = null,
        string? repo = null)
    {
        var repositoryPath = guard.Resolve(repo);
        var session = await agentService.StartSessionAsync(
            new StartAgentSessionRequest(
                repositoryPath,
                purpose,
                "mcp-acp-bridge",
                AgentSessionMode.ReadOnly,
                goalId,
                taskId),
            cancellationToken);
        var turn = await agentService.ReceivePromptAsync(
            new ReceiveAgentPromptRequest(
                repositoryPath,
                session.SessionId,
                prompt,
                purpose,
                goalId,
                taskId,
                PersistTrigger: false),
            cancellationToken);

        var payload = new
        {
            command = "ctx_acp_session_smoke",
            transport = "mcp-tool-over-internal-agent-service",
            acpShape = "initialize -> session/new -> session/update -> session/prompt",
            initialize = new
            {
                protocolVersion = 1,
                agentCapabilities = new
                {
                    loadSession = false,
                    promptCapabilities = new
                    {
                        image = false,
                        audio = false,
                        embeddedContext = false
                    },
                    mcpCapabilities = new
                    {
                        http = false,
                        sse = false
                    },
                    sessionCapabilities = new { },
                    _meta = new
                    {
                        ctx = new
                        {
                            phase = "read-only",
                            cognitiveWrite = false,
                            filesystemWrite = false,
                            terminal = false,
                            git = false,
                            release = false,
                            publicSync = false
                        }
                    }
                },
                agentInfo = new
                {
                    name = "ctx-agent-acp",
                    title = "CTX Agent ACP",
                    version = DomainConstants.ProductVersion
                },
                authMethods = Array.Empty<object>()
            },
            sessionNew = new
            {
                sessionId = session.SessionId,
                _meta = BuildSessionMeta(session)
            },
            sessionUpdate = new
            {
                sessionId = turn.Session.SessionId,
                update = new
                {
                    sessionUpdate = "plan",
                    entries = BuildEntries(turn.Plan.Planning)
                }
            },
            sessionPrompt = new
            {
                stopReason = "end_turn",
                _meta = new
                {
                    ctx = new
                    {
                        mutated = turn.Mutated,
                        lastPlanPacketId = turn.Session.LastPlanPacketId,
                        activeGoalId = turn.Session.ActiveGoalId,
                        activeTaskId = turn.Session.ActiveTaskId
                    }
                }
            }
        };

        return new CommandResult(true, "ACP-style read-only session smoke completed.", payload);
    }

    private static object BuildSessionMeta(AgentSessionSummary session)
        => new
        {
            ctx = new
            {
                mode = "read-only",
                state = session.State.ToString().ToLowerInvariant(),
                lastPlanPacketId = session.LastPlanPacketId,
                activeGoalId = session.ActiveGoalId,
                activeTaskId = session.ActiveTaskId
            }
        };

    private static IReadOnlyList<object> BuildEntries(PlanningSummary planning)
    {
        var entries = new List<object>();
        if (planning.Next.Recommended is not null)
        {
            entries.Add(new
            {
                content = $"Next: {planning.Next.Recommended.Title}",
                priority = "high",
                status = "pending"
            });
        }

        foreach (var runbook in planning.RunbookSuggestions.Take(2))
        {
            entries.Add(new
            {
                content = $"Runbook: {runbook.Title}",
                priority = "medium",
                status = "pending"
            });
        }

        foreach (var guidance in planning.Guidance.Take(3))
        {
            entries.Add(new
            {
                content = guidance,
                priority = "low",
                status = "pending"
            });
        }

        entries.Add(new
        {
            content = $"Use context packet {planning.Context.Id.Value} as the compact planning anchor for this turn.",
            priority = "low",
            status = "pending"
        });

        return entries;
    }
}
