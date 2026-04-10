namespace Ctx.Domain;

public record Traceability(
    string CreatedBy,
    DateTimeOffset CreatedAtUtc,
    string? UpdatedBy,
    DateTimeOffset? UpdatedAtUtc,
    IReadOnlyList<string> Tags,
    IReadOnlyList<string> RelatedIds,
    string? ModelName = null,
    string? ModelVersion = null);

public abstract record CognitiveEntity<TId>(TId Id, Traceability Trace);

public record TokenUsage(
    int InputTokens,
    int OutputTokens,
    decimal AcuCost,
    TimeSpan Duration)
{
    public int TotalTokens => InputTokens + OutputTokens;
}

public record EntityReference(string EntityType, string EntityId);

public record ContentSection(string Title, string Content, IReadOnlyList<EntityReference> References);
