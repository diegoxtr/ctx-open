const repoForm = document.getElementById("repo-form");
const repoPathInput = document.getElementById("repo-path");
const branchSelect = document.getElementById("branch-select");
const refreshButton = document.getElementById("refresh-button");
const autoRefreshToggle = document.getElementById("auto-refresh-toggle");
const summaryCards = document.getElementById("summary-cards");
const hypothesisRanking = document.getElementById("hypothesis-ranking");
const hypothesisCount = document.getElementById("hypothesis-count");
const taskCount = document.getElementById("task-count");
const taskSummary = document.getElementById("task-summary");
const taskActiveCount = document.getElementById("task-active-count");
const taskClosedCount = document.getElementById("task-closed-count");
const taskActiveList = document.getElementById("task-active-list");
const taskClosedList = document.getElementById("task-closed-list");
const commitCount = document.getElementById("commit-count");
const historySortSelect = document.getElementById("history-sort-select");
const lastLoaded = document.getElementById("last-loaded");
const freshnessStatus = document.getElementById("freshness-status");
const commitList = document.getElementById("commit-list");
const graphCanvas = document.getElementById("graph-canvas");
const graphCaption = document.getElementById("graph-caption");
const graphFocusCaption = document.getElementById("graph-focus-caption");
const commitDetail = document.getElementById("commit-detail");
const nodeDetail = document.getElementById("node-detail");
const viewerHint = document.getElementById("viewer-hint");
const taskStateFilters = Array.from(document.querySelectorAll("[data-task-filter]"));
const graphPresetButtons = Array.from(document.querySelectorAll("[data-graph-preset]"));
const layout = document.querySelector(".layout");
const panelDividers = Array.from(document.querySelectorAll(".panel-divider"));
const viewModeButtons = Array.from(document.querySelectorAll(".view-mode-button"));
const viewModeCaption = document.getElementById("view-mode-caption");
const commitFocusToggle = document.getElementById("commit-focus-toggle");

const typeOrder = ["Project", "Goal", "Sub-goal", "Task", "Hypothesis", "Evidence", "Decision", "Conclusion", "Run", "ContextPacket"];
const lanePalette = ["#214a88", "#0f766e", "#8a3ffc", "#c2410c", "#0369a1", "#7c3aed", "#be123c"];
const viewerModeKey = "ctx-viewer-mode";
const graphPresetStorageKey = "ctx-viewer-graph-preset";
const graphFocusModesStorageKey = "ctx-viewer-graph-focus-modes";
const graphTaskFilterStorageKey = "ctx-viewer-graph-task-filters";
const commitFocusStorageKey = "ctx-viewer-commit-focus";
const historyBranchFilterStorageKey = "ctx-viewer-history-branch-filters";
const historySortStorageKey = "ctx-viewer-history-sort";
const panelLayoutStorageKey = "ctx-viewer-panel-layout";
const repositoryPathStorageKey = "ctx-viewer-repository-path";
const branchStorageKey = "ctx-viewer-branch";
const autoRefreshStorageKey = "ctx-viewer-auto-refresh";
const defaultBranchName = "main";
const viewModeDescriptions = {
    history: "History mode keeps timeline and commit detail readable by collapsing the graph panel.",
    split: "Split mode uses a compact commit navigator so the graph and detail panels stay readable.",
    graph: "Graph mode keeps only a compact commit navigator beside the graph and detail panels."
};
let currentOverview = null;
let currentGraph = null;
let currentRenderedGraph = null;
let currentHypothesisRanking = [];
let selectedCommitId = null;
let selectedNodeId = null;
let lastLoadedAt = null;
let autoRefreshHandle = null;
const autoRefreshIntervalMs = 5000;
let currentViewMode = loadStoredViewMode();
let currentGraphPreset = "all";
let currentGraphFocusModes = new Set(["all"]);
let currentHistoryBranchFilters = new Set();
let currentHistorySort = loadStoredHistorySort();
let currentPanelLayout = loadStoredPanelLayout();
let currentCommitFocus = null;
let currentCommitFocusEnabled = loadStoredCommitFocus();
let transientGraphFocusSnapshot = null;
let suppressGraphFocusPersist = false;

const graphPresets = {
    all: ["Draft", "Ready", "InProgress", "Blocked", "Done"],
    working: ["Ready", "InProgress", "Blocked"],
    thinking: ["Draft", "Ready"],
    closed: ["Done"]
};

for (const input of taskStateFilters) {
    input.addEventListener("change", () => {
        if (transientGraphFocusSnapshot) {
            transientGraphFocusSnapshot = null;
        }

        if (!currentGraph) {
            persistGraphFocusSelection();
            return;
        }

        syncPresetFromFilters();
        persistGraphFocusSelection();
        selectedNodeId = null;
        renderGraph(currentGraph);
        nodeDetail.textContent = "Click a node.";
    });
}

for (const button of graphPresetButtons) {
    button.addEventListener("click", () => {
        toggleGraphFocusMode(button.dataset.graphPreset);
    });
}

repoForm.addEventListener("submit", async (event) => {
    event.preventDefault();
    await loadOverview();
});

branchSelect.addEventListener("change", async () => {
    persistBranchSelection(branchSelect.value);
    await loadOverview(branchSelect.value);
});

refreshButton.addEventListener("click", async () => {
    await loadOverview(branchSelect.value || undefined);
});

historySortSelect.addEventListener("change", () => {
    currentHistorySort = historySortSelect.value === "oldest" ? "oldest" : "newest";
    persistHistorySort();
    if (currentOverview) {
        renderCommits(currentOverview);
    }
});

autoRefreshToggle.addEventListener("change", () => {
    persistAutoRefreshPreference();
    syncAutoRefresh();
    renderFreshnessStatus();
});

if (commitFocusToggle) {
    commitFocusToggle.addEventListener("change", () => {
        currentCommitFocusEnabled = commitFocusToggle.checked;
        persistCommitFocusSelection();
        if (currentGraph) {
            renderGraph(currentGraph);
        }
    });
}

window.addEventListener("load", async () => {
    applyViewMode(currentViewMode);
    restoreGraphFocusSelection();
    historySortSelect.value = currentHistorySort;
    autoRefreshToggle.checked = loadStoredAutoRefreshPreference();
    if (commitFocusToggle) {
        commitFocusToggle.checked = currentCommitFocusEnabled;
    }
    attachPanelDividerHandlers();
    applyPanelLayout();

    const params = new URLSearchParams(window.location.search);
    const path = params.get("path");
    const branch = params.get("branch");
    repoPathInput.value = path ?? loadStoredRepositoryPath() ?? "";
    await loadOverview(branch ?? loadStoredBranch() ?? defaultBranchName);
});

window.addEventListener("resize", () => {
    applyPanelLayout();
});

for (const button of viewModeButtons) {
    button.addEventListener("click", () => {
        applyViewMode(button.dataset.viewMode);
    });
}

async function loadOverview(branch) {
    const previousOverview = currentOverview;
    const previousSelectedCommitId = selectedCommitId;
    const repositoryPath = repoPathInput.value.trim();
    const params = new URLSearchParams();
    if (repositoryPath) {
        params.set("path", repositoryPath);
    }
    if (branch) {
        params.set("branch", branch);
    }

    const response = await fetch(`/api/overview?${params.toString()}`);
    if (!response.ok) {
        const error = await response.json();
        resetViewer(error.message ?? "Failed to load repository.");
        return;
    }

    const overview = await response.json();
    currentOverview = overview;
    lastLoadedAt = new Date();
    restoreHistoryBranchFilters(overview);
    repoPathInput.value = overview.repositoryPath;
    persistRepositoryPath(overview.repositoryPath);
    persistBranchSelection(overview.selectedBranch);
    viewerHint.textContent = `Loaded ${overview.repositoryPath}`;
    currentHypothesisRanking = await loadHypothesisRanking();
    renderFreshnessStatus();

    renderBranchSelect(overview);
    renderSummary(overview);
    renderHypothesisRanking(currentHypothesisRanking);
    renderTasks(overview);
    renderCommits(overview);

    const commitStillExists = previousSelectedCommitId
        && overview.timelineCommits.some(commit => commit.id === previousSelectedCommitId);
    const preferredCommit =
        commitStillExists ? previousSelectedCommitId :
        previousOverview && previousSelectedCommitId === null ? null :
        overview.timelineCommits[0]?.id ?? null;

    await selectCommit(preferredCommit);
    syncAutoRefresh();
}

function renderBranchSelect(overview) {
    branchSelect.innerHTML = "";
    for (const branch of overview.branches) {
        const option = document.createElement("option");
        option.value = branch.name;
        option.textContent = branch.name;
        option.selected = branch.name === overview.selectedBranch;
        branchSelect.appendChild(option);
    }
}

function loadStoredRepositoryPath() {
    return window.localStorage.getItem(repositoryPathStorageKey);
}

function persistRepositoryPath(path) {
    window.localStorage.setItem(repositoryPathStorageKey, path);
}

function loadStoredBranch() {
    return window.localStorage.getItem(branchStorageKey);
}

function persistBranchSelection(branch) {
    if (!branch) {
        return;
    }

    window.localStorage.setItem(branchStorageKey, branch);
}

function loadStoredAutoRefreshPreference() {
    const storedValue = window.localStorage.getItem(autoRefreshStorageKey);
    if (storedValue === null) {
        return true;
    }

    return storedValue !== "false";
}

function loadStoredCommitFocus() {
    const storedValue = window.localStorage.getItem(commitFocusStorageKey);
    if (storedValue === null) {
        return true;
    }

    return storedValue !== "false";
}

function persistCommitFocusSelection() {
    window.localStorage.setItem(commitFocusStorageKey, currentCommitFocusEnabled ? "true" : "false");
}

function persistAutoRefreshPreference() {
    window.localStorage.setItem(autoRefreshStorageKey, autoRefreshToggle.checked ? "true" : "false");
}

