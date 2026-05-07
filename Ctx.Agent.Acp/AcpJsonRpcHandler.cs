namespace Ctx.Agent.Acp;

using System.Text.Json;
using System.Collections.Concurrent;
using Ctx.Agent;
using Ctx.Application;
using Ctx.Domain;

public sealed class AcpJsonRpcHandler
{
    private const int ProtocolVersion = 1;

    private readonly ICtxAgentService _agentService;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly string? _repositoryRoot;
    private readonly ConcurrentDictionary<string, string> _sessionRepositories = new(StringComparer.Ordinal);

    public AcpJsonRpcHandler(ICtxAgentService agentService, JsonSerializerOptions? jsonOptions = null, string? repositoryRoot = null)
    {
        _agentService = agentService;
        _repositoryRoot = string.IsNullOrWhiteSpace(repositoryRoot) ? null : Path.GetFullPath(repositoryRoot);
        _jsonOptions = jsonOptions is null
            ? new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            }
            : new JsonSerializerOptions(jsonOptions)
        {
            WriteIndented = false
        };
    }

    public async Task<IReadOnlyList<string>> HandleLineAsync(string line, CancellationToken cancellationToken)
    {
        try
        {
            using var document = JsonDocument.Parse(line);
            var root = document.RootElement;
            var id = root.TryGetProperty("id", out var idElement) ? idElement.Clone() : default(JsonElement?);

            if (!root.TryGetProperty("jsonrpc", out var jsonrpc) || jsonrpc.GetString() != "2.0")
            {
                return id is null ? Array.Empty<string>() : One(Error(id.Value, -32600, "Invalid JSON-RPC version."));
            }

            if (!root.TryGetProperty("method", out var methodElement))
            {
                return id is null ? Array.Empty<string>() : One(Error(id.Value, -32600, "Missing JSON-RPC method."));
            }

            var method = methodElement.GetString();
            var parameters = root.TryGetProperty("params", out var paramsElement) ? paramsElement : default;

            return method switch
            {
                "initialize" => id is null ? Array.Empty<string>() : One(Result(id.Value, HandleInitialize(parameters))),
                "session/new" => id is null ? Array.Empty<string>() : One(Result(id.Value, await HandleSessionNewAsync(parameters, cancellationToken))),
                "session/prompt" => id is null
                    ? Array.Empty<string>()
                    : await HandleSessionPromptAsync(id.Value, parameters, cancellationToken),
                "session/cancel" => Array.Empty<string>(),
                _ => id is null ? Array.Empty<string>() : One(Error(id.Value, -32601, $"Method '{method}' is not supported by Ctx.Agent.Acp Phase 1."))
            };
        }
        catch (JsonException exception)
        {
            return One(JsonSerializer.Serialize(new
            {
                jsonrpc = "2.0",
                id = (object?)null,
                error = new
                {
                    code = -32700,
                    message = $"Parse error: {exception.Message}"
                }
            }, _jsonOptions));
        }
        catch (Exception exception)
        {
            return One(JsonSerializer.Serialize(new
            {
                jsonrpc = "2.0",
                id = (object?)null,
                error = new
                {
                    code = -32000,
                    message = exception.Message
                }
            }, _jsonOptions));
        }
    }

    private object HandleInitialize(JsonElement parameters)
    {
        var requestedVersion = parameters.ValueKind == JsonValueKind.Object
            && parameters.TryGetProperty("protocolVersion", out var protocolVersion)
            && protocolVersion.TryGetInt32(out var value)
                ? value
                : ProtocolVersion;

        return new
        {
            protocolVersion = requestedVersion == ProtocolVersion ? requestedVersion : ProtocolVersion,
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
        };
    }

    private async Task<object> HandleSessionNewAsync(JsonElement parameters, CancellationToken cancellationToken)
    {
        var cwd = RequiredString(parameters, "cwd");
        if (!Path.IsPathFullyQualified(cwd))
        {
            throw new InvalidOperationException("session/new requires an absolute cwd.");
        }

        var repositoryPath = Path.GetFullPath(cwd);
        if (_repositoryRoot is not null && !string.Equals(repositoryPath, _repositoryRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"session/new cwd must match the configured repository: {_repositoryRoot}");
        }

        var session = await _agentService.StartSessionAsync(
            new StartAgentSessionRequest(
                repositoryPath,
                "ACP session",
                "acp-client",
                AgentSessionMode.ReadOnly),
            cancellationToken);
        _sessionRepositories[session.SessionId] = repositoryPath;

        return new
        {
            sessionId = session.SessionId,
            _meta = BuildSessionMeta(session)
        };
    }

    private async Task<IReadOnlyList<string>> HandleSessionPromptAsync(JsonElement id, JsonElement parameters, CancellationToken cancellationToken)
    {
        var sessionId = RequiredString(parameters, "sessionId");
        var prompt = ExtractPromptText(parameters);
        var repositoryPath = ResolveRepositoryPath(sessionId, parameters);
        var result = await _agentService.ReceivePromptAsync(
            new ReceiveAgentPromptRequest(repositoryPath, sessionId, prompt),
            cancellationToken);

        return new[]
        {
            Notification("session/update", new
            {
                sessionId,
                update = BuildPlanUpdate(result.Plan.Planning)
            }),
            Result(id, new
            {
                stopReason = "end_turn",
                _meta = new
                {
                    ctx = new
                    {
                        result.Mutated,
                        result.Session.LastPlanPacketId,
                        result.Session.ActiveGoalId,
                        result.Session.ActiveTaskId
                    }
                }
            })
        };
    }

    private static object BuildPlanUpdate(PlanningSummary planning)
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

        foreach (var runbook in planning.RunbookSuggestions)
        {
            entries.Add(new
            {
                content = $"Runbook: {runbook.Title}. Check Preconditions, follow applicable Do steps, verify with Verify, and stop at EscalationBoundary if a failure signal appears.",
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

        if (entries.Count == 0)
        {
            entries.Add(new
            {
                content = "Use the CTX planning packet as the next-turn context.",
                priority = "medium",
                status = "pending"
            });
        }

        return new
        {
            sessionUpdate = "plan",
            entries
        };
    }

    private string ResolveRepositoryPath(string sessionId, JsonElement parameters)
    {
        if (_sessionRepositories.TryGetValue(sessionId, out var repositoryPath))
        {
            return repositoryPath;
        }

        return ResolveRepositoryPathFromMeta(parameters);
    }

    private static string ResolveRepositoryPathFromMeta(JsonElement parameters)
    {
        if (parameters.ValueKind == JsonValueKind.Object
            && parameters.TryGetProperty("_meta", out var meta)
            && meta.ValueKind == JsonValueKind.Object
            && meta.TryGetProperty("ctx", out var ctx)
            && ctx.ValueKind == JsonValueKind.Object
            && ctx.TryGetProperty("repositoryPath", out var repositoryPath)
            && repositoryPath.ValueKind == JsonValueKind.String
            && !string.IsNullOrWhiteSpace(repositoryPath.GetString()))
        {
            return Path.GetFullPath(repositoryPath.GetString()!);
        }

        throw new InvalidOperationException("session/prompt requires params._meta.ctx.repositoryPath in Ctx.Agent.Acp Phase 1.");
    }

    private static string ExtractPromptText(JsonElement parameters)
    {
        if (!parameters.TryGetProperty("prompt", out var prompt) || prompt.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException("session/prompt requires a prompt array.");
        }

        var parts = new List<string>();
        foreach (var block in prompt.EnumerateArray())
        {
            if (block.ValueKind != JsonValueKind.Object || !block.TryGetProperty("type", out var type))
            {
                continue;
            }

            if (type.GetString() == "text" && block.TryGetProperty("text", out var text))
            {
                parts.Add(text.GetString() ?? string.Empty);
            }
            else if ((type.GetString() == "resource_link" || type.GetString() == "resourceLink")
                && block.TryGetProperty("uri", out var uri))
            {
                parts.Add($"Resource: {uri.GetString()}");
            }
        }

        var promptText = string.Join(Environment.NewLine, parts.Where(part => !string.IsNullOrWhiteSpace(part)));
        if (string.IsNullOrWhiteSpace(promptText))
        {
            throw new InvalidOperationException("session/prompt did not include supported text or resource_link content.");
        }

        return promptText;
    }

    private static string RequiredString(JsonElement parameters, string propertyName)
    {
        if (parameters.ValueKind != JsonValueKind.Object
            || !parameters.TryGetProperty(propertyName, out var value)
            || value.ValueKind != JsonValueKind.String
            || string.IsNullOrWhiteSpace(value.GetString()))
        {
            throw new InvalidOperationException($"Missing required parameter: {propertyName}");
        }

        return value.GetString()!;
    }

    private static object BuildSessionMeta(AgentSessionSummary session)
    {
        return new
        {
            ctx = new
            {
                mode = ToWireMode(session.Mode),
                state = session.State.ToString().ToLowerInvariant(),
                session.LastPlanPacketId,
                session.ActiveGoalId,
                session.ActiveTaskId
            }
        };
    }

    private string Result(JsonElement id, object result)
    {
        return JsonSerializer.Serialize(new
        {
            jsonrpc = "2.0",
            id,
            result
        }, _jsonOptions);
    }

    private string Error(JsonElement id, int code, string message)
    {
        return JsonSerializer.Serialize(new
        {
            jsonrpc = "2.0",
            id,
            error = new
            {
                code,
                message
            }
        }, _jsonOptions);
    }

    private string Notification(string method, object parameters)
    {
        return JsonSerializer.Serialize(new
        {
            jsonrpc = "2.0",
            method,
            @params = parameters
        }, _jsonOptions);
    }

    private static IReadOnlyList<string> One(string response)
    {
        return new[] { response };
    }

    private static string ToWireMode(AgentSessionMode mode)
    {
        return mode switch
        {
            AgentSessionMode.ReadOnly => "read-only",
            AgentSessionMode.CognitiveWrite => "cognitive-write",
            AgentSessionMode.Execution => "execution",
            _ => mode.ToString().ToLowerInvariant()
        };
    }
}
