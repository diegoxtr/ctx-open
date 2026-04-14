namespace Ctx.Core;

using Ctx.Application;
using Ctx.Domain;

public sealed class RunOrchestrator : IRunOrchestrator
{
    private readonly IContextBuilder _contextBuilder;
    private readonly IAIProviderRegistry _providerRegistry;
    private readonly IPacketRepository _packetRepository;
    private readonly IRunRepository _runRepository;
    private readonly IMetricsRepository _metricsRepository;
    private readonly IClock _clock;
    private readonly IHashingService _hashingService;
    private readonly IOperationalRunbookRepository? _runbookRepository;
    private readonly ICognitiveTriggerRepository? _triggerRepository;

    public RunOrchestrator(
        IContextBuilder contextBuilder,
        IAIProviderRegistry providerRegistry,
        IPacketRepository packetRepository,
        IRunRepository runRepository,
        IMetricsRepository metricsRepository,
        IClock clock,
        IHashingService hashingService,
        IOperationalRunbookRepository? runbookRepository = null,
        ICognitiveTriggerRepository? triggerRepository = null)
    {
        _contextBuilder = contextBuilder;
        _providerRegistry = providerRegistry;
        _packetRepository = packetRepository;
        _runRepository = runRepository;
        _metricsRepository = metricsRepository;
        _clock = clock;
        _hashingService = hashingService;
        _runbookRepository = runbookRepository;
        _triggerRepository = triggerRepository;
    }

    public async System.Threading.Tasks.Task<Run> ExecuteAsync(string repositoryPath, WorkingContext workingContext, RunRequest request, CancellationToken cancellationToken)
    {
        var runbooks = _runbookRepository is null
            ? Array.Empty<OperationalRunbook>()
            : await _runbookRepository.ListAsync(repositoryPath, cancellationToken);
        var triggers = _triggerRepository is null
            ? Array.Empty<CognitiveTrigger>()
            : await _triggerRepository.ListAsync(repositoryPath, cancellationToken);

        var packet = _contextBuilder.Build(workingContext, runbooks, triggers, request.Purpose, request.GoalId, request.TaskId);
        await _packetRepository.SaveAsync(repositoryPath, packet, cancellationToken);

        var provider = _providerRegistry.Get(request.Provider);
        var result = await provider.ExecuteAsync(packet, new ProviderExecutionRequest(request.Model, request.Purpose), cancellationToken);

        var run = new Run(
            RunId.New(),
            result.Provider,
            result.Model,
            RunState.Completed,
            _clock.UtcNow.Subtract(result.Usage.Duration),
            _clock.UtcNow,
            packet.Id,
            result.Usage,
            _hashingService.Hash($"{packet.Fingerprint}:{result.Provider}:{result.Model}:{request.Purpose}"),
            result.Summary,
            result.Artifacts,
            new Traceability(
                request.RequestedBy,
                _clock.UtcNow,
                null,
                null,
                new[] { "run", request.Provider },
                new[] { packet.Id.Value },
                request.Model,
                Environment.GetEnvironmentVariable("CTX_MODEL_VERSION")));

        await _runRepository.SaveAsync(repositoryPath, run, cancellationToken);

        var metrics = await _metricsRepository.LoadAsync(repositoryPath, cancellationToken);
        await _metricsRepository.SaveAsync(
            repositoryPath,
            metrics with
            {
                TotalRuns = metrics.TotalRuns + 1,
                TotalTokens = metrics.TotalTokens + result.Usage.TotalTokens,
                TotalAcuCost = metrics.TotalAcuCost + result.Usage.AcuCost,
                TotalExecutionTime = metrics.TotalExecutionTime + result.Usage.Duration,
                AvoidedRedundancyCount = metrics.AvoidedRedundancyCount + 1
            },
            cancellationToken);

        return run;
    }
}
