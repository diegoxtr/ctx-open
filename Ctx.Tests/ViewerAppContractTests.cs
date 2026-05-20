namespace Ctx.Tests;

public sealed class ViewerAppContractTests
{
    [Fact]
    public async Task CompareGraph_IsExplicitSurfaceAndOwnsZoom()
    {
        var script = await ReadViewerAppScriptAsync();

        Assert.Contains("graphSurfaceBackButton", script);
        Assert.Contains("Compare Graph", script);
        Assert.Contains("Back to Trace Graph", await ReadViewerIndexAsync());
        Assert.Contains("traceGraphDiffActive && currentComparisonDiffGraph", script);
        Assert.Contains("renderComparisonDiffGraphInTrace(", script);
        Assert.Contains("restoreTraceGraphAfterDiff({ force: true })", script);
    }

    [Fact]
    public async Task CompareGraph_UsesMappedDiffEndpointWithParentFastPath()
    {
        var script = await ReadViewerAppScriptAsync();
        var program = await ReadViewerProgramAsync();

        Assert.Contains("""apiGet("/api/diff", params)""", script);
        Assert.Contains("app.MapGet(\"/api/diff\"", program);
        Assert.Contains("workingRepository.ExistsAsync(repositoryPath", program);
        Assert.Contains("ResolveCommitReferenceAsync(repositoryPath, from", program);
        Assert.Contains("ResolveCommitReferenceAsync(repositoryPath, to", program);
        Assert.Contains("targetCommit.ParentIds.Any", program);
        Assert.Contains("targetCommit.Diff.Summary", program);
        Assert.Contains("DiffAsync(repositoryPath", program);
    }

    [Fact]
    public async Task CommitDetail_ExposesParentComparison()
    {
        var script = await ReadViewerAppScriptAsync();

        Assert.Contains("renderCommitDetailDiffActions", script);
        Assert.Contains("Compare with parent", script);
        Assert.Contains("data-commit-detail-diff-action=\"parent\"", script);
        Assert.Contains("handleCommitDetailDiffAction", script);
        Assert.Contains("loadCommitComparison(parentId, detail.id", script);
    }

    [Fact]
    public async Task CompareGraph_IncludesSnapshotContextOverlay()
    {
        var script = await ReadViewerAppScriptAsync();
        var program = await ReadViewerProgramAsync();

        Assert.Contains("BuildDiffSnapshotOverlay", program);
        Assert.Contains("ContextDiffChange(\"Context\"", program);
        Assert.Contains("overlay = BuildDiffSnapshotOverlay", program);
        Assert.Contains("comparison?.overlay?.context", script);
        Assert.Contains("[\"Snapshot Context\", \"Context\", overlayContext]", script);
        Assert.Contains("changed and snapshot-context cognitive entities", script);
    }

    [Fact]
    public async Task WorkingContextSignal_FingerprintsFullCognitiveState()
    {
        var program = await ReadViewerProgramAsync();

        Assert.Contains("app.MapGet(\"/api/working-context/signal\"", program);
        Assert.Contains("context.Dirty.ToString()", program);
        Assert.Contains("context.HeadCommitId?.Value", program);
        Assert.Contains("context.Project.Id.Value", program);
        Assert.Contains("context.Project.State.ToString()", program);
        Assert.Contains("context.Epics", program);
        Assert.Contains("context.Hypotheses", program);
        Assert.Contains("context.Decisions", program);
        Assert.Contains("context.Evidence", program);
        Assert.Contains("context.Conclusions", program);
        Assert.Contains("ComputeStableHash(fingerprintSource)", program);
    }

    [Fact]
    public async Task CompareGraph_CollapsesLargeDiffGroups()
    {
        var script = await ReadViewerAppScriptAsync();

        Assert.Contains("comparisonDiffGraphMaxItemsPerGroup = 60", script);
        Assert.Contains("Overflow:${type}", script);
        Assert.Contains("hiddenItems", script);
        Assert.Contains("stateCounts", script);
        Assert.Contains("collapsed into summary nodes", script);
    }

