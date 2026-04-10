namespace Ctx.Tests;

using Ctx.Core;
using Ctx.Persistence;

public sealed class MetricsRepositoryTests
{
    [Fact]
    public async System.Threading.Tasks.Task LoadAsync_CanReadWhileAnotherHandleHasTheFileOpen()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repositoryPath);

        try
        {
            var jsonSerializer = new DefaultJsonSerializer();
            var metricsRepository = new FileSystemMetricsRepository(jsonSerializer);
            var service = CreateService(jsonSerializer);

            await service.InitAsync(repositoryPath, new Ctx.Application.InitRepositoryRequest("CTX", "Metrics lock test", "main", "tester"), CancellationToken.None);
            var metricsPath = Path.Combine(repositoryPath, ".ctx", "metrics", "usage.json");

            await using var lockStream = new FileStream(
                metricsPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite | FileShare.Delete);

            var snapshot = await metricsRepository.LoadAsync(repositoryPath, CancellationToken.None);

            Assert.Equal(0, snapshot.TotalRuns);
        }
        finally
        {
            if (Directory.Exists(repositoryPath))
            {
                Directory.Delete(repositoryPath, recursive: true);
            }
        }
    }

    [Fact]
    public async System.Threading.Tasks.Task SaveAsync_WritesUpdatedMetricsSnapshot()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repositoryPath);

        try
        {
            var jsonSerializer = new DefaultJsonSerializer();
            var metricsRepository = new FileSystemMetricsRepository(jsonSerializer);
            var service = CreateService(jsonSerializer);

            await service.InitAsync(repositoryPath, new Ctx.Application.InitRepositoryRequest("CTX", "Metrics save test", "main", "tester"), CancellationToken.None);

            var snapshot = new Ctx.Domain.MetricsSnapshot(1, 42, 1.5m, 0, 0, TimeSpan.FromSeconds(3))
                .RecordCommandUsage("status", true, TimeSpan.FromMilliseconds(100), DateTimeOffset.UtcNow);

            await metricsRepository.SaveAsync(repositoryPath, snapshot, CancellationToken.None);
            var reloaded = await metricsRepository.LoadAsync(repositoryPath, CancellationToken.None);

            Assert.Equal(1, reloaded.TotalRuns);
            Assert.Equal(42, reloaded.TotalTokens);
            Assert.Equal(1, reloaded.TotalCommandInvocations);
            Assert.Single(reloaded.CommandUsage);
        }
        finally
        {
            if (Directory.Exists(repositoryPath))
            {
                Directory.Delete(repositoryPath, recursive: true);
            }
        }
    }

    private static Ctx.Core.CtxApplicationService CreateService(DefaultJsonSerializer jsonSerializer)
    {
        var clock = new FixedClock(new DateTimeOffset(2026, 4, 9, 12, 0, 0, TimeSpan.Zero));
        var hashing = new Sha256HashingService();
        var workingRepository = new FileSystemWorkingContextRepository(jsonSerializer);
        var commitRepository = new FileSystemCommitRepository(jsonSerializer);
        var branchRepository = new FileSystemBranchRepository(jsonSerializer);
        var packetRepository = new FileSystemPacketRepository(jsonSerializer);
        var runRepository = new FileSystemRunRepository(jsonSerializer);
        var metricsRepository = new FileSystemMetricsRepository(jsonSerializer);
        var contextBuilder = new ContextBuilder(clock, hashing);
        var commitEngine = new CommitEngine(clock, hashing, jsonSerializer, new DiffEngine());
        var mergeEngine = new MergeEngine();
        var providers = new Ctx.Providers.AIProviderRegistry(Array.Empty<Ctx.Application.IAIProvider>());
        var runOrchestrator = new RunOrchestrator(contextBuilder, providers, packetRepository, runRepository, metricsRepository, clock, hashing);
        return new CtxApplicationService(workingRepository, commitRepository, branchRepository, runRepository, packetRepository, metricsRepository, runOrchestrator, contextBuilder, commitEngine, mergeEngine, clock);
    }
}
