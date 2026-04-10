namespace Ctx.Persistence;

using System.Text;
using Ctx.Application;
using Ctx.Domain;

internal static class RepositoryPaths
{
    private static readonly char[] InvalidBranchFileNameChars = Path.GetInvalidFileNameChars();

    public static string Root(string repositoryPath) => Path.Combine(repositoryPath, DomainConstants.RepositoryFolderName);
    public static string Version(string repositoryPath) => Path.Combine(Root(repositoryPath), "version.json");
    public static string Config(string repositoryPath) => Path.Combine(Root(repositoryPath), "config.json");
    public static string Project(string repositoryPath) => Path.Combine(Root(repositoryPath), "project.json");
    public static string Head(string repositoryPath) => Path.Combine(Root(repositoryPath), "HEAD");
    public static string Branches(string repositoryPath) => Path.Combine(Root(repositoryPath), "branches");
    public static string Commits(string repositoryPath) => Path.Combine(Root(repositoryPath), "commits");
    public static string Graph(string repositoryPath) => Path.Combine(Root(repositoryPath), "graph");
    public static string Working(string repositoryPath) => Path.Combine(Root(repositoryPath), "working");
    public static string Staging(string repositoryPath) => Path.Combine(Root(repositoryPath), "staging");
    public static string Runs(string repositoryPath) => Path.Combine(Root(repositoryPath), "runs");
    public static string Packets(string repositoryPath) => Path.Combine(Root(repositoryPath), "packets");
    public static string Index(string repositoryPath) => Path.Combine(Root(repositoryPath), "index");
    public static string Metrics(string repositoryPath) => Path.Combine(Root(repositoryPath), "metrics");
    public static string Providers(string repositoryPath) => Path.Combine(Root(repositoryPath), "providers");
    public static string Logs(string repositoryPath) => Path.Combine(Root(repositoryPath), "logs");
    public static string WorkingContext(string repositoryPath) => Path.Combine(Working(repositoryPath), "working-context.json");
    public static string StagingContext(string repositoryPath) => Path.Combine(Staging(repositoryPath), "staged-context.json");
    public static string GraphState(string repositoryPath) => Path.Combine(Graph(repositoryPath), "current-graph.json");
    public static string MetricsState(string repositoryPath) => Path.Combine(Metrics(repositoryPath), "usage.json");
    public static string Commit(string repositoryPath, ContextCommitId commitId) => Path.Combine(Commits(repositoryPath), $"{commitId.Value}.json");
    public static string Branch(string repositoryPath, string branchName) => Path.Combine(Branches(repositoryPath), $"{SanitizeBranchFileName(branchName)}.json");
    public static string Run(string repositoryPath, RunId runId) => Path.Combine(Runs(repositoryPath), $"{runId.Value}.json");
    public static string Packet(string repositoryPath, ContextPacketId packetId) => Path.Combine(Packets(repositoryPath), $"{packetId.Value}.json");

    private static string SanitizeBranchFileName(string branchName)
    {
        var builder = new StringBuilder(branchName.Length);
        foreach (var character in branchName)
        {
            if (character == '/' || character == '\\' || InvalidBranchFileNameChars.Contains(character))
            {
                builder.Append('_');
                continue;
            }

            builder.Append(character);
        }

        return builder.ToString();
    }
}

public sealed class FileSystemWorkingContextRepository : IWorkingContextRepository
{
    private readonly IJsonSerializer _jsonSerializer;

    public FileSystemWorkingContextRepository(IJsonSerializer jsonSerializer)
    {
        _jsonSerializer = jsonSerializer;
    }

    public System.Threading.Tasks.Task<bool> ExistsAsync(string repositoryPath, CancellationToken cancellationToken)
        => System.Threading.Tasks.Task.FromResult(
            File.Exists(RepositoryPaths.Version(repositoryPath))
            && File.Exists(RepositoryPaths.Config(repositoryPath))
            && File.Exists(RepositoryPaths.WorkingContext(repositoryPath)));

    public async System.Threading.Tasks.Task InitializeAsync(string repositoryPath, RepositoryVersion version, RepositoryConfig config, Project project, HeadReference head, BranchReference branch, WorkingContext context, CancellationToken cancellationToken)
    {
        CreateStructure(repositoryPath);
        await WriteAsync(RepositoryPaths.Version(repositoryPath), version, cancellationToken);
        await WriteAsync(RepositoryPaths.Config(repositoryPath), config, cancellationToken);
        await WriteAsync(RepositoryPaths.Project(repositoryPath), project, cancellationToken);
        await WriteTextAsync(RepositoryPaths.Head(repositoryPath), $"{head.Branch}:{head.CommitId?.Value ?? "null"}", cancellationToken);
        await WriteAsync(RepositoryPaths.Branch(repositoryPath, branch.Name), branch, cancellationToken);
        await SaveWorkingAsync(repositoryPath, context, cancellationToken);
        await SaveStagingAsync(repositoryPath, context, cancellationToken);
        await WriteAsync(RepositoryPaths.GraphState(repositoryPath), context.ToGraph(), cancellationToken);
        await WriteAsync(RepositoryPaths.MetricsState(repositoryPath), new MetricsSnapshot(0, 0, 0m, 0, 0, TimeSpan.Zero), cancellationToken);
        await WriteTextAsync(Path.Combine(RepositoryPaths.Index(repositoryPath), "README.txt"), "Reserved for lexical and graph indexes.", cancellationToken);
        await WriteTextAsync(Path.Combine(RepositoryPaths.Providers(repositoryPath), "README.txt"), "Provider-specific cached data and credentials references.", cancellationToken);
        await WriteTextAsync(Path.Combine(RepositoryPaths.Logs(repositoryPath), "README.txt"), "Operational logs.", cancellationToken);
    }

