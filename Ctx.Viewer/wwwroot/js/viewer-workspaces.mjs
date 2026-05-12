export function createWorkspaceTabId() {
    return `workspace-${Math.random().toString(36).slice(2, 10)}`;
}

export function deriveWorkspaceTabTitle(path) {
    const normalized = String(path ?? "").trim();
    if (!normalized) {
        return "New repo";
    }

    const segments = normalized.split(/[\\/]+/).filter(Boolean);
    return segments[segments.length - 1] ?? normalized;
}

export function deriveWorkspaceTabPathLabel(path) {
    const normalized = String(path ?? "").trim();
    if (!normalized) {
        return "No repository loaded";
    }

    const segments = normalized.split(/[\\/]+/).filter(Boolean);
    if (segments.length <= 3) {
        return normalized;
    }

    return `...\\${segments.slice(-3).join("\\")}`;
}

export function createWorkspaceTabState(seed = {}, defaults = {}) {
    return {
        id: seed.id ?? createWorkspaceTabId(),
        title: seed.title ?? deriveWorkspaceTabTitle(seed.repositoryPath),
        repositoryPath: seed.repositoryPath ?? "",
        branch: seed.branch ?? defaults.branch ?? "main",
        overview: seed.overview ?? null,
        graph: seed.graph ?? null,
        renderedGraph: seed.renderedGraph ?? null,
        hypothesisRanking: seed.hypothesisRanking ?? [],
        selectedCommitId: seed.selectedCommitId ?? null,
        compareBaseCommit: seed.compareBaseCommit ?? null,
        currentCommitComparison: seed.currentCommitComparison ?? null,
        diffViewMode: seed.diffViewMode ?? defaults.diffViewMode ?? "list",
        selectedNodeId: seed.selectedNodeId ?? null,
        lastLoadedAt: seed.lastLoadedAt ?? null,
        historyBranchFilters: seed.historyBranchFilters ?? defaults.historyBranchFilters ?? [],
        historyTimelineScope: seed.historyTimelineScope ?? defaults.historyTimelineScope ?? "selected",
        historySort: seed.historySort ?? defaults.historySort ?? "newest",
        graphPreset: seed.graphPreset ?? defaults.graphPreset ?? "all",
        graphFocusModes: seed.graphFocusModes ?? defaults.graphFocusModes ?? ["all"],
        graphZoom: seed.graphZoom ?? defaults.graphZoom ?? 1,
        taskStateSelection: seed.taskStateSelection ?? defaults.taskStateSelection ?? [],
        leftTab: seed.leftTab ?? defaults.leftTab ?? "history",
        detailTab: seed.detailTab ?? defaults.detailTab ?? "details",
        commitFocusEnabled: seed.commitFocusEnabled ?? defaults.commitFocusEnabled ?? true,
        primaryLineageOnly: seed.primaryLineageOnly ?? defaults.primaryLineageOnly ?? false,
        expandActiveLines: seed.expandActiveLines ?? defaults.expandActiveLines ?? false,
        showInterpretationRelations: seed.showInterpretationRelations ?? defaults.showInterpretationRelations ?? false,
        currentCommitFocus: seed.currentCommitFocus ?? null,
        currentGraphDemandMode: seed.currentGraphDemandMode ?? "full",
        expandedAllGraphNodeIds: seed.expandedAllGraphNodeIds ?? [],
        latestWorkingContextGraph: seed.latestWorkingContextGraph ?? null,
        retainedWorkingContextGraph: seed.retainedWorkingContextGraph ?? null,
        recentWorkingContextNewNodeIds: seed.recentWorkingContextNewNodeIds ?? [],
        recentGraphNewNodeIds: seed.recentGraphNewNodeIds ?? [],
        currentGraphSurfaceKey: seed.currentGraphSurfaceKey ?? null,
        workingContextSignalFingerprint: seed.workingContextSignalFingerprint ?? null,
        pendingStartupWorkingFocus: seed.pendingStartupWorkingFocus ?? true
    };
}

export function createWorkspaceTabShellPayload(tabs, defaultBranchName = "main") {
    return tabs.map(tab => ({
        id: tab.id,
        repositoryPath: tab.repositoryPath ?? "",
        branch: tab.branch ?? defaultBranchName,
        title: tab.title ?? deriveWorkspaceTabTitle(tab.repositoryPath)
    }));
}
