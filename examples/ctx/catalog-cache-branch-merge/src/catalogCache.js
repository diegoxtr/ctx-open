class CatalogCache {
  constructor() {
    this.values = new Map();
    this.tags = new Map();
  }

  set(key, value, tagList = []) {
    this.values.set(key, value);

    for (const tag of tagList) {
      if (!this.tags.has(tag)) {
        this.tags.set(tag, new Set());
      }

      this.tags.get(tag).add(key);
    }
  }

  get(key) {
    return this.values.get(key);
  }

  invalidateTag(tag) {
    const keys = this.tags.get(tag);
    if (!keys) {
      return 0;
    }

    let removed = 0;
    for (const key of keys) {
      if (this.values.delete(key)) {
        removed += 1;
      }
    }

    this.tags.delete(tag);
    return removed;
  }
}

module.exports = {
  CatalogCache
};