function renderSummary(overview) {
    const summary = overview.graphSummary;
    const cards = [
        ["Branch", overview.selectedBranch],
        ["Head", overview.headCommitId ? overview.headCommitId.slice(0, 8) : "working"],
        ["Branches", overview.branches.length],
        ["Timeline", overview.timelineCommits.length],
        ["Open Tasks", overview.taskSummary.open],
        ["Closed Tasks", overview.taskSummary.closed],
        ["Nodes", summary.graph.nodes],
        ["Edges", summary.graph.edges]
    ];

    lastLoaded.textContent = lastLoadedAt
        ? `Last loaded ${lastLoadedAt.toLocaleString()}`
        : "Not loaded yet";

    summaryCards.innerHTML = "";
    for (const [label, value] of cards) {
        const card = document.createElement("div");
        card.className = "card";
        card.innerHTML = `<span class="card-label">${label}</span><span class="card-value">${value}</span>`;
        summaryCards.appendChild(card);
    }
}

function renderTasks(overview) {
    const tasks = Array.isArray(overview.tasks) ? overview.tasks : [];
    const active = tasks.filter(task => task.state !== "Done");
    const closed = tasks.filter(task => task.state === "Done");

    taskCount.textContent = `${tasks.length}`;
    taskActiveCount.textContent = `${active.length}`;
    taskClosedCount.textContent = `${closed.length}`;
    taskSummary.innerHTML = "";
    taskActiveList.innerHTML = "";
    taskClosedList.innerHTML = "";

    const summaryItems = [
        ["In Progress", overview.taskSummary.inProgress],
        ["Ready", overview.taskSummary.ready],
        ["Blocked", overview.taskSummary.blocked]
    ];

    for (const [label, value] of summaryItems) {
        const badge = document.createElement("span");
        badge.className = "task-summary-pill";
        badge.innerHTML = `<strong>${escapeHtml(String(value))}</strong><span>${escapeHtml(label)}</span>`;
        taskSummary.appendChild(badge);
    }

    renderTaskList(taskActiveList, active, "No active tasks.");
    renderTaskList(taskClosedList, closed.slice(0, 12), closed.length > 12 ? `${closed.length - 12} more closed tasks exist in this workspace.` : "No closed tasks yet.");
}

function renderTaskList(container, items, emptyMessage) {
    if (items.length === 0) {
        container.innerHTML = `<li class="task-list-empty">${escapeHtml(emptyMessage)}</li>`;
        return;
    }

    for (const task of items) {
        const row = document.createElement("li");
        row.className = "task-list-row";
        const taskButton = document.createElement("button");
        taskButton.type = "button";
        taskButton.className = `task-item ${selectedNodeId === `Task:${task.id}` ? "active" : ""}`;
        taskButton.innerHTML = `
            <div class="task-item-header">
                <span class="task-state-badge task-state-${task.state.toLowerCase()}">${escapeHtml(task.state)}</span>
                <code>${escapeHtml(task.id.slice(0, 8))}</code>
            </div>
            <strong>${escapeHtml(task.title)}</strong>
            <span class="task-goal">${escapeHtml(task.goalTitle ?? "No goal")}</span>
            <span class="task-meta">${escapeHtml(String(task.hypothesisCount))} hypotheses${task.dependsOnTaskIds.length ? ` | ${escapeHtml(String(task.dependsOnTaskIds.length))} deps` : ""}</span>
        `;
        taskButton.addEventListener("click", async () => {
            await focusTask(task);
        });
        row.appendChild(taskButton);
        container.appendChild(row);
    }

    if (items.length > 0 && container === taskClosedList && currentOverview.tasks.length > items.length + taskActiveList.children.length) {
        const more = document.createElement("li");
        more.className = "task-list-empty";
        more.textContent = emptyMessage;
        container.appendChild(more);
    }
}

function applyViewMode(mode) {
    if (!viewModeDescriptions[mode]) {
        mode = "history";
    }

    currentViewMode = mode;
    layout.dataset.viewMode = mode;
    viewModeCaption.textContent = viewModeDescriptions[mode];
    window.localStorage.setItem(viewerModeKey, mode);

    for (const button of viewModeButtons) {
        button.classList.toggle("active", button.dataset.viewMode === mode);
    }

    applyPanelLayout();

    if (currentOverview) {
        renderCommits(currentOverview);
    }
}

function loadStoredViewMode() {
    const storedMode = window.localStorage.getItem(viewerModeKey);
    return viewModeDescriptions[storedMode] ? storedMode : "history";
}

function persistGraphFocusSelection() {
    if (suppressGraphFocusPersist) {
        return;
    }

    const selectedStates = taskStateFilters
        .filter(input => input.checked)
        .map(input => input.value);
    window.localStorage.setItem(graphPresetStorageKey, currentGraphPreset);
    window.localStorage.setItem(graphFocusModesStorageKey, JSON.stringify(Array.from(currentGraphFocusModes)));
    window.localStorage.setItem(graphTaskFilterStorageKey, JSON.stringify(selectedStates));
}

function loadStoredPanelLayout() {
    const raw = window.localStorage.getItem(panelLayoutStorageKey);
    const defaults = getDefaultPanelLayout();
    if (!raw) {
        return defaults;
    }

    try {
        const parsed = JSON.parse(raw);
        return {
            history: normalizePanelModeLayout(parsed.history, defaults.history),
            split: normalizePanelModeLayout(parsed.split, defaults.split),
            graph: normalizePanelModeLayout(parsed.graph, defaults.graph)
        };
    } catch {
        return defaults;
    }
}

function normalizePanelModeLayout(value, fallback) {
    if (!value || typeof value !== "object") {
        return { ...fallback };
    }

    return {
        left: Number.isFinite(value.left) ? value.left : fallback.left,
        right: Number.isFinite(value.right) ? value.right : fallback.right
    };
}

function getDefaultPanelLayout() {
    return {
        history: { left: 920, right: 360 },
        split: { left: 320, right: 360 },
        graph: { left: 280, right: 320 }
    };
}

function getPanelLayoutForMode(mode) {
    if (!currentPanelLayout[mode]) {
        currentPanelLayout[mode] = { ...getDefaultPanelLayout()[mode] };
    }

    return currentPanelLayout[mode];
}

function persistPanelLayout() {
    window.localStorage.setItem(panelLayoutStorageKey, JSON.stringify(currentPanelLayout));
}

function applyPanelLayout() {
    if (!layout) {
        return;
    }

    const mode = currentViewMode;
    const panelLayout = getPanelLayoutForMode(mode);
    const clamped = clampPanelWidths(mode, panelLayout.left, panelLayout.right);
    currentPanelLayout[mode] = clamped;
    layout.style.setProperty("--panel-left-width", `${clamped.left}px`);
    layout.style.setProperty("--panel-right-width", `${clamped.right}px`);
    persistPanelLayout();
}

function clampPanelWidths(mode, leftWidth, rightWidth) {
    const defaults = getDefaultPanelLayout()[mode] ?? getDefaultPanelLayout().split;
    const layoutWidth = Math.max(layout.getBoundingClientRect().width, 0);
    const dividerSize = 12;
    const computedStyle = window.getComputedStyle(layout);
    const columnGap = Number.parseFloat(computedStyle.columnGap ?? "0") || 0;
    const gapCount = mode === "history" ? 2 : 4;

    if (layoutWidth <= 0) {
        return {
            left: leftWidth ?? defaults.left,
            right: rightWidth ?? defaults.right
        };
    }

    const constraints = {
        history: { leftMin: 560, leftMax: 1400, rightMin: 280, rightMax: 640, centerMin: 0 },
        split: { leftMin: 260, leftMax: 720, rightMin: 280, rightMax: 640, centerMin: 360 },
        graph: { leftMin: 220, leftMax: 560, rightMin: 260, rightMax: 520, centerMin: 420 }
    }[mode] ?? { leftMin: 260, leftMax: 720, rightMin: 280, rightMax: 640, centerMin: 360 };

    let left = clampNumber(leftWidth ?? defaults.left, constraints.leftMin, constraints.leftMax);
    let right = clampNumber(rightWidth ?? defaults.right, constraints.rightMin, constraints.rightMax);
    const availableForPanels = layoutWidth
        - (mode === "history" ? dividerSize : dividerSize * 2)
        - (columnGap * gapCount);

    if (mode === "history") {
        const maxLeftFromViewport = Math.max(constraints.leftMin, availableForPanels - constraints.rightMin);
        left = clampNumber(left, constraints.leftMin, Math.min(constraints.leftMax, maxLeftFromViewport));
        const maxRightFromViewport = Math.max(constraints.rightMin, availableForPanels - left);
        right = clampNumber(right, constraints.rightMin, Math.min(constraints.rightMax, maxRightFromViewport));
        left = Math.max(constraints.leftMin, availableForPanels - right);
        return { left, right };
    }

    const availableForCenter = availableForPanels - left - right;
    if (availableForCenter < constraints.centerMin) {
        const deficit = constraints.centerMin - availableForCenter;
        const leftShrinkCapacity = Math.max(0, left - constraints.leftMin);
        const rightShrinkCapacity = Math.max(0, right - constraints.rightMin);
        const totalCapacity = leftShrinkCapacity + rightShrinkCapacity;

        if (totalCapacity > 0) {
            const leftShare = Math.min(leftShrinkCapacity, deficit * (leftShrinkCapacity / totalCapacity));
            const rightShare = Math.min(rightShrinkCapacity, deficit - leftShare);
            left -= leftShare;
            right -= rightShare;

            const remainingDeficit = constraints.centerMin - (availableForPanels - left - right);
            if (remainingDeficit > 0) {
                if (left - constraints.leftMin >= right - constraints.rightMin) {
                    left = Math.max(constraints.leftMin, left - remainingDeficit);
                } else {
                    right = Math.max(constraints.rightMin, right - remainingDeficit);
                }
            }
        } else {
            left = constraints.leftMin;
            right = constraints.rightMin;
        }
    }

    return { left, right };
}

