namespace Ctx.Tests;

using Ctx.Application;
using Ctx.Infrastructure;
using Ctx.Mcp.Mcp;
using System.Reflection;
using Xunit;

public sealed class McpToolTests
{
    [Fact]
    public void ToolCatalog_ContainsMustHaveMcpParitySurface()
    {
        var actual = DiscoverToolNames();
        var expected = new[]
        {
            "ctx_version",
            "ctx_doctor",
            "ctx_status",
            "ctx_audit",
            "ctx_next",
            "ctx_gaps",
            "ctx_roadmap",
            "ctx_plan",
            "ctx_context",
            "ctx_graph_summary",
            "ctx_graph_show",
            "ctx_graph_export",
            "ctx_graph_lineage",
            "ctx_thread_reconstruct",
            "ctx_preflight",
            "ctx_operational_review",
            "ctx_check",
            "ctx_closeout",
            "ctx_log",
            "ctx_diff",
            "ctx_artifact_list",
            "ctx_artifact_show",
            "ctx_goal_list",
            "ctx_goal_show",
            "ctx_epic_list",
            "ctx_epic_show",
            "ctx_task_list",
            "ctx_task_show",
            "ctx_hypothesis_list",
            "ctx_hypothesis_show",
            "ctx_hypothesis_rank",
            "ctx_evidence_list",
            "ctx_evidence_show",
            "ctx_decision_list",
            "ctx_decision_show",
            "ctx_conclusion_list",
            "ctx_conclusion_show",
            "ctx_runbook_list",
            "ctx_runbook_show",
            "ctx_trigger_list",
            "ctx_trigger_show",
            "ctx_bootstrap_map",
            "ctx_acp_session_smoke",
            "ctx_init",
            "ctx_goal_add",
            "ctx_goal_update",
            "ctx_epic_add",
            "ctx_epic_update",
            "ctx_epic_promote",
            "ctx_line_open",
            "ctx_task_add",
            "ctx_task_update",
            "ctx_hypothesis_add",
            "ctx_hypothesis_update",
            "ctx_hypothesis_relate",
            "ctx_hypothesis_merge",
            "ctx_hypothesis_supersede",
            "ctx_evidence_add",
            "ctx_evidence_share",
            "ctx_decision_add",
            "ctx_decision_update",
            "ctx_conclusion_add",
            "ctx_conclusion_update",
            "ctx_runbook_add",
            "ctx_runbook_update",
            "ctx_trigger_add",
            "ctx_bootstrap_apply",
            "ctx_commit"
        };

        Assert.Empty(expected.Except(actual, StringComparer.Ordinal));
        Assert.Equal(actual.Count, actual.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public async Task AcpSessionSmoke_ReturnsReadOnlySessionShapeWithoutMutating()
    {
        var repositoryPath = CreateTempRepositoryPath();
        var runtime = Bootstrapper.Create();

        try
        {
            await runtime.ApplicationService.InitAsync(
                repositoryPath,
                new InitRepositoryRequest("MCP ACP Bridge Test", "read-only ACP bridge", "main", "tester"),
                CancellationToken.None);
            await runtime.ApplicationService.AddTaskAsync(
                repositoryPath,
                new AddTaskRequest("Validate ACP bridge", "Exercise ACP-style session smoke from MCP", null, Array.Empty<string>(), "tester"),
                CancellationToken.None);
            var guard = new RepositoryGuard(new CtxMcpOptions(repositoryPath, "write", null));
            var before = Assert.IsType<StatusSummary>((await runtime.ApplicationService.StatusAsync(repositoryPath, CancellationToken.None)).Data);

            var result = await CtxAcpBridgeTools.AcpSessionSmokeAsync(
                runtime.AgentService,
                guard,
                CancellationToken.None,
                "What is next?");
            var after = Assert.IsType<StatusSummary>((await runtime.ApplicationService.StatusAsync(repositoryPath, CancellationToken.None)).Data);

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            var json = System.Text.Json.JsonSerializer.Serialize(result.Data);
            Assert.Contains("ctx_acp_session_smoke", json, StringComparison.Ordinal);
            Assert.Contains("sessionNew", json, StringComparison.Ordinal);
            Assert.Contains("sessionUpdate", json, StringComparison.Ordinal);
            Assert.Contains("end_turn", json, StringComparison.Ordinal);
            Assert.Contains("\"mutated\":false", json, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(before.Dirty, after.Dirty);
        }
        finally
        {
            DeleteTempRepository(repositoryPath);
        }
    }

    [Fact]
    public async Task WriteTool_ThrowsWhenServerRunsReadOnly()
    {
        var repositoryPath = CreateTempRepositoryPath();
        var runtime = Bootstrapper.Create();

        try
        {
            await runtime.ApplicationService.InitAsync(
                repositoryPath,
                new InitRepositoryRequest("MCP Test", "Read-only guard", "main", "tester"),
                CancellationToken.None);
            var guard = new RepositoryGuard(new CtxMcpOptions(repositoryPath, "read-only", null));

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                CtxWriteTools.AddTaskAsync(
                    runtime.ApplicationService,
                    guard,
                    CancellationToken.None,
                    "Blocked task"));

            Assert.Contains("read-only mode", exception.Message);
        }
        finally
        {
            DeleteTempRepository(repositoryPath);
        }
    }

    [Fact]
    public async Task NewWriteTools_ThrowWhenServerRunsReadOnly()
    {
        var repositoryPath = CreateTempRepositoryPath();
        var sourcePath = Path.Combine(repositoryPath, "source");
        var runtime = Bootstrapper.Create();

        try
        {
            Directory.CreateDirectory(sourcePath);
            await File.WriteAllTextAsync(Path.Combine(sourcePath, "README.md"), "source", CancellationToken.None);
            await runtime.ApplicationService.InitAsync(
                repositoryPath,
                new InitRepositoryRequest("MCP Read-only", "New write guard", "main", "tester"),
                CancellationToken.None);
            var guard = new RepositoryGuard(new CtxMcpOptions(repositoryPath, "read-only", null));

            var bootstrapMap = await CtxWriteTools.BootstrapMapAsync(
                runtime.ApplicationService,
                guard,
                CancellationToken.None,
                sourcePath,
                requestedBy: "tester");
            var runbookException = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                CtxWriteTools.AddRunbookAsync(
                    runtime.ApplicationService,
                    guard,
                    CancellationToken.None,
                    "Blocked runbook",
                    "Procedure",
                    "Never",
                    steps: new[] { "blocked" }));
            var bootstrapApplyException = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                CtxWriteTools.BootstrapApplyAsync(
                    runtime.ApplicationService,
                    guard,
                    CancellationToken.None,
                    sourcePath,
                    requestedBy: "tester"));

            Assert.True(bootstrapMap.Success);
            Assert.Contains("read-only mode", runbookException.Message);
            Assert.Contains("read-only mode", bootstrapApplyException.Message);
        }
        finally
        {
            DeleteTempRepository(repositoryPath);
        }
    }

