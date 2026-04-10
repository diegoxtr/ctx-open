namespace Ctx.Persistence;

using Ctx.Application;
using Ctx.Domain;

public sealed class FileSystemPacketRepository : IPacketRepository
{
    private readonly IJsonSerializer _jsonSerializer;

    public FileSystemPacketRepository(IJsonSerializer jsonSerializer)
    {
        _jsonSerializer = jsonSerializer;
    }

    public System.Threading.Tasks.Task SaveAsync(string repositoryPath, ContextPacket packet, CancellationToken cancellationToken)
        => File.WriteAllTextAsync(RepositoryPaths.Packet(repositoryPath, packet.Id), _jsonSerializer.Serialize(packet), cancellationToken);

    public async System.Threading.Tasks.Task<IReadOnlyList<ContextPacket>> ListAsync(string repositoryPath, CancellationToken cancellationToken)
    {
        var files = Directory.Exists(RepositoryPaths.Packets(repositoryPath))
            ? Directory.GetFiles(RepositoryPaths.Packets(repositoryPath), "*.json")
            : Array.Empty<string>();
        var items = new List<ContextPacket>();
        foreach (var file in files)
        {
            var json = await File.ReadAllTextAsync(file, cancellationToken);
            items.Add(_jsonSerializer.Deserialize<ContextPacket>(json));
        }

        return items.OrderByDescending(item => item.CreatedAtUtc).ToArray();
    }

    public async System.Threading.Tasks.Task<ContextPacket?> LoadAsync(string repositoryPath, ContextPacketId packetId, CancellationToken cancellationToken)
    {
        var path = RepositoryPaths.Packet(repositoryPath, packetId);
        if (!File.Exists(path))
        {
            return null;
        }

        var json = await File.ReadAllTextAsync(path, cancellationToken);
        return _jsonSerializer.Deserialize<ContextPacket>(json);
    }
}