function clampNumber(value, min, max) {
    return Math.min(max, Math.max(min, value));
}

function attachPanelDividerHandlers() {
    if (!layout || panelDividers.length === 0) {
        return;
    }

    for (const divider of panelDividers) {
        divider.addEventListener("pointerdown", event => {
            if (window.matchMedia("(max-width: 1200px)").matches) {
                return;
            }

            event.preventDefault();
            const dividerSide = divider.dataset.divider;
            const activeMode = currentViewMode;
            const pointerId = event.pointerId;
            divider.classList.add("dragging");
            divider.setPointerCapture(pointerId);

            const handleMove = moveEvent => {
                const layoutRect = layout.getBoundingClientRect();
                const modeLayout = getPanelLayoutForMode(activeMode);
                let nextLeft = modeLayout.left;
                let nextRight = modeLayout.right;

                if (dividerSide === "left") {
                    nextLeft = moveEvent.clientX - layoutRect.left;
                } else if (dividerSide === "right") {
                    nextRight = layoutRect.right - moveEvent.clientX;
                }

                currentPanelLayout[activeMode] = clampPanelWidths(activeMode, nextLeft, nextRight);

                if (activeMode === currentViewMode) {
                    applyPanelLayout();
                }
            };

            const finishDrag = () => {
                divider.classList.remove("dragging");
                divider.removeEventListener("pointermove", handleMove);
                divider.removeEventListener("pointerup", finishDrag);
                divider.removeEventListener("pointercancel", finishDrag);
                persistPanelLayout();
            };

            divider.addEventListener("pointermove", handleMove);
            divider.addEventListener("pointerup", finishDrag);
            divider.addEventListener("pointercancel", finishDrag);
        });
    }
}

function restoreGraphFocusSelection() {
    const storedStates = loadStoredTaskStateSelection();
    const storedFocusModes = loadStoredGraphFocusModes();
    const storedPreset = window.localStorage.getItem(graphPresetStorageKey);

    currentGraphFocusModes = storedFocusModes.size > 0 ? storedFocusModes : new Set(["all"]);

    if (storedStates.length > 0) {
        for (const input of taskStateFilters) {
            input.checked = storedStates.includes(input.value);
        }
    } else {
        for (const input of taskStateFilters) {
            input.checked = true;
        }
    }

    if (storedPreset === "custom") {
        currentGraphPreset = "custom";
    } else {
        syncPresetFromFilters();
    }

    updatePresetButtons();
}

function loadStoredGraphFocusModes() {
    const raw = window.localStorage.getItem(graphFocusModesStorageKey);
    if (!raw) {
        return new Set(["all"]);
    }

    try {
        const parsed = JSON.parse(raw);
        if (!Array.isArray(parsed)) {
            return new Set(["all"]);
        }

        const validModes = parsed.filter(mode => mode === "all" || graphPresets[mode]);
        return new Set(validModes.length > 0 ? validModes : ["all"]);
    } catch {
        return new Set(["all"]);
    }
}

function loadStoredTaskStateSelection() {
    const raw = window.localStorage.getItem(graphTaskFilterStorageKey);
    if (!raw) {
        return [];
    }

    try {
        const parsed = JSON.parse(raw);
        if (!Array.isArray(parsed)) {
            return [];
        }

        return parsed.filter(state => taskStateFilters.some(input => input.value === state));
    } catch {
        return [];
    }
}

function renderCommits(overview) {
    commitList.innerHTML = "";
    commitCount.textContent = `${overview.timelineCommits.length}`;
    const lanes = buildLaneMap(overview);
    const laneCount = Math.max(overview.branches.length, 1);
    const filteredBranchNames = resolveVisibleHistoryBranches(overview);
    const groupedCommits = new Map();

    for (const branch of overview.branches) {
        groupedCommits.set(branch.name, []);
    }

    for (const commit of overview.timelineCommits) {
        if (!groupedCommits.has(commit.branch)) {
            groupedCommits.set(commit.branch, []);
        }
        groupedCommits.get(commit.branch).push(commit);
    }

    const orderedBranches = getOrderedHistoryBranches(overview, filteredBranchNames, groupedCommits);

    if (currentViewMode === "history") {
        renderBranchFirstHistory(overview, orderedBranches, groupedCommits, lanes, laneCount);
        return;
    }

    renderCompactHistoryNavigator(overview, orderedBranches, groupedCommits, lanes, laneCount);
}

function renderBranchFirstHistory(overview, orderedBranches, groupedCommits, lanes, laneCount) {
    const historyLayout = document.createElement("div");
    historyLayout.className = "history-browser";

    const branchPanel = document.createElement("aside");
    branchPanel.className = "history-branch-panel";
    branchPanel.innerHTML = `<div class="history-branch-panel-heading"><h4>Branches</h4><button type="button" class="history-branch-reset">All</button></div>`;

    const branchList = document.createElement("div");
    branchList.className = "history-branch-list";

    for (const branch of orderedBranches) {
        const branchButton = document.createElement("button");
        branchButton.type = "button";
        const isActive = currentHistoryBranchFilters.has(branch.name);
        branchButton.className = `history-branch-item ${isActive ? "active" : ""} ${branch.name === overview.selectedBranch ? "selected-branch" : ""}`;
        branchButton.innerHTML = `
            <span class="history-branch-chip" style="--branch-color:${laneColor(lanes.get(branch.name))}"></span>
            <span class="history-branch-name">${escapeHtml(branch.name)}</span>
            <span class="history-branch-count">${escapeHtml(String(groupedCommits.get(branch.name)?.length ?? 0))}</span>
        `;
        branchButton.onclick = () => {
            toggleHistoryBranchFilter(branch.name, overview);
        };
        branchList.appendChild(branchButton);
    }

    branchPanel.querySelector(".history-branch-reset").onclick = () => {
        currentHistoryBranchFilters = new Set(overview.branches.map(branch => branch.name));
        persistHistoryBranchFilters();
        renderCommits(overview);
    };
    branchPanel.appendChild(branchList);
    historyLayout.appendChild(branchPanel);

    const historyTable = document.createElement("div");
    historyTable.className = "history-table history-table-branch";

    const laneLegend = document.createElement("div");
    laneLegend.className = "timeline-legend";
    laneLegend.innerHTML = orderedBranches
        .map(branch => `<span class="branch-chip compact" style="--branch-color:${laneColor(lanes.get(branch.name))}"><span class="branch-chip-dot"></span>${escapeHtml(branch.name)}</span>`)
        .join("");
    historyTable.appendChild(laneLegend);

    const header = document.createElement("div");
    header.className = "history-header";
    header.innerHTML = `
        <span>Graph</span>
        <span>Description</span>
        <span>Changes</span>
        <span>Date</span>
        <span>Author</span>
        <span>Model</span>
        <span>Commit</span>
    `;
    historyTable.appendChild(header);

    const rows = document.createElement("div");
    rows.className = "history-rows";
    rows.appendChild(renderWorkingHistoryRow(laneCount));

    for (const branch of orderedBranches) {
        const branchGroup = document.createElement("section");
        branchGroup.className = "history-branch-group";
        branchGroup.innerHTML = `
            <div class="history-branch-group-header">
                <span class="branch-chip" style="--branch-color:${laneColor(lanes.get(branch.name))}"><span class="branch-chip-dot"></span>${escapeHtml(branch.name)}</span>
                <span class="history-branch-group-meta">${escapeHtml(String(groupedCommits.get(branch.name)?.length ?? 0))} commits</span>
            </div>
        `;

        const branchRows = document.createElement("div");
        branchRows.className = "history-branch-group-rows";

        for (const commit of getOrderedHistoryCommits(groupedCommits.get(branch.name) ?? [])) {
            branchRows.appendChild(renderHistoryCommitRow(commit, overview, lanes, laneCount));
        }

        branchGroup.appendChild(branchRows);
        rows.appendChild(branchGroup);
    }

    historyTable.appendChild(rows);
    historyLayout.appendChild(historyTable);
    commitList.appendChild(historyLayout);
}

function renderCompactHistoryNavigator(overview, orderedBranches, groupedCommits, lanes, laneCount) {
    const compactLayout = document.createElement("div");
    compactLayout.className = "history-compact";

    const branchBar = document.createElement("div");
    branchBar.className = "history-compact-branches";
    branchBar.innerHTML = `<button type="button" class="history-compact-branch ${currentHistoryBranchFilters.size === overview.branches.length ? "active" : ""}">All</button>`;
    branchBar.firstElementChild.onclick = () => {
        currentHistoryBranchFilters = new Set(overview.branches.map(branch => branch.name));
        persistHistoryBranchFilters();
        renderCommits(overview);
    };

    for (const branch of orderedBranches) {
        const button = document.createElement("button");
        button.type = "button";
        const isActive = currentHistoryBranchFilters.has(branch.name);
        button.className = `history-compact-branch ${isActive ? "active" : ""} ${branch.name === overview.selectedBranch ? "selected-branch" : ""}`;
        button.innerHTML = `<span class="history-branch-chip" style="--branch-color:${laneColor(lanes.get(branch.name))}"></span>${escapeHtml(branch.name)}`;
        button.onclick = () => toggleHistoryBranchFilter(branch.name, overview);
        branchBar.appendChild(button);
    }
    compactLayout.appendChild(branchBar);

    const compactList = document.createElement("div");
    compactList.className = "history-table history-table-compact";

    const rows = document.createElement("div");
    rows.className = "history-compact-rows";
    rows.appendChild(renderWorkingHistoryRow(laneCount, true));

    const filteredCommits = [];
    for (const branch of orderedBranches) {
        for (const commit of getOrderedHistoryCommits(groupedCommits.get(branch.name) ?? [])) {
            filteredCommits.push(commit);
        }
    }

    for (const commit of getOrderedHistoryCommits(filteredCommits)) {
        rows.appendChild(renderHistoryCommitRow(commit, overview, lanes, laneCount, true));
    }

    compactList.appendChild(rows);
    compactLayout.appendChild(compactList);
    commitList.appendChild(compactLayout);
}

