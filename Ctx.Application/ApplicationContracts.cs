namespace Ctx.Application;

using Ctx.Domain;

public record CommandResult(bool Success, string Message, object? Data = null);

public record InitRepositoryRequest(string ProjectName, string Description, string Branch, string CreatedBy);
public record AddGoalRequest(string Title, string Description, int Priority, string? ParentGoalId, string CreatedBy);
public record AddTaskRequest(string Title, string Description, string? GoalId, IReadOnlyList<string> DependsOnTaskIds, string CreatedBy, string? ParentTaskId = null);
public record UpdateTaskRequest(string TaskId, string? Title, string? Description, string? State, string UpdatedBy);
public record AddHypothesisRequest(string Statement, string Rationale, decimal Confidence, decimal Impact, decimal EvidenceStrength, decimal CostToValidate, string? TaskId, string CreatedBy);
public record UpdateHypothesisRequest(string HypothesisId, string? Statement, string? Rationale, decimal? Confidence, decimal? Impact, decimal? EvidenceStrength, decimal? CostToValidate, string? State, string UpdatedBy);
public record AddDecisionRequest(string Title, string Rationale, string State, IReadOnlyList<string> HypothesisIds, IReadOnlyList<string> EvidenceIds, string CreatedBy);
public record AddEvidenceRequest(string Title, string Summary, string Source, string Kind, decimal Confidence, IReadOnlyList<string> Supports, string CreatedBy);
public record AddConclusionRequest(string Summary, string State, IReadOnlyList<string> DecisionIds, IReadOnlyList<string> EvidenceIds, IReadOnlyList<string> GoalIds, IReadOnlyList<string> TaskIds, string CreatedBy);
public record UpdateConclusionRequest(string ConclusionId, string? Summary, string? State, string UpdatedBy);
public record RunRequest(string Provider, string Purpose, string Model, string? GoalId, string? TaskId, string RequestedBy);
public record CommitRequest(string Message, string CreatedBy);
public record GraphExportRequest(string Format);

public interface IWorkingContextRepository
{
    System.Threading.Tasks.Task<bool> ExistsAsync(string repositoryPath, CancellationToken cancellationToken);
    System.Threading.Tasks.Task InitializeAsync(string repositoryPath, RepositoryVersion version, RepositoryConfig config, Project project, HeadReference head, BranchReference branch, WorkingContext context, CancellationToken cancellationToken);
    System.Threading.Tasks.Task ImportAsync(string repositoryPath, RepositoryExport exportData, CancellationToken cancellationToken);
    System.Threading.Tasks.Task<WorkingContext> LoadAsync(string repositoryPath, CancellationToken cancellationToken);
    System.Threading.Tasks.Task SaveWorkingAsync(string repositoryPath, WorkingContext workingContext, CancellationToken cancellationToken);
    System.Threading.Tasks.Task SaveStagingAsync(string repositoryPath, WorkingContext stagingContext, CancellationToken cancellationToken);
    System.Threading.Tasks.Task<HeadReference> LoadHeadAsync(string repositoryPath, CancellationToken cancellationToken);
    System.Threading.Tasks.Task SaveHeadAsync(string repositoryPath, HeadReference head, CancellationToken cancellationToken);
    System.Threading.Tasks.Task<RepositoryConfig> LoadConfigAsync(string repositoryPath, CancellationToken cancellationToken);
    System.Threading.Tasks.Task<RepositoryVersion> LoadVersionAsync(string repositoryPath, CancellationToken cancellationToken);
}

public interface ICommitRepository
{
    System.Threading.Tasks.Task SaveAsync(string repositoryPath, ContextCommit commit, CancellationToken cancellationToken);
    System.Threading.Tasks.Task<ContextCommit?> LoadAsync(string repositoryPath, ContextCommitId commitId, CancellationToken cancellationToken);
    System.Threading.Tasks.Task<IReadOnlyList<ContextCommit>> GetHistoryAsync(string repositoryPath, string branch, CancellationToken cancellationToken);
}

public interface IBranchRepository
{
    System.Threading.Tasks.Task SaveAsync(string repositoryPath, BranchReference branch, CancellationToken cancellationToken);
    System.Threading.Tasks.Task<BranchReference?> LoadAsync(string repositoryPath, string branchName, CancellationToken cancellationToken);
    System.Threading.Tasks.Task<IReadOnlyList<BranchReference>> ListAsync(string repositoryPath, CancellationToken cancellationToken);
}

public interface IRunRepository
{
    System.Threading.Tasks.Task SaveAsync(string repositoryPath, Run run, CancellationToken cancellationToken);
    System.Threading.Tasks.Task<IReadOnlyList<Run>> ListAsync(string repositoryPath, CancellationToken cancellationToken);
    System.Threading.Tasks.Task<Run?> LoadAsync(string repositoryPath, RunId runId, CancellationToken cancellationToken);
}

public interface IPacketRepository
{
    System.Threading.Tasks.Task SaveAsync(string repositoryPath, ContextPacket packet, CancellationToken cancellationToken);
    System.Threading.Tasks.Task<IReadOnlyList<ContextPacket>> ListAsync(string repositoryPath, CancellationToken cancellationToken);
    System.Threading.Tasks.Task<ContextPacket?> LoadAsync(string repositoryPath, ContextPacketId packetId, CancellationToken cancellationToken);
}

public interface IMetricsRepository
{
    System.Threading.Tasks.Task<MetricsSnapshot> LoadAsync(string repositoryPath, CancellationToken cancellationToken);
    System.Threading.Tasks.Task SaveAsync(string repositoryPath, MetricsSnapshot snapshot, CancellationToken cancellationToken);
}

public interface IRepositoryWriteLock
{
    System.Threading.Tasks.Task<IAsyncDisposable> AcquireAsync(string repositoryPath, CancellationToken cancellationToken);
}

public interface IContextBuilder
{
    ContextPacket Build(WorkingContext context, string purpose, string? goalId = null, string? taskId = null);
}

public interface IDiffEngine
{
    ContextDiff Diff(ContextCommit? previous, WorkingContext current);
}

public interface ICommitEngine
{
    ContextCommit CreateCommit(WorkingContext current, ContextCommit? previous, string message, string createdBy);
}

public interface IMergeEngine
{
    MergeResult Merge(WorkingContext current, ContextCommit sourceCommit);
}

public interface IRunOrchestrator
{
    System.Threading.Tasks.Task<Run> ExecuteAsync(string repositoryPath, WorkingContext workingContext, RunRequest request, CancellationToken cancellationToken);
}

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}

public interface IHashingService
{
    string Hash(string value);
}

public interface IJsonSerializer
{
    string Serialize<T>(T value);
    T Deserialize<T>(string json);
}

public interface IAIProvider
{
    string Name { get; }
    System.Threading.Tasks.Task<ProviderExecutionResult> ExecuteAsync(ContextPacket packet, ProviderExecutionRequest request, CancellationToken cancellationToken);
}

public interface IAIProviderRegistry
{
    IAIProvider Get(string providerName);
    IReadOnlyCollection<string> List();
}

public record ProviderExecutionRequest(string Model, string Purpose);

public record ProviderExecutionResult(
    string Provider,
    string Model,
    string Summary,
    IReadOnlyList<RunArtifact> Artifacts,
    TokenUsage Usage);
