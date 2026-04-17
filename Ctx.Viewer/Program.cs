using Ctx.Application;
using Ctx.Core;
using Ctx.Domain;
using Ctx.Infrastructure;
using Ctx.Persistence;

var viewerProjectRoot = ResolveViewerProjectRoot();
var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = viewerProjectRoot,
    WebRootPath = Path.Combine(viewerProjectRoot, "wwwroot")
});
builder.Services.AddRouting();

var app = builder.Build();
var runtime = Bootstrapper.Create();
var jsonSerializer = new DefaultJsonSerializer();
var workingRepository = new FileSystemWorkingContextRepository(jsonSerializer);
var commitRepository = new FileSystemCommitRepository(jsonSerializer);
var branchRepository = new FileSystemBranchRepository(jsonSerializer);
var operationalRunbookRepository = new FileSystemOperationalRunbookRepository(jsonSerializer);
var cognitiveTriggerRepository = new FileSystemCognitiveTriggerRepository(jsonSerializer);

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapGet("/favicon.ico", () => Results.NoContent());

app.MapGet("/api/overview", async (string? path, string? branch, int? historyLimit, string? historyCursor, CancellationToken cancellationToken) =>
{
    var repositoryPath = ResolveRepositoryPath(path);

    if (!await workingRepository.ExistsAsync(repositoryPath, cancellationToken))
    {
        return Results.NotFound(new { message = $"No .ctx repository found at '{repositoryPath}'." });
    }

    var head = await workingRepository.LoadHeadAsync(repositoryPath, cancellationToken);
    var branches = await branchRepository.ListAsync(repositoryPath, cancellationToken);
    var selectedBranch = string.IsNullOrWhiteSpace(branch) ? head.Branch : branch.Trim();
    var commits = await commitRepository.GetHistoryAsync(repositoryPath, selectedBranch, cancellationToken);
    var allCommits = new Dictionary<string, ContextCommit>(StringComparer.OrdinalIgnoreCase);
    foreach (var branchItem in branches)
    {
        var branchCommits = await commitRepository.GetHistoryAsync(repositoryPath, branchItem.Name, cancellationToken);
        foreach (var commit in branchCommits)
        {
            allCommits.TryAdd(commit.Id.Value, commit);
        }
    }

    var orderedTimelineCommits = allCommits.Values
        .OrderByDescending(commit => commit.CreatedAtUtc)
        .ThenByDescending(commit => commit.Id.Value, StringComparer.OrdinalIgnoreCase)
        .ToArray();

    var branchHeadsByCommit = branches
        .Where(item => item.CommitId is not null)
        .GroupBy(item => item.CommitId!.Value.Value, StringComparer.OrdinalIgnoreCase)
        .ToDictionary(
            group => group.Key,
            group => group.Select(item => item.Name).OrderBy(name => name, StringComparer.OrdinalIgnoreCase).ToArray(),
            StringComparer.OrdinalIgnoreCase);

    var branchTimelineCounts = orderedTimelineCommits
        .GroupBy(commit => commit.Branch, StringComparer.OrdinalIgnoreCase)
        .ToDictionary(
            group => group.Key,
            group => group.Count(),
            StringComparer.OrdinalIgnoreCase);
    var timelinePage = BuildTimelinePage(orderedTimelineCommits, historyCursor, historyLimit);

    var summary = await runtime.ApplicationService.GraphSummaryAsync(repositoryPath, cancellationToken);
    var context = await workingRepository.LoadAsync(repositoryPath, cancellationToken);
    var goalTitles = context.Goals.ToDictionary(goal => goal.Id, goal => goal.Title);
    var taskDtos = context.Tasks
        .Select(task => new
        {
            id = task.Id.Value,
            goalId = task.GoalId?.Value,
            goalTitle = task.GoalId is GoalId goalId && goalTitles.TryGetValue(goalId, out var goalTitle) ? goalTitle : null,
            title = task.Title,
            description = task.Description,
            state = task.State.ToString(),
            hypothesisCount = task.HypothesisIds.Count,
            dependsOnTaskIds = task.DependsOnTaskIds.Select(item => item.Value).ToArray()
        })
        .OrderBy(task => task.state.Equals("InProgress", StringComparison.OrdinalIgnoreCase) ? 0 :
            task.state.Equals("Ready", StringComparison.OrdinalIgnoreCase) ? 1 :
            task.state.Equals("Draft", StringComparison.OrdinalIgnoreCase) ? 2 :
            task.state.Equals("Blocked", StringComparison.OrdinalIgnoreCase) ? 3 : 4)
        .ThenBy(task => task.goalTitle ?? string.Empty, StringComparer.OrdinalIgnoreCase)
        .ThenBy(task => task.title, StringComparer.OrdinalIgnoreCase)
        .ToArray();

    var taskSummary = new
    {
        total = taskDtos.Length,
        open = taskDtos.Count(task => !task.state.Equals("Done", StringComparison.OrdinalIgnoreCase)),
        closed = taskDtos.Count(task => task.state.Equals("Done", StringComparison.OrdinalIgnoreCase)),
        inProgress = taskDtos.Count(task => task.state.Equals("InProgress", StringComparison.OrdinalIgnoreCase)),
        ready = taskDtos.Count(task => task.state.Equals("Ready", StringComparison.OrdinalIgnoreCase)),
        blocked = taskDtos.Count(task => task.state.Equals("Blocked", StringComparison.OrdinalIgnoreCase))
    };

    return Results.Json(new
    {
        productVersion = DomainConstants.ProductVersion,
        repositoryPath,
        currentBranch = head.Branch,
        selectedBranch,
        headCommitId = head.CommitId?.Value,
        branches = branches.Select(item => new
        {
            name = item.Name,
            commitId = item.CommitId?.Value,
            updatedAtUtc = item.UpdatedAtUtc,
            timelineCommitCount = branchTimelineCounts.TryGetValue(item.Name, out var timelineCommitCount) ? timelineCommitCount : 0
        }),
        commits = commits.Select(commit => new
        {
            id = commit.Id.Value,
            branch = commit.Branch,
            author = commit.Trace.CreatedBy,
            modelName = commit.Trace.ModelName,
            modelVersion = commit.Trace.ModelVersion,
            message = commit.Message,
            createdAtUtc = commit.CreatedAtUtc,
            snapshotHash = commit.SnapshotHash,
            summary = commit.Diff.Summary,
            changedEntityCount = CountChangedEntities(commit.Diff),
            changedEntitySummary = BuildChangeSummary(commit.Diff),
            parentIds = commit.ParentIds.Select(parent => parent.Value),
            cognitivePath = BuildCognitivePath(commit)
        }),
        timelineCommits = timelinePage.Items.Select(commit => ToViewerTimelineCommit(commit, branchHeadsByCommit)),
        timelinePage = new
        {
            totalCount = orderedTimelineCommits.Length,
            loadedCount = timelinePage.Items.Count,
            limit = timelinePage.Limit,
            cursor = timelinePage.Cursor,
            nextCursor = timelinePage.NextCursor,
            hasMore = timelinePage.HasMore
        },
        graphSummary = summary.Data,
        tasks = taskDtos,
        taskSummary
    });
});