    public System.Threading.Tasks.Task<WorkingContext> LoadAsync(string repositoryPath, CancellationToken cancellationToken)
        => ReadAsync<WorkingContext>(RepositoryPaths.WorkingContext(repositoryPath), cancellationToken);

    public async System.Threading.Tasks.Task ImportAsync(string repositoryPath, RepositoryExport exportData, CancellationToken cancellationToken)
    {
        CreateStructure(repositoryPath);
        await WriteAsync(RepositoryPaths.Version(repositoryPath), exportData.RepositoryVersion, cancellationToken);
        await WriteAsync(RepositoryPaths.Config(repositoryPath), exportData.Config, cancellationToken);
        await WriteAsync(RepositoryPaths.Project(repositoryPath), exportData.WorkingContext.Project, cancellationToken);
        await SaveHeadAsync(repositoryPath, exportData.Head, cancellationToken);
        await SaveWorkingAsync(repositoryPath, exportData.WorkingContext, cancellationToken);
        await SaveStagingAsync(repositoryPath, exportData.WorkingContext, cancellationToken);
        await WriteAsync(RepositoryPaths.GraphState(repositoryPath), exportData.WorkingContext.ToGraph(), cancellationToken);
        await WriteAsync(RepositoryPaths.MetricsState(repositoryPath), exportData.Metrics, cancellationToken);
        await WriteTextAsync(Path.Combine(RepositoryPaths.Index(repositoryPath), "README.txt"), "Reserved for lexical and graph indexes.", cancellationToken);
        await WriteTextAsync(Path.Combine(RepositoryPaths.Providers(repositoryPath), "README.txt"), "Provider-specific cached data and credentials references.", cancellationToken);
        await WriteTextAsync(Path.Combine(RepositoryPaths.Logs(repositoryPath), "README.txt"), "Operational logs.", cancellationToken);
    }

    public async System.Threading.Tasks.Task SaveWorkingAsync(string repositoryPath, WorkingContext workingContext, CancellationToken cancellationToken)
    {
        await WriteAsync(RepositoryPaths.WorkingContext(repositoryPath), workingContext, cancellationToken);
        await WriteAsync(RepositoryPaths.GraphState(repositoryPath), workingContext.ToGraph(), cancellationToken);
    }

    public System.Threading.Tasks.Task SaveStagingAsync(string repositoryPath, WorkingContext stagingContext, CancellationToken cancellationToken)
        => WriteAsync(RepositoryPaths.StagingContext(repositoryPath), stagingContext, cancellationToken);

    public async System.Threading.Tasks.Task<HeadReference> LoadHeadAsync(string repositoryPath, CancellationToken cancellationToken)
    {
        var raw = await File.ReadAllTextAsync(RepositoryPaths.Head(repositoryPath), cancellationToken);
        var parts = raw.Split(':', 2, StringSplitOptions.TrimEntries);
        var commitId = parts.Length == 2 && parts[1] != "null" ? new ContextCommitId(parts[1]) : (ContextCommitId?)null;
        return new HeadReference(parts[0], commitId);
    }

    public System.Threading.Tasks.Task SaveHeadAsync(string repositoryPath, HeadReference head, CancellationToken cancellationToken)
        => WriteTextAsync(RepositoryPaths.Head(repositoryPath), $"{head.Branch}:{head.CommitId?.Value ?? "null"}", cancellationToken);

    public System.Threading.Tasks.Task<RepositoryConfig> LoadConfigAsync(string repositoryPath, CancellationToken cancellationToken)
        => ReadAsync<RepositoryConfig>(RepositoryPaths.Config(repositoryPath), cancellationToken);

    public System.Threading.Tasks.Task<RepositoryVersion> LoadVersionAsync(string repositoryPath, CancellationToken cancellationToken)
        => ReadAsync<RepositoryVersion>(RepositoryPaths.Version(repositoryPath), cancellationToken);

    private static void CreateStructure(string repositoryPath)
    {
        Directory.CreateDirectory(RepositoryPaths.Root(repositoryPath));
        Directory.CreateDirectory(RepositoryPaths.Branches(repositoryPath));
        Directory.CreateDirectory(RepositoryPaths.Commits(repositoryPath));
        Directory.CreateDirectory(RepositoryPaths.Graph(repositoryPath));
        Directory.CreateDirectory(RepositoryPaths.Working(repositoryPath));
        Directory.CreateDirectory(RepositoryPaths.Staging(repositoryPath));
        Directory.CreateDirectory(RepositoryPaths.Runs(repositoryPath));
        Directory.CreateDirectory(RepositoryPaths.Packets(repositoryPath));
        Directory.CreateDirectory(RepositoryPaths.Index(repositoryPath));
        Directory.CreateDirectory(RepositoryPaths.Metrics(repositoryPath));
        Directory.CreateDirectory(RepositoryPaths.Providers(repositoryPath));
        Directory.CreateDirectory(RepositoryPaths.Logs(repositoryPath));
    }

    private async System.Threading.Tasks.Task WriteAsync<T>(string path, T data, CancellationToken cancellationToken)
    {
        await File.WriteAllTextAsync(path, _jsonSerializer.Serialize(data), Encoding.UTF8, cancellationToken);
    }

    private async System.Threading.Tasks.Task<T> ReadAsync<T>(string path, CancellationToken cancellationToken)
    {
        var json = await File.ReadAllTextAsync(path, cancellationToken);
        return _jsonSerializer.Deserialize<T>(json);
    }

    private static System.Threading.Tasks.Task WriteTextAsync(string path, string content, CancellationToken cancellationToken)
        => File.WriteAllTextAsync(path, content, Encoding.UTF8, cancellationToken);
}