    [Fact]
    public async Task CompareGraph_NodesAreInspectable()
    {
        var script = await ReadViewerAppScriptAsync();
        var styles = await ReadViewerStylesAsync();

        Assert.Contains(".diff-graph-node[data-diff-node-id]", script);
        Assert.Contains("showComparisonDiffNode", script);
        Assert.Contains("renderComparisonDiffNodeDetail", script);
        Assert.Contains("data-diff-node-id", script);
        Assert.Contains("applyDetailTab(\"details\")", script);
        Assert.Contains("expandPanel(\"right\")", script);
        Assert.Contains("incoming", script);
        Assert.Contains("outgoing", script);
        Assert.Contains("connectedNodes", script);
        Assert.Contains(".diff-graph-node.active", styles);
        Assert.Contains("cursor: pointer", styles);
    }

    [Fact]
    public async Task CognitiveDiff_HasConciseSummaryStrip()
    {
        var script = await ReadViewerAppScriptAsync();
        var styles = await ReadViewerStylesAsync();

        Assert.Contains("buildComparisonSummaryMetrics", script);
        Assert.Contains("renderComparisonSummaryStrip", script);
        Assert.Contains("diff-summary-strip", script);
        Assert.Contains("Cognitive diff summary", script);
        Assert.Contains("label === \"Conflicts\"", script);
        Assert.Contains("type === \"Conflict\"", script);
        Assert.Contains(".diff-summary-states", styles);
        Assert.Contains("repeat(auto-fit, minmax(72px, 1fr))", styles);
        Assert.Contains(".diff-summary-card.diff-state-conflict", styles);
    }

    [Fact]
    public async Task GraphCanvas_OwnsScrollingAndIsResizable()
    {
        var script = await ReadViewerAppScriptAsync();
        var styles = await ReadViewerStylesAsync();
        var graphCanvasStyles = ExtractCssRule(styles, ".graph-canvas");
        var diffCanvasStyles = ExtractCssRule(styles, ".graph-canvas-diff .diff-graph-canvas");
        var graphFooterStyles = ExtractCssRule(styles, ".graph-footer");
        var epicRailStyles = ExtractCssRule(styles, ".epic-rail");

        Assert.Contains("overflow: auto", graphCanvasStyles);
        Assert.Contains("resize: both", graphCanvasStyles);
        Assert.Contains("scrollbar-gutter: stable both-edges", graphCanvasStyles);
        Assert.Contains("--graph-window-width: 100%", styles);
        Assert.Contains("width: var(--graph-window-width, 100%)", graphFooterStyles);
        Assert.Contains("width: var(--graph-window-width, 100%)", epicRailStyles);
        Assert.Contains("initializeGraphWindowSizing", script);
        Assert.Contains("ResizeObserver", script);
        Assert.Contains("overflow-y: auto", styles);
        Assert.Contains("overscroll-behavior: contain", styles);
        Assert.Contains("padding-bottom: calc(5.25rem + var(--viewer-footer-height))", styles);
        Assert.Contains("overflow: visible", diffCanvasStyles);
        Assert.Contains("position: static", graphFooterStyles);
        Assert.DoesNotContain("bottom:", graphFooterStyles);
        Assert.DoesNotContain("updateGraphFooterAnchor", script);
        Assert.DoesNotContain("--graph-footer-left", styles);
    }