function renderWorkingHistoryRow(laneCount, compact = false) {
    const workingItem = document.createElement("button");
    workingItem.type = "button";
    workingItem.className = `history-row history-row-working ${compact ? "history-row-compact" : ""} ${selectedCommitId === null ? "active" : ""}`;
    workingItem.innerHTML = compact
        ? `
            <div class="history-cell history-lane-cell">
                <div class="timeline-gutter">
                    ${renderLaneGrid(laneCount, null, [], true)}
                </div>
            </div>
            <div class="history-cell history-main-cell">
                <div class="commit-meta compact">
                    <code>working</code>
                    <span class="commit-branch-label">uncommitted</span>
                </div>
                <span class="commit-message">Working context</span>
                <span class="commit-summary">Current .ctx state</span>
            </div>
            <div class="history-cell history-date-cell">
                <span class="commit-date">working tree</span>
            </div>
        `
        : `
            <div class="history-cell history-lane-cell">
                <div class="timeline-gutter">
                    ${renderLaneGrid(laneCount, null, [], true)}
                </div>
            </div>
            <div class="history-cell history-main-cell">
                <div class="commit-meta compact">
                    <code>working</code>
                    <span class="commit-branch-label">uncommitted</span>
                </div>
                <span class="commit-message">Working context</span>
                <span class="commit-summary">Current .ctx state</span>
            </div>
            <div class="history-cell history-change-cell">
                <span class="history-change-count">live</span>
                <span class="history-change-summary">uncommitted context</span>
            </div>
            <div class="history-cell history-date-cell">
                <span class="commit-date">working tree</span>
            </div>
            <div class="history-cell history-author-cell">
                <span class="history-author">live workspace</span>
            </div>
            <div class="history-cell history-model-cell">
                <span class="history-model">not recorded</span>
            </div>
            <div class="history-cell history-id-cell">
                <code>working</code>
            </div>`;
    workingItem.onclick = () => {
        applyWorkingContextFiltersTransient();
        selectCommit(null);
    };
    return workingItem;
}

function renderHistoryCommitRow(commit, overview, lanes, laneCount, compact = false) {
    const laneIndex = lanes.get(commit.branch) ?? 0;
    const headLaneIndexes = (commit.headBranches ?? [])
        .map(branchName => lanes.get(branchName))
        .filter(index => Number.isInteger(index));
    const headBadges = (commit.headBranches ?? [])
        .map(branchName => `<span class="branch-chip head-chip compact" style="--branch-color:${laneColor(lanes.get(branchName))}"><span class="branch-chip-dot"></span>${escapeHtml(branchName)}</span>`)
        .join("");
    const branchState = commit.branch === overview.selectedBranch ? "selected-branch" : "";
    const pathMeta = buildCognitivePathMeta(commit.cognitivePath);
    const goalMarkup = pathMeta.goal
        ? `<span class="history-goal-chip" title="${escapeHtml(pathMeta.goal)}">${escapeHtml(pathMeta.goal)}</span>`
        : `<span class="history-goal-chip muted">No goal</span>`;
    const pathMarkup = pathMeta.segments.length > 0
        ? `<div class="history-cognitive-path">${pathMeta.segments.map(segment => `<span class="history-path-segment"><span class="history-path-label">${escapeHtml(segment.label)}</span><span class="history-path-value">${escapeHtml(segment.value)}</span></span>`).join("<span class=\"history-path-arrow\">&rarr;</span>")}</div>`
        : `<span class="commit-summary">No cognitive path captured for this commit.</span>`;
    const item = document.createElement("button");
    item.type = "button";
    item.className = `history-row ${compact ? "history-row-compact" : ""} ${branchState} ${selectedCommitId === commit.id ? "active" : ""}`;
    item.innerHTML = compact
        ? `
            <div class="history-cell history-lane-cell">
                <div class="timeline-gutter">
                    ${renderLaneGrid(laneCount, laneIndex, headLaneIndexes)}
                </div>
            </div>
            <div class="history-cell history-main-cell">
                <div class="commit-meta compact">
                    <span class="commit-branch-label">${escapeHtml(commit.branch)}</span>
                    ${headBadges}
                </div>
                ${goalMarkup}
                <span class="commit-message">${escapeHtml(commit.message)}</span>
                ${pathMeta.segments.length > 0 ? `<span class="commit-summary">${escapeHtml(pathMeta.compactSummary)}</span>` : ""}
                ${pathMeta.goal ? `<span class="commit-summary compact-goal-line">Goal: ${escapeHtml(pathMeta.goal)}</span>` : ""}
                <span class="commit-summary">${escapeHtml(commit.author ?? "unknown")} | ${escapeHtml(new Date(commit.createdAtUtc).toLocaleString())} | ${escapeHtml(commit.id.slice(0, 8))}</span>
                <span class="history-author-meta">${escapeHtml(formatModelIdentity(commit.modelName, commit.modelVersion))}</span>
            </div>
            <div class="history-cell history-date-cell">
                <span class="history-change-count">${escapeHtml(String(commit.changedEntityCount ?? 0))}</span>
                <span class="history-change-summary">${escapeHtml(commit.changedEntitySummary ?? "No entity changes")}</span>
            </div>
        `
        : `
            <div class="history-cell history-lane-cell">
                <div class="timeline-gutter">
                    ${renderLaneGrid(laneCount, laneIndex, headLaneIndexes)}
                </div>
            </div>
            <div class="history-cell history-main-cell">
                <div class="commit-meta compact">
                    <span class="commit-branch-label">${escapeHtml(commit.branch)}</span>
                    ${headBadges}
                </div>
                ${goalMarkup}
                <span class="commit-message">${escapeHtml(commit.message)}</span>
                <span class="commit-summary">${escapeHtml(commit.summary)}</span>
                ${pathMarkup}
            </div>
            <div class="history-cell history-change-cell">
                <span class="history-change-count">${escapeHtml(String(commit.changedEntityCount ?? 0))}</span>
                <span class="history-change-summary">${escapeHtml(commit.changedEntitySummary ?? "No entity changes")}</span>
            </div>
            <div class="history-cell history-date-cell">
                <span class="commit-date">${new Date(commit.createdAtUtc).toLocaleString()}</span>
            </div>
            <div class="history-cell history-author-cell">
                <span class="history-author">${escapeHtml(commit.author ?? "unknown")}</span>
            </div>
            <div class="history-cell history-model-cell">
                <span class="history-model">${escapeHtml(formatModelIdentity(commit.modelName, commit.modelVersion))}</span>
            </div>
            <div class="history-cell history-id-cell">
                <code>${escapeHtml(commit.id.slice(0, 8))}</code>
            </div>
        `;
    item.onclick = () => selectCommit(commit.id);
    return item;
}

function restoreHistoryBranchFilters(overview) {
    const raw = window.localStorage.getItem(historyBranchFilterStorageKey);
    const branchNames = new Set(overview.branches.map(branch => branch.name));

    if (!raw) {
        currentHistoryBranchFilters = new Set(branchNames);
        return;
    }

    try {
        const parsed = JSON.parse(raw);
        const filtered = Array.isArray(parsed)
            ? parsed.filter(name => branchNames.has(name))
            : [];
        currentHistoryBranchFilters = new Set(filtered.length > 0 ? filtered : branchNames);
    } catch {
        currentHistoryBranchFilters = new Set(branchNames);
    }
}

function loadStoredHistorySort() {
    const storedValue = window.localStorage.getItem(historySortStorageKey);
    return storedValue === "oldest" ? "oldest" : "newest";
}

function persistHistorySort() {
    window.localStorage.setItem(historySortStorageKey, currentHistorySort);
}

function persistHistoryBranchFilters() {
    window.localStorage.setItem(historyBranchFilterStorageKey, JSON.stringify(Array.from(currentHistoryBranchFilters)));
}

function resolveVisibleHistoryBranches(overview) {
    const branchNames = overview.branches.map(branch => branch.name);
    if (currentHistoryBranchFilters.size === 0) {
        currentHistoryBranchFilters = new Set(branchNames);
    }

    return new Set(branchNames.filter(name => currentHistoryBranchFilters.has(name)));
}

function getOrderedHistoryBranches(overview, filteredBranchNames, groupedCommits) {
    return overview.branches
        .filter(item => filteredBranchNames.has(item.name))
        .slice()
        .sort((left, right) => {
            if (left.name === overview.selectedBranch && right.name !== overview.selectedBranch) {
                return -1;
            }

            if (right.name === overview.selectedBranch && left.name !== overview.selectedBranch) {
                return 1;
            }

            const leftLatest = getLatestCommitTimestamp(groupedCommits.get(left.name) ?? []);
            const rightLatest = getLatestCommitTimestamp(groupedCommits.get(right.name) ?? []);
            if (rightLatest !== leftLatest) {
                return rightLatest - leftLatest;
            }

            return left.name.localeCompare(right.name);
        });
}

function getOrderedHistoryCommits(commits) {
    return commits
        .slice()
        .sort((left, right) => {
            const leftTime = new Date(left.createdAtUtc).getTime();
            const rightTime = new Date(right.createdAtUtc).getTime();
            return currentHistorySort === "oldest"
                ? leftTime - rightTime
                : rightTime - leftTime;
        });
}

function getLatestCommitTimestamp(commits) {
    if (!Array.isArray(commits) || commits.length === 0) {
        return 0;
    }

    return Math.max(...commits.map(commit => new Date(commit.createdAtUtc).getTime()));
}

function toggleHistoryBranchFilter(branchName, overview) {
    if (currentHistoryBranchFilters.has(branchName)) {
        currentHistoryBranchFilters.delete(branchName);
    } else {
        currentHistoryBranchFilters.add(branchName);
    }

    if (currentHistoryBranchFilters.size === 0) {
        currentHistoryBranchFilters = new Set(overview.branches.map(branch => branch.name));
    }

    persistHistoryBranchFilters();
    renderCommits(overview);
}

