const assert = require("assert");
const { CatalogCache } = require("../src/catalogCache");

function testReadsReturnStoredProduct() {
  const cache = new CatalogCache();
  cache.set("product:1", { price: 120 }, ["catalog", "product:1"]);

  assert.deepStrictEqual(cache.get("product:1"), { price: 120 });
}

function testTagInvalidationClearsRelatedEntries() {
  const cache = new CatalogCache();
  cache.set("product:1", { price: 120 }, ["catalog", "category:boots"]);
  cache.set("product:2", { price: 95 }, ["catalog", "category:boots"]);
  cache.set("product:3", { price: 80 }, ["catalog", "category:sneakers"]);

  const removed = cache.invalidateTag("category:boots");

  assert.strictEqual(removed, 2);
  assert.strictEqual(cache.get("product:1"), undefined);
  assert.strictEqual(cache.get("product:2"), undefined);
  assert.deepStrictEqual(cache.get("product:3"), { price: 80 });
}

function run() {
  testReadsReturnStoredProduct();
  testTagInvalidationClearsRelatedEntries();
  console.log("catalog cache tests passed");
}

run();
