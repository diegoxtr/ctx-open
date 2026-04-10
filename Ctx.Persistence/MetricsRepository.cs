namespace Ctx.Persistence;

using System.Text;
using Ctx.Application;
using Ctx.Domain;

public sealed class FileSystemMetricsRepository : IMetricsRepository
{
    private readonly IJsonSerializer _jsonSerializer;

    public FileSystemMetricsRepository(IJsonSerializer jsonSerializer)
    {
        _jsonSerializer = jsonSerializer;
    }

    public async System.Threading.Tasks.Task<MetricsSnapshot> LoadAsync(string repositoryPath, CancellationToken cancellationToken)
    {
        var path = RepositoryPaths.MetricsState(repositoryPath);
        if (!File.Exists(path))
        {
            return new MetricsSnapshot(0, 0, 0m, 0, 0, TimeSpan.Zero);
        }

        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete);

        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        var json = await reader.ReadToEndAsync(cancellationToken);
        return _jsonSerializer.Deserialize<MetricsSnapshot>(json);
    }

    public async System.Threading.Tasks.Task SaveAsync(string repositoryPath, MetricsSnapshot snapshot, CancellationToken cancellationToken)
    {
        var path = RepositoryPaths.MetricsState(repositoryPath);
        var directory = Path.GetDirectoryName(path)
            ?? throw new InvalidOperationException("Metrics directory path could not be resolved.");
        Directory.CreateDirectory(directory);

        var tempPath = Path.Combine(directory, $"{Path.GetFileName(path)}.{Guid.NewGuid():N}.tmp");

        try
        {
            await File.WriteAllTextAsync(tempPath, _jsonSerializer.Serialize(snapshot), Encoding.UTF8, cancellationToken);

            if (File.Exists(path))
            {
                File.Copy(tempPath, path, overwrite: true);
            }
            else
            {
                File.Move(tempPath, path);
                tempPath = string.Empty;
            }
        }
        finally
        {
            if (!string.IsNullOrWhiteSpace(tempPath) && File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }
}
