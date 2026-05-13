using Ctx.Application;
using Ctx.Core;
using Ctx.Domain;
using Ctx.Infrastructure;
using Ctx.Persistence;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Text.Json;

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
var mcpSupervisor = new McpProcessSupervisor();
var releaseStatusClient = new HttpClient
{
    Timeout = TimeSpan.FromSeconds(5)
};
releaseStatusClient.DefaultRequestHeaders.UserAgent.ParseAdd("CTX-Viewer");

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapGet("/favicon.ico", () => Results.NoContent());

app.MapGet("/api/browse-directory", async () =>
{
    if (!OperatingSystem.IsWindows())
    {
        return Results.BadRequest(new { message = "Browse directory is only supported in the local Windows viewer today." });
    }

    var selectedPath = await TrySelectDirectoryOnWindowsAsync();
    if (string.IsNullOrWhiteSpace(selectedPath))
    {
        return Results.Json(new { path = (string?)null, cancelled = true });
    }

    return Results.Json(new { path = selectedPath, cancelled = false });
});

app.MapGet("/api/repository-candidates", (string? path, bool? selectedOnly) =>
{
    var roots = selectedOnly.GetValueOrDefault(false)
        ? BuildSelectedRepositoryCandidateRoots(path)
        : BuildRepositoryCandidateRoots(path);
    var repositories = FindRepositoryCandidates(roots);
    return Results.Json(new
    {
        roots,
        repositories
    });
});

app.MapGet("/api/mcp-status", (string? path, bool? ensure) =>
{
    var repositoryPath = ResolveRepositoryPath(path);
    return Results.Json(GetLocalMcpStatus(repositoryPath, ensure.GetValueOrDefault(false), mcpSupervisor));
});

app.MapPost("/api/mcp-start", (string? path) =>
{
    var repositoryPath = ResolveRepositoryPath(path);
    return Results.Json(GetLocalMcpStatus(repositoryPath, ensureRunning: true, mcpSupervisor));
});

app.MapPost("/api/mcp-stop", (string? path) =>
{
    var repositoryPath = ResolveRepositoryPath(path);
    mcpSupervisor.Stop(repositoryPath);
    var stoppedProcessCount = StopLocalMcpProcesses();
    return Results.Json(GetLocalMcpStatus(repositoryPath, ensureRunning: false, mcpSupervisor, stoppedProcessCount));
});

app.MapGet("/api/release-status", async (CancellationToken cancellationToken) =>
{
    return Results.Json(await GetReleaseStatusAsync(releaseStatusClient, cancellationToken));
});

app.MapGet("/api/overview", async (string? path, string? branch, int? historyLimit, string? historyCursor, string? timelineScope, CancellationToken cancellationToken) =>
{
    var repositoryPath = ResolveRepositoryPath(path);

    if (!await workingRepository.ExistsAsync(repositoryPath, cancellationToken))
    {
        return Results.NotFound(new { message = $"No .ctx repository found at '{repositoryPath}'." });
    }

    var head = await workingRepository.LoadHeadAsync(repositoryPath, cancellationToken);
    var branches = await branchRepository.ListAsync(repositoryPath, cancellationToken);
    var selectedBranch = string.IsNullOrWhiteSpace(branch) ? head.Branch : branch.Trim();
    var normalizedTimelineScope = NormalizeTimelineScope(timelineScope);

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
        productVersionLabel = ResolveViewerVersionLabel(viewerProjectRoot),
        repositoryPath,
        currentBranch = head.Branch,
        selectedBranch,
        headCommitId = head.CommitId?.Value,
        branches = branches.Select(item => new
        {
            name = item.Name,
            commitId = item.CommitId?.Value,
            updatedAtUtc = item.UpdatedAtUtc,
            timelineCommitCount = (int?)null
        }),
        timelineCommits = Array.Empty<object>(),
        timelinePage = new
        {
            totalCount = (int?)null,
            loadedCount = 0,
            limit = historyLimit.GetValueOrDefault(20),
            cursor = historyCursor,
            nextCursor = (string?)null,
            hasMore = false
        },
        timelineScope = normalizedTimelineScope,
        historyPending = true,
        graphSummary = (object?)null,
        tasks = taskDtos,
        taskSummary
    });
});

app.MapGet("/api/graph-summary", async (string? path, CancellationToken cancellationToken) =>
{
    var repositoryPath = ResolveRepositoryPath(path);

    if (!await workingRepository.ExistsAsync(repositoryPath, cancellationToken))
    {
        return Results.NotFound(new { message = $"No .ctx repository found at '{repositoryPath}'." });
    }

    var summary = await runtime.ApplicationService.GraphSummaryAsync(repositoryPath, cancellationToken);
    return summary.Success
        ? Results.Json(new
        {
            repositoryPath,
            graphSummary = summary.Data
        })
        : Results.BadRequest(new { message = summary.Message });
});