    [Fact]
    public async Task WriteTools_CanAddTaskAndCommitWhenServerRunsWriteMode()
    {
        var repositoryPath = CreateTempRepositoryPath();
        var runtime = Bootstrapper.Create();

        try
        {
            await runtime.ApplicationService.InitAsync(
                repositoryPath,
                new InitRepositoryRequest("MCP Test", "Write mode", "main", "tester"),
                CancellationToken.None);
            var guard = new RepositoryGuard(new CtxMcpOptions(repositoryPath, "write", null));

            var addTask = await CtxWriteTools.AddTaskAsync(
                runtime.ApplicationService,
                guard,
                CancellationToken.None,
                "MCP writable task",
                createdBy: "tester");
            var dirtyStatus = await CtxReadTools.StatusAsync(
                runtime.ApplicationService,
                guard,
                CancellationToken.None);
            var commit = await CtxWriteTools.CommitAsync(
                runtime.ApplicationService,
                guard,
                CancellationToken.None,
                "mcp writable commit",
                createdBy: "tester");
            var cleanStatus = await CtxReadTools.StatusAsync(
                runtime.ApplicationService,
                guard,
                CancellationToken.None);

            Assert.True(addTask.Success);
            Assert.True(commit.Success);
            Assert.True(((StatusSummary)dirtyStatus.Data!).Dirty);
            Assert.False(((StatusSummary)cleanStatus.Data!).Dirty);
        }
        finally
        {
            DeleteTempRepository(repositoryPath);
        }
    }

