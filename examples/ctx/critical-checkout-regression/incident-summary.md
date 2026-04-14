# Incident Summary

## Incident

VIP checkout totals were reported below the expected value when a promotional coupon was also present.

Expected business rule:

- VIP applies a single 10% discount.
- Coupon `WELCOME5` adds 5%.
- The final stacked rate for a VIP customer with `WELCOME5` is 15%.

## Observed Failure

The regression hypothesis was that the VIP rate was being applied twice in a coupon-enabled code path, causing undercharging on critical checkout flows.

## Root Cause

The investigation converged on discount accumulation logic as the risky area. The final implementation keeps discount composition explicit and rounds the rate before downstream calculations so the runtime output stays stable.

## Final Outcome

- Regular checkout remains unchanged.
- VIP-only checkout totals at 90 for a subtotal of 100.
- VIP plus `WELCOME5` totals at 170 for a subtotal of 200.
- Regression tests pass.
