namespace Ctx.Tests;

using System.Text.Json;
using Ctx.Agent.Acp;
using Ctx.Application;
using Ctx.Infrastructure;
using Xunit;

public sealed class AcpJsonRpcHandlerTests
{
    [Fact]
    public async Task Initialize_ReturnsReadOnlyAgentCapabilities()
    {
        var runtime = Bootstrapper.Create();
        var handler = new AcpJsonRpcHandler(runtime.AgentService, runtime.JsonOptions);

        var responses = await handler.HandleLineAsync(
            """
            {"jsonrpc":"2.0","id":0,"method":"initialize","params":{"protocolVersion":1,"clientCapabilities":{"terminal":true}}}
            """,
            CancellationToken.None);

        var response = ParseSingle(responses);
        Assert.Equal(0, response.RootElement.GetProperty("id").GetInt32());
        var result = response.RootElement.GetProperty("result");
        Assert.Equal(1, result.GetProperty("protocolVersion").GetInt32());
        Assert.False(result.GetProperty("agentCapabilities").GetProperty("loadSession").GetBoolean());
        Assert.False(result.GetProperty("agentCapabilities").GetProperty("promptCapabilities").GetProperty("image").GetBoolean());
        Assert.False(result.GetProperty("agentCapabilities").GetProperty("mcpCapabilities").GetProperty("http").GetBoolean());
        Assert.Equal("ctx-agent-acp", result.GetProperty("agentInfo").GetProperty("name").GetString());
    }

    [Fact]
    public async Task SessionNew_ReturnsSessionIdForAbsoluteCtxRepository()
    {
        var repositoryPath = CreateTempRepositoryPath();
        var runtime = Bootstrapper.Create();

        try
        {
            await runtime.ApplicationService.InitAsync(
                repositoryPath,
                new InitRepositoryRequest("ACP Test", "session/new", "main", "tester"),
                CancellationToken.None);
            var handler = new AcpJsonRpcHandler(runtime.AgentService, runtime.JsonOptions);

            var responses = await handler.HandleLineAsync(
                JsonSerializer.Serialize(new
                {
                    jsonrpc = "2.0",
                    id = 1,
                    method = "session/new",
                    @params = new
                    {
                        cwd = repositoryPath,
                        mcpServers = Array.Empty<object>()
                    }
                }),
                CancellationToken.None);

            var response = ParseSingle(responses);
            var sessionId = response.RootElement.GetProperty("result").GetProperty("sessionId").GetString();
            Assert.False(string.IsNullOrWhiteSpace(sessionId));
        }
        finally
        {
            DeleteTempRepository(repositoryPath);
        }
    }

    [Fact]
    public async Task SessionPrompt_EmitsPlanNotificationAndEndTurnResponse()
    {
        var repositoryPath = CreateTempRepositoryPath();
        var runtime = Bootstrapper.Create();

        try
        {
            await runtime.ApplicationService.InitAsync(
                repositoryPath,
                new InitRepositoryRequest("ACP Test", "session/prompt", "main", "tester"),
                CancellationToken.None);
            await runtime.ApplicationService.AddTaskAsync(
                repositoryPath,
                new AddTaskRequest("ACP planning task", "Plan through ACP", null, Array.Empty<string>(), "tester"),
                CancellationToken.None);
            var handler = new AcpJsonRpcHandler(runtime.AgentService, runtime.JsonOptions);
            var sessionResponse = ParseSingle(await handler.HandleLineAsync(
                JsonSerializer.Serialize(new
                {
                    jsonrpc = "2.0",
                    id = 1,
                    method = "session/new",
                    @params = new
                    {
                        cwd = repositoryPath,
                        mcpServers = Array.Empty<object>()
                    }
                }),
                CancellationToken.None));
            var sessionId = sessionResponse.RootElement.GetProperty("result").GetProperty("sessionId").GetString();

            var responses = await handler.HandleLineAsync(
                JsonSerializer.Serialize(new
                {
                    jsonrpc = "2.0",
                    id = 2,
                    method = "session/prompt",
                    @params = new
                    {
                        sessionId,
                        prompt = new[]
                        {
                            new
                            {
                                type = "text",
                                text = "What is next?"
                            }
                        }
                    }
                }),
                CancellationToken.None);

            Assert.Equal(2, responses.Count);
            using var notification = JsonDocument.Parse(responses[0]);
            using var promptResponse = JsonDocument.Parse(responses[1]);
            Assert.Equal("session/update", notification.RootElement.GetProperty("method").GetString());
            Assert.Equal("plan", notification.RootElement.GetProperty("params").GetProperty("update").GetProperty("sessionUpdate").GetString());
            Assert.Equal("end_turn", promptResponse.RootElement.GetProperty("result").GetProperty("stopReason").GetString());
        }
        finally
        {
            DeleteTempRepository(repositoryPath);
        }
    }