async function loadHypothesisRanking() {
    const params = new URLSearchParams({ path: repoPathInput.value });
    const response = await fetch(`/api/hypotheses/rank?${params.toString()}`);
    if (!response.ok) {
        return [];
    }

    const payload = await response.json();
    return Array.isArray(payload) ? payload.slice(0, 5) : [];
}

function renderHypothesisRanking(items) {
    hypothesisCount.textContent = `${items.length}`;
    hypothesisRanking.innerHTML = "";

    for (const item of items) {
        const row = document.createElement("li");
        row.className = "hypothesis-ranking-item";
        row.innerHTML = `
            <div class="hypothesis-ranking-header">
                <span class="hypothesis-score">score ${escapeHtml(String(item.score))}</span>
                <code>${escapeHtml(item.id.slice(0, 8))}</code>
            </div>
            <strong>${escapeHtml(item.statement)}</strong>
            <span class="hypothesis-ranking-meta">p ${escapeHtml(String(item.probability))} | i ${escapeHtml(String(item.impact))}</span>
        `;
        hypothesisRanking.appendChild(row);
    }
}

async function selectCommit(commitId) {
    if (!currentOverview) {
        return;
    }

    if (commitId !== null && transientGraphFocusSnapshot) {
        restoreGraphFocus(transientGraphFocusSnapshot);
        transientGraphFocusSnapshot = null;
    }

    selectedCommitId = commitId;
    renderCommits(currentOverview);
    currentCommitFocus = null;

    const params = new URLSearchParams({ path: repoPathInput.value });
    if (commitId) {
        params.set("commitId", commitId);
    }

    const graphResponse = await fetch(`/api/graph?${params.toString()}`);
    if (!graphResponse.ok) {
        const error = await graphResponse.json();
        graphCanvas.innerHTML = "";
        graphCaption.textContent = "Graph unavailable";
        nodeDetail.textContent = JSON.stringify(error, null, 2);
        return;
    }

    currentGraph = await graphResponse.json();
    selectedNodeId = null;

    if (commitId) {
        const detailResponse = await fetch(`/api/commit?id=${encodeURIComponent(commitId)}&path=${encodeURIComponent(repoPathInput.value)}`);
        const detail = await detailResponse.json();
        currentCommitFocus = buildCommitFocus(detail, currentGraph);
        ensureAllTaskStateFiltersChecked();
        renderCommitDetail(detail);
        graphCaption.textContent = `Commit ${commitId.slice(0, 8)}`;
    } else {
        renderWorkingDetail();
        graphCaption.textContent = "Working context";
    }

    renderGraph(currentGraph);
    renderTasks(currentOverview);

    nodeDetail.textContent = "Click a node.";
}

function renderGraph(graph) {
    const scopedGraph = currentCommitFocusEnabled && currentCommitFocus
        ? filterGraphByCommitFocus(graph, currentCommitFocus)
        : graph;
    const filteredGraph = filterGraphByTaskState(scopedGraph);
    currentRenderedGraph = filteredGraph;
    graphCanvas.innerHTML = "";
    renderGraphFocusCaption(filteredGraph);

    if (filteredGraph.nodes.length === 0) {
        const manualStates = taskStateFilters
            .filter(input => input.checked)
            .map(input => input.dataset.taskFilter);
        const effectiveStates = resolveEffectiveTaskStates();
        let hint = "";

        if (manualStates.length === 0) {
            hint = "Select at least one task state.";
        } else if (effectiveStates.length === 0) {
            hint = "Your focus presets exclude all selected states. Enable All, Working, Thinking, or Closed.";
        } else if (selectedCommitId) {
            hint = currentCommitFocus
                ? "No nodes match this commit focus with the current task-state filters."
                : "This commit may only contain closed tasks. Include Done or select Working context.";
        }

        graphCanvas.innerHTML = `<p class="detail-empty">No graph nodes match the selected task states.${hint ? ` ${hint}` : ""}</p>`;
        return;
    }

    const stage = document.createElement("div");
    stage.className = "graph-stage";
    graphCanvas.appendChild(stage);

    const svg = document.createElementNS("http://www.w3.org/2000/svg", "svg");
    svg.classList.add("graph-svg");
    stage.appendChild(svg);

    const grouped = new Map();
    for (const type of typeOrder) {
        grouped.set(type, []);
    }

    const subGoalIds = new Set();
    if (Array.isArray(currentCommitFocus?.subGoalIds)) {
        for (const id of currentCommitFocus.subGoalIds) {
            if (id) {
                subGoalIds.add(`Goal:${id}`);
            }
        }
    }
    for (const edge of graph?.edges ?? []) {
        if (edge.relationship === "subgoal" && edge.to?.startsWith("Goal:")) {
            subGoalIds.add(edge.to);
        }
    }
    const hasSubGoalNodes = subGoalIds.size > 0;
    for (const edge of filteredGraph.edges ?? []) {
        if (edge.relationship === "subgoal" && edge.to?.startsWith("Goal:")) {
            subGoalIds.add(edge.to);
        }
    }

    for (const node of filteredGraph.nodes) {
        const displayType = node.type === "Goal" && subGoalIds.has(node.id) ? "Sub-goal" : node.type;
        if (!grouped.has(displayType)) {
            grouped.set(displayType, []);
        }

        grouped.get(displayType).push(node);
    }

    const positions = new Map();
    const columnGap = 190;
    const rowGap = 110;
    let columnIndex = 0;
    let maxRows = 1;

    for (const [type, nodes] of grouped.entries()) {
        if (nodes.length === 0 && !(type === "Sub-goal" && hasSubGoalNodes)) {
            continue;
        }

        sortGraphNodes(nodes);
        maxRows = Math.max(maxRows, nodes.length);
        const column = document.createElement("div");
        column.className = "graph-column";
        column.style.left = `${columnIndex * columnGap}px`;
        column.innerHTML = `<h4>${type}</h4>`;
        stage.appendChild(column);

        nodes.forEach((node, rowIndex) => {
            const y = 50 + rowIndex * rowGap;
            positions.set(node.id, { x: columnIndex * columnGap + 10, y });
            const scoreBadge = node.type === "Hypothesis" && node.metadata?.score
                ? `<span class="node-score">score ${escapeHtml(node.metadata.score)}</span>`
                : "";
            const displayType = node.type === "Goal" && subGoalIds.has(node.id) ? "Sub-goal" : node.type;

            const card = document.createElement("button");
            const commitFocusClass = currentCommitFocus ? "commit-focus" : "";
            const commitChangedClass = currentCommitFocus?.changedIds.has(node.id) ? "commit-changed" : "";
            card.className = `graph-node ${commitFocusClass} ${commitChangedClass} ${selectedNodeId === node.id ? "active" : ""}`;
            card.style.left = `${columnIndex * columnGap + 10}px`;
            card.style.top = `${y}px`;
            card.innerHTML = `<small>${displayType}</small><strong>${escapeHtml(node.label)}</strong>${scoreBadge}`;
            card.onclick = () => showNode(node.id);
            stage.appendChild(card);
        });

        if (nodes.length === 0 && type === "Sub-goal") {
            const empty = document.createElement("p");
            empty.className = "graph-empty";
            empty.textContent = "No sub-goals in focus.";
            empty.style.left = `${columnIndex * columnGap + 10}px`;
            empty.style.top = "50px";
            stage.appendChild(empty);
        }

        columnIndex += 1;
    }

    stage.style.width = `${Math.max(960, columnIndex * columnGap + 220)}px`;
    stage.style.height = `${Math.max(720, maxRows * rowGap + 150)}px`;
    svg.setAttribute("viewBox", `0 0 ${Math.max(960, columnIndex * columnGap + 220)} ${Math.max(720, maxRows * rowGap + 150)}`);

    for (const edge of filteredGraph.edges) {
        const from = positions.get(edge.from);
        const to = positions.get(edge.to);
        if (!from || !to) {
            continue;
        }

        const line = document.createElementNS("http://www.w3.org/2000/svg", "path");
        const startX = from.x + 160;
        const startY = from.y + 28;
        const endX = to.x;
        const endY = to.y + 28;
        const midX = (startX + endX) / 2;
        line.setAttribute("d", `M ${startX} ${startY} C ${midX} ${startY}, ${midX} ${endY}, ${endX} ${endY}`);
        line.setAttribute("fill", "none");
        line.setAttribute("stroke", "#91a9ce");
        line.setAttribute("stroke-width", "2");
        line.classList.add("graph-edge");

        if (currentCommitFocus?.nodeIds?.has(edge.from) && currentCommitFocus?.nodeIds?.has(edge.to)) {
            line.classList.add("commit-path");
        }
        if (currentCommitFocus?.changedIds?.has(edge.from) && currentCommitFocus?.changedIds?.has(edge.to)) {
            line.classList.add("commit-path-changed");
        }
        svg.appendChild(line);
    }
}

function showNode(nodeId) {
    selectedNodeId = nodeId;
    renderGraph(currentGraph);
    if (currentOverview) {
        renderTasks(currentOverview);
    }

    const graph = currentRenderedGraph ?? currentGraph;
    const node = graph.nodes.find(item => item.id === nodeId);
    const incoming = graph.edges.filter(edge => edge.to === nodeId);
    const outgoing = graph.edges.filter(edge => edge.from === nodeId);
    const connectedNodeIds = [...new Set([...incoming.map(edge => edge.from), ...outgoing.map(edge => edge.to)])];
    const connectedNodes = graph.nodes.filter(nodeItem => connectedNodeIds.includes(nodeItem.id));

    nodeDetail.textContent = JSON.stringify({ node, incoming, outgoing, connectedNodes }, null, 2);

    const activeNode = graphCanvas.querySelector(".graph-node.active");
    activeNode?.scrollIntoView({ block: "center", inline: "center", behavior: "smooth" });
}

