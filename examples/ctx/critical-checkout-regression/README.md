# Critical Checkout Regression

This example demonstrates a cognitive repository for a critical pricing bug.

Scenario:

- VIP customers should receive a single 10% discount.
- A regression caused the discount to be applied twice for VIP customers with coupon usage enabled.
- The final state in this example includes the fixed implementation, a minimal test suite, and a complete `.ctx` repository that records the investigation, evidence, decision, and closure.

Files:

- `src/pricing.js`: final fixed pricing logic
- `tests/pricing.test.js`: regression coverage
- `.ctx/`: full cognitive repository created with `ctx`

Run the regression tests:

```powershell
node .\tests\pricing.test.js
```
