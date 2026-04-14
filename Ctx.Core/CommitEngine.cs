namespace Ctx.Core;

using Ctx.Application;
using Ctx.Domain;

public sealed class CommitEngine : ICommitEngine
{
    private readonly IClock _clock;
    private readonly IHashingService _hashingService;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IDiffEngine _diffEngine;

    public CommitEngine(IClock clock, IHashingService hashingService, IJsonSerializer jsonSerializer, IDiffEngine diffEngine)
    {
        _clock = clock;
        _hashingService = hashingService;
        _jsonSerializer = jsonSerializer;
        _diffEngine = diffEngine;
    }

    public ContextCommit CreateCommit(WorkingContext current, IReadOnlyList<OperationalRunbook> runbooks, ContextCommit? previous, string message, string createdBy)
        => CreateCommit(current, runbooks, Array.Empty<CognitiveTrigger>(), previous, message, createdBy);

    public ContextCommit CreateCommit(WorkingContext current, IReadOnlyList<OperationalRunbook> runbooks, IReadOnlyList<CognitiveTrigger> triggers, ContextCommit? previous, string message, string createdBy)
    {
        var cleanSnapshot = current with
        {
            Dirty = false,
            Trace = current.Trace with
            {
                UpdatedAtUtc = _clock.UtcNow,
                UpdatedBy = createdBy
            }
        };

        var repositorySnapshot = new RepositorySnapshot(
            cleanSnapshot,
            runbooks.OrderBy(item => item.Title, StringComparer.OrdinalIgnoreCase).ToArray(),
            triggers.OrderBy(item => item.Trace.CreatedAtUtc).ToArray());
        var snapshotHash = _hashingService.Hash(_jsonSerializer.Serialize(repositorySnapshot));
        var commitId = ContextCommitId.New();
        var commitWorkingSnapshot = cleanSnapshot with { HeadCommitId = commitId };
        var commitSnapshot = repositorySnapshot with { WorkingContext = commitWorkingSnapshot };
        var diff = _diffEngine.Diff(previous, commitSnapshot);
        var trace = new Traceability(
            createdBy,
            _clock.UtcNow,
            null,
            null,
            new[] { "commit" },
            previous is null ? Array.Empty<string>() : new[] { previous.Id.Value },
            ResolveModelName(),
            ResolveModelVersion());

        return new ContextCommit(
            commitId,
            commitWorkingSnapshot.CurrentBranch,
            message,
            previous is null ? Array.Empty<ContextCommitId>() : new[] { previous.Id },
            _clock.UtcNow,
            snapshotHash,
            diff,
            commitSnapshot,
            trace);
    }

    private static string? ResolveModelName()
        => Environment.GetEnvironmentVariable("CTX_MODEL_NAME")
            ?? Environment.GetEnvironmentVariable("OPENAI_MODEL");

    private static string? ResolveModelVersion()
        => Environment.GetEnvironmentVariable("CTX_MODEL_VERSION");
}
