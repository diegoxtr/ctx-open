namespace Ctx.Providers;

using System.Text;
using System.Text.Json;
using Ctx.Application;
using Ctx.Domain;

public sealed class AnthropicProvider : HttpAiProviderBase
{
    public AnthropicProvider(HttpClient httpClient) : base(httpClient)
    {
    }

    public override string Name => "anthropic";

    protected override string? ResolveApiKey() => Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");

    protected override HttpRequestMessage BuildRequest(ContextPacket packet, ProviderExecutionRequest request, string apiKey)
    {
        var payload = JsonSerializer.Serialize(new
        {
            model = request.Model,
            max_tokens = 1024,
            system = "You are a structured cognitive reasoning engine. Return explicit decisions, evidence, and next actions.",
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = $"Purpose: {request.Purpose}{Environment.NewLine}{Environment.NewLine}{RenderPacket(packet)}"
                }
            }
        });

        var message = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages");
        message.Headers.Add("x-api-key", apiKey);
        message.Headers.Add("anthropic-version", "2023-06-01");
        message.Content = new StringContent(payload, Encoding.UTF8, "application/json");
        return message;
    }

    protected override ProviderExecutionResult ParseResponse(string body, ProviderExecutionRequest request)
    {
        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;
        var summary = body;
        if (root.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.Array && content.GetArrayLength() > 0)
        {
            var first = content[0];
            if (first.TryGetProperty("text", out var text))
            {
                summary = text.GetString() ?? body;
            }
        }

        var inputTokens = root.TryGetProperty("usage", out var usage) && usage.TryGetProperty("input_tokens", out var input) ? input.GetInt32() : 0;
        var outputTokens = root.TryGetProperty("usage", out usage) && usage.TryGetProperty("output_tokens", out var output) ? output.GetInt32() : 0;

        return new ProviderExecutionResult(
            Name,
            request.Model,
            summary,
            new[] { new RunArtifact("provider-output", "anthropic-output", summary, Array.Empty<EntityReference>()) },
            new TokenUsage(inputTokens, outputTokens, 0m, TimeSpan.FromSeconds(1)));
    }
}
