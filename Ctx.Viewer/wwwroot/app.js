const repoForm = document.getElementById("repo-form");
const repoPathInput = document.getElementById("repo-path");
const branchSelect = document.getElementById("branch-select");
const refreshButton = document.getElementById("refresh-button");
const autoRefreshToggle = document.getElementById("auto-refresh-toggle");
const topbarVersion = document.getElementById("topbar-version");
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
const originDetail = document.getElementById("origin-detail");
const playbookDetail = document.getElementById("playbook-detail");
const leftTabButtons = Array.from(document.querySelectorAll("[data-left-tab]"));
const leftTabPanels = Array.from(document.querySelectorAll("[data-left-panel]"));
const leftRailTabButtons = Array.from(document.querySelectorAll("[data-left-rail-tab]"));
const detailTabButtons = Array.from(document.querySelectorAll("[data-detail-tab]"));
const detailTabPanels = Array.from(document.querySelectorAll("[data-detail-panel]"));
const detailRailTabButtons = Array.from(document.querySelectorAll("[data-detail-rail-tab]"));
const viewerHint = document.getElementById("viewer-hint");
const taskStateFilters = Array.from(document.querySelectorAll("[data-task-filter]"));
const graphPresetButtons = Array.from(document.querySelectorAll("[data-graph-preset]"));
const layout = document.querySelector(".layout");
const panelDividers = Array.from(document.querySelectorAll(".panel-divider"));
const panelCollapseButtons = Array.from(document.querySelectorAll("[data-toggle-panel]"));
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
const leftTabStorageKey = "ctx-viewer-left-tab";
const detailTabStorageKey = "ctx-viewer-detail-tab";
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
let overviewRequestSequence = 0;
let selectionRequestSequence = 0;
let currentViewMode = loadStoredViewMode();
let currentGraphPreset = "all";
let currentGraphFocusModes = new Set(["all"]);
let currentHistoryBranchFilters = new Set();
let currentHistorySort = loadStoredHistorySort();
let currentPanelLayout = loadStoredPanelLayout();
let currentCommitFocus = null;
let currentCommitFocusEnabled = loadStoredCommitFocus();
let currentLeftTab = loadStoredLeftTab();
let currentDetailTab = loadStoredDetailTab();
let transientGraphFocusSnapshot = null;
let suppressGraphFocusPersist = false;
let playbookRequestSequence = 0;
let originRequestSequence = 0;

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
        renderGraph(currentGraph, { preserveViewport: true });
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
            renderGraph(currentGraph, { preserveViewport: true });
        }
    });
}

