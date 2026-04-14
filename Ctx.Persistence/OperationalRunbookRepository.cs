namespace Ctx.Persistence;

using Ctx.Application;
using Ctx.Domain;

public sealed class FileSystemOperationalRunbookRepository : IOperationalRunbookRepository
{
    private readonly IJsonSerializer _jsonSerializer;

    public FileSystemOperationalRunbookRepository(IJsonSerializer jsonSerializer)
    {
        _jsonSerializer = jsonSerializer;
    }

    public System.Threading.Tasks.Task SaveAsync(string repositoryPath, OperationalRunbook runbook, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(RepositoryPaths.Runbooks(repositoryPath));
        return File.WriteAllTextAsync(RepositoryPaths.Runbook(repositoryPath, runbook.Id), _jsonSerializer.Serialize(runbook), cancellationToken);
    }

    public async System.Threading.Tasks.Task<IReadOnlyList<OperationalRunbook>> ListAsync(string repositoryPath, CancellationToken cancellationToken)
    {
        var files = Directory.Exists(RepositoryPaths.Runbooks(repositoryPath))
            ? Directory.GetFiles(RepositoryPaths.Runbooks(repositoryPath), "*.json")
            : Array.Empty<string>();

        var items = new List<OperationalRunbook>();
        foreach (var file in files)
        {
            var json = await File.ReadAllTextAsync(file, cancellationToken);
            items.Add(_jsonSerializer.Deserialize<OperationalRunbook>(json));
        }

        return items
            .OrderBy(item => item.Title, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public async System.Threading.Tasks.Task<OperationalRunbook?> LoadAsync(string repositoryPath, OperationalRunbookId runbookId, CancellationToken cancellationToken)
    {
        var path = RepositoryPaths.Runbook(repositoryPath, runbookId);
        if (!File.Exists(path))
        {
            return null;
        }

        var json = await File.ReadAllTextAsync(path, cancellationToken);
        return _jsonSerializer.Deserialize<OperationalRunbook>(json);
    }
}