    [Fact]
    public async Task ViewerChromeAndPlanningLayer_AreUserAdjustable()
    {
        var index = await ReadViewerIndexAsync();
        var script = await ReadViewerAppScriptAsync();
        var styles = await ReadViewerStylesAsync();
        var chrome = await ReadViewerChromeModuleAsync();

        Assert.Contains("topbar-collapse-toggle", index);
        Assert.Contains("topbar-inner", index);
        Assert.Contains("workspace-tab-strip-inner", index);
        Assert.Contains("view-toolbar-inner", index);
        Assert.Contains("workspace-tabs-collapse-toggle", index);
        Assert.DoesNotContain("workspace-tabs-density-toggle", index);
        Assert.Contains("topbarCollapsedStorageKey", script);
        Assert.DoesNotContain("workspaceTabsCompactStorageKey", script);
        Assert.DoesNotContain("data.workspaceTabsCompact", script);
        Assert.Contains("planningLayerCollapsedStorageKey", script);
        Assert.Contains("compareGraphAutoFitStorageKey", script);
        Assert.Contains("compareGraphExpandedStorageKey", script);
        Assert.Contains("graphIntroCollapsedStorageKey", script);
        Assert.Contains("graph-intro-toggle", index);
        Assert.Contains("diff-graph-top", script);
        Assert.Contains("data-compare-graph-action=\"auto-fit\"", script);
        Assert.Contains("data-toggle-planning-layer", script);
        Assert.Contains("resize: vertical", ExtractCssRule(styles, ".workspace-tab-strip"));
        Assert.Contains("min-width: 170px", ExtractCssRule(styles, ".workspace-tab {"));
        Assert.Contains("max-width: 230px", ExtractCssRule(styles, ".workspace-tab {"));
        Assert.Contains("font-size: 0.78rem", ExtractCssRule(styles, ".workspace-tab-label {"));
        Assert.Contains("font-size: 0.66rem", ExtractCssRule(styles, ".workspace-tab-path {"));
        Assert.DoesNotContain("data-workspace-tabs-compact", styles);
        Assert.Contains("body[data-topbar-collapsed=\"true\"] .topbar", styles);
        Assert.Contains("body[data-graph-intro-collapsed=\"true\"] #graph-caption", styles);
        Assert.Contains(".graph-canvas-diff.graph-canvas-expanded", styles);
        Assert.Contains(".epic-rail.is-collapsed .epic-rail-list", styles);
        Assert.Contains("overflow-x: auto", ExtractCssRule(styles, ".epic-rail-list"));
        Assert.Contains("flex: 0 0 min(320px, 78vw)", ExtractCssRule(styles, ".epic-rail-card"));
        Assert.Contains("--viewer-viewport-top", styles);
        Assert.Contains("top: var(--viewer-viewport-top)", ExtractCssRule(styles, ".layout"));
        Assert.Contains("position: fixed", ExtractCssRule(styles, ".topbar"));
        Assert.Contains("position: fixed", ExtractCssRule(styles, ".workspace-tab-strip"));
        Assert.Contains("padding: 0", ExtractCssRule(styles, ".topbar"));
        Assert.Contains("padding: 0", ExtractCssRule(styles, ".workspace-tab-strip"));
        Assert.Contains("padding: 0", ExtractCssRule(styles, ".view-toolbar"));
        Assert.Contains("padding: 0.58rem var(--viewer-chrome-inline-pad)", ExtractCssRule(styles, ".topbar-inner"));
        Assert.Contains("padding: 0.45rem var(--viewer-chrome-inline-pad) 0.35rem", ExtractCssRule(styles, ".workspace-tab-strip-inner"));
        Assert.Contains("padding: 0.75rem var(--viewer-chrome-inline-pad) 0.65rem", ExtractCssRule(styles, ".view-toolbar-inner"));
        Assert.Contains("overflow: hidden", ExtractCssRule(styles, ".workspace-tab-strip"));
        Assert.Contains("overflow: auto", ExtractCssRule(styles, ".workspace-tab-strip-inner"));
        Assert.Contains("overflow: visible", ExtractCssRule(styles, ".panel-divider {"));
        Assert.Contains("z-index: 35", ExtractCssRule(styles, ".panel-divider {"));
        Assert.Contains("z-index: 60", ExtractCssRule(styles, ".panel-collapse-toggle"));
        Assert.Contains("updateViewerChromeLayout", script);
        Assert.Contains("initializeViewerChromeLayout", script);
        Assert.Contains("document.body?.style.setProperty(name, value)", chrome);
        Assert.Contains("document.body?.style.removeProperty(name)", chrome);
    }

    [Fact]
    public async Task ParkedEpicRail_OnlyShowsParkedEpics()
    {
        var script = await ReadViewerAppScriptAsync();

        Assert.Contains("Parked Epics", script);
        Assert.Contains("normalizeGraphNodeState(node.state).toLowerCase() === \"parked\"", script);
        Assert.Contains("No parked epics.", script);
    }

    [Fact]
    public async Task GraphZoom_DoesNotForceFullGraphRerender()
    {
        var script = await ReadViewerAppScriptAsync();
        var renderKeyStart = script.IndexOf("function buildGraphRenderKey", StringComparison.Ordinal);
        var renderKeyEnd = script.IndexOf("function renderEpicRail", renderKeyStart, StringComparison.Ordinal);
        var renderKeyBody = script[renderKeyStart..renderKeyEnd];

        Assert.Contains("applyGraphZoomToRenderedSurface()", script);
        Assert.Contains("stage.style.transform = `scale(${currentGraphZoom})`", script);
        Assert.Contains("diffStage.style.transform = `scale(${currentGraphZoom})`", script);
        Assert.DoesNotContain("currentGraphZoom.toFixed", renderKeyBody);
    }