async function focusTask(task) {
    if (!currentOverview) {
        return;
    }

    if (selectedCommitId !== null) {
        await selectCommit(null);
    }

    ensureTaskStateVisible(task.state);

    if (currentViewMode === "history") {
        applyViewMode("split");
    }

    showNode(`Task:${task.id}`);
}

function ensureTaskStateVisible(taskState) {
    const input = taskStateFilters.find(filter => filter.dataset.taskFilter === taskState);
    if (!input) {
        return;
    }

    if (!input.checked) {
        input.checked = true;
        syncPresetFromFilters();
        persistGraphFocusSelection();
        if (currentGraph) {
            renderGraph(currentGraph);
        }
    }
}

function resetViewer(message) {
    currentOverview = null;
    currentGraph = null;
    currentRenderedGraph = null;
    currentHypothesisRanking = [];
    selectedCommitId = null;
    selectedNodeId = null;
    currentCommitFocus = null;
    branchSelect.innerHTML = "";
    summaryCards.innerHTML = "";
    hypothesisRanking.innerHTML = "";
    hypothesisCount.textContent = "";
    taskCount.textContent = "";
    taskSummary.innerHTML = "";
    taskActiveCount.textContent = "";
    taskClosedCount.textContent = "";
    commitCount.textContent = "";
    taskActiveList.innerHTML = "";
    taskClosedList.innerHTML = "";
    lastLoaded.textContent = "Not loaded yet";
    freshnessStatus.textContent = "Auto-refresh off";
    commitList.innerHTML = "";
    graphCanvas.innerHTML = "";
    graphCaption.textContent = "No repository loaded";
    graphFocusCaption.textContent = "Showing all task states.";
    commitDetail.innerHTML = `<p class="detail-empty">Select a commit.</p>`;
    nodeDetail.textContent = "Click a node.";
    viewerHint.textContent = message;
    stopAutoRefresh();
}

function filterGraphByTaskState(graph) {
    const manuallySelectedStates = new Set(
        taskStateFilters
            .filter(input => input.checked)
            .map(input => input.dataset.taskFilter)
    );

    const effectiveStates = new Set(resolveEffectiveTaskStates());

    if (manuallySelectedStates.size === 0 || effectiveStates.size === 0) {
        return { nodes: [], edges: [] };
    }

    const taskNodes = graph.nodes.filter(node => node.type === "Task");
    if (taskNodes.length === 0) {
        return graph;
    }

    const selectedTaskIds = new Set(
        taskNodes
            .filter(node => effectiveStates.has(node.state))
            .map(node => node.id)
    );

    if (selectedTaskIds.size === 0) {
        return { nodes: [], edges: [] };
    }

    if (selectedTaskIds.size === taskNodes.length) {
        return graph;
    }

    const adjacency = new Map();
    for (const node of graph.nodes) {
        adjacency.set(node.id, []);
    }

    for (const edge of graph.edges) {
        adjacency.get(edge.from)?.push(edge.to);
        adjacency.get(edge.to)?.push(edge.from);
    }

    const nodeMap = new Map(graph.nodes.map(node => [node.id, node]));
    const nodeAssociations = new Map(graph.nodes.map(node => [node.id, new Set()]));

    for (const taskId of taskNodes.map(node => node.id)) {
        const queue = [taskId];
        const visited = new Set([taskId]);

        while (queue.length > 0) {
            const currentId = queue.shift();
            nodeAssociations.get(currentId)?.add(taskId);

            for (const neighborId of adjacency.get(currentId) ?? []) {
                if (visited.has(neighborId)) {
                    continue;
                }

                const neighbor = nodeMap.get(neighborId);
                if (!neighbor) {
                    continue;
                }

                if (neighbor.type === "Task" && neighbor.id !== taskId) {
                    continue;
                }

                visited.add(neighborId);
                queue.push(neighborId);
            }
        }
    }

    const visibleIds = new Set();

    for (const node of graph.nodes) {
        if (node.type === "Task") {
            if (selectedTaskIds.has(node.id)) {
                visibleIds.add(node.id);
            }
            continue;
        }

        if (node.type === "Project") {
            visibleIds.add(node.id);
            continue;
        }

        const associatedTaskIds = nodeAssociations.get(node.id) ?? new Set();
        if (associatedTaskIds.size === 0) {
            continue;
        }

        const hasSelectedAssociation = Array.from(associatedTaskIds).some(taskId => selectedTaskIds.has(taskId));
        const hasUnselectedAssociation = Array.from(associatedTaskIds).some(taskId => !selectedTaskIds.has(taskId));

        if (node.type === "Goal") {
            if (hasSelectedAssociation) {
                visibleIds.add(node.id);
            }
            continue;
        }

        if (hasSelectedAssociation && !hasUnselectedAssociation) {
            visibleIds.add(node.id);
        }
    }

    return {
        nodes: graph.nodes.filter(node => visibleIds.has(node.id)),
        edges: graph.edges.filter(edge => visibleIds.has(edge.from) && visibleIds.has(edge.to))
    };
}

function buildCommitFocus(detail, graph) {
    const diff = detail?.diff ?? {};
    if (!graph || !diff) {
        return null;
    }

    const path = detail?.cognitivePath ?? {};
    const focusGoalIds = Array.isArray(path.goalIds) ? path.goalIds : [];
    const focusSubGoalIds = Array.isArray(path.subGoalIds) ? path.subGoalIds : [];

    const addedIds = new Set();
    collectDiffNodeIds(diff, addedIds, {
        changeTypes: ["Added"],
        excludeTypes: ["Evidence", "Project", "Run", "ContextPacket"]
    });

    const changedIds = new Set(addedIds);
    if (changedIds.size === 0) {
        collectPathNodeIds(detail?.cognitivePath, changedIds, { strict: true });
    }

    if (changedIds.size === 0) {
        collectDiffNodeIds(diff, changedIds, {
            excludeTypes: ["Evidence", "Project", "Run", "ContextPacket"]
        });
    }

    if (changedIds.size === 0) {
        return null;
    }

    return buildCommitFocusFromIds(changedIds, graph, {
        goalIds: focusGoalIds,
        subGoalIds: focusSubGoalIds
    });
}

function collectDiffNodeIds(diff, changedIds, options = {}) {
    const changeTypes = Array.isArray(options.changeTypes)
        ? new Set(options.changeTypes.map(value => String(value).toLowerCase()))
        : null;
    const excludeTypes = Array.isArray(options.excludeTypes)
        ? new Set(options.excludeTypes.map(value => String(value)))
        : null;
    const changeGroups = [
        diff.tasks,
        diff.hypotheses,
        diff.evidence,
        diff.decisions,
        diff.conclusions
    ];

    for (const group of changeGroups) {
        if (!Array.isArray(group)) {
            continue;
        }

        for (const change of group) {
            if (!change || typeof change !== "object") {
                continue;
            }

            if (changeTypes && !changeTypes.has(String(change.changeType ?? "").toLowerCase())) {
                continue;
            }

            const entityType = normalizeGraphEntityType(change.entityType);
            const entityId = change.entityId;
            if (!entityType || !entityId) {
                continue;
            }
            if (excludeTypes && excludeTypes.has(entityType)) {
                continue;
            }

            changedIds.add(`${entityType}:${entityId}`);
        }
    }
}

function buildCommitFocusFromIds(changedIds, graph, focusInfo = {}) {
    const adjacency = new Map();
    const nodeMap = new Map();
    for (const node of graph.nodes) {
        adjacency.set(node.id, []);
        nodeMap.set(node.id, node);
    }
    for (const edge of graph.edges ?? []) {
        adjacency.get(edge.from)?.push({ id: edge.to, edge });
        adjacency.get(edge.to)?.push({ id: edge.from, edge });
    }

    const focusIds = new Set(changedIds);
    const queue = Array.from(changedIds, id => ({ id, depth: 0 }));
    const visited = new Set(changedIds);
    const maxDepth = Number.isFinite(focusInfo.maxDepth) ? focusInfo.maxDepth : 3;

    while (queue.length > 0) {
        const current = queue.shift();
        if (!current || current.depth >= maxDepth) {
            continue;
        }

        const currentNode = nodeMap.get(current.id);
        if (!currentNode || currentNode.type === "Goal" || currentNode.type === "Project") {
            continue;
        }

        for (const entry of adjacency.get(current.id) ?? []) {
            const neighborNode = nodeMap.get(entry.id);
            if (!neighborNode || !shouldIncludeCommitFocusNeighbor(currentNode, neighborNode)) {
                continue;
            }

            focusIds.add(entry.id);
            if (visited.has(entry.id)) {
                continue;
            }

            visited.add(entry.id);
            if (neighborNode.type !== "Goal" && neighborNode.type !== "Project") {
                queue.push({ id: entry.id, depth: current.depth + 1 });
            }
        }
    }

    return {
        nodeIds: focusIds,
        changedIds,
        goalIds: Array.isArray(focusInfo.goalIds) ? focusInfo.goalIds : [],
        subGoalIds: Array.isArray(focusInfo.subGoalIds) ? focusInfo.subGoalIds : []
    };
}

function shouldIncludeCommitFocusNeighbor(currentNode, neighborNode) {
    if (!currentNode || !neighborNode) {
        return false;
    }

    if (neighborNode.type === "Project" || neighborNode.type === "Evidence") {
        return false;
    }

    return true;
}

