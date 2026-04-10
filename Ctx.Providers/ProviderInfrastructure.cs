namespace Ctx.Providers;

using System.Text.Json;
using Ctx.Application;
using Ctx.Domain;

public sealed class AIProviderRegistry : IAIProviderRegistry
{
    private readonly Dictionary<string, IAIProvider> _providers;

    public AIProviderRegistry(IEnumerable<IAIProvider> providers)
    {
        _providers = providers.ToDictionary(provider => provider.Name, StringComparer.OrdinalIgnoreCase);
    }

    public IAIProvider Get(string providerName)
    {
        if (_providers.TryGetValue(providerName, out var provider))
        {
            return provider;
        }

        throw new InvalidOperationException($"Provider '{providerName}' is not registered. Available: {string.Join(", ", _providers.Keys)}");
    }

    public IReadOnlyCollection<string> List() => _providers.Keys.OrderBy(item => item).ToArray();
}

public abstract class HttpAiProviderBase : IAIProvider
{
    private readonly HttpClient _httpClient;

    protected HttpAiProviderBase(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public abstract string Name { get; }

    public async System.Threading.Tasks.Task<ProviderExecutionResult> ExecuteAsync(ContextPacket packet, ProviderExecutionRequest request, CancellationToken cancellationToken)
    {
        var apiKey = ResolveApiKey();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return BuildOfflineResult(packet, request);
        }

        using var message = BuildRequest(packet, request, apiKey);
        using var response = await _httpClient.SendAsync(message, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        response.EnsureSuccessStatusCode();
        return ParseResponse(body, request);
    }

    protected abstract string? ResolveApiKey();
    protected abstract HttpRequestMessage BuildRequest(ContextPacket packet, ProviderExecutionRequest request, string apiKey);
    protected abstract ProviderExecutionResult ParseResponse(string body, ProviderExecutionRequest request);

    protected virtual ProviderExecutionResult BuildOfflineResult(ContextPacket packet, ProviderExecutionRequest request)
        => new(
            Name,
            request.Model,
            $"Offline structured result for '{request.Purpose}'. Configure credentials to reach the live provider.",
            new[]
            {
                new RunArtifact("summary", "offline-result", $"Packet {packet.Id.Value} built with {packet.EstimatedTokens} estimated tokens.", Array.Empty<EntityReference>())
            },
            new TokenUsage(packet.EstimatedTokens, 120, 0.01m, TimeSpan.FromMilliseconds(50)));

    protected static string RenderPacket(ContextPacket packet)
        => string.Join(Environment.NewLine + Environment.NewLine, packet.Sections.Select(section => $"{section.Title}:{Environment.NewLine}{section.Content}"));
}
