namespace Ctx.Infrastructure;

using System.Text.Json;
using Ctx.Application;
using Ctx.Core;
using Ctx.Persistence;
using Ctx.Providers;

public sealed class CtxRuntime
{
    public CtxRuntime(ICtxApplicationService applicationService, IMetricsRepository metricsRepository, JsonSerializerOptions jsonOptions)
    {
        ApplicationService = applicationService;
        MetricsRepository = metricsRepository;
        JsonOptions = jsonOptions;
    }

    public ICtxApplicationService ApplicationService { get; }
    public IMetricsRepository MetricsRepository { get; }
    public JsonSerializerOptions JsonOptions { get; }
}

public static class Bootstrapper
{
    public static CtxRuntime Create()
    {
        var jsonSerializer = new DefaultJsonSerializer();
        var clock = new SystemClock();
        var hashingService = new Sha256HashingService();

        var workingRepository = new FileSystemWorkingContextRepository(jsonSerializer);
        var commitRepository = new FileSystemCommitRepository(jsonSerializer);
        var branchRepository = new FileSystemBranchRepository(jsonSerializer);
        var packetRepository = new FileSystemPacketRepository(jsonSerializer);
        var runRepository = new FileSystemRunRepository(jsonSerializer);
        var metricsRepository = new FileSystemMetricsRepository(jsonSerializer);
        var repositoryWriteLock = new FileSystemRepositoryWriteLock();

        var contextBuilder = new ContextBuilder(clock, hashingService);
        var diffEngine = new DiffEngine();
        var commitEngine = new CommitEngine(clock, hashingService, jsonSerializer, diffEngine);
        var mergeEngine = new MergeEngine();
        var providerRegistry = new AIProviderRegistry(new IAIProvider[]
        {
            new OpenAiProvider(new HttpClient()),
            new AnthropicProvider(new HttpClient())
        });
        var runOrchestrator = new RunOrchestrator(contextBuilder, providerRegistry, packetRepository, runRepository, metricsRepository, clock, hashingService);
        var applicationService = new CtxApplicationService(workingRepository, commitRepository, branchRepository, runRepository, packetRepository, metricsRepository, runOrchestrator, contextBuilder, commitEngine, mergeEngine, clock, repositoryWriteLock);

        return new CtxRuntime(
            applicationService,
            metricsRepository,
            new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
    }
}
