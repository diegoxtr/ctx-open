export function buildApiUrl(endpoint, params = null) {
    const query = params instanceof URLSearchParams
        ? params.toString()
        : new URLSearchParams(params ?? {}).toString();

    return query ? `${endpoint}?${query}` : endpoint;
}

export function apiGet(endpoint, params = null) {
    return fetch(buildApiUrl(endpoint, params), { cache: "no-store" });
}

export function apiPost(endpoint, params = null) {
    return fetch(buildApiUrl(endpoint, params), {
        method: "POST",
        cache: "no-store"
    });
}
