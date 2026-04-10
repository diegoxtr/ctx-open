namespace Ctx.Core;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Ctx.Application;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}

public sealed class Sha256HashingService : IHashingService
{
    public string Hash(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}

public sealed class DefaultJsonSerializer : IJsonSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public string Serialize<T>(T value) => JsonSerializer.Serialize(value, Options);

    public T Deserialize<T>(string json) =>
        JsonSerializer.Deserialize<T>(json, Options)
        ?? throw new InvalidOperationException($"Unable to deserialize {typeof(T).Name}.");
}