app.MapGet("/api/history", async (string? path, string? branch, int? limit, string? cursor, string? timelineScope, CancellationToken cancellationToken) =>
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
    var normalizedTimelineScope = NormalizeTimelineScope(timelineScope);
    var orderedTimelineHeaders = await BuildTimelineCommitHeadersAsync(
        repositoryPath,
        selectedBranch,
        normalizedTimelineScope,
        cancellationToken);
    var branchHeadsByCommit = branches
        .Where(item => item.CommitId is not null)
        .GroupBy(item => item.CommitId!.Value.Value, StringComparer.OrdinalIgnoreCase)
        .ToDictionary(
            group => group.Key,
            group => group.Select(item => item.Name).OrderBy(name => name, StringComparer.OrdinalIgnoreCase).ToArray(),
            StringComparer.OrdinalIgnoreCase);
    var branchTimelineCounts = BuildBranchTimelineHeaderCounts(orderedTimelineHeaders, selectedBranch, normalizedTimelineScope);
    var timelinePage = BuildTimelineHeaderPage(orderedTimelineHeaders, cursor, limit);
    var timelineCommits = new List<ContextCommit>(timelinePage.Items.Count);
    foreach (var header in timelinePage.Items)
    {
        var commit = await commitRepository.LoadAsync(repositoryPath, new ContextCommitId(header.Id), cancellationToken);
        if (commit is not null)
        {
            timelineCommits.Add(commit);
        }
    }

    return Results.Json(new
    {
        repositoryPath,
        selectedBranch,
        timelineCommits = timelineCommits.Select(commit => ToViewerTimelineCommit(commit, branchHeadsByCommit)),
        timelinePage = new
        {
            totalCount = orderedTimelineHeaders.Length,
            loadedCount = timelineCommits.Count,
            limit = timelinePage.Limit,
            cursor = timelinePage.Cursor,
            nextCursor = timelinePage.NextCursor,
            hasMore = timelinePage.HasMore
        },
        timelineScope = normalizedTimelineScope,
        branchTimelineCounts
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

app.MapGet("/api/diff", async (string? from, string? to, string? path, CancellationToken cancellationToken) =>
{
    try
    {
        var repositoryPath = ResolveRepositoryPath(path);
        if (!await workingRepository.ExistsAsync(repositoryPath, cancellationToken))
        {
            return Results.NotFound(new { message = $"No .ctx repository found at '{repositoryPath}'." });
        }

        var resolvedFromCommitId = await ResolveCommitReferenceAsync(repositoryPath, from, commitRepository, branchRepository, cancellationToken);
        var resolvedToCommitId = await ResolveCommitReferenceAsync(repositoryPath, to, commitRepository, branchRepository, cancellationToken);
        if (!string.IsNullOrWhiteSpace(resolvedFromCommitId) && !string.IsNullOrWhiteSpace(resolvedToCommitId))
        {
            var targetCommit = await commitRepository.LoadAsync(repositoryPath, new ContextCommitId(resolvedToCommitId), cancellationToken);
            if (targetCommit is not null
                && targetCommit.ParentIds.Any(parent => parent.Value.Equals(resolvedFromCommitId, StringComparison.OrdinalIgnoreCase)))
            {
                return Results.Json(new
                {
                    summary = targetCommit.Diff.Summary,
                    diff = targetCommit.Diff
                });
            }
        }

        var result = await runtime.ApplicationService.DiffAsync(repositoryPath, resolvedFromCommitId, resolvedToCommitId, cancellationToken);
        return result.Success
            ? Results.Json(result.Data)
            : Results.BadRequest(new { message = result.Message });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

app.MapGet("/api/entity-origin-commit", async (string type, string id, string? path, CancellationToken cancellationToken) =>
{
    try
    {
        var repositoryPath = ResolveRepositoryPath(path);
        if (!await workingRepository.ExistsAsync(repositoryPath, cancellationToken))
        {
            return Results.NotFound(new { message = $"No .ctx repository found at '{repositoryPath}'." });
        }

        var origin = await FindEntityOriginCommitAsync(repositoryPath, type, id, commitRepository, cancellationToken);
        return origin is null
            ? Results.NotFound(new { message = $"{type} '{id}' does not have a visible origin commit." })
            : Results.Json(new
            {
                entityType = NormalizeEntityOriginType(type),
                entityId = id,
                commitId = origin.Id.Value,
                branch = origin.Branch,
                message = origin.Message,
                createdAtUtc = origin.CreatedAtUtc
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
    var operationalReviewResult = await runtime.ApplicationService.OperationalReviewAsync(repositoryPath, effectivePurpose, 2, cancellationToken);

    return Results.Json(new
    {
        purpose = effectivePurpose,
        operationalReview = operationalReviewResult.Success ? operationalReviewResult.Data : null,
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

static IReadOnlyList<string> BuildSelectedRepositoryCandidateRoots(string? path)
{
    if (string.IsNullOrWhiteSpace(path))
    {
        return [];
    }

    try
    {
        var fullPath = Path.GetFullPath(path);
        return Directory.Exists(fullPath) ? [fullPath] : [];
    }
    catch
    {
        return [];
    }
}

static IReadOnlyList<string> BuildRepositoryCandidateRoots(string? path)
{
    var roots = new List<string>();
    AddRoot(path);
    AddRoot(ResolveDefaultRepositoryRoot());

    var currentDirectory = Directory.GetCurrentDirectory();
    AddRoot(currentDirectory);
    AddRoot(Directory.GetParent(currentDirectory)?.FullName);

    if (OperatingSystem.IsWindows())
    {
        AddRoot(@"C:\sources");
        AddRoot(@"D:\sources");
    }

    var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    AddRoot(Path.Combine(userProfile, "source", "repos"));
    AddRoot(Path.Combine(userProfile, "sources"));
    AddRoot(Path.Combine(userProfile, "repos"));

    return roots;

    void AddRoot(string? candidate)
    {
        if (string.IsNullOrWhiteSpace(candidate))
        {
            return;
        }

        try
        {
            var fullPath = Path.GetFullPath(candidate);
            if (File.Exists(fullPath))
            {
                fullPath = Path.GetDirectoryName(fullPath) ?? fullPath;
            }

            if (Directory.Exists(Path.Combine(fullPath, ".ctx")))
            {
                roots.Add(fullPath);
            }

            var parent = Directory.GetParent(fullPath)?.FullName;
            if (!string.IsNullOrWhiteSpace(parent))
            {
                roots.Add(parent);
            }

            if (Directory.Exists(fullPath))
            {
                roots.Add(fullPath);
            }
        }
        catch
        {
            // Candidate roots are best-effort; unavailable drives or malformed paths are ignored.
        }
    }
}

static IReadOnlyList<object> FindRepositoryCandidates(IReadOnlyList<string> roots)
{
    var startedAt = Stopwatch.StartNew();
    var normalizedRoots = roots
        .Where(Directory.Exists)
        .Select(path => Path.GetFullPath(path))
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .Take(12)
        .ToArray();
    var repositories = new Dictionary<string, (string Name, string Path)>(StringComparer.OrdinalIgnoreCase);

    foreach (var root in normalizedRoots)
    {
        foreach (var repositoryPath in EnumerateRepositoryCandidates(root))
        {
            if (repositories.Count >= 80 || startedAt.ElapsedMilliseconds > 850)
            {
                break;
            }

            var name = Path.GetFileName(repositoryPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            repositories.TryAdd(repositoryPath, (
                string.IsNullOrWhiteSpace(name) ? repositoryPath : name,
                repositoryPath));
        }

        if (repositories.Count >= 80 || startedAt.ElapsedMilliseconds > 850)
        {
            break;
        }
    }

    return repositories.Values
        .OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
        .Select(item => new { name = item.Name, path = item.Path })
        .ToArray();
}

static IEnumerable<string> EnumerateRepositoryCandidates(string root)
{
    var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    foreach (var candidate in EnumerateCandidatePaths(root))
    {
        string fullPath;
        try
        {
            fullPath = Path.GetFullPath(candidate);
        }
        catch
        {
            continue;
        }

        if (!visited.Add(fullPath) || IsIgnoredRepositoryBrowserPath(fullPath))
        {
            continue;
        }

        if (Directory.Exists(Path.Combine(fullPath, ".ctx")))
        {
            yield return fullPath;
        }
    }
}

static IEnumerable<string> EnumerateCandidatePaths(string root)
{
    yield return root;

    IEnumerable<string> children;
    try
    {
        children = Directory.EnumerateDirectories(root);
    }
    catch
    {
        yield break;
    }

    foreach (var child in children.Take(250))
    {
        yield return child;
    }
}

static bool IsIgnoredRepositoryBrowserPath(string path)
{
    var name = Path.GetFileName(path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
    return name is ".git" or "bin" or "obj" or "node_modules" or ".vs" or ".idea" or ".vscode";
}

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

static object GetLocalMcpStatus(string repositoryPath, bool ensureRunning, McpProcessSupervisor supervisor, int? stoppedProcessCount = null)
{
    var installRoot = ResolveInstallRootForLocalTools();
    var launcherPath = Path.Combine(installRoot, "bin", OperatingSystem.IsWindows() ? "ctx-mcp.cmd" : "ctx-mcp");
    var executablePath = Path.Combine(installRoot, "mcp", OperatingSystem.IsWindows() ? "Ctx.Mcp.exe" : "Ctx.Mcp");
    var launcherExists = File.Exists(launcherPath);
    var executableExists = File.Exists(executablePath);
    var supervisorStatus = ensureRunning && executableExists
        ? supervisor.EnsureRunning(executablePath, repositoryPath)
        : supervisor.GetStatus(repositoryPath);
    var runningProcessCount = CountMcpProcesses();
    var healthy = launcherExists && executableExists && (runningProcessCount > 0 || supervisorStatus.Running);

    return new
    {
        healthy,
        status = healthy ? "ok" : "unavailable",
        launcherExists,
        executableExists,
        runningProcessCount,
        repositoryProcessCount = supervisorStatus.Running ? 1 : 0,
        ownedProcessId = supervisorStatus.Running ? supervisorStatus.ProcessId : null,
        ownedRepositoryPath = supervisorStatus.RepositoryPath,
        startedByViewer = supervisorStatus.Running,
        startupError = supervisorStatus.Error,
        stoppedProcessCount,
        launcherPath,
        executablePath,
        repositoryPath,
        checkedAtUtc = DateTimeOffset.UtcNow
    };
}

static string ResolveInstallRootForLocalTools()
{
    var configuredRoot = Environment.GetEnvironmentVariable("CTX_INSTALL_ROOT");
    if (!string.IsNullOrWhiteSpace(configuredRoot))
    {
        return Path.GetFullPath(configuredRoot);
    }

    var inferredRoot = TryInferInstallRootFromViewerPath();
    if (!string.IsNullOrWhiteSpace(inferredRoot))
    {
        return inferredRoot;
    }

    return OperatingSystem.IsWindows()
        ? @"C:\ctx"
        : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share", "ctx");
}

static string? TryInferInstallRootFromViewerPath()
{
    try
    {
        var viewerDirectory = new DirectoryInfo(AppContext.BaseDirectory);
        var installRoot = viewerDirectory.Parent;
        if (installRoot is null)
        {
            return null;
        }

        var launcherPath = Path.Combine(installRoot.FullName, "bin", OperatingSystem.IsWindows() ? "ctx-mcp.cmd" : "ctx-mcp");
        var executablePath = Path.Combine(installRoot.FullName, "mcp", OperatingSystem.IsWindows() ? "Ctx.Mcp.exe" : "Ctx.Mcp");
        return File.Exists(launcherPath) && File.Exists(executablePath)
            ? installRoot.FullName
            : null;
    }
    catch
    {
        return null;
    }
}

static int CountMcpProcesses()
{
    try
    {
        return Process.GetProcesses()
            .Count(process =>
            {
                try
                {
                    return process.ProcessName.Contains("Ctx.Mcp", StringComparison.OrdinalIgnoreCase);
                }
                catch
                {
                    return false;
                }
            });
    }
    catch
    {
        return 0;
    }
}

static int StopLocalMcpProcesses()
{
    var stoppedCount = 0;
    try
    {
        foreach (var process in Process.GetProcesses())
        {
            try
            {
                if (!process.ProcessName.Contains("Ctx.Mcp", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                process.Kill(entireProcessTree: true);
                process.WaitForExit(3000);
                stoppedCount++;
            }
            catch
            {
                // Best-effort local control; status polling will report any process that remains.
            }
            finally
            {
                process.Dispose();
            }
        }
    }
    catch
    {
        return stoppedCount;
    }

    return stoppedCount;
}

static async Task<object> GetReleaseStatusAsync(HttpClient httpClient, CancellationToken cancellationToken)
{
    var owner = Environment.GetEnvironmentVariable("CTX_RELEASE_OWNER");
    if (string.IsNullOrWhiteSpace(owner))
    {
        owner = "diegoxtr";
    }

    var repository = Environment.GetEnvironmentVariable("CTX_RELEASE_REPOSITORY");
    if (string.IsNullOrWhiteSpace(repository))
    {
        repository = "ctx-open";
    }

    var currentVersion = DomainConstants.ProductVersion;
    var latestReleaseApiUrl = $"https://api.github.com/repos/{owner.Trim()}/{repository.Trim()}/releases/latest";

    try
    {
        using var response = await httpClient.GetAsync(latestReleaseApiUrl, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return new
            {
                status = "unavailable",
                currentVersion,
                currentTag = $"v{currentVersion}",
                latestVersion = (string?)null,
                latestTag = (string?)null,
                latestReleaseUrl = $"https://github.com/{owner}/{repository}/releases",
                updateAvailable = false,
                error = $"GitHub returned {(int)response.StatusCode}.",
                checkedAtUtc = DateTimeOffset.UtcNow
            };
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var root = document.RootElement;
        var latestTag = root.TryGetProperty("tag_name", out var tagElement)
            ? tagElement.GetString()
            : null;
        var latestReleaseUrl = root.TryGetProperty("html_url", out var urlElement)
            ? urlElement.GetString()
            : null;
        var latestVersion = NormalizeReleaseVersion(latestTag);
        var updateAvailable = CompareReleaseVersions(latestVersion, currentVersion) > 0;

        return new
        {
            status = updateAvailable ? "update-available" : "current",
            currentVersion,
            currentTag = $"v{currentVersion}",
            latestVersion,
            latestTag,
            latestReleaseUrl = latestReleaseUrl ?? $"https://github.com/{owner}/{repository}/releases",
            updateAvailable,
            error = (string?)null,
            checkedAtUtc = DateTimeOffset.UtcNow
        };
    }
    catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or JsonException)
    {
        return new
        {
            status = "unavailable",
            currentVersion,
            currentTag = $"v{currentVersion}",
            latestVersion = (string?)null,
            latestTag = (string?)null,
            latestReleaseUrl = $"https://github.com/{owner}/{repository}/releases",
            updateAvailable = false,
            error = exception.Message,
            checkedAtUtc = DateTimeOffset.UtcNow
        };
    }
}

static string? NormalizeReleaseVersion(string? value)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        return null;
    }

    var match = Regex.Match(value.Trim(), @"^v?(?<version>\d+(?:\.\d+){0,3})", RegexOptions.IgnoreCase);
    return match.Success ? match.Groups["version"].Value : null;
}

static int CompareReleaseVersions(string? left, string? right)
{
    if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right))
    {
        return 0;
    }

    var leftParts = left.Split('.').Select(ParseVersionPart).ToArray();
    var rightParts = right.Split('.').Select(ParseVersionPart).ToArray();
    var length = Math.Max(leftParts.Length, rightParts.Length);
    for (var index = 0; index < length; index++)
    {
        var leftPart = index < leftParts.Length ? leftParts[index] : 0;
        var rightPart = index < rightParts.Length ? rightParts[index] : 0;
        var comparison = leftPart.CompareTo(rightPart);
        if (comparison != 0)
        {
            return comparison;
        }
    }

    return 0;
}

static int ParseVersionPart(string value)
    => int.TryParse(value, out var parsed) ? parsed : 0;

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

static string ResolveViewerVersionLabel(string viewerRoot)
{
    var normalizedViewerRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(viewerRoot));
    var normalizedLocalInstallRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath("C:\\ctx\\viewer"));

    return normalizedViewerRoot.Equals(normalizedLocalInstallRoot, StringComparison.OrdinalIgnoreCase)
        ? "local-version"
        : $"v{DomainConstants.ProductVersion}";
}

static string NormalizeTimelineScope(string? scope)
    => string.Equals(scope, "all", StringComparison.OrdinalIgnoreCase)
        ? "all"
        : "selected";

static async Task<TimelineCommitHeader[]> BuildTimelineCommitHeadersAsync(
    string repositoryPath,
    string selectedBranch,
    string timelineScope,
    CancellationToken cancellationToken)
{
    var headers = await ReadTimelineCommitHeadersAsync(repositoryPath, cancellationToken);
    var scopedHeaders = string.Equals(timelineScope, "all", StringComparison.OrdinalIgnoreCase)
        ? headers
        : headers.Where(commit => commit.Branch.Equals(selectedBranch, StringComparison.OrdinalIgnoreCase));

    return scopedHeaders
        .OrderByDescending(commit => commit.CreatedAtUtc)
        .ThenByDescending(commit => commit.Id, StringComparer.OrdinalIgnoreCase)
        .ToArray();
}

static async Task<IReadOnlyList<TimelineCommitHeader>> ReadTimelineCommitHeadersAsync(
    string repositoryPath,
    CancellationToken cancellationToken)
{
    var commitsPath = Path.Combine(repositoryPath, ".ctx", "commits");
    if (!Directory.Exists(commitsPath))
    {
        return Array.Empty<TimelineCommitHeader>();
    }

    var headers = new List<TimelineCommitHeader>();
    foreach (var file in Directory.EnumerateFiles(commitsPath, "*.json"))
    {
        cancellationToken.ThrowIfCancellationRequested();
        var header = await TryReadTimelineCommitHeaderAsync(file, cancellationToken);
        if (header is not null)
        {
            headers.Add(header);
        }
    }

    return headers;
}

static async Task<TimelineCommitHeader?> TryReadTimelineCommitHeaderAsync(
    string file,
    CancellationToken cancellationToken)
{
    var id = Path.GetFileNameWithoutExtension(file);
    string? branch = null;
    DateTimeOffset? createdAtUtc = null;

    using (var reader = new StreamReader(file))
    {
        for (var index = 0; index < 80 && !reader.EndOfStream; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var line = await reader.ReadLineAsync(cancellationToken);
            if (line is null)
            {
                break;
            }

            if (branch is null && TryReadJsonStringLineProperty(line, "branch", out var branchValue))
            {
                branch = branchValue;
            }
            else if (createdAtUtc is null && TryReadJsonStringLineProperty(line, "createdAtUtc", out var createdValue)
                && DateTimeOffset.TryParse(createdValue, out var parsedCreatedAtUtc))
            {
                createdAtUtc = parsedCreatedAtUtc;
            }

            if (!string.IsNullOrWhiteSpace(branch) && createdAtUtc.HasValue)
            {
                return new TimelineCommitHeader(id, branch, createdAtUtc.Value);
            }
        }
    }

    await using var stream = File.OpenRead(file);
    using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
    var root = document.RootElement;
    if (string.IsNullOrWhiteSpace(branch)
        && root.TryGetProperty("branch", out var branchElement)
        && branchElement.ValueKind == JsonValueKind.String)
    {
        branch = branchElement.GetString();
    }

    if (!createdAtUtc.HasValue
        && root.TryGetProperty("createdAtUtc", out var createdElement)
        && createdElement.ValueKind == JsonValueKind.String
        && DateTimeOffset.TryParse(createdElement.GetString(), out var fallbackCreatedAtUtc))
    {
        createdAtUtc = fallbackCreatedAtUtc;
    }

    return string.IsNullOrWhiteSpace(branch) || !createdAtUtc.HasValue
        ? null
        : new TimelineCommitHeader(id, branch, createdAtUtc.Value);
}

static bool TryReadJsonStringLineProperty(string line, string propertyName, out string? value)
{
    value = null;
    var trimmed = line.Trim();
    var prefix = $"\"{propertyName}\"";
    if (!trimmed.StartsWith(prefix, StringComparison.Ordinal))
    {
        return false;
    }

    var separatorIndex = trimmed.IndexOf(':');
    if (separatorIndex < 0)
    {
        return false;
    }

    var rawValue = trimmed[(separatorIndex + 1)..].Trim().TrimEnd(',');
    try
    {
        value = JsonSerializer.Deserialize<string>(rawValue);
        return value is not null;
    }
    catch (JsonException)
    {
        value = null;
        return false;
    }
}

static IReadOnlyDictionary<string, int> BuildBranchTimelineHeaderCounts(
    IReadOnlyList<TimelineCommitHeader> orderedTimelineCommits,
    string selectedBranch,
    string timelineScope)
{
    if (!string.Equals(timelineScope, "all", StringComparison.OrdinalIgnoreCase))
    {
        return new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            [selectedBranch] = orderedTimelineCommits.Count
        };
    }

    return orderedTimelineCommits
        .GroupBy(commit => commit.Branch, StringComparer.OrdinalIgnoreCase)
        .ToDictionary(
            group => group.Key,
            group => group.Count(),
            StringComparer.OrdinalIgnoreCase);
}

static async Task<string?> TrySelectDirectoryOnWindowsAsync()
{
    const string script = """
Add-Type -AssemblyName System.Windows.Forms
$owner = New-Object System.Windows.Forms.Form
$owner.TopMost = $true
$owner.StartPosition = [System.Windows.Forms.FormStartPosition]::CenterScreen
$owner.Width = 1
$owner.Height = 1
$owner.ShowInTaskbar = $false
$owner.Opacity = 0
$owner.Show()
$owner.Activate()
$dialog = New-Object System.Windows.Forms.FolderBrowserDialog
$dialog.ShowNewFolderButton = $false
if ($dialog.ShowDialog($owner) -eq [System.Windows.Forms.DialogResult]::OK) {
    $dialog.SelectedPath
}
$owner.Close()
$owner.Dispose()
""";

    var encodedScript = Convert.ToBase64String(System.Text.Encoding.Unicode.GetBytes(script));
    var startInfo = new ProcessStartInfo
    {
        FileName = "powershell",
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
    };
    startInfo.ArgumentList.Add("-NoProfile");
    startInfo.ArgumentList.Add("-STA");
    startInfo.ArgumentList.Add("-EncodedCommand");
    startInfo.ArgumentList.Add(encodedScript);

    using var process = Process.Start(startInfo);
    if (process is null)
    {
        return null;
    }

    var outputTask = process.StandardOutput.ReadToEndAsync();
    var errorTask = process.StandardError.ReadToEndAsync();
    await process.WaitForExitAsync();

    var output = (await outputTask).Trim();
    var error = (await errorTask).Trim();
    if (process.ExitCode != 0 && string.IsNullOrWhiteSpace(output))
    {
        throw new InvalidOperationException(string.IsNullOrWhiteSpace(error)
            ? "Directory selection failed."
            : error);
    }

    return string.IsNullOrWhiteSpace(output) ? null : output;
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

static async Task<ContextCommit?> FindEntityOriginCommitAsync(
    string repositoryPath,
    string type,
    string id,
    ICommitRepository commitRepository,
    CancellationToken cancellationToken)
{
    var normalizedType = NormalizeEntityOriginType(type);
    if (string.IsNullOrWhiteSpace(id))
    {
        throw new InvalidOperationException("Entity id is required.");
    }

    var headers = await ReadTimelineCommitHeadersAsync(repositoryPath, cancellationToken);
    foreach (var header in headers.OrderBy(commit => commit.CreatedAtUtc).ThenBy(commit => commit.Id, StringComparer.OrdinalIgnoreCase))
    {
        var commitFile = Path.Combine(repositoryPath, ".ctx", "commits", $"{header.Id}.json");
        if (await CommitDiffContainsEntityAsync(commitFile, id, cancellationToken))
        {
            return await commitRepository.LoadAsync(repositoryPath, new ContextCommitId(header.Id), cancellationToken);
        }
    }

    return null;
}

static async Task<bool> CommitDiffContainsEntityAsync(
    string commitFile,
    string entityId,
    CancellationToken cancellationToken)
{
    if (!File.Exists(commitFile))
    {
        return false;
    }

    using var reader = new StreamReader(commitFile);
    while (!reader.EndOfStream)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var line = await reader.ReadLineAsync(cancellationToken);
        if (line is null)
        {
            break;
        }

        if (line.Contains("\"snapshot\"", StringComparison.OrdinalIgnoreCase))
        {
            break;
        }

        if (TryReadJsonStringLineProperty(line, "entityId", out var diffEntityId)
            && diffEntityId is not null
            && diffEntityId.Equals(entityId, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
    }

    return false;
}

static string NormalizeEntityOriginType(string type)
    => type.Trim().ToLowerInvariant() switch
    {
        "epic" or "epics" => "Epic",
        "task" or "tasks" => "Task",
        "hypothesis" or "hypotheses" => "Hypothesis",
        "decision" or "decisions" => "Decision",
        "evidence" => "Evidence",
        "conclusion" or "conclusions" => "Conclusion",
        "runbook" or "runbooks" => "Runbook",
        "trigger" or "triggers" => "Trigger",
        _ => throw new InvalidOperationException($"Unsupported origin entity type '{type}'.")
    };

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
    var epicsById = (snapshot.Epics ?? Array.Empty<Epic>()).ToDictionary(epic => epic.Id.Value, StringComparer.OrdinalIgnoreCase);

    var rootGoalIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    var subGoalIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    var taskIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    var epicIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
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

    foreach (var change in commit.Diff.Epics ?? Array.Empty<ContextDiffChange>())
    {
        if (!epicsById.TryGetValue(change.EntityId, out var epic))
        {
            continue;
        }

        epicIds.Add(epic.Id.Value);
        foreach (var goalId in epic.GoalIds)
        {
            AddGoalHierarchy(rootGoalIds, subGoalIds, goalsById, goalId);
        }

        foreach (var taskId in epic.PromotedTaskIds)
        {
            AddTaskAndGoal(taskIds, rootGoalIds, subGoalIds, goalsById, tasksById, taskId);
        }
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
    var epicTitles = epicIds
        .Select(id => epicsById.TryGetValue(id, out var epic) ? epic.Title : null)
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
        rootGoalIds.ToArray(),
        subGoalIds.ToArray(),
        taskIds.ToArray(),
        epicIds.ToArray(),
        hypothesisIds.ToArray(),
        decisionIds.ToArray(),
        conclusionIds.ToArray(),
        goalTitles,
        subGoalTitles,
        taskTitles,
        epicTitles,
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

static ViewerTimelineHeaderPage BuildTimelineHeaderPage(IReadOnlyList<TimelineCommitHeader> orderedTimelineCommits, string? cursor, int? limit)
{
    var normalizedLimit = NormalizeTimelinePageLimit(limit);
    var startIndex = ResolveTimelineHeaderPageStartIndex(orderedTimelineCommits, cursor);
    var items = orderedTimelineCommits
        .Skip(startIndex)
        .Take(normalizedLimit)
        .ToArray();
    var nextIndex = startIndex + items.Length;
    var hasMore = nextIndex < orderedTimelineCommits.Count;
    var nextCursor = hasMore && items.Length > 0
        ? EncodeTimelineHeaderCursor(items[^1])
        : null;

    return new ViewerTimelineHeaderPage(items, normalizedLimit, cursor, nextCursor, hasMore);
}

static int NormalizeTimelinePageLimit(int? limit)
{
    const int defaultLimit = 20;
    const int maxLimit = 120;

    if (!limit.HasValue || limit.Value <= 0)
    {
        return defaultLimit;
    }

    return Math.Min(limit.Value, maxLimit);
}

static int ResolveTimelineHeaderPageStartIndex(IReadOnlyList<TimelineCommitHeader> orderedTimelineCommits, string? cursor)
{
    if (!TryDecodeTimelineCursor(cursor, out var cursorTicks, out var cursorCommitId))
    {
        return 0;
    }

    for (var index = 0; index < orderedTimelineCommits.Count; index++)
    {
        var commit = orderedTimelineCommits[index];
        if (commit.CreatedAtUtc.UtcTicks == cursorTicks
            && string.Equals(commit.Id, cursorCommitId, StringComparison.OrdinalIgnoreCase))
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
            && StringComparer.OrdinalIgnoreCase.Compare(commit.Id, cursorCommitId) < 0)
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

static string EncodeTimelineHeaderCursor(TimelineCommitHeader commit)
    => $"{commit.CreatedAtUtc.UtcTicks}|{commit.Id}";

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
    IReadOnlyList<string> EpicIds,
    IReadOnlyList<string> HypothesisIds,
    IReadOnlyList<string> DecisionIds,
    IReadOnlyList<string> ConclusionIds,
    IReadOnlyList<string> GoalTitles,
    IReadOnlyList<string> SubGoalTitles,
    IReadOnlyList<string> TaskTitles,
    IReadOnlyList<string> EpicTitles,
    IReadOnlyList<string> HypothesisTitles,
    IReadOnlyList<string> DecisionTitles,
    IReadOnlyList<string> ConclusionSummaries);

internal record TimelineCommitHeader(
    string Id,
    string Branch,
    DateTimeOffset CreatedAtUtc);

internal record ViewerTimelineHeaderPage(
    IReadOnlyList<TimelineCommitHeader> Items,
    int Limit,
    string? Cursor,
    string? NextCursor,
    bool HasMore);

internal sealed class McpProcessSupervisor
{
    private readonly object _sync = new();
    private Process? _process;
    private string? _repositoryPath;
    private string? _lastError;

    public McpSupervisorStatus EnsureRunning(string executablePath, string repositoryPath)
    {
        lock (_sync)
        {
            var normalizedRepositoryPath = Path.GetFullPath(repositoryPath);
            if (_process is not null && !_process.HasExited)
            {
                if (string.Equals(_repositoryPath, normalizedRepositoryPath, StringComparison.OrdinalIgnoreCase))
                {
                    return new McpSupervisorStatus(true, _process.Id, _repositoryPath, _lastError);
                }

                StopOwnedProcess();
            }

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = executablePath,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                startInfo.ArgumentList.Add("--repo");
                startInfo.ArgumentList.Add(normalizedRepositoryPath);

                _process = Process.Start(startInfo);
                _repositoryPath = normalizedRepositoryPath;
                _lastError = _process is null ? "MCP process did not start." : null;
            }
            catch (Exception exception)
            {
                _process = null;
                _repositoryPath = normalizedRepositoryPath;
                _lastError = exception.Message;
            }

            return GetStatusCore(normalizedRepositoryPath);
        }
    }

    public McpSupervisorStatus GetStatus(string repositoryPath)
    {
        lock (_sync)
        {
            return GetStatusCore(Path.GetFullPath(repositoryPath));
        }
    }

    public McpSupervisorStatus Stop(string repositoryPath)
    {
        lock (_sync)
        {
            var normalizedRepositoryPath = Path.GetFullPath(repositoryPath);
            if (_process is not null
                && !_process.HasExited
                && string.Equals(_repositoryPath, normalizedRepositoryPath, StringComparison.OrdinalIgnoreCase))
            {
                StopOwnedProcess();
                _lastError = null;
            }

            return GetStatusCore(normalizedRepositoryPath);
        }
    }

    private McpSupervisorStatus GetStatusCore(string repositoryPath)
    {
        var running = _process is not null
            && !_process.HasExited
            && string.Equals(_repositoryPath, repositoryPath, StringComparison.OrdinalIgnoreCase);
        return new McpSupervisorStatus(running, running ? _process!.Id : null, _repositoryPath, _lastError);
    }

    private void StopOwnedProcess()
    {
        try
        {
            if (_process is not null && !_process.HasExited)
            {
                _process.Kill(entireProcessTree: true);
                _process.WaitForExit(3000);
            }
        }
        catch
        {
            // Best-effort cleanup; an external MCP process is never touched here.
        }
        finally
        {
            _process?.Dispose();
            _process = null;
            _repositoryPath = null;
        }
    }
}

internal sealed record McpSupervisorStatus(bool Running, int? ProcessId, string? RepositoryPath, string? Error);
