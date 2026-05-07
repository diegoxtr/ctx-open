namespace Ctx.Tests;

using Ctx.Agent;
using Ctx.Application;
using Ctx.Infrastructure;
using Xunit;

public sealed class AgentServiceTests
{
    [Fact]
    public async Task StartSessionAsync_ReturnsInitialPlanningPacket()
    {
        var repositoryPath = CreateTempRepositoryPath();
        var runtime = Bootstrapper.Create();

        try
        {
            await runtime.ApplicationService.InitAsync(
                repositoryPath,
                new InitRepositoryRequest("Agent Test", "Read-only agent session", "main", "tester"),
                CancellationToken.None);
            var task = await runtime.ApplicationService.AddTaskAsync(
                repositoryPath,
                new AddTaskRequest("Agent task", "Plan this work", null, Array.Empty<string>(), "tester"),
                CancellationToken.None);
            var taskId = ((Ctx.Domain.Task)task.Data!).Id.Value;

            var session = await runtime.AgentService.StartSessionAsync(
                new StartAgentSessionRequest(repositoryPath, "agent smoke", "tester", TaskId: taskId),
                CancellationToken.None);

            Assert.Equal(AgentSessionMode.ReadOnly, session.Mode);
            Assert.Equal(AgentSessionState.Active, session.State);
            Assert.Equal(taskId, session.ActiveTaskId);
            Assert.NotNull(session.InitialPlan);
            Assert.Equal(taskId, session.InitialPlan.Next.Recommended?.EntityId);
            Assert.False(string.IsNullOrWhiteSpace(session.LastPlanPacketId));
        }
        finally
        {
            DeleteTempRepository(repositoryPath);
        }
    }

    [Fact]
    public async Task ReceivePromptAsync_InReadOnlyModePlansWithoutMutatingContext()
    {
        var repositoryPath = CreateTempRepositoryPath();
        var runtime = Bootstrapper.Create();

        try
        {
            await runtime.ApplicationService.InitAsync(
                repositoryPath,
                new InitRepositoryRequest("Agent Test", "Read-only prompt", "main", "tester"),
                CancellationToken.None);
            var session = await runtime.AgentService.StartSessionAsync(
                new StartAgentSessionRequest(repositoryPath, "agent prompt", "tester"),
                CancellationToken.None);
            var before = Assert.IsType<StatusSummary>((await runtime.ApplicationService.StatusAsync(repositoryPath, CancellationToken.None)).Data);

            var result = await runtime.AgentService.ReceivePromptAsync(
                new ReceiveAgentPromptRequest(repositoryPath, session.SessionId, "What should happen next?"),
                CancellationToken.None);
            var after = Assert.IsType<StatusSummary>((await runtime.ApplicationService.StatusAsync(repositoryPath, CancellationToken.None)).Data);

            Assert.False(result.Mutated);
            Assert.Equal(session.SessionId, result.Session.SessionId);
            Assert.Contains(result.Guidance, item => item.Contains("read-only", StringComparison.OrdinalIgnoreCase));
            Assert.Equal(before.Dirty, after.Dirty);
            Assert.Equal(before.Goals, after.Goals);
            Assert.Equal(before.Tasks, after.Tasks);
            Assert.Equal(before.Hypotheses, after.Hypotheses);
            Assert.Equal(before.Decisions, after.Decisions);
            Assert.Equal(before.Evidence, after.Evidence);
            Assert.Equal(before.Conclusions, after.Conclusions);
        }
        finally
        {
            DeleteTempRepository(repositoryPath);
        }
    }

    [Fact]
    public async Task ReceivePromptAsync_RejectsTriggerPersistenceInReadOnlyMode()
    {
        var repositoryPath = CreateTempRepositoryPath();
        var runtime = Bootstrapper.Create();

        try
        {
            await runtime.ApplicationService.InitAsync(
                repositoryPath,
                new InitRepositoryRequest("Agent Test", "Read-only trigger guard", "main", "tester"),
                CancellationToken.None);
            var session = await runtime.AgentService.StartSessionAsync(
                new StartAgentSessionRequest(repositoryPath, "agent prompt", "tester"),
                CancellationToken.None);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                runtime.AgentService.ReceivePromptAsync(
                    new ReceiveAgentPromptRequest(repositoryPath, session.SessionId, "Persist this", PersistTrigger: true),
                    CancellationToken.None));

            Assert.Contains("Read-only agent sessions", exception.Message);
        }
        finally
        {
            DeleteTempRepository(repositoryPath);
        }
    }

    private static string CreateTempRepositoryPath()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-agent-tests", Guid.NewGuid().ToString("N"));
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
