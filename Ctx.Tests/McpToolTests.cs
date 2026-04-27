namespace Ctx.Tests;

using Ctx.Application;
using Ctx.Infrastructure;
using Ctx.Mcp.Mcp;
using Xunit;

public sealed class McpToolTests
{
    [Fact]
    public async Task WriteTool_ThrowsWhenServerRunsReadOnly()
    {
        var repositoryPath = CreateTempRepositoryPath();
        var runtime = Bootstrapper.Create();

        try
        {
            await runtime.ApplicationService.InitAsync(
                repositoryPath,
                new InitRepositoryRequest("MCP Test", "Read-only guard", "main", "tester"),
                CancellationToken.None);
            var guard = new RepositoryGuard(new CtxMcpOptions(repositoryPath, "read-only", null));

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                CtxWriteTools.AddTaskAsync(
                    runtime.ApplicationService,
                    guard,
                    CancellationToken.None,
                    "Blocked task"));

            Assert.Contains("read-only mode", exception.Message);
        }
        finally
        {
            DeleteTempRepository(repositoryPath);
        }
    }

    [Fact]
    public async Task WriteTools_CanAddTaskAndCommitWhenServerRunsWriteMode()
    {
        var repositoryPath = CreateTempRepositoryPath();
        var runtime = Bootstrapper.Create();

        try
        {
            await runtime.ApplicationService.InitAsync(
                repositoryPath,
                new InitRepositoryRequest("MCP Test", "Write mode", "main", "tester"),
                CancellationToken.None);
            var guard = new RepositoryGuard(new CtxMcpOptions(repositoryPath, "write", null));

            var addTask = await CtxWriteTools.AddTaskAsync(
                runtime.ApplicationService,
                guard,
                CancellationToken.None,
                "MCP writable task",
                createdBy: "tester");
            var dirtyStatus = await CtxReadTools.StatusAsync(
                runtime.ApplicationService,
                guard,
                CancellationToken.None);
            var commit = await CtxWriteTools.CommitAsync(
                runtime.ApplicationService,
                guard,
                CancellationToken.None,
                "mcp writable commit",
                createdBy: "tester");
            var cleanStatus = await CtxReadTools.StatusAsync(
                runtime.ApplicationService,
                guard,
                CancellationToken.None);

            Assert.True(addTask.Success);
            Assert.True(commit.Success);
            Assert.True(((StatusSummary)dirtyStatus.Data!).Dirty);
            Assert.False(((StatusSummary)cleanStatus.Data!).Dirty);
        }
        finally
        {
            DeleteTempRepository(repositoryPath);
        }
    }

    [Fact]
    public async Task RepositoryGuard_BlocksDynamicRepositoryWithoutAllowRoot()
    {
        var configuredPath = CreateTempRepositoryPath();
        var otherPath = CreateTempRepositoryPath();
        var runtime = Bootstrapper.Create();

        try
        {
            await runtime.ApplicationService.InitAsync(
                configuredPath,
                new InitRepositoryRequest("Configured", "Configured repo", "main", "tester"),
                CancellationToken.None);
            await runtime.ApplicationService.InitAsync(
                otherPath,
                new InitRepositoryRequest("Other", "Other repo", "main", "tester"),
                CancellationToken.None);
            var guard = new RepositoryGuard(new CtxMcpOptions(configuredPath, "write", null));

            var exception = Assert.Throws<InvalidOperationException>(() => guard.Resolve(otherPath));

            Assert.Contains("Dynamic repo paths are disabled", exception.Message);
        }
        finally
        {
            DeleteTempRepository(configuredPath);
            DeleteTempRepository(otherPath);
        }
    }

    private static string CreateTempRepositoryPath()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-mcp-tests", Guid.NewGuid().ToString("N"));
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
