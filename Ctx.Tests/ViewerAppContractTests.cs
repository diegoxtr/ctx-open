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
        var styles = await ReadViewerStylesAsync();
        var graphCanvasStyles = ExtractCssRule(styles, ".graph-canvas");
        var diffCanvasStyles = ExtractCssRule(styles, ".graph-canvas-diff .diff-graph-canvas");

        Assert.Contains("overflow: auto", graphCanvasStyles);
        Assert.Contains("resize: vertical", graphCanvasStyles);
        Assert.Contains("scrollbar-gutter: stable both-edges", graphCanvasStyles);
        Assert.Contains("overflow: visible", diffCanvasStyles);
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
