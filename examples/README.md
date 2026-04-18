# Examples

This folder contains two different kinds of example material:

- `ctx/`: showcase cognitive repositories that are meant to be opened, audited, and demonstrated individually
- `viewer-demo/`: a compact repository used to validate viewer behavior such as branches, history lanes, and graph rendering
- `bootstrap-agriculture-demo/`: public source text plus testing notes for validating bootstrap map/apply against a complex agricultural systems article
- `bootstrap-agriculture-demo-v2/`: the second-pass design pack for the same contradiction-heavy agricultural case, focused on hypothesis isolation instead of early synthesis
- `bootstrap-agriculture-demo-v3/`: the implementation-validation pack that proves coexistence-first bootstrap behavior against the same contradiction-heavy article
- `bootstrap-agriculture-demo-v4/`: the branch-semantics regression pack for the same contradiction-heavy agricultural article

## Recommended Order

If you want to understand CTX as a product showcase, start here:

1. `ctx/critical-checkout-regression`
2. `ctx/catalog-cache-branch-merge`
3. `ctx/agent-session-continuity`

If you want to validate the viewer itself, open:

- `viewer-demo`

If you want to validate bootstrap indexing quality, open:

- `bootstrap-agriculture-demo`
- `bootstrap-agriculture-demo-v2`
- `bootstrap-agriculture-demo-v3`
- `bootstrap-agriculture-demo-v4`

Public packaging note:

- the bootstrap agriculture folders in `ctx-open` now ship their `.ctx` workspaces alongside the source texts, plans, and testing notes
- the public examples are meant to be opened directly in the viewer and audited as reproducible cognitive repositories

## Important Note

The `ctx/` folder also contains legacy repository artifacts at its own root:

- `version.json`
- `config.json`
- `HEAD`
- `branches/`
- `metrics/`

Those files are not the primary demos. The primary demos are the subfolders listed above.

