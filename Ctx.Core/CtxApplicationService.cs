namespace Ctx.Core;

using Ctx.Application;
using Ctx.Domain;
using System.Globalization;
using System.Text.Json;
using System.Text;

public sealed class CtxApplicationService : ICtxApplicationService
{
    private readonly IWorkingContextRepository _workingContextRepository;
    private readonly ICommitRepository _commitRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly IRunRepository _runRepository;
    private readonly IPacketRepository _packetRepository;
    private readonly IMetricsRepository _metricsRepository;
    private readonly IRunOrchestrator _runOrchestrator;
    private readonly IContextBuilder _contextBuilder;
    private readonly ICommitEngine _commitEngine;
    private readonly IMergeEngine _mergeEngine;
    private readonly IClock _clock;
    private readonly IHashingService _hashingService;
    private readonly IRepositoryWriteLock? _repositoryWriteLock;
    private readonly IOperationalRunbookRepository? _operationalRunbookRepository;
    private readonly ICognitiveTriggerRepository? _cognitiveTriggerRepository;

    public CtxApplicationService(
        IWorkingContextRepository workingContextRepository,
        ICommitRepository commitRepository,
        IBranchRepository branchRepository,
        IRunRepository runRepository,
        IPacketRepository packetRepository,
        IMetricsRepository metricsRepository,
        IRunOrchestrator runOrchestrator,
        IContextBuilder contextBuilder,
        ICommitEngine commitEngine,
        IMergeEngine mergeEngine,
        IClock clock,
        IHashingService hashingService,
        IRepositoryWriteLock? repositoryWriteLock = null,
        IOperationalRunbookRepository? operationalRunbookRepository = null,
        ICognitiveTriggerRepository? cognitiveTriggerRepository = null)
    {
        _workingContextRepository = workingContextRepository;
        _commitRepository = commitRepository;
        _branchRepository = branchRepository;
        _runRepository = runRepository;
        _packetRepository = packetRepository;
        _metricsRepository = metricsRepository;
        _runOrchestrator = runOrchestrator;
        _contextBuilder = contextBuilder;
        _commitEngine = commitEngine;
        _mergeEngine = mergeEngine;
        _clock = clock;
        _hashingService = hashingService;
        _repositoryWriteLock = repositoryWriteLock;
        _operationalRunbookRepository = operationalRunbookRepository;
        _cognitiveTriggerRepository = cognitiveTriggerRepository;
    }

    public CtxApplicationService(
        IWorkingContextRepository workingContextRepository,
        ICommitRepository commitRepository,
        IBranchRepository branchRepository,
        IRunRepository runRepository,
        IPacketRepository packetRepository,
        IMetricsRepository metricsRepository,
        IRunOrchestrator runOrchestrator,
        IContextBuilder contextBuilder,
        ICommitEngine commitEngine,
        IMergeEngine mergeEngine,
        IClock clock,
        IRepositoryWriteLock? repositoryWriteLock = null,
        IOperationalRunbookRepository? operationalRunbookRepository = null,
        ICognitiveTriggerRepository? cognitiveTriggerRepository = null)
        : this(
            workingContextRepository,
            commitRepository,
            branchRepository,
            runRepository,
            packetRepository,
            metricsRepository,
            runOrchestrator,
            contextBuilder,
            commitEngine,
            mergeEngine,
            clock,
            new Sha256HashingService(),
            repositoryWriteLock,
            operationalRunbookRepository,
            cognitiveTriggerRepository)
    {
    }

    public async System.Threading.Tasks.Task<CommandResult> InitAsync(string repositoryPath, InitRepositoryRequest request, CancellationToken cancellationToken)
    {
        return await ExecuteWriteLockedAsync(repositoryPath, cancellationToken, async () =>
        {
            if (await _workingContextRepository.ExistsAsync(repositoryPath, cancellationToken))
            {
                return new CommandResult(false, "CTX repository already exists.");
            }

            var trace = NewTrace(request.CreatedBy, "project");
            var project = new Project(ProjectId.New(), request.ProjectName, request.Description, request.Branch, LifecycleState.Active, trace);
            var workingContext = new WorkingContext(
                WorkingContextId.New(),
                DomainConstants.CurrentRepositoryVersion,
                request.Branch,
                null,
                false,
                project,
                Array.Empty<Goal>(),
                Array.Empty<Ctx.Domain.Task>(),
                Array.Empty<Hypothesis>(),
                Array.Empty<Decision>(),
                Array.Empty<Evidence>(),
                Array.Empty<Conclusion>(),
                Array.Empty<Run>(),
                trace);

            var config = new RepositoryConfig(
                "openai",
                new[]
                {
                    new ProviderConfiguration("openai", "gpt-4.1", "https://api.openai.com/v1/responses", true),
                    new ProviderConfiguration("anthropic", "claude-3-7-sonnet-latest", "https://api.anthropic.com/v1/messages", true)
                },
                16000,
                true);

            await _workingContextRepository.InitializeAsync(
                repositoryPath,
                new RepositoryVersion(DomainConstants.CurrentRepositoryVersion, _clock.UtcNow),
                config,
                project,
                new HeadReference(request.Branch, null),
                new BranchReference(request.Branch, null, _clock.UtcNow),
                workingContext,
                cancellationToken);

            return new CommandResult(true, $"Initialized empty cognitive repository in {Path.Combine(repositoryPath, DomainConstants.RepositoryFolderName)}", workingContext);
        });
    }

    public async System.Threading.Tasks.Task<CommandResult> StatusAsync(string repositoryPath, CancellationToken cancellationToken)
    {
        var context = await _workingContextRepository.LoadAsync(repositoryPath, cancellationToken);
        var runbooks = await LoadRunbooksAsync(repositoryPath, cancellationToken);
        var triggers = await LoadTriggersAsync(repositoryPath, cancellationToken);
        var head = await _workingContextRepository.LoadHeadAsync(repositoryPath, cancellationToken);
        var previous = head.CommitId is null
            ? null
            : await _commitRepository.LoadAsync(repositoryPath, head.CommitId.Value, cancellationToken);
        var pending = BuildStatusPendingSummary(previous?.Snapshot, new RepositorySnapshot(context, runbooks, triggers));
        var summary = new StatusSummary(
            head.Branch,
            head.CommitId?.Value,
            context.Dirty,
            context.Goals.Count,
            context.Tasks.Count,
            context.Hypotheses.Count,
            context.Decisions.Count,
            context.Evidence.Count,
            context.Conclusions.Count,
            context.Runs.Count,
            pending);
        var message = context.Dirty
            ? $"On branch {head.Branch} with pending cognitive changes"
            : $"On branch {head.Branch}";
        return new(true, message, summary);
    }

    public async System.Threading.Tasks.Task<CommandResult> CheckAsync(string repositoryPath, string? taskId, CancellationToken cancellationToken)
    {
        var context = await _workingContextRepository.LoadAsync(repositoryPath, cancellationToken);
        var (task, selectionReason) = ResolveCheckTask(context, taskId);
        var runbooks = await LoadRunbooksAsync(repositoryPath, cancellationToken);
        var summary = BuildBlockCheckSummary(context, task, selectionReason, runbooks);
        var message = summary.ReadyForCommit
            ? $"Task '{summary.TaskTitle}' is ready for cognitive commit."
            : $"Task '{summary.TaskTitle}' still has block-level closure gaps.";

        return new(true, message, summary);
    }

    public async System.Threading.Tasks.Task<CommandResult> CloseoutAsync(string repositoryPath, CancellationToken cancellationToken)
    {
        var context = await _workingContextRepository.LoadAsync(repositoryPath, cancellationToken);
        var runbooks = await LoadRunbooksAsync(repositoryPath, cancellationToken);
        var triggers = await LoadTriggersAsync(repositoryPath, cancellationToken);
        var head = await _workingContextRepository.LoadHeadAsync(repositoryPath, cancellationToken);
        var previous = head.CommitId is null
            ? null
            : await _commitRepository.LoadAsync(repositoryPath, head.CommitId.Value, cancellationToken);
        var diff = BuildCloseoutDiff(previous?.Snapshot, new RepositorySnapshot(context, runbooks, triggers));
        var hasPendingChanges = context.Dirty;
        var pendingItems = hasPendingChanges
            ? EnumerateCloseoutPendingItems(diff)
            : Array.Empty<CloseoutPendingItem>();
        var diffSummary = hasPendingChanges ? BuildDiffSummary(diff) : "working matches HEAD";
        var guidance = BuildCloseoutGuidance(context, diff, pendingItems);
        var microCloseout = BuildMicroCloseoutSuggestion(diff, pendingItems);
        var closeout = new CloseoutSummary(
            head.Branch,
            head.CommitId?.Value,
            context.Dirty,
            hasPendingChanges,
            diffSummary,
            pendingItems,
            guidance,
            microCloseout);

        var message = hasPendingChanges
            ? $"Closeout review for branch {head.Branch}: {diffSummary}"
            : $"Closeout review for branch {head.Branch}: working matches HEAD";

        return new(true, message, closeout);
    }

    public async System.Threading.Tasks.Task<CommandResult> AddGoalAsync(string repositoryPath, AddGoalRequest request, CancellationToken cancellationToken)
    {
        return await ExecuteWriteLockedAsync(repositoryPath, cancellationToken, async () =>
        {
            var context = await _workingContextRepository.LoadAsync(repositoryPath, cancellationToken);
            Goal? parentGoal = null;
            if (!string.IsNullOrWhiteSpace(request.ParentGoalId))
            {
                parentGoal = context.Goals.SingleOrDefault(item => item.Id.Value.Equals(request.ParentGoalId, StringComparison.OrdinalIgnoreCase))
                    ?? throw new InvalidOperationException($"Goal '{request.ParentGoalId}' was not found.");
            }

            var goal = new Goal(GoalId.New(), parentGoal?.Id, request.Title, request.Description, request.Priority, LifecycleState.Active, NewTrace(request.CreatedBy, "goal"), Array.Empty<TaskId>());
            context = context with
            {
                Dirty = true,
                Goals = context.Goals.Append(goal).ToArray(),
                Trace = context.Trace with { UpdatedAtUtc = _clock.UtcNow, UpdatedBy = request.CreatedBy }
            };
            await _workingContextRepository.SaveWorkingAsync(repositoryPath, context, cancellationToken);
            await TryCreateAutomaticTriggerAsync(
                repositoryPath,
                CognitiveTriggerKind.AgentPrompt,
                $"Open goal line: {goal.Title}",
                goal.Description,
                new[] { goal.Id },
                Array.Empty<TaskId>(),
                request.CreatedBy,
                cancellationToken);
            return new CommandResult(true, $"Goal added: {goal.Title}", goal);
        });
    }

    public async System.Threading.Tasks.Task<CommandResult> OpenWorkLineAsync(string repositoryPath, OpenWorkLineRequest request, CancellationToken cancellationToken)
    {
        return await ExecuteWriteLockedAsync(repositoryPath, cancellationToken, async () =>
        {
            var context = await _workingContextRepository.LoadAsync(repositoryPath, cancellationToken);
            var parentGoal = ResolveGoal(context, request.ParentGoalId);
            var priority = request.Priority ?? parentGoal.Priority;
            var goal = new Goal(
                GoalId.New(),
                parentGoal.Id,
                request.Title,
                request.Description,
                priority,
                LifecycleState.Active,
                NewTrace(request.CreatedBy, "goal"),
                Array.Empty<TaskId>());

            Ctx.Domain.Task? seedTask = null;
            var goals = context.Goals.Append(goal).ToArray();

            if (!string.IsNullOrWhiteSpace(request.TaskTitle))
            {
                seedTask = new Ctx.Domain.Task(
                    TaskId.New(),
                    goal.Id,
                    request.TaskTitle,
                    request.TaskDescription ?? string.Empty,
                    TaskExecutionState.Ready,
                    NewTrace(request.CreatedBy, "task"),
                    Array.Empty<TaskId>(),
                    Array.Empty<HypothesisId>());
                goals = goals
                    .Select(item => item.Id == goal.Id ? item with { TaskIds = item.TaskIds.Append(seedTask.Id).ToArray() } : item)
                    .ToArray();
            }

            context = context with
            {
                Dirty = true,
                Goals = goals,
                Tasks = seedTask is null ? context.Tasks : context.Tasks.Append(seedTask).ToArray(),
                Trace = context.Trace with { UpdatedAtUtc = _clock.UtcNow, UpdatedBy = request.CreatedBy }
            };
            await _workingContextRepository.SaveWorkingAsync(repositoryPath, context, cancellationToken);
            await TryCreateAutomaticTriggerAsync(
                repositoryPath,
                CognitiveTriggerKind.AgentPrompt,
                $"Open tactical line: {goal.Title}",
                string.IsNullOrWhiteSpace(goal.Description) ? request.TaskTitle : goal.Description,
                new[] { goal.Id },
                seedTask is null ? Array.Empty<TaskId>() : new[] { seedTask.Id },
                request.CreatedBy,
                cancellationToken);

            var summary = new OpenWorkLineSummary(
                parentGoal.Id.Value,
                parentGoal.Title,
                goal.Id.Value,
                goal.Title,
                seedTask?.Id.Value,
                seedTask?.Title);
            var message = seedTask is null
                ? $"Opened tactical work line '{goal.Title}' under '{parentGoal.Title}'."
                : $"Opened tactical work line '{goal.Title}' and seeded task '{seedTask.Title}'.";
            return new CommandResult(true, message, summary);
        });
    }

