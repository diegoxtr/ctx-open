const assert = require("assert");
const { buildDigest } = require("../src/digestBuilder");

function testUnreadNotificationsAppearInDigest() {
  const result = buildDigest([
    { id: "n1", title: "Build failed", category: "ops", read: false, hour: 10 },
    { id: "n2", title: "Comment added", category: "collab", read: true, hour: 11 }
  ]);

  assert.strictEqual(result.total, 1);
  assert.deepStrictEqual(result.categories, ["ops"]);
  assert.strictEqual(result.items[0].id, "n1");
}

function testQuietHoursSuppressNotifications() {
  const result = buildDigest(
    [
      { id: "n1", title: "Night alert", category: "ops", read: false, hour: 23 },
      { id: "n2", title: "Morning alert", category: "ops", read: false, hour: 9 }
    ],
    { quietHours: { start: 22, end: 7 } }
  );

  assert.strictEqual(result.total, 1);
  assert.strictEqual(result.items[0].id, "n2");
}

function run() {
  testUnreadNotificationsAppearInDigest();
  testQuietHoursSuppressNotifications();
  console.log("digest builder tests passed");
}

run();