app.MapGet("/api/history", async (string? path, string? branch, int? limit, string? cursor, CancellationToken cancellationToken) =>
{
    var repositoryPath = ResolveRepositoryPath(path);

    if (!await workingRepository.ExistsAsync(repositoryPath, cancellationToken))
    {
        return Results.NotFound(new { message = $"No .ctx repository found at '{repositoryPath}'." });
    }

    var branches = await branchRepository.ListAsync(repositoryPath, cancellationToken);
    var selectedBranch = string.IsNullOrWhiteSpace(branch)
        ? (await workingRepository.LoadHeadAsync(repositoryPath, cancellationToken)).Branch
        : branch.Trim();
    var allCommits = new Dictionary<string, ContextCommit>(StringComparer.OrdinalIgnoreCase);
    foreach (var branchItem in branches)
    {
        var branchCommits = await commitRepository.GetHistoryAsync(repositoryPath, branchItem.Name, cancellationToken);
        foreach (var commit in branchCommits)
        {
            allCommits.TryAdd(commit.Id.Value, commit);
        }
    }

    var orderedTimelineCommits = allCommits.Values
        .OrderByDescending(commit => commit.CreatedAtUtc)
        .ThenByDescending(commit => commit.Id.Value, StringComparer.OrdinalIgnoreCase)
        .ToArray();
    var branchHeadsByCommit = branches
        .Where(item => item.CommitId is not null)
        .GroupBy(item => item.CommitId!.Value.Value, StringComparer.OrdinalIgnoreCase)
        .ToDictionary(
            group => group.Key,
            group => group.Select(item => item.Name).OrderBy(name => name, StringComparer.OrdinalIgnoreCase).ToArray(),
            StringComparer.OrdinalIgnoreCase);
    var timelinePage = BuildTimelinePage(orderedTimelineCommits, cursor, limit);

    return Results.Json(new
    {
        repositoryPath,
        selectedBranch,
        timelineCommits = timelinePage.Items.Select(commit => ToViewerTimelineCommit(commit, branchHeadsByCommit)),
        timelinePage = new
        {
            totalCount = orderedTimelineCommits.Length,
            loadedCount = timelinePage.Items.Count,
            limit = timelinePage.Limit,
            cursor = timelinePage.Cursor,
            nextCursor = timelinePage.NextCursor,
            hasMore = timelinePage.HasMore
        }
    });
});