    [Fact]
    public async Task InitTool_CanCreateRepositoryWhenServerRunsWriteMode()
    {
        var repositoryPath = CreateTempRepositoryPath();
        var runtime = Bootstrapper.Create();

        try
        {
            var guard = new RepositoryGuard(new CtxMcpOptions(repositoryPath, "write", null));

            var init = await CtxWriteTools.InitAsync(
                runtime.ApplicationService,
                guard,
                CancellationToken.None,
                "MCP Init",
                "Initialized through MCP",
                createdBy: "tester");
            var status = await CtxReadTools.StatusAsync(
                runtime.ApplicationService,
                guard,
                CancellationToken.None);

            Assert.True(init.Success);
            Assert.True(status.Success);
            Assert.True(Directory.Exists(Path.Combine(repositoryPath, ".ctx")));
        }
        finally
        {
            DeleteTempRepository(repositoryPath);
        }
    }

    [Fact]
    public async Task ReadTools_ExposeHistoryArtifactsLineageAndDiff()
    {
        var repositoryPath = CreateTempRepositoryPath();
        var runtime = Bootstrapper.Create();

        try
        {
            await runtime.ApplicationService.InitAsync(
                repositoryPath,
                new InitRepositoryRequest("MCP Read", "Read tool parity", "main", "tester"),
                CancellationToken.None);
            var guard = new RepositoryGuard(new CtxMcpOptions(repositoryPath, "write", null));

            var goal = await CtxWriteTools.AddGoalAsync(
                runtime.ApplicationService,
                guard,
                CancellationToken.None,
                "Read parity goal",
                createdBy: "tester");
            var goalId = ((Ctx.Domain.Goal)goal.Data!).Id.Value;
            var task = await CtxWriteTools.AddTaskAsync(
                runtime.ApplicationService,
                guard,
                CancellationToken.None,
                "Read parity task",
                goalId: goalId,
                createdBy: "tester");
            var taskId = ((Ctx.Domain.Task)task.Data!).Id.Value;
            var hypothesis = await CtxWriteTools.AddHypothesisAsync(
                runtime.ApplicationService,
                guard,
                CancellationToken.None,
                "MCP read tools expose enough inspection state",
                taskId: taskId,
                createdBy: "tester");
            var hypothesisId = ((Ctx.Domain.Hypothesis)hypothesis.Data!).Id.Value;
            var evidence = await CtxWriteTools.AddEvidenceAsync(
                runtime.ApplicationService,
                guard,
                CancellationToken.None,
                "Read parity evidence",
                "Evidence for read parity",
                "test",
                "Observation",
                supports: new[] { $"hypothesis:{hypothesisId}" },
                createdBy: "tester");
            var evidenceId = ((Ctx.Domain.Evidence)evidence.Data!).Id.Value;
            var decision = await CtxWriteTools.AddDecisionAsync(
                runtime.ApplicationService,
                guard,
                CancellationToken.None,
                "Read parity decision",
                "Exercise decision read tools",
                "Accepted",
                hypothesisIds: new[] { hypothesisId },
                evidenceIds: new[] { evidenceId },
                createdBy: "tester");
            var decisionId = ((Ctx.Domain.Decision)decision.Data!).Id.Value;
            var conclusion = await CtxWriteTools.AddConclusionAsync(
                runtime.ApplicationService,
                guard,
                CancellationToken.None,
                "Read parity conclusion",
                "Accepted",
                decisionIds: new[] { decisionId },
                evidenceIds: new[] { evidenceId },
                goalIds: new[] { goalId },
                taskIds: new[] { taskId },
                createdBy: "tester");
            var conclusionId = ((Ctx.Domain.Conclusion)conclusion.Data!).Id.Value;

            var audit = await CtxReadTools.AuditAsync(runtime.ApplicationService, guard, CancellationToken.None);
            var gaps = await CtxReadTools.GapsAsync(runtime.ApplicationService, guard, CancellationToken.None);
            var roadmap = await CtxReadTools.RoadmapAsync(runtime.ApplicationService, guard, CancellationToken.None);
            var plan = await CtxReadTools.PlanAsync(runtime.ApplicationService, guard, CancellationToken.None, taskId: taskId);
            var context = await CtxReadTools.ContextAsync(runtime.ApplicationService, guard, CancellationToken.None, taskId: taskId);
            var graphShow = await CtxReadTools.GraphShowAsync(runtime.ApplicationService, guard, CancellationToken.None, $"Task:{taskId}");
            var check = await CtxReadTools.CheckAsync(runtime.ApplicationService, guard, CancellationToken.None, taskId);
            var closeout = await CtxReadTools.CloseoutAsync(runtime.ApplicationService, guard, CancellationToken.None);
            var commit = await CtxWriteTools.CommitAsync(
                runtime.ApplicationService,
                guard,
                CancellationToken.None,
                "read parity seed",
                createdBy: "tester");
            var log = await CtxReadTools.LogAsync(runtime.ApplicationService, guard, CancellationToken.None);
            var diff = await CtxReadTools.DiffAsync(runtime.ApplicationService, guard, CancellationToken.None);
            var goalList = await CtxReadTools.GoalListAsync(runtime.ApplicationService, guard, CancellationToken.None);
            var goalShow = await CtxReadTools.GoalShowAsync(runtime.ApplicationService, guard, CancellationToken.None, goalId);
            var taskList = await CtxReadTools.TaskListAsync(runtime.ApplicationService, guard, CancellationToken.None);
            var taskShow = await CtxReadTools.TaskShowAsync(runtime.ApplicationService, guard, CancellationToken.None, taskId);
            var hypothesisList = await CtxReadTools.HypothesisListAsync(runtime.ApplicationService, guard, CancellationToken.None);
            var hypothesisShow = await CtxReadTools.HypothesisShowAsync(runtime.ApplicationService, guard, CancellationToken.None, hypothesisId);
            var hypothesisRank = await CtxReadTools.HypothesisRankAsync(runtime.ApplicationService, guard, CancellationToken.None);
            var evidenceList = await CtxReadTools.EvidenceListAsync(runtime.ApplicationService, guard, CancellationToken.None);
            var evidenceShow = await CtxReadTools.EvidenceShowAsync(runtime.ApplicationService, guard, CancellationToken.None, evidenceId);
            var decisionList = await CtxReadTools.DecisionListAsync(runtime.ApplicationService, guard, CancellationToken.None);
            var decisionShow = await CtxReadTools.DecisionShowAsync(runtime.ApplicationService, guard, CancellationToken.None, decisionId);
            var conclusionList = await CtxReadTools.ConclusionListAsync(runtime.ApplicationService, guard, CancellationToken.None);
            var conclusionShow = await CtxReadTools.ConclusionShowAsync(runtime.ApplicationService, guard, CancellationToken.None, conclusionId);
            var genericArtifactList = await CtxReadTools.ArtifactListAsync(runtime.ApplicationService, guard, CancellationToken.None, "task");
            var genericArtifactShow = await CtxReadTools.ArtifactShowAsync(runtime.ApplicationService, guard, CancellationToken.None, "task", taskId);
            var graphExport = await CtxReadTools.GraphExportAsync(runtime.ApplicationService, guard, CancellationToken.None);
            var lineage = await CtxReadTools.GraphLineageAsync(runtime.ApplicationService, guard, CancellationToken.None, "task", taskId);
            var version = CtxReadTools.Version();
            var doctor = await CtxReadTools.DoctorAsync(runtime.ApplicationService, guard, CancellationToken.None);

            Assert.True(audit.Success);
            Assert.True(gaps.Success);
            Assert.True(roadmap.Success);
            Assert.True(plan.Success);
            Assert.True(context.Success);
            Assert.True(graphShow.Success);
            Assert.True(check.Success);
            Assert.True(closeout.Success);
            Assert.True(commit.Success);
            Assert.True(log.Success);
            Assert.True(diff.Success);
            Assert.True(goalList.Success);
            Assert.True(goalShow.Success);
            Assert.True(taskList.Success);
            Assert.True(taskShow.Success);
            Assert.True(hypothesisList.Success);
            Assert.True(hypothesisShow.Success);
            Assert.True(hypothesisRank.Success);
            Assert.True(evidenceList.Success);
            Assert.True(evidenceShow.Success);
            Assert.True(decisionList.Success);
            Assert.True(decisionShow.Success);
            Assert.True(conclusionList.Success);
            Assert.True(conclusionShow.Success);
            Assert.True(genericArtifactList.Success);
            Assert.True(genericArtifactShow.Success);
            Assert.True(graphExport.Success);
            Assert.True(lineage.Success);
            Assert.True(version.Success);
            Assert.True(doctor.Success);
            Assert.Equal(goalId, ((Ctx.Domain.Goal)goalShow.Data!).Id.Value);
            Assert.Equal(taskId, ((Ctx.Domain.Task)taskShow.Data!).Id.Value);
            Assert.Equal(taskId, ((Ctx.Domain.Task)genericArtifactShow.Data!).Id.Value);
            Assert.Equal(taskId, ((Ctx.Application.PlanningSummary)plan.Data!).Next.Recommended?.EntityId);
            Assert.Equal(hypothesisId, ((Ctx.Domain.Hypothesis)hypothesisShow.Data!).Id.Value);
            Assert.Equal(evidenceId, ((Ctx.Domain.Evidence)evidenceShow.Data!).Id.Value);
            Assert.Equal(decisionId, ((Ctx.Domain.Decision)decisionShow.Data!).Id.Value);
            Assert.Equal(conclusionId, ((Ctx.Domain.Conclusion)conclusionShow.Data!).Id.Value);
            Assert.False(string.IsNullOrWhiteSpace(hypothesisId));
        }
        finally
        {
            DeleteTempRepository(repositoryPath);
        }
    }

