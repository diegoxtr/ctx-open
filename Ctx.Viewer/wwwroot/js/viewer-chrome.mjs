export function createViewerChromeLayoutController({
    document,
    window,
    elements,
    isTopbarCollapsed,
    onLayoutChange
}) {
    let resizeObserver = null;

    function update() {
        if (window.matchMedia("(max-width: 900px)").matches) {
            removeChromeVariable("--viewer-topbar-height");
            removeChromeVariable("--viewer-projects-height");
            removeChromeVariable("--viewer-view-toolbar-height");
            removeChromeVariable("--viewer-footer-height");
            onLayoutChange?.();
            return;
        }

        const topbarHeight = isTopbarCollapsed() ? 0 : measureElementHeight(elements.topbar);
        setChromeVariable("--viewer-topbar-height", `${topbarHeight}px`);
        setChromeVariable("--viewer-projects-height", `${measureElementHeight(elements.workspaceTabStrip)}px`);
        setChromeVariable("--viewer-view-toolbar-height", `${measureElementHeight(elements.viewToolbar)}px`);
        setChromeVariable("--viewer-footer-height", `${measureElementHeight(elements.appFooter)}px`);
        onLayoutChange?.();
    }

    function initialize() {
        update();

        if (typeof ResizeObserver !== "undefined" && !resizeObserver) {
            resizeObserver = new ResizeObserver(() => update());
            for (const element of Object.values(elements)) {
                if (element) {
                    resizeObserver.observe(element);
                }
            }
        }
    }

    return {
        initialize,
        update
    };

    function setChromeVariable(name, value) {
        document.documentElement.style.setProperty(name, value);
        document.body?.style.setProperty(name, value);
    }

    function removeChromeVariable(name) {
        document.documentElement.style.removeProperty(name);
        document.body?.style.removeProperty(name);
    }
}

function measureElementHeight(element) {
    if (!element) {
        return 0;
    }

    return Math.ceil(element.getBoundingClientRect().height);
}