window.addEventListener("load", async () => {
    applyViewMode(currentViewMode);
    applyLeftTab(currentLeftTab);
    applyDetailTab(currentDetailTab);
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

for (const button of leftTabButtons) {
    button.addEventListener("click", () => {
        applyLeftTab(button.dataset.leftTab);
    });
}

for (const button of leftRailTabButtons) {
    button.addEventListener("click", () => {
        applyLeftTab(button.dataset.leftRailTab);
        expandPanel("left");
    });
}

for (const button of detailTabButtons) {
    button.addEventListener("click", () => {
        applyDetailTab(button.dataset.detailTab);
    });
}

for (const button of detailRailTabButtons) {
    button.addEventListener("click", () => {
        applyDetailTab(button.dataset.detailRailTab);
        expandPanel("right");
    });
}

async function loadOverview(branch) {
    const requestSequence = ++overviewRequestSequence;
    const previousOverview = currentOverview;
    const requestStartedCommitId = selectedCommitId;
    const requestStartedNodeId = selectedNodeId;
    const repositoryPath = repoPathInput.value.trim();
    const params = new URLSearchParams();
    if (repositoryPath) {
        params.set("path", repositoryPath);
    }
    if (branch) {
        params.set("branch", branch);
    }

    const response = await fetch(`/api/overview?${params.toString()}`);
    if (requestSequence !== overviewRequestSequence) {
        return;
    }

    if (!response.ok) {
        const error = await response.json();
        if (requestSequence !== overviewRequestSequence) {
            return;
        }
        resetViewer(error.message ?? "Failed to load repository.");
        return;
    }

    const overview = await response.json();
    if (requestSequence !== overviewRequestSequence) {
        return;
    }

    currentOverview = overview;
    lastLoadedAt = new Date();
    restoreHistoryBranchFilters(overview);
    repoPathInput.value = overview.repositoryPath;
    persistRepositoryPath(overview.repositoryPath);
    persistBranchSelection(overview.selectedBranch);
    if (topbarVersion) {
        topbarVersion.textContent = overview.productVersion ? `v${overview.productVersion}` : "";
    }
    viewerHint.textContent = `Loaded ${overview.repositoryPath}`;
    currentHypothesisRanking = await loadHypothesisRanking();
    if (requestSequence !== overviewRequestSequence) {
        return;
    }

    renderFreshnessStatus();

    renderBranchSelect(overview);
    renderSummary(overview);
    renderHypothesisRanking(currentHypothesisRanking);
    renderTasks(overview);
    renderCommits(overview);

    const currentSelectedCommitId = selectedCommitId;
    const currentSelectedNodeId = selectedNodeId;
    const effectiveSelectedCommitId = currentSelectedCommitId !== requestStartedCommitId
        ? currentSelectedCommitId
        : requestStartedCommitId;
    const effectiveSelectedNodeId = currentSelectedCommitId !== requestStartedCommitId
        ? currentSelectedNodeId
        : requestStartedNodeId;
    const commitStillExists = effectiveSelectedCommitId
        && overview.timelineCommits.some(commit => commit.id === effectiveSelectedCommitId);
    const preferredCommit =
        commitStillExists ? effectiveSelectedCommitId :
        previousOverview && effectiveSelectedCommitId === null ? null :
        overview.timelineCommits[0]?.id ?? null;
    const preferredNodeId = effectiveSelectedCommitId === preferredCommit ? effectiveSelectedNodeId : null;
    const preserveViewport = previousOverview && effectiveSelectedCommitId === preferredCommit;

    await selectCommit(preferredCommit, {
        preferredNodeId,
        scrollSelectedNodeIntoView: false,
        preserveViewport
    });
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
        right: Number.isFinite(value.right) ? value.right : fallback.right,
        leftCollapsed: typeof value.leftCollapsed === "boolean" ? value.leftCollapsed : fallback.leftCollapsed,
        rightCollapsed: typeof value.rightCollapsed === "boolean" ? value.rightCollapsed : fallback.rightCollapsed
    };
}

function getDefaultPanelLayout() {
    return {
        history: { left: 920, right: 360, leftCollapsed: false, rightCollapsed: false },
        split: { left: 320, right: 360, leftCollapsed: false, rightCollapsed: false },
        graph: { left: 280, right: 320, leftCollapsed: false, rightCollapsed: false }
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

function loadStoredLeftTab() {
    const stored = window.localStorage.getItem(leftTabStorageKey);
    return stored === "tasks" ? "tasks" : "history";
}

function persistLeftTab() {
    window.localStorage.setItem(leftTabStorageKey, currentLeftTab);
}

function applyLeftTab(tab) {
    currentLeftTab = tab === "tasks" ? "tasks" : "history";

    for (const button of leftTabButtons) {
        const active = button.dataset.leftTab === currentLeftTab;
        button.classList.toggle("active", active);
        button.setAttribute("aria-selected", active ? "true" : "false");
    }

    for (const button of leftRailTabButtons) {
        const active = button.dataset.leftRailTab === currentLeftTab;
        button.classList.toggle("active", active);
        button.setAttribute("aria-selected", active ? "true" : "false");
    }

    for (const panel of leftTabPanels) {
        const active = panel.dataset.leftPanel === currentLeftTab;
        panel.classList.toggle("active", active);
        panel.hidden = !active;
    }

    persistLeftTab();
}

function loadStoredDetailTab() {
    const stored = window.localStorage.getItem(detailTabStorageKey);
    return stored === "origin" || stored === "playbook" || stored === "hypotheses" ? stored : "details";
}

function persistDetailTab() {
    window.localStorage.setItem(detailTabStorageKey, currentDetailTab);
}

function applyDetailTab(tab) {
    currentDetailTab = tab === "origin" || tab === "playbook" || tab === "hypotheses" ? tab : "details";

    for (const button of detailTabButtons) {
        const active = button.dataset.detailTab === currentDetailTab;
        button.classList.toggle("active", active);
        button.setAttribute("aria-selected", active ? "true" : "false");
    }

    for (const button of detailRailTabButtons) {
        const active = button.dataset.detailRailTab === currentDetailTab;
        button.classList.toggle("active", active);
        button.setAttribute("aria-selected", active ? "true" : "false");
    }

    for (const panel of detailTabPanels) {
        const active = panel.dataset.detailPanel === currentDetailTab;
        panel.classList.toggle("active", active);
        panel.hidden = !active;
    }

    persistDetailTab();
}

function applyPanelLayout() {
    if (!layout) {
        return;
    }

    const mode = currentViewMode;
    const panelLayout = getPanelLayoutForMode(mode);
    const clamped = clampPanelWidths(mode, panelLayout.left, panelLayout.right);
    currentPanelLayout[mode] = {
        ...clamped,
        leftCollapsed: panelLayout.leftCollapsed ?? false,
        rightCollapsed: panelLayout.rightCollapsed ?? false
    };
    const constraints = getPanelConstraints(mode);
    const leftCollapsed = currentPanelLayout[mode].leftCollapsed;
    const rightCollapsed = currentPanelLayout[mode].rightCollapsed;
    const collapsedWidth = 56;
    layout.style.setProperty("--panel-left-width", `${leftCollapsed ? collapsedWidth : clamped.left}px`);
    layout.style.setProperty("--panel-right-width", `${rightCollapsed ? collapsedWidth : clamped.right}px`);
    layout.style.setProperty("--panel-left-min", `${leftCollapsed ? collapsedWidth : constraints.leftMin}px`);
    layout.style.setProperty("--panel-right-min", `${rightCollapsed ? collapsedWidth : constraints.rightMin}px`);
    layout.dataset.leftCollapsed = leftCollapsed ? "true" : "false";
    layout.dataset.rightCollapsed = rightCollapsed ? "true" : "false";
    syncPanelCollapseButtons();
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

    const constraints = getPanelConstraints(mode);

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

function getPanelConstraints(mode) {
    return {
        history: { leftMin: 560, leftMax: 1400, rightMin: 280, rightMax: 640, centerMin: 0 },
        split: { leftMin: 260, leftMax: 720, rightMin: 280, rightMax: 640, centerMin: 360 },
        graph: { leftMin: 220, leftMax: 560, rightMin: 260, rightMax: 520, centerMin: 420 }
    }[mode] ?? { leftMin: 260, leftMax: 720, rightMin: 280, rightMax: 640, centerMin: 360 };
}

function attachPanelDividerHandlers() {
    if (!layout || panelDividers.length === 0) {
        return;
    }

    for (const divider of panelDividers) {
        divider.addEventListener("pointerdown", event => {
            if (event.target instanceof Element && event.target.closest(".panel-collapse-toggle")) {
                console.debug("[ctx-viewer] collapse button pointerdown bypassed divider drag", {
                    side: divider.dataset.divider
                });
                return;
            }

            if (window.matchMedia("(max-width: 1200px)").matches) {
                return;
            }

            event.preventDefault();
            const dividerSide = divider.dataset.divider;
            const activeMode = currentViewMode;
            const panelLayout = getPanelLayoutForMode(activeMode);
            if ((dividerSide === "left" && panelLayout.leftCollapsed) || (dividerSide === "right" && panelLayout.rightCollapsed)) {
                return;
            }
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

for (const button of panelCollapseButtons) {
    button.addEventListener("click", event => {
        event.preventDefault();
        event.stopPropagation();
        console.debug("[ctx-viewer] collapse button click", {
            side: button.dataset.togglePanel,
            mode: currentViewMode
        });
        togglePanelCollapse(button.dataset.togglePanel);
    });
}

function togglePanelCollapse(side) {
    const panelLayout = getPanelLayoutForMode(currentViewMode);
    if (side === "left") {
        panelLayout.leftCollapsed = !panelLayout.leftCollapsed;
    } else if (side === "right") {
        panelLayout.rightCollapsed = !panelLayout.rightCollapsed;
    }

    console.debug("[ctx-viewer] togglePanelCollapse", {
        side,
        leftCollapsed: panelLayout.leftCollapsed,
        rightCollapsed: panelLayout.rightCollapsed,
        mode: currentViewMode
    });
    applyPanelLayout();
}

function expandPanel(side) {
    const panelLayout = getPanelLayoutForMode(currentViewMode);
    if (side === "left" && panelLayout.leftCollapsed) {
        panelLayout.leftCollapsed = false;
    } else if (side === "right" && panelLayout.rightCollapsed) {
        panelLayout.rightCollapsed = false;
    } else {
        return;
    }

    applyPanelLayout();
}

function syncPanelCollapseButtons() {
    const panelLayout = getPanelLayoutForMode(currentViewMode);
    for (const button of panelCollapseButtons) {
        const side = button.dataset.togglePanel;
        const collapsed = side === "left"
            ? panelLayout.leftCollapsed
            : panelLayout.rightCollapsed;
        button.textContent = side === "left"
            ? (collapsed ? "›" : "‹")
            : (collapsed ? "‹" : "›");
        button.setAttribute("aria-label", `${collapsed ? "Expand" : "Collapse"} ${side} panel`);
        button.title = `${collapsed ? "Expand" : "Collapse"} ${side} panel`;
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
    const historyScrollSnapshot = captureHistoryPanelScroll();
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
        restoreHistoryPanelScroll(historyScrollSnapshot);
        return;
    }

    renderCompactHistoryNavigator(overview, orderedBranches, groupedCommits, lanes, laneCount);
    restoreHistoryPanelScroll(historyScrollSnapshot);
}

function captureHistoryPanelScroll() {
    return {
        branchList: captureElementScroll(".history-branch-list"),
        rows: captureElementScroll(".history-rows"),
        compactRows: captureElementScroll(".history-compact-rows")
    };
}

function captureElementScroll(selector) {
    const element = commitList.querySelector(selector);
    if (!element) {
        return null;
    }

    return {
        top: element.scrollTop,
        left: element.scrollLeft
    };
}

function restoreHistoryPanelScroll(snapshot) {
    if (!snapshot) {
        return;
    }

    restoreElementScroll(".history-branch-list", snapshot.branchList);
    restoreElementScroll(".history-rows", snapshot.rows);
    restoreElementScroll(".history-compact-rows", snapshot.compactRows);
}

function restoreElementScroll(selector, snapshot) {
    if (!snapshot) {
        return;
    }

    const element = commitList.querySelector(selector);
    if (!element) {
        return;
    }

    element.scrollTop = snapshot.top;
    element.scrollLeft = snapshot.left;
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

async function selectCommit(commitId, options = {}) {
    if (!currentOverview) {
        return;
    }

    const requestSequence = ++selectionRequestSequence;

    const preferredNodeId = options.preferredNodeId ?? null;
    const scrollSelectedNodeIntoView = options.scrollSelectedNodeIntoView ?? false;
    const preserveViewport = options.preserveViewport ?? false;

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
    if (requestSequence !== selectionRequestSequence) {
        return;
    }

    if (!graphResponse.ok) {
        const error = await graphResponse.json();
        if (requestSequence !== selectionRequestSequence) {
            return;
        }
        graphCanvas.innerHTML = "";
        graphCaption.textContent = "Graph unavailable";
        nodeDetail.textContent = JSON.stringify(error, null, 2);
        return;
    }

    currentGraph = await graphResponse.json();
    if (requestSequence !== selectionRequestSequence) {
        return;
    }

    selectedNodeId = preferredNodeId && currentGraph.nodes.some(node => node.id === preferredNodeId)
        ? preferredNodeId
        : null;

    if (commitId) {
        const detailResponse = await fetch(`/api/commit?id=${encodeURIComponent(commitId)}&path=${encodeURIComponent(repoPathInput.value)}`);
        if (requestSequence !== selectionRequestSequence) {
            return;
        }
        const detail = await detailResponse.json();
        if (requestSequence !== selectionRequestSequence) {
            return;
        }
        currentCommitFocus = buildCommitFocus(detail, currentGraph);
        if (!selectedNodeId) {
            selectedNodeId = resolveCommitFocusSelectedNodeId(detail, currentGraph, currentCommitFocus);
        }
        ensureAllTaskStateFiltersChecked();
        renderCommitDetail(detail);
        graphCaption.textContent = `Commit ${commitId.slice(0, 8)}`;
    } else {
        renderWorkingDetail();
        graphCaption.textContent = "Working context";
    }

    renderGraph(currentGraph, { preserveViewport });
    renderTasks(currentOverview);
    await loadOriginDetail();
    await loadPlaybookDetail();

    if (selectedNodeId) {
        renderSelectedNodeDetail(selectedNodeId, { scrollIntoView: scrollSelectedNodeIntoView });
    } else {
        nodeDetail.textContent = "Click a node.";
    }
}

function captureGraphViewport() {
    return {
        left: graphCanvas.scrollLeft,
        top: graphCanvas.scrollTop
    };
}

function restoreGraphViewport(viewport) {
    if (!viewport) {
        return;
    }

    graphCanvas.scrollLeft = viewport.left;
    graphCanvas.scrollTop = viewport.top;
}

function resetGraphViewport() {
    graphCanvas.scrollLeft = 0;
    graphCanvas.scrollTop = 0;
}

function renderGraph(graph, options = {}) {
    const preserveViewport = options.preserveViewport ?? false;
    const viewportSnapshot = preserveViewport ? captureGraphViewport() : null;
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
        if (preserveViewport) {
            restoreGraphViewport(viewportSnapshot);
        } else {
            resetGraphViewport();
        }
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
    const columnGap = 182;
    const nodeVerticalGap = 2;
    const topLaneBaseY = 48;
    let columnIndex = 0;
    let maxColumnBottom = 0;
    const selectedPathState = buildSelectedPathState(filteredGraph, selectedNodeId);

    for (const [type, nodes] of grouped.entries()) {
        if (nodes.length === 0 && !(type === "Sub-goal" && hasSubGoalNodes)) {
            continue;
        }

        sortGraphNodes(nodes);
        const column = document.createElement("div");
        column.className = "graph-column";
        column.style.left = `${columnIndex * columnGap + 10}px`;
        column.innerHTML = `<h4>${type}</h4>`;
        stage.appendChild(column);
        let columnY = 64;

        nodes.forEach((node) => {
            const y = columnY;
            const inlineBadge = buildGraphNodeBadge(node);
            const displayType = node.type === "Goal" && subGoalIds.has(node.id) ? "Sub-goal" : node.type;

            const card = document.createElement("button");
            const commitFocusClass = currentCommitFocus ? "commit-focus" : "";
            const commitChangedClass = currentCommitFocus?.changedIds.has(node.id) ? "commit-changed" : "";
            const nodePathClass = buildSelectedNodePathClass(selectedPathState, node.id);
            card.className = `graph-node ${commitFocusClass} ${commitChangedClass} ${selectedNodeId === node.id ? "active" : ""} ${nodePathClass}`;
            card.style.left = `${columnIndex * columnGap + 10}px`;
            card.style.top = `${y}px`;
            card.innerHTML = `<small>${displayType}</small><strong>${escapeHtml(node.label)}</strong>${inlineBadge}`;
            card.onclick = () => showNode(node.id, { scrollIntoView: false });
            stage.appendChild(card);

            const cardHeight = card.offsetHeight || 56;
            positions.set(node.id, {
                x: columnIndex * columnGap + 10,
                y,
                width: 160,
                height: cardHeight,
                centerY: y + cardHeight / 2,
                columnIndex
            });
            columnY += cardHeight + nodeVerticalGap;
        });

        maxColumnBottom = Math.max(maxColumnBottom, columnY);

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

    const stageWidth = Math.max(960, columnIndex * columnGap + 220);
    const stageHeight = Math.max(720, maxColumnBottom + 90);
    stage.style.width = `${stageWidth}px`;
    stage.style.height = `${stageHeight}px`;
    svg.setAttribute("viewBox", `0 0 ${stageWidth} ${stageHeight}`);
    const edgePlans = buildGraphEdgePlans(filteredGraph.edges, positions, { topLaneBaseY });

    filteredGraph.edges.forEach((edge, edgeIndex) => {
        const from = positions.get(edge.from);
        const to = positions.get(edge.to);
        if (!from || !to) {
            return;
        }

        const line = document.createElementNS("http://www.w3.org/2000/svg", "path");
        const sourceAnchor = resolveGraphNodeAnchor(from, to.centerY, "outgoing");
        const targetAnchor = resolveGraphNodeAnchor(to, from.centerY, "incoming");
        line.setAttribute("d", buildGraphEdgePath(sourceAnchor, targetAnchor, edgePlans[edgeIndex]));
        line.setAttribute("fill", "none");
        line.setAttribute("stroke", "#91a9ce");
        line.setAttribute("stroke-width", "2");
        line.setAttribute("stroke-linecap", "round");
        line.setAttribute("stroke-linejoin", "round");
        line.classList.add("graph-edge");

        const edgeKey = `${edge.from}->${edge.to}`;
        if (selectedPathState.hasSelection) {
            if (selectedPathState.sharedEdges.has(edgeKey)) {
                line.classList.add("selected-shared");
            } else if (selectedPathState.outgoingEdges.has(edgeKey)) {
                line.classList.add("selected-outgoing");
            } else if (selectedPathState.incomingEdges.has(edgeKey)) {
                line.classList.add("selected-incoming");
            } else {
                line.classList.add("edge-dimmed");
            }
        }

        if (currentCommitFocus?.nodeIds?.has(edge.from) && currentCommitFocus?.nodeIds?.has(edge.to)) {
            line.classList.add("commit-path");
        }
        if (currentCommitFocus?.changedIds?.has(edge.from) && currentCommitFocus?.changedIds?.has(edge.to)) {
            line.classList.add("commit-path-changed");
        }
        svg.appendChild(line);
    });

    if (preserveViewport) {
        restoreGraphViewport(viewportSnapshot);
    } else {
        resetGraphViewport();
    }
}

function resolveGraphNodeAnchor(nodeBox, targetCenterY, direction) {
    const width = nodeBox.width ?? 160;
    const centerY = nodeBox.centerY ?? (nodeBox.y + (nodeBox.height ?? 56) / 2);
    const deltaY = (targetCenterY ?? centerY) - centerY;
    const biasLimit = Math.max(8, Math.min(20, (nodeBox.height ?? 56) * 0.22));
    const biasedY = centerY + Math.max(-biasLimit, Math.min(biasLimit, deltaY * 0.35));

    return {
        x: direction === "outgoing" ? nodeBox.x + width : nodeBox.x,
        y: Math.round(biasedY * 10) / 10
    };
}

function buildSelectedPathState(graph, selectedId) {
    if (!selectedId || !graph?.nodes?.some(node => node.id === selectedId)) {
        return {
            hasSelection: false,
            incomingNodes: new Set(),
            outgoingNodes: new Set(),
            sharedNodes: new Set(),
            incomingEdges: new Set(),
            outgoingEdges: new Set(),
            sharedEdges: new Set()
        };
    }

    const incomingAdjacency = new Map();
    const outgoingAdjacency = new Map();
    graph.edges.forEach((edge) => {
        if (!outgoingAdjacency.has(edge.from)) {
            outgoingAdjacency.set(edge.from, []);
        }
        outgoingAdjacency.get(edge.from).push(edge);

        if (!incomingAdjacency.has(edge.to)) {
            incomingAdjacency.set(edge.to, []);
        }
        incomingAdjacency.get(edge.to).push(edge);
    });

    const incomingNodes = new Set([selectedId]);
    const outgoingNodes = new Set([selectedId]);
    const incomingEdges = new Set();
    const outgoingEdges = new Set();

    walkSelectedPath(selectedId, incomingAdjacency, "incoming", incomingNodes, incomingEdges);
    walkSelectedPath(selectedId, outgoingAdjacency, "outgoing", outgoingNodes, outgoingEdges);

    const sharedNodes = new Set(
        [...incomingNodes].filter(nodeId => outgoingNodes.has(nodeId) && nodeId !== selectedId)
    );
    const sharedEdges = new Set(
        [...incomingEdges].filter(edgeKey => outgoingEdges.has(edgeKey))
    );

    return {
        hasSelection: true,
        incomingNodes,
        outgoingNodes,
        sharedNodes,
        incomingEdges,
        outgoingEdges,
        sharedEdges
    };
}

function walkSelectedPath(startNodeId, adjacency, direction, nodeSet, edgeSet) {
    const queue = [startNodeId];
    const visited = new Set([startNodeId]);

    while (queue.length > 0) {
        const currentNodeId = queue.shift();
        const edges = adjacency.get(currentNodeId) ?? [];
        edges.forEach((edge) => {
            const edgeKey = `${edge.from}->${edge.to}`;
            edgeSet.add(edgeKey);

            const nextNodeId = direction === "incoming" ? edge.from : edge.to;
            nodeSet.add(nextNodeId);
            if (!visited.has(nextNodeId)) {
                visited.add(nextNodeId);
                queue.push(nextNodeId);
            }
        });
    }
}

function buildSelectedNodePathClass(pathState, nodeId) {
    if (!pathState?.hasSelection || nodeId === selectedNodeId) {
        return "";
    }

    if (pathState.sharedNodes.has(nodeId)) {
        return "path-shared";
    }

    if (pathState.outgoingNodes.has(nodeId)) {
        return "path-outgoing";
    }

    if (pathState.incomingNodes.has(nodeId)) {
        return "path-incoming";
    }

    return "path-dimmed";
}

function buildGraphEdgePlans(edges, positions, layout = {}) {
    const plans = edges.map((edge) => {
        const from = positions.get(edge.from);
        const to = positions.get(edge.to);
        const spanColumns = Math.max(0, (to?.columnIndex ?? 0) - (from?.columnIndex ?? 0));
        const verticalDelta = Math.abs((to?.centerY ?? 0) - (from?.centerY ?? 0));
        return {
            mode: spanColumns >= 2 && verticalDelta >= 120 ? "top-lane" : "direct",
            spanColumns,
            verticalDelta,
            laneIndex: 0,
            from,
            to
        };
    });

    const grouped = new Map();
    plans.forEach((plan, index) => {
        if (plan.mode !== "top-lane" || !plan.from || !plan.to) {
            return;
        }

        const key = `${plan.from.columnIndex}->${plan.to.columnIndex}`;
        if (!grouped.has(key)) {
            grouped.set(key, []);
        }

        grouped.get(key).push({ index, sortY: Math.min(plan.from.centerY, plan.to.centerY) });
    });

    for (const group of grouped.values()) {
        group.sort((left, right) => left.sortY - right.sortY);
        group.forEach((entry, laneIndex) => {
            plans[entry.index].laneIndex = laneIndex;
            plans[entry.index].laneY = Math.max(10, (layout.topLaneBaseY ?? 36) - laneIndex * 8);
        });
    }

    return plans;
}

function buildGraphEdgePath(sourceAnchor, targetAnchor, plan) {
    if (plan?.mode === "top-lane") {
        return buildSeparatedTopLaneEdgePath(sourceAnchor, targetAnchor, plan);
    }

    const gap = Math.max(8, targetAnchor.x - sourceAnchor.x);
    const stub = Math.max(2, Math.min(8, gap * 0.28));
    const control = Math.max(4, Math.min(20, gap * 0.42));
    const sourceStubX = sourceAnchor.x + stub;
    const targetStubX = targetAnchor.x - stub;
    const sourceControlX = sourceStubX + control;
    const targetControlX = targetStubX - control;

    return [
        `M ${sourceAnchor.x} ${sourceAnchor.y}`,
        `L ${sourceStubX} ${sourceAnchor.y}`,
        `C ${sourceControlX} ${sourceAnchor.y}, ${targetControlX} ${targetAnchor.y}, ${targetStubX} ${targetAnchor.y}`,
        `L ${targetAnchor.x} ${targetAnchor.y}`
    ].join(" ");
}

function buildSeparatedTopLaneEdgePath(sourceAnchor, targetAnchor, plan) {
    const laneIndex = plan?.laneIndex ?? 0;
    const laneY = plan?.laneY ?? 36;
    const sourceLaneInset = 10 + laneIndex * 10;
    const targetLaneInset = 10 + laneIndex * 10;
    const sourceLiftY = sourceAnchor.y - (8 + laneIndex * 5);
    const targetDropY = targetAnchor.y - (8 + laneIndex * 5);
    const sourceLaneX = sourceAnchor.x + sourceLaneInset;
    const targetLaneX = targetAnchor.x - targetLaneInset;

    return buildRoundedPolylinePath([
        { x: sourceAnchor.x, y: sourceAnchor.y },
        { x: sourceLaneX, y: sourceAnchor.y },
        { x: sourceLaneX, y: sourceLiftY },
        { x: sourceLaneX, y: laneY },
        { x: targetLaneX, y: laneY },
        { x: targetLaneX, y: targetDropY },
        { x: targetLaneX, y: targetAnchor.y },
        { x: targetAnchor.x, y: targetAnchor.y }
    ], 10);
}

function buildRoundedPolylinePath(points, radius) {
    if (!Array.isArray(points) || points.length < 2) {
        return "";
    }

    const effectiveRadius = Math.max(0, radius ?? 0);
    const commands = [`M ${points[0].x} ${points[0].y}`];

    for (let index = 1; index < points.length - 1; index += 1) {
        const previous = points[index - 1];
        const current = points[index];
        const next = points[index + 1];
        const incomingLength = Math.hypot(current.x - previous.x, current.y - previous.y);
        const outgoingLength = Math.hypot(next.x - current.x, next.y - current.y);

        if (incomingLength === 0 || outgoingLength === 0 || effectiveRadius === 0) {
            commands.push(`L ${current.x} ${current.y}`);
            continue;
        }

        const cornerRadius = Math.min(effectiveRadius, incomingLength / 2, outgoingLength / 2);
        const entryX = current.x - ((current.x - previous.x) / incomingLength) * cornerRadius;
        const entryY = current.y - ((current.y - previous.y) / incomingLength) * cornerRadius;
        const exitX = current.x + ((next.x - current.x) / outgoingLength) * cornerRadius;
        const exitY = current.y + ((next.y - current.y) / outgoingLength) * cornerRadius;

        commands.push(`L ${roundGraphCoordinate(entryX)} ${roundGraphCoordinate(entryY)}`);
        commands.push(`Q ${current.x} ${current.y}, ${roundGraphCoordinate(exitX)} ${roundGraphCoordinate(exitY)}`);
    }

    const lastPoint = points[points.length - 1];
    commands.push(`L ${lastPoint.x} ${lastPoint.y}`);
    return commands.join(" ");
}

function roundGraphCoordinate(value) {
    return Math.round(value * 10) / 10;
}

function buildGraphNodeBadge(node) {
    if (node.type === "Hypothesis" && node.metadata?.score) {
        return `<span class="node-score">score ${escapeHtml(node.metadata.score)}</span>`;
    }

    const state = normalizeGraphNodeState(node.state);
    if (!state) {
        return "";
    }

    if (node.type === "Task") {
        return `<span class="node-badge node-badge-task">${escapeHtml(state)}</span>`;
    }

    if (node.type === "Decision") {
        return `<span class="node-badge node-badge-decision">${escapeHtml(state)}</span>`;
    }

    if (node.type === "Conclusion") {
        return `<span class="node-badge node-badge-conclusion">${escapeHtml(state)}</span>`;
    }

    return "";
}

function normalizeGraphNodeState(state) {
    if (!state) {
        return "";
    }

    return String(state)
        .replace(/([a-z])([A-Z])/g, "$1 $2")
        .trim();
}

function renderSelectedNodeDetail(nodeId, options = {}) {
    const scrollIntoView = options.scrollIntoView ?? false;
    const graph = currentRenderedGraph ?? currentGraph;
    if (!graph) {
        nodeDetail.textContent = "Click a node.";
        return;
    }

    const node = graph.nodes.find(item => item.id === nodeId);
    if (!node) {
        nodeDetail.textContent = "Selected node is no longer visible with the current filters.";
        return;
    }

    const incoming = graph.edges.filter(edge => edge.to === nodeId);
    const outgoing = graph.edges.filter(edge => edge.from === nodeId);
    const connectedNodeIds = [...new Set([...incoming.map(edge => edge.from), ...outgoing.map(edge => edge.to)])];
    const connectedNodes = graph.nodes.filter(nodeItem => connectedNodeIds.includes(nodeItem.id));

    nodeDetail.textContent = JSON.stringify({ node, incoming, outgoing, connectedNodes }, null, 2);

    if (scrollIntoView) {
        const activeNode = graphCanvas.querySelector(".graph-node.active");
        activeNode?.scrollIntoView({ block: "center", inline: "center", behavior: "smooth" });
    }
}

function showNode(nodeId, options = {}) {
    selectedNodeId = nodeId;
    renderGraph(currentGraph, { preserveViewport: true });
    if (currentOverview) {
        renderTasks(currentOverview);
    }

    renderSelectedNodeDetail(nodeId, options);
    void loadOriginDetail();
    void loadPlaybookDetail();
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

    showNode(`Task:${task.id}`, { scrollIntoView: true });
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
            renderGraph(currentGraph, { preserveViewport: true });
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
    playbookDetail.innerHTML = `<p class="detail-empty">Select a task, goal, or commit focus to view operational runbooks.</p>`;
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

    let selectedTaskIds = new Set(
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
    const taskGoalIds = new Map();
    const subGoalParentIds = new Map();

    for (const edge of graph.edges) {
        if (edge.relationship === "contains" && edge.from?.startsWith("Goal:") && edge.to?.startsWith("Task:")) {
            taskGoalIds.set(edge.to, edge.from);
        }

        if (edge.relationship === "subgoal" && edge.from?.startsWith("Goal:") && edge.to?.startsWith("Goal:")) {
            subGoalParentIds.set(edge.to, edge.from);
        }
    }

    const focusedTaskId = resolveWorkingFocusTaskId(graph, selectedTaskIds, taskGoalIds);
    if (focusedTaskId && selectedTaskIds.has(focusedTaskId) && !selectedCommitId && !currentCommitFocus) {
        selectedTaskIds = new Set([focusedTaskId]);
    }

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
    const directGoalIds = new Set();
    const parentGoalIds = new Set();
    let hasSelectedSubGoal = false;

    for (const taskId of selectedTaskIds) {
        const directGoalId = taskGoalIds.get(taskId);
        if (!directGoalId) {
            continue;
        }

        directGoalIds.add(directGoalId);
        if (subGoalParentIds.has(directGoalId)) {
            hasSelectedSubGoal = true;
            parentGoalIds.add(subGoalParentIds.get(directGoalId));
        }
    }

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
            if (hasSelectedSubGoal) {
                if (directGoalIds.has(node.id) || parentGoalIds.has(node.id)) {
                    visibleIds.add(node.id);
                }
            } else if (hasSelectedAssociation) {
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

function resolveWorkingFocusTaskId(graph, selectedTaskIds, taskGoalIds) {
    if (!(selectedTaskIds instanceof Set) || selectedTaskIds.size === 0) {
        return null;
    }

    if (!currentGraphFocusModes.has("working")) {
        return null;
    }

    const selectedNodeTaskId = resolveFocusedTaskIdFromSelectedNode(graph, selectedTaskIds);
    if (selectedNodeTaskId) {
        return selectedNodeTaskId;
    }

    const preferredOverviewTask = (currentOverview?.tasks ?? []).find(task => task.state === "InProgress")
        ?? (currentOverview?.tasks ?? []).find(task => task.state === "Ready")
        ?? (currentOverview?.tasks ?? []).find(task => task.state !== "Done");

    if (preferredOverviewTask) {
        const preferredTaskNodeId = `Task:${preferredOverviewTask.id}`;
        if (selectedTaskIds.has(preferredTaskNodeId)) {
            return preferredTaskNodeId;
        }
    }

    const orderedTasks = Array.from(selectedTaskIds).sort((left, right) => {
        const leftGoal = taskGoalIds.get(left) ?? "";
        const rightGoal = taskGoalIds.get(right) ?? "";
        if (leftGoal !== rightGoal) {
            return leftGoal.localeCompare(rightGoal);
        }

        return left.localeCompare(right);
    });

    return orderedTasks[0] ?? null;
}

function resolveFocusedTaskIdFromSelectedNode(graph, selectedTaskIds) {
    if (!selectedNodeId) {
        return null;
    }

    if (selectedNodeId.startsWith("Task:") && selectedTaskIds.has(selectedNodeId)) {
        return selectedNodeId;
    }

    const adjacency = new Map();
    for (const node of graph.nodes ?? []) {
        adjacency.set(node.id, []);
    }

    for (const edge of graph.edges ?? []) {
        adjacency.get(edge.from)?.push(edge.to);
        adjacency.get(edge.to)?.push(edge.from);
    }

    const queue = [selectedNodeId];
    const visited = new Set([selectedNodeId]);

    while (queue.length > 0) {
        const currentId = queue.shift();
        for (const neighborId of adjacency.get(currentId) ?? []) {
            if (visited.has(neighborId)) {
                continue;
            }

            if (neighborId.startsWith("Task:") && selectedTaskIds.has(neighborId)) {
                return neighborId;
            }

            visited.add(neighborId);
            queue.push(neighborId);
        }
    }

    return null;
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

function resolveCommitFocusSelectedNodeId(detail, graph, commitFocus) {
    if (!graph?.nodes?.length) {
        return null;
    }

    const availableNodeIds = new Set(graph.nodes.map(node => node.id));
    const candidates = [];
    collectPathNodeIds(detail?.cognitivePath, candidates, { strict: true, output: "array" });

    const preferredTypes = ["Task", "Hypothesis", "Decision", "Conclusion", "Goal"];
    for (const type of preferredTypes) {
        const candidate = candidates.find(nodeId => nodeId.startsWith(`${type}:`) && availableNodeIds.has(nodeId));
        if (candidate) {
            return candidate;
        }
    }

    const changedIds = Array.from(commitFocus?.changedIds ?? []);
    for (const type of preferredTypes) {
        const candidate = changedIds.find(nodeId => nodeId.startsWith(`${type}:`) && availableNodeIds.has(nodeId));
        if (candidate) {
            return candidate;
        }
    }

    return graph.nodes.find(node => availableNodeIds.has(node.id))?.id ?? null;
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
    const output = options.output === "array" ? [] : ids;
    const goalIds = Array.isArray(path.goalIds) ? path.goalIds : [];
    const subGoalIds = Array.isArray(path.subGoalIds) ? path.subGoalIds : [];
    const taskIds = Array.isArray(path.taskIds) ? path.taskIds : [];
    const hypothesisIds = Array.isArray(path.hypothesisIds) ? path.hypothesisIds : [];
    const decisionIds = Array.isArray(path.decisionIds) ? path.decisionIds : [];
    const conclusionIds = Array.isArray(path.conclusionIds) ? path.conclusionIds : [];
    if (strict) {
        if (goalIds[0]) {
            pushPathNodeId(output, `Goal:${goalIds[0]}`);
        }
        if (subGoalIds[0]) {
            pushPathNodeId(output, `Goal:${subGoalIds[0]}`);
        }
        if (taskIds[0]) {
            pushPathNodeId(output, `Task:${taskIds[0]}`);
        }
        if (hypothesisIds[0]) {
            pushPathNodeId(output, `Hypothesis:${hypothesisIds[0]}`);
        }
        if (decisionIds[0]) {
            pushPathNodeId(output, `Decision:${decisionIds[0]}`);
        }
        if (!decisionIds[0] && conclusionIds[0]) {
            pushPathNodeId(output, `Conclusion:${conclusionIds[0]}`);
        }
        return output;
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
            pushPathNodeId(output, `${normalized}:${value}`);
        }
    }

    return output;
}

function pushPathNodeId(output, nodeId) {
    if (Array.isArray(output)) {
        output.push(nodeId);
        return;
    }

    output.add(nodeId);
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
        renderGraph(currentGraph, { preserveViewport: true });
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
        renderGraph(currentGraph, { preserveViewport: true });
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

function formatDateTime(value) {
    if (!value) {
        return "";
    }

    const date = new Date(value);
    return Number.isNaN(date.getTime()) ? String(value) : date.toLocaleString();
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

async function loadPlaybookDetail() {
    const graph = currentRenderedGraph ?? currentGraph;
    if (!currentOverview || !graph) {
        playbookDetail.innerHTML = `<p class="detail-empty">Select a task, goal, or commit focus to view operational runbooks.</p>`;
        return;
    }

    const focus = resolvePlaybookFocus(graph);
    const params = new URLSearchParams({ path: repoPathInput.value });
    if (focus.goalId) {
        params.set("goalId", focus.goalId);
    }
    if (focus.taskId) {
        params.set("taskId", focus.taskId);
    }
    params.set("purpose", focus.purpose);

    const requestSequence = ++playbookRequestSequence;
    const response = await fetch(`/api/playbook?${params.toString()}`);
    if (requestSequence !== playbookRequestSequence) {
        return;
    }

    if (!response.ok) {
        playbookDetail.innerHTML = `<p class="detail-empty">Playbook unavailable for the current focus.</p>`;
        return;
    }

    const payload = await response.json();
    if (requestSequence !== playbookRequestSequence) {
        return;
    }

    renderPlaybookDetail(payload);
}

async function loadOriginDetail() {
    const graph = currentRenderedGraph ?? currentGraph;
    if (!currentOverview || !graph) {
        originDetail.innerHTML = `<p class="detail-empty">Select a task, goal, or commit focus to view the origin of this cognitive line.</p>`;
        return;
    }

    const focus = resolvePlaybookFocus(graph);
    const params = new URLSearchParams({ path: repoPathInput.value });
    if (focus.goalId) {
        params.set("goalId", focus.goalId);
    }
    if (focus.taskId) {
        params.set("taskId", focus.taskId);
    }
    params.set("purpose", focus.purpose);

    const requestSequence = ++originRequestSequence;
    const response = await fetch(`/api/origin?${params.toString()}`);
    if (requestSequence !== originRequestSequence) {
        return;
    }

    if (!response.ok) {
        originDetail.innerHTML = `<p class="detail-empty">Origin unavailable for the current focus.</p>`;
        return;
    }

    const payload = await response.json();
    if (requestSequence !== originRequestSequence) {
        return;
    }

    renderOriginDetail(payload);
}

function resolvePlaybookFocus(graph) {
    const selectedNode = selectedNodeId
        ? graph.nodes.find(node => node.id === selectedNodeId) ?? null
        : null;

    if (selectedNode) {
        const nodeFocus = resolvePlaybookFocusFromNode(graph, selectedNode);
        if (nodeFocus) {
            return nodeFocus;
        }
    }

    if (selectedCommitId && currentCommitFocus) {
        if (Array.isArray(currentCommitFocus.taskIds) && currentCommitFocus.taskIds[0]) {
            return {
                taskId: currentCommitFocus.taskIds[0],
                goalId: currentCommitFocus.goalIds?.[0] ?? null,
                purpose: `viewer commit focus ${currentCommitFocus.taskTitles?.[0] ?? selectedCommitId}`
            };
        }

        if (Array.isArray(currentCommitFocus.goalIds) && currentCommitFocus.goalIds[0]) {
            return {
                taskId: null,
                goalId: currentCommitFocus.goalIds[0],
                purpose: `viewer commit focus ${currentCommitFocus.goalTitles?.[0] ?? selectedCommitId}`
            };
        }
    }

    const activeTask = (currentOverview.tasks ?? []).find(task => task.state === "InProgress")
        ?? (currentOverview.tasks ?? []).find(task => task.state === "Ready")
        ?? (currentOverview.tasks ?? []).find(task => task.state !== "Done");

    if (activeTask) {
        return {
            taskId: activeTask.id,
            goalId: activeTask.goalId ?? null,
            purpose: `viewer working context ${activeTask.title}`
        };
    }

    return {
        taskId: null,
        goalId: null,
        purpose: "viewer working context"
    };
}

function resolvePlaybookFocusFromNode(graph, selectedNode) {
    const [type, entityId] = selectedNode.id.split(":");
    if (!type || !entityId) {
        return null;
    }

    if (type === "Task") {
        const task = (currentOverview?.tasks ?? []).find(item => item.id === entityId);
        return {
            taskId: entityId,
            goalId: task?.goalId ?? null,
            purpose: `viewer node task ${selectedNode.label}`
        };
    }

    if (type === "Goal") {
        return {
            taskId: null,
            goalId: entityId,
            purpose: `viewer node goal ${selectedNode.label}`
        };
    }

    const nearestTaskId = findNearestNodeIdByType(graph, selectedNode.id, "Task");
    if (nearestTaskId) {
        const taskId = nearestTaskId.split(":")[1];
        const task = (currentOverview?.tasks ?? []).find(item => item.id === taskId);
        return {
            taskId,
            goalId: task?.goalId ?? null,
            purpose: `viewer node ${selectedNode.type} ${selectedNode.label}`
        };
    }

    const nearestGoalId = findNearestNodeIdByType(graph, selectedNode.id, "Goal");
    if (nearestGoalId) {
        return {
            taskId: null,
            goalId: nearestGoalId.split(":")[1],
            purpose: `viewer node ${selectedNode.type} ${selectedNode.label}`
        };
    }

    return null;
}

function findNearestNodeIdByType(graph, startNodeId, nodeType) {
    const adjacency = new Map();
    for (const node of graph.nodes ?? []) {
        adjacency.set(node.id, []);
    }

    for (const edge of graph.edges ?? []) {
        adjacency.get(edge.from)?.push(edge.to);
        adjacency.get(edge.to)?.push(edge.from);
    }

    const queue = [startNodeId];
    const visited = new Set([startNodeId]);

    while (queue.length > 0) {
        const currentNodeId = queue.shift();
        const currentNode = (graph.nodes ?? []).find(node => node.id === currentNodeId);
        if (currentNode && currentNode.type === nodeType && currentNode.id !== startNodeId) {
            return currentNode.id;
        }

        for (const neighborId of adjacency.get(currentNodeId) ?? []) {
            if (visited.has(neighborId)) {
                continue;
            }

            visited.add(neighborId);
            queue.push(neighborId);
        }
    }

    return null;
}

function renderPlaybookDetail(payload) {
    const selected = Array.isArray(payload?.selected) ? payload.selected : [];
    const available = Array.isArray(payload?.available) ? payload.available : [];

    if (selected.length === 0) {
        playbookDetail.innerHTML = `
            <p class="detail-empty">No operational runbooks match the current focus.</p>
            ${available.length > 0 ? `
                <details class="detail-disclosure runbook-available">
                    <summary>Available runbooks <span class="detail-count">${available.length}</span></summary>
                    <ul class="detail-list">
                        ${available.map(runbook => `<li>${escapeHtml(runbook.title)} <span class="detail-count">${escapeHtml(runbook.kind)}</span></li>`).join("")}
                    </ul>
                </details>` : ""}
        `;
        return;
    }

    playbookDetail.innerHTML = `
        <div class="runbook-stack">
            ${selected.map(runbook => `
                <article class="runbook-card">
                    <div class="runbook-card-header">
                        <strong>${escapeHtml(runbook.title)}</strong>
                        <span class="runbook-kind">${escapeHtml(runbook.kind)}</span>
                    </div>
                    <div class="runbook-meta">
                        <div class="runbook-meta-row">
                            <span class="runbook-meta-row-label">When</span>
                            <div>${escapeHtml(runbook.whenToUse)}</div>
                        </div>
                        ${Array.isArray(runbook.do) && runbook.do.length > 0 ? `
                            <div class="runbook-meta-row">
                                <span class="runbook-meta-row-label">Do</span>
                                <ul>${runbook.do.map(item => `<li>${escapeHtml(item)}</li>`).join("")}</ul>
                            </div>` : ""}
                        ${Array.isArray(runbook.verify) && runbook.verify.length > 0 ? `
                            <div class="runbook-meta-row">
                                <span class="runbook-meta-row-label">Verify</span>
                                <ul>${runbook.verify.map(item => `<li>${escapeHtml(item)}</li>`).join("")}</ul>
                            </div>` : ""}
                        ${Array.isArray(runbook.references) && runbook.references.length > 0 ? `
                            <div class="runbook-meta-row">
                                <span class="runbook-meta-row-label">References</span>
                                <ul>${runbook.references.map(item => `<li><code>${escapeHtml(item)}</code></li>`).join("")}</ul>
                            </div>` : ""}
                    </div>
                </article>
            `).join("")}
        </div>
        ${available.length > 0 ? `
            <details class="detail-disclosure runbook-available">
                <summary>Additional runbooks available <span class="detail-count">${available.length}</span></summary>
                <ul class="detail-list">
                    ${available.map(runbook => `<li>${escapeHtml(runbook.title)} <span class="detail-count">${escapeHtml(runbook.kind)}</span></li>`).join("")}
                </ul>
            </details>` : ""}
    `;
}

function renderOriginDetail(payload) {
    const selected = Array.isArray(payload?.selected) ? payload.selected : [];
    const available = Array.isArray(payload?.available) ? payload.available : [];

    if (selected.length === 0) {
        originDetail.innerHTML = `
            <p class="detail-empty">No cognitive origin matches the current focus.</p>
            ${available.length > 0 ? `
                <details class="detail-disclosure runbook-available">
                    <summary>Additional origins available <span class="detail-count">${available.length}</span></summary>
                    <ul class="detail-list">
                        ${available.map(trigger => `<li>${escapeHtml(trigger.summary)} <span class="detail-count">${escapeHtml(trigger.kind)}</span></li>`).join("")}
                    </ul>
                </details>` : ""}
        `;
        return;
    }

    originDetail.innerHTML = `
        <div class="runbook-stack">
            ${selected.map(trigger => `
                <article class="runbook-card">
                    <div class="runbook-card-header">
                        <strong>${escapeHtml(trigger.summary)}</strong>
                        <div class="origin-card-badges">
                            <span class="runbook-kind">${escapeHtml(trigger.kind)}</span>
                            <span class="origin-resolution ${trigger.resolution === "inherited" ? "inherited" : "direct"}">${trigger.resolution === "inherited" ? "Inherited" : "Direct"}</span>
                        </div>
                    </div>
                    <p class="origin-resolution-hint">${trigger.resolution === "inherited"
                        ? "Inherited from the nearest matching cognitive line."
                        : "Directly attached to the current focus."}</p>
                    <div class="origin-meta-strip">
                        ${trigger.createdBy ? `<span>${escapeHtml(trigger.createdBy)}</span>` : ""}
                        ${trigger.createdAtUtc ? `<span>${escapeHtml(formatDateTime(trigger.createdAtUtc))}</span>` : ""}
                    </div>
                    <div class="runbook-meta">
                        ${trigger.text ? `
                            <div class="runbook-meta-row">
                                <span class="runbook-meta-row-label">Text</span>
                                <div>${escapeHtml(trigger.text)}</div>
                            </div>` : ""}
                        ${Array.isArray(trigger.goalTitles) && trigger.goalTitles.length > 0 ? `
                            <div class="runbook-meta-row">
                                <span class="runbook-meta-row-label">Goals</span>
                                <div class="detail-pill-row">
                                    ${trigger.goalTitles.map((item, index) => `
                                        <button type="button" class="detail-pill origin-link" data-origin-goal-id="${escapeHtml(trigger.goalIds?.[index] ?? "")}">
                                            ${escapeHtml(item)}
                                        </button>`).join("")}
                                </div>
                            </div>` : ""}
                        ${Array.isArray(trigger.taskTitles) && trigger.taskTitles.length > 0 ? `
                            <div class="runbook-meta-row">
                                <span class="runbook-meta-row-label">Tasks</span>
                                <div class="detail-pill-row">
                                    ${trigger.taskTitles.map((item, index) => `
                                        <button type="button" class="detail-pill origin-link" data-origin-task-id="${escapeHtml(trigger.taskIds?.[index] ?? "")}">
                                            ${escapeHtml(item)}
                                        </button>`).join("")}
                                </div>
                            </div>` : ""}
                        ${Array.isArray(trigger.runbookTitles) && trigger.runbookTitles.length > 0 ? `
                            <div class="runbook-meta-row">
                                <span class="runbook-meta-row-label">Runbooks</span>
                                <div class="detail-pill-row">
                                    ${trigger.runbookTitles.map((item, index) => `
                                        <button type="button" class="detail-pill origin-link" data-origin-runbook-id="${escapeHtml(trigger.runbookIds?.[index] ?? "")}">
                                            ${escapeHtml(item)}
                                        </button>`).join("")}
                                </div>
                            </div>` : ""}
                    </div>
                </article>
            `).join("")}
        </div>
        ${available.length > 0 ? `
            <details class="detail-disclosure runbook-available">
                <summary>Additional origins available <span class="detail-count">${available.length}</span></summary>
                <ul class="detail-list">
                    ${available.map(trigger => `<li>${escapeHtml(trigger.summary)} <span class="detail-count">${escapeHtml(trigger.kind)}</span></li>`).join("")}
                </ul>
            </details>` : ""}
    `;

    for (const button of originDetail.querySelectorAll("[data-origin-task-id]")) {
        button.addEventListener("click", async () => {
            const taskId = button.dataset.originTaskId;
            const task = (currentOverview?.tasks ?? []).find(item => item.id === taskId);
            if (task) {
                await focusTask(task);
                applyDetailTab("origin");
            }
        });
    }

    for (const button of originDetail.querySelectorAll("[data-origin-goal-id]")) {
        button.addEventListener("click", async () => {
            const goalId = button.dataset.originGoalId;
            if (!goalId || !currentRenderedGraph) {
                return;
            }

            const goalNode = (currentRenderedGraph.nodes ?? []).find(node => node.id === `Goal:${goalId}`);
            if (!goalNode) {
                return;
            }

            if (selectedCommitId !== null) {
                await selectCommit(null);
            }

            if (currentViewMode === "history") {
                applyViewMode("split");
            }

            showNode(goalNode.id, { scrollIntoView: true });
            applyDetailTab("origin");
        });
    }

    for (const button of originDetail.querySelectorAll("[data-origin-runbook-id]")) {
        button.addEventListener("click", () => {
            applyDetailTab("playbook");
        });
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
