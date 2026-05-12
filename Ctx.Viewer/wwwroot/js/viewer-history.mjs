export function normalizeOverviewHistoryState(overview, defaultPageSize) {
    const timelineCommits = Array.isArray(overview?.timelineCommits)
        ? overview.timelineCommits
        : [];
    const timelinePage = overview?.timelinePage ?? {};

    return {
        ...overview,
        timelineCommits,
        timelinePage: normalizeTimelinePage(timelinePage, timelineCommits.length, defaultPageSize),
        timelineScope: overview?.timelineScope === "all" ? "all" : "selected"
    };
}

export function applyHydratedHistory(overview, payload, options = {}) {
    const hydratedTimelineCommits = Array.isArray(payload?.timelineCommits) ? payload.timelineCommits : [];
    const targetMaterializedCommitId = options.targetMaterializedCommitId ?? null;
    const selectedMaterializedCommit = targetMaterializedCommitId
        ? overview.timelineCommits.find(commit => commit.id === targetMaterializedCommitId)
        : null;
    const timelineCommits = selectedMaterializedCommit
        && !hydratedTimelineCommits.some(commit => commit.id === selectedMaterializedCommit.id)
        ? [...hydratedTimelineCommits, selectedMaterializedCommit]
        : hydratedTimelineCommits;

    return {
        ...overview,
        timelineCommits,
        timelinePage: payload?.timelinePage ?? overview.timelinePage,
        timelineScope: payload?.timelineScope ?? overview.timelineScope,
        historyPending: false,
        branches: applyBranchTimelineCounts(overview.branches, payload?.branchTimelineCounts)
    };
}

export function appendHistoryPage(overview, page) {
    const loadedCommits = Array.isArray(page?.timelineCommits) ? page.timelineCommits : [];
    const existingCommitIds = new Set(overview.timelineCommits.map(commit => commit.id));
    const appendedCommits = loadedCommits.filter(commit => !existingCommitIds.has(commit.id));
    const timelineCommits = [...overview.timelineCommits, ...appendedCommits];

    return {
        overview: {
            ...overview,
            timelineCommits,
            timelinePage: {
                ...(overview.timelinePage ?? {}),
                ...(page?.timelinePage ?? {}),
                loadedCount: timelineCommits.length
            }
        },
        appendedCount: appendedCommits.length
    };
}

function normalizeTimelinePage(timelinePage, loadedCount, defaultPageSize) {
    return {
        totalCount: Number.isFinite(timelinePage.totalCount) ? timelinePage.totalCount : loadedCount,
        loadedCount: Number.isFinite(timelinePage.loadedCount) ? timelinePage.loadedCount : loadedCount,
        limit: Number.isFinite(timelinePage.limit) ? timelinePage.limit : defaultPageSize,
        cursor: typeof timelinePage.cursor === "string" ? timelinePage.cursor : null,
        nextCursor: typeof timelinePage.nextCursor === "string" ? timelinePage.nextCursor : null,
        hasMore: Boolean(timelinePage.hasMore)
    };
}

function applyBranchTimelineCounts(branches, branchTimelineCounts) {
    if (!Array.isArray(branches)) {
        return branches;
    }

    const counts = branchTimelineCounts ?? {};
    return branches.map(branchItem => ({
        ...branchItem,
        timelineCommitCount: Object.prototype.hasOwnProperty.call(counts, branchItem.name)
            ? counts[branchItem.name]
            : branchItem.timelineCommitCount
    }));
}