    [Fact]
    public async Task WriteTools_ExposeRunbookTriggerHypothesisRelationsAndBootstrap()
    {
        var repositoryPath = CreateTempRepositoryPath();
        var sourcePath = Path.Combine(repositoryPath, "source");
        var runtime = Bootstrapper.Create();

        try
        {
            Directory.CreateDirectory(sourcePath);
            await File.WriteAllTextAsync(
                Path.Combine(sourcePath, "README.md"),
                "A neighborhood store uses margin, credit, rain and delivery rules to decide daily operations.",
                CancellationToken.None);
            await runtime.ApplicationService.InitAsync(
                repositoryPath,
                new InitRepositoryRequest("MCP Write", "Write tool parity", "main", "tester"),
                CancellationToken.None);
            var guard = new RepositoryGuard(new CtxMcpOptions(repositoryPath, "write", null));

            var runbook = await CtxWriteTools.AddRunbookAsync(
                runtime.ApplicationService,
                guard,
                CancellationToken.None,
                "MCP parity runbook",
                "Procedure",
                "Use during MCP parity tests",
                triggers: new[] { "mcp-parity" },
                steps: new[] { "Call the MCP tool" },
                verify: new[] { "Tool result succeeds" },
                createdBy: "tester");
            var runbookId = ((Ctx.Domain.OperationalRunbook)runbook.Data!).Id.Value;
            var trigger = await CtxWriteTools.AddTriggerAsync(
                runtime.ApplicationService,
                guard,
                CancellationToken.None,
                "UserPrompt",
                "MCP parity test trigger",
                runbookIds: new[] { runbookId },
                createdBy: "tester");
            var runbookList = await CtxReadTools.RunbookListAsync(runtime.ApplicationService, guard, CancellationToken.None);
            var runbookShow = await CtxReadTools.RunbookShowAsync(runtime.ApplicationService, guard, CancellationToken.None, runbookId);
            var triggerList = await CtxReadTools.TriggerListAsync(runtime.ApplicationService, guard, CancellationToken.None);
            var triggerShow = await CtxReadTools.TriggerShowAsync(
                runtime.ApplicationService,
                guard,
                CancellationToken.None,
                ((Ctx.Domain.CognitiveTrigger)trigger.Data!).Id.Value);

            var task = await CtxWriteTools.AddTaskAsync(runtime.ApplicationService, guard, CancellationToken.None, "Hypothesis relation task", createdBy: "tester");
            var taskId = ((Ctx.Domain.Task)task.Data!).Id.Value;
            var firstHypothesis = await CtxWriteTools.AddHypothesisAsync(
                runtime.ApplicationService,
                guard,
                CancellationToken.None,
                "First MCP relation hypothesis",
                taskId: taskId,
                createdBy: "tester");
            var secondHypothesis = await CtxWriteTools.AddHypothesisAsync(
                runtime.ApplicationService,
                guard,
                CancellationToken.None,
                "Second MCP relation hypothesis",
                taskId: taskId,
                createdBy: "tester");
            var firstHypothesisId = ((Ctx.Domain.Hypothesis)firstHypothesis.Data!).Id.Value;
            var secondHypothesisId = ((Ctx.Domain.Hypothesis)secondHypothesis.Data!).Id.Value;
            var relation = await CtxWriteTools.RelateHypothesisAsync(
                runtime.ApplicationService,
                guard,
                CancellationToken.None,
                firstHypothesisId,
                "derived-from",
                secondHypothesisId,
                updatedBy: "tester");
            var bootstrapMap = await CtxWriteTools.BootstrapMapAsync(
                runtime.ApplicationService,
                guard,
                CancellationToken.None,
                sourcePath,
                mode: "project",
                requestedBy: "tester");
            var bootstrapApply = await CtxWriteTools.BootstrapApplyAsync(
                runtime.ApplicationService,
                guard,
                CancellationToken.None,
                sourcePath,
                mode: "project",
                requestedBy: "tester");

            Assert.True(runbook.Success);
            Assert.True(trigger.Success);
            Assert.True(runbookList.Success);
            Assert.True(runbookShow.Success);
            Assert.True(triggerList.Success);
            Assert.True(triggerShow.Success);
            Assert.True(relation.Success);
            Assert.True(bootstrapMap.Success);
            Assert.True(bootstrapApply.Success);
        }
        finally
        {
            DeleteTempRepository(repositoryPath);
        }
    }

