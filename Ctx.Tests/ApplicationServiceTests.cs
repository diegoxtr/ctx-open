namespace Ctx.Tests;

using Ctx.Core;
using Ctx.Persistence;
using System.Text.Json;
using Xunit;

public sealed class ApplicationServiceTests
{
    [Fact]
    public async Task InitAddGoalAndCommit_PersistsRepositoryState()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repositoryPath);

        try
        {
            var clock = new FixedClock(new DateTimeOffset(2026, 4, 7, 14, 0, 0, TimeSpan.Zero));
            var jsonSerializer = new DefaultJsonSerializer();
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
            var service = new CtxApplicationService(workingRepository, commitRepository, branchRepository, runRepository, packetRepository, metricsRepository, runOrchestrator, contextBuilder, commitEngine, mergeEngine, clock);

            var init = await service.InitAsync(repositoryPath, new Ctx.Application.InitRepositoryRequest("CTX", "Test repo", "main", "tester"), CancellationToken.None);
            var goal = await service.AddGoalAsync(repositoryPath, new Ctx.Application.AddGoalRequest("Ship", "Ship core", 1, null, "tester"), CancellationToken.None);
            var commit = await service.CommitAsync(repositoryPath, new Ctx.Application.CommitRequest("seed", "tester"), CancellationToken.None);
            var status = await service.StatusAsync(repositoryPath, CancellationToken.None);

            Assert.True(init.Success);
            Assert.True(goal.Success);
            Assert.True(commit.Success);
            Assert.True(status.Success);
            Assert.Contains("On branch main", status.Message);
            var statusData = Assert.IsType<Ctx.Application.StatusSummary>(status.Data);
            Assert.False(statusData.Dirty);
            Assert.Null(statusData.Pending);
            Assert.True(Directory.Exists(Path.Combine(repositoryPath, ".ctx", "commits")));
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
    public async Task AddDecisionEvidenceConclusion_WiresTraceableArtifacts()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repositoryPath);

        try
        {
            var clock = new FixedClock(new DateTimeOffset(2026, 4, 7, 16, 0, 0, TimeSpan.Zero));
            var jsonSerializer = new DefaultJsonSerializer();
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
            var service = new CtxApplicationService(workingRepository, commitRepository, branchRepository, runRepository, packetRepository, metricsRepository, runOrchestrator, contextBuilder, commitEngine, mergeEngine, clock);

            await service.InitAsync(repositoryPath, new Ctx.Application.InitRepositoryRequest("CTX", "Test repo", "main", "tester"), CancellationToken.None);
            var task = await service.AddTaskAsync(repositoryPath, new Ctx.Application.AddTaskRequest("Task", "desc", null, Array.Empty<string>(), "tester"), CancellationToken.None);
            var taskId = ((Ctx.Domain.Task)task.Data!).Id.Value;
            var hypothesis = await service.AddHypothesisAsync(repositoryPath, new Ctx.Application.AddHypothesisRequest("Hypothesis", "why", 0.8m, 0.7m, 0.6m, 0.3m, taskId, "tester"), CancellationToken.None);
            var hypothesisId = ((Ctx.Domain.Hypothesis)hypothesis.Data!).Id.Value;
            var evidence = await service.AddEvidenceAsync(repositoryPath, new Ctx.Application.AddEvidenceRequest("Evidence", "supports idea", "benchmark", "Benchmark", 0.9m, new[] { $"hypothesis:{hypothesisId}" }, "tester"), CancellationToken.None);
            var evidenceId = ((Ctx.Domain.Evidence)evidence.Data!).Id.Value;
            var decision = await service.AddDecisionAsync(repositoryPath, new Ctx.Application.AddDecisionRequest("Choose approach", "best tradeoff", "Accepted", new[] { hypothesisId }, new[] { evidenceId }, "tester"), CancellationToken.None);
            var decisionId = ((Ctx.Domain.Decision)decision.Data!).Id.Value;
            var conclusion = await service.AddConclusionAsync(repositoryPath, new Ctx.Application.AddConclusionRequest("Proceed with the chosen approach", "Accepted", new[] { decisionId }, new[] { evidenceId }, Array.Empty<string>(), Array.Empty<string>(), "tester"), CancellationToken.None);
            var status = await service.StatusAsync(repositoryPath, CancellationToken.None);

            Assert.True(evidence.Success);
            Assert.True(decision.Success);
            Assert.True(conclusion.Success);
            var statusData = Assert.IsType<Ctx.Application.StatusSummary>(status.Data);
            Assert.Equal(1, statusData.Conclusions);
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
    public async Task AddTask_StoresOptionalModelIdentityWhenConfigured()
    {
        var previousModelName = Environment.GetEnvironmentVariable("CTX_MODEL_NAME");
        var previousModelVersion = Environment.GetEnvironmentVariable("CTX_MODEL_VERSION");
        Environment.SetEnvironmentVariable("CTX_MODEL_NAME", "codex");
        Environment.SetEnvironmentVariable("CTX_MODEL_VERSION", "test-build");

        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repositoryPath);

        try
        {
            var clock = new FixedClock(new DateTimeOffset(2026, 4, 7, 16, 15, 0, TimeSpan.Zero));
            var jsonSerializer = new DefaultJsonSerializer();
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
            var service = new CtxApplicationService(workingRepository, commitRepository, branchRepository, runRepository, packetRepository, metricsRepository, runOrchestrator, contextBuilder, commitEngine, mergeEngine, clock);

            await service.InitAsync(repositoryPath, new Ctx.Application.InitRepositoryRequest("CTX", "Test repo", "main", "tester"), CancellationToken.None);
            var taskResult = await service.AddTaskAsync(repositoryPath, new Ctx.Application.AddTaskRequest("Task", "desc", null, Array.Empty<string>(), "tester"), CancellationToken.None);

            var task = Assert.IsType<Ctx.Domain.Task>(taskResult.Data);
            Assert.Equal("codex", task.Trace.ModelName);
            Assert.Equal("test-build", task.Trace.ModelVersion);
        }
        finally
        {
            Environment.SetEnvironmentVariable("CTX_MODEL_NAME", previousModelName);
            Environment.SetEnvironmentVariable("CTX_MODEL_VERSION", previousModelVersion);

            if (Directory.Exists(repositoryPath))
            {
                Directory.Delete(repositoryPath, recursive: true);
            }
        }
    }

    [Fact]
    public async Task Conclusion_CanReferenceGoalsAndTasks()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repositoryPath);

        try
        {
            var clock = new FixedClock(new DateTimeOffset(2026, 4, 7, 16, 30, 0, TimeSpan.Zero));
            var jsonSerializer = new DefaultJsonSerializer();
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
            var service = new CtxApplicationService(workingRepository, commitRepository, branchRepository, runRepository, packetRepository, metricsRepository, runOrchestrator, contextBuilder, commitEngine, mergeEngine, clock);

            await service.InitAsync(repositoryPath, new Ctx.Application.InitRepositoryRequest("CTX", "Conclusion goal/task test", "main", "tester"), CancellationToken.None);
            var goal = await service.AddGoalAsync(repositoryPath, new Ctx.Application.AddGoalRequest("Deliver V1", "Close product loop", 1, null, "tester"), CancellationToken.None);
            var goalId = ((Ctx.Domain.Goal)goal.Data!).Id.Value;
            var task = await service.AddTaskAsync(repositoryPath, new Ctx.Application.AddTaskRequest("Validate conclusion links", "Attach outputs to work", goalId, Array.Empty<string>(), "tester"), CancellationToken.None);
            var taskId = ((Ctx.Domain.Task)task.Data!).Id.Value;
            var hypothesis = await service.AddHypothesisAsync(repositoryPath, new Ctx.Application.AddHypothesisRequest("Explicit conclusion links improve traceability", "Need direct closure", 0.8m, 0.7m, 0.6m, 0.3m, taskId, "tester"), CancellationToken.None);
            var hypothesisId = ((Ctx.Domain.Hypothesis)hypothesis.Data!).Id.Value;
            var evidence = await service.AddEvidenceAsync(repositoryPath, new Ctx.Application.AddEvidenceRequest("Pilot evidence", "Links help scanning", "pilot", "Observation", 0.9m, new[] { $"hypothesis:{hypothesisId}" }, "tester"), CancellationToken.None);
            var evidenceId = ((Ctx.Domain.Evidence)evidence.Data!).Id.Value;
            var decision = await service.AddDecisionAsync(repositoryPath, new Ctx.Application.AddDecisionRequest("Keep explicit conclusion links", "Improves closure", "Accepted", new[] { hypothesisId }, new[] { evidenceId }, "tester"), CancellationToken.None);
            var decisionId = ((Ctx.Domain.Decision)decision.Data!).Id.Value;

            var conclusion = await service.AddConclusionAsync(repositoryPath, new Ctx.Application.AddConclusionRequest("Close the loop against work and objective", "Accepted", new[] { decisionId }, new[] { evidenceId }, new[] { goalId }, new[] { taskId }, "tester"), CancellationToken.None);

            Assert.True(conclusion.Success);
            var entity = Assert.IsType<Ctx.Domain.Conclusion>(conclusion.Data);
            Assert.Contains(entity.GoalIds, item => item.Value == goalId);
            Assert.Contains(entity.TaskIds, item => item.Value == taskId);
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
    public async Task ListAndShowArtifacts_ReturnStructuredResults()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repositoryPath);

        try
        {
            var clock = new FixedClock(new DateTimeOffset(2026, 4, 7, 17, 0, 0, TimeSpan.Zero));
            var jsonSerializer = new DefaultJsonSerializer();
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
            var service = new CtxApplicationService(workingRepository, commitRepository, branchRepository, runRepository, packetRepository, metricsRepository, runOrchestrator, contextBuilder, commitEngine, mergeEngine, clock);

            await service.InitAsync(repositoryPath, new Ctx.Application.InitRepositoryRequest("CTX", "Test repo", "main", "tester"), CancellationToken.None);
            var goal = await service.AddGoalAsync(repositoryPath, new Ctx.Application.AddGoalRequest("Ship", "Ship core", 1, null, "tester"), CancellationToken.None);
            var goalId = ((Ctx.Domain.Goal)goal.Data!).Id.Value;

            var list = await service.ListArtifactsAsync(repositoryPath, "goal", CancellationToken.None);
            var show = await service.ShowArtifactAsync(repositoryPath, "goal", goalId, CancellationToken.None);

            Assert.True(list.Success);
            Assert.True(show.Success);
            Assert.Single((IReadOnlyList<Ctx.Domain.Goal>)list.Data!);
            Assert.Equal(goalId, ((Ctx.Domain.Goal)show.Data!).Id.Value);
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
    public async Task OperationalInspection_ReturnsProvidersRunsPacketsAndMetrics()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repositoryPath);

        try
        {
            var clock = new FixedClock(new DateTimeOffset(2026, 4, 7, 18, 0, 0, TimeSpan.Zero));
            var jsonSerializer = new DefaultJsonSerializer();
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
            var providers = new Ctx.Providers.AIProviderRegistry(new Ctx.Application.IAIProvider[]
            {
                new Ctx.Providers.OpenAiProvider(new HttpClient()),
                new Ctx.Providers.AnthropicProvider(new HttpClient())
            });
            var runOrchestrator = new RunOrchestrator(contextBuilder, providers, packetRepository, runRepository, metricsRepository, clock, hashing);
            var service = new CtxApplicationService(workingRepository, commitRepository, branchRepository, runRepository, packetRepository, metricsRepository, runOrchestrator, contextBuilder, commitEngine, mergeEngine, clock);

            await service.InitAsync(repositoryPath, new Ctx.Application.InitRepositoryRequest("CTX", "Test repo", "main", "tester"), CancellationToken.None);
            await service.AddGoalAsync(repositoryPath, new Ctx.Application.AddGoalRequest("Ship", "Ship core", 1, null, "tester"), CancellationToken.None);
            var run = await service.RunAsync(repositoryPath, new Ctx.Application.RunRequest("openai", "Evaluate architecture", "gpt-4.1", null, null, "tester"), CancellationToken.None);
            var runId = ((Ctx.Domain.Run)run.Data!).Id.Value;
            var packetId = ((Ctx.Domain.Run)run.Data!).PacketId.Value;

            var providersResult = await service.ListProvidersAsync(repositoryPath, CancellationToken.None);
            var runsResult = await service.ListRunsAsync(repositoryPath, CancellationToken.None);
            var showRunResult = await service.ShowRunAsync(repositoryPath, runId, CancellationToken.None);
            var packetsResult = await service.ListPacketsAsync(repositoryPath, CancellationToken.None);
            var showPacketResult = await service.ShowPacketAsync(repositoryPath, packetId, CancellationToken.None);
            var metricsResult = await service.ShowMetricsAsync(repositoryPath, CancellationToken.None);

            Assert.True(providersResult.Success);
            Assert.True(runsResult.Success);
            Assert.True(showRunResult.Success);
            Assert.True(packetsResult.Success);
            Assert.True(showPacketResult.Success);
            Assert.True(metricsResult.Success);
            Assert.Equal(2, ((IReadOnlyList<Ctx.Domain.ProviderConfiguration>)providersResult.Data!).Count);
            Assert.Single((IReadOnlyList<Ctx.Domain.Run>)runsResult.Data!);
            Assert.Single((IReadOnlyList<Ctx.Domain.ContextPacket>)packetsResult.Data!);
            Assert.True(((Ctx.Domain.MetricsSnapshot)metricsResult.Data!).TotalRuns >= 1);
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
    public async Task Doctor_ReportsWarningWhenRepositoryIsMissing()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repositoryPath);

        try
        {
            var clock = new FixedClock(new DateTimeOffset(2026, 4, 8, 11, 0, 0, TimeSpan.Zero));
            var jsonSerializer = new DefaultJsonSerializer();
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
            var service = new CtxApplicationService(workingRepository, commitRepository, branchRepository, runRepository, packetRepository, metricsRepository, runOrchestrator, contextBuilder, commitEngine, mergeEngine, clock);

            var doctor = await service.DoctorAsync(repositoryPath, CancellationToken.None);

            Assert.True(doctor.Success);
            var report = Assert.IsType<Ctx.Domain.DoctorReport>(doctor.Data);
            Assert.False(report.RepositoryDetected);
            Assert.Contains(report.Checks, check => check.Name == "ctx-repository" && check.Status == "warning");
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
    public async Task Doctor_ReportsRepositoryStateWhenRepositoryExists()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repositoryPath);

        try
        {
            var clock = new FixedClock(new DateTimeOffset(2026, 4, 8, 12, 0, 0, TimeSpan.Zero));
            var jsonSerializer = new DefaultJsonSerializer();
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
            var service = new CtxApplicationService(workingRepository, commitRepository, branchRepository, runRepository, packetRepository, metricsRepository, runOrchestrator, contextBuilder, commitEngine, mergeEngine, clock);

            await service.InitAsync(repositoryPath, new Ctx.Application.InitRepositoryRequest("CTX", "Doctor test", "main", "tester"), CancellationToken.None);
            var doctor = await service.DoctorAsync(repositoryPath, CancellationToken.None);

            Assert.True(doctor.Success);
            var report = Assert.IsType<Ctx.Domain.DoctorReport>(doctor.Data);
            Assert.True(report.RepositoryDetected);
            Assert.Contains(report.Checks, check => check.Name == "repository-format" && check.Status == "ok");
            Assert.Contains(report.Checks, check => check.Name == "working-context" && check.Status == "ok");
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
    public async Task Audit_FindsLifecycleConsistencyIssues()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repositoryPath);

        try
        {
            var clock = new FixedClock(new DateTimeOffset(2026, 4, 8, 12, 30, 0, TimeSpan.Zero));
            var jsonSerializer = new DefaultJsonSerializer();
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
            var service = new CtxApplicationService(workingRepository, commitRepository, branchRepository, runRepository, packetRepository, metricsRepository, runOrchestrator, contextBuilder, commitEngine, mergeEngine, clock);

            await service.InitAsync(repositoryPath, new Ctx.Application.InitRepositoryRequest("CTX", "Audit test", "main", "tester"), CancellationToken.None);
            var goal = await service.AddGoalAsync(repositoryPath, new Ctx.Application.AddGoalRequest("Deliver", "Audit", 1, null, "tester"), CancellationToken.None);
            var goalId = Assert.IsType<Ctx.Domain.Goal>(goal.Data).Id.Value;
            var task = await service.AddTaskAsync(repositoryPath, new Ctx.Application.AddTaskRequest("Close lifecycle", string.Empty, goalId, Array.Empty<string>(), "tester"), CancellationToken.None);
            var taskId = Assert.IsType<Ctx.Domain.Task>(task.Data).Id.Value;
            var hypothesis = await service.AddHypothesisAsync(repositoryPath, new Ctx.Application.AddHypothesisRequest("Done work should not stay cognitively open", string.Empty, 0.9m, 0.8m, 0.7m, 0.2m, taskId, "tester"), CancellationToken.None);
            var hypothesisId = Assert.IsType<Ctx.Domain.Hypothesis>(hypothesis.Data).Id.Value;
            await service.UpdateTaskAsync(repositoryPath, new Ctx.Application.UpdateTaskRequest(taskId, null, null, "Done", "tester"), CancellationToken.None);
            await service.AddConclusionAsync(repositoryPath, new Ctx.Application.AddConclusionRequest("Still draft", "Draft", Array.Empty<string>(), Array.Empty<string>(), new[] { goalId }, new[] { taskId }, "tester"), CancellationToken.None);

            var audit = await service.AuditAsync(repositoryPath, CancellationToken.None);

            Assert.True(audit.Success);
            var report = Assert.IsType<Ctx.Domain.AuditReport>(audit.Data);
            Assert.True(report.Issues.Count >= 2);
            Assert.Contains(report.Issues, issue => issue.IssueType == "OpenHypothesisOnClosedTasks" && issue.EntityId == hypothesisId);
            Assert.Contains(report.Issues, issue => issue.IssueType == "DraftConclusionOnDoneTask");
            Assert.True(report.ConsistencyScore < 1m);
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
    public async Task Export_ReturnsFailureWhenRepositoryIsMissing()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repositoryPath);

        try
        {
            var clock = new FixedClock(new DateTimeOffset(2026, 4, 8, 13, 0, 0, TimeSpan.Zero));
            var jsonSerializer = new DefaultJsonSerializer();
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
            var service = new CtxApplicationService(workingRepository, commitRepository, branchRepository, runRepository, packetRepository, metricsRepository, runOrchestrator, contextBuilder, commitEngine, mergeEngine, clock);

            var export = await service.ExportAsync(repositoryPath, "ctx-export.json", CancellationToken.None);

            Assert.False(export.Success);
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
    public async Task Export_WritesRepositorySnapshotToJson()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repositoryPath);

        try
        {
            var clock = new FixedClock(new DateTimeOffset(2026, 4, 8, 14, 0, 0, TimeSpan.Zero));
            var jsonSerializer = new DefaultJsonSerializer();
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
            var service = new CtxApplicationService(workingRepository, commitRepository, branchRepository, runRepository, packetRepository, metricsRepository, runOrchestrator, contextBuilder, commitEngine, mergeEngine, clock);

            await service.InitAsync(repositoryPath, new Ctx.Application.InitRepositoryRequest("CTX", "Export test", "main", "tester"), CancellationToken.None);
            await service.AddGoalAsync(repositoryPath, new Ctx.Application.AddGoalRequest("Ship", "Export flow", 1, null, "tester"), CancellationToken.None);
            await service.CommitAsync(repositoryPath, new Ctx.Application.CommitRequest("seed export", "tester"), CancellationToken.None);

            var outputPath = Path.Combine(repositoryPath, "ctx-export.json");
            var export = await service.ExportAsync(repositoryPath, outputPath, CancellationToken.None);

            Assert.True(export.Success);
            Assert.True(File.Exists(outputPath));

            var json = await File.ReadAllTextAsync(outputPath);
            Assert.Contains("\"productVersion\"", json);
            Assert.Contains("\"workingContext\"", json);
            Assert.Contains("\"commits\"", json);
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
    public async Task Import_RestoresExportedRepositorySnapshot()
    {
        var sourcePath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        var targetPath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(sourcePath);
        Directory.CreateDirectory(targetPath);

        try
        {
            var clock = new FixedClock(new DateTimeOffset(2026, 4, 8, 15, 0, 0, TimeSpan.Zero));
            var jsonSerializer = new DefaultJsonSerializer();
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
            var service = new CtxApplicationService(workingRepository, commitRepository, branchRepository, runRepository, packetRepository, metricsRepository, runOrchestrator, contextBuilder, commitEngine, mergeEngine, clock);

            await service.InitAsync(sourcePath, new Ctx.Application.InitRepositoryRequest("CTX", "Import test", "main", "tester"), CancellationToken.None);
            await service.AddGoalAsync(sourcePath, new Ctx.Application.AddGoalRequest("Ship", "Import flow", 1, null, "tester"), CancellationToken.None);
            await service.CommitAsync(sourcePath, new Ctx.Application.CommitRequest("seed import", "tester"), CancellationToken.None);

            var exportPath = Path.Combine(sourcePath, "ctx-export.json");
            var export = await service.ExportAsync(sourcePath, exportPath, CancellationToken.None);
            Assert.True(export.Success);

            var import = await service.ImportAsync(targetPath, exportPath, CancellationToken.None);
            Assert.True(import.Success);

            var status = await service.StatusAsync(targetPath, CancellationToken.None);
            var log = await service.LogAsync(targetPath, CancellationToken.None);

            Assert.True(status.Success);
            Assert.True(log.Success);
            Assert.Contains("\"goals\":1", System.Text.Json.JsonSerializer.Serialize(
                status.Data,
                new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase }));
            var logJson = JsonSerializer.Serialize(
                log.Data,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            Assert.Contains("\"count\":1", logJson);
            Assert.Contains("\"commits\"", logJson);
        }
        finally
        {
            if (Directory.Exists(sourcePath))
            {
                Directory.Delete(sourcePath, recursive: true);
            }

            if (Directory.Exists(targetPath))
            {
                Directory.Delete(targetPath, recursive: true);
            }
        }
    }

    [Fact]
    public async Task LogAndDiff_ReturnStructuredSummariesForCliUsage()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repositoryPath);

        try
        {
            var clock = new FixedClock(new DateTimeOffset(2026, 4, 8, 16, 0, 0, TimeSpan.Zero));
            var jsonSerializer = new DefaultJsonSerializer();
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
            var service = new CtxApplicationService(workingRepository, commitRepository, branchRepository, runRepository, packetRepository, metricsRepository, runOrchestrator, contextBuilder, commitEngine, mergeEngine, clock);

            await service.InitAsync(repositoryPath, new Ctx.Application.InitRepositoryRequest("CTX", "Summary test", "main", "tester"), CancellationToken.None);
            await service.AddTaskAsync(repositoryPath, new Ctx.Application.AddTaskRequest("Inspect summaries", "Improve CLI readability", null, Array.Empty<string>(), "tester"), CancellationToken.None);
            await service.CommitAsync(repositoryPath, new Ctx.Application.CommitRequest("seed summary", "tester"), CancellationToken.None);
            await service.AddHypothesisAsync(repositoryPath, new Ctx.Application.AddHypothesisRequest("Summaries improve scanning", "Operators need a compact view", 0.7m, 0.6m, 0.5m, 0.4m, null, "tester"), CancellationToken.None);

            var log = await service.LogAsync(repositoryPath, CancellationToken.None);
            var diff = await service.DiffAsync(repositoryPath, null, null, CancellationToken.None);

            Assert.True(log.Success);
            Assert.True(diff.Success);

            var logJson = JsonSerializer.Serialize(
                log.Data,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var diffJson = JsonSerializer.Serialize(
                diff.Data,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            Assert.Contains("\"summary\"", logJson);
            Assert.Contains("\"commits\"", logJson);
            Assert.Contains("seed summary", logJson);

            Assert.Contains("\"summary\"", diffJson);
            Assert.Contains("\"diff\"", diffJson);
            Assert.Contains("hypotheses:1", diff.Message);
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
    public async Task GraphExport_ReturnsStructuredGraphFromWorkingContext()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repositoryPath);

        try
        {
            var clock = new FixedClock(new DateTimeOffset(2026, 4, 8, 17, 0, 0, TimeSpan.Zero));
            var jsonSerializer = new DefaultJsonSerializer();
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
            var service = new CtxApplicationService(workingRepository, commitRepository, branchRepository, runRepository, packetRepository, metricsRepository, runOrchestrator, contextBuilder, commitEngine, mergeEngine, clock);

            await service.InitAsync(repositoryPath, new Ctx.Application.InitRepositoryRequest("CTX", "Graph test", "main", "tester"), CancellationToken.None);
            var goal = await service.AddGoalAsync(repositoryPath, new Ctx.Application.AddGoalRequest("Map reasoning", "Build graph export", 1, null, "tester"), CancellationToken.None);
            var goalId = ((Ctx.Domain.Goal)goal.Data!).Id.Value;
            var task = await service.AddTaskAsync(repositoryPath, new Ctx.Application.AddTaskRequest("Export graph", "Create graph json", goalId, Array.Empty<string>(), "tester"), CancellationToken.None);
            var taskId = ((Ctx.Domain.Task)task.Data!).Id.Value;
            var hypothesis = await service.AddHypothesisAsync(repositoryPath, new Ctx.Application.AddHypothesisRequest("Graph view reveals gaps", "Relationships become visible", 0.8m, 0.7m, 0.6m, 0.3m, taskId, "tester"), CancellationToken.None);
            var hypothesisId = ((Ctx.Domain.Hypothesis)hypothesis.Data!).Id.Value;
            var evidence = await service.AddEvidenceAsync(repositoryPath, new Ctx.Application.AddEvidenceRequest("User feedback", "Visual map aids reasoning", "pilot", "Observation", 0.9m, new[] { $"hypothesis:{hypothesisId}" }, "tester"), CancellationToken.None);
            var evidenceId = ((Ctx.Domain.Evidence)evidence.Data!).Id.Value;
            await service.AddDecisionAsync(repositoryPath, new Ctx.Application.AddDecisionRequest("Build graph export", "Needed for lineage", "Accepted", new[] { hypothesisId }, new[] { evidenceId }, "tester"), CancellationToken.None);

            var export = await service.ExportGraphAsync(repositoryPath, "json", null, CancellationToken.None);

            Assert.True(export.Success);
            var graph = Assert.IsType<Ctx.Domain.CognitiveGraphExport>(export.Data);
            Assert.Contains(graph.Nodes, node => node.Type == "Goal");
            Assert.Contains(graph.Nodes, node => node.Type == "Task");
            Assert.Contains(graph.Nodes, node => node.Type == "Hypothesis");
            Assert.Contains(graph.Nodes, node => node.Type == "Evidence");
            Assert.Contains(graph.Nodes, node => node.Type == "Decision");
            Assert.Contains(graph.Edges, edge => edge.Relationship == "contains");
            Assert.Contains(graph.Edges, edge => edge.Relationship == "influences");
            Assert.Equal("main", graph.Metadata["branch"]);
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
    public async Task GraphExport_IncludesSubgoalsAndTaskDependencies()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repositoryPath);

        try
        {
            var clock = new FixedClock(new DateTimeOffset(2026, 4, 8, 17, 30, 0, TimeSpan.Zero));
            var jsonSerializer = new DefaultJsonSerializer();
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
            var service = new CtxApplicationService(workingRepository, commitRepository, branchRepository, runRepository, packetRepository, metricsRepository, runOrchestrator, contextBuilder, commitEngine, mergeEngine, clock);

            await service.InitAsync(repositoryPath, new Ctx.Application.InitRepositoryRequest("CTX", "Dependency graph test", "main", "tester"), CancellationToken.None);
            var parentGoal = await service.AddGoalAsync(repositoryPath, new Ctx.Application.AddGoalRequest("Ship V1", "Main objective", 1, null, "tester"), CancellationToken.None);
            var parentGoalId = ((Ctx.Domain.Goal)parentGoal.Data!).Id.Value;
            var childGoal = await service.AddGoalAsync(repositoryPath, new Ctx.Application.AddGoalRequest("Ship graph tooling", "Subobjective", 2, parentGoalId, "tester"), CancellationToken.None);
            var childGoalId = ((Ctx.Domain.Goal)childGoal.Data!).Id.Value;
            var baseTask = await service.AddTaskAsync(repositoryPath, new Ctx.Application.AddTaskRequest("Build graph export", "Base task", childGoalId, Array.Empty<string>(), "tester"), CancellationToken.None);
            var baseTaskId = ((Ctx.Domain.Task)baseTask.Data!).Id.Value;
            await service.AddTaskAsync(repositoryPath, new Ctx.Application.AddTaskRequest("Build graph lineage", "Dependent task", childGoalId, new[] { baseTaskId }, "tester"), CancellationToken.None);

            var export = await service.ExportGraphAsync(repositoryPath, "json", null, CancellationToken.None);

            Assert.True(export.Success);
            var graph = Assert.IsType<Ctx.Domain.CognitiveGraphExport>(export.Data);
            Assert.Contains(graph.Edges, edge => edge.Relationship == "subgoal");
            Assert.Contains(graph.Edges, edge => edge.Relationship == "depends-on");
            Assert.Contains(graph.Nodes, node => node.Id == $"Goal:{childGoalId}");
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
    public async Task CloseoutAsync_ReportsCleanWhenWorkingMatchesHead()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repositoryPath);

        try
        {
            var clock = new FixedClock(new DateTimeOffset(2026, 4, 8, 18, 0, 0, TimeSpan.Zero));
            var jsonSerializer = new DefaultJsonSerializer();
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
            var service = new CtxApplicationService(workingRepository, commitRepository, branchRepository, runRepository, packetRepository, metricsRepository, runOrchestrator, contextBuilder, commitEngine, mergeEngine, clock);

            await service.InitAsync(repositoryPath, new Ctx.Application.InitRepositoryRequest("CTX", "Closeout test", "main", "tester"), CancellationToken.None);
            await service.AddTaskAsync(repositoryPath, new Ctx.Application.AddTaskRequest("Inspect closeout", "seed", null, Array.Empty<string>(), "tester"), CancellationToken.None);
            await service.CommitAsync(repositoryPath, new Ctx.Application.CommitRequest("seed closeout", "tester"), CancellationToken.None);

            var closeout = await service.CloseoutAsync(repositoryPath, CancellationToken.None);

            Assert.True(closeout.Success);
            var data = Assert.IsType<Ctx.Application.CloseoutSummary>(closeout.Data);
            Assert.False(data.Dirty);
            Assert.False(data.HasPendingChanges);
            Assert.Equal("working matches HEAD", data.DiffSummary);
            Assert.Empty(data.PendingItems);
            Assert.Null(data.MicroCloseout);
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
    public async Task StatusAsync_ExplainsDirtyStateWithPendingPreview()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repositoryPath);

        try
        {
            var clock = new FixedClock(new DateTimeOffset(2026, 4, 8, 18, 15, 0, TimeSpan.Zero));
            var jsonSerializer = new DefaultJsonSerializer();
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
            var service = new CtxApplicationService(workingRepository, commitRepository, branchRepository, runRepository, packetRepository, metricsRepository, runOrchestrator, contextBuilder, commitEngine, mergeEngine, clock);

            await service.InitAsync(repositoryPath, new Ctx.Application.InitRepositoryRequest("CTX", "Status preview test", "main", "tester"), CancellationToken.None);
            var task = await service.AddTaskAsync(repositoryPath, new Ctx.Application.AddTaskRequest("Inspect status", "seed", null, Array.Empty<string>(), "tester"), CancellationToken.None);
            var taskId = ((Ctx.Domain.Task)task.Data!).Id.Value;
            await service.CommitAsync(repositoryPath, new Ctx.Application.CommitRequest("seed status", "tester"), CancellationToken.None);
            var hypothesis = await service.AddHypothesisAsync(repositoryPath, new Ctx.Application.AddHypothesisRequest("Dirty status should explain pending work", "status should show the trailing artifact delta", 0.7m, 0.7m, 0.5m, 0.2m, taskId, "tester"), CancellationToken.None);
            var hypothesisId = ((Ctx.Domain.Hypothesis)hypothesis.Data!).Id.Value;
            await service.AddEvidenceAsync(repositoryPath, new Ctx.Application.AddEvidenceRequest("Status preview evidence", "shows pending evidence in status", "test", "Observation", 0.8m, new[] { $"hypothesis:{hypothesisId}" }, "tester"), CancellationToken.None);

            var status = await service.StatusAsync(repositoryPath, CancellationToken.None);

            Assert.True(status.Success);
            Assert.Contains("pending cognitive changes", status.Message);
            var data = Assert.IsType<Ctx.Application.StatusSummary>(status.Data);
            Assert.True(data.Dirty);
            Assert.NotNull(data.Pending);
            Assert.True(data.Pending!.HasPendingChanges);
            Assert.Equal(3, data.Pending.PendingArtifactCount);
            Assert.Contains(data.Pending.PendingItems, item => item.EntityType == "Hypothesis");
            Assert.Contains(data.Pending.PendingItems, item => item.EntityType == "Evidence");
            Assert.Contains(data.Pending.PendingItems, item => item.EntityType == "Task");
            Assert.Contains("ctx closeout", data.Pending.NextAction, StringComparison.OrdinalIgnoreCase);
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
    public async Task OpenWorkLineAsync_CreatesSubGoalAndOptionalSeedTask()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repositoryPath);

        try
        {
            var clock = new FixedClock(new DateTimeOffset(2026, 4, 13, 15, 10, 0, TimeSpan.Zero));
            var jsonSerializer = new DefaultJsonSerializer();
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
            var service = new CtxApplicationService(workingRepository, commitRepository, branchRepository, runRepository, packetRepository, metricsRepository, runOrchestrator, contextBuilder, commitEngine, mergeEngine, clock);

            await service.InitAsync(repositoryPath, new Ctx.Application.InitRepositoryRequest("CTX", "Work-line test", "main", "tester"), CancellationToken.None);
            var parentGoal = await service.AddGoalAsync(repositoryPath, new Ctx.Application.AddGoalRequest("Improve viewer", "Strategic viewer lane", 10, null, "tester"), CancellationToken.None);
            var parentGoalId = ((Ctx.Domain.Goal)parentGoal.Data!).Id.Value;

            var opened = await service.OpenWorkLineAsync(
                repositoryPath,
                new Ctx.Application.OpenWorkLineRequest(
                    parentGoalId,
                    "Viewer working-focus UX",
                    "Tactical line under the viewer goal",
                    null,
                    "Reduce umbrella-goal noise in Working view",
                    "Seed the first task under the new line",
                    "tester"),
                CancellationToken.None);

            Assert.True(opened.Success);
            var summary = Assert.IsType<Ctx.Application.OpenWorkLineSummary>(opened.Data);
            Assert.Equal(parentGoalId, summary.ParentGoalId);
            Assert.Equal("Improve viewer", summary.ParentGoalTitle);
            Assert.Equal("Viewer working-focus UX", summary.GoalTitle);
            Assert.Equal("Reduce umbrella-goal noise in Working view", summary.SeedTaskTitle);

            var goal = Assert.IsType<Ctx.Domain.Goal>((await service.ShowArtifactAsync(repositoryPath, "goal", summary.GoalId, CancellationToken.None)).Data);
            var task = Assert.IsType<Ctx.Domain.Task>((await service.ShowArtifactAsync(repositoryPath, "task", summary.SeedTaskId!, CancellationToken.None)).Data);

            Assert.Equal(parentGoalId, goal.ParentGoalId!.Value.Value);
            Assert.Equal(goal.Id, task.GoalId);
            Assert.Equal(Ctx.Domain.TaskExecutionState.Ready, task.State);
            Assert.Contains(goal.TaskIds, item => item.Value == task.Id.Value);
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
    public async Task CloseoutAsync_EnumeratesPendingArtifactsWhenWorkingIsAheadOfHead()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repositoryPath);

        try
        {
            var clock = new FixedClock(new DateTimeOffset(2026, 4, 8, 18, 30, 0, TimeSpan.Zero));
            var jsonSerializer = new DefaultJsonSerializer();
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
            var service = new CtxApplicationService(workingRepository, commitRepository, branchRepository, runRepository, packetRepository, metricsRepository, runOrchestrator, contextBuilder, commitEngine, mergeEngine, clock);

            await service.InitAsync(repositoryPath, new Ctx.Application.InitRepositoryRequest("CTX", "Closeout test", "main", "tester"), CancellationToken.None);
            var task = await service.AddTaskAsync(repositoryPath, new Ctx.Application.AddTaskRequest("Inspect closeout", "seed", null, Array.Empty<string>(), "tester"), CancellationToken.None);
            var taskId = ((Ctx.Domain.Task)task.Data!).Id.Value;
            await service.CommitAsync(repositoryPath, new Ctx.Application.CommitRequest("seed closeout", "tester"), CancellationToken.None);
            var hypothesis = await service.AddHypothesisAsync(repositoryPath, new Ctx.Application.AddHypothesisRequest("Pending evidence should be visible", "closeout should explain trailing artifacts", 0.7m, 0.7m, 0.6m, 0.2m, taskId, "tester"), CancellationToken.None);
            var hypothesisId = ((Ctx.Domain.Hypothesis)hypothesis.Data!).Id.Value;
            await service.AddEvidenceAsync(repositoryPath, new Ctx.Application.AddEvidenceRequest("Trailing evidence", "evidence after last commit", "test", "Observation", 0.8m, new[] { $"hypothesis:{hypothesisId}" }, "tester"), CancellationToken.None);

            var closeout = await service.CloseoutAsync(repositoryPath, CancellationToken.None);

            Assert.True(closeout.Success);
            var data = Assert.IsType<Ctx.Application.CloseoutSummary>(closeout.Data);
            Assert.True(data.Dirty);
            Assert.True(data.HasPendingChanges);
            Assert.Contains(data.PendingItems, item => item.EntityType == "Evidence");
            Assert.Contains(data.Guidance, item => item.Contains("ctx commit", StringComparison.OrdinalIgnoreCase));
            Assert.NotNull(data.MicroCloseout);
            Assert.Equal("HypothesisEvidence", data.MicroCloseout!.Kind);
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
    public async Task CloseoutAsync_ClassifiesSmallEvidenceOnlyDelta()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repositoryPath);

        try
        {
            var clock = new FixedClock(new DateTimeOffset(2026, 4, 8, 18, 45, 0, TimeSpan.Zero));
            var jsonSerializer = new DefaultJsonSerializer();
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
            var service = new CtxApplicationService(workingRepository, commitRepository, branchRepository, runRepository, packetRepository, metricsRepository, runOrchestrator, contextBuilder, commitEngine, mergeEngine, clock);

            await service.InitAsync(repositoryPath, new Ctx.Application.InitRepositoryRequest("CTX", "Evidence-only closeout test", "main", "tester"), CancellationToken.None);
            var task = await service.AddTaskAsync(repositoryPath, new Ctx.Application.AddTaskRequest("Inspect closeout", "seed", null, Array.Empty<string>(), "tester"), CancellationToken.None);
            var taskId = ((Ctx.Domain.Task)task.Data!).Id.Value;
            await service.CommitAsync(repositoryPath, new Ctx.Application.CommitRequest("seed evidence-only closeout", "tester"), CancellationToken.None);
            await service.AddEvidenceAsync(repositoryPath, new Ctx.Application.AddEvidenceRequest("Trailing evidence", "evidence after last commit", "test", "Observation", 0.8m, new[] { $"task:{taskId}" }, "tester"), CancellationToken.None);

            var closeout = await service.CloseoutAsync(repositoryPath, CancellationToken.None);

            Assert.True(closeout.Success);
            var data = Assert.IsType<Ctx.Application.CloseoutSummary>(closeout.Data);
            Assert.NotNull(data.MicroCloseout);
            Assert.Equal("EvidenceOnly", data.MicroCloseout!.Kind);
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
    public async Task GraphExport_IncludesConclusionGoalAndTaskLinks()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repositoryPath);

        try
        {
            var clock = new FixedClock(new DateTimeOffset(2026, 4, 8, 17, 45, 0, TimeSpan.Zero));
            var jsonSerializer = new DefaultJsonSerializer();
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
            var service = new CtxApplicationService(workingRepository, commitRepository, branchRepository, runRepository, packetRepository, metricsRepository, runOrchestrator, contextBuilder, commitEngine, mergeEngine, clock);

            await service.InitAsync(repositoryPath, new Ctx.Application.InitRepositoryRequest("CTX", "Conclusion link graph test", "main", "tester"), CancellationToken.None);
            var goal = await service.AddGoalAsync(repositoryPath, new Ctx.Application.AddGoalRequest("Ship graph", "Main objective", 1, null, "tester"), CancellationToken.None);
            var goalId = ((Ctx.Domain.Goal)goal.Data!).Id.Value;
            var task = await service.AddTaskAsync(repositoryPath, new Ctx.Application.AddTaskRequest("Wire conclusions", "Attach outcomes", goalId, Array.Empty<string>(), "tester"), CancellationToken.None);
            var taskId = ((Ctx.Domain.Task)task.Data!).Id.Value;
            var hypothesis = await service.AddHypothesisAsync(repositoryPath, new Ctx.Application.AddHypothesisRequest("Explicit closure improves traceability", "Need direct links", 0.8m, 0.7m, 0.6m, 0.3m, taskId, "tester"), CancellationToken.None);
            var hypothesisId = ((Ctx.Domain.Hypothesis)hypothesis.Data!).Id.Value;
            var evidence = await service.AddEvidenceAsync(repositoryPath, new Ctx.Application.AddEvidenceRequest("Pilot feedback", "Confirms closure", "pilot", "Observation", 0.8m, new[] { $"hypothesis:{hypothesisId}" }, "tester"), CancellationToken.None);
            var evidenceId = ((Ctx.Domain.Evidence)evidence.Data!).Id.Value;
            var decision = await service.AddDecisionAsync(repositoryPath, new Ctx.Application.AddDecisionRequest("Link conclusions explicitly", "Makes outputs traceable", "Accepted", new[] { hypothesisId }, new[] { evidenceId }, "tester"), CancellationToken.None);
            var decisionId = ((Ctx.Domain.Decision)decision.Data!).Id.Value;
            await service.AddConclusionAsync(repositoryPath, new Ctx.Application.AddConclusionRequest("Outcome mapped to both work and objective", "Accepted", new[] { decisionId }, new[] { evidenceId }, new[] { goalId }, new[] { taskId }, "tester"), CancellationToken.None);

            var export = await service.ExportGraphAsync(repositoryPath, "json", null, CancellationToken.None);

            Assert.True(export.Success);
            var graph = Assert.IsType<Ctx.Domain.CognitiveGraphExport>(export.Data);
            Assert.Contains(graph.Edges, edge => edge.From == $"Goal:{goalId}" && edge.Relationship == "resolved-by");
            Assert.Contains(graph.Edges, edge => edge.From == $"Task:{taskId}" && edge.Relationship == "resolved-by");
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
    public async Task GraphExport_Mermaid_ReturnsDiagramText()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repositoryPath);

        try
        {
            var clock = new FixedClock(new DateTimeOffset(2026, 4, 8, 18, 0, 0, TimeSpan.Zero));
            var jsonSerializer = new DefaultJsonSerializer();
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
            var service = new CtxApplicationService(workingRepository, commitRepository, branchRepository, runRepository, packetRepository, metricsRepository, runOrchestrator, contextBuilder, commitEngine, mergeEngine, clock);

            await service.InitAsync(repositoryPath, new Ctx.Application.InitRepositoryRequest("CTX", "Mermaid graph test", "main", "tester"), CancellationToken.None);
            var goal = await service.AddGoalAsync(repositoryPath, new Ctx.Application.AddGoalRequest("Map reasoning", "Build graph export", 1, null, "tester"), CancellationToken.None);
            var goalId = ((Ctx.Domain.Goal)goal.Data!).Id.Value;
            await service.AddTaskAsync(repositoryPath, new Ctx.Application.AddTaskRequest("Export graph", "Create graph mermaid", goalId, Array.Empty<string>(), "tester"), CancellationToken.None);

            var export = await service.ExportGraphAsync(repositoryPath, "mermaid", null, CancellationToken.None);

            Assert.True(export.Success);
            var exportJson = JsonSerializer.Serialize(
                export.Data,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            Assert.Contains("graph TD", exportJson);
            Assert.Contains("contains", exportJson);
            Assert.Contains("Goal: Map reasoning", exportJson);
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
    public async Task GraphLineage_Hypothesis_ReturnsFocusedSubgraph()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repositoryPath);

        try
        {
            var clock = new FixedClock(new DateTimeOffset(2026, 4, 8, 19, 0, 0, TimeSpan.Zero));
            var jsonSerializer = new DefaultJsonSerializer();
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
            var service = new CtxApplicationService(workingRepository, commitRepository, branchRepository, runRepository, packetRepository, metricsRepository, runOrchestrator, contextBuilder, commitEngine, mergeEngine, clock);

            await service.InitAsync(repositoryPath, new Ctx.Application.InitRepositoryRequest("CTX", "Lineage test", "main", "tester"), CancellationToken.None);
            var goal = await service.AddGoalAsync(repositoryPath, new Ctx.Application.AddGoalRequest("Map reasoning", "Focus lineage", 1, null, "tester"), CancellationToken.None);
            var goalId = ((Ctx.Domain.Goal)goal.Data!).Id.Value;
            var task = await service.AddTaskAsync(repositoryPath, new Ctx.Application.AddTaskRequest("Trace hypothesis", "Build lineage", goalId, Array.Empty<string>(), "tester"), CancellationToken.None);
            var taskId = ((Ctx.Domain.Task)task.Data!).Id.Value;
            var hypothesis = await service.AddHypothesisAsync(repositoryPath, new Ctx.Application.AddHypothesisRequest("Lineage clarifies reasoning", "Need focused graph", 0.9m, 0.8m, 0.7m, 0.2m, taskId, "tester"), CancellationToken.None);
            var hypothesisId = ((Ctx.Domain.Hypothesis)hypothesis.Data!).Id.Value;
            var evidence = await service.AddEvidenceAsync(repositoryPath, new Ctx.Application.AddEvidenceRequest("Feedback", "Helps explain flow", "pilot", "Observation", 0.8m, new[] { $"hypothesis:{hypothesisId}" }, "tester"), CancellationToken.None);
            var evidenceId = ((Ctx.Domain.Evidence)evidence.Data!).Id.Value;
            var decision = await service.AddDecisionAsync(repositoryPath, new Ctx.Application.AddDecisionRequest("Ship lineage", "Improves graph utility", "Accepted", new[] { hypothesisId }, new[] { evidenceId }, "tester"), CancellationToken.None);
            var decisionId = ((Ctx.Domain.Decision)decision.Data!).Id.Value;
            await service.AddConclusionAsync(repositoryPath, new Ctx.Application.AddConclusionRequest("Lineage should be visible", "Accepted", new[] { decisionId }, new[] { evidenceId }, Array.Empty<string>(), Array.Empty<string>(), "tester"), CancellationToken.None);

            var lineage = await service.GraphLineageAsync(repositoryPath, "hypothesis", hypothesisId, "json", null, CancellationToken.None);

            Assert.True(lineage.Success);
            var result = Assert.IsType<Ctx.Domain.CognitiveGraphLineage>(lineage.Data);
            Assert.Equal("Hypothesis", result.FocusType);
            Assert.Contains(result.Graph.Nodes, node => node.Type == "Hypothesis");
            Assert.Contains(result.Graph.Nodes, node => node.Type == "Task");
            Assert.Contains(result.Graph.Nodes, node => node.Type == "Evidence");
            Assert.Contains(result.Graph.Nodes, node => node.Type == "Decision");
            Assert.Contains(result.Graph.Nodes, node => node.Type == "Conclusion");
            Assert.Contains(result.Graph.Edges, edge => edge.Relationship == "influences");
            Assert.Contains(result.Graph.Metadata, pair => pair.Key == "focusId" && pair.Value == hypothesisId);
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
    public async Task GraphLineage_Goal_ReturnsFocusedSubgraph()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repositoryPath);

        try
        {
            var clock = new FixedClock(new DateTimeOffset(2026, 4, 8, 18, 45, 0, TimeSpan.Zero));
            var jsonSerializer = new DefaultJsonSerializer();
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
            var service = new CtxApplicationService(workingRepository, commitRepository, branchRepository, runRepository, packetRepository, metricsRepository, runOrchestrator, contextBuilder, commitEngine, mergeEngine, clock);

            await service.InitAsync(repositoryPath, new Ctx.Application.InitRepositoryRequest("CTX", "Goal lineage test", "main", "tester"), CancellationToken.None);
            var goal = await service.AddGoalAsync(repositoryPath, new Ctx.Application.AddGoalRequest("Deliver graph lineage", "Drive the graph work", 1, null, "tester"), CancellationToken.None);
            var goalId = ((Ctx.Domain.Goal)goal.Data!).Id.Value;
            var task = await service.AddTaskAsync(repositoryPath, new Ctx.Application.AddTaskRequest("Trace goal lineage", "Follow goal descendants", goalId, Array.Empty<string>(), "tester"), CancellationToken.None);
            var taskId = ((Ctx.Domain.Task)task.Data!).Id.Value;
            var hypothesis = await service.AddHypothesisAsync(repositoryPath, new Ctx.Application.AddHypothesisRequest("Goal lineage should expose downstream decisions", "Need top-down inspection", 0.8m, 0.7m, 0.6m, 0.3m, taskId, "tester"), CancellationToken.None);
            var hypothesisId = ((Ctx.Domain.Hypothesis)hypothesis.Data!).Id.Value;
            var evidence = await service.AddEvidenceAsync(repositoryPath, new Ctx.Application.AddEvidenceRequest("Goal evidence", "Supports the goal path", "pilot", "Observation", 0.75m, new[] { $"hypothesis:{hypothesisId}" }, "tester"), CancellationToken.None);
            var evidenceId = ((Ctx.Domain.Evidence)evidence.Data!).Id.Value;
            var decision = await service.AddDecisionAsync(repositoryPath, new Ctx.Application.AddDecisionRequest("Ship goal lineage", "Improves strategic navigation", "Accepted", new[] { hypothesisId }, new[] { evidenceId }, "tester"), CancellationToken.None);
            var decisionId = ((Ctx.Domain.Decision)decision.Data!).Id.Value;
            await service.AddConclusionAsync(repositoryPath, new Ctx.Application.AddConclusionRequest("Goal lineage closes top-down navigation", "Accepted", new[] { decisionId }, new[] { evidenceId }, Array.Empty<string>(), Array.Empty<string>(), "tester"), CancellationToken.None);

            var lineage = await service.GraphLineageAsync(repositoryPath, "goal", goalId, "json", null, CancellationToken.None);

            Assert.True(lineage.Success);
            var result = Assert.IsType<Ctx.Domain.CognitiveGraphLineage>(lineage.Data);
            Assert.Equal("Goal", result.FocusType);
            Assert.Contains(result.Graph.Nodes, node => node.Type == "Project");
            Assert.Contains(result.Graph.Nodes, node => node.Type == "Goal" && node.Id.EndsWith(goalId, StringComparison.Ordinal));
            Assert.Contains(result.Graph.Nodes, node => node.Type == "Task");
            Assert.Contains(result.Graph.Nodes, node => node.Type == "Hypothesis");
            Assert.Contains(result.Graph.Nodes, node => node.Type == "Decision");
            Assert.Contains(result.Graph.Nodes, node => node.Type == "Conclusion");
            Assert.Contains(result.Graph.Edges, edge => edge.Relationship == "contains");
            Assert.Contains(result.Graph.Metadata, pair => pair.Key == "focusId" && pair.Value == goalId);
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
    public async Task GraphLineage_Conclusion_ReturnsFocusedSubgraph()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repositoryPath);

        try
        {
            var clock = new FixedClock(new DateTimeOffset(2026, 4, 8, 19, 10, 0, TimeSpan.Zero));
            var jsonSerializer = new DefaultJsonSerializer();
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
            var service = new CtxApplicationService(workingRepository, commitRepository, branchRepository, runRepository, packetRepository, metricsRepository, runOrchestrator, contextBuilder, commitEngine, mergeEngine, clock);

            await service.InitAsync(repositoryPath, new Ctx.Application.InitRepositoryRequest("CTX", "Conclusion lineage test", "main", "tester"), CancellationToken.None);
            var goal = await service.AddGoalAsync(repositoryPath, new Ctx.Application.AddGoalRequest("Close reasoning loops", "Trace conclusions backwards", 1, null, "tester"), CancellationToken.None);
            var goalId = ((Ctx.Domain.Goal)goal.Data!).Id.Value;
            var task = await service.AddTaskAsync(repositoryPath, new Ctx.Application.AddTaskRequest("Trace conclusion lineage", "Follow conclusion ancestry", goalId, Array.Empty<string>(), "tester"), CancellationToken.None);
            var taskId = ((Ctx.Domain.Task)task.Data!).Id.Value;
            var hypothesis = await service.AddHypothesisAsync(repositoryPath, new Ctx.Application.AddHypothesisRequest("Conclusion lineage reveals upstream causes", "Need reverse inspection", 0.82m, 0.72m, 0.62m, 0.28m, taskId, "tester"), CancellationToken.None);
            var hypothesisId = ((Ctx.Domain.Hypothesis)hypothesis.Data!).Id.Value;
            var evidence = await service.AddEvidenceAsync(repositoryPath, new Ctx.Application.AddEvidenceRequest("Conclusion evidence", "Supports the conclusion path", "pilot", "Observation", 0.8m, new[] { $"hypothesis:{hypothesisId}" }, "tester"), CancellationToken.None);
            var evidenceId = ((Ctx.Domain.Evidence)evidence.Data!).Id.Value;
            var decision = await service.AddDecisionAsync(repositoryPath, new Ctx.Application.AddDecisionRequest("Expose conclusion lineage", "Improves reverse navigation", "Accepted", new[] { hypothesisId }, new[] { evidenceId }, "tester"), CancellationToken.None);
            var decisionId = ((Ctx.Domain.Decision)decision.Data!).Id.Value;
            var conclusion = await service.AddConclusionAsync(repositoryPath, new Ctx.Application.AddConclusionRequest("Conclusion lineage should be explorable", "Accepted", new[] { decisionId }, new[] { evidenceId }, Array.Empty<string>(), Array.Empty<string>(), "tester"), CancellationToken.None);
            var conclusionId = ((Ctx.Domain.Conclusion)conclusion.Data!).Id.Value;

            var lineage = await service.GraphLineageAsync(repositoryPath, "conclusion", conclusionId, "json", null, CancellationToken.None);

            Assert.True(lineage.Success);
            var result = Assert.IsType<Ctx.Domain.CognitiveGraphLineage>(lineage.Data);
            Assert.Equal("Conclusion", result.FocusType);
            Assert.Contains(result.Graph.Nodes, node => node.Type == "Conclusion" && node.Id.EndsWith(conclusionId, StringComparison.Ordinal));
            Assert.Contains(result.Graph.Nodes, node => node.Type == "Decision");
            Assert.Contains(result.Graph.Nodes, node => node.Type == "Evidence");
            Assert.Contains(result.Graph.Nodes, node => node.Type == "Hypothesis");
            Assert.Contains(result.Graph.Nodes, node => node.Type == "Task");
            Assert.Contains(result.Graph.Nodes, node => node.Type == "Goal");
            Assert.Contains(result.Graph.Edges, edge => edge.Relationship == "leads-to");
            Assert.Contains(result.Graph.Metadata, pair => pair.Key == "focusId" && pair.Value == conclusionId);
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
    public async Task GraphLineage_Decision_ReturnsFocusedSubgraph()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repositoryPath);

        try
        {
            var clock = new FixedClock(new DateTimeOffset(2026, 4, 8, 19, 30, 0, TimeSpan.Zero));
            var jsonSerializer = new DefaultJsonSerializer();
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
            var service = new CtxApplicationService(workingRepository, commitRepository, branchRepository, runRepository, packetRepository, metricsRepository, runOrchestrator, contextBuilder, commitEngine, mergeEngine, clock);

            await service.InitAsync(repositoryPath, new Ctx.Application.InitRepositoryRequest("CTX", "Decision lineage test", "main", "tester"), CancellationToken.None);
            var goal = await service.AddGoalAsync(repositoryPath, new Ctx.Application.AddGoalRequest("Map decisions", "Focus decision lineage", 1, null, "tester"), CancellationToken.None);
            var goalId = ((Ctx.Domain.Goal)goal.Data!).Id.Value;
            var task = await service.AddTaskAsync(repositoryPath, new Ctx.Application.AddTaskRequest("Trace decision", "Build decision lineage", goalId, Array.Empty<string>(), "tester"), CancellationToken.None);
            var taskId = ((Ctx.Domain.Task)task.Data!).Id.Value;
            var hypothesis = await service.AddHypothesisAsync(repositoryPath, new Ctx.Application.AddHypothesisRequest("Decision lineage clarifies outcomes", "Need a decision-focused graph", 0.85m, 0.75m, 0.65m, 0.25m, taskId, "tester"), CancellationToken.None);
            var hypothesisId = ((Ctx.Domain.Hypothesis)hypothesis.Data!).Id.Value;
            var evidence = await service.AddEvidenceAsync(repositoryPath, new Ctx.Application.AddEvidenceRequest("Decision evidence", "Supports the chosen path", "pilot", "Observation", 0.85m, new[] { $"hypothesis:{hypothesisId}" }, "tester"), CancellationToken.None);
            var evidenceId = ((Ctx.Domain.Evidence)evidence.Data!).Id.Value;
            var decision = await service.AddDecisionAsync(repositoryPath, new Ctx.Application.AddDecisionRequest("Adopt lineage views", "Improves cognitive traceability", "Accepted", new[] { hypothesisId }, new[] { evidenceId }, "tester"), CancellationToken.None);
            var decisionId = ((Ctx.Domain.Decision)decision.Data!).Id.Value;
            await service.AddConclusionAsync(repositoryPath, new Ctx.Application.AddConclusionRequest("Decision lineage should be exposed", "Accepted", new[] { decisionId }, new[] { evidenceId }, Array.Empty<string>(), Array.Empty<string>(), "tester"), CancellationToken.None);

            var lineage = await service.GraphLineageAsync(repositoryPath, "decision", decisionId, "json", null, CancellationToken.None);

            Assert.True(lineage.Success);
            var result = Assert.IsType<Ctx.Domain.CognitiveGraphLineage>(lineage.Data);
            Assert.Equal("Decision", result.FocusType);
            Assert.Contains(result.Graph.Nodes, node => node.Type == "Decision" && node.Id.EndsWith(decisionId, StringComparison.Ordinal));
            Assert.Contains(result.Graph.Nodes, node => node.Type == "Hypothesis");
            Assert.Contains(result.Graph.Nodes, node => node.Type == "Task");
            Assert.Contains(result.Graph.Nodes, node => node.Type == "Evidence");
            Assert.Contains(result.Graph.Nodes, node => node.Type == "Conclusion");
            Assert.Contains(result.Graph.Edges, edge => edge.Relationship == "influences");
            Assert.Contains(result.Graph.Edges, edge => edge.Relationship == "leads-to");
            Assert.Contains(result.Graph.Metadata, pair => pair.Key == "focusId" && pair.Value == decisionId);
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
    public async Task GraphLineage_Task_ReturnsFocusedSubgraph()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repositoryPath);

        try
        {
            var clock = new FixedClock(new DateTimeOffset(2026, 4, 8, 19, 45, 0, TimeSpan.Zero));
            var jsonSerializer = new DefaultJsonSerializer();
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
            var service = new CtxApplicationService(workingRepository, commitRepository, branchRepository, runRepository, packetRepository, metricsRepository, runOrchestrator, contextBuilder, commitEngine, mergeEngine, clock);

            await service.InitAsync(repositoryPath, new Ctx.Application.InitRepositoryRequest("CTX", "Task lineage test", "main", "tester"), CancellationToken.None);
            var goal = await service.AddGoalAsync(repositoryPath, new Ctx.Application.AddGoalRequest("Ship graph lineage", "Trace work", 1, null, "tester"), CancellationToken.None);
            var goalId = ((Ctx.Domain.Goal)goal.Data!).Id.Value;
            var task = await service.AddTaskAsync(repositoryPath, new Ctx.Application.AddTaskRequest("Trace task lineage", "Build task lineage", goalId, Array.Empty<string>(), "tester"), CancellationToken.None);
            var taskId = ((Ctx.Domain.Task)task.Data!).Id.Value;
            var hypothesis = await service.AddHypothesisAsync(repositoryPath, new Ctx.Application.AddHypothesisRequest("Task lineage should expose downstream reasoning", "Need task-focused inspection", 0.8m, 0.7m, 0.6m, 0.3m, taskId, "tester"), CancellationToken.None);
            var hypothesisId = ((Ctx.Domain.Hypothesis)hypothesis.Data!).Id.Value;
            var evidence = await service.AddEvidenceAsync(repositoryPath, new Ctx.Application.AddEvidenceRequest("Task evidence", "Supports the task path", "pilot", "Observation", 0.75m, new[] { $"hypothesis:{hypothesisId}" }, "tester"), CancellationToken.None);
            var evidenceId = ((Ctx.Domain.Evidence)evidence.Data!).Id.Value;
            var decision = await service.AddDecisionAsync(repositoryPath, new Ctx.Application.AddDecisionRequest("Implement task lineage", "Improves operational tracing", "Accepted", new[] { hypothesisId }, new[] { evidenceId }, "tester"), CancellationToken.None);
            var decisionId = ((Ctx.Domain.Decision)decision.Data!).Id.Value;
            await service.AddConclusionAsync(repositoryPath, new Ctx.Application.AddConclusionRequest("Task lineage adds practical visibility", "Accepted", new[] { decisionId }, new[] { evidenceId }, Array.Empty<string>(), Array.Empty<string>(), "tester"), CancellationToken.None);

            var lineage = await service.GraphLineageAsync(repositoryPath, "task", taskId, "json", null, CancellationToken.None);

            Assert.True(lineage.Success);
            var result = Assert.IsType<Ctx.Domain.CognitiveGraphLineage>(lineage.Data);
            Assert.Equal("Task", result.FocusType);
            Assert.Contains(result.Graph.Nodes, node => node.Type == "Goal");
            Assert.Contains(result.Graph.Nodes, node => node.Type == "Task" && node.Id.EndsWith(taskId, StringComparison.Ordinal));
            Assert.Contains(result.Graph.Nodes, node => node.Type == "Hypothesis");
            Assert.Contains(result.Graph.Nodes, node => node.Type == "Evidence");
            Assert.Contains(result.Graph.Nodes, node => node.Type == "Decision");
            Assert.Contains(result.Graph.Nodes, node => node.Type == "Conclusion");
            Assert.Contains(result.Graph.Edges, edge => edge.Relationship == "contains");
            Assert.Contains(result.Graph.Edges, edge => edge.Relationship == "informs");
            Assert.Contains(result.Graph.Metadata, pair => pair.Key == "focusId" && pair.Value == taskId);
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
    public async Task GraphLineage_Mermaid_ReturnsDiagramText()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repositoryPath);

        try
        {
            var clock = new FixedClock(new DateTimeOffset(2026, 4, 8, 19, 55, 0, TimeSpan.Zero));
            var jsonSerializer = new DefaultJsonSerializer();
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
            var service = new CtxApplicationService(workingRepository, commitRepository, branchRepository, runRepository, packetRepository, metricsRepository, runOrchestrator, contextBuilder, commitEngine, mergeEngine, clock);

            await service.InitAsync(repositoryPath, new Ctx.Application.InitRepositoryRequest("CTX", "Mermaid lineage test", "main", "tester"), CancellationToken.None);
            var goal = await service.AddGoalAsync(repositoryPath, new Ctx.Application.AddGoalRequest("Render lineage", "Export partial graph", 1, null, "tester"), CancellationToken.None);
            var goalId = ((Ctx.Domain.Goal)goal.Data!).Id.Value;
            var task = await service.AddTaskAsync(repositoryPath, new Ctx.Application.AddTaskRequest("Trace mermaid lineage", "Generate focused diagram", goalId, Array.Empty<string>(), "tester"), CancellationToken.None);
            var taskId = ((Ctx.Domain.Task)task.Data!).Id.Value;

            var lineage = await service.GraphLineageAsync(repositoryPath, "task", taskId, "mermaid", null, CancellationToken.None);

            Assert.True(lineage.Success);
            var lineageJson = JsonSerializer.Serialize(
                lineage.Data,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            Assert.Contains("graph TD", lineageJson);
            Assert.Contains("\"format\":\"mermaid\"", lineageJson);
            Assert.Contains("Task: Trace mermaid lineage", lineageJson);
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
    public async Task GraphLineage_Output_WritesRequestedFile()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repositoryPath);

        try
        {
            var clock = new FixedClock(new DateTimeOffset(2026, 4, 8, 20, 5, 0, TimeSpan.Zero));
            var jsonSerializer = new DefaultJsonSerializer();
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
            var service = new CtxApplicationService(workingRepository, commitRepository, branchRepository, runRepository, packetRepository, metricsRepository, runOrchestrator, contextBuilder, commitEngine, mergeEngine, clock);

            await service.InitAsync(repositoryPath, new Ctx.Application.InitRepositoryRequest("CTX", "Lineage output test", "main", "tester"), CancellationToken.None);
            var task = await service.AddTaskAsync(repositoryPath, new Ctx.Application.AddTaskRequest("Persist lineage", "Write output file", null, Array.Empty<string>(), "tester"), CancellationToken.None);
            var taskId = ((Ctx.Domain.Task)task.Data!).Id.Value;
            var outputPath = Path.Combine(repositoryPath, "tmp", "task-lineage.mmd");

            var lineage = await service.GraphLineageAsync(repositoryPath, "task", taskId, "mermaid", outputPath, CancellationToken.None);

            Assert.True(lineage.Success);
            Assert.True(File.Exists(outputPath));
            var content = await File.ReadAllTextAsync(outputPath);
            Assert.Contains("graph TD", content);
            var lineageJson = JsonSerializer.Serialize(
                lineage.Data,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            Assert.Contains("task-lineage.mmd", lineageJson);
            Assert.Contains("\"outputPath\"", lineageJson);
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
    public async Task GraphSummary_ReturnsCountsAndLineageFocuses()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repositoryPath);

        try
        {
            var clock = new FixedClock(new DateTimeOffset(2026, 4, 8, 20, 10, 0, TimeSpan.Zero));
            var jsonSerializer = new DefaultJsonSerializer();
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
            var service = new CtxApplicationService(workingRepository, commitRepository, branchRepository, runRepository, packetRepository, metricsRepository, runOrchestrator, contextBuilder, commitEngine, mergeEngine, clock);

            await service.InitAsync(repositoryPath, new Ctx.Application.InitRepositoryRequest("CTX", "Graph summary test", "main", "tester"), CancellationToken.None);
            var goal = await service.AddGoalAsync(repositoryPath, new Ctx.Application.AddGoalRequest("Summarize graph", "Inspect graph quickly", 1, null, "tester"), CancellationToken.None);
            var goalId = ((Ctx.Domain.Goal)goal.Data!).Id.Value;
            var task = await service.AddTaskAsync(repositoryPath, new Ctx.Application.AddTaskRequest("Review graph summary", "Check graph counts", goalId, Array.Empty<string>(), "tester"), CancellationToken.None);
            var taskId = ((Ctx.Domain.Task)task.Data!).Id.Value;
            var hypothesis = await service.AddHypothesisAsync(repositoryPath, new Ctx.Application.AddHypothesisRequest("Graph summary accelerates inspection", "Need a quick view", 0.7m, 0.6m, 0.5m, 0.4m, taskId, "tester"), CancellationToken.None);
            var hypothesisId = ((Ctx.Domain.Hypothesis)hypothesis.Data!).Id.Value;
            var evidence = await service.AddEvidenceAsync(repositoryPath, new Ctx.Application.AddEvidenceRequest("Pilot signal", "Supports graph summary", "pilot", "Observation", 0.8m, new[] { $"hypothesis:{hypothesisId}" }, "tester"), CancellationToken.None);
            var evidenceId = ((Ctx.Domain.Evidence)evidence.Data!).Id.Value;
            var decision = await service.AddDecisionAsync(repositoryPath, new Ctx.Application.AddDecisionRequest("Ship graph summary", "Improves CLI inspection", "Accepted", new[] { hypothesisId }, new[] { evidenceId }, "tester"), CancellationToken.None);
            var decisionId = ((Ctx.Domain.Decision)decision.Data!).Id.Value;
            var conclusion = await service.AddConclusionAsync(repositoryPath, new Ctx.Application.AddConclusionRequest("Graph summary should be available", "Accepted", new[] { decisionId }, new[] { evidenceId }, Array.Empty<string>(), Array.Empty<string>(), "tester"), CancellationToken.None);
            var conclusionId = ((Ctx.Domain.Conclusion)conclusion.Data!).Id.Value;

            var summary = await service.GraphSummaryAsync(repositoryPath, CancellationToken.None);

            Assert.True(summary.Success);
            var summaryJson = JsonSerializer.Serialize(
                summary.Data,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            Assert.Contains("\"nodes\":7", summaryJson);
            Assert.Contains("\"edges\":8", summaryJson);
            Assert.Contains(goalId, summaryJson);
            Assert.Contains(taskId, summaryJson);
            Assert.Contains(hypothesisId, summaryJson);
            Assert.Contains(decisionId, summaryJson);
            Assert.Contains(conclusionId, summaryJson);
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
    public async Task GraphShow_ReturnsNodeAndImmediateConnections()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repositoryPath);

        try
        {
            var clock = new FixedClock(new DateTimeOffset(2026, 4, 8, 20, 15, 0, TimeSpan.Zero));
            var jsonSerializer = new DefaultJsonSerializer();
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
            var service = new CtxApplicationService(workingRepository, commitRepository, branchRepository, runRepository, packetRepository, metricsRepository, runOrchestrator, contextBuilder, commitEngine, mergeEngine, clock);

            await service.InitAsync(repositoryPath, new Ctx.Application.InitRepositoryRequest("CTX", "Graph show test", "main", "tester"), CancellationToken.None);
            var goal = await service.AddGoalAsync(repositoryPath, new Ctx.Application.AddGoalRequest("Inspect nodes", "Need a focused node view", 1, null, "tester"), CancellationToken.None);
            var goalId = ((Ctx.Domain.Goal)goal.Data!).Id.Value;
            var task = await service.AddTaskAsync(repositoryPath, new Ctx.Application.AddTaskRequest("Show graph node", "Inspect direct edges", goalId, Array.Empty<string>(), "tester"), CancellationToken.None);
            var taskId = ((Ctx.Domain.Task)task.Data!).Id.Value;
            var hypothesis = await service.AddHypothesisAsync(repositoryPath, new Ctx.Application.AddHypothesisRequest("Node inspection speeds debugging", "Need node-level graph view", 0.75m, 0.65m, 0.55m, 0.35m, taskId, "tester"), CancellationToken.None);
            var hypothesisId = ((Ctx.Domain.Hypothesis)hypothesis.Data!).Id.Value;

            var show = await service.GraphShowAsync(repositoryPath, hypothesisId, CancellationToken.None);

            Assert.True(show.Success);
            var showJson = JsonSerializer.Serialize(
                show.Data,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            Assert.Contains($"\"id\":\"Hypothesis:{hypothesisId}\"", showJson);
            Assert.Contains("\"incoming\"", showJson);
            Assert.Contains("\"outgoing\"", showJson);
            Assert.Contains("\"connectedNodes\"", showJson);
            Assert.Contains(taskId, showJson);
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
    public async Task GraphExport_Commit_ReturnsHistoricalSnapshotGraph()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repositoryPath);

        try
        {
            var clock = new FixedClock(new DateTimeOffset(2026, 4, 8, 20, 0, 0, TimeSpan.Zero));
            var jsonSerializer = new DefaultJsonSerializer();
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
            var service = new CtxApplicationService(workingRepository, commitRepository, branchRepository, runRepository, packetRepository, metricsRepository, runOrchestrator, contextBuilder, commitEngine, mergeEngine, clock);

            await service.InitAsync(repositoryPath, new Ctx.Application.InitRepositoryRequest("CTX", "Historical graph test", "main", "tester"), CancellationToken.None);
            await service.AddGoalAsync(repositoryPath, new Ctx.Application.AddGoalRequest("Frozen goal", "Present in commit", 1, null, "tester"), CancellationToken.None);
            var commit = await service.CommitAsync(repositoryPath, new Ctx.Application.CommitRequest("seed graph history", "tester"), CancellationToken.None);
            var commitId = ((Ctx.Domain.ContextCommit)commit.Data!).Id.Value;
            await service.AddTaskAsync(repositoryPath, new Ctx.Application.AddTaskRequest("Later task", "Only in working context", null, Array.Empty<string>(), "tester"), CancellationToken.None);

            var export = await service.ExportGraphAsync(repositoryPath, "json", commitId, CancellationToken.None);

            Assert.True(export.Success);
            var graph = Assert.IsType<Ctx.Domain.CognitiveGraphExport>(export.Data);
            Assert.Contains(graph.Nodes, node => node.Type == "Goal");
            Assert.DoesNotContain(graph.Nodes, node => node.Type == "Task" && node.Label == "Later task");
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
    public async Task Branch_WithSlashName_CanBeCreatedAndCheckedOut()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repositoryPath);

        try
        {
            var clock = new FixedClock(new DateTimeOffset(2026, 4, 8, 21, 15, 0, TimeSpan.Zero));
            var jsonSerializer = new DefaultJsonSerializer();
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
            var service = new CtxApplicationService(workingRepository, commitRepository, branchRepository, runRepository, packetRepository, metricsRepository, runOrchestrator, contextBuilder, commitEngine, mergeEngine, clock);

            await service.InitAsync(repositoryPath, new Ctx.Application.InitRepositoryRequest("CTX", "Branch slash test", "main", "tester"), CancellationToken.None);

            var branch = await service.BranchAsync(repositoryPath, "feature/ux-timeline", CancellationToken.None);
            var checkout = await service.CheckoutAsync(repositoryPath, "feature/ux-timeline", CancellationToken.None);
            var listed = await branchRepository.ListAsync(repositoryPath, CancellationToken.None);
            var branchFile = Path.Combine(repositoryPath, ".ctx", "branches", "feature_ux-timeline.json");

            Assert.True(branch.Success);
            Assert.True(checkout.Success);
            Assert.Contains(listed, item => item.Name == "feature/ux-timeline");
            Assert.True(File.Exists(branchFile));
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
    public async Task AddHypothesis_PersistsScoringDimensionsAndComputedScore()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repositoryPath);

        try
        {
            var clock = new FixedClock(new DateTimeOffset(2026, 4, 8, 21, 30, 0, TimeSpan.Zero));
            var jsonSerializer = new DefaultJsonSerializer();
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
            var service = new CtxApplicationService(workingRepository, commitRepository, branchRepository, runRepository, packetRepository, metricsRepository, runOrchestrator, contextBuilder, commitEngine, mergeEngine, clock);

            await service.InitAsync(repositoryPath, new Ctx.Application.InitRepositoryRequest("CTX", "Scoring test", "main", "tester"), CancellationToken.None);
            var task = await service.AddTaskAsync(repositoryPath, new Ctx.Application.AddTaskRequest("Score hypothesis", "Model dimensions", null, Array.Empty<string>(), "tester"), CancellationToken.None);
            var taskId = ((Ctx.Domain.Task)task.Data!).Id.Value;

            var hypothesisResult = await service.AddHypothesisAsync(
                repositoryPath,
                new Ctx.Application.AddHypothesisRequest("Score should persist", "Need explicit prioritization", 0.8m, 0.9m, 0.5m, 0.2m, taskId, "tester"),
                CancellationToken.None);

            Assert.True(hypothesisResult.Success);
            var hypothesis = Assert.IsType<Ctx.Domain.Hypothesis>(hypothesisResult.Data);
            Assert.Equal(0.8m, hypothesis.Probability);
            Assert.Equal(0.9m, hypothesis.Impact);
            Assert.Equal(0.5m, hypothesis.EvidenceStrength);
            Assert.Equal(0.2m, hypothesis.CostToValidate);
            Assert.Equal(0.675m, hypothesis.Score);
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
    public async Task RankHypotheses_ReturnsDescendingByScore()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repositoryPath);

        try
        {
            var clock = new FixedClock(new DateTimeOffset(2026, 4, 8, 21, 40, 0, TimeSpan.Zero));
            var jsonSerializer = new DefaultJsonSerializer();
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
            var service = new CtxApplicationService(workingRepository, commitRepository, branchRepository, runRepository, packetRepository, metricsRepository, runOrchestrator, contextBuilder, commitEngine, mergeEngine, clock);

            await service.InitAsync(repositoryPath, new Ctx.Application.InitRepositoryRequest("CTX", "Hypothesis rank test", "main", "tester"), CancellationToken.None);
            await service.AddHypothesisAsync(repositoryPath, new Ctx.Application.AddHypothesisRequest("Lower score", "baseline", 0.6m, 0.5m, 0.4m, 0.5m, null, "tester"), CancellationToken.None);
            await service.AddHypothesisAsync(repositoryPath, new Ctx.Application.AddHypothesisRequest("Higher score", "higher impact", 0.9m, 0.9m, 0.7m, 0.2m, null, "tester"), CancellationToken.None);

            var ranked = await service.RankHypothesesAsync(repositoryPath, CancellationToken.None);

            Assert.True(ranked.Success);
            var json = JsonSerializer.Serialize(ranked.Data, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            Assert.Contains("Higher score", json);
            Assert.Contains("Lower score", json);
            Assert.True(json.IndexOf("Higher score", StringComparison.Ordinal) < json.IndexOf("Lower score", StringComparison.Ordinal));
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
    public async Task NextAsync_RanksOpenTasksByCompositeScore()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repositoryPath);

        try
        {
            var clock = new FixedClock(new DateTimeOffset(2026, 4, 9, 12, 30, 0, TimeSpan.Zero));
            var jsonSerializer = new DefaultJsonSerializer();
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
            var service = new CtxApplicationService(workingRepository, commitRepository, branchRepository, runRepository, packetRepository, metricsRepository, runOrchestrator, contextBuilder, commitEngine, mergeEngine, clock);

            await service.InitAsync(repositoryPath, new Ctx.Application.InitRepositoryRequest("CTX", "Next scoring test", "main", "tester"), CancellationToken.None);
            var goalA = await service.AddGoalAsync(repositoryPath, new Ctx.Application.AddGoalRequest("Primary goal", "Highest priority", 1, null, "tester"), CancellationToken.None);
            var goalB = await service.AddGoalAsync(repositoryPath, new Ctx.Application.AddGoalRequest("Secondary goal", "Lower priority", 100, null, "tester"), CancellationToken.None);
            var primaryGoalId = ((Ctx.Domain.Goal)goalA.Data!).Id.Value;
            var secondaryGoalId = ((Ctx.Domain.Goal)goalB.Data!).Id.Value;

            var topTask = await service.AddTaskAsync(repositoryPath, new Ctx.Application.AddTaskRequest("Finish scoring engine", "Highest value", primaryGoalId, Array.Empty<string>(), "tester"), CancellationToken.None);
            var blockedDependency = await service.AddTaskAsync(repositoryPath, new Ctx.Application.AddTaskRequest("Resolve dependency", "Needs to finish first", secondaryGoalId, Array.Empty<string>(), "tester"), CancellationToken.None);
            var dependentTask = await service.AddTaskAsync(repositoryPath, new Ctx.Application.AddTaskRequest("Dependent follow-up", "Should rank lower because blocked", primaryGoalId, new[] { ((Ctx.Domain.Task)blockedDependency.Data!).Id.Value }, "tester"), CancellationToken.None);

            var topTaskId = ((Ctx.Domain.Task)topTask.Data!).Id.Value;
            var dependentTaskId = ((Ctx.Domain.Task)dependentTask.Data!).Id.Value;

            await service.AddHypothesisAsync(repositoryPath, new Ctx.Application.AddHypothesisRequest("Scoring engine is the next best task", "Highest impact", 0.95m, 0.95m, 0.80m, 0.20m, topTaskId, "tester"), CancellationToken.None);
            await service.AddHypothesisAsync(repositoryPath, new Ctx.Application.AddHypothesisRequest("Dependent task can wait", "Blocked by another task", 0.70m, 0.50m, 0.30m, 0.60m, dependentTaskId, "tester"), CancellationToken.None);

            var result = await service.NextAsync(repositoryPath, CancellationToken.None);

            Assert.True(result.Success);
            var json = JsonSerializer.Serialize(result.Data, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            using var document = JsonDocument.Parse(json);
            var recommended = document.RootElement.GetProperty("recommended");
            Assert.Equal(topTaskId, recommended.GetProperty("entityId").GetString());
            Assert.Equal("Task", recommended.GetProperty("candidateType").GetString());

            var candidates = document.RootElement.GetProperty("candidates");
            Assert.Equal(3, candidates.GetArrayLength());
            Assert.Equal(topTaskId, candidates[0].GetProperty("entityId").GetString());
            var diagnostics = document.RootElement.GetProperty("diagnostics");
            Assert.Equal("Task", diagnostics.GetProperty("selectionMode").GetString());
            Assert.Equal(3, diagnostics.GetProperty("openTaskCount").GetInt32());
            var dependentIndex = Enumerable.Range(0, candidates.GetArrayLength())
                .Single(index => candidates[index].GetProperty("entityId").GetString() == dependentTaskId);
            Assert.True(dependentIndex > 0);
            Assert.True(candidates[0].GetProperty("score").GetDecimal() > candidates[dependentIndex].GetProperty("score").GetDecimal());
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
    public async Task AddTaskAsync_WithParent_InheritsGoalAndStoresParentTaskId()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repositoryPath);

        try
        {
            var clock = new FixedClock(new DateTimeOffset(2026, 4, 9, 19, 0, 0, TimeSpan.Zero));
            var jsonSerializer = new DefaultJsonSerializer();
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
            var service = new CtxApplicationService(workingRepository, commitRepository, branchRepository, runRepository, packetRepository, metricsRepository, runOrchestrator, contextBuilder, commitEngine, mergeEngine, clock);

            await service.InitAsync(repositoryPath, new Ctx.Application.InitRepositoryRequest("CTX", "Subtask support test", "main", "tester"), CancellationToken.None);
            var goal = await service.AddGoalAsync(repositoryPath, new Ctx.Application.AddGoalRequest("Primary goal", "Track local work", 1, null, "tester"), CancellationToken.None);
            var goalId = ((Ctx.Domain.Goal)goal.Data!).Id.Value;

            var parentTaskResult = await service.AddTaskAsync(repositoryPath, new Ctx.Application.AddTaskRequest("Parent task", "Top-level work", goalId, Array.Empty<string>(), "tester"), CancellationToken.None);
            var parentTask = Assert.IsType<Ctx.Domain.Task>(parentTaskResult.Data);

            var childTaskResult = await service.AddTaskAsync(repositoryPath, new Ctx.Application.AddTaskRequest("Child task", "Discovered nearby work", null, Array.Empty<string>(), "tester", parentTask.Id.Value), CancellationToken.None);
            var childTask = Assert.IsType<Ctx.Domain.Task>(childTaskResult.Data);

            Assert.Equal(parentTask.Id, childTask.ParentTaskId);
            Assert.Equal(parentTask.GoalId, childTask.GoalId);

            var shown = await service.ShowArtifactAsync(repositoryPath, "task", childTask.Id.Value, CancellationToken.None);
            var persistedChildTask = Assert.IsType<Ctx.Domain.Task>(shown.Data);
            Assert.Equal(parentTask.Id, persistedChildTask.ParentTaskId);
            Assert.Equal(goalId, persistedChildTask.GoalId?.Value);
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
    public async Task NextAsync_ReturnsGapCandidateWhenNoOpenTasksRemain()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repositoryPath);

        try
        {
            var clock = new FixedClock(new DateTimeOffset(2026, 4, 9, 13, 0, 0, TimeSpan.Zero));
            var jsonSerializer = new DefaultJsonSerializer();
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
            var service = new CtxApplicationService(workingRepository, commitRepository, branchRepository, runRepository, packetRepository, metricsRepository, runOrchestrator, contextBuilder, commitEngine, mergeEngine, clock);

            await service.InitAsync(repositoryPath, new Ctx.Application.InitRepositoryRequest("CTX", "Gap scoring test", "main", "tester"), CancellationToken.None);
            var goal = await service.AddGoalAsync(repositoryPath, new Ctx.Application.AddGoalRequest("Preserve autonomous flow", "Find the next gap", 1, null, "tester"), CancellationToken.None);
            var goalId = ((Ctx.Domain.Goal)goal.Data!).Id.Value;

            var completedTask = await service.AddTaskAsync(repositoryPath, new Ctx.Application.AddTaskRequest("Close the last explicit task", "Leaves only gap candidates", goalId, Array.Empty<string>(), "tester"), CancellationToken.None);
            var completedTaskId = ((Ctx.Domain.Task)completedTask.Data!).Id.Value;

            var gapHypothesis = await service.AddHypothesisAsync(repositoryPath, new Ctx.Application.AddHypothesisRequest("The next-step scorer should promote the strongest recorded gap", "Do not fall back to chat habit", 0.92m, 0.91m, 0.80m, 0.20m, completedTaskId, "tester"), CancellationToken.None);
            var lowerGapHypothesis = await service.AddHypothesisAsync(repositoryPath, new Ctx.Application.AddHypothesisRequest("Lower value gap", "Still useful but not the first one", 0.60m, 0.55m, 0.45m, 0.40m, completedTaskId, "tester"), CancellationToken.None);

            await service.UpdateTaskAsync(repositoryPath, new Ctx.Application.UpdateTaskRequest(completedTaskId, null, null, "Done", "tester"), CancellationToken.None);
            await service.UpdateHypothesisAsync(
                repositoryPath,
                new Ctx.Application.UpdateHypothesisRequest(((Ctx.Domain.Hypothesis)lowerGapHypothesis.Data!).Id.Value, null, null, null, null, null, null, "Archived", "tester"),
                CancellationToken.None);

            var result = await service.NextAsync(repositoryPath, CancellationToken.None);

            Assert.True(result.Success);
            var json = JsonSerializer.Serialize(result.Data, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            using var document = JsonDocument.Parse(json);
            var recommended = document.RootElement.GetProperty("recommended");
            Assert.Equal("Gap", recommended.GetProperty("candidateType").GetString());
            Assert.Equal(((Ctx.Domain.Hypothesis)gapHypothesis.Data!).Id.Value, recommended.GetProperty("entityId").GetString());

            var candidates = document.RootElement.GetProperty("candidates");
            Assert.Equal(1, candidates.GetArrayLength());
            Assert.Equal("Open a new task from this gap", candidates[0].GetProperty("factors").GetProperty("recommendedAction").GetString());
            var diagnostics = document.RootElement.GetProperty("diagnostics");
            Assert.Equal("Gap", diagnostics.GetProperty("selectionMode").GetString());
            Assert.Equal(0, diagnostics.GetProperty("openTaskCount").GetInt32());
            Assert.Equal(1, diagnostics.GetProperty("gapCandidateCount").GetInt32());
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
    public async Task NextAsync_DoesNotReturnGapForHypothesisClosedByAcceptedConclusion()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repositoryPath);

        try
        {
            var clock = new FixedClock(new DateTimeOffset(2026, 4, 9, 13, 30, 0, TimeSpan.Zero));
            var jsonSerializer = new DefaultJsonSerializer();
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
            var service = new CtxApplicationService(workingRepository, commitRepository, branchRepository, runRepository, packetRepository, metricsRepository, runOrchestrator, contextBuilder, commitEngine, mergeEngine, clock);

            await service.InitAsync(repositoryPath, new Ctx.Application.InitRepositoryRequest("CTX", "Resolved gap test", "main", "tester"), CancellationToken.None);
            var goal = await service.AddGoalAsync(repositoryPath, new Ctx.Application.AddGoalRequest("Preserve autonomous flow", "Filter closed gaps", 1, null, "tester"), CancellationToken.None);
            var goalId = ((Ctx.Domain.Goal)goal.Data!).Id.Value;

            var completedTask = await service.AddTaskAsync(repositoryPath, new Ctx.Application.AddTaskRequest("Thread reconstruction MVP", "Already delivered", goalId, Array.Empty<string>(), "tester"), CancellationToken.None);
            var completedTaskId = ((Ctx.Domain.Task)completedTask.Data!).Id.Value;

            var resolvedHypothesis = await service.AddHypothesisAsync(repositoryPath, new Ctx.Application.AddHypothesisRequest("Task-focused JSON reconstruction validates the model quickly", "Already implemented", 0.92m, 0.91m, 0.80m, 0.20m, completedTaskId, "tester"), CancellationToken.None);
            var resolvedHypothesisId = ((Ctx.Domain.Hypothesis)resolvedHypothesis.Data!).Id.Value;

            var evidence = await service.AddEvidenceAsync(repositoryPath, new Ctx.Application.AddEvidenceRequest("Thread MVP evidence", "The MVP exists and is validated", "tests", "Experiment", 0.9m, new[] { $"hypothesis:{resolvedHypothesisId}" }, "tester"), CancellationToken.None);
            var evidenceId = ((Ctx.Domain.Evidence)evidence.Data!).Id.Value;

            var decision = await service.AddDecisionAsync(repositoryPath, new Ctx.Application.AddDecisionRequest("Keep task-first reconstruction", "The MVP already validates the model", "Accepted", new[] { resolvedHypothesisId }, new[] { evidenceId }, "tester"), CancellationToken.None);
            var decisionId = ((Ctx.Domain.Decision)decision.Data!).Id.Value;

            await service.AddConclusionAsync(repositoryPath, new Ctx.Application.AddConclusionRequest("The reconstruction gap is already closed by the accepted MVP thread.", "Accepted", new[] { decisionId }, new[] { evidenceId }, new[] { goalId }, new[] { completedTaskId }, "tester"), CancellationToken.None);
            await service.UpdateTaskAsync(repositoryPath, new Ctx.Application.UpdateTaskRequest(completedTaskId, null, null, "Done", "tester"), CancellationToken.None);

            var lowerGapTask = await service.AddTaskAsync(repositoryPath, new Ctx.Application.AddTaskRequest("Keep another closed task", "Provides a remaining valid gap candidate", goalId, Array.Empty<string>(), "tester"), CancellationToken.None);
            var lowerGapTaskId = ((Ctx.Domain.Task)lowerGapTask.Data!).Id.Value;
            var lowerGapHypothesis = await service.AddHypothesisAsync(repositoryPath, new Ctx.Application.AddHypothesisRequest("A remaining open gap should still be returned", "Still worth surfacing", 0.60m, 0.55m, 0.45m, 0.40m, lowerGapTaskId, "tester"), CancellationToken.None);
            await service.UpdateTaskAsync(repositoryPath, new Ctx.Application.UpdateTaskRequest(lowerGapTaskId, null, null, "Done", "tester"), CancellationToken.None);

            var result = await service.NextAsync(repositoryPath, CancellationToken.None);

            Assert.True(result.Success);
            var json = JsonSerializer.Serialize(result.Data, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            using var document = JsonDocument.Parse(json);
            var recommended = document.RootElement.GetProperty("recommended");
            Assert.Equal("Gap", recommended.GetProperty("candidateType").GetString());
            Assert.Equal(((Ctx.Domain.Hypothesis)lowerGapHypothesis.Data!).Id.Value, recommended.GetProperty("entityId").GetString());

            var candidates = document.RootElement.GetProperty("candidates");
            Assert.Equal(1, candidates.GetArrayLength());
            Assert.Equal(((Ctx.Domain.Hypothesis)lowerGapHypothesis.Data!).Id.Value, candidates[0].GetProperty("entityId").GetString());
            var diagnostics = document.RootElement.GetProperty("diagnostics");
            Assert.Equal(1, diagnostics.GetProperty("gapExcludedByAcceptedConclusions").GetInt32());
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
    public async Task NextAsync_ExplainsWhyNoCandidatesRemain()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repositoryPath);

        try
        {
            var clock = new FixedClock(new DateTimeOffset(2026, 4, 9, 13, 45, 0, TimeSpan.Zero));
            var jsonSerializer = new DefaultJsonSerializer();
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
            var service = new CtxApplicationService(workingRepository, commitRepository, branchRepository, runRepository, packetRepository, metricsRepository, runOrchestrator, contextBuilder, commitEngine, mergeEngine, clock);

            await service.InitAsync(repositoryPath, new Ctx.Application.InitRepositoryRequest("CTX", "No candidates test", "main", "tester"), CancellationToken.None);
            var goal = await service.AddGoalAsync(repositoryPath, new Ctx.Application.AddGoalRequest("Preserve autonomous flow", "Explain no next-step result", 1, null, "tester"), CancellationToken.None);
            var goalId = ((Ctx.Domain.Goal)goal.Data!).Id.Value;

            var completedTask = await service.AddTaskAsync(repositoryPath, new Ctx.Application.AddTaskRequest("Close the last explicit task", "Leaves only resolved threads", goalId, Array.Empty<string>(), "tester"), CancellationToken.None);
            var completedTaskId = ((Ctx.Domain.Task)completedTask.Data!).Id.Value;

            var resolvedHypothesis = await service.AddHypothesisAsync(repositoryPath, new Ctx.Application.AddHypothesisRequest("A resolved gap should not resurface", "Accepted conclusion closes this thread", 0.88m, 0.82m, 0.7m, 0.2m, completedTaskId, "tester"), CancellationToken.None);
            var resolvedHypothesisId = ((Ctx.Domain.Hypothesis)resolvedHypothesis.Data!).Id.Value;
            var evidence = await service.AddEvidenceAsync(repositoryPath, new Ctx.Application.AddEvidenceRequest("Resolution evidence", "Thread is already closed", "tests", "Observation", 0.9m, new[] { $"hypothesis:{resolvedHypothesisId}" }, "tester"), CancellationToken.None);
            var evidenceId = ((Ctx.Domain.Evidence)evidence.Data!).Id.Value;
            var decision = await service.AddDecisionAsync(repositoryPath, new Ctx.Application.AddDecisionRequest("Keep closure", "Accepted outcome already exists", "Accepted", new[] { resolvedHypothesisId }, new[] { evidenceId }, "tester"), CancellationToken.None);
            var decisionId = ((Ctx.Domain.Decision)decision.Data!).Id.Value;

            await service.AddConclusionAsync(repositoryPath, new Ctx.Application.AddConclusionRequest("The thread is closed and should not become a gap candidate.", "Accepted", new[] { decisionId }, new[] { evidenceId }, new[] { goalId }, new[] { completedTaskId }, "tester"), CancellationToken.None);
            await service.UpdateTaskAsync(repositoryPath, new Ctx.Application.UpdateTaskRequest(completedTaskId, null, null, "Done", "tester"), CancellationToken.None);

            var result = await service.NextAsync(repositoryPath, CancellationToken.None);

            Assert.True(result.Success);
            Assert.Contains("Review diagnostics", result.Message);
            var json = JsonSerializer.Serialize(result.Data, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            using var document = JsonDocument.Parse(json);
            Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("recommended").ValueKind);
            Assert.Equal(0, document.RootElement.GetProperty("candidates").GetArrayLength());
            var diagnostics = document.RootElement.GetProperty("diagnostics");
            Assert.Equal("None", diagnostics.GetProperty("selectionMode").GetString());
            Assert.Equal(0, diagnostics.GetProperty("openTaskCount").GetInt32());
            Assert.Equal(0, diagnostics.GetProperty("gapCandidateCount").GetInt32());
            Assert.Equal(1, diagnostics.GetProperty("gapExcludedByAcceptedConclusions").GetInt32());
            Assert.NotEqual(0, diagnostics.GetProperty("guidance").GetArrayLength());
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
    public async Task CheckAsync_UsesTopRankedTaskAndReportsMissingClosure()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repositoryPath);

        try
        {
            var clock = new FixedClock(new DateTimeOffset(2026, 4, 9, 14, 0, 0, TimeSpan.Zero));
            var jsonSerializer = new DefaultJsonSerializer();
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
            var service = new CtxApplicationService(workingRepository, commitRepository, branchRepository, runRepository, packetRepository, metricsRepository, runOrchestrator, contextBuilder, commitEngine, mergeEngine, clock);

            await service.InitAsync(repositoryPath, new Ctx.Application.InitRepositoryRequest("CTX", "Block check test", "main", "tester"), CancellationToken.None);
            var goal = await service.AddGoalAsync(repositoryPath, new Ctx.Application.AddGoalRequest("Close work block", "Check readiness", 1, null, "tester"), CancellationToken.None);
            var goalId = ((Ctx.Domain.Goal)goal.Data!).Id.Value;
            var task = await service.AddTaskAsync(repositoryPath, new Ctx.Application.AddTaskRequest("Check current block", "Need readiness guidance", goalId, Array.Empty<string>(), "tester"), CancellationToken.None);
            var taskId = ((Ctx.Domain.Task)task.Data!).Id.Value;
            await service.AddHypothesisAsync(repositoryPath, new Ctx.Application.AddHypothesisRequest("Readiness check should find missing closure", "No decision or conclusion yet", 0.9m, 0.8m, 0.6m, 0.2m, taskId, "tester"), CancellationToken.None);

            var result = await service.CheckAsync(repositoryPath, null, CancellationToken.None);

            Assert.True(result.Success);
            Assert.Contains("closure gaps", result.Message);
            var data = Assert.IsType<Ctx.Application.BlockCheckSummary>(result.Data);
            Assert.Equal(taskId, data.TaskId);
            Assert.Equal("top-ranked task from ctx next", data.SelectionReason);
            Assert.False(data.ReadyForCommit);
            Assert.Contains(data.Missing, item => item.ItemType == "TaskState");
            Assert.Contains(data.Missing, item => item.ItemType == "Evidence");
            Assert.Contains(data.Missing, item => item.ItemType == "Decision");
            Assert.Contains(data.Missing, item => item.ItemType == "Conclusion");
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
    public async Task CheckAsync_ReportsReadyWhenTaskThreadIsFullyClosed()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repositoryPath);

        try
        {
            var clock = new FixedClock(new DateTimeOffset(2026, 4, 9, 14, 15, 0, TimeSpan.Zero));
            var jsonSerializer = new DefaultJsonSerializer();
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
            var service = new CtxApplicationService(workingRepository, commitRepository, branchRepository, runRepository, packetRepository, metricsRepository, runOrchestrator, contextBuilder, commitEngine, mergeEngine, clock);

            await service.InitAsync(repositoryPath, new Ctx.Application.InitRepositoryRequest("CTX", "Ready block check test", "main", "tester"), CancellationToken.None);
            var goal = await service.AddGoalAsync(repositoryPath, new Ctx.Application.AddGoalRequest("Close work block", "Check readiness", 1, null, "tester"), CancellationToken.None);
            var goalId = ((Ctx.Domain.Goal)goal.Data!).Id.Value;
            var task = await service.AddTaskAsync(repositoryPath, new Ctx.Application.AddTaskRequest("Finish closed block", "Should be ready", goalId, Array.Empty<string>(), "tester"), CancellationToken.None);
            var taskId = ((Ctx.Domain.Task)task.Data!).Id.Value;
            var hypothesis = await service.AddHypothesisAsync(repositoryPath, new Ctx.Application.AddHypothesisRequest("Closed thread should be commit-ready", "All thread artifacts exist", 0.9m, 0.8m, 0.7m, 0.2m, taskId, "tester"), CancellationToken.None);
            var hypothesisId = ((Ctx.Domain.Hypothesis)hypothesis.Data!).Id.Value;
            var evidence = await service.AddEvidenceAsync(repositoryPath, new Ctx.Application.AddEvidenceRequest("Closure evidence", "Supports readiness", "tests", "Observation", 0.9m, new[] { $"hypothesis:{hypothesisId}" }, "tester"), CancellationToken.None);
            var evidenceId = ((Ctx.Domain.Evidence)evidence.Data!).Id.Value;
            var decision = await service.AddDecisionAsync(repositoryPath, new Ctx.Application.AddDecisionRequest("Proceed to commit", "The block is closed", "Accepted", new[] { hypothesisId }, new[] { evidenceId }, "tester"), CancellationToken.None);
            var decisionId = ((Ctx.Domain.Decision)decision.Data!).Id.Value;
            await service.AddConclusionAsync(repositoryPath, new Ctx.Application.AddConclusionRequest("The task thread is fully closed.", "Accepted", new[] { decisionId }, new[] { evidenceId }, new[] { goalId }, new[] { taskId }, "tester"), CancellationToken.None);
            await service.UpdateTaskAsync(repositoryPath, new Ctx.Application.UpdateTaskRequest(taskId, null, null, "Done", "tester"), CancellationToken.None);

            var result = await service.CheckAsync(repositoryPath, taskId, CancellationToken.None);

            Assert.True(result.Success);
            Assert.Contains("ready for cognitive commit", result.Message);
            var data = Assert.IsType<Ctx.Application.BlockCheckSummary>(result.Data);
            Assert.True(data.ReadyForCommit);
            Assert.Equal("explicit task selection", data.SelectionReason);
            Assert.Empty(data.Missing);
            Assert.Equal(1, data.AcceptedDecisionCount);
            Assert.Equal(1, data.AcceptedConclusionCount);
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
    public async Task ThreadReconstruct_Task_ReturnsSemanticThreadTimelineAndGaps()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repositoryPath);

        try
        {
            var clock = new FixedClock(new DateTimeOffset(2026, 4, 9, 4, 30, 0, TimeSpan.Zero));
            var jsonSerializer = new DefaultJsonSerializer();
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
            var service = new CtxApplicationService(workingRepository, commitRepository, branchRepository, runRepository, packetRepository, metricsRepository, runOrchestrator, contextBuilder, commitEngine, mergeEngine, clock);

            await service.InitAsync(repositoryPath, new Ctx.Application.InitRepositoryRequest("CTX", "Thread test", "main", "tester"), CancellationToken.None);
            var goal = await service.AddGoalAsync(repositoryPath, new Ctx.Application.AddGoalRequest("Recover rationale", "Need canonical reconstruction", 1, null, "tester"), CancellationToken.None);
            var goalId = ((Ctx.Domain.Goal)goal.Data!).Id.Value;
            var task = await service.AddTaskAsync(repositoryPath, new Ctx.Application.AddTaskRequest("Implement thread MVP", "Task focused reconstruction", goalId, Array.Empty<string>(), "tester"), CancellationToken.None);
            var taskId = ((Ctx.Domain.Task)task.Data!).Id.Value;
            var hypothesis = await service.AddHypothesisAsync(repositoryPath, new Ctx.Application.AddHypothesisRequest("Task thread is enough for MVP", "Start small", 0.9m, 0.8m, 0.7m, 0.2m, taskId, "tester"), CancellationToken.None);
            var hypothesisId = ((Ctx.Domain.Hypothesis)hypothesis.Data!).Id.Value;
            var evidence = await service.AddEvidenceAsync(repositoryPath, new Ctx.Application.AddEvidenceRequest("MVP scope evidence", "Task focus keeps scope bounded", "design", "Observation", 0.85m, new[] { $"hypothesis:{hypothesisId}" }, "tester"), CancellationToken.None);
            var evidenceId = ((Ctx.Domain.Evidence)evidence.Data!).Id.Value;
            var decision = await service.AddDecisionAsync(repositoryPath, new Ctx.Application.AddDecisionRequest("Start with task JSON", "Fastest validation path", "Accepted", new[] { hypothesisId }, new[] { evidenceId }, "tester"), CancellationToken.None);
            var decisionId = ((Ctx.Domain.Decision)decision.Data!).Id.Value;
            await service.AddConclusionAsync(repositoryPath, new Ctx.Application.AddConclusionRequest("Task thread MVP is sufficient for first reconstruction pass", "Accepted", new[] { decisionId }, new[] { evidenceId }, new[] { goalId }, new[] { taskId }, "tester"), CancellationToken.None);
            await service.CommitAsync(repositoryPath, new Ctx.Application.CommitRequest("seed task thread", "tester"), CancellationToken.None);

            var thread = await service.ThreadReconstructAsync(repositoryPath, "task", taskId, "json", CancellationToken.None);

            Assert.True(thread.Success);
            var payload = Assert.IsType<Ctx.Domain.ContextThread>(thread.Data);
            Assert.Equal("Task", payload.Focus.EntityType);
            Assert.Equal(taskId, payload.Focus.EntityId);
            Assert.Contains(payload.SemanticThread, item => item.EntityType == nameof(Ctx.Domain.Goal));
            Assert.Contains(payload.SemanticThread, item => item.EntityType == nameof(Ctx.Domain.Hypothesis));
            Assert.Contains(payload.SemanticThread, item => item.EntityType == nameof(Ctx.Domain.Decision));
            Assert.Contains(payload.SemanticThread, item => item.EntityType == nameof(Ctx.Domain.Conclusion));
            Assert.NotEmpty(payload.Timeline);
            Assert.Empty(payload.Gaps);
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
    public async Task ThreadReconstruct_Task_Markdown_ReturnsReadableNarrative()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repositoryPath);

        try
        {
            var clock = new FixedClock(new DateTimeOffset(2026, 4, 9, 13, 30, 0, TimeSpan.Zero));
            var jsonSerializer = new DefaultJsonSerializer();
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
            var service = new CtxApplicationService(workingRepository, commitRepository, branchRepository, runRepository, packetRepository, metricsRepository, runOrchestrator, contextBuilder, commitEngine, mergeEngine, clock);

            await service.InitAsync(repositoryPath, new Ctx.Application.InitRepositoryRequest("CTX", "Thread markdown test", "main", "tester"), CancellationToken.None);
            var task = await service.AddTaskAsync(repositoryPath, new Ctx.Application.AddTaskRequest("Write thread markdown", "Need a readable artifact", null, Array.Empty<string>(), "tester"), CancellationToken.None);
            var taskId = ((Ctx.Domain.Task)task.Data!).Id.Value;
            await service.AddHypothesisAsync(repositoryPath, new Ctx.Application.AddHypothesisRequest("Markdown is easier to read", "Narratives are easier to review", 0.9m, 0.8m, 0.7m, 0.2m, taskId, "tester"), CancellationToken.None);
            await service.CommitAsync(repositoryPath, new Ctx.Application.CommitRequest("seed markdown thread", "tester"), CancellationToken.None);

            var result = await service.ThreadReconstructAsync(repositoryPath, "task", taskId, "markdown", CancellationToken.None);

            Assert.True(result.Success);
            var markdown = Assert.IsType<string>(result.Data);
            Assert.Contains("# CTX Thread:", markdown);
            Assert.Contains("## Semantic Thread", markdown);
            Assert.Contains("## Timeline", markdown);
            Assert.Contains("## Gaps", markdown);
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
    public async Task UpdateHypothesis_UpdatesScoringDimensionsForExistingHypothesis()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repositoryPath);

        try
        {
            var clock = new FixedClock(new DateTimeOffset(2026, 4, 9, 3, 20, 0, TimeSpan.Zero));
            var jsonSerializer = new DefaultJsonSerializer();
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
            var service = new CtxApplicationService(workingRepository, commitRepository, branchRepository, runRepository, packetRepository, metricsRepository, runOrchestrator, contextBuilder, commitEngine, mergeEngine, clock);

            await service.InitAsync(repositoryPath, new Ctx.Application.InitRepositoryRequest("CTX", "Hypothesis update test", "main", "tester"), CancellationToken.None);
            var added = await service.AddHypothesisAsync(repositoryPath, new Ctx.Application.AddHypothesisRequest("Legacy", "before scoring", 0.7m, 0m, 0m, 0m, null, "tester"), CancellationToken.None);
            var addedJson = JsonSerializer.Serialize(added.Data, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var hypothesisId = JsonDocument.Parse(addedJson).RootElement.GetProperty("id").GetProperty("value").GetString()!;

            var updated = await service.UpdateHypothesisAsync(
                repositoryPath,
                new Ctx.Application.UpdateHypothesisRequest(hypothesisId, null, null, 0.7m, 0.8m, 0.6m, 0.2m, "Supported", "tester"),
                CancellationToken.None);

            Assert.True(updated.Success);

            var shown = await service.ShowArtifactAsync(repositoryPath, "hypothesis", hypothesisId, CancellationToken.None);
            var shownHypothesis = Assert.IsType<Ctx.Domain.Hypothesis>(shown.Data);
            Assert.Equal(0.8m, shownHypothesis.Impact);
            Assert.Equal(0.6m, shownHypothesis.EvidenceStrength);
            Assert.Equal(0.2m, shownHypothesis.CostToValidate);
            Assert.Equal(0.625m, shownHypothesis.Score);
            Assert.Equal(Ctx.Domain.HypothesisState.Supported, shownHypothesis.State);
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
    public async Task UpdateTask_ChangesExecutionState()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repositoryPath);

        try
        {
            var clock = new FixedClock(new DateTimeOffset(2026, 4, 9, 5, 0, 0, TimeSpan.Zero));
            var jsonSerializer = new DefaultJsonSerializer();
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
            var service = new CtxApplicationService(workingRepository, commitRepository, branchRepository, runRepository, packetRepository, metricsRepository, runOrchestrator, contextBuilder, commitEngine, mergeEngine, clock);

            await service.InitAsync(repositoryPath, new Ctx.Application.InitRepositoryRequest("CTX", "Task state test", "main", "tester"), CancellationToken.None);
            var added = await service.AddTaskAsync(repositoryPath, new Ctx.Application.AddTaskRequest("Close telemetry task", "Needs explicit task state", null, Array.Empty<string>(), "tester"), CancellationToken.None);
            var taskId = ((Ctx.Domain.Task)added.Data!).Id.Value;

            var updated = await service.UpdateTaskAsync(
                repositoryPath,
                new Ctx.Application.UpdateTaskRequest(taskId, null, null, "Done", "tester"),
                CancellationToken.None);

            Assert.True(updated.Success);
            var shown = await service.ShowArtifactAsync(repositoryPath, "task", taskId, CancellationToken.None);
            var task = Assert.IsType<Ctx.Domain.Task>(shown.Data);
            Assert.Equal(Ctx.Domain.TaskExecutionState.Done, task.State);
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
    public async Task UpdateConclusion_ChangesConclusionState()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repositoryPath);

        try
        {
            var clock = new FixedClock(new DateTimeOffset(2026, 4, 9, 14, 0, 0, TimeSpan.Zero));
            var jsonSerializer = new DefaultJsonSerializer();
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
            var service = new CtxApplicationService(workingRepository, commitRepository, branchRepository, runRepository, packetRepository, metricsRepository, runOrchestrator, contextBuilder, commitEngine, mergeEngine, clock);

            await service.InitAsync(repositoryPath, new Ctx.Application.InitRepositoryRequest("CTX", "Conclusion state test", "main", "tester"), CancellationToken.None);
            var added = await service.AddConclusionAsync(repositoryPath, new Ctx.Application.AddConclusionRequest("Needs final acceptance", "Draft", Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), "tester"), CancellationToken.None);
            var conclusionId = ((Ctx.Domain.Conclusion)added.Data!).Id.Value;

            var updated = await service.UpdateConclusionAsync(
                repositoryPath,
                new Ctx.Application.UpdateConclusionRequest(conclusionId, "Formally accepted closeout", "Accepted", "tester"),
                CancellationToken.None);

            Assert.True(updated.Success);
            var shown = await service.ShowArtifactAsync(repositoryPath, "conclusion", conclusionId, CancellationToken.None);
            var conclusion = Assert.IsType<Ctx.Domain.Conclusion>(shown.Data);
            Assert.Equal("Formally accepted closeout", conclusion.Summary);
            Assert.Equal(Ctx.Domain.ConclusionState.Accepted, conclusion.State);
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
    public async Task OperationalRunbook_AddListShow_ReturnsStructuredResults()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repositoryPath);

        try
        {
            var service = CreateService(new DateTimeOffset(2026, 4, 13, 17, 0, 0, TimeSpan.Zero));
            await service.InitAsync(repositoryPath, new Ctx.Application.InitRepositoryRequest("CTX", "Runbook test", "main", "tester"), CancellationToken.None);
            var goal = await service.AddGoalAsync(repositoryPath, new Ctx.Application.AddGoalRequest("Operate viewer", "Keep local viewer stable", 1, null, "tester"), CancellationToken.None);
            var goalId = ((Ctx.Domain.Goal)goal.Data!).Id.Value;
            var task = await service.AddTaskAsync(repositoryPath, new Ctx.Application.AddTaskRequest("Publish local viewer", "Refresh installed app", goalId, Array.Empty<string>(), "tester"), CancellationToken.None);
            var taskId = ((Ctx.Domain.Task)task.Data!).Id.Value;

            var added = await service.AddOperationalRunbookAsync(
                repositoryPath,
                new Ctx.Application.AddOperationalRunbookRequest(
                    "Local publish",
                    "Procedure",
                    new[] { "publish-local", "viewer" },
                    "Use when refreshing the installed local viewer.",
                    new[] { "Run scripts/publish-local.ps1", "Check C:\\ctx\\viewer" },
                    new[] { "Viewer loads locally" },
                    new[] { "docs/LOCAL_CTX_INSTALLATION.md", "scripts/publish-local.ps1" },
                    new[] { goalId },
                    new[] { taskId },
                    "tester"),
                CancellationToken.None);

            var list = await service.ListOperationalRunbooksAsync(repositoryPath, CancellationToken.None);
            var runbookId = ((Ctx.Domain.OperationalRunbook)added.Data!).Id.Value;
            var show = await service.ShowOperationalRunbookAsync(repositoryPath, runbookId, CancellationToken.None);

            Assert.True(added.Success);
            Assert.True(list.Success);
            Assert.True(show.Success);
            var listData = Assert.IsAssignableFrom<IReadOnlyList<Ctx.Domain.OperationalRunbook>>(list.Data);
            Assert.Single(listData);
            var runbook = Assert.IsType<Ctx.Domain.OperationalRunbook>(show.Data);
            Assert.Equal("Local publish", runbook.Title);
            Assert.Equal(Ctx.Domain.OperationalRunbookKind.Procedure, runbook.Kind);
            Assert.Contains(runbook.GoalIds, item => item.Value == goalId);
            Assert.Contains(runbook.TaskIds, item => item.Value == taskId);
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
    public async Task OperationalRunbook_Add_RecreatesMissingRunbookDirectoryForLegacyRepository()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repositoryPath);

        try
        {
            var service = CreateService(new DateTimeOffset(2026, 4, 13, 17, 5, 0, TimeSpan.Zero));
            await service.InitAsync(repositoryPath, new Ctx.Application.InitRepositoryRequest("CTX", "Legacy runbook dir test", "main", "tester"), CancellationToken.None);
            var goal = await service.AddGoalAsync(repositoryPath, new Ctx.Application.AddGoalRequest("Operate viewer", "Keep local viewer stable", 1, null, "tester"), CancellationToken.None);
            var goalId = ((Ctx.Domain.Goal)goal.Data!).Id.Value;

            var runbookDirectory = Path.Combine(repositoryPath, ".ctx", "runbooks");
            if (Directory.Exists(runbookDirectory))
            {
                Directory.Delete(runbookDirectory, recursive: true);
            }

            var added = await service.AddOperationalRunbookAsync(
                repositoryPath,
                new Ctx.Application.AddOperationalRunbookRequest(
                    "Local publish",
                    "Procedure",
                    new[] { "publish-local" },
                    "Use when refreshing the installed local viewer.",
                    new[] { "Run scripts/publish-local.ps1" },
                    new[] { "Viewer responds locally" },
                    new[] { "scripts/publish-local.ps1" },
                    new[] { goalId },
                    Array.Empty<string>(),
                    "tester"),
                CancellationToken.None);

            Assert.True(added.Success);
            Assert.True(Directory.Exists(runbookDirectory));
            Assert.Single(Directory.GetFiles(runbookDirectory, "*.json"));
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
    public async Task Context_IncludesTopTwoRunbooksAndAdditionalAvailability()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repositoryPath);

        try
        {
            var service = CreateService(new DateTimeOffset(2026, 4, 13, 17, 15, 0, TimeSpan.Zero));
            await service.InitAsync(repositoryPath, new Ctx.Application.InitRepositoryRequest("CTX", "Context runbook test", "main", "tester"), CancellationToken.None);
            var goal = await service.AddGoalAsync(repositoryPath, new Ctx.Application.AddGoalRequest("Operate viewer", "Keep local viewer stable", 1, null, "tester"), CancellationToken.None);
            var goalId = ((Ctx.Domain.Goal)goal.Data!).Id.Value;
            var task = await service.AddTaskAsync(repositoryPath, new Ctx.Application.AddTaskRequest("Publish local viewer", "Refresh installed app", goalId, Array.Empty<string>(), "tester"), CancellationToken.None);
            var taskId = ((Ctx.Domain.Task)task.Data!).Id.Value;

            await service.AddOperationalRunbookAsync(repositoryPath, new Ctx.Application.AddOperationalRunbookRequest(
                "Task-specific publish",
                "Procedure",
                new[] { "publish-local" },
                "Use when publishing the local viewer task.",
                new[] { "Run scripts/publish-local.ps1" },
                new[] { "Installed viewer responds" },
                new[] { "scripts/publish-local.ps1" },
                Array.Empty<string>(),
                new[] { taskId },
                "tester"), CancellationToken.None);

            await service.AddOperationalRunbookAsync(repositoryPath, new Ctx.Application.AddOperationalRunbookRequest(
                "Goal guardrail",
                "Guardrail",
                new[] { "git-commit" },
                "Use before committing viewer changes.",
                new[] { "Run ctx closeout first" },
                new[] { "ctx status is clean" },
                new[] { "ctx closeout" },
                new[] { goalId },
                Array.Empty<string>(),
                "tester"), CancellationToken.None);

            await service.AddOperationalRunbookAsync(repositoryPath, new Ctx.Application.AddOperationalRunbookRequest(
                "Publish validation",
                "Procedure",
                new[] { "publish-local" },
                "Use when validating local publish output.",
                new[] { "Open the installed viewer" },
                new[] { "Viewer loads locally" },
                new[] { "docs/LOCAL_CTX_INSTALLATION.md" },
                Array.Empty<string>(),
                Array.Empty<string>(),
                "tester"), CancellationToken.None);

            await service.AddOperationalRunbookAsync(repositoryPath, new Ctx.Application.AddOperationalRunbookRequest(
                "Recover index lock",
                "Troubleshooting",
                new[] { "index.lock" },
                "Use when git is blocked by index.lock.",
                new[] { "Inspect and remove stale lock" },
                new[] { "git status works" },
                new[] { ".git/index.lock" },
                Array.Empty<string>(),
                Array.Empty<string>(),
                "tester"), CancellationToken.None);

            var result = await service.ContextAsync(repositoryPath, "publish-local git-commit viewer refresh", goalId, taskId, CancellationToken.None);

            Assert.True(result.Success);
            var packet = Assert.IsType<Ctx.Domain.ContextPacket>(result.Data);
            Assert.NotNull(packet.RunbookIds);
            Assert.Equal(2, packet.RunbookIds!.Count);

            var runbookSection = Assert.Single(packet.Sections.Where(section => section.Title == "Operational Runbooks"));
            Assert.Contains("Task-specific publish", runbookSection.Content);
            Assert.Contains("Goal guardrail", runbookSection.Content);
            Assert.Contains("Additional runbooks available: Publish validation", runbookSection.Content);
            Assert.DoesNotContain("Recover index lock", runbookSection.Content, StringComparison.OrdinalIgnoreCase);
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
    public async Task AddGoalAsync_CreatesAutomaticCognitiveTrigger()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repositoryPath);

        try
        {
            var service = CreateService(new DateTimeOffset(2026, 4, 13, 18, 10, 0, TimeSpan.Zero));
            await service.InitAsync(repositoryPath, new Ctx.Application.InitRepositoryRequest("CTX", "Auto trigger goal test", "main", "tester"), CancellationToken.None);

            var goal = await service.AddGoalAsync(repositoryPath, new Ctx.Application.AddGoalRequest("Map reasoning", "Build a new investigation lane", 1, null, "tester"), CancellationToken.None);
            var goalId = ((Ctx.Domain.Goal)goal.Data!).Id.Value;
            var triggers = await service.ListCognitiveTriggersAsync(repositoryPath, CancellationToken.None);

            Assert.True(goal.Success);
            var triggerList = Assert.IsAssignableFrom<IReadOnlyList<Ctx.Domain.CognitiveTrigger>>(triggers.Data);
            var trigger = Assert.Single(triggerList);
            Assert.Equal(Ctx.Domain.CognitiveTriggerKind.AgentPrompt, trigger.Kind);
            Assert.Equal("Open goal line: Map reasoning", trigger.Summary);
            Assert.Contains(trigger.GoalIds, item => item.Value == goalId);
            Assert.Empty(trigger.TaskIds);
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
    public async Task OpenWorkLineAsync_CreatesAutomaticTriggerForSubGoalAndSeedTask()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repositoryPath);

        try
        {
            var service = CreateService(new DateTimeOffset(2026, 4, 13, 18, 15, 0, TimeSpan.Zero));
            await service.InitAsync(repositoryPath, new Ctx.Application.InitRepositoryRequest("CTX", "Auto trigger work line test", "main", "tester"), CancellationToken.None);
            var parentGoal = await service.AddGoalAsync(repositoryPath, new Ctx.Application.AddGoalRequest("Improve viewer", "Strategic viewer lane", 10, null, "tester"), CancellationToken.None);
            var parentGoalId = ((Ctx.Domain.Goal)parentGoal.Data!).Id.Value;

            var opened = await service.OpenWorkLineAsync(
                repositoryPath,
                new Ctx.Application.OpenWorkLineRequest(
                    parentGoalId,
                    "Viewer runtime reliability",
                    "Stabilize refresh and publish flow.",
                    12,
                    "Inspect viewer runtime",
                    "Check the next runtime failure.",
                    "tester"),
                CancellationToken.None);

            var summary = Assert.IsType<Ctx.Application.OpenWorkLineSummary>(opened.Data);
            var triggers = await service.ListCognitiveTriggersAsync(repositoryPath, CancellationToken.None);
            var triggerList = Assert.IsAssignableFrom<IReadOnlyList<Ctx.Domain.CognitiveTrigger>>(triggers.Data);
            var trigger = Assert.Single(triggerList.Where(item => item.GoalIds.Any(id => id.Value == summary.GoalId) && item.TaskIds.Any(id => id.Value == summary.SeedTaskId)));

            Assert.Equal("Open tactical line: Viewer runtime reliability", trigger.Summary);
            Assert.Equal(Ctx.Domain.CognitiveTriggerKind.AgentPrompt, trigger.Kind);
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
    public async Task AddTaskAsync_CreatesAutomaticTriggerOnlyForTopLevelIndependentTasks()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repositoryPath);

        try
        {
            var service = CreateService(new DateTimeOffset(2026, 4, 13, 18, 20, 0, TimeSpan.Zero));
            await service.InitAsync(repositoryPath, new Ctx.Application.InitRepositoryRequest("CTX", "Auto trigger task test", "main", "tester"), CancellationToken.None);

            var topLevel = await service.AddTaskAsync(repositoryPath, new Ctx.Application.AddTaskRequest("Investigate graph gap", "Open a focused task lane", null, Array.Empty<string>(), "tester"), CancellationToken.None);
            var topLevelTask = Assert.IsType<Ctx.Domain.Task>(topLevel.Data);

            var child = await service.AddTaskAsync(repositoryPath, new Ctx.Application.AddTaskRequest("Document sub-step", "Continuation only", null, Array.Empty<string>(), "tester", topLevelTask.Id.Value), CancellationToken.None);
            var childTask = Assert.IsType<Ctx.Domain.Task>(child.Data);

            var dependent = await service.AddTaskAsync(repositoryPath, new Ctx.Application.AddTaskRequest("Follow dependency", "Depends on the first task", null, new[] { topLevelTask.Id.Value }, "tester"), CancellationToken.None);
            var dependentTask = Assert.IsType<Ctx.Domain.Task>(dependent.Data);

            var triggers = await service.ListCognitiveTriggersAsync(repositoryPath, CancellationToken.None);
            var triggerList = Assert.IsAssignableFrom<IReadOnlyList<Ctx.Domain.CognitiveTrigger>>(triggers.Data);

            Assert.Single(triggerList);
            var trigger = triggerList[0];
            Assert.Equal($"Open task line: {topLevelTask.Title}", trigger.Summary);
            Assert.Contains(trigger.TaskIds, item => item == topLevelTask.Id);
            Assert.DoesNotContain(trigger.TaskIds, item => item == childTask.Id);
            Assert.DoesNotContain(trigger.TaskIds, item => item == dependentTask.Id);
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
    public async Task Check_IncludesSuggestedRunbooksForFocusedTask()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repositoryPath);

        try
        {
            var service = CreateService(new DateTimeOffset(2026, 4, 13, 18, 0, 0, TimeSpan.Zero));
            await service.InitAsync(repositoryPath, new Ctx.Application.InitRepositoryRequest("CTX", "Check runbook test", "main", "tester"), CancellationToken.None);
            var goal = await service.AddGoalAsync(repositoryPath, new Ctx.Application.AddGoalRequest("Operate viewer", "Keep local viewer stable", 1, null, "tester"), CancellationToken.None);
            var goalId = ((Ctx.Domain.Goal)goal.Data!).Id.Value;
            var task = await service.AddTaskAsync(repositoryPath, new Ctx.Application.AddTaskRequest("Publish local viewer", "publish-local git-commit viewer refresh", goalId, Array.Empty<string>(), "tester"), CancellationToken.None);
            var taskId = ((Ctx.Domain.Task)task.Data!).Id.Value;

            await service.AddOperationalRunbookAsync(repositoryPath, new Ctx.Application.AddOperationalRunbookRequest(
                "Local publish",
                "Procedure",
                new[] { "publish-local" },
                "Use when refreshing the installed local viewer.",
                new[] { "Run scripts/publish-local.ps1" },
                new[] { "Viewer responds locally" },
                new[] { "scripts/publish-local.ps1" },
                new[] { goalId },
                new[] { taskId },
                "tester"), CancellationToken.None);

            await service.AddOperationalRunbookAsync(repositoryPath, new Ctx.Application.AddOperationalRunbookRequest(
                "Git closeout",
                "Guardrail",
                new[] { "git-commit" },
                "Use before git closeout for viewer changes.",
                new[] { "Run ctx closeout" },
                new[] { "ctx status is clean" },
                new[] { "ctx closeout" },
                new[] { goalId },
                Array.Empty<string>(),
                "tester"), CancellationToken.None);

            await service.AddOperationalRunbookAsync(repositoryPath, new Ctx.Application.AddOperationalRunbookRequest(
                "Viewer validation",
                "Procedure",
                new[] { "viewer" },
                "Use when validating the installed viewer.",
                new[] { "Open the installed viewer" },
                new[] { "Viewer loads locally" },
                new[] { "docs/LOCAL_CTX_INSTALLATION.md" },
                Array.Empty<string>(),
                Array.Empty<string>(),
                "tester"), CancellationToken.None);

            var result = await service.CheckAsync(repositoryPath, taskId, CancellationToken.None);

            Assert.True(result.Success);
            var data = Assert.IsType<Ctx.Application.BlockCheckSummary>(result.Data);
            Assert.Equal(2, data.RunbookSuggestions.Count);
            Assert.Contains(data.RunbookSuggestions, item => item.Title == "Local publish");
            Assert.Contains(data.RunbookSuggestions, item => item.Title == "Git closeout");
            Assert.Contains("Viewer validation", data.AdditionalRunbooksAvailable);
            Assert.Contains(data.Guidance, item => item.Contains("operational runbook suggestion", StringComparison.OrdinalIgnoreCase));
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
    public async Task ExportImport_PreservesOperationalRunbooks()
    {
        var sourceRepositoryPath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        var targetRepositoryPath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        var exportPath = Path.Combine(Path.GetTempPath(), "ctx-tests", $"{Guid.NewGuid():N}.json");
        Directory.CreateDirectory(sourceRepositoryPath);
        Directory.CreateDirectory(targetRepositoryPath);

        try
        {
            var service = CreateService(new DateTimeOffset(2026, 4, 13, 17, 30, 0, TimeSpan.Zero));
            await service.InitAsync(sourceRepositoryPath, new Ctx.Application.InitRepositoryRequest("CTX", "Export runbook test", "main", "tester"), CancellationToken.None);
            var goal = await service.AddGoalAsync(sourceRepositoryPath, new Ctx.Application.AddGoalRequest("Operate viewer", "Keep local viewer stable", 1, null, "tester"), CancellationToken.None);
            var goalId = ((Ctx.Domain.Goal)goal.Data!).Id.Value;

            await service.AddOperationalRunbookAsync(sourceRepositoryPath, new Ctx.Application.AddOperationalRunbookRequest(
                "Git closeout",
                "Policy",
                new[] { "git-commit", "git-push" },
                "Use before publishing viewer changes.",
                new[] { "Run ctx closeout", "Commit CTX before Git" },
                new[] { "git status is clean" },
                new[] { "ctx closeout" },
                new[] { goalId },
                Array.Empty<string>(),
                "tester"), CancellationToken.None);

            var export = await service.ExportAsync(sourceRepositoryPath, exportPath, CancellationToken.None);
            var importedService = CreateService(new DateTimeOffset(2026, 4, 13, 17, 35, 0, TimeSpan.Zero));
            var import = await importedService.ImportAsync(targetRepositoryPath, exportPath, CancellationToken.None);
            var runbooks = await importedService.ListOperationalRunbooksAsync(targetRepositoryPath, CancellationToken.None);

            Assert.True(export.Success);
            Assert.True(import.Success);
            var runbookList = Assert.IsAssignableFrom<IReadOnlyList<Ctx.Domain.OperationalRunbook>>(runbooks.Data);
            Assert.Single(runbookList);
            Assert.Equal("Git closeout", runbookList[0].Title);
        }
        finally
        {
            if (Directory.Exists(sourceRepositoryPath))
            {
                Directory.Delete(sourceRepositoryPath, recursive: true);
            }

            if (Directory.Exists(targetRepositoryPath))
            {
                Directory.Delete(targetRepositoryPath, recursive: true);
            }

            if (File.Exists(exportPath))
            {
                File.Delete(exportPath);
            }
        }
    }

    [Fact]
    public async Task ConcurrentTaskUpdates_PersistBothChangesWithRepositoryLock()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repositoryPath);

        try
        {
            var clock = new FixedClock(new DateTimeOffset(2026, 4, 9, 5, 30, 0, TimeSpan.Zero));
            var jsonSerializer = new DefaultJsonSerializer();
            var hashing = new Sha256HashingService();
            var workingRepository = new FileSystemWorkingContextRepository(jsonSerializer);
            var commitRepository = new FileSystemCommitRepository(jsonSerializer);
            var branchRepository = new FileSystemBranchRepository(jsonSerializer);
            var packetRepository = new FileSystemPacketRepository(jsonSerializer);
            var runRepository = new FileSystemRunRepository(jsonSerializer);
            var metricsRepository = new FileSystemMetricsRepository(jsonSerializer);
            var repositoryWriteLock = new FileSystemRepositoryWriteLock();
            var contextBuilder = new ContextBuilder(clock, hashing);
            var commitEngine = new CommitEngine(clock, hashing, jsonSerializer, new DiffEngine());
            var mergeEngine = new MergeEngine();
            var providers = new Ctx.Providers.AIProviderRegistry(Array.Empty<Ctx.Application.IAIProvider>());
            var runOrchestrator = new RunOrchestrator(contextBuilder, providers, packetRepository, runRepository, metricsRepository, clock, hashing);
            var service = new CtxApplicationService(workingRepository, commitRepository, branchRepository, runRepository, packetRepository, metricsRepository, runOrchestrator, contextBuilder, commitEngine, mergeEngine, clock, repositoryWriteLock);

            await service.InitAsync(repositoryPath, new Ctx.Application.InitRepositoryRequest("CTX", "Concurrent task update test", "main", "tester"), CancellationToken.None);
            var first = await service.AddTaskAsync(repositoryPath, new Ctx.Application.AddTaskRequest("Task A", "first", null, Array.Empty<string>(), "tester"), CancellationToken.None);
            var second = await service.AddTaskAsync(repositoryPath, new Ctx.Application.AddTaskRequest("Task B", "second", null, Array.Empty<string>(), "tester"), CancellationToken.None);
            var firstTaskId = ((Ctx.Domain.Task)first.Data!).Id.Value;
            var secondTaskId = ((Ctx.Domain.Task)second.Data!).Id.Value;

            await Task.WhenAll(
                service.UpdateTaskAsync(repositoryPath, new Ctx.Application.UpdateTaskRequest(firstTaskId, null, null, "Done", "tester"), CancellationToken.None),
                service.UpdateTaskAsync(repositoryPath, new Ctx.Application.UpdateTaskRequest(secondTaskId, null, null, "Done", "tester"), CancellationToken.None));

            var firstShown = await service.ShowArtifactAsync(repositoryPath, "task", firstTaskId, CancellationToken.None);
            var secondShown = await service.ShowArtifactAsync(repositoryPath, "task", secondTaskId, CancellationToken.None);

            Assert.Equal(Ctx.Domain.TaskExecutionState.Done, Assert.IsType<Ctx.Domain.Task>(firstShown.Data).State);
            Assert.Equal(Ctx.Domain.TaskExecutionState.Done, Assert.IsType<Ctx.Domain.Task>(secondShown.Data).State);
        }
        finally
        {
            if (Directory.Exists(repositoryPath))
            {
                Directory.Delete(repositoryPath, recursive: true);
            }
        }
    }

    private static CtxApplicationService CreateService(DateTimeOffset timestamp)
    {
        var clock = new FixedClock(timestamp);
        var jsonSerializer = new DefaultJsonSerializer();
        var hashing = new Sha256HashingService();
        var workingRepository = new FileSystemWorkingContextRepository(jsonSerializer);
        var commitRepository = new FileSystemCommitRepository(jsonSerializer);
        var branchRepository = new FileSystemBranchRepository(jsonSerializer);
        var packetRepository = new FileSystemPacketRepository(jsonSerializer);
        var runRepository = new FileSystemRunRepository(jsonSerializer);
        var metricsRepository = new FileSystemMetricsRepository(jsonSerializer);
        var runbookRepository = new FileSystemOperationalRunbookRepository(jsonSerializer);
        var triggerRepository = new FileSystemCognitiveTriggerRepository(jsonSerializer);
        var contextBuilder = new ContextBuilder(clock, hashing);
        var commitEngine = new CommitEngine(clock, hashing, jsonSerializer, new DiffEngine());
        var mergeEngine = new MergeEngine();
        var providers = new Ctx.Providers.AIProviderRegistry(Array.Empty<Ctx.Application.IAIProvider>());
        var runOrchestrator = new RunOrchestrator(contextBuilder, providers, packetRepository, runRepository, metricsRepository, clock, hashing, runbookRepository, triggerRepository);
        return new CtxApplicationService(workingRepository, commitRepository, branchRepository, runRepository, packetRepository, metricsRepository, runOrchestrator, contextBuilder, commitEngine, mergeEngine, clock, null, runbookRepository, triggerRepository);
    }
}