    [Fact]
    public async Task Viewer_UsesNativeModuleEntrypointAndUtilityModule()
    {
        var script = await ReadViewerAppScriptAsync();
        var index = await ReadViewerIndexAsync();
        var utilities = await ReadViewerUtilityModuleAsync();
        var api = await ReadViewerApiModuleAsync();
        var storage = await ReadViewerStorageModuleAsync();
        var workspaces = await ReadViewerWorkspacesModuleAsync();
        var history = await ReadViewerHistoryModuleAsync();

        Assert.Contains("""<script type="module" src="/app.mjs"></script>""", index);
        Assert.Contains("from \"./js/viewer-utils.mjs\"", script);
        Assert.Contains("from \"./js/viewer-api.mjs\"", script);
        Assert.Contains("from \"./js/viewer-storage.mjs\"", script);
        Assert.Contains("from \"./js/viewer-workspaces.mjs\"", script);
        Assert.Contains("from \"./js/viewer-history.mjs\"", script);
        Assert.DoesNotContain("fetch(", script);
        Assert.DoesNotContain("window.localStorage", script);
        Assert.Contains("export function escapeHtml", utilities);
        Assert.Contains("export function formatDateTime", utilities);
        Assert.Contains("export function normalizeRepositoryPath", utilities);
        Assert.Contains("export function apiGet", api);
        Assert.Contains("export function apiPost", api);
        Assert.Contains("export function buildApiUrl", api);
        Assert.Contains("export function readStorageValue", storage);
        Assert.Contains("export function writeStorageJson", storage);
        Assert.Contains("export function readStorageBoolean", storage);
        Assert.Contains("export function createWorkspaceTabState", workspaces);
        Assert.Contains("export function deriveWorkspaceTabTitle", workspaces);
        Assert.Contains("export function createWorkspaceTabShellPayload", workspaces);
        Assert.Contains("export function normalizeOverviewHistoryState", history);
        Assert.Contains("export function applyHydratedHistory", history);
        Assert.Contains("export function appendHistoryPage", history);
    }

    private static Task<string> ReadViewerAppScriptAsync()
        => File.ReadAllTextAsync(FindRepositoryFile("Ctx.Viewer", "wwwroot", "app.mjs"));

    private static Task<string> ReadViewerProgramAsync()
        => File.ReadAllTextAsync(FindRepositoryFile("Ctx.Viewer", "Program.cs"));

    private static Task<string> ReadViewerUtilityModuleAsync()
        => File.ReadAllTextAsync(FindRepositoryFile("Ctx.Viewer", "wwwroot", "js", "viewer-utils.mjs"));

    private static Task<string> ReadViewerApiModuleAsync()
        => File.ReadAllTextAsync(FindRepositoryFile("Ctx.Viewer", "wwwroot", "js", "viewer-api.mjs"));

    private static Task<string> ReadViewerStorageModuleAsync()
        => File.ReadAllTextAsync(FindRepositoryFile("Ctx.Viewer", "wwwroot", "js", "viewer-storage.mjs"));

    private static Task<string> ReadViewerWorkspacesModuleAsync()
        => File.ReadAllTextAsync(FindRepositoryFile("Ctx.Viewer", "wwwroot", "js", "viewer-workspaces.mjs"));

    private static Task<string> ReadViewerHistoryModuleAsync()
        => File.ReadAllTextAsync(FindRepositoryFile("Ctx.Viewer", "wwwroot", "js", "viewer-history.mjs"));

    private static Task<string> ReadViewerChromeModuleAsync()
        => File.ReadAllTextAsync(FindRepositoryFile("Ctx.Viewer", "wwwroot", "js", "viewer-chrome.mjs"));

    private static Task<string> ReadViewerIndexAsync()
        => File.ReadAllTextAsync(FindRepositoryFile("Ctx.Viewer", "wwwroot", "index.html"));

    private static Task<string> ReadViewerStylesAsync()
        => File.ReadAllTextAsync(FindRepositoryFile("Ctx.Viewer", "wwwroot", "styles.css"));

    private static string ExtractCssRule(string styles, string selector)
    {
        var start = styles.IndexOf(selector, StringComparison.Ordinal);
        if (start < 0)
        {
            return "";
        }

        var end = styles.IndexOf('}', start);
        return end < 0 ? styles[start..] : styles[start..(end + 1)];
    }

    private static string FindRepositoryFile(params string[] relativeParts)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(new[] { directory.FullName }.Concat(relativeParts).ToArray());
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Could not find repository file '{Path.Combine(relativeParts)}'.");
    }
}