function collectPathNodeIds(cognitivePath, ids = new Set(), options = {}) {
    const path = cognitivePath ?? {};
    const strict = Boolean(options.strict);
    const goalIds = Array.isArray(path.goalIds) ? path.goalIds : [];
    const subGoalIds = Array.isArray(path.subGoalIds) ? path.subGoalIds : [];
    const taskIds = Array.isArray(path.taskIds) ? path.taskIds : [];
    const hypothesisIds = Array.isArray(path.hypothesisIds) ? path.hypothesisIds : [];
    const decisionIds = Array.isArray(path.decisionIds) ? path.decisionIds : [];
    const conclusionIds = Array.isArray(path.conclusionIds) ? path.conclusionIds : [];
    if (strict) {
        if (goalIds[0]) {
            ids.add(`Goal:${goalIds[0]}`);
        }
        if (subGoalIds[0]) {
            ids.add(`Goal:${subGoalIds[0]}`);
        }
        if (taskIds[0]) {
            ids.add(`Task:${taskIds[0]}`);
        }
        if (hypothesisIds[0]) {
            ids.add(`Hypothesis:${hypothesisIds[0]}`);
        }
        if (decisionIds[0]) {
            ids.add(`Decision:${decisionIds[0]}`);
        }
        if (!decisionIds[0] && conclusionIds[0]) {
            ids.add(`Conclusion:${conclusionIds[0]}`);
        }
        return ids;
    }

    const pathGroups = [
        ["Goal", goalIds],
        ["Goal", subGoalIds],
        ["Task", taskIds],
        ["Hypothesis", hypothesisIds],
        ["Decision", decisionIds],
        ["Conclusion", conclusionIds]
    ];

    for (const [type, values] of pathGroups) {
        if (!Array.isArray(values)) {
            continue;
        }

        for (const value of values) {
            if (!value) {
                continue;
            }

            const normalized = normalizeGraphEntityType(type);
            ids.add(`${normalized}:${value}`);
        }
    }

    return ids;
}

function filterGraphByCommitFocus(graph, commitFocus) {
    if (!commitFocus || commitFocus.nodeIds.size === 0) {
        return graph;
    }

    const nodes = graph.nodes.filter(node => commitFocus.nodeIds.has(node.id));
    const edges = graph.edges.filter(edge => commitFocus.nodeIds.has(edge.from) && commitFocus.nodeIds.has(edge.to));
    return { nodes, edges };
}

function normalizeGraphEntityType(entityType) {
    if (!entityType) {
        return null;
    }

    const normalized = entityType.trim();
    if (!normalized) {
        return null;
    }

    switch (normalized.toLowerCase()) {
        case "project":
            return "Project";
        case "goal":
            return "Goal";
        case "task":
            return "Task";
        case "hypothesis":
        case "hypo":
            return "Hypothesis";
        case "decision":
            return "Decision";
        case "evidence":
            return "Evidence";
        case "conclusion":
            return "Conclusion";
        case "run":
            return "Run";
        case "contextpacket":
        case "context-packet":
            return "ContextPacket";
        default:
            return normalized;
    }
}

function ensureAllTaskStateFiltersChecked() {
    for (const input of taskStateFilters) {
        input.checked = true;
    }

    currentGraphPreset = "all";
    currentGraphFocusModes = new Set(["all"]);
    updatePresetButtons();
    persistGraphFocusSelection();
}

function sortGraphNodes(nodes) {
    if (!currentCommitFocus) {
        nodes.sort((left, right) => left.label.localeCompare(right.label, undefined, { sensitivity: "base" }));
        return;
    }

    const changed = currentCommitFocus.changedIds;
    nodes.sort((left, right) => {
        const leftChanged = changed.has(left.id) ? 0 : 1;
        const rightChanged = changed.has(right.id) ? 0 : 1;
        if (leftChanged !== rightChanged) {
            return leftChanged - rightChanged;
        }
        return left.label.localeCompare(right.label, undefined, { sensitivity: "base" });
    });
}

function buildCommitFocusCaption() {
    if (!currentCommitFocusEnabled || !currentCommitFocus) {
        return "";
    }

    const changedCount = currentCommitFocus.changedIds.size;
    const totalCount = currentCommitFocus.nodeIds.size;
    return `Commit focus: ${changedCount} path nodes, ${totalCount} visible in focus. `;
}

function toggleGraphFocusMode(presetName) {
    if (transientGraphFocusSnapshot) {
        transientGraphFocusSnapshot = null;
    }

    if (presetName === "all") {
        currentGraphFocusModes = new Set(["all"]);
    } else {
        if (currentGraphFocusModes.has("all")) {
            currentGraphFocusModes.delete("all");
        }

        if (currentGraphFocusModes.has(presetName)) {
            currentGraphFocusModes.delete(presetName);
        } else if (graphPresets[presetName]) {
            currentGraphFocusModes.add(presetName);
        }

        if (currentGraphFocusModes.size === 0) {
            currentGraphFocusModes = new Set(["all"]);
        }
    }

    syncPresetFromFilters();
    updatePresetButtons();
    persistGraphFocusSelection();

    if (currentGraph) {
        selectedNodeId = null;
        renderGraph(currentGraph);
        nodeDetail.textContent = "Click a node.";
    } else {
        renderGraphFocusCaption();
    }
}

function snapshotGraphFocus() {
    return {
        focusModes: new Set(currentGraphFocusModes),
        taskStates: taskStateFilters.map(input => ({
            state: input.dataset.taskFilter,
            checked: input.checked
        })),
        preset: currentGraphPreset
    };
}

function restoreGraphFocus(snapshot) {
    currentGraphFocusModes = new Set(snapshot.focusModes ?? []);
    for (const input of taskStateFilters) {
        const match = snapshot.taskStates?.find(item => item.state === input.dataset.taskFilter);
        input.checked = match ? match.checked : input.checked;
    }
    currentGraphPreset = snapshot.preset ?? currentGraphPreset;
    syncPresetFromFilters();
    updatePresetButtons();
    persistGraphFocusSelection();
}

function applyWorkingContextFiltersTransient() {
    if (!transientGraphFocusSnapshot) {
        transientGraphFocusSnapshot = snapshotGraphFocus();
    }

    suppressGraphFocusPersist = true;
    currentGraphFocusModes = new Set(["working", "thinking"]);

    const allowedStates = new Set(["Draft", "Ready", "InProgress", "Blocked"]);
    for (const input of taskStateFilters) {
        input.checked = allowedStates.has(input.dataset.taskFilter);
    }

    syncPresetFromFilters();
    updatePresetButtons();
    suppressGraphFocusPersist = false;

    if (currentGraph) {
        selectedNodeId = null;
        renderGraph(currentGraph);
        nodeDetail.textContent = "Click a node.";
    } else {
        renderGraphFocusCaption();
    }
}

function resolveEffectiveTaskStates() {
    const manualStates = taskStateFilters
        .filter(input => input.checked)
        .map(input => input.dataset.taskFilter);

    if (manualStates.length === 0) {
        return [];
    }

    if (currentGraphFocusModes.has("all")) {
        return manualStates;
    }

    const focusStates = new Set();
    for (const focusMode of currentGraphFocusModes) {
        for (const state of graphPresets[focusMode] ?? []) {
            focusStates.add(state);
        }
    }

    return manualStates.filter(state => focusStates.has(state));
}

function syncPresetFromFilters() {
    if (currentGraphFocusModes.has("all")) {
        const selectedStates = taskStateFilters
            .filter(input => input.checked)
            .map(input => input.dataset.taskFilter)
            .sort()
            .join("|");
        const allStates = graphPresets.all.slice().sort().join("|");
        currentGraphPreset = selectedStates === allStates ? "all" : "custom";
        return;
    }

    const focusModes = Array.from(currentGraphFocusModes).sort().join("|");
    const matchingPreset = Object.keys(graphPresets)
        .filter(name => name !== "all")
        .sort()
        .join("|");

    currentGraphPreset = focusModes === matchingPreset ? "all" : "custom";
    updatePresetButtons();
}

function updatePresetButtons() {
    for (const button of graphPresetButtons) {
        const presetName = button.dataset.graphPreset;
        const isActive = presetName === "all"
            ? currentGraphFocusModes.has("all")
            : currentGraphFocusModes.has(presetName);
        button.classList.toggle("active", isActive);
    }
}

function renderGraphFocusCaption(filteredGraph) {
    let presetLabel = "Focus: custom task-state mix.";
    if (currentGraphFocusModes.has("all")) {
        presetLabel = "Focus: all threads.";
    } else {
        const labels = [];
        if (currentGraphFocusModes.has("working")) {
            labels.push("Working");
        }
        if (currentGraphFocusModes.has("thinking")) {
            labels.push("Thinking");
        }
        if (currentGraphFocusModes.has("closed")) {
            labels.push("Closed");
        }

        if (labels.length > 0) {
            presetLabel = `Focus: ${labels.join(" + ")}.`;
        }
    }

    const effectiveStates = resolveEffectiveTaskStates();
    const stateLabel = effectiveStates.length > 0
        ? `States: ${effectiveStates.join(", ")}.`
        : "States: none selected.";

    if (!filteredGraph) {
        graphFocusCaption.textContent = `${buildCommitFocusCaption()}${presetLabel} ${stateLabel}`.trim();
        return;
    }

    const visibleTasks = filteredGraph.nodes.filter(node => node.type === "Task").length;
    graphFocusCaption.textContent = `${buildCommitFocusCaption()}${presetLabel} ${stateLabel} Visible tasks: ${visibleTasks}.`.trim();
}

function syncAutoRefresh() {
    if (!autoRefreshToggle.checked || !repoPathInput.value.trim()) {
        stopAutoRefresh();
        return;
    }

    stopAutoRefresh();
    autoRefreshHandle = window.setInterval(async () => {
        if (!repoPathInput.value.trim()) {
            stopAutoRefresh();
            renderFreshnessStatus();
            return;
        }

        try {
            await loadOverview(branchSelect.value || undefined);
        } catch (error) {
            viewerHint.textContent = `Auto-refresh failed: ${error?.message ?? error}`;
            stopAutoRefresh();
            renderFreshnessStatus("Auto-refresh paused after failure");
        }
    }, autoRefreshIntervalMs);
}

function stopAutoRefresh() {
    if (autoRefreshHandle !== null) {
        window.clearInterval(autoRefreshHandle);
        autoRefreshHandle = null;
    }
}

