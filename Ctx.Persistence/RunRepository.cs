namespace Ctx.Persistence;

using Ctx.Application;
using Ctx.Domain;

public sealed class FileSystemRunRepository : IRunRepository
{
    private readonly IJsonSerializer _jsonSerializer;

    public FileSystemRunRepository(IJsonSerializer jsonSerializer)
    {
        _jsonSerializer = jsonSerializer;
    }

    public System.Threading.Tasks.Task SaveAsync(string repositoryPath, Run run, CancellationToken cancellationToken)
        => File.WriteAllTextAsync(RepositoryPaths.Run(repositoryPath, run.Id), _jsonSerializer.Serialize(run), cancellationToken);

    public async System.Threading.Tasks.Task<IReadOnlyList<Run>> ListAsync(string repositoryPath, CancellationToken cancellationToken)
    {
        var files = Directory.Exists(RepositoryPaths.Runs(repositoryPath))
            ? Directory.GetFiles(RepositoryPaths.Runs(repositoryPath), "*.json")
            : Array.Empty<string>();
        var items = new List<Run>();
        foreach (var file in files)
        {
            var json = await File.ReadAllTextAsync(file, cancellationToken);
            items.Add(_jsonSerializer.Deserialize<Run>(json));
        }

        return items.OrderByDescending(item => item.StartedAtUtc).ToArray();
    }

    public async System.Threading.Tasks.Task<Run?> LoadAsync(string repositoryPath, RunId runId, CancellationToken cancellationToken)
    {
        var path = RepositoryPaths.Run(repositoryPath, runId);
        if (!File.Exists(path))
        {
            return null;
        }

        var json = await File.ReadAllTextAsync(path, cancellationToken);
        return _jsonSerializer.Deserialize<Run>(json);
    }
}
