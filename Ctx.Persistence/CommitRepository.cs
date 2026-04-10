namespace Ctx.Persistence;

using Ctx.Application;
using Ctx.Domain;

public sealed class FileSystemCommitRepository : ICommitRepository
{
    private readonly IJsonSerializer _jsonSerializer;

    public FileSystemCommitRepository(IJsonSerializer jsonSerializer)
    {
        _jsonSerializer = jsonSerializer;
    }

    public System.Threading.Tasks.Task SaveAsync(string repositoryPath, ContextCommit commit, CancellationToken cancellationToken)
        => File.WriteAllTextAsync(RepositoryPaths.Commit(repositoryPath, commit.Id), _jsonSerializer.Serialize(commit), cancellationToken);

    public async System.Threading.Tasks.Task<ContextCommit?> LoadAsync(string repositoryPath, ContextCommitId commitId, CancellationToken cancellationToken)
    {
        var path = RepositoryPaths.Commit(repositoryPath, commitId);
        if (!File.Exists(path))
        {
            return null;
        }

        var json = await File.ReadAllTextAsync(path, cancellationToken);
        return _jsonSerializer.Deserialize<ContextCommit>(json);
    }

    public async System.Threading.Tasks.Task<IReadOnlyList<ContextCommit>> GetHistoryAsync(string repositoryPath, string branch, CancellationToken cancellationToken)
    {
        var files = Directory.Exists(RepositoryPaths.Commits(repositoryPath))
            ? Directory.GetFiles(RepositoryPaths.Commits(repositoryPath), "*.json")
            : Array.Empty<string>();

        var commits = new List<ContextCommit>();
        foreach (var file in files)
        {
            var json = await File.ReadAllTextAsync(file, cancellationToken);
            var commit = _jsonSerializer.Deserialize<ContextCommit>(json);
            if (commit.Branch.Equals(branch, StringComparison.OrdinalIgnoreCase))
            {
                commits.Add(commit);
            }
        }

        return commits.OrderByDescending(commit => commit.CreatedAtUtc).ToArray();
    }
}