app.MapGet("/api/working-context/signal", async (string? path, CancellationToken cancellationToken) =>
{
    var repositoryPath = ResolveRepositoryPath(path);

    if (!await workingRepository.ExistsAsync(repositoryPath, cancellationToken))
    {
        return Results.NotFound(new { message = $"No .ctx repository found at '{repositoryPath}'." });
    }

    var head = await workingRepository.LoadHeadAsync(repositoryPath, cancellationToken);
    var context = await workingRepository.LoadAsync(repositoryPath, cancellationToken);
    var openTasks = context.Tasks
        .Where(task => task.State is not TaskExecutionState.Done)
        .OrderBy(task => task.Id.Value, StringComparer.OrdinalIgnoreCase)
        .Select(task => $"{task.Id.Value}:{task.State}:{task.GoalId?.Value ?? string.Empty}:{task.Title}")
        .ToArray();
    var activeGoals = context.Goals
        .OrderBy(goal => goal.Id.Value, StringComparer.OrdinalIgnoreCase)
        .Select(goal => $"{goal.Id.Value}:{goal.Title}")
        .ToArray();
    var openEvidence = context.Evidence
        .Where(item => item.Supports.Any(link => link.EntityType == "Task" && openTasks.Any(task => task.StartsWith($"{link.EntityId}:", StringComparison.OrdinalIgnoreCase))))
        .OrderBy(item => item.Id.Value, StringComparer.OrdinalIgnoreCase)
        .Select(item => $"{item.Id.Value}:{item.Title}")
        .ToArray();
    var fingerprintSource = string.Join("|", new[]
    {
        repositoryPath,
        head.Branch,
        head.CommitId?.Value ?? "working",
        string.Join(";", openTasks),
        string.Join(";", activeGoals),
        string.Join(";", openEvidence)
    });

    return Results.Json(new
    {
        repositoryPath,
        branch = head.Branch,
        headCommitId = head.CommitId?.Value,
        openTaskCount = openTasks.Length,
        fingerprint = ComputeStableHash(fingerprintSource)
    });
});

app.MapGet("/api/hypotheses/rank", async (string? path, CancellationToken cancellationToken) =>
{
    var repositoryPath = ResolveRepositoryPath(path);
    var result = await runtime.ApplicationService.RankHypothesesAsync(repositoryPath, cancellationToken);
    return result.Success
        ? Results.Json(result.Data)
        : Results.BadRequest(new { message = result.Message });
});

