namespace Ctx.Providers;

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Ctx.Application;
using Ctx.Domain;

public sealed class OpenAiProvider : HttpAiProviderBase
{
    public OpenAiProvider(HttpClient httpClient) : base(httpClient)
    {
    }

    public override string Name => "openai";

    protected override string? ResolveApiKey() => Environment.GetEnvironmentVariable("OPENAI_API_KEY");

    protected override HttpRequestMessage BuildRequest(ContextPacket packet, ProviderExecutionRequest request, string apiKey)
    {
        var payload = JsonSerializer.Serialize(new
        {
            model = request.Model,
            input = new object[]
            {
                new
                {
                    role = "system",
                    content = new object[]
                    {
                        new
                        {
                            type = "input_text",
                            text = "You are a structured cognitive reasoning engine. Return concise structured output with explicit decisions, evidence, and next actions."
                        }
                    }
                },
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new
                        {
                            type = "input_text",
                            text = $"Purpose: {request.Purpose}{Environment.NewLine}{Environment.NewLine}{RenderPacket(packet)}"
                        }
                    }
                }
            }
        });

        var message = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/responses");
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        message.Content = new StringContent(payload, Encoding.UTF8, "application/json");
        return message;
    }

    protected override ProviderExecutionResult ParseResponse(string body, ProviderExecutionRequest request)
    {
        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;
        var outputText = root.TryGetProperty("output_text", out var textNode)
            ? textNode.GetString()
            : body;

        var inputTokens = root.TryGetProperty("usage", out var usage) && usage.TryGetProperty("input_tokens", out var input) ? input.GetInt32() : 0;
        var outputTokens = root.TryGetProperty("usage", out usage) && usage.TryGetProperty("output_tokens", out var output) ? output.GetInt32() : 0;

        return new ProviderExecutionResult(
            Name,
            request.Model,
            outputText ?? string.Empty,
            new[] { new RunArtifact("provider-output", "openai-output", outputText ?? string.Empty, Array.Empty<EntityReference>()) },
            new TokenUsage(inputTokens, outputTokens, 0m, TimeSpan.FromSeconds(1)));
    }
}
