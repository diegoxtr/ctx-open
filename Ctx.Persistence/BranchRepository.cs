namespace Ctx.Persistence;

using Ctx.Application;
using Ctx.Domain;

public sealed class FileSystemBranchRepository : IBranchRepository
{
    private readonly IJsonSerializer _jsonSerializer;

    public FileSystemBranchRepository(IJsonSerializer jsonSerializer)
    {
        _jsonSerializer = jsonSerializer;
    }

    public System.Threading.Tasks.Task SaveAsync(string repositoryPath, BranchReference branch, CancellationToken cancellationToken)
        => File.WriteAllTextAsync(RepositoryPaths.Branch(repositoryPath, branch.Name), _jsonSerializer.Serialize(branch), cancellationToken);

    public async System.Threading.Tasks.Task<BranchReference?> LoadAsync(string repositoryPath, string branchName, CancellationToken cancellationToken)
    {
        var path = RepositoryPaths.Branch(repositoryPath, branchName);
        if (!File.Exists(path))
        {
            return null;
        }

        var json = await File.ReadAllTextAsync(path, cancellationToken);
        return _jsonSerializer.Deserialize<BranchReference>(json);
    }

    public async System.Threading.Tasks.Task<IReadOnlyList<BranchReference>> ListAsync(string repositoryPath, CancellationToken cancellationToken)
    {
        var files = Directory.Exists(RepositoryPaths.Branches(repositoryPath))
            ? Directory.GetFiles(RepositoryPaths.Branches(repositoryPath), "*.json")
            : Array.Empty<string>();

        var branches = new List<BranchReference>();
        foreach (var file in files)
        {
            var json = await File.ReadAllTextAsync(file, cancellationToken);
            branches.Add(_jsonSerializer.Deserialize<BranchReference>(json));
        }

        return branches.OrderBy(branch => branch.Name).ToArray();
    }
}