    [Fact]
    public async Task RepositoryGuard_BlocksDynamicRepositoryWithoutAllowRoot()
    {
        var configuredPath = CreateTempRepositoryPath();
        var otherPath = CreateTempRepositoryPath();
        var runtime = Bootstrapper.Create();

        try
        {
            await runtime.ApplicationService.InitAsync(
                configuredPath,
                new InitRepositoryRequest("Configured", "Configured repo", "main", "tester"),
                CancellationToken.None);
            await runtime.ApplicationService.InitAsync(
                otherPath,
                new InitRepositoryRequest("Other", "Other repo", "main", "tester"),
                CancellationToken.None);
            var guard = new RepositoryGuard(new CtxMcpOptions(configuredPath, "write", null));

            var exception = Assert.Throws<InvalidOperationException>(() => guard.Resolve(otherPath));

            Assert.Contains("Dynamic repo paths are disabled", exception.Message);
        }
        finally
        {
            DeleteTempRepository(configuredPath);
            DeleteTempRepository(otherPath);
        }
    }

    private static string CreateTempRepositoryPath()
    {
        var repositoryPath = Path.Combine(Path.GetTempPath(), "ctx-mcp-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repositoryPath);
        return repositoryPath;
    }

    private static void DeleteTempRepository(string repositoryPath)
    {
        if (Directory.Exists(repositoryPath))
        {
            Directory.Delete(repositoryPath, recursive: true);
        }
    }

    private static IReadOnlySet<string> DiscoverToolNames()
    {
        return new[] { typeof(CtxReadTools), typeof(CtxAcpBridgeTools), typeof(CtxWriteTools) }
            .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Static))
            .SelectMany(method => method.GetCustomAttributes(inherit: false))
            .Where(attribute => attribute.GetType().Name == "McpServerToolAttribute")
            .Select(attribute => attribute.GetType().GetProperty("Name")?.GetValue(attribute) as string)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .ToHashSet(StringComparer.Ordinal)!;
    }
}
