namespace Ctx.Persistence;

using System.Collections.Concurrent;
using Ctx.Application;
using Ctx.Domain;

public sealed class FileSystemRepositoryWriteLock : IRepositoryWriteLock
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> InProcessLocks = new(StringComparer.OrdinalIgnoreCase);

    public async System.Threading.Tasks.Task<IAsyncDisposable> AcquireAsync(string repositoryPath, CancellationToken cancellationToken)
    {
        var rootPath = RepositoryPaths.Root(repositoryPath);
        Directory.CreateDirectory(rootPath);

        var gate = InProcessLocks.GetOrAdd(rootPath, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken);

        FileStream? lockStream = null;
        try
        {
            var lockPath = Path.Combine(rootPath, "write.lock");
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    lockStream = new FileStream(
                        lockPath,
                        FileMode.OpenOrCreate,
                        FileAccess.ReadWrite,
                        FileShare.None,
                        bufferSize: 1,
                        options: FileOptions.Asynchronous);
                    break;
                }
                catch (IOException)
                {
                    await System.Threading.Tasks.Task.Delay(25, cancellationToken);
                }
            }

            return new Releaser(gate, lockStream);
        }
        catch
        {
            gate.Release();
            lockStream?.Dispose();
            throw;
        }
    }

    private sealed class Releaser : IAsyncDisposable
    {
        private readonly SemaphoreSlim _gate;
        private readonly FileStream _lockStream;

        public Releaser(SemaphoreSlim gate, FileStream lockStream)
        {
            _gate = gate;
            _lockStream = lockStream;
        }

        public ValueTask DisposeAsync()
        {
            _lockStream.Dispose();
            _gate.Release();
            return ValueTask.CompletedTask;
        }
    }
}