    [Fact]
    public async Task LocalConnectionFlow_InitializeSessionPromptReturnsReadOnlyPlan()
    {
        var repositoryPath = CreateTempRepositoryPath();
        var runtime = Bootstrapper.Create();

        try
        {
            await runtime.ApplicationService.InitAsync(
                repositoryPath,
                new InitRepositoryRequest("ACP Connection Test", "local protocol connection", "main", "tester"),
                CancellationToken.None);
            await runtime.ApplicationService.AddTaskAsync(
                repositoryPath,
                new AddTaskRequest("Review ACP connection", "Validate the local ACP handshake", null, Array.Empty<string>(), "tester"),
                CancellationToken.None);
            var handler = new AcpJsonRpcHandler(runtime.AgentService, runtime.JsonOptions, repositoryPath);

            var initialize = ParseSingle(await handler.HandleLineAsync(
                """
                {"jsonrpc":"2.0","id":0,"method":"initialize","params":{"protocolVersion":1,"clientCapabilities":{}}}
                """,
                CancellationToken.None));
            var capabilities = initialize.RootElement.GetProperty("result").GetProperty("agentCapabilities");
            Assert.False(capabilities.GetProperty("_meta").GetProperty("ctx").GetProperty("cognitiveWrite").GetBoolean());
            Assert.False(capabilities.GetProperty("_meta").GetProperty("ctx").GetProperty("filesystemWrite").GetBoolean());
            Assert.False(capabilities.GetProperty("_meta").GetProperty("ctx").GetProperty("terminal").GetBoolean());
            Assert.False(capabilities.GetProperty("_meta").GetProperty("ctx").GetProperty("git").GetBoolean());

            var session = ParseSingle(await handler.HandleLineAsync(
                JsonSerializer.Serialize(new
                {
                    jsonrpc = "2.0",
                    id = 1,
                    method = "session/new",
                    @params = new
                    {
                        cwd = repositoryPath,
                        mcpServers = Array.Empty<object>()
                    }
                }),
                CancellationToken.None));
            var sessionId = session.RootElement.GetProperty("result").GetProperty("sessionId").GetString();
            Assert.False(string.IsNullOrWhiteSpace(sessionId));

            var before = Assert.IsType<StatusSummary>((await runtime.ApplicationService.StatusAsync(repositoryPath, CancellationToken.None)).Data);
            var responses = await handler.HandleLineAsync(
                JsonSerializer.Serialize(new
                {
                    jsonrpc = "2.0",
                    id = 2,
                    method = "session/prompt",
                    @params = new
                    {
                        sessionId,
                        prompt = new[]
                        {
                            new
                            {
                                type = "text",
                                text = "What is the next safe planning step from CTX?"
                            }
                        },
                        _meta = new
                        {
                            ctx = new
                            {
                                repositoryPath
                            }
                        }
                    }
                }),
                CancellationToken.None);
            var after = Assert.IsType<StatusSummary>((await runtime.ApplicationService.StatusAsync(repositoryPath, CancellationToken.None)).Data);

            Assert.Equal(2, responses.Count);
            using var update = JsonDocument.Parse(responses[0]);
            using var final = JsonDocument.Parse(responses[1]);
            Assert.Equal("session/update", update.RootElement.GetProperty("method").GetString());
            var entries = update.RootElement.GetProperty("params").GetProperty("update").GetProperty("entries");
            Assert.True(entries.GetArrayLength() > 0);
            Assert.Equal("end_turn", final.RootElement.GetProperty("result").GetProperty("stopReason").GetString());
            Assert.False(final.RootElement.GetProperty("result").GetProperty("_meta").GetProperty("ctx").GetProperty("mutated").GetBoolean());
            Assert.Equal(before.Dirty, after.Dirty);
        }
        finally
        {
            DeleteTempRepository(repositoryPath);
        }
    }

    [Fact]
    public async Task SessionNew_RejectsCwdOutsideConfiguredRepository()
    {
        var configuredRepositoryPath = CreateTempRepositoryPath();
        var otherRepositoryPath = CreateTempRepositoryPath();
        var runtime = Bootstrapper.Create();

        try
        {
            await runtime.ApplicationService.InitAsync(
                configuredRepositoryPath,
                new InitRepositoryRequest("ACP Configured", "configured", "main", "tester"),
                CancellationToken.None);
            await runtime.ApplicationService.InitAsync(
                otherRepositoryPath,
                new InitRepositoryRequest("ACP Other", "other", "main", "tester"),
                CancellationToken.None);
            var handler = new AcpJsonRpcHandler(runtime.AgentService, runtime.JsonOptions, configuredRepositoryPath);

            var responses = await handler.HandleLineAsync(
                JsonSerializer.Serialize(new
                {
                    jsonrpc = "2.0",
                    id = 3,
                    method = "session/new",
                    @params = new
                    {
                        cwd = otherRepositoryPath,
                        mcpServers = Array.Empty<object>()
                    }
                }),
                CancellationToken.None);

            var response = ParseSingle(responses);
            Assert.Equal(-32000, response.RootElement.GetProperty("error").GetProperty("code").GetInt32());
            Assert.Contains("configured repository", response.RootElement.GetProperty("error").GetProperty("message").GetString());
        }
        finally
        {
            DeleteTempRepository(configuredRepositoryPath);
            DeleteTempRepository(otherRepositoryPath);
        }
    }

    private static JsonDocument ParseSingle(IReadOnlyList<string> responses)
    {
        Assert.Single(responses);
        return JsonDocument.Parse(responses[0]);
    }

    private static string CreateTempRepositoryPath()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-acp-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repositoryPath);
        return repositoryPath;
    }

    private static void DeleteTempRepository(string repositoryPath)
    {
        if (Directory.Exists(repositoryPath))
        {
            Directory.Delete(repositoryPath, recursive: true);
        }
    }
}