function renderFreshnessStatus(overrideText) {
    if (overrideText) {
        freshnessStatus.textContent = overrideText;
        return;
    }

    if (!autoRefreshToggle.checked) {
        freshnessStatus.textContent = "Auto-refresh off";
        return;
    }

    const loadedText = lastLoadedAt
        ? `Auto-refresh on (${Math.round(autoRefreshIntervalMs / 1000)}s) · last sync ${lastLoadedAt.toLocaleTimeString()}`
        : `Auto-refresh on (${Math.round(autoRefreshIntervalMs / 1000)}s)`;
    freshnessStatus.textContent = loadedText;
}

function escapeHtml(value) {
    value ??= "";
    return value
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;");
}

function buildLaneMap(overview) {
    const laneMap = new Map();
    overview.branches
        .map(branch => branch.name)
        .sort((left, right) => left.localeCompare(right))
        .forEach((branchName, index) => laneMap.set(branchName, index));

    return laneMap;
}

function laneColor(index) {
    return lanePalette[(index ?? 0) % lanePalette.length];
}

function renderLaneGrid(laneCount, activeLaneIndex, headLaneIndexes, isWorking = false) {
    const headSet = new Set(headLaneIndexes ?? []);
    const cells = Array.from({ length: laneCount }, (_, index) => {
        const classes = ["lane-cell"];
        if (activeLaneIndex === index) {
            classes.push("active");
        }
        if (headSet.has(index)) {
            classes.push("head");
        }
        if (isWorking) {
            classes.push("working");
        }

        return `<span class="${classes.join(" ")}" style="--branch-color:${laneColor(index)}"><span class="lane-rail"></span><span class="lane-dot"></span></span>`;
    }).join("");

    return `<div class="lane-grid" style="--lane-count:${laneCount}">${cells}</div>`;
}

function renderWorkingDetail() {
    commitDetail.innerHTML = `
        <p class="detail-empty">Working context for <strong>${escapeHtml(currentOverview.selectedBranch)}</strong>.</p>
        <div class="detail-grid">
            <div class="detail-card">
                <span class="detail-card-label">State</span>
                <span class="detail-card-value">Uncommitted changes view</span>
            </div>
            <div class="detail-card">
                <span class="detail-card-label">Repository</span>
                <span class="detail-card-value">${escapeHtml(currentOverview.repositoryPath)}</span>
            </div>
            <div class="detail-card">
                <span class="detail-card-label">Model</span>
                <span class="detail-card-value">Not recorded</span>
            </div>
        </div>`;
}

function renderCommitDetail(detail) {
    const diff = detail.diff ?? {};
    const totalChanges =
        (diff.decisions?.length ?? 0)
        + (diff.hypotheses?.length ?? 0)
        + (diff.evidence?.length ?? 0)
        + (diff.tasks?.length ?? 0)
        + (diff.conclusions?.length ?? 0)
        + (diff.conflicts?.length ?? 0);
    const sections = [
        ["Decisions", diff.decisions],
        ["Hypotheses", diff.hypotheses],
        ["Evidence", diff.evidence],
        ["Tasks", diff.tasks],
        ["Conclusions", diff.conclusions],
        ["Conflicts", diff.conflicts]
    ];
    const pathMeta = buildCognitivePathMeta(detail.cognitivePath);

    commitDetail.innerHTML = `
        <div class="detail-header">
            <div>
                <h4 class="detail-title">${escapeHtml(detail.message)}</h4>
                <p class="detail-subtitle">${escapeHtml(detail.id)}</p>
            </div>
            <span class="branch-chip" style="--branch-color:${laneColor(buildLaneMap(currentOverview).get(detail.branch))}">${escapeHtml(detail.branch)}</span>
        </div>
        <div class="detail-grid">
            <div class="detail-card">
                <span class="detail-card-label">Created</span>
                <span class="detail-card-value">${new Date(detail.createdAtUtc).toLocaleString()}</span>
            </div>
            <div class="detail-card">
                <span class="detail-card-label">Author</span>
                <span class="detail-card-value">${escapeHtml(detail.author ?? "unknown")}</span>
            </div>
            <div class="detail-card">
                <span class="detail-card-label">Model</span>
                <span class="detail-card-value">${escapeHtml(formatModelIdentity(detail.modelName, detail.modelVersion))}</span>
            </div>
            <div class="detail-card">
                <span class="detail-card-label">Commit</span>
                <span class="detail-card-value">${escapeHtml(detail.id.slice(0, 8))}</span>
            </div>
            <div class="detail-card">
                <span class="detail-card-label">Snapshot</span>
                <span class="detail-card-value">${escapeHtml(detail.snapshotHash)}</span>
            </div>
            <div class="detail-card">
                <span class="detail-card-label">Parents</span>
                <div class="detail-pill-row" id="parent-pills"></div>
            </div>
            <div class="detail-card">
                <span class="detail-card-label">Changes</span>
                <span class="detail-card-value">${escapeHtml(String(totalChanges))}</span>
            </div>
            <div class="detail-card">
                <span class="detail-card-label">Summary</span>
                <span class="detail-card-value">${escapeHtml(diff.summary ?? "No diff summary")}</span>
            </div>
        </div>
        ${renderCognitivePathSection(pathMeta)}
        ${sections.map(([label, items]) => renderDiffSection(label, items)).join("")}
    `;

    const parentContainer = commitDetail.querySelector("#parent-pills");
    const parentIds = detail.parentIds ?? [];
    if (parentIds.length === 0) {
        parentContainer.innerHTML = `<span class="detail-count">Root commit</span>`;
    } else {
        for (const parentId of parentIds) {
            const button = document.createElement("button");
            button.className = "detail-pill";
            button.textContent = parentId.slice(0, 8);
            button.onclick = () => selectCommit(parentId);
            parentContainer.appendChild(button);
        }
    }
}

function buildCognitivePathMeta(cognitivePath) {
    const path = cognitivePath ?? {};
    const goalTitles = Array.isArray(path.goalTitles) ? path.goalTitles.filter(Boolean) : [];
    const subGoalTitles = Array.isArray(path.subGoalTitles) ? path.subGoalTitles.filter(Boolean) : [];
    const taskTitles = Array.isArray(path.taskTitles) ? path.taskTitles.filter(Boolean) : [];
    const hypothesisTitles = Array.isArray(path.hypothesisTitles) ? path.hypothesisTitles.filter(Boolean) : [];
    const decisionTitles = Array.isArray(path.decisionTitles) ? path.decisionTitles.filter(Boolean) : [];
    const conclusionSummaries = Array.isArray(path.conclusionSummaries) ? path.conclusionSummaries.filter(Boolean) : [];
    const segments = [];

    if (goalTitles[0]) {
        segments.push({ label: "Goal", value: summarizePathValue(goalTitles) });
    }

    if (subGoalTitles[0]) {
        segments.push({ label: "Sub-goal", value: summarizePathValue(subGoalTitles) });
    }

    if (taskTitles[0]) {
        segments.push({ label: "Task", value: summarizePathValue(taskTitles) });
    }

    if (hypothesisTitles[0]) {
        segments.push({ label: "Hypothesis", value: summarizePathValue(hypothesisTitles) });
    }

    if (decisionTitles[0]) {
        segments.push({ label: "Decision", value: summarizePathValue(decisionTitles) });
    } else if (conclusionSummaries[0]) {
        segments.push({ label: "Conclusion", value: summarizePathValue(conclusionSummaries) });
    }

    return {
        goal: goalTitles[0] ?? null,
        goalTitles,
        subGoalTitles,
        taskTitles,
        hypothesisTitles,
        decisionTitles,
        conclusionSummaries,
        segments,
        compactSummary: segments.map(segment => `${segment.label}: ${segment.value}`).join(" -> ")
    };
}

function summarizePathValue(values) {
    const list = Array.isArray(values) ? values.filter(Boolean) : [];
    if (list.length === 0) {
        return "";
    }

    if (list.length === 1) {
        return list[0];
    }

    return `${list[0]} +${list.length - 1}`;
}

function renderCognitivePathSection(pathMeta) {
    const groups = [
        ["Goals", pathMeta.goalTitles],
        ["Sub-goals", pathMeta.subGoalTitles],
        ["Tasks", pathMeta.taskTitles],
        ["Hypotheses", pathMeta.hypothesisTitles],
        ["Decisions", pathMeta.decisionTitles],
        ["Conclusions", pathMeta.conclusionSummaries]
    ].filter(([, items]) => Array.isArray(items) && items.length > 0);

    if (groups.length === 0) {
        return "";
    }

    return `
        <section class="detail-section">
            <h4>Cognitive Path</h4>
            <div class="detail-path-stack">
                ${groups.map(([label, items]) => `
                    <div class="detail-path-group">
                        <span class="detail-path-label">${escapeHtml(label)}</span>
                        <div class="detail-pill-row">
                            ${items.map(item => `<span class="detail-pill static">${escapeHtml(item)}</span>`).join("")}
                        </div>
                    </div>
                `).join("")}
            </div>
        </section>`;
}

function renderDiffSection(label, items) {
    const list = Array.isArray(items) ? items : [];
    if (list.length === 0) {
        return "";
    }

    const entries = list
        .map(item => item && typeof item === "object"
            ? `${item.title ? `${item.title}: ` : ""}${item.summary ?? JSON.stringify(item)}`
            : String(item))
        .map(item => `<li>${escapeHtml(item)}</li>`)
        .join("");

    if (label === "Evidence") {
        return `
            <section class="detail-section">
                <details class="detail-disclosure">
                    <summary>Evidence <span class="detail-count">${list.length}</span></summary>
                    <ul class="detail-list">
                        ${entries}
                    </ul>
                </details>
            </section>`;
    }

    return `
        <section class="detail-section">
            <h4>${escapeHtml(label)} <span class="detail-count">${list.length}</span></h4>
            <ul class="detail-list">
                ${entries}
            </ul>
        </section>`;
}

function formatModelIdentity(modelName, modelVersion) {
    if (modelName && modelVersion) {
        return `${modelName} (${modelVersion})`;
    }

    if (modelName) {
        return modelName;
    }

    if (modelVersion) {
        return modelVersion;
    }

    return "not recorded";
}
