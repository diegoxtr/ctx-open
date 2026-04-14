namespace Ctx.Persistence;

using Ctx.Application;
using Ctx.Domain;

public sealed class FileSystemCognitiveTriggerRepository : ICognitiveTriggerRepository
{
    private readonly IJsonSerializer _jsonSerializer;

    public FileSystemCognitiveTriggerRepository(IJsonSerializer jsonSerializer)
    {
        _jsonSerializer = jsonSerializer;
    }

    public System.Threading.Tasks.Task SaveAsync(string repositoryPath, CognitiveTrigger trigger, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(RepositoryPaths.Triggers(repositoryPath));
        return File.WriteAllTextAsync(RepositoryPaths.Trigger(repositoryPath, trigger.Id), _jsonSerializer.Serialize(trigger), cancellationToken);
    }

    public async System.Threading.Tasks.Task<IReadOnlyList<CognitiveTrigger>> ListAsync(string repositoryPath, CancellationToken cancellationToken)
    {
        var files = Directory.Exists(RepositoryPaths.Triggers(repositoryPath))
            ? Directory.GetFiles(RepositoryPaths.Triggers(repositoryPath), "*.json")
            : Array.Empty<string>();

        var items = new List<CognitiveTrigger>();
        foreach (var file in files)
        {
            var json = await File.ReadAllTextAsync(file, cancellationToken);
            items.Add(_jsonSerializer.Deserialize<CognitiveTrigger>(json));
        }

        return items
            .OrderBy(item => item.Trace.CreatedAtUtc)
            .ToArray();
    }

    public async System.Threading.Tasks.Task<CognitiveTrigger?> LoadAsync(string repositoryPath, CognitiveTriggerId triggerId, CancellationToken cancellationToken)
    {
        var path = RepositoryPaths.Trigger(repositoryPath, triggerId);
        if (!File.Exists(path))
        {
            return null;
        }

        var json = await File.ReadAllTextAsync(path, cancellationToken);
        return _jsonSerializer.Deserialize<CognitiveTrigger>(json);
    }
}