    public async System.Threading.Tasks.Task<CommandResult> AddOperationalRunbookAsync(string repositoryPath, AddOperationalRunbookRequest request, CancellationToken cancellationToken)
    {
        if (_operationalRunbookRepository is null)
        {
            throw new InvalidOperationException("Operational runbooks are not configured.");
        }

        return await ExecuteWriteLockedAsync(repositoryPath, cancellationToken, async () =>
        {
            var context = await _workingContextRepository.LoadAsync(repositoryPath, cancellationToken);
            var goalIds = request.GoalIds.Select(id => ResolveGoal(context, id).Id).ToArray();
            var taskIds = request.TaskIds.Select(id => ResolveTask(context, id).Id).ToArray();
            var kind = Enum.TryParse<OperationalRunbookKind>(request.Kind, true, out var parsedKind)
                ? parsedKind
                : throw new InvalidOperationException($"Runbook kind '{request.Kind}' is not supported.");

            var runbook = new OperationalRunbook(
                OperationalRunbookId.New(),
                request.Title,
                kind,
                request.Triggers.Where(item => !string.IsNullOrWhiteSpace(item)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
                request.WhenToUse,
                request.Do.Where(item => !string.IsNullOrWhiteSpace(item)).ToArray(),
                request.Verify.Where(item => !string.IsNullOrWhiteSpace(item)).ToArray(),
                request.References.Where(item => !string.IsNullOrWhiteSpace(item)).ToArray(),
                goalIds,
                taskIds,
                LifecycleState.Active,
                NewTrace(request.CreatedBy, "runbook"));

            await _operationalRunbookRepository.SaveAsync(repositoryPath, runbook, cancellationToken);
            return new CommandResult(true, $"OperationalRunbook added: {runbook.Title}", runbook);
        });
    }

    public async System.Threading.Tasks.Task<CommandResult> AddCognitiveTriggerAsync(string repositoryPath, AddCognitiveTriggerRequest request, CancellationToken cancellationToken)
    {
        if (_cognitiveTriggerRepository is null)
        {
            throw new InvalidOperationException("Cognitive triggers are not configured.");
        }

        return await ExecuteWriteLockedAsync(repositoryPath, cancellationToken, async () =>
        {
            var context = await _workingContextRepository.LoadAsync(repositoryPath, cancellationToken);
            var runbooks = await LoadRunbooksAsync(repositoryPath, cancellationToken);
            var goalIds = request.GoalIds.Select(id => ResolveGoal(context, id).Id).Distinct().ToArray();
            var taskIds = request.TaskIds.Select(id => ResolveTask(context, id).Id).Distinct().ToArray();
            var runbookIds = request.RunbookIds
                .Select(id => runbooks.SingleOrDefault(item => item.Id.Value.Equals(id, StringComparison.OrdinalIgnoreCase))?.Id
                    ?? throw new InvalidOperationException($"OperationalRunbook '{id}' was not found."))
                .Distinct()
                .ToArray();
            var kind = Enum.TryParse<CognitiveTriggerKind>(request.Kind, true, out var parsedKind)
                ? parsedKind
                : throw new InvalidOperationException($"Cognitive trigger kind '{request.Kind}' is not supported.");
            var summary = request.Summary.Trim();
            var text = string.IsNullOrWhiteSpace(request.Text) ? null : request.Text.Trim();

            var trigger = new CognitiveTrigger(
                CognitiveTriggerId.New(),
                kind,
                summary,
                text,
                _hashingService.Hash(text ?? summary),
                goalIds,
                taskIds,
                runbookIds,
                LifecycleState.Active,
                NewTrace(request.CreatedBy, "trigger"));

            await _cognitiveTriggerRepository.SaveAsync(repositoryPath, trigger, cancellationToken);
            return new CommandResult(true, $"CognitiveTrigger added: {trigger.Summary}", trigger);
        });
    }

    public async System.Threading.Tasks.Task<CommandResult> AddTaskAsync(string repositoryPath, AddTaskRequest request, CancellationToken cancellationToken)
    {
        return await ExecuteWriteLockedAsync(repositoryPath, cancellationToken, async () =>
        {
            var context = await _workingContextRepository.LoadAsync(repositoryPath, cancellationToken);
            Ctx.Domain.Task? parentTask = null;
            if (!string.IsNullOrWhiteSpace(request.ParentTaskId))
            {
                parentTask = ResolveTask(context, request.ParentTaskId);
            }

            Goal? goal = null;
            if (!string.IsNullOrWhiteSpace(request.GoalId))
            {
                goal = context.Goals.SingleOrDefault(item => item.Id.Value.Equals(request.GoalId, StringComparison.OrdinalIgnoreCase))
                    ?? throw new InvalidOperationException($"Goal '{request.GoalId}' was not found.");
            }

            if (parentTask is not null)
            {
                if (goal is not null && goal.Id != parentTask.GoalId)
                {
                    throw new InvalidOperationException("A subtask cannot target a different goal than its parent task.");
                }

                if (goal is null && parentTask.GoalId is not null)
                {
                    goal = context.Goals.Single(item => item.Id == parentTask.GoalId);
                }
            }

            var dependsOnTaskIds = request.DependsOnTaskIds
                .Select(taskId => ResolveTask(context, taskId).Id)
                .Distinct()
                .ToArray();

            var task = new Ctx.Domain.Task(
                TaskId.New(),
                goal?.Id,
                request.Title,
                request.Description,
                TaskExecutionState.Ready,
                NewTrace(request.CreatedBy, "task"),
                dependsOnTaskIds,
                Array.Empty<HypothesisId>(),
                parentTask?.Id);
            var goals = context.Goals.Select(item => item.Id.Equals(goal?.Id) ? item with { TaskIds = item.TaskIds.Append(task.Id).ToArray() } : item).ToArray();
            context = context with
            {
                Dirty = true,
                Goals = goals,
                Tasks = context.Tasks.Append(task).ToArray(),
                Trace = context.Trace with { UpdatedAtUtc = _clock.UtcNow, UpdatedBy = request.CreatedBy }
            };
            await _workingContextRepository.SaveWorkingAsync(repositoryPath, context, cancellationToken);
            if (ShouldCreateAutomaticTaskTrigger(task, parentTask, dependsOnTaskIds))
            {
                await TryCreateAutomaticTriggerAsync(
                    repositoryPath,
                    CognitiveTriggerKind.AgentPrompt,
                    $"Open task line: {task.Title}",
                    task.Description,
                    goal is null ? Array.Empty<GoalId>() : new[] { goal.Id },
                    new[] { task.Id },
                    request.CreatedBy,
                    cancellationToken);
            }

            return new CommandResult(true, $"Task added: {task.Title}", task);
        });
    }

    public async System.Threading.Tasks.Task<CommandResult> UpdateTaskAsync(string repositoryPath, UpdateTaskRequest request, CancellationToken cancellationToken)
    {
        return await ExecuteWriteLockedAsync(repositoryPath, cancellationToken, async () =>
        {
            var context = await _workingContextRepository.LoadAsync(repositoryPath, cancellationToken);
            var task = context.Tasks.SingleOrDefault(item => item.Id.Value.Equals(request.TaskId, StringComparison.OrdinalIgnoreCase));

            if (task is null)
            {
                return new CommandResult(false, $"Task not found: {request.TaskId}");
            }

            var parsedState = task.State;
            if (!string.IsNullOrWhiteSpace(request.State)
                && !Enum.TryParse<TaskExecutionState>(request.State, true, out parsedState))
            {
                throw new InvalidOperationException($"Unsupported task state '{request.State}'.");
            }

            var updated = task with
            {
                Title = request.Title ?? task.Title,
                Description = request.Description ?? task.Description,
                State = parsedState,
                Trace = task.Trace with
                {
                    UpdatedBy = request.UpdatedBy,
                    UpdatedAtUtc = _clock.UtcNow
                }
            };

            context = context with
            {
                Dirty = true,
                Tasks = context.Tasks.Select(item => item.Id == task.Id ? updated : item).ToArray(),
                Trace = context.Trace with { UpdatedAtUtc = _clock.UtcNow, UpdatedBy = request.UpdatedBy }
            };

            await _workingContextRepository.SaveWorkingAsync(repositoryPath, context, cancellationToken);
            return new CommandResult(true, $"Task updated: {updated.Title}", updated);
        });
    }

    public async System.Threading.Tasks.Task<CommandResult> AddHypothesisAsync(string repositoryPath, AddHypothesisRequest request, CancellationToken cancellationToken)
    {
        return await ExecuteWriteLockedAsync(repositoryPath, cancellationToken, async () =>
        {
            var context = await _workingContextRepository.LoadAsync(repositoryPath, cancellationToken);
            Ctx.Domain.Task? task = null;
            if (!string.IsNullOrWhiteSpace(request.TaskId))
            {
                task = context.Tasks.SingleOrDefault(item => item.Id.Value.Equals(request.TaskId, StringComparison.OrdinalIgnoreCase))
                    ?? throw new InvalidOperationException($"Task '{request.TaskId}' was not found.");
            }

            var hypothesis = new Hypothesis(
                HypothesisId.New(),
                request.Statement,
                request.Rationale,
                request.Confidence,
                request.Impact,
                request.EvidenceStrength,
                request.CostToValidate,
                HypothesisState.Proposed,
                NewTrace(request.CreatedBy, "hypothesis"),
                task is null ? Array.Empty<TaskId>() : new[] { task.Id },
                Array.Empty<EvidenceId>());
            var tasks = context.Tasks.Select(item => item.Id.Equals(task?.Id) ? item with { HypothesisIds = item.HypothesisIds.Append(hypothesis.Id).ToArray() } : item).ToArray();
            context = context with
            {
                Dirty = true,
                Tasks = tasks,
                Hypotheses = context.Hypotheses.Append(hypothesis).ToArray(),
                Trace = context.Trace with { UpdatedAtUtc = _clock.UtcNow, UpdatedBy = request.CreatedBy }
            };
            await _workingContextRepository.SaveWorkingAsync(repositoryPath, context, cancellationToken);
            return new CommandResult(true, $"Hypothesis added: {hypothesis.Statement}", hypothesis);
        });
    }

    public async System.Threading.Tasks.Task<CommandResult> UpdateHypothesisAsync(string repositoryPath, UpdateHypothesisRequest request, CancellationToken cancellationToken)
    {
        return await ExecuteWriteLockedAsync(repositoryPath, cancellationToken, async () =>
        {
            var context = await _workingContextRepository.LoadAsync(repositoryPath, cancellationToken);
            var hypothesis = context.Hypotheses.SingleOrDefault(item => item.Id.Value.Equals(request.HypothesisId, StringComparison.OrdinalIgnoreCase));

            if (hypothesis is null)
            {
                return new CommandResult(false, $"Hypothesis not found: {request.HypothesisId}");
            }

            var parsedState = hypothesis.State;
            if (!string.IsNullOrWhiteSpace(request.State)
                && !Enum.TryParse<HypothesisState>(request.State, true, out parsedState))
            {
                throw new InvalidOperationException($"Unsupported hypothesis state '{request.State}'.");
            }

            var updated = hypothesis with
            {
                Statement = request.Statement ?? hypothesis.Statement,
                Rationale = request.Rationale ?? hypothesis.Rationale,
                Confidence = request.Confidence ?? hypothesis.Confidence,
                Impact = request.Impact ?? hypothesis.Impact,
                EvidenceStrength = request.EvidenceStrength ?? hypothesis.EvidenceStrength,
                CostToValidate = request.CostToValidate ?? hypothesis.CostToValidate,
                State = parsedState,
                Trace = hypothesis.Trace with
                {
                    UpdatedBy = request.UpdatedBy,
                    UpdatedAtUtc = _clock.UtcNow
                }
            };

            context = context with
            {
                Dirty = true,
                Hypotheses = context.Hypotheses.Select(item => item.Id == hypothesis.Id ? updated : item).ToArray(),
                Trace = context.Trace with { UpdatedAtUtc = _clock.UtcNow, UpdatedBy = request.UpdatedBy }
            };

            await _workingContextRepository.SaveWorkingAsync(repositoryPath, context, cancellationToken);
            return new CommandResult(true, $"Hypothesis updated: {updated.Statement}", updated);
        });
    }

    public async System.Threading.Tasks.Task<CommandResult> RankHypothesesAsync(string repositoryPath, CancellationToken cancellationToken)
    {
        var context = await _workingContextRepository.LoadAsync(repositoryPath, cancellationToken);
        var ranked = context.Hypotheses
            .OrderByDescending(item => item.Score)
            .ThenByDescending(item => item.Impact)
            .ThenByDescending(item => item.Probability)
            .Select(item => new
            {
                id = item.Id.Value,
                statement = item.Statement,
                score = item.Score,
                probability = item.Probability,
                impact = item.Impact,
                evidenceStrength = item.EvidenceStrength,
                costToValidate = item.CostToValidate,
                taskIds = item.TaskIds.Select(taskId => taskId.Value).ToArray(),
                evidenceIds = item.EvidenceIds.Select(evidenceId => evidenceId.Value).ToArray()
            })
            .ToArray();

        return new(true, $"Ranked {ranked.Length} hypotheses.", ranked);
    }

    public async System.Threading.Tasks.Task<CommandResult> NextAsync(string repositoryPath, CancellationToken cancellationToken)
    {
        var context = await _workingContextRepository.LoadAsync(repositoryPath, cancellationToken);
        var candidates = BuildNextWorkCandidates(context);
        var diagnostics = BuildNextWorkDiagnostics(context, candidates);
        var message = candidates.Count == 0
            ? "No next-step candidates were found. Review diagnostics for recovery guidance."
            : $"Ranked {candidates.Count} next-step candidates from {diagnostics.SelectionMode}.";

        return new CommandResult(
            true,
            message,
            new NextWorkSummary(
                candidates.FirstOrDefault(),
                candidates,
                diagnostics));
    }

    public async System.Threading.Tasks.Task<CommandResult> AddDecisionAsync(string repositoryPath, AddDecisionRequest request, CancellationToken cancellationToken)
    {
        return await ExecuteWriteLockedAsync(repositoryPath, cancellationToken, async () =>
        {
            var context = await _workingContextRepository.LoadAsync(repositoryPath, cancellationToken);
            var hypothesisIds = request.HypothesisIds.Select(id => ResolveHypothesis(context, id).Id).ToArray();
            var evidenceIds = request.EvidenceIds.Select(id => ResolveEvidence(context, id).Id).ToArray();
            var state = Enum.TryParse<DecisionState>(request.State, true, out var parsedState) ? parsedState : DecisionState.Proposed;

            var decision = new Decision(
                DecisionId.New(),
                request.Title,
                request.Rationale,
                state,
                NewTrace(request.CreatedBy, "decision"),
                hypothesisIds,
                evidenceIds);

            context = context with
            {
                Dirty = true,
                Decisions = context.Decisions.Append(decision).ToArray(),
                Trace = context.Trace with { UpdatedAtUtc = _clock.UtcNow, UpdatedBy = request.CreatedBy }
            };

            await _workingContextRepository.SaveWorkingAsync(repositoryPath, context, cancellationToken);
            return new CommandResult(true, $"Decision added: {decision.Title}", decision);
        });
    }

    public async System.Threading.Tasks.Task<CommandResult> AddEvidenceAsync(string repositoryPath, AddEvidenceRequest request, CancellationToken cancellationToken)
    {
        return await ExecuteWriteLockedAsync(repositoryPath, cancellationToken, async () =>
        {
            var context = await _workingContextRepository.LoadAsync(repositoryPath, cancellationToken);
            var supports = request.Supports.Select(reference => ResolveReference(context, reference)).ToArray();
            var kind = Enum.TryParse<EvidenceKind>(request.Kind, true, out var parsedKind) ? parsedKind : EvidenceKind.Document;

            var evidence = new Evidence(
                EvidenceId.New(),
                request.Title,
                request.Summary,
                request.Source,
                kind,
                request.Confidence,
                LifecycleState.Validated,
                NewTrace(request.CreatedBy, "evidence"),
                supports);

            var hypotheses = context.Hypotheses
                .Select(item => supports.Any(support => support.EntityType == nameof(Hypothesis) && support.EntityId == item.Id.Value)
                    ? item with { EvidenceIds = item.EvidenceIds.Append(evidence.Id).Distinct().ToArray() }
                    : item)
                .ToArray();

            context = context with
            {
                Dirty = true,
                Hypotheses = hypotheses,
                Evidence = context.Evidence.Append(evidence).ToArray(),
                Trace = context.Trace with { UpdatedAtUtc = _clock.UtcNow, UpdatedBy = request.CreatedBy }
            };

            await _workingContextRepository.SaveWorkingAsync(repositoryPath, context, cancellationToken);
            return new CommandResult(true, $"Evidence added: {evidence.Title}", evidence);
        });
    }

    public async System.Threading.Tasks.Task<CommandResult> AddConclusionAsync(string repositoryPath, AddConclusionRequest request, CancellationToken cancellationToken)
    {
        return await ExecuteWriteLockedAsync(repositoryPath, cancellationToken, async () =>
        {
            var context = await _workingContextRepository.LoadAsync(repositoryPath, cancellationToken);
            var decisionIds = request.DecisionIds.Select(id => ResolveDecision(context, id).Id).ToArray();
            var evidenceIds = request.EvidenceIds.Select(id => ResolveEvidence(context, id).Id).ToArray();
            var goalIds = request.GoalIds.Select(id => ResolveGoal(context, id).Id).ToArray();
            var taskIds = request.TaskIds.Select(id => ResolveTask(context, id).Id).ToArray();
            var state = Enum.TryParse<ConclusionState>(request.State, true, out var parsedState) ? parsedState : ConclusionState.Draft;

            var conclusion = new Conclusion(
                ConclusionId.New(),
                request.Summary,
                state,
                NewTrace(request.CreatedBy, "conclusion"),
                decisionIds,
                evidenceIds,
                goalIds,
                taskIds);

            context = context with
            {
                Dirty = true,
                Conclusions = context.Conclusions.Append(conclusion).ToArray(),
                Trace = context.Trace with { UpdatedAtUtc = _clock.UtcNow, UpdatedBy = request.CreatedBy }
            };

            await _workingContextRepository.SaveWorkingAsync(repositoryPath, context, cancellationToken);
            return new CommandResult(true, $"Conclusion added.", conclusion);
        });
    }

    public async System.Threading.Tasks.Task<CommandResult> UpdateConclusionAsync(string repositoryPath, UpdateConclusionRequest request, CancellationToken cancellationToken)
    {
        return await ExecuteWriteLockedAsync(repositoryPath, cancellationToken, async () =>
        {
            var context = await _workingContextRepository.LoadAsync(repositoryPath, cancellationToken);
            var conclusion = context.Conclusions.SingleOrDefault(item => item.Id.Value.Equals(request.ConclusionId, StringComparison.OrdinalIgnoreCase));

            if (conclusion is null)
            {
                return new CommandResult(false, $"Conclusion not found: {request.ConclusionId}");
            }

            var parsedState = conclusion.State;
            if (!string.IsNullOrWhiteSpace(request.State)
                && !Enum.TryParse<ConclusionState>(request.State, true, out parsedState))
            {
                throw new InvalidOperationException($"Unsupported conclusion state '{request.State}'.");
            }

            var updated = conclusion with
            {
                Summary = request.Summary ?? conclusion.Summary,
                State = parsedState,
                Trace = conclusion.Trace with
                {
                    UpdatedBy = request.UpdatedBy,
                    UpdatedAtUtc = _clock.UtcNow
                }
            };

            context = context with
            {
                Dirty = true,
                Conclusions = context.Conclusions.Select(item => item.Id == conclusion.Id ? updated : item).ToArray(),
                Trace = context.Trace with { UpdatedAtUtc = _clock.UtcNow, UpdatedBy = request.UpdatedBy }
            };

            await _workingContextRepository.SaveWorkingAsync(repositoryPath, context, cancellationToken);
            return new CommandResult(true, $"Conclusion updated.", updated);
        });
    }

    public async System.Threading.Tasks.Task<CommandResult> RunAsync(string repositoryPath, RunRequest request, CancellationToken cancellationToken)
    {
        return await ExecuteWriteLockedAsync(repositoryPath, cancellationToken, async () =>
        {
            var context = await _workingContextRepository.LoadAsync(repositoryPath, cancellationToken);
            var run = await _runOrchestrator.ExecuteAsync(repositoryPath, context, request, cancellationToken);
            context = context with
            {
                Dirty = true,
                Runs = context.Runs.Append(run).ToArray(),
                Trace = context.Trace with { UpdatedAtUtc = _clock.UtcNow, UpdatedBy = request.RequestedBy }
            };
            await _workingContextRepository.SaveWorkingAsync(repositoryPath, context, cancellationToken);
            return new CommandResult(true, $"Run completed with provider {run.Provider}", run);
        });
    }

    public async System.Threading.Tasks.Task<CommandResult> CommitAsync(string repositoryPath, CommitRequest request, CancellationToken cancellationToken)
    {
        return await ExecuteWriteLockedAsync(repositoryPath, cancellationToken, async () =>
        {
            var context = await _workingContextRepository.LoadAsync(repositoryPath, cancellationToken);
            var head = await _workingContextRepository.LoadHeadAsync(repositoryPath, cancellationToken);
            var previous = head.CommitId is null ? null : await _commitRepository.LoadAsync(repositoryPath, head.CommitId.Value, cancellationToken);
            var runbooks = await LoadRunbooksAsync(repositoryPath, cancellationToken);
            var triggers = await LoadTriggersAsync(repositoryPath, cancellationToken);
            var commit = _commitEngine.CreateCommit(context, runbooks, triggers, previous, request.Message, request.CreatedBy);

            await _commitRepository.SaveAsync(repositoryPath, commit, cancellationToken);
            await _workingContextRepository.SaveWorkingAsync(repositoryPath, commit.Snapshot.WorkingContext, cancellationToken);
            await _workingContextRepository.SaveStagingAsync(repositoryPath, commit.Snapshot.WorkingContext, cancellationToken);
            await _workingContextRepository.SaveHeadAsync(repositoryPath, new HeadReference(head.Branch, commit.Id), cancellationToken);
            await _branchRepository.SaveAsync(repositoryPath, new BranchReference(head.Branch, commit.Id, _clock.UtcNow), cancellationToken);

            return new CommandResult(true, $"[{head.Branch} {commit.Id.Value[..8]}] {request.Message}", commit);
        });
    }

    public async System.Threading.Tasks.Task<CommandResult> LogAsync(string repositoryPath, CancellationToken cancellationToken)
    {
        var head = await _workingContextRepository.LoadHeadAsync(repositoryPath, cancellationToken);
        var commits = await _commitRepository.GetHistoryAsync(repositoryPath, head.Branch, cancellationToken);
        var summary = commits.Count == 0
            ? $"Branch {head.Branch} has no commits."
            : string.Join(Environment.NewLine, commits.Select(commit =>
                $"{commit.Id.Value[..8]} {commit.CreatedAtUtc:yyyy-MM-dd HH:mm:ss} {commit.Message}"));

        return new(true, $"History for branch {head.Branch}", new
        {
            branch = head.Branch,
            count = commits.Count,
            summary,
            commits
        });
    }

    public async System.Threading.Tasks.Task<CommandResult> DiffAsync(string repositoryPath, string? fromCommitId, string? toCommitId, CancellationToken cancellationToken)
    {
        var context = await _workingContextRepository.LoadAsync(repositoryPath, cancellationToken);
        ContextCommit? fromCommit = null;
        ContextCommit? toCommit = null;

        if (!string.IsNullOrWhiteSpace(fromCommitId))
        {
            fromCommit = await _commitRepository.LoadAsync(repositoryPath, new ContextCommitId(fromCommitId), cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(toCommitId))
        {
            toCommit = await _commitRepository.LoadAsync(repositoryPath, new ContextCommitId(toCommitId), cancellationToken);
        }

        var diff = toCommit is not null
            ? _commitEngine.CreateCommit(toCommit.Snapshot.WorkingContext, toCommit.Snapshot.Runbooks, toCommit.Snapshot.Triggers, fromCommit, "diff-preview", "ctx").Diff
            : _commitEngine.CreateCommit(
                context,
                await LoadRunbooksAsync(repositoryPath, cancellationToken),
                await LoadTriggersAsync(repositoryPath, cancellationToken),
                fromCommit,
                "diff-preview",
                "ctx").Diff;

        if (fromCommit is not null && toCommit is not null)
        {
            var mergePreview = _mergeEngine.Merge(fromCommit.Snapshot, toCommit);
            diff = diff with
            {
                Conflicts = mergePreview.Conflicts,
                Summary = $"{diff.Summary} conflicts:{mergePreview.Conflicts.Count}"
            };
        }

        var diffSummary = BuildDiffSummary(diff);
        return new(true, diffSummary, new
        {
            summary = diffSummary,
            diff
        });
    }

    public async System.Threading.Tasks.Task<CommandResult> BranchAsync(string repositoryPath, string branchName, CancellationToken cancellationToken)
    {
        return await ExecuteWriteLockedAsync(repositoryPath, cancellationToken, async () =>
        {
            var head = await _workingContextRepository.LoadHeadAsync(repositoryPath, cancellationToken);
            var branch = new BranchReference(branchName, head.CommitId, _clock.UtcNow);
            await _branchRepository.SaveAsync(repositoryPath, branch, cancellationToken);
            return new CommandResult(true, $"Branch created: {branchName}", branch);
        });
    }

    public async System.Threading.Tasks.Task<CommandResult> CheckoutAsync(string repositoryPath, string branchName, CancellationToken cancellationToken)
    {
        return await ExecuteWriteLockedAsync(repositoryPath, cancellationToken, async () =>
        {
            var branch = await _branchRepository.LoadAsync(repositoryPath, branchName, cancellationToken)
                ?? throw new InvalidOperationException($"Branch '{branchName}' was not found.");

            var context = branch.CommitId is null
                ? await _workingContextRepository.LoadAsync(repositoryPath, cancellationToken)
                : (await _commitRepository.LoadAsync(repositoryPath, branch.CommitId.Value, cancellationToken))?.Snapshot.WorkingContext
                    ?? throw new InvalidOperationException($"Commit '{branch.CommitId.Value}' was not found.");

            var switched = context with { CurrentBranch = branchName, HeadCommitId = branch.CommitId, Dirty = false };
            await _workingContextRepository.SaveWorkingAsync(repositoryPath, switched, cancellationToken);
            await _workingContextRepository.SaveHeadAsync(repositoryPath, new HeadReference(branchName, branch.CommitId), cancellationToken);
            return new CommandResult(true, $"Switched to branch '{branchName}'", switched);
        });
    }

    public async System.Threading.Tasks.Task<CommandResult> MergeAsync(string repositoryPath, string sourceBranch, CancellationToken cancellationToken)
    {
        return await ExecuteWriteLockedAsync(repositoryPath, cancellationToken, async () =>
        {
            var targetHead = await _workingContextRepository.LoadHeadAsync(repositoryPath, cancellationToken);
            var source = await _branchRepository.LoadAsync(repositoryPath, sourceBranch, cancellationToken)
                ?? throw new InvalidOperationException($"Branch '{sourceBranch}' was not found.");

            if (source.CommitId is null)
            {
                return new CommandResult(true, $"Branch '{sourceBranch}' has no commits to merge.");
            }

            var sourceCommit = await _commitRepository.LoadAsync(repositoryPath, source.CommitId.Value, cancellationToken)
                ?? throw new InvalidOperationException($"Commit '{source.CommitId.Value}' was not found.");

            var currentContext = await _workingContextRepository.LoadAsync(repositoryPath, cancellationToken);
            var currentRunbooks = await LoadRunbooksAsync(repositoryPath, cancellationToken);
            var currentTriggers = await LoadTriggersAsync(repositoryPath, cancellationToken);
            var mergeResult = _mergeEngine.Merge(new RepositorySnapshot(currentContext, currentRunbooks, currentTriggers), sourceCommit);

            await _workingContextRepository.SaveWorkingAsync(repositoryPath, mergeResult.MergedSnapshot.WorkingContext, cancellationToken);
            if (_operationalRunbookRepository is not null)
            {
                foreach (var runbook in mergeResult.MergedSnapshot.Runbooks)
                {
                    await _operationalRunbookRepository.SaveAsync(repositoryPath, runbook, cancellationToken);
                }
            }
            if (_cognitiveTriggerRepository is not null)
            {
                foreach (var trigger in mergeResult.MergedSnapshot.Triggers)
                {
                    await _cognitiveTriggerRepository.SaveAsync(repositoryPath, trigger, cancellationToken);
                }
            }
            return new CommandResult(true, $"Merged branch '{sourceBranch}' into '{targetHead.Branch}'. {mergeResult.Summary}", mergeResult);
        });
    }

    public async System.Threading.Tasks.Task<CommandResult> ContextAsync(string repositoryPath, string purpose, string? goalId, string? taskId, CancellationToken cancellationToken)
    {
        var context = await _workingContextRepository.LoadAsync(repositoryPath, cancellationToken);
        var runbooks = await LoadRunbooksAsync(repositoryPath, cancellationToken);
        var triggers = await LoadTriggersAsync(repositoryPath, cancellationToken);
        var packet = _contextBuilder.Build(context, runbooks, triggers, purpose, goalId, taskId);
        return new(true, $"Built packet {packet.Id.Value}", packet);
    }

    public async System.Threading.Tasks.Task<CommandResult> ListOperationalRunbooksAsync(string repositoryPath, CancellationToken cancellationToken)
    {
        if (_operationalRunbookRepository is null)
        {
            throw new InvalidOperationException("Operational runbooks are not configured.");
        }

        var runbooks = await _operationalRunbookRepository.ListAsync(repositoryPath, cancellationToken);
        return new(true, $"Listed {runbooks.Count} operational runbooks.", runbooks);
    }

    public async System.Threading.Tasks.Task<CommandResult> ShowOperationalRunbookAsync(string repositoryPath, string runbookId, CancellationToken cancellationToken)
    {
        if (_operationalRunbookRepository is null)
        {
            throw new InvalidOperationException("Operational runbooks are not configured.");
        }

        var runbook = await _operationalRunbookRepository.LoadAsync(repositoryPath, new OperationalRunbookId(runbookId), cancellationToken)
            ?? throw new InvalidOperationException($"OperationalRunbook '{runbookId}' was not found.");
        return new(true, $"Showing operational runbook '{runbookId}'.", runbook);
    }

    public async System.Threading.Tasks.Task<CommandResult> ListCognitiveTriggersAsync(string repositoryPath, CancellationToken cancellationToken)
    {
        if (_cognitiveTriggerRepository is null)
        {
            throw new InvalidOperationException("Cognitive triggers are not configured.");
        }

        var triggers = await _cognitiveTriggerRepository.ListAsync(repositoryPath, cancellationToken);
        return new(true, $"Listed {triggers.Count} cognitive triggers.", triggers);
    }

    public async System.Threading.Tasks.Task<CommandResult> ShowCognitiveTriggerAsync(string repositoryPath, string triggerId, CancellationToken cancellationToken)
    {
        if (_cognitiveTriggerRepository is null)
        {
            throw new InvalidOperationException("Cognitive triggers are not configured.");
        }

        var trigger = await _cognitiveTriggerRepository.LoadAsync(repositoryPath, new CognitiveTriggerId(triggerId), cancellationToken)
            ?? throw new InvalidOperationException($"CognitiveTrigger '{triggerId}' was not found.");
        return new(true, $"Showing cognitive trigger '{triggerId}'.", trigger);
    }

    public async System.Threading.Tasks.Task<CommandResult> ListArtifactsAsync(string repositoryPath, string artifactType, CancellationToken cancellationToken)
    {
        var context = await _workingContextRepository.LoadAsync(repositoryPath, cancellationToken);
        var normalized = NormalizeArtifactType(artifactType);

        return normalized switch
        {
            "goal" => new(true, $"Listed {context.Goals.Count} goals.", context.Goals),
            "task" => new(true, $"Listed {context.Tasks.Count} tasks.", context.Tasks),
            "hypothesis" => new(true, $"Listed {context.Hypotheses.Count} hypotheses.", context.Hypotheses),
            "decision" => new(true, $"Listed {context.Decisions.Count} decisions.", context.Decisions),
            "evidence" => new(true, $"Listed {context.Evidence.Count} evidence items.", context.Evidence),
            "conclusion" => new(true, $"Listed {context.Conclusions.Count} conclusions.", context.Conclusions),
            _ => throw new InvalidOperationException($"Unsupported artifact type '{artifactType}'.")
        };
    }

    public async System.Threading.Tasks.Task<CommandResult> ShowArtifactAsync(string repositoryPath, string artifactType, string artifactId, CancellationToken cancellationToken)
    {
        var context = await _workingContextRepository.LoadAsync(repositoryPath, cancellationToken);
        var normalized = NormalizeArtifactType(artifactType);

        object data = normalized switch
        {
            "goal" => context.Goals.SingleOrDefault(item => item.Id.Value.Equals(artifactId, StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidOperationException($"Goal '{artifactId}' was not found."),
            "task" => context.Tasks.SingleOrDefault(item => item.Id.Value.Equals(artifactId, StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidOperationException($"Task '{artifactId}' was not found."),
            "hypothesis" => ResolveHypothesis(context, artifactId),
            "decision" => ResolveDecision(context, artifactId),
            "evidence" => ResolveEvidence(context, artifactId),
            "conclusion" => context.Conclusions.SingleOrDefault(item => item.Id.Value.Equals(artifactId, StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidOperationException($"Conclusion '{artifactId}' was not found."),
            _ => throw new InvalidOperationException($"Unsupported artifact type '{artifactType}'.")
        };

        return new(true, $"Showing {normalized} '{artifactId}'.", data);
    }

    public async System.Threading.Tasks.Task<CommandResult> ListProvidersAsync(string repositoryPath, CancellationToken cancellationToken)
    {
        var config = await _workingContextRepository.LoadConfigAsync(repositoryPath, cancellationToken);
        return new(true, $"Listed {config.Providers.Count} providers.", config.Providers);
    }

    public async System.Threading.Tasks.Task<CommandResult> ListRunsAsync(string repositoryPath, CancellationToken cancellationToken)
    {
        var runs = await _runRepository.ListAsync(repositoryPath, cancellationToken);
        return new(true, $"Listed {runs.Count} runs.", runs);
    }

    public async System.Threading.Tasks.Task<CommandResult> ShowRunAsync(string repositoryPath, string runId, CancellationToken cancellationToken)
    {
        var run = await _runRepository.LoadAsync(repositoryPath, new RunId(runId), cancellationToken)
            ?? throw new InvalidOperationException($"Run '{runId}' was not found.");
        return new(true, $"Showing run '{runId}'.", run);
    }

    public async System.Threading.Tasks.Task<CommandResult> ListPacketsAsync(string repositoryPath, CancellationToken cancellationToken)
    {
        var packets = await _packetRepository.ListAsync(repositoryPath, cancellationToken);
        return new(true, $"Listed {packets.Count} packets.", packets);
    }

    public async System.Threading.Tasks.Task<CommandResult> ShowPacketAsync(string repositoryPath, string packetId, CancellationToken cancellationToken)
    {
        var packet = await _packetRepository.LoadAsync(repositoryPath, new ContextPacketId(packetId), cancellationToken)
            ?? throw new InvalidOperationException($"Packet '{packetId}' was not found.");
        return new(true, $"Showing packet '{packetId}'.", packet);
    }

    public async System.Threading.Tasks.Task<CommandResult> ShowMetricsAsync(string repositoryPath, CancellationToken cancellationToken)
    {
        var metrics = await _metricsRepository.LoadAsync(repositoryPath, cancellationToken);
        return new(true, "Showing metrics.", metrics);
    }

    public async System.Threading.Tasks.Task<CommandResult> GraphSummaryAsync(string repositoryPath, CancellationToken cancellationToken)
    {
        var head = await _workingContextRepository.LoadHeadAsync(repositoryPath, cancellationToken);
        var context = await _workingContextRepository.LoadAsync(repositoryPath, cancellationToken);
        var graph = BuildGraphExport(context, head);

        var summary = new
        {
            branch = head.Branch,
            headCommitId = head.CommitId?.Value,
            graph = new
            {
                nodes = graph.Nodes.Count,
                edges = graph.Edges.Count
            },
            entities = new
            {
                goals = context.Goals.Count,
                tasks = context.Tasks.Count,
                hypotheses = context.Hypotheses.Count,
                decisions = context.Decisions.Count,
                evidence = context.Evidence.Count,
                conclusions = context.Conclusions.Count,
                runs = context.Runs.Count
            },
            lineageFocuses = new
            {
                goals = context.Goals.Select(item => item.Id.Value).ToArray(),
                tasks = context.Tasks.Select(item => item.Id.Value).ToArray(),
                hypotheses = context.Hypotheses.Select(item => item.Id.Value).ToArray(),
                decisions = context.Decisions.Select(item => item.Id.Value).ToArray(),
                conclusions = context.Conclusions.Select(item => item.Id.Value).ToArray()
            }
        };

        return new(true, "Graph summary generated.", summary);
    }

    public async System.Threading.Tasks.Task<CommandResult> GraphShowAsync(string repositoryPath, string nodeId, CancellationToken cancellationToken)
    {
        var head = await _workingContextRepository.LoadHeadAsync(repositoryPath, cancellationToken);
        var context = await _workingContextRepository.LoadAsync(repositoryPath, cancellationToken);
        var graph = BuildGraphExport(context, head);
        var requestedNodeId = ParseGraphNodeId(nodeId, context);

        var node = graph.Nodes.SingleOrDefault(item => item.Id.Equals(requestedNodeId, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"Graph node '{nodeId}' was not found.");

        var incoming = graph.Edges.Where(edge => edge.To.Equals(node.Id, StringComparison.OrdinalIgnoreCase)).ToArray();
        var outgoing = graph.Edges.Where(edge => edge.From.Equals(node.Id, StringComparison.OrdinalIgnoreCase)).ToArray();
        var connectedNodeIds = incoming.Select(edge => edge.From).Concat(outgoing.Select(edge => edge.To)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        var connectedNodes = graph.Nodes.Where(candidate => connectedNodeIds.Contains(candidate.Id, StringComparer.OrdinalIgnoreCase)).ToArray();

        return new(true, $"Graph node '{node.Id}' loaded.", new
        {
            branch = head.Branch,
            headCommitId = head.CommitId?.Value,
            node,
            incoming,
            outgoing,
            connectedNodes
        });
    }

    public async System.Threading.Tasks.Task<CommandResult> ExportGraphAsync(string repositoryPath, string format, string? commitId, CancellationToken cancellationToken)
    {
        if (!format.Equals("json", StringComparison.OrdinalIgnoreCase)
            && !format.Equals("mermaid", StringComparison.OrdinalIgnoreCase))
        {
            return new(false, $"Unsupported graph export format '{format}'.");
        }

        var head = await _workingContextRepository.LoadHeadAsync(repositoryPath, cancellationToken);
        var context = await ResolveGraphContextAsync(repositoryPath, commitId, cancellationToken);
        var graph = BuildGraphExport(context, head);

        if (format.Equals("mermaid", StringComparison.OrdinalIgnoreCase))
        {
            var mermaid = BuildMermaidGraph(graph);
            return new(true, "Graph exported in mermaid format.", new
            {
                format = "mermaid",
                diagram = mermaid,
                graph.Metadata
            });
        }

        return new(true, "Graph exported in json format.", graph);
    }

    public async System.Threading.Tasks.Task<CommandResult> GraphLineageAsync(string repositoryPath, string focusType, string focusId, string format, string? outputPath, CancellationToken cancellationToken)
    {
        var head = await _workingContextRepository.LoadHeadAsync(repositoryPath, cancellationToken);
        var context = await _workingContextRepository.LoadAsync(repositoryPath, cancellationToken);
        var fullGraph = BuildGraphExport(context, head);
        var lineage = focusType.Trim().ToLowerInvariant() switch
        {
            "goal" => BuildGoalLineage(fullGraph, context, ResolveGoal(context, focusId)),
            "conclusion" => BuildConclusionLineage(fullGraph, context, ResolveConclusion(context, focusId)),
            "hypothesis" or "hypo" => BuildHypothesisLineage(fullGraph, context, ResolveHypothesis(context, focusId)),
            "decision" => BuildDecisionLineage(fullGraph, context, ResolveDecision(context, focusId)),
            "task" => BuildTaskLineage(fullGraph, context, ResolveTask(context, focusId)),
            _ => throw new InvalidOperationException($"Unsupported lineage focus type '{focusType}'.")
        };

        if (format.Equals("mermaid", StringComparison.OrdinalIgnoreCase))
        {
            var diagram = BuildMermaidGraph(lineage.Graph);
            var data = new
            {
                format = "mermaid",
                focusType = lineage.FocusType,
                focusId,
                focusNodeId = lineage.FocusNodeId,
                diagram,
                lineage.Graph.Metadata
            };

            if (!string.IsNullOrWhiteSpace(outputPath))
            {
                var finalOutputPath = ResolveOutputPath(repositoryPath, outputPath);
                await WriteOutputAsync(finalOutputPath, diagram, cancellationToken);
                return new(true, $"Lineage for {lineage.FocusType.ToLowerInvariant()} '{focusId}' exported to {finalOutputPath}.", new
                {
                    data.format,
                    data.focusType,
                    data.focusId,
                    data.focusNodeId,
                    data.diagram,
                    data.Metadata,
                    outputPath = finalOutputPath
                });
            }

            return new(true, $"Lineage for {lineage.FocusType.ToLowerInvariant()} '{focusId}' in mermaid format.", data);
        }

        if (!format.Equals("json", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Unsupported lineage format '{format}'.");
        }

        if (!string.IsNullOrWhiteSpace(outputPath))
        {
            var finalOutputPath = ResolveOutputPath(repositoryPath, outputPath);
            var json = System.Text.Json.JsonSerializer.Serialize(
                lineage,
                new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                });

            await WriteOutputAsync(finalOutputPath, json, cancellationToken);
            return new(true, $"Lineage for {lineage.FocusType.ToLowerInvariant()} '{focusId}' exported to {finalOutputPath}.", new
            {
                outputPath = finalOutputPath,
                format = "json",
                focusType = lineage.FocusType,
                focusId = lineage.Graph.Metadata["focusId"]
            });
        }

        return new(true, $"Lineage for {lineage.FocusType.ToLowerInvariant()} '{focusId}'.", lineage);
    }

    public async System.Threading.Tasks.Task<CommandResult> ThreadReconstructAsync(string repositoryPath, string focusType, string focusId, string format, CancellationToken cancellationToken)
    {
        if (!format.Equals("json", StringComparison.OrdinalIgnoreCase)
            && !format.Equals("markdown", StringComparison.OrdinalIgnoreCase)
            && !format.Equals("md", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Unsupported thread reconstruction format '{format}'.");
        }

        var head = await _workingContextRepository.LoadHeadAsync(repositoryPath, cancellationToken);
        var context = await _workingContextRepository.LoadAsync(repositoryPath, cancellationToken);

        var thread = focusType.Trim().ToLowerInvariant() switch
        {
            "task" => await BuildTaskThreadAsync(repositoryPath, head, context, ResolveTask(context, focusId), cancellationToken),
            _ => throw new InvalidOperationException($"Unsupported thread focus type '{focusType}'.")
        };

        object payload = format.Equals("json", StringComparison.OrdinalIgnoreCase)
            ? thread
            : BuildThreadMarkdown(thread);

        return new(true, $"Thread reconstructed for {focusType.ToLowerInvariant()} '{focusId}'.", payload);
    }

    public async System.Threading.Tasks.Task<CommandResult> DoctorAsync(string repositoryPath, CancellationToken cancellationToken)
    {
        var checks = new List<DoctorCheck>
        {
            new("product-version", "ok", DomainConstants.ProductVersion),
            new("repository-path", Directory.Exists(repositoryPath) ? "ok" : "error", repositoryPath)
        };

        var repositoryDetected = await _workingContextRepository.ExistsAsync(repositoryPath, cancellationToken);
        checks.Add(new("ctx-repository", repositoryDetected ? "ok" : "warning", repositoryDetected ? "Repository detected." : "No .ctx repository found in current directory."));

        if (repositoryDetected)
        {
            try
            {
                var version = await _workingContextRepository.LoadVersionAsync(repositoryPath, cancellationToken);
                var head = await _workingContextRepository.LoadHeadAsync(repositoryPath, cancellationToken);
                var config = await _workingContextRepository.LoadConfigAsync(repositoryPath, cancellationToken);
                var working = await _workingContextRepository.LoadAsync(repositoryPath, cancellationToken);
                var metrics = await _metricsRepository.LoadAsync(repositoryPath, cancellationToken);

                checks.Add(new("repository-format", "ok", version.CurrentVersion));
                checks.Add(new("head", "ok", $"{head.Branch}:{head.CommitId?.Value ?? "null"}"));
                checks.Add(new("working-context", "ok", $"dirty={working.Dirty}; goals={working.Goals.Count}; tasks={working.Tasks.Count}; runs={working.Runs.Count}"));
                checks.Add(new("metrics", "ok", $"runs={metrics.TotalRuns}; tokens={metrics.TotalTokens}; acu={metrics.TotalAcuCost}"));

                foreach (var provider in config.Providers)
                {
                    var envVar = provider.Name.Equals("openai", StringComparison.OrdinalIgnoreCase)
                        ? "OPENAI_API_KEY"
                        : provider.Name.Equals("anthropic", StringComparison.OrdinalIgnoreCase)
                            ? "ANTHROPIC_API_KEY"
                            : null;

                    var hasCredential = envVar is not null && !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(envVar));
                    checks.Add(new(
                        $"provider:{provider.Name}",
                        provider.Enabled ? "ok" : "warning",
                        hasCredential
                            ? $"enabled; model={provider.DefaultModel}; credentials detected"
                            : $"enabled={provider.Enabled}; model={provider.DefaultModel}; credentials not detected"));
                }
            }
            catch (Exception exception)
            {
                checks.Add(new("repository-read", "error", exception.Message));
            }
        }
        else
        {
            checks.Add(new("repository-read", "warning", "Run `ctx init` inside a working directory to create a repository."));
            checks.Add(new("provider:openai", string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("OPENAI_API_KEY")) ? "warning" : "ok", string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("OPENAI_API_KEY")) ? "OPENAI_API_KEY not detected" : "OPENAI_API_KEY detected"));
            checks.Add(new("provider:anthropic", string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY")) ? "warning" : "ok", string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY")) ? "ANTHROPIC_API_KEY not detected" : "ANTHROPIC_API_KEY detected"));
        }

        var report = new DoctorReport(
            DomainConstants.ProductVersion,
            repositoryPath,
            repositoryDetected,
            checks);

        return new(true, "Doctor report generated.", report);
    }

    public async System.Threading.Tasks.Task<CommandResult> AuditAsync(string repositoryPath, CancellationToken cancellationToken)
    {
        var context = await _workingContextRepository.LoadAsync(repositoryPath, cancellationToken);
        var head = await _workingContextRepository.LoadHeadAsync(repositoryPath, cancellationToken);

        var acceptedConclusionTaskIds = context.Conclusions
            .Where(item => item.State == ConclusionState.Accepted)
            .SelectMany(item => item.TaskIds)
            .Select(item => item.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var issues = new List<AuditIssue>();

        foreach (var task in context.Tasks)
        {
            if (task.HypothesisIds.Count == 0)
            {
                issues.Add(new AuditIssue(
                    "warning",
                    "MissingHypothesis",
                    "Task",
                    task.Id.Value,
                    $"Task '{task.Title}' has no linked hypotheses.",
                    "Link or create at least one hypothesis for the task thread."));
            }

            if (task.State == TaskExecutionState.Done && !acceptedConclusionTaskIds.Contains(task.Id.Value))
            {
                issues.Add(new AuditIssue(
                    "warning",
                    "MissingAcceptedConclusion",
                    "Task",
                    task.Id.Value,
                    $"Task '{task.Title}' is Done but has no accepted conclusion linked to it.",
                    "Accept an existing conclusion or add a closing conclusion for the task."));
            }
        }

        foreach (var hypothesis in context.Hypotheses)
        {
            var relatedTasks = hypothesis.TaskIds
                .Select(id => context.Tasks.SingleOrDefault(task => task.Id == id))
                .Where(task => task is not null)
                .Cast<Ctx.Domain.Task>()
                .ToArray();

            if (hypothesis.EvidenceIds.Count == 0)
            {
                issues.Add(new AuditIssue(
                    "warning",
                    "MissingEvidence",
                    "Hypothesis",
                    hypothesis.Id.Value,
                    $"Hypothesis '{hypothesis.Statement}' has no supporting evidence.",
                    "Add evidence or refute/archive the hypothesis."));
            }

            if (hypothesis.State is HypothesisState.Proposed or HypothesisState.UnderEvaluation
                && relatedTasks.Length > 0
                && relatedTasks.All(task => task.State == TaskExecutionState.Done))
            {
                issues.Add(new AuditIssue(
                    "warning",
                    "OpenHypothesisOnClosedTasks",
                    "Hypothesis",
                    hypothesis.Id.Value,
                    $"Hypothesis '{hypothesis.Statement}' is still open even though all related tasks are Done.",
                    "Promote the hypothesis to Supported, Refuted or Archived after reviewing closure evidence."));
            }
        }

        foreach (var decision in context.Decisions.Where(item => item.State == DecisionState.Accepted))
        {
            if (string.IsNullOrWhiteSpace(decision.Rationale) || decision.EvidenceIds.Count == 0)
            {
                issues.Add(new AuditIssue(
                    "warning",
                    "AcceptedDecisionMissingSupport",
                    "Decision",
                    decision.Id.Value,
                    $"Accepted decision '{decision.Title}' is missing rationale or linked evidence.",
                    "Add rationale and evidence so the decision can be audited later."));
            }
        }

        foreach (var conclusion in context.Conclusions.Where(item => item.State == ConclusionState.Draft))
        {
            var relatedDoneTasks = conclusion.TaskIds
                .Select(id => context.Tasks.SingleOrDefault(task => task.Id == id))
                .Where(task => task is not null && task.State == TaskExecutionState.Done)
                .Cast<Ctx.Domain.Task>()
                .ToArray();

            if (relatedDoneTasks.Length > 0)
            {
                issues.Add(new AuditIssue(
                    "warning",
                    "DraftConclusionOnDoneTask",
                    "Conclusion",
                    conclusion.Id.Value,
                    $"Draft conclusion '{conclusion.Summary}' is linked to Done tasks.",
                    "Accept or supersede the conclusion so the closed work stops surfacing as a gap."));
            }
        }

        var consistencyScore = Math.Max(0m, decimal.Round(1m - (issues.Count * 0.05m), 4, MidpointRounding.AwayFromZero));
        var summary = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["tasks"] = context.Tasks.Count,
            ["hypotheses"] = context.Hypotheses.Count,
            ["decisions"] = context.Decisions.Count,
            ["conclusions"] = context.Conclusions.Count,
            ["issues"] = issues.Count,
            ["warnings"] = issues.Count(item => item.Severity.Equals("warning", StringComparison.OrdinalIgnoreCase)),
            ["errors"] = issues.Count(item => item.Severity.Equals("error", StringComparison.OrdinalIgnoreCase))
        };

        var report = new AuditReport(
            context.CurrentBranch,
            head.CommitId?.Value,
            consistencyScore,
            issues,
            summary);

        return new(true, issues.Count == 0 ? "Audit passed with no consistency issues." : $"Audit found {issues.Count} consistency issues.", report);
    }

    public async System.Threading.Tasks.Task<CommandResult> ExportAsync(string repositoryPath, string outputPath, CancellationToken cancellationToken)
    {
        var repositoryDetected = await _workingContextRepository.ExistsAsync(repositoryPath, cancellationToken);
        if (!repositoryDetected)
        {
            return new(false, "No .ctx repository found in current directory.");
        }

        var version = await _workingContextRepository.LoadVersionAsync(repositoryPath, cancellationToken);
        var config = await _workingContextRepository.LoadConfigAsync(repositoryPath, cancellationToken);
        var head = await _workingContextRepository.LoadHeadAsync(repositoryPath, cancellationToken);
        var working = await _workingContextRepository.LoadAsync(repositoryPath, cancellationToken);
        var metrics = await _metricsRepository.LoadAsync(repositoryPath, cancellationToken);
        var branches = await _branchRepository.ListAsync(repositoryPath, cancellationToken);
        var commits = await _commitRepository.GetHistoryAsync(repositoryPath, head.Branch, cancellationToken);
        var runbooks = await LoadRunbooksAsync(repositoryPath, cancellationToken);
        var triggers = await LoadTriggersAsync(repositoryPath, cancellationToken);

        var export = new RepositoryExport(
            DomainConstants.ProductVersion,
            version,
            config,
            head,
            new RepositorySnapshot(working, runbooks, triggers),
            metrics,
            branches,
            commits);

        var finalOutputPath = Path.IsPathRooted(outputPath)
            ? outputPath
            : Path.Combine(repositoryPath, outputPath);

        var directory = Path.GetDirectoryName(finalOutputPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = System.Text.Json.JsonSerializer.Serialize(
            export,
            new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
            });

        await File.WriteAllTextAsync(finalOutputPath, json, Encoding.UTF8, cancellationToken);

        return new(true, $"Repository exported to {finalOutputPath}", new
        {
            outputPath = finalOutputPath,
            commitCount = commits.Count,
            branchCount = branches.Count
        });
    }

    public async System.Threading.Tasks.Task<CommandResult> ImportAsync(string repositoryPath, string inputPath, CancellationToken cancellationToken)
    {
        return await ExecuteWriteLockedAsync(repositoryPath, cancellationToken, async () =>
        {
            var finalInputPath = Path.IsPathRooted(inputPath)
                ? inputPath
                : Path.Combine(repositoryPath, inputPath);

            if (!File.Exists(finalInputPath))
            {
                return new CommandResult(false, $"Import file not found: {finalInputPath}");
            }

            var json = await File.ReadAllTextAsync(finalInputPath, cancellationToken);
            var export = System.Text.Json.JsonSerializer.Deserialize<RepositoryExport>(
                json,
                new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (export is null)
            {
                return new CommandResult(false, "Import file could not be parsed.");
            }

            await _workingContextRepository.ImportAsync(repositoryPath, export, cancellationToken);

            foreach (var branch in export.Branches)
            {
                await _branchRepository.SaveAsync(repositoryPath, branch, cancellationToken);
            }

            foreach (var commit in export.Commits.OrderBy(commit => commit.CreatedAtUtc))
            {
                await _commitRepository.SaveAsync(repositoryPath, commit, cancellationToken);
            }

            if (_operationalRunbookRepository is not null)
            {
                foreach (var runbook in export.Snapshot.Runbooks)
                {
                    await _operationalRunbookRepository.SaveAsync(repositoryPath, runbook, cancellationToken);
                }
            }

            if (_cognitiveTriggerRepository is not null)
            {
                foreach (var trigger in export.Snapshot.Triggers)
                {
                    await _cognitiveTriggerRepository.SaveAsync(repositoryPath, trigger, cancellationToken);
                }
            }

            await _workingContextRepository.SaveHeadAsync(repositoryPath, export.Head, cancellationToken);

            return new CommandResult(true, $"Repository imported from {finalInputPath}", new
            {
                inputPath = finalInputPath,
                commitCount = export.Commits.Count,
                branchCount = export.Branches.Count
            });
        });
    }

    private Traceability NewTrace(string createdBy, string tag) =>
        new(
            createdBy,
            _clock.UtcNow,
            null,
            null,
            new[] { tag },
            Array.Empty<string>(),
            ResolveModelName(),
            ResolveModelVersion());

    private static string? ResolveModelName()
        => Environment.GetEnvironmentVariable("CTX_MODEL_NAME")
            ?? Environment.GetEnvironmentVariable("OPENAI_MODEL");

    private static string? ResolveModelVersion()
        => Environment.GetEnvironmentVariable("CTX_MODEL_VERSION");

    private static Hypothesis ResolveHypothesis(WorkingContext context, string hypothesisId)
        => context.Hypotheses.SingleOrDefault(item => item.Id.Value.Equals(hypothesisId, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"Hypothesis '{hypothesisId}' was not found.");

    private static Decision ResolveDecision(WorkingContext context, string decisionId)
        => context.Decisions.SingleOrDefault(item => item.Id.Value.Equals(decisionId, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"Decision '{decisionId}' was not found.");

    private static Goal ResolveGoal(WorkingContext context, string goalId)
        => context.Goals.SingleOrDefault(item => item.Id.Value.Equals(goalId, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"Goal '{goalId}' was not found.");

    private static Ctx.Domain.Task ResolveTask(WorkingContext context, string taskId)
        => context.Tasks.SingleOrDefault(item => item.Id.Value.Equals(taskId, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"Task '{taskId}' was not found.");

    private static Conclusion ResolveConclusion(WorkingContext context, string conclusionId)
        => context.Conclusions.SingleOrDefault(item => item.Id.Value.Equals(conclusionId, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"Conclusion '{conclusionId}' was not found.");

    private static Evidence ResolveEvidence(WorkingContext context, string evidenceId)
        => context.Evidence.SingleOrDefault(item => item.Id.Value.Equals(evidenceId, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"Evidence '{evidenceId}' was not found.");

    private static EntityReference ResolveReference(WorkingContext context, string reference)
    {
        var parts = reference.Split(':', 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
        {
            throw new InvalidOperationException($"Reference '{reference}' must use '<type>:<id>' format.");
        }

        var type = parts[0].ToLowerInvariant();
        var id = parts[1];

        return type switch
        {
            "hypothesis" or "hypo" => new EntityReference(nameof(Hypothesis), ResolveHypothesis(context, id).Id.Value),
            "decision" => new EntityReference(nameof(Decision), ResolveDecision(context, id).Id.Value),
            "task" => new EntityReference("Task", context.Tasks.SingleOrDefault(item => item.Id.Value.Equals(id, StringComparison.OrdinalIgnoreCase))?.Id.Value
                ?? throw new InvalidOperationException($"Task '{id}' was not found.")),
            _ => throw new InvalidOperationException($"Unsupported evidence reference type '{parts[0]}'.")
        };
    }

    private static string NormalizeArtifactType(string artifactType) => artifactType.Trim().ToLowerInvariant() switch
    {
        "goal" or "goals" => "goal",
        "task" or "tasks" => "task",
        "hypothesis" or "hypotheses" or "hypo" => "hypothesis",
        "decision" or "decisions" => "decision",
        "evidence" => "evidence",
        "conclusion" or "conclusions" => "conclusion",
        _ => artifactType.Trim().ToLowerInvariant()
    };

    private static bool ShouldCreateAutomaticTaskTrigger(
        Ctx.Domain.Task task,
        Ctx.Domain.Task? parentTask,
        IReadOnlyCollection<TaskId> dependsOnTaskIds)
        => parentTask is null
            && task.ParentTaskId is null
            && dependsOnTaskIds.Count == 0;

    private async System.Threading.Tasks.Task TryCreateAutomaticTriggerAsync(
        string repositoryPath,
        CognitiveTriggerKind kind,
        string summary,
        string? text,
        IReadOnlyList<GoalId> goalIds,
        IReadOnlyList<TaskId> taskIds,
        string createdBy,
        CancellationToken cancellationToken)
    {
        if (_cognitiveTriggerRepository is null)
        {
            return;
        }

        var normalizedSummary = summary.Trim();
        if (string.IsNullOrWhiteSpace(normalizedSummary))
        {
            return;
        }

        var normalizedText = string.IsNullOrWhiteSpace(text) ? null : text.Trim();
        var existing = await LoadTriggersAsync(repositoryPath, cancellationToken);
        var duplicate = existing.Any(item =>
            item.State != LifecycleState.Archived
            && item.Kind == kind
            && string.Equals(item.Summary, normalizedSummary, StringComparison.Ordinal)
            && string.Equals(item.Text, normalizedText, StringComparison.Ordinal)
            && item.GoalIds.SequenceEqual(goalIds)
            && item.TaskIds.SequenceEqual(taskIds));

        if (duplicate)
        {
            return;
        }

        var trigger = new CognitiveTrigger(
            CognitiveTriggerId.New(),
            kind,
            normalizedSummary,
            normalizedText,
            _hashingService.Hash(normalizedText ?? normalizedSummary),
            goalIds.Distinct().ToArray(),
            taskIds.Distinct().ToArray(),
            Array.Empty<OperationalRunbookId>(),
            LifecycleState.Active,
            NewTrace(createdBy, "trigger"));

        await _cognitiveTriggerRepository.SaveAsync(repositoryPath, trigger, cancellationToken);
    }

    private async System.Threading.Tasks.Task<IReadOnlyList<OperationalRunbook>> LoadRunbooksAsync(string repositoryPath, CancellationToken cancellationToken)
        => _operationalRunbookRepository is null
            ? Array.Empty<OperationalRunbook>()
            : await _operationalRunbookRepository.ListAsync(repositoryPath, cancellationToken);

    private async System.Threading.Tasks.Task<IReadOnlyList<CognitiveTrigger>> LoadTriggersAsync(string repositoryPath, CancellationToken cancellationToken)
        => _cognitiveTriggerRepository is null
            ? Array.Empty<CognitiveTrigger>()
            : await _cognitiveTriggerRepository.ListAsync(repositoryPath, cancellationToken);

    private static string BuildDiffSummary(ContextDiff diff)
    {
        var parts = new List<string>
        {
            $"decisions:{diff.Decisions.Count}",
            $"hypotheses:{diff.Hypotheses.Count}",
            $"evidence:{diff.Evidence.Count}",
            $"tasks:{diff.Tasks.Count}",
            $"conclusions:{diff.Conclusions.Count}",
            $"runbooks:{diff.Runbooks.Count}",
            $"triggers:{diff.Triggers.Count}"
        };

        if (diff.Conflicts.Count > 0)
        {
            parts.Add($"conflicts:{diff.Conflicts.Count}");
        }

        return string.Join(" | ", parts);
    }

    private static IReadOnlyList<CloseoutPendingItem> EnumerateCloseoutPendingItems(ContextDiff diff)
        => diff.Decisions
            .Concat(diff.Hypotheses)
            .Concat(diff.Evidence)
            .Concat(diff.Tasks)
            .Concat(diff.Conclusions)
            .Concat(diff.Runbooks)
            .Concat(diff.Triggers)
            .Select(change => new CloseoutPendingItem(change.ChangeType, change.EntityType, change.EntityId, change.Summary))
            .ToArray();

    private static (Ctx.Domain.Task Task, string SelectionReason) ResolveCheckTask(WorkingContext context, string? taskId)
    {
        if (!string.IsNullOrWhiteSpace(taskId))
        {
            return (ResolveTask(context, taskId), "explicit task selection");
        }

        var inProgressTasks = context.Tasks.Where(item => item.State == TaskExecutionState.InProgress).ToArray();
        if (inProgressTasks.Length == 1)
        {
            return (inProgressTasks[0], "single in-progress task");
        }

        var nextTask = BuildNextWorkCandidates(context).FirstOrDefault(candidate => candidate.CandidateType == "Task");
        if (nextTask is not null)
        {
            return (ResolveTask(context, nextTask.EntityId), "top-ranked task from ctx next");
        }

        var onlyOpenTask = context.Tasks.Where(item => item.State != TaskExecutionState.Done).ToArray();
        if (onlyOpenTask.Length == 1)
        {
            return (onlyOpenTask[0], "only open task");
        }

        throw new InvalidOperationException("No task could be resolved for block-level checking. Pass --task <taskId> or open a task first.");
    }

    private static BlockCheckSummary BuildBlockCheckSummary(WorkingContext context, Ctx.Domain.Task task, string selectionReason, IReadOnlyList<OperationalRunbook> runbooks)
    {
        var hypotheses = context.Hypotheses
            .Where(item => item.TaskIds.Any(id => id == task.Id) || task.HypothesisIds.Contains(item.Id))
            .DistinctBy(item => item.Id.Value)
            .ToArray();

        var evidence = context.Evidence
            .Where(item =>
                item.Supports.Any(reference => reference.EntityType == "Task" && reference.EntityId.Equals(task.Id.Value, StringComparison.OrdinalIgnoreCase))
                || hypotheses.Any(hypothesis => item.Supports.Any(reference => reference.EntityType == nameof(Hypothesis) && reference.EntityId.Equals(hypothesis.Id.Value, StringComparison.OrdinalIgnoreCase)))
                || hypotheses.Any(hypothesis => hypothesis.EvidenceIds.Contains(item.Id)))
            .DistinctBy(item => item.Id.Value)
            .ToArray();

        var decisions = context.Decisions
            .Where(item => item.HypothesisIds.Any(id => hypotheses.Any(hypothesis => hypothesis.Id == id)))
            .DistinctBy(item => item.Id.Value)
            .ToArray();

        var acceptedDecisions = decisions.Where(item => item.State == DecisionState.Accepted).ToArray();

        var conclusions = context.Conclusions
            .Where(item =>
                item.TaskIds.Contains(task.Id)
                || item.DecisionIds.Any(id => decisions.Any(decision => decision.Id == id)))
            .DistinctBy(item => item.Id.Value)
            .ToArray();

        var acceptedConclusions = conclusions.Where(item => item.State == ConclusionState.Accepted).ToArray();
        var selectedGoal = task.GoalId is null
            ? null
            : context.Goals.SingleOrDefault(goal => goal.Id == task.GoalId);
        var runbookSelection = OperationalRunbookSelection.Select(
            runbooks,
            $"{task.Title} {task.Description} {selectedGoal?.Title ?? string.Empty} {selectedGoal?.Description ?? string.Empty}",
            task.GoalId?.Value,
            task.Id.Value,
            selectedGoal is null ? Array.Empty<Goal>() : new[] { selectedGoal },
            new[] { task });
        var missing = new List<BlockCheckMissingItem>();

        if (task.State != TaskExecutionState.Done)
        {
            missing.Add(new BlockCheckMissingItem("TaskState", $"Task is still {task.State}; close the work state before commit if the block is meant to be finished."));
        }

        if (hypotheses.Length == 0)
        {
            missing.Add(new BlockCheckMissingItem("Hypothesis", "No hypotheses are linked to this task thread."));
        }

        if (hypotheses.Any(item => item.EvidenceIds.Count == 0))
        {
            missing.Add(new BlockCheckMissingItem("Evidence", "At least one linked hypothesis has no supporting evidence."));
        }

        if (acceptedDecisions.Length == 0)
        {
            missing.Add(new BlockCheckMissingItem("Decision", "No accepted decision has been linked to this task thread."));
        }

        if (acceptedConclusions.Length == 0)
        {
            missing.Add(new BlockCheckMissingItem("Conclusion", "No accepted conclusion closes this task thread yet."));
        }

        var readyForCommit = missing.Count == 0;
        var guidance = new List<string>();

        if (readyForCommit)
        {
            guidance.Add("This task thread has the expected closure elements for a cognitive commit.");
            guidance.Add("Run `ctx closeout` and then `ctx commit -m \"<message>\"` when the code or docs block is also ready.");
        }
        else
        {
            guidance.Add("Use the missing items list to close the current task thread before the next cognitive commit.");
            guidance.Add("Run `ctx closeout` after fixing the missing elements to confirm the working delta is coherent.");
        }

        if (runbookSelection.Selected.Count > 0)
        {
            guidance.Add($"Review {runbookSelection.Selected.Count} operational runbook suggestion(s) before continuing execution or closeout.");
        }

        return new BlockCheckSummary(
            task.Id.Value,
            task.Title,
            task.State.ToString(),
            selectionReason,
            readyForCommit,
            hypotheses.Length,
            evidence.Length,
            decisions.Length,
            acceptedDecisions.Length,
            conclusions.Length,
            acceptedConclusions.Length,
            missing,
            runbookSelection.Selected.Select(runbook => new RunbookSuggestion(
                runbook.Id.Value,
                runbook.Title,
                runbook.Kind.ToString(),
                runbook.WhenToUse,
                runbook.Do.Take(3).ToArray(),
                runbook.Verify.Take(2).ToArray(),
                runbook.References.Take(3).ToArray())).ToArray(),
            runbookSelection.Available.Select(runbook => runbook.Title).ToArray(),
            guidance);
    }

    private static StatusPendingSummary? BuildStatusPendingSummary(RepositorySnapshot? previousSnapshot, RepositorySnapshot currentSnapshot)
    {
        if (!currentSnapshot.WorkingContext.Dirty)
        {
            return null;
        }

        var diff = BuildCloseoutDiff(previousSnapshot, currentSnapshot);
        var pendingItems = EnumerateCloseoutPendingItems(diff);
        return new StatusPendingSummary(
            pendingItems.Count > 0,
            BuildDiffSummary(diff),
            pendingItems.Count,
            pendingItems.Take(5).ToArray(),
            pendingItems.Count > 0
                ? "Run `ctx closeout` for the full pending artifact review before the next cognitive or Git commit."
                : "Run `ctx closeout` to verify whether the workspace and HEAD are aligned.");
    }

    private static ContextDiff BuildCloseoutDiff(RepositorySnapshot? previousSnapshot, RepositorySnapshot currentSnapshot)
    {
        var current = currentSnapshot.WorkingContext;
        var previousWorkingContext = previousSnapshot?.WorkingContext;
        var decisions = DiffCloseoutEntities(previousWorkingContext?.Decisions ?? Array.Empty<Decision>(), current.Decisions, item => item.Id.Value, item => $"{item.Title} [{item.State}]");
        var hypotheses = DiffCloseoutEntities(previousWorkingContext?.Hypotheses ?? Array.Empty<Hypothesis>(), current.Hypotheses, item => item.Id.Value, item => $"{item.Statement} [{item.State}]");
        var evidence = DiffCloseoutEntities(previousWorkingContext?.Evidence ?? Array.Empty<Evidence>(), current.Evidence, item => item.Id.Value, item => $"{item.Title} [{item.Kind}]");
        var tasks = DiffCloseoutEntities(previousWorkingContext?.Tasks ?? Array.Empty<Ctx.Domain.Task>(), current.Tasks, item => item.Id.Value, item => $"{item.Title} [{item.State}]");
        var conclusions = DiffCloseoutEntities(previousWorkingContext?.Conclusions ?? Array.Empty<Conclusion>(), current.Conclusions, item => item.Id.Value, item => $"{item.Summary} [{item.State}]");
        var runbooks = DiffCloseoutEntities(previousSnapshot?.Runbooks ?? Array.Empty<OperationalRunbook>(), currentSnapshot.Runbooks, item => item.Id.Value, item => $"{item.Title} [{item.Kind}]");
        var triggers = DiffCloseoutEntities(previousSnapshot?.Triggers ?? Array.Empty<CognitiveTrigger>(), currentSnapshot.Triggers, item => item.Id.Value, item => $"{item.Kind}:{item.Summary}");
        var summary = $"decisions:{decisions.Count} hypotheses:{hypotheses.Count} evidence:{evidence.Count} tasks:{tasks.Count} conclusions:{conclusions.Count} runbooks:{runbooks.Count} triggers:{triggers.Count}";
        return new ContextDiff(previousWorkingContext?.HeadCommitId, current.HeadCommitId, decisions, hypotheses, evidence, tasks, conclusions, runbooks, triggers, Array.Empty<CognitiveConflict>(), summary);
    }

    private static IReadOnlyList<ContextDiffChange> DiffCloseoutEntities<T>(
        IEnumerable<T> previous,
        IEnumerable<T> current,
        Func<T, string> idSelector,
        Func<T, string> summarySelector)
    {
        var previousMap = previous.ToDictionary(idSelector, item => item);
        var currentMap = current.ToDictionary(idSelector, item => item);
        var changes = new List<ContextDiffChange>();

        foreach (var pair in currentMap)
        {
            if (!previousMap.TryGetValue(pair.Key, out var previousItem))
            {
                changes.Add(new("Added", typeof(T).Name, pair.Key, summarySelector(pair.Value)));
                continue;
            }

            if (!JsonSerializer.Serialize(previousItem).Equals(JsonSerializer.Serialize(pair.Value), StringComparison.Ordinal))
            {
                changes.Add(new("Modified", typeof(T).Name, pair.Key, summarySelector(pair.Value)));
            }
        }

        foreach (var pair in previousMap)
        {
            if (!currentMap.ContainsKey(pair.Key))
            {
                changes.Add(new("Removed", typeof(T).Name, pair.Key, summarySelector(pair.Value)));
            }
        }

        return changes;
    }

    private static IReadOnlyList<string> BuildCloseoutGuidance(
        WorkingContext context,
        ContextDiff diff,
        IReadOnlyList<CloseoutPendingItem> pendingItems)
    {
        if (pendingItems.Count == 0)
        {
            return new[]
            {
                "No pending cognitive changes remain between working and HEAD.",
                "The current block is already closed at the cognitive level."
            };
        }

        var guidance = new List<string>
        {
            $"Review {pendingItems.Count} pending cognitive artifact(s) before the next Git closeout.",
            "If this block is complete, capture these changes with `ctx commit -m \"<message>\"` before the Git commit."
        };

        if (diff.Evidence.Count > 0 && diff.Decisions.Count == 0 && diff.Conclusions.Count == 0)
        {
            guidance.Add("Evidence changed without a matching decision or conclusion in this delta; verify whether the block still needs cognitive closure.");
        }

        if (diff.Tasks.Count > 0 && diff.Conclusions.Count == 0)
        {
            guidance.Add("Task changes are pending without a conclusion in this delta; check whether the current task should be explicitly closed.");
        }

        if (!context.Dirty)
        {
            guidance.Add("Working state is marked clean, but the pending diff preview is non-empty; verify branch and HEAD alignment before closing.");
        }

        return guidance;
    }

    private static MicroCloseoutSuggestion? BuildMicroCloseoutSuggestion(
        ContextDiff diff,
        IReadOnlyList<CloseoutPendingItem> pendingItems)
    {
        if (pendingItems.Count == 0 || pendingItems.Count > 3 || diff.Decisions.Count > 0)
        {
            return null;
        }

        if (diff.Evidence.Count > 0 && diff.Tasks.Count <= 1 && diff.Hypotheses.Count == 0 && diff.Conclusions.Count == 0)
        {
            return new MicroCloseoutSuggestion(
                "EvidenceOnly",
                "This looks like a small evidence-only delta.",
                new[]
                {
                    "Confirm whether the new evidence extends an already valid thread instead of opening a fresh closure block.",
                    "If no new decision or conclusion is required, a short cognitive commit may be enough after the evidence is recorded."
                });
        }

        if (diff.Hypotheses.Count > 0 && diff.Evidence.Count > 0 && diff.Tasks.Count <= 1 && diff.Conclusions.Count == 0)
        {
            return new MicroCloseoutSuggestion(
                "HypothesisEvidence",
                "This looks like a small hypothesis-plus-evidence delta.",
                new[]
                {
                    "Check whether the evidence is sufficient to support or refute the hypothesis before creating a larger closure chain.",
                    "If the task thread remains open, keep the closeout small and avoid forcing a premature conclusion."
                });
        }

        if (diff.Tasks.Count > 0 && diff.Conclusions.Count > 0 && pendingItems.Count <= 3)
        {
            return new MicroCloseoutSuggestion(
                "TaskClosure",
                "This looks like a small task-closure delta.",
                new[]
                {
                    "Verify that the task state and accepted conclusion now match the real end of the block.",
                    "If the task thread is coherent, run a short cognitive commit before the Git closeout."
                });
        }

        if (pendingItems.Count <= 2)
        {
            return new MicroCloseoutSuggestion(
                "SmallDelta",
                "This looks like a small trailing cognitive delta.",
                new[]
                {
                    "Prefer the smallest closure move that keeps the thread coherent.",
                    "Use `ctx check` if you need to confirm whether the current task thread is already commit-ready."
                });
        }

        return null;
    }

    private static string ParseGraphNodeId(string nodeId, WorkingContext context)
    {
        if (nodeId.Contains(':', StringComparison.Ordinal))
        {
            var parts = nodeId.Split(':', 2, StringSplitOptions.TrimEntries);
            return NodeId(parts[0], parts[1]);
        }

        return context.Project.Id.Value.Equals(nodeId, StringComparison.OrdinalIgnoreCase)
            ? NodeId("Project", context.Project.Id.Value)
            : context.Goals.Any(item => item.Id.Value.Equals(nodeId, StringComparison.OrdinalIgnoreCase))
                ? NodeId("Goal", nodeId)
                : context.Tasks.Any(item => item.Id.Value.Equals(nodeId, StringComparison.OrdinalIgnoreCase))
                    ? NodeId("Task", nodeId)
                    : context.Hypotheses.Any(item => item.Id.Value.Equals(nodeId, StringComparison.OrdinalIgnoreCase))
                        ? NodeId("Hypothesis", nodeId)
                        : context.Decisions.Any(item => item.Id.Value.Equals(nodeId, StringComparison.OrdinalIgnoreCase))
                            ? NodeId("Decision", nodeId)
                            : context.Evidence.Any(item => item.Id.Value.Equals(nodeId, StringComparison.OrdinalIgnoreCase))
                                ? NodeId("Evidence", nodeId)
                                : context.Conclusions.Any(item => item.Id.Value.Equals(nodeId, StringComparison.OrdinalIgnoreCase))
                                    ? NodeId("Conclusion", nodeId)
                                    : context.Runs.Any(item => item.Id.Value.Equals(nodeId, StringComparison.OrdinalIgnoreCase))
                                        ? NodeId("Run", nodeId)
                                        : NodeId("ContextPacket", nodeId);
    }

    private static string ResolveOutputPath(string repositoryPath, string outputPath)
    {
        var finalOutputPath = Path.IsPathRooted(outputPath)
            ? outputPath
            : Path.Combine(repositoryPath, outputPath);

        var directory = Path.GetDirectoryName(finalOutputPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        return finalOutputPath;
    }

    private static async System.Threading.Tasks.Task WriteOutputAsync(string outputPath, string content, CancellationToken cancellationToken)
        => await File.WriteAllTextAsync(outputPath, content, Encoding.UTF8, cancellationToken);

    private async System.Threading.Tasks.Task<WorkingContext> ResolveGraphContextAsync(string repositoryPath, string? commitId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(commitId))
        {
            return await _workingContextRepository.LoadAsync(repositoryPath, cancellationToken);
        }

        var commit = await _commitRepository.LoadAsync(repositoryPath, new ContextCommitId(commitId), cancellationToken)
            ?? throw new InvalidOperationException($"Commit '{commitId}' was not found.");

        return commit.Snapshot.WorkingContext;
    }

    private static CognitiveGraphExport BuildGraphExport(WorkingContext context, HeadReference head)
    {
        var nodes = new List<CognitiveGraphNode>();
        var edges = new List<CognitiveGraphEdge>();

        nodes.Add(new CognitiveGraphNode(
            NodeId("Project", context.Project.Id.Value),
            "Project",
            context.Project.Name,
            context.Project.State.ToString(),
            new Dictionary<string, string>
            {
                ["description"] = context.Project.Description,
                ["defaultBranch"] = context.Project.DefaultBranch
            }));

        foreach (var goal in context.Goals)
        {
            nodes.Add(new CognitiveGraphNode(
                NodeId("Goal", goal.Id.Value),
                "Goal",
                goal.Title,
                goal.State.ToString(),
                new Dictionary<string, string>
                {
                    ["priority"] = goal.Priority.ToString(),
                    ["description"] = goal.Description
                }));

            if (goal.ParentGoalId is not null)
            {
                edges.Add(Edge(NodeId("Goal", goal.ParentGoalId.Value.Value), NodeId("Goal", goal.Id.Value), "subgoal"));
            }
            else
            {
                edges.Add(Edge(NodeId("Project", context.Project.Id.Value), NodeId("Goal", goal.Id.Value), "contains"));
            }
        }

        foreach (var task in context.Tasks)
        {
            nodes.Add(new CognitiveGraphNode(
                NodeId("Task", task.Id.Value),
                "Task",
                task.Title,
                task.State.ToString(),
                new Dictionary<string, string>
                {
                    ["description"] = task.Description
                }));

            if (task.GoalId is not null)
            {
                edges.Add(Edge(NodeId("Goal", task.GoalId.Value.Value), NodeId("Task", task.Id.Value), "contains"));
            }

            foreach (var dependencyTaskId in task.DependsOnTaskIds)
            {
                edges.Add(Edge(NodeId("Task", task.Id.Value), NodeId("Task", dependencyTaskId.Value), "depends-on"));
            }
        }

        foreach (var hypothesis in context.Hypotheses)
        {
            nodes.Add(new CognitiveGraphNode(
                NodeId("Hypothesis", hypothesis.Id.Value),
                "Hypothesis",
                hypothesis.Statement,
                hypothesis.State.ToString(),
                new Dictionary<string, string>
                {
                    ["confidence"] = hypothesis.Confidence.ToString("0.##"),
                    ["probability"] = hypothesis.Probability.ToString("0.##"),
                    ["impact"] = hypothesis.Impact.ToString("0.##"),
                    ["evidenceStrength"] = hypothesis.EvidenceStrength.ToString("0.##"),
                    ["costToValidate"] = hypothesis.CostToValidate.ToString("0.##"),
                    ["score"] = hypothesis.Score.ToString("0.####"),
                    ["rationale"] = hypothesis.Rationale
                }));

            foreach (var taskId in hypothesis.TaskIds)
            {
                edges.Add(Edge(NodeId("Task", taskId.Value), NodeId("Hypothesis", hypothesis.Id.Value), "informs"));
            }
        }

        foreach (var evidence in context.Evidence)
        {
            nodes.Add(new CognitiveGraphNode(
                NodeId("Evidence", evidence.Id.Value),
                "Evidence",
                evidence.Title,
                evidence.State.ToString(),
                new Dictionary<string, string>
                {
                    ["kind"] = evidence.Kind.ToString(),
                    ["confidence"] = evidence.Confidence.ToString("0.##"),
                    ["source"] = evidence.Source
                }));

            foreach (var support in evidence.Supports)
            {
                edges.Add(Edge(NodeId(support.EntityType, support.EntityId), NodeId("Evidence", evidence.Id.Value), "supported-by"));
            }
        }

        foreach (var decision in context.Decisions)
        {
            nodes.Add(new CognitiveGraphNode(
                NodeId("Decision", decision.Id.Value),
                "Decision",
                decision.Title,
                decision.State.ToString(),
                new Dictionary<string, string>
                {
                    ["rationale"] = decision.Rationale
                }));

            foreach (var hypothesisId in decision.HypothesisIds)
            {
                edges.Add(Edge(NodeId("Hypothesis", hypothesisId.Value), NodeId("Decision", decision.Id.Value), "influences"));
            }

            foreach (var evidenceId in decision.EvidenceIds)
            {
                edges.Add(Edge(NodeId("Evidence", evidenceId.Value), NodeId("Decision", decision.Id.Value), "supports"));
            }
        }

        foreach (var conclusion in context.Conclusions)
        {
            nodes.Add(new CognitiveGraphNode(
                NodeId("Conclusion", conclusion.Id.Value),
                "Conclusion",
                conclusion.Summary,
                conclusion.State.ToString(),
                new Dictionary<string, string>()));

            foreach (var decisionId in conclusion.DecisionIds)
            {
                edges.Add(Edge(NodeId("Decision", decisionId.Value), NodeId("Conclusion", conclusion.Id.Value), "leads-to"));
            }

            foreach (var evidenceId in conclusion.EvidenceIds)
            {
                edges.Add(Edge(NodeId("Evidence", evidenceId.Value), NodeId("Conclusion", conclusion.Id.Value), "supports"));
            }

            foreach (var goalId in conclusion.GoalIds)
            {
                edges.Add(Edge(NodeId("Goal", goalId.Value), NodeId("Conclusion", conclusion.Id.Value), "resolved-by"));
            }

            foreach (var taskId in conclusion.TaskIds)
            {
                edges.Add(Edge(NodeId("Task", taskId.Value), NodeId("Conclusion", conclusion.Id.Value), "resolved-by"));
            }
        }

        foreach (var run in context.Runs)
        {
            nodes.Add(new CognitiveGraphNode(
                NodeId("Run", run.Id.Value),
                "Run",
                run.Summary,
                run.State.ToString(),
                new Dictionary<string, string>
                {
                    ["provider"] = run.Provider,
                    ["model"] = run.Model,
                    ["packetId"] = run.PacketId.Value
                }));

            edges.Add(Edge(NodeId("ContextPacket", run.PacketId.Value), NodeId("Run", run.Id.Value), "executed-as"));

            foreach (var artifact in run.Artifacts)
            {
                foreach (var reference in artifact.References)
                {
                    edges.Add(Edge(NodeId("Run", run.Id.Value), NodeId(reference.EntityType, reference.EntityId), "references"));
                }
            }
        }

        var packetIds = context.Runs
            .Select(run => run.PacketId.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        foreach (var packetId in packetIds)
        {
            nodes.Add(new CognitiveGraphNode(
                NodeId("ContextPacket", packetId),
                "ContextPacket",
                packetId[..Math.Min(8, packetId.Length)],
                "Generated",
                new Dictionary<string, string>()));
        }

        var metadata = new Dictionary<string, string>
        {
            ["branch"] = head.Branch,
            ["headCommitId"] = head.CommitId?.Value ?? "null",
            ["generatedAtUtc"] = context.Trace.UpdatedAtUtc?.ToString("O") ?? context.Trace.CreatedAtUtc.ToString("O"),
            ["nodeCount"] = nodes.Count.ToString(),
            ["edgeCount"] = edges.Count.ToString()
        };

        return new CognitiveGraphExport(nodes, edges, metadata);
    }

    private static CognitiveGraphLineage BuildHypothesisLineage(CognitiveGraphExport fullGraph, WorkingContext context, Hypothesis hypothesis)
    {
        var focusNodeId = NodeId("Hypothesis", hypothesis.Id.Value);
        var relatedNodeIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            focusNodeId
        };

        foreach (var taskId in hypothesis.TaskIds)
        {
            relatedNodeIds.Add(NodeId("Task", taskId.Value));
        }

        foreach (var evidenceId in hypothesis.EvidenceIds)
        {
            relatedNodeIds.Add(NodeId("Evidence", evidenceId.Value));
        }

        foreach (var decision in context.Decisions.Where(item => item.HypothesisIds.Contains(hypothesis.Id)))
        {
            relatedNodeIds.Add(NodeId("Decision", decision.Id.Value));
            foreach (var evidenceId in decision.EvidenceIds)
            {
                relatedNodeIds.Add(NodeId("Evidence", evidenceId.Value));
            }

            foreach (var conclusion in context.Conclusions.Where(item => item.DecisionIds.Contains(decision.Id)))
            {
                relatedNodeIds.Add(NodeId("Conclusion", conclusion.Id.Value));
                foreach (var evidenceId in conclusion.EvidenceIds)
                {
                    relatedNodeIds.Add(NodeId("Evidence", evidenceId.Value));
                }
            }
        }

        foreach (var evidence in context.Evidence.Where(item => item.Supports.Any(support =>
                     support.EntityType == nameof(Hypothesis)
                     && support.EntityId.Equals(hypothesis.Id.Value, StringComparison.OrdinalIgnoreCase))))
        {
            relatedNodeIds.Add(NodeId("Evidence", evidence.Id.Value));
        }

        foreach (var run in context.Runs.Where(run => run.Artifacts.Any(artifact =>
                     artifact.References.Any(reference =>
                         reference.EntityType == nameof(Hypothesis)
                         && reference.EntityId.Equals(hypothesis.Id.Value, StringComparison.OrdinalIgnoreCase)))))
        {
            relatedNodeIds.Add(NodeId("Run", run.Id.Value));
            relatedNodeIds.Add(NodeId("ContextPacket", run.PacketId.Value));
        }

        var nodes = fullGraph.Nodes.Where(node => relatedNodeIds.Contains(node.Id)).ToArray();
        var edges = fullGraph.Edges.Where(edge => relatedNodeIds.Contains(edge.From) && relatedNodeIds.Contains(edge.To)).ToArray();

        var metadata = new Dictionary<string, string>(fullGraph.Metadata)
        {
            ["focusType"] = "Hypothesis",
            ["focusId"] = hypothesis.Id.Value,
            ["lineageNodeCount"] = nodes.Length.ToString(),
            ["lineageEdgeCount"] = edges.Length.ToString()
        };

        return new CognitiveGraphLineage(
            focusNodeId,
            "Hypothesis",
            new CognitiveGraphExport(nodes, edges, metadata));
    }

    private static CognitiveGraphLineage BuildGoalLineage(CognitiveGraphExport fullGraph, WorkingContext context, Goal goal)
    {
        var focusNodeId = NodeId("Goal", goal.Id.Value);
        var relatedNodeIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            focusNodeId,
            NodeId("Project", context.Project.Id.Value)
        };

        foreach (var taskId in goal.TaskIds)
        {
            relatedNodeIds.Add(NodeId("Task", taskId.Value));

            var task = context.Tasks.SingleOrDefault(item => item.Id.Equals(taskId));
            if (task is null)
            {
                continue;
            }

            foreach (var hypothesisId in task.HypothesisIds)
            {
                relatedNodeIds.Add(NodeId("Hypothesis", hypothesisId.Value));

                var hypothesis = context.Hypotheses.SingleOrDefault(item => item.Id.Equals(hypothesisId));
                if (hypothesis is null)
                {
                    continue;
                }

                foreach (var evidenceId in hypothesis.EvidenceIds)
                {
                    relatedNodeIds.Add(NodeId("Evidence", evidenceId.Value));
                }

                foreach (var decision in context.Decisions.Where(item => item.HypothesisIds.Contains(hypothesis.Id)))
                {
                    relatedNodeIds.Add(NodeId("Decision", decision.Id.Value));

                    foreach (var evidenceId in decision.EvidenceIds)
                    {
                        relatedNodeIds.Add(NodeId("Evidence", evidenceId.Value));
                    }

                    foreach (var conclusion in context.Conclusions.Where(item => item.DecisionIds.Contains(decision.Id)))
                    {
                        relatedNodeIds.Add(NodeId("Conclusion", conclusion.Id.Value));

                        foreach (var evidenceId in conclusion.EvidenceIds)
                        {
                            relatedNodeIds.Add(NodeId("Evidence", evidenceId.Value));
                        }
                    }
                }

                foreach (var run in context.Runs.Where(run => run.Artifacts.Any(artifact =>
                             artifact.References.Any(reference =>
                                 reference.EntityType == nameof(Hypothesis)
                                 && reference.EntityId.Equals(hypothesis.Id.Value, StringComparison.OrdinalIgnoreCase)))))
                {
                    relatedNodeIds.Add(NodeId("Run", run.Id.Value));
                    relatedNodeIds.Add(NodeId("ContextPacket", run.PacketId.Value));
                }
            }
        }

        var nodes = fullGraph.Nodes.Where(node => relatedNodeIds.Contains(node.Id)).ToArray();
        var edges = fullGraph.Edges.Where(edge => relatedNodeIds.Contains(edge.From) && relatedNodeIds.Contains(edge.To)).ToArray();

        var metadata = new Dictionary<string, string>(fullGraph.Metadata)
        {
            ["focusType"] = "Goal",
            ["focusId"] = goal.Id.Value,
            ["lineageNodeCount"] = nodes.Length.ToString(),
            ["lineageEdgeCount"] = edges.Length.ToString()
        };

        return new CognitiveGraphLineage(
            focusNodeId,
            "Goal",
            new CognitiveGraphExport(nodes, edges, metadata));
    }

    private static CognitiveGraphLineage BuildDecisionLineage(CognitiveGraphExport fullGraph, WorkingContext context, Decision decision)
    {
        var focusNodeId = NodeId("Decision", decision.Id.Value);
        var relatedNodeIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            focusNodeId
        };

        foreach (var hypothesisId in decision.HypothesisIds)
        {
            relatedNodeIds.Add(NodeId("Hypothesis", hypothesisId.Value));

            var hypothesis = context.Hypotheses.SingleOrDefault(item => item.Id.Equals(hypothesisId));
            if (hypothesis is null)
            {
                continue;
            }

            foreach (var taskId in hypothesis.TaskIds)
            {
                relatedNodeIds.Add(NodeId("Task", taskId.Value));
            }

            foreach (var evidenceId in hypothesis.EvidenceIds)
            {
                relatedNodeIds.Add(NodeId("Evidence", evidenceId.Value));
            }
        }

        foreach (var evidenceId in decision.EvidenceIds)
        {
            relatedNodeIds.Add(NodeId("Evidence", evidenceId.Value));
        }

        foreach (var conclusion in context.Conclusions.Where(item => item.DecisionIds.Contains(decision.Id)))
        {
            relatedNodeIds.Add(NodeId("Conclusion", conclusion.Id.Value));
            foreach (var evidenceId in conclusion.EvidenceIds)
            {
                relatedNodeIds.Add(NodeId("Evidence", evidenceId.Value));
            }
        }

        foreach (var evidence in context.Evidence.Where(item => item.Supports.Any(support =>
                     support.EntityType == nameof(Decision)
                     && support.EntityId.Equals(decision.Id.Value, StringComparison.OrdinalIgnoreCase))))
        {
            relatedNodeIds.Add(NodeId("Evidence", evidence.Id.Value));
        }

        foreach (var run in context.Runs.Where(run => run.Artifacts.Any(artifact =>
                     artifact.References.Any(reference =>
                         reference.EntityType == nameof(Decision)
                         && reference.EntityId.Equals(decision.Id.Value, StringComparison.OrdinalIgnoreCase)))))
        {
            relatedNodeIds.Add(NodeId("Run", run.Id.Value));
            relatedNodeIds.Add(NodeId("ContextPacket", run.PacketId.Value));
        }

        var nodes = fullGraph.Nodes.Where(node => relatedNodeIds.Contains(node.Id)).ToArray();
        var edges = fullGraph.Edges.Where(edge => relatedNodeIds.Contains(edge.From) && relatedNodeIds.Contains(edge.To)).ToArray();

        var metadata = new Dictionary<string, string>(fullGraph.Metadata)
        {
            ["focusType"] = "Decision",
            ["focusId"] = decision.Id.Value,
            ["lineageNodeCount"] = nodes.Length.ToString(),
            ["lineageEdgeCount"] = edges.Length.ToString()
        };

        return new CognitiveGraphLineage(
            focusNodeId,
            "Decision",
            new CognitiveGraphExport(nodes, edges, metadata));
    }

    private static CognitiveGraphLineage BuildConclusionLineage(CognitiveGraphExport fullGraph, WorkingContext context, Conclusion conclusion)
    {
        var focusNodeId = NodeId("Conclusion", conclusion.Id.Value);
        var relatedNodeIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            focusNodeId
        };

        foreach (var evidenceId in conclusion.EvidenceIds)
        {
            relatedNodeIds.Add(NodeId("Evidence", evidenceId.Value));
        }

        foreach (var goalId in conclusion.GoalIds)
        {
            relatedNodeIds.Add(NodeId("Goal", goalId.Value));
        }

        foreach (var taskId in conclusion.TaskIds)
        {
            relatedNodeIds.Add(NodeId("Task", taskId.Value));
        }

        foreach (var decisionId in conclusion.DecisionIds)
        {
            relatedNodeIds.Add(NodeId("Decision", decisionId.Value));

            var decision = context.Decisions.SingleOrDefault(item => item.Id.Equals(decisionId));
            if (decision is null)
            {
                continue;
            }

            foreach (var evidenceId in decision.EvidenceIds)
            {
                relatedNodeIds.Add(NodeId("Evidence", evidenceId.Value));
            }

            foreach (var hypothesisId in decision.HypothesisIds)
            {
                relatedNodeIds.Add(NodeId("Hypothesis", hypothesisId.Value));

                var hypothesis = context.Hypotheses.SingleOrDefault(item => item.Id.Equals(hypothesisId));
                if (hypothesis is null)
                {
                    continue;
                }

                foreach (var evidenceId in hypothesis.EvidenceIds)
                {
                    relatedNodeIds.Add(NodeId("Evidence", evidenceId.Value));
                }

                foreach (var taskId in hypothesis.TaskIds)
                {
                    relatedNodeIds.Add(NodeId("Task", taskId.Value));

                    var task = context.Tasks.SingleOrDefault(item => item.Id.Equals(taskId));
                    if (task?.GoalId is not null)
                    {
                        relatedNodeIds.Add(NodeId("Goal", task.GoalId.Value.Value));
                    }
                }
            }
        }

        foreach (var run in context.Runs.Where(run => run.Artifacts.Any(artifact =>
                     artifact.References.Any(reference =>
                         reference.EntityType == nameof(Conclusion)
                         && reference.EntityId.Equals(conclusion.Id.Value, StringComparison.OrdinalIgnoreCase)))))
        {
            relatedNodeIds.Add(NodeId("Run", run.Id.Value));
            relatedNodeIds.Add(NodeId("ContextPacket", run.PacketId.Value));
        }

        var nodes = fullGraph.Nodes.Where(node => relatedNodeIds.Contains(node.Id)).ToArray();
        var edges = fullGraph.Edges.Where(edge => relatedNodeIds.Contains(edge.From) && relatedNodeIds.Contains(edge.To)).ToArray();

        var metadata = new Dictionary<string, string>(fullGraph.Metadata)
        {
            ["focusType"] = "Conclusion",
            ["focusId"] = conclusion.Id.Value,
            ["lineageNodeCount"] = nodes.Length.ToString(),
            ["lineageEdgeCount"] = edges.Length.ToString()
        };

        return new CognitiveGraphLineage(
            focusNodeId,
            "Conclusion",
            new CognitiveGraphExport(nodes, edges, metadata));
    }

    private static CognitiveGraphLineage BuildTaskLineage(CognitiveGraphExport fullGraph, WorkingContext context, Ctx.Domain.Task task)
    {
        var focusNodeId = NodeId("Task", task.Id.Value);
        var relatedNodeIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            focusNodeId
        };

        if (task.GoalId is not null)
        {
            relatedNodeIds.Add(NodeId("Goal", task.GoalId.Value.Value));
        }

        foreach (var hypothesisId in task.HypothesisIds)
        {
            relatedNodeIds.Add(NodeId("Hypothesis", hypothesisId.Value));

            var hypothesis = context.Hypotheses.SingleOrDefault(item => item.Id.Equals(hypothesisId));
            if (hypothesis is null)
            {
                continue;
            }

            foreach (var evidenceId in hypothesis.EvidenceIds)
            {
                relatedNodeIds.Add(NodeId("Evidence", evidenceId.Value));
            }

            foreach (var decision in context.Decisions.Where(item => item.HypothesisIds.Contains(hypothesis.Id)))
            {
                relatedNodeIds.Add(NodeId("Decision", decision.Id.Value));

                foreach (var evidenceId in decision.EvidenceIds)
                {
                    relatedNodeIds.Add(NodeId("Evidence", evidenceId.Value));
                }

                foreach (var conclusion in context.Conclusions.Where(item => item.DecisionIds.Contains(decision.Id)))
                {
                    relatedNodeIds.Add(NodeId("Conclusion", conclusion.Id.Value));

                    foreach (var evidenceId in conclusion.EvidenceIds)
                    {
                        relatedNodeIds.Add(NodeId("Evidence", evidenceId.Value));
                    }
                }
            }

            foreach (var run in context.Runs.Where(run => run.Artifacts.Any(artifact =>
                         artifact.References.Any(reference =>
                             reference.EntityType == nameof(Hypothesis)
                             && reference.EntityId.Equals(hypothesis.Id.Value, StringComparison.OrdinalIgnoreCase)))))
            {
                relatedNodeIds.Add(NodeId("Run", run.Id.Value));
                relatedNodeIds.Add(NodeId("ContextPacket", run.PacketId.Value));
            }
        }

        foreach (var evidence in context.Evidence.Where(item => item.Supports.Any(support =>
                     support.EntityType == "Task"
                     && support.EntityId.Equals(task.Id.Value, StringComparison.OrdinalIgnoreCase))))
        {
            relatedNodeIds.Add(NodeId("Evidence", evidence.Id.Value));
        }

        var nodes = fullGraph.Nodes.Where(node => relatedNodeIds.Contains(node.Id)).ToArray();
        var edges = fullGraph.Edges.Where(edge => relatedNodeIds.Contains(edge.From) && relatedNodeIds.Contains(edge.To)).ToArray();

        var metadata = new Dictionary<string, string>(fullGraph.Metadata)
        {
            ["focusType"] = "Task",
            ["focusId"] = task.Id.Value,
            ["lineageNodeCount"] = nodes.Length.ToString(),
            ["lineageEdgeCount"] = edges.Length.ToString()
        };

        return new CognitiveGraphLineage(
            focusNodeId,
            "Task",
            new CognitiveGraphExport(nodes, edges, metadata));
    }

    private static CognitiveGraphEdge Edge(string from, string to, string relationship)
        => new(from, to, relationship, new Dictionary<string, string>());

    private async System.Threading.Tasks.Task<ContextThread> BuildTaskThreadAsync(
        string repositoryPath,
        HeadReference head,
        WorkingContext context,
        Ctx.Domain.Task task,
        CancellationToken cancellationToken)
    {
        var relatedHypotheses = context.Hypotheses
            .Where(item => item.TaskIds.Contains(task.Id) || task.HypothesisIds.Contains(item.Id))
            .ToArray();

        var relatedEvidence = context.Evidence
            .Where(item =>
                item.Supports.Any(support => support.EntityType == "Task" && support.EntityId.Equals(task.Id.Value, StringComparison.OrdinalIgnoreCase))
                || item.Supports.Any(support => support.EntityType == nameof(Hypothesis) && relatedHypotheses.Any(hypothesis => hypothesis.Id.Value.Equals(support.EntityId, StringComparison.OrdinalIgnoreCase)))
                || relatedHypotheses.Any(hypothesis => hypothesis.EvidenceIds.Contains(item.Id)))
            .ToArray();

        var relatedDecisions = context.Decisions
            .Where(item => item.HypothesisIds.Any(hypothesisId => relatedHypotheses.Any(hypothesis => hypothesis.Id.Equals(hypothesisId))))
            .ToArray();

        var relatedConclusions = context.Conclusions
            .Where(item =>
                item.TaskIds.Contains(task.Id)
                || item.DecisionIds.Any(decisionId => relatedDecisions.Any(decision => decision.Id.Equals(decisionId))))
            .ToArray();

        Goal? goal = null;
        if (task.GoalId is not null)
        {
            goal = context.Goals.SingleOrDefault(item => item.Id.Equals(task.GoalId.Value));
        }

        var semanticSteps = new List<CognitiveThreadStep>();
        var order = 1;

        if (goal is not null)
        {
            semanticSteps.Add(new CognitiveThreadStep(order++, "contains", nameof(Goal), goal.Id.Value, goal.Title, goal.State.ToString()));
        }

        semanticSteps.Add(new CognitiveThreadStep(order++, "focus", "Task", task.Id.Value, task.Title, task.State.ToString()));

        foreach (var hypothesis in relatedHypotheses.OrderByDescending(item => item.Score).ThenBy(item => item.Statement, StringComparer.OrdinalIgnoreCase))
        {
            semanticSteps.Add(new CognitiveThreadStep(order++, "informs", nameof(Hypothesis), hypothesis.Id.Value, hypothesis.Statement, hypothesis.State.ToString()));

            foreach (var evidence in relatedEvidence
                         .Where(item =>
                             item.Supports.Any(support => support.EntityType == nameof(Hypothesis) && support.EntityId.Equals(hypothesis.Id.Value, StringComparison.OrdinalIgnoreCase))
                             || hypothesis.EvidenceIds.Contains(item.Id))
                         .OrderBy(item => item.Title, StringComparer.OrdinalIgnoreCase))
            {
                semanticSteps.Add(new CognitiveThreadStep(order++, "supported-by", nameof(Evidence), evidence.Id.Value, evidence.Title, evidence.State.ToString()));
            }

            foreach (var decision in relatedDecisions
                         .Where(item => item.HypothesisIds.Contains(hypothesis.Id))
                         .OrderBy(item => item.Title, StringComparer.OrdinalIgnoreCase))
            {
                semanticSteps.Add(new CognitiveThreadStep(order++, "influences", nameof(Decision), decision.Id.Value, decision.Title, decision.State.ToString()));
            }
        }

        foreach (var conclusion in relatedConclusions.OrderBy(item => item.Summary, StringComparer.OrdinalIgnoreCase))
        {
            semanticSteps.Add(new CognitiveThreadStep(order++, "leads-to", nameof(Conclusion), conclusion.Id.Value, conclusion.Summary, conclusion.State.ToString()));
        }

        var relatedEntityIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            task.Id.Value
        };

        if (goal is not null)
        {
            relatedEntityIds.Add(goal.Id.Value);
        }

        foreach (var hypothesis in relatedHypotheses)
        {
            relatedEntityIds.Add(hypothesis.Id.Value);
        }

        foreach (var evidence in relatedEvidence)
        {
            relatedEntityIds.Add(evidence.Id.Value);
        }

        foreach (var decision in relatedDecisions)
        {
            relatedEntityIds.Add(decision.Id.Value);
        }

        foreach (var conclusion in relatedConclusions)
        {
            relatedEntityIds.Add(conclusion.Id.Value);
        }

        var history = await _commitRepository.GetHistoryAsync(repositoryPath, head.Branch, cancellationToken);
        var timeline = history
            .OrderBy(commit => commit.CreatedAtUtc)
            .SelectMany(commit => BuildThreadTimelineEvents(commit, relatedEntityIds))
            .ToArray();

        var gaps = new List<CognitiveThreadGap>();
        var openQuestions = new List<string>();

        if (relatedHypotheses.Length == 0)
        {
            gaps.Add(new CognitiveThreadGap("MissingHypothesis", "Task", task.Id.Value, "The task has no linked hypotheses."));
            openQuestions.Add("What hypothesis justifies this task?");
        }

        foreach (var hypothesis in relatedHypotheses)
        {
            var hypothesisEvidence = relatedEvidence.Where(item =>
                    item.Supports.Any(support => support.EntityType == nameof(Hypothesis) && support.EntityId.Equals(hypothesis.Id.Value, StringComparison.OrdinalIgnoreCase))
                    || hypothesis.EvidenceIds.Contains(item.Id))
                .ToArray();

            if (hypothesisEvidence.Length == 0)
            {
                gaps.Add(new CognitiveThreadGap("MissingEvidence", nameof(Hypothesis), hypothesis.Id.Value, $"Hypothesis '{hypothesis.Statement}' has no supporting evidence in the current thread."));
                openQuestions.Add($"What evidence supports hypothesis '{hypothesis.Statement}'?");
            }
        }

        if (relatedDecisions.Length == 0)
        {
            gaps.Add(new CognitiveThreadGap("MissingDecision", "Task", task.Id.Value, "No decisions have been linked to this task thread yet."));
            openQuestions.Add("What decision has this task thread produced?");
        }

        if (relatedConclusions.Length == 0)
        {
            gaps.Add(new CognitiveThreadGap("MissingConclusion", "Task", task.Id.Value, "No conclusion has closed this task thread yet."));
            openQuestions.Add("What conclusion closes this task thread?");
        }

        var relatedCommitIds = timeline
            .Select(item => item.CommitId)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new ContextThread(
            new CognitiveThreadFocus("Task", task.Id.Value, task.Title, task.State.ToString()),
            semanticSteps,
            timeline,
            new CognitiveThreadBranchContext(head.Branch, head.CommitId?.Value, relatedCommitIds),
            openQuestions.Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
            gaps);
    }

    private static IReadOnlyList<CognitiveThreadTimelineEvent> BuildThreadTimelineEvents(ContextCommit commit, IReadOnlySet<string> relatedEntityIds)
    {
        var events = new List<CognitiveThreadTimelineEvent>();

        AppendThreadTimelineEvents(events, commit, commit.Diff.Tasks, relatedEntityIds);
        AppendThreadTimelineEvents(events, commit, commit.Diff.Hypotheses, relatedEntityIds);
        AppendThreadTimelineEvents(events, commit, commit.Diff.Evidence, relatedEntityIds);
        AppendThreadTimelineEvents(events, commit, commit.Diff.Decisions, relatedEntityIds);
        AppendThreadTimelineEvents(events, commit, commit.Diff.Conclusions, relatedEntityIds);

        return events;
    }

    private static void AppendThreadTimelineEvents(
        ICollection<CognitiveThreadTimelineEvent> events,
        ContextCommit commit,
        IReadOnlyList<ContextDiffChange> changes,
        IReadOnlySet<string> relatedEntityIds)
    {
        foreach (var change in changes.Where(change => relatedEntityIds.Contains(change.EntityId)))
        {
            events.Add(new CognitiveThreadTimelineEvent(
                commit.Id.Value,
                commit.Branch,
                commit.CreatedAtUtc,
                change.ChangeType,
                change.EntityType,
                change.EntityId,
                change.Summary));
        }
    }

    private static string BuildMermaidGraph(CognitiveGraphExport graph)
    {
        var builder = new StringBuilder();
        builder.AppendLine("graph TD");

        foreach (var node in graph.Nodes)
        {
            builder.Append("    ");
            builder.Append(MermaidNodeId(node.Id));
            builder.Append("[\"");
            builder.Append(EscapeMermaidLabel($"{node.Type}: {node.Label}"));
            builder.AppendLine("\"]");
        }

        foreach (var edge in graph.Edges)
        {
            builder.Append("    ");
            builder.Append(MermaidNodeId(edge.From));
            builder.Append(" -->|");
            builder.Append(EscapeMermaidLabel(edge.Relationship));
            builder.Append("| ");
            builder.AppendLine(MermaidNodeId(edge.To));
        }

        return builder.ToString().TrimEnd();
    }

    private static string BuildThreadMarkdown(ContextThread thread)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"# CTX Thread: {thread.Focus.Label}");
        builder.AppendLine();
        builder.AppendLine($"- Focus: `{thread.Focus.EntityType}:{thread.Focus.EntityId}`");
        builder.AppendLine($"- State: `{thread.Focus.State}`");
        builder.AppendLine($"- Branch: `{thread.BranchContext.Branch}`");
        builder.AppendLine($"- Head Commit: `{thread.BranchContext.HeadCommitId ?? "working"}`");
        builder.AppendLine();

        builder.AppendLine("## Semantic Thread");
        foreach (var step in thread.SemanticThread)
        {
            builder.AppendLine($"- {step.Order}. `{step.Relationship}` -> `{step.EntityType}:{step.EntityId}` {step.Summary} [{step.State}]");
        }

        builder.AppendLine();
        builder.AppendLine("## Timeline");
        if (thread.Timeline.Count == 0)
        {
            builder.AppendLine("- No related commit timeline events were found.");
        }
        else
        {
            foreach (var item in thread.Timeline.OrderBy(entry => entry.CreatedAtUtc).ThenBy(entry => entry.EntityType, StringComparer.OrdinalIgnoreCase))
            {
                builder.AppendLine($"- {item.CreatedAtUtc:yyyy-MM-dd HH:mm:ss} `{item.Branch}` `{item.CommitId[..8]}` {item.ChangeType} {item.EntityType}:{item.EntityId} - {item.Summary}");
            }
        }

        builder.AppendLine();
        builder.AppendLine("## Gaps");
        if (thread.Gaps.Count == 0)
        {
            builder.AppendLine("- No structural gaps detected.");
        }
        else
        {
            foreach (var gap in thread.Gaps)
            {
                builder.AppendLine($"- `{gap.GapType}` on `{gap.EntityType}:{gap.EntityId}`: {gap.Detail}");
            }
        }

        builder.AppendLine();
        builder.AppendLine("## Open Questions");
        if (thread.OpenQuestions.Count == 0)
        {
            builder.AppendLine("- No open questions.");
        }
        else
        {
            foreach (var question in thread.OpenQuestions)
            {
                builder.AppendLine($"- {question}");
            }
        }

        return builder.ToString().TrimEnd();
    }

    private static string NodeId(string entityType, string entityId)
        => $"{NormalizeEntityType(entityType)}:{entityId}";

    private static string MermaidNodeId(string nodeId)
        => "n_" + nodeId.Replace(':', '_').Replace('-', '_');

    private static string EscapeMermaidLabel(string value)
        => value.Replace("\"", "\\\"");

    private static string NormalizeEntityType(string entityType)
        => entityType.Trim() switch
        {
            nameof(Project) => "Project",
            nameof(Goal) => "Goal",
            nameof(Hypothesis) => "Hypothesis",
            nameof(Decision) => "Decision",
            nameof(Evidence) => "Evidence",
            nameof(Conclusion) => "Conclusion",
            nameof(Run) => "Run",
            nameof(ContextPacket) => "ContextPacket",
            "Task" => "Task",
            _ => entityType.Trim()
        };

    private static IReadOnlyList<NextWorkCandidate> BuildNextWorkCandidates(WorkingContext context)
    {
        var goalsById = context.Goals.ToDictionary(item => item.Id.Value, StringComparer.OrdinalIgnoreCase);
        var tasksById = context.Tasks.ToDictionary(item => item.Id.Value, StringComparer.OrdinalIgnoreCase);
        var hypothesesById = context.Hypotheses.ToDictionary(item => item.Id.Value, StringComparer.OrdinalIgnoreCase);
        var acceptedConclusionTaskIds = context.Conclusions
            .Where(item => item.State == ConclusionState.Accepted)
            .SelectMany(item => item.TaskIds)
            .Select(item => item.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var taskCandidates = context.Tasks
            .Where(task => task.State != TaskExecutionState.Done)
            .Select(task =>
            {
                var stateScore = task.State switch
                {
                    TaskExecutionState.InProgress => 1.00m,
                    TaskExecutionState.Ready => 0.80m,
                    TaskExecutionState.Draft => 0.35m,
                    TaskExecutionState.Blocked => 0.15m,
                    _ => 0.10m
                };

                var goalPriorityScore = 0.50m;
                var goalPriority = "unassigned";

                if (task.GoalId is not null && goalsById.TryGetValue(task.GoalId.Value.Value, out var goal))
                {
                    goalPriority = goal.Priority.ToString(CultureInfo.InvariantCulture);
                    goalPriorityScore = NormalizeGoalPriority(goal.Priority, context.Goals);
                }

                var relatedHypotheses = task.HypothesisIds
                    .Select(id => hypothesesById.TryGetValue(id.Value, out var hypothesis) ? hypothesis : null)
                    .Where(hypothesis => hypothesis is not null)
                    .Cast<Hypothesis>()
                    .ToArray();

                var hypothesisScore = relatedHypotheses.Length == 0
                    ? 0.35m
                    : relatedHypotheses.Max(item => item.Score);

                var openDependencies = task.DependsOnTaskIds
                    .Select(id => tasksById.TryGetValue(id.Value, out var dependency) ? dependency : null)
                    .Where(dependency => dependency is not null && dependency.State != TaskExecutionState.Done)
                    .Cast<Ctx.Domain.Task>()
                    .ToArray();

                var dependencyReadinessScore = openDependencies.Length == 0 ? 1.00m : 0.00m;

                var score = Math.Round(
                    (stateScore * 0.40m)
                    + (goalPriorityScore * 0.25m)
                    + (hypothesisScore * 0.25m)
                    + (dependencyReadinessScore * 0.10m),
                    4,
                    MidpointRounding.AwayFromZero);

                return new NextWorkCandidate(
                    "Task",
                    task.Id.Value,
                    task.Title,
                    task.State.ToString(),
                    score,
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["stateScore"] = stateScore.ToString("0.####", CultureInfo.InvariantCulture),
                        ["goalPriority"] = goalPriority,
                        ["goalPriorityScore"] = goalPriorityScore.ToString("0.####", CultureInfo.InvariantCulture),
                        ["hypothesisScore"] = hypothesisScore.ToString("0.####", CultureInfo.InvariantCulture),
                        ["dependencyReadinessScore"] = dependencyReadinessScore.ToString("0.####", CultureInfo.InvariantCulture),
                        ["openDependencies"] = openDependencies.Length.ToString(CultureInfo.InvariantCulture)
                    });
            })
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.Title, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (taskCandidates.Length > 0)
        {
            return taskCandidates;
        }

        return context.Hypotheses
            .Where(hypothesis => hypothesis.State is HypothesisState.Proposed or HypothesisState.UnderEvaluation)
            .Select(hypothesis =>
            {
                var relatedTasks = hypothesis.TaskIds
                    .Select(id => tasksById.TryGetValue(id.Value, out var task) ? task : null)
                    .Where(task => task is not null)
                    .Cast<Ctx.Domain.Task>()
                    .ToArray();

                var hasOnlyClosedTasks = relatedTasks.Length > 0 && relatedTasks.All(task => task.State == TaskExecutionState.Done);
                if (!hasOnlyClosedTasks)
                {
                    return null;
                }

                var hasAcceptedTaskConclusion = relatedTasks.Any(task => acceptedConclusionTaskIds.Contains(task.Id.Value));
                if (hasAcceptedTaskConclusion)
                {
                    return null;
                }

                var highestPriorityGoal = relatedTasks
                    .Select(task => task.GoalId?.Value)
                    .Where(id => id is not null && goalsById.ContainsKey(id))
                    .Select(id => goalsById[id!])
                    .OrderBy(goal => goal.Priority)
                    .FirstOrDefault();

                var goalPriority = highestPriorityGoal?.Priority.ToString(CultureInfo.InvariantCulture) ?? "unassigned";
                var goalPriorityScore = highestPriorityGoal is null
                    ? 0.50m
                    : NormalizeGoalPriority(highestPriorityGoal.Priority, context.Goals);

                var relatedEvidenceCount = hypothesis.EvidenceIds.Count.ToString(CultureInfo.InvariantCulture);
                var relatedTaskCount = relatedTasks.Length.ToString(CultureInfo.InvariantCulture);
                var score = Math.Round(
                    (hypothesis.Score * 0.70m)
                    + (goalPriorityScore * 0.20m)
                    + (0.10m * (relatedTasks.Length > 0 ? 1.00m : 0.25m)),
                    4,
                    MidpointRounding.AwayFromZero);

                return new NextWorkCandidate(
                    "Gap",
                    hypothesis.Id.Value,
                    hypothesis.Statement,
                    "Gap",
                    score,
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["sourceType"] = "Hypothesis",
                        ["hypothesisScore"] = hypothesis.Score.ToString("0.####", CultureInfo.InvariantCulture),
                        ["goalPriority"] = goalPriority,
                        ["goalPriorityScore"] = goalPriorityScore.ToString("0.####", CultureInfo.InvariantCulture),
                        ["relatedClosedTasks"] = relatedTaskCount,
                        ["relatedEvidence"] = relatedEvidenceCount,
                        ["recommendedAction"] = "Open a new task from this gap"
                    });
            })
            .Where(candidate => candidate is not null)
            .Cast<NextWorkCandidate>()
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.Title, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static NextWorkDiagnostics BuildNextWorkDiagnostics(WorkingContext context, IReadOnlyList<NextWorkCandidate> candidates)
    {
        var tasksById = context.Tasks.ToDictionary(item => item.Id.Value, StringComparer.OrdinalIgnoreCase);
        var acceptedConclusionTaskIds = context.Conclusions
            .Where(item => item.State == ConclusionState.Accepted)
            .SelectMany(item => item.TaskIds)
            .Select(item => item.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var openTaskCount = context.Tasks.Count(task => task.State != TaskExecutionState.Done);
        var draftTaskCount = context.Tasks.Count(task => task.State == TaskExecutionState.Draft);
        var readyTaskCount = context.Tasks.Count(task => task.State == TaskExecutionState.Ready);
        var inProgressTaskCount = context.Tasks.Count(task => task.State == TaskExecutionState.InProgress);
        var blockedTaskCount = context.Tasks.Count(task => task.State == TaskExecutionState.Blocked);
        var doneTaskCount = context.Tasks.Count(task => task.State == TaskExecutionState.Done);

        var eligibleHypotheses = context.Hypotheses
            .Where(hypothesis => hypothesis.State is HypothesisState.Proposed or HypothesisState.UnderEvaluation)
            .ToArray();

        var gapExcludedByNonClosedTasks = 0;
        var gapExcludedByAcceptedConclusions = 0;
        var gapCandidateCount = 0;

        foreach (var hypothesis in eligibleHypotheses)
        {
            var relatedTasks = hypothesis.TaskIds
                .Select(id => tasksById.TryGetValue(id.Value, out var task) ? task : null)
                .Where(task => task is not null)
                .Cast<Ctx.Domain.Task>()
                .ToArray();

            var hasOnlyClosedTasks = relatedTasks.Length > 0 && relatedTasks.All(task => task.State == TaskExecutionState.Done);
            if (!hasOnlyClosedTasks)
            {
                gapExcludedByNonClosedTasks++;
                continue;
            }

            var hasAcceptedTaskConclusion = relatedTasks.Any(task => acceptedConclusionTaskIds.Contains(task.Id.Value));
            if (hasAcceptedTaskConclusion)
            {
                gapExcludedByAcceptedConclusions++;
                continue;
            }

            gapCandidateCount++;
        }

        var selectionMode = candidates.FirstOrDefault()?.CandidateType ?? "None";
        var guidance = BuildNextWorkGuidance(selectionMode, openTaskCount, gapCandidateCount, gapExcludedByNonClosedTasks, gapExcludedByAcceptedConclusions);

        return new NextWorkDiagnostics(
            selectionMode,
            openTaskCount,
            draftTaskCount,
            readyTaskCount,
            inProgressTaskCount,
            blockedTaskCount,
            doneTaskCount,
            eligibleHypotheses.Length,
            gapCandidateCount,
            gapExcludedByNonClosedTasks,
            gapExcludedByAcceptedConclusions,
            guidance);
    }

    private static IReadOnlyList<string> BuildNextWorkGuidance(
        string selectionMode,
        int openTaskCount,
        int gapCandidateCount,
        int gapExcludedByNonClosedTasks,
        int gapExcludedByAcceptedConclusions)
    {
        if (string.Equals(selectionMode, "Task", StringComparison.OrdinalIgnoreCase))
        {
            return new[]
            {
                "CTX prioritized open tasks before any gap candidates.",
                "Use the top-ranked task unless recent evidence or blocking conditions change the thread."
            };
        }

        if (string.Equals(selectionMode, "Gap", StringComparison.OrdinalIgnoreCase))
        {
            return new[]
            {
                "No open tasks remained, so CTX promoted the strongest unresolved gap from active hypotheses.",
                "Open a new task from the recommended gap before continuing execution."
            };
        }

        var guidance = new List<string>();
        if (openTaskCount == 0)
        {
            guidance.Add("No open tasks remain in the current workspace.");
        }
        else
        {
            guidance.Add("Open tasks exist, but none produced a ranked next-step candidate. Review task states and dependencies.");
        }

        if (gapCandidateCount == 0)
        {
            guidance.Add("No eligible gap hypotheses were available for promotion into a new task.");
        }

        if (gapExcludedByNonClosedTasks > 0)
        {
            guidance.Add($"Gap promotion skipped {gapExcludedByNonClosedTasks} active hypothesis thread(s) because they still have non-closed tasks.");
        }

        if (gapExcludedByAcceptedConclusions > 0)
        {
            guidance.Add($"Gap promotion skipped {gapExcludedByAcceptedConclusions} hypothesis thread(s) because accepted conclusions already closed their task threads.");
        }

        guidance.Add("Run `ctx audit`, `ctx closeout`, or inspect task/hypothesis state if continuation still feels ambiguous.");
        return guidance;
    }

    private static decimal NormalizeGoalPriority(int priority, IReadOnlyList<Goal> goals)
    {
        if (goals.Count == 0)
        {
            return 0.50m;
        }

        var minPriority = goals.Min(item => item.Priority);
        var maxPriority = goals.Max(item => item.Priority);

        if (minPriority == maxPriority)
        {
            return 1.00m;
        }

        var normalized = 1.00m - ((priority - minPriority) / (decimal)(maxPriority - minPriority));
        return Math.Clamp(normalized, 0.00m, 1.00m);
    }

    private async System.Threading.Tasks.Task<T> ExecuteWriteLockedAsync<T>(string repositoryPath, CancellationToken cancellationToken, Func<System.Threading.Tasks.Task<T>> action)
    {
        if (_repositoryWriteLock is null)
        {
            return await action();
        }

        await using var _ = await _repositoryWriteLock.AcquireAsync(repositoryPath, cancellationToken);
        return await action();
    }
}
