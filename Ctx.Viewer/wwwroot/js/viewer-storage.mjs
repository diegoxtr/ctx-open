export function readStorageValue(key) {
    return window.localStorage.getItem(key);
}

export function writeStorageValue(key, value) {
    window.localStorage.setItem(key, value);
}

export function readStorageJson(key, fallback) {
    const raw = readStorageValue(key);
    if (!raw) {
        return fallback;
    }

    try {
        return JSON.parse(raw);
    } catch {
        return fallback;
    }
}

export function writeStorageJson(key, value) {
    writeStorageValue(key, JSON.stringify(value));
}

export function readStorageBoolean(key, fallback = false) {
    const storedValue = readStorageValue(key);
    if (storedValue === null) {
        return fallback;
    }

    return storedValue === "true";
}

export function writeStorageBoolean(key, value) {
    writeStorageValue(key, value ? "true" : "false");
}