app.MapGet("/api/graph", async (string? path, string? commitId, string? mode, string? focusNodeId, int? depth, CancellationToken cancellationToken) =>
{
    try
    {
        var repositoryPath = ResolveRepositoryPath(path);
        var resolvedCommitId = await ResolveCommitReferenceAsync(repositoryPath, commitId, commitRepository, branchRepository, cancellationToken);
        var result = await runtime.ApplicationService.ExportGraphAsync(
            repositoryPath,
            "json",
            resolvedCommitId,
            mode,
            focusNodeId,
            depth,
            cancellationToken);
        return result.Success
            ? Results.Json(result.Data)
            : Results.BadRequest(new { message = result.Message });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

app.MapGet("/api/commit", async (string id, string? path, CancellationToken cancellationToken) =>
{
    try
    {
        var repositoryPath = ResolveRepositoryPath(path);
        var resolvedCommitId = await ResolveCommitReferenceAsync(repositoryPath, id, commitRepository, branchRepository, cancellationToken);
        var commit = await commitRepository.LoadAsync(repositoryPath, new ContextCommitId(resolvedCommitId ?? id), cancellationToken);
        return commit is null
            ? Results.NotFound(new { message = $"Commit '{id}' was not found." })
            : Results.Json(new
            {
                id = commit.Id.Value,
                branch = commit.Branch,
                author = commit.Trace.CreatedBy,
                modelName = commit.Trace.ModelName,
                modelVersion = commit.Trace.ModelVersion,
                message = commit.Message,
                createdAtUtc = commit.CreatedAtUtc,
                snapshotHash = commit.SnapshotHash,
                changedEntityCount = CountChangedEntities(commit.Diff),
                changedEntitySummary = BuildChangeSummary(commit.Diff),
                parentIds = commit.ParentIds.Select(parent => parent.Value),
                diff = commit.Diff,
                cognitivePath = BuildCognitivePath(commit)
            });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

app.MapGet("/api/playbook", async (string? path, string? goalId, string? taskId, string? purpose, CancellationToken cancellationToken) =>
{
    var repositoryPath = ResolveRepositoryPath(path);

    if (!await workingRepository.ExistsAsync(repositoryPath, cancellationToken))
    {
        return Results.NotFound(new { message = $"No .ctx repository found at '{repositoryPath}'." });
    }

    var working = await workingRepository.LoadAsync(repositoryPath, cancellationToken);
    var runbooks = await operationalRunbookRepository.ListAsync(repositoryPath, cancellationToken);
    var effectivePurpose = string.IsNullOrWhiteSpace(purpose)
        ? "viewer working context"
        : purpose.Trim();
    var packetResult = await runtime.ApplicationService.ContextAsync(repositoryPath, effectivePurpose, goalId, taskId, cancellationToken);

    if (!packetResult.Success || packetResult.Data is not ContextPacket packet)
    {
        return Results.BadRequest(new { message = packetResult.Message });
    }

    var selectedGoals = working.Goals
        .Where(goal => packet.GoalIds.Contains(goal.Id))
        .ToArray();
    var selectedTasks = working.Tasks
        .Where(task => packet.TaskIds.Contains(task.Id))
        .ToArray();
    var selection = OperationalRunbookSelection.Select(runbooks, effectivePurpose, goalId, taskId, selectedGoals, selectedTasks);

    return Results.Json(new
    {
        purpose = effectivePurpose,
        selected = selection.Selected.Select(runbook => new
        {
            id = runbook.Id.Value,
            title = runbook.Title,
            kind = runbook.Kind.ToString(),
            whenToUse = runbook.WhenToUse,
            @do = runbook.Do.Take(5).ToArray(),
            verify = runbook.Verify.Take(4).ToArray(),
            references = runbook.References.Take(5).ToArray(),
            goalIds = runbook.GoalIds.Select(item => item.Value).ToArray(),
            taskIds = runbook.TaskIds.Select(item => item.Value).ToArray()
        }),
        available = selection.Available.Select(runbook => new
        {
            id = runbook.Id.Value,
            title = runbook.Title,
            kind = runbook.Kind.ToString()
        })
    });
});

app.MapGet("/api/origin", async (string? path, string? goalId, string? taskId, string? purpose, CancellationToken cancellationToken) =>
{
    var repositoryPath = ResolveRepositoryPath(path);

    if (!await workingRepository.ExistsAsync(repositoryPath, cancellationToken))
    {
        return Results.NotFound(new { message = $"No .ctx repository found at '{repositoryPath}'." });
    }

    var working = await workingRepository.LoadAsync(repositoryPath, cancellationToken);
    var triggers = await cognitiveTriggerRepository.ListAsync(repositoryPath, cancellationToken);
    var runbooks = await operationalRunbookRepository.ListAsync(repositoryPath, cancellationToken);
    var effectivePurpose = string.IsNullOrWhiteSpace(purpose)
        ? "viewer working context"
        : purpose.Trim();
    var packetResult = await runtime.ApplicationService.ContextAsync(repositoryPath, effectivePurpose, goalId, taskId, cancellationToken);

    if (!packetResult.Success || packetResult.Data is not ContextPacket packet)
    {
        return Results.BadRequest(new { message = packetResult.Message });
    }

    var selectedGoals = working.Goals
        .Where(goal => packet.GoalIds.Contains(goal.Id))
        .ToArray();
    var selectedTasks = working.Tasks
        .Where(task => packet.TaskIds.Contains(task.Id))
        .ToArray();
    var selection = SelectOriginTriggers(triggers, goalId, taskId, selectedGoals, selectedTasks, working.Tasks);
    var goalTitles = working.Goals.ToDictionary(item => item.Id, item => item.Title);
    var taskTitles = working.Tasks.ToDictionary(item => item.Id, item => item.Title);
    var runbookTitles = runbooks.ToDictionary(item => item.Id, item => item.Title);

    return Results.Json(new
    {
        purpose = effectivePurpose,
        selected = selection.Selected.Select(item => new
        {
            id = item.Trigger.Id.Value,
            kind = item.Trigger.Kind.ToString(),
            resolution = item.Resolution,
            summary = item.Trigger.Summary,
            createdBy = item.Trigger.Trace.CreatedBy,
            createdAtUtc = item.Trigger.Trace.CreatedAtUtc,
            text = string.IsNullOrWhiteSpace(item.Trigger.Text)
                ? null
                : item.Trigger.Text.Length > 240
                    ? $"{item.Trigger.Text[..240]}..."
                    : item.Trigger.Text,
            goalIds = item.Trigger.GoalIds.Select(id => id.Value).ToArray(),
            goalTitles = item.Trigger.GoalIds
                .Select(id => goalTitles.TryGetValue(id, out var title) ? title : null)
                .Where(title => !string.IsNullOrWhiteSpace(title))
                .ToArray(),
            taskIds = item.Trigger.TaskIds.Select(id => id.Value).ToArray(),
            taskTitles = item.Trigger.TaskIds
                .Select(id => taskTitles.TryGetValue(id, out var title) ? title : null)
                .Where(title => !string.IsNullOrWhiteSpace(title))
                .ToArray(),
            runbookIds = item.Trigger.OperationalRunbookIds.Select(id => id.Value).ToArray(),
            runbookTitles = item.Trigger.OperationalRunbookIds
                .Select(id => runbookTitles.TryGetValue(id, out var title) ? title : null)
                .Where(title => !string.IsNullOrWhiteSpace(title))
                .ToArray()
        }),
        available = selection.Available.Select(item => new
        {
            id = item.Trigger.Id.Value,
            kind = item.Trigger.Kind.ToString(),
            resolution = item.Resolution,
            summary = item.Trigger.Summary
        })
    });
});

app.Run();

static string ResolveRepositoryPath(string? path)
    => string.IsNullOrWhiteSpace(path) ? ResolveDefaultRepositoryRoot() : Path.GetFullPath(path);

static async Task<string?> ResolveCommitReferenceAsync(
    string repositoryPath,
    string? commitId,
    ICommitRepository commitRepository,
    IBranchRepository branchRepository,
    CancellationToken cancellationToken)
{
    if (string.IsNullOrWhiteSpace(commitId))
    {
        return commitId;
    }

    var trimmedCommitId = commitId.Trim();
    var exactCommit = await commitRepository.LoadAsync(repositoryPath, new ContextCommitId(trimmedCommitId), cancellationToken);
    if (exactCommit is not null)
    {
        return exactCommit.Id.Value;
    }

    var branches = await branchRepository.ListAsync(repositoryPath, cancellationToken);
    var matches = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    foreach (var branch in branches)
    {
        var history = await commitRepository.GetHistoryAsync(repositoryPath, branch.Name, cancellationToken);
        foreach (var commit in history)
        {
            if (commit.Id.Value.StartsWith(trimmedCommitId, StringComparison.OrdinalIgnoreCase))
            {
                matches.Add(commit.Id.Value);
            }
        }
    }

    return matches.Count switch
    {
        0 => throw new InvalidOperationException($"Commit '{trimmedCommitId}' was not found."),
        1 => matches.Single(),
        _ => throw new InvalidOperationException($"Commit '{trimmedCommitId}' is ambiguous. Use a longer id.")
    };
}

static (IReadOnlyList<(CognitiveTrigger Trigger, string Resolution)> Selected, IReadOnlyList<(CognitiveTrigger Trigger, string Resolution)> Available) SelectOriginTriggers(
    IReadOnlyList<CognitiveTrigger> triggers,
    string? goalId,
    string? taskId,
    IReadOnlyList<Goal> selectedGoals,
    IReadOnlyList<Ctx.Domain.Task> selectedTasks,
    IReadOnlyList<Ctx.Domain.Task> allTasks)
{
    if (triggers.Count == 0)
    {
        return (Array.Empty<(CognitiveTrigger, string)>(), Array.Empty<(CognitiveTrigger, string)>());
    }

    var directRanked = triggers
        .Where(item => item.State != LifecycleState.Archived)
        .Select(item =>
        {
            var taskMatch = !string.IsNullOrWhiteSpace(taskId) && item.TaskIds.Any(id => id.Value.Equals(taskId, StringComparison.OrdinalIgnoreCase));
            var goalMatch = !string.IsNullOrWhiteSpace(goalId) && item.GoalIds.Any(id => id.Value.Equals(goalId, StringComparison.OrdinalIgnoreCase));
            var selectedTaskMatch = item.TaskIds.Any(id => selectedTasks.Any(task => task.Id == id));
            var selectedGoalMatch = item.GoalIds.Any(id => selectedGoals.Any(goal => goal.Id == id));
            var global = item.TaskIds.Count == 0 && item.GoalIds.Count == 0;
            var score =
                (taskMatch ? 100 : 0) +
                (goalMatch ? 80 : 0) +
                (selectedTaskMatch ? 40 : 0) +
                (selectedGoalMatch ? 20 : 0) +
                (global ? 5 : 0);

            return new { Trigger = item, Score = score };
        })
        .Where(item => item.Score > 0)
        .OrderByDescending(item => item.Score)
        .ThenByDescending(item => item.Trigger.Trace.CreatedAtUtc)
        .ToArray();

    if (directRanked.Length > 0)
    {
        return (
            directRanked.Take(2).Select(item => (item.Trigger, "direct")).ToArray(),
            directRanked.Skip(2).Select(item => (item.Trigger, "direct")).ToArray());
    }

    var selectedTaskGoalIds = selectedTasks
        .Where(task => task.GoalId is not null)
        .Select(task => task.GoalId!.Value.Value)
        .ToHashSet(StringComparer.OrdinalIgnoreCase);
    var selectedTaskCreatedAtUtc = selectedTasks
        .Where(task => !string.IsNullOrWhiteSpace(taskId) && task.Id.Value.Equals(taskId, StringComparison.OrdinalIgnoreCase))
        .Select(task => task.Trace.CreatedAtUtc)
        .DefaultIfEmpty(DateTimeOffset.MaxValue)
        .Max();
    var selectedGoalIds = selectedGoals
        .Select(goal => goal.Id.Value)
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

    if (selectedTaskGoalIds.Count == 0 && selectedGoalIds.Count == 0)
    {
        return (Array.Empty<(CognitiveTrigger, string)>(), Array.Empty<(CognitiveTrigger, string)>());
    }

    var tasksById = allTasks.ToDictionary(task => task.Id.Value, StringComparer.OrdinalIgnoreCase);
    var fallbackRanked = triggers
        .Where(item => item.State != LifecycleState.Archived)
        .Select(item =>
        {
            var triggerTaskGoalMatch = item.TaskIds.Any(id =>
                tasksById.TryGetValue(id.Value, out var triggerTask) &&
                triggerTask.GoalId is GoalId triggerGoal &&
                triggerTask.Trace.CreatedAtUtc <= selectedTaskCreatedAtUtc &&
                (selectedTaskGoalIds.Contains(triggerGoal.Value) || selectedGoalIds.Contains(triggerGoal.Value)));
            var triggerGoalMatch = item.GoalIds.Any(id => selectedGoalIds.Contains(id.Value));
            var score = (triggerTaskGoalMatch ? 30 : 0) + (triggerGoalMatch ? 20 : 0);
            return new { Trigger = item, Score = score };
        })
        .Where(item => item.Score > 0)
        .OrderByDescending(item => item.Score)
        .ThenByDescending(item => item.Trigger.Trace.CreatedAtUtc)
        .ToArray();

    return (
        fallbackRanked.Take(2).Select(item => (item.Trigger, "inherited")).ToArray(),
        fallbackRanked.Skip(2).Select(item => (item.Trigger, "inherited")).ToArray());
}

static string ResolveDefaultRepositoryRoot()
{
    var configuredDefaultPath = Environment.GetEnvironmentVariable("CTX_VIEWER_DEFAULT_REPOSITORY_PATH")
        ?? Environment.GetEnvironmentVariable("Viewer__DefaultRepositoryPath");

    if (!string.IsNullOrWhiteSpace(configuredDefaultPath))
    {
        return Path.GetFullPath(configuredDefaultPath);
    }

    var currentDirectory = Directory.GetCurrentDirectory();
    var directory = new DirectoryInfo(currentDirectory);

    while (directory is not null)
    {
        var gitDirectory = Path.Combine(directory.FullName, ".git");
        if (Directory.Exists(gitDirectory) || File.Exists(gitDirectory))
        {
            return directory.FullName;
        }

        directory = directory.Parent;
    }

    return currentDirectory;
}

static string ResolveViewerProjectRoot()
{
    var currentDirectory = new DirectoryInfo(AppContext.BaseDirectory);

    // Published installs run without the source project file, so prefer the
    // executable directory when it already contains the bundled web root.
    var publishedWebRoot = Path.Combine(currentDirectory.FullName, "wwwroot");
    if (Directory.Exists(publishedWebRoot))
    {
        return currentDirectory.FullName;
    }

    while (currentDirectory is not null)
    {
        var projectFile = Path.Combine(currentDirectory.FullName, "Ctx.Viewer.csproj");
        var webRoot = Path.Combine(currentDirectory.FullName, "wwwroot");
        if (File.Exists(projectFile) && Directory.Exists(webRoot))
        {
            return currentDirectory.FullName;
        }

        currentDirectory = currentDirectory.Parent;
    }

    return Directory.GetCurrentDirectory();
}

static int CountChangedEntities(ContextDiff diff)
    => diff.Decisions.Count
     + diff.Hypotheses.Count
     + diff.Evidence.Count
     + diff.Tasks.Count
     + diff.Conclusions.Count
     + diff.Conflicts.Count;

static string BuildChangeSummary(ContextDiff diff)
{
    var parts = new List<string>();

    if (diff.Tasks.Count > 0)
    {
        parts.Add($"{diff.Tasks.Count} task");
    }

    if (diff.Hypotheses.Count > 0)
    {
        parts.Add($"{diff.Hypotheses.Count} hypo");
    }

    if (diff.Evidence.Count > 0)
    {
        parts.Add($"{diff.Evidence.Count} ev");
    }

    if (diff.Decisions.Count > 0)
    {
        parts.Add($"{diff.Decisions.Count} dec");
    }

    if (diff.Conclusions.Count > 0)
    {
        parts.Add($"{diff.Conclusions.Count} con");
    }

    if (diff.Conflicts.Count > 0)
    {
        parts.Add($"{diff.Conflicts.Count} conflict");
    }

    return parts.Count == 0 ? "No entity changes" : string.Join(", ", parts);
}

static string ComputeStableHash(string input)
{
    var bytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(input));
    return Convert.ToHexString(bytes);
}

static ViewerCognitivePath BuildCognitivePath(ContextCommit commit)
{
    var snapshot = commit.Snapshot?.WorkingContext;
    if (snapshot is null)
    {
        return EmptyViewerCognitivePath();
    }

    var goalsById = snapshot.Goals.ToDictionary(goal => goal.Id.Value, StringComparer.OrdinalIgnoreCase);
    var tasksById = snapshot.Tasks.ToDictionary(task => task.Id.Value, StringComparer.OrdinalIgnoreCase);
    var hypothesesById = snapshot.Hypotheses.ToDictionary(hypothesis => hypothesis.Id.Value, StringComparer.OrdinalIgnoreCase);
    var decisionsById = snapshot.Decisions.ToDictionary(decision => decision.Id.Value, StringComparer.OrdinalIgnoreCase);
    var conclusionsById = snapshot.Conclusions.ToDictionary(conclusion => conclusion.Id.Value, StringComparer.OrdinalIgnoreCase);

    var rootGoalIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    var subGoalIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    var taskIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    var hypothesisIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    var decisionIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    var conclusionIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    foreach (var change in commit.Diff.Tasks)
    {
        if (!tasksById.TryGetValue(change.EntityId, out var task))
        {
            continue;
        }

        taskIds.Add(task.Id.Value);
        AddGoalHierarchy(rootGoalIds, subGoalIds, goalsById, task.GoalId);
    }

    foreach (var change in commit.Diff.Hypotheses)
    {
        if (!hypothesesById.TryGetValue(change.EntityId, out var hypothesis))
        {
            continue;
        }

        hypothesisIds.Add(hypothesis.Id.Value);
        foreach (var taskId in hypothesis.TaskIds)
        {
            AddTaskAndGoal(taskIds, rootGoalIds, subGoalIds, goalsById, tasksById, taskId);
        }
    }

    foreach (var change in commit.Diff.Decisions)
    {
        if (!decisionsById.TryGetValue(change.EntityId, out var decision))
        {
            continue;
        }

        decisionIds.Add(decision.Id.Value);
        foreach (var hypothesisId in decision.HypothesisIds)
        {
            if (!hypothesesById.TryGetValue(hypothesisId.Value, out var hypothesis))
            {
                continue;
            }

            hypothesisIds.Add(hypothesis.Id.Value);
            foreach (var taskId in hypothesis.TaskIds)
            {
                AddTaskAndGoal(taskIds, rootGoalIds, subGoalIds, goalsById, tasksById, taskId);
            }
        }
    }

    foreach (var change in commit.Diff.Conclusions)
    {
        if (!conclusionsById.TryGetValue(change.EntityId, out var conclusion))
        {
            continue;
        }

        conclusionIds.Add(conclusion.Id.Value);
        foreach (var goalId in conclusion.GoalIds)
        {
            AddGoalHierarchy(rootGoalIds, subGoalIds, goalsById, goalId);
        }

        foreach (var taskId in conclusion.TaskIds)
        {
            AddTaskAndGoal(taskIds, rootGoalIds, subGoalIds, goalsById, tasksById, taskId);
        }

        foreach (var decisionId in conclusion.DecisionIds)
        {
            if (!decisionsById.TryGetValue(decisionId.Value, out var decision))
            {
                continue;
            }

            decisionIds.Add(decision.Id.Value);
            foreach (var hypothesisId in decision.HypothesisIds)
            {
                if (!hypothesesById.TryGetValue(hypothesisId.Value, out var hypothesis))
                {
                    continue;
                }

                hypothesisIds.Add(hypothesis.Id.Value);
                foreach (var taskId in hypothesis.TaskIds)
                {
                    AddTaskAndGoal(taskIds, rootGoalIds, subGoalIds, goalsById, tasksById, taskId);
                }
            }
        }
    }

    var goalTitles = rootGoalIds
        .Select(id => goalsById.TryGetValue(id, out var goal) ? goal.Title : null)
        .Where(title => !string.IsNullOrWhiteSpace(title))
        .Cast<string>()
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();
    var subGoalTitles = subGoalIds
        .Select(id => goalsById.TryGetValue(id, out var goal) ? goal.Title : null)
        .Where(title => !string.IsNullOrWhiteSpace(title))
        .Cast<string>()
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();
    var taskTitles = taskIds
        .Select(id => tasksById.TryGetValue(id, out var task) ? task.Title : null)
        .Where(title => !string.IsNullOrWhiteSpace(title))
        .Cast<string>()
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();
    var hypothesisTitles = hypothesisIds
        .Select(id => hypothesesById.TryGetValue(id, out var hypothesis) ? hypothesis.Statement : null)
        .Where(title => !string.IsNullOrWhiteSpace(title))
        .Cast<string>()
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();
    var decisionTitles = decisionIds
        .Select(id => decisionsById.TryGetValue(id, out var decision) ? decision.Title : null)
        .Where(title => !string.IsNullOrWhiteSpace(title))
        .Cast<string>()
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();
    var conclusionSummaries = conclusionIds
        .Select(id => conclusionsById.TryGetValue(id, out var conclusion) ? conclusion.Summary : null)
        .Where(title => !string.IsNullOrWhiteSpace(title))
        .Cast<string>()
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();

    return new ViewerCognitivePath(
        rootGoalIds.Concat(subGoalIds).Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
        subGoalIds.ToArray(),
        taskIds.ToArray(),
        hypothesisIds.ToArray(),
        decisionIds.ToArray(),
        conclusionIds.ToArray(),
        goalTitles,
        subGoalTitles,
        taskTitles,
        hypothesisTitles,
        decisionTitles,
        conclusionSummaries);
}

static ViewerCognitivePath EmptyViewerCognitivePath()
    => new(
        Array.Empty<string>(),
        Array.Empty<string>(),
        Array.Empty<string>(),
        Array.Empty<string>(),
        Array.Empty<string>(),
        Array.Empty<string>(),
        Array.Empty<string>(),
        Array.Empty<string>(),
        Array.Empty<string>(),
        Array.Empty<string>(),
        Array.Empty<string>(),
        Array.Empty<string>());

static void AddGoalHierarchy(
    HashSet<string> rootGoalIds,
    HashSet<string> subGoalIds,
    IReadOnlyDictionary<string, Goal> goalsById,
    GoalId? goalId)
{
    if (goalId.HasValue)
    {
        if (goalsById.TryGetValue(goalId.Value.Value, out var goal) && goal.ParentGoalId.HasValue)
        {
            subGoalIds.Add(goal.Id.Value);
            rootGoalIds.Add(goal.ParentGoalId.Value.Value);
        }
        else
        {
            rootGoalIds.Add(goalId.Value.Value);
        }
    }
}

static void AddTaskAndGoal(
    HashSet<string> taskIds,
    HashSet<string> rootGoalIds,
    HashSet<string> subGoalIds,
    IReadOnlyDictionary<string, Goal> goalsById,
    IReadOnlyDictionary<string, Ctx.Domain.Task> tasksById,
    TaskId taskId)
{
    if (!tasksById.TryGetValue(taskId.Value, out var task))
    {
        return;
    }

    taskIds.Add(task.Id.Value);
    AddGoalHierarchy(rootGoalIds, subGoalIds, goalsById, task.GoalId);
}

static ViewerTimelinePage BuildTimelinePage(IReadOnlyList<ContextCommit> orderedTimelineCommits, string? cursor, int? limit)
{
    var normalizedLimit = NormalizeTimelinePageLimit(limit);
    var startIndex = ResolveTimelinePageStartIndex(orderedTimelineCommits, cursor);
    var items = orderedTimelineCommits
        .Skip(startIndex)
        .Take(normalizedLimit)
        .ToArray();
    var nextIndex = startIndex + items.Length;
    var hasMore = nextIndex < orderedTimelineCommits.Count;
    var nextCursor = hasMore && items.Length > 0
        ? EncodeTimelineCursor(items[^1])
        : null;

    return new ViewerTimelinePage(items, normalizedLimit, cursor, nextCursor, hasMore);
}

static int NormalizeTimelinePageLimit(int? limit)
{
    const int defaultLimit = 40;
    const int maxLimit = 120;

    if (!limit.HasValue || limit.Value <= 0)
    {
        return defaultLimit;
    }

    return Math.Min(limit.Value, maxLimit);
}

static int ResolveTimelinePageStartIndex(IReadOnlyList<ContextCommit> orderedTimelineCommits, string? cursor)
{
    if (!TryDecodeTimelineCursor(cursor, out var cursorTicks, out var cursorCommitId))
    {
        return 0;
    }

    for (var index = 0; index < orderedTimelineCommits.Count; index++)
    {
        var commit = orderedTimelineCommits[index];
        if (commit.CreatedAtUtc.UtcTicks == cursorTicks
            && string.Equals(commit.Id.Value, cursorCommitId, StringComparison.OrdinalIgnoreCase))
        {
            return index + 1;
        }
    }

    for (var index = 0; index < orderedTimelineCommits.Count; index++)
    {
        var commit = orderedTimelineCommits[index];
        var tickComparison = commit.CreatedAtUtc.UtcTicks.CompareTo(cursorTicks);
        if (tickComparison < 0)
        {
            return index;
        }

        if (tickComparison == 0
            && StringComparer.OrdinalIgnoreCase.Compare(commit.Id.Value, cursorCommitId) < 0)
        {
            return index;
        }
    }

    return orderedTimelineCommits.Count;
}

static bool TryDecodeTimelineCursor(string? cursor, out long ticks, out string commitId)
{
    ticks = 0;
    commitId = string.Empty;

    if (string.IsNullOrWhiteSpace(cursor))
    {
        return false;
    }

    var parts = cursor.Split('|', 2, StringSplitOptions.TrimEntries);
    if (parts.Length != 2 || !long.TryParse(parts[0], out ticks) || string.IsNullOrWhiteSpace(parts[1]))
    {
        ticks = 0;
        commitId = string.Empty;
        return false;
    }

    commitId = parts[1];
    return true;
}

static string EncodeTimelineCursor(ContextCommit commit)
    => $"{commit.CreatedAtUtc.UtcTicks}|{commit.Id.Value}";

static object ToViewerTimelineCommit(
    ContextCommit commit,
    IReadOnlyDictionary<string, string[]> branchHeadsByCommit)
    => new
    {
        id = commit.Id.Value,
        branch = commit.Branch,
        author = commit.Trace.CreatedBy,
        modelName = commit.Trace.ModelName,
        modelVersion = commit.Trace.ModelVersion,
        message = commit.Message,
        createdAtUtc = commit.CreatedAtUtc,
        snapshotHash = commit.SnapshotHash,
        summary = commit.Diff.Summary,
        changedEntityCount = CountChangedEntities(commit.Diff),
        changedEntitySummary = BuildChangeSummary(commit.Diff),
        parentIds = commit.ParentIds.Select(parent => parent.Value),
        headBranches = branchHeadsByCommit.TryGetValue(commit.Id.Value, out var headBranches) ? headBranches : Array.Empty<string>(),
        cognitivePath = BuildCognitivePath(commit)
    };

internal record ViewerCognitivePath(
    IReadOnlyList<string> GoalIds,
    IReadOnlyList<string> SubGoalIds,
    IReadOnlyList<string> TaskIds,
    IReadOnlyList<string> HypothesisIds,
    IReadOnlyList<string> DecisionIds,
    IReadOnlyList<string> ConclusionIds,
    IReadOnlyList<string> GoalTitles,
    IReadOnlyList<string> SubGoalTitles,
    IReadOnlyList<string> TaskTitles,
    IReadOnlyList<string> HypothesisTitles,
    IReadOnlyList<string> DecisionTitles,
    IReadOnlyList<string> ConclusionSummaries);

internal record ViewerTimelinePage(
    IReadOnlyList<ContextCommit> Items,
    int Limit,
    string? Cursor,
    string? NextCursor,
    bool HasMore);
