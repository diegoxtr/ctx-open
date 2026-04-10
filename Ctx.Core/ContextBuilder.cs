namespace Ctx.Core;

using Ctx.Application;
using Ctx.Domain;

public sealed class ContextBuilder : IContextBuilder
{
    private readonly IClock _clock;
    private readonly IHashingService _hashingService;

    public ContextBuilder(IClock clock, IHashingService hashingService)
    {
        _clock = clock;
        _hashingService = hashingService;
    }

    public ContextPacket Build(WorkingContext context, string purpose, string? goalId = null, string? taskId = null)
    {
        var selectedGoals = context.Goals
            .Where(goal => goalId is null || goal.Id.Value.Equals(goalId, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(goal => goal.Priority)
            .Take(3)
            .ToList();

        var selectedTasks = context.Tasks
            .Where(task => taskId is null || task.Id.Value.Equals(taskId, StringComparison.OrdinalIgnoreCase))
            .Where(task => goalId is null || task.GoalId?.Value.Equals(goalId, StringComparison.OrdinalIgnoreCase) == true || taskId is not null)
            .OrderBy(task => task.State)
            .ThenBy(task => task.Title)
            .Take(6)
            .ToList();

        if (!selectedGoals.Any() && context.Goals.Any())
        {
            selectedGoals = context.Goals.OrderByDescending(goal => goal.Priority).Take(2).ToList();
        }

        if (!selectedTasks.Any() && context.Tasks.Any())
        {
            selectedTasks = context.Tasks
                .Where(task => selectedGoals.Count == 0 || selectedGoals.Any(goal => goal.Id.Equals(task.GoalId)))
                .Take(4)
                .ToList();
        }

        var relatedHypotheses = context.Hypotheses
            .Where(hypothesis => selectedTasks.Any(task => hypothesis.TaskIds.Contains(task.Id)))
            .OrderByDescending(hypothesis => hypothesis.Confidence)
            .Take(6)
            .ToList();

        var relatedDecisions = context.Decisions
            .Where(decision => relatedHypotheses.Any(hypothesis => decision.HypothesisIds.Contains(hypothesis.Id)))
            .Take(6)
            .ToList();

        var relatedEvidence = context.Evidence
            .Where(evidence =>
                evidence.Supports.Any(reference =>
                    relatedHypotheses.Any(h => h.Id.Value == reference.EntityId) ||
                    relatedDecisions.Any(d => d.Id.Value == reference.EntityId)))
            .OrderByDescending(evidence => evidence.Confidence)
            .Take(8)
            .ToList();

        var relatedConclusions = context.Conclusions
            .Where(conclusion => conclusion.DecisionIds.Any(id => relatedDecisions.Any(decision => decision.Id.Equals(id))))
            .Take(4)
            .ToList();

        var sections = new List<ContentSection>
        {
            new("Purpose", purpose, Array.Empty<EntityReference>()),
            new("Project", $"{context.Project.Name}: {context.Project.Description}", new[] { new EntityReference(nameof(Project), context.Project.Id.Value) }),
            new("Goals", string.Join(Environment.NewLine, selectedGoals.Select(goal => $"- [{goal.State}] {goal.Title}: {goal.Description}")), selectedGoals.Select(goal => new EntityReference(nameof(Goal), goal.Id.Value)).ToArray()),
            new("Tasks", string.Join(Environment.NewLine, selectedTasks.Select(task => $"- [{task.State}] {task.Title}: {task.Description}")), selectedTasks.Select(task => new EntityReference("Task", task.Id.Value)).ToArray()),
            new("Hypotheses", string.Join(Environment.NewLine, relatedHypotheses.Select(hypothesis => $"- [{hypothesis.State}] ({hypothesis.Confidence:P0}) {hypothesis.Statement}")), relatedHypotheses.Select(hypothesis => new EntityReference(nameof(Hypothesis), hypothesis.Id.Value)).ToArray()),
            new("Decisions", string.Join(Environment.NewLine, relatedDecisions.Select(decision => $"- [{decision.State}] {decision.Title}: {decision.Rationale}")), relatedDecisions.Select(decision => new EntityReference(nameof(Decision), decision.Id.Value)).ToArray()),
            new("Evidence", string.Join(Environment.NewLine, relatedEvidence.Select(evidence => $"- [{evidence.Kind}] ({evidence.Confidence:P0}) {evidence.Title}: {evidence.Summary}")), relatedEvidence.Select(evidence => new EntityReference(nameof(Evidence), evidence.Id.Value)).ToArray()),
            new("Conclusions", string.Join(Environment.NewLine, relatedConclusions.Select(conclusion => $"- [{conclusion.State}] {conclusion.Summary}")), relatedConclusions.Select(conclusion => new EntityReference(nameof(Conclusion), conclusion.Id.Value)).ToArray())
        };

        var normalizedSections = sections.Where(section => !string.IsNullOrWhiteSpace(section.Content)).ToList();
        var fingerprintMaterial = string.Join("|", normalizedSections.Select(section => $"{section.Title}:{section.Content}"));

        return new ContextPacket(
            ContextPacketId.New(),
            context.Project.Id,
            _clock.UtcNow,
            purpose,
            _hashingService.Hash(fingerprintMaterial),
            EstimateTokens(normalizedSections),
            selectedGoals.Select(goal => goal.Id).ToArray(),
            selectedTasks.Select(task => task.Id).ToArray(),
            relatedHypotheses.Select(hypothesis => hypothesis.Id).ToArray(),
            relatedDecisions.Select(decision => decision.Id).ToArray(),
            relatedEvidence.Select(evidence => evidence.Id).ToArray(),
            relatedConclusions.Select(conclusion => conclusion.Id).ToArray(),
            normalizedSections);
    }

    private static int EstimateTokens(IEnumerable<ContentSection> sections)
        => sections.Sum(section => Math.Max(1, section.Content.Length / 4));
}
