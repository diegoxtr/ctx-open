export function cssEscape(value) {
    if (window.CSS?.escape) {
        return window.CSS.escape(value);
    }

    return String(value).replace(/["\\]/g, "\\$&");
}

export function escapeHtml(value) {
    value ??= "";
    return String(value)
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;");
}

export function escapeAttribute(value) {
    return escapeHtml(value)
        .replaceAll("\"", "&quot;")
        .replaceAll("'", "&#39;");
}

export function formatDateTime(value) {
    if (!value) {
        return "";
    }

    const date = new Date(value);
    return Number.isNaN(date.getTime()) ? String(value) : date.toLocaleString();
}

export function normalizeRepositoryPath(path) {
    return String(path ?? "").trim().toLowerCase();
}
