const assert = require("assert");
const { calculateTotal } = require("../src/pricing");

function testRegularOrderWithoutDiscount() {
  const result = calculateTotal({
    subtotal: 100,
    isVip: false,
    coupon: ""
  });

  assert.strictEqual(result.discountRate, 0);
  assert.strictEqual(result.total, 100);
}

function testVipReceivesSingleDiscount() {
  const result = calculateTotal({
    subtotal: 100,
    isVip: true,
    coupon: ""
  });

  assert.strictEqual(result.discountRate, 0.1);
  assert.strictEqual(result.discountAmount, 10);
  assert.strictEqual(result.total, 90);
}

function testVipWithCouponStacksOncePerRule() {
  const result = calculateTotal({
    subtotal: 200,
    isVip: true,
    coupon: "WELCOME5"
  });

  assert.strictEqual(result.discountRate, 0.15);
  assert.strictEqual(result.discountAmount, 30);
  assert.strictEqual(result.total, 170);
}

function run() {
  testRegularOrderWithoutDiscount();
  testVipReceivesSingleDiscount();
  testVipWithCouponStacksOncePerRule();
  console.log("pricing regression tests passed");
}

run();
