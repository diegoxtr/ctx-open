# Bootstrap Agriculture Demo V4 Real Testing

Repo under test:

- `C:\sources\ctx-open\examples\bootstrap-agriculture-demo-v4`

Source artifact:

- [agricultural-systems-contradictions.md](C:/sources/ctx-open/examples/bootstrap-agriculture-demo-v4/agricultural-systems-contradictions.md)

## Objective

Validate whether branch-like hypothesis semantics behave coherently on the same contradiction-heavy agricultural case used in prior bootstrap regressions.

This file should document:

- the exact commands used
- which branch-like fields were set
- which relations were recorded
- whether merge or supersede behavior was coherent
- whether evidence sharing preserved interpretation boundaries
- what still felt weak or confusing in CLI and viewer

## Commands used

- `C:\ctx\bin\Ctx.Cli.exe bootstrap map --from .\agricultural-systems-contradictions.md --mode article`
- `C:\ctx\bin\Ctx.Cli.exe bootstrap apply --from .\agricultural-systems-contradictions.md --mode article --parent-goal 8508aa108adf444fba9af1dcfd1f6d4e`
- `C:\ctx\bin\Ctx.Cli.exe hypo update 56519b9772164151aa51ff8adcd17127 --branch-state Active --branch-role Competing --lineage-group agriculture-conflict-v4`
- `C:\ctx\bin\Ctx.Cli.exe hypo update 1723ca1a8a384863a79082484bd8afee --branch-state Active --branch-role Competing --lineage-group agriculture-conflict-v4`
- `C:\ctx\bin\Ctx.Cli.exe hypo update 820713e2c36b4affa745a01715f673fa --branch-state Active --branch-role Integrative --lineage-group agriculture-conflict-v4`
- `C:\ctx\bin\Ctx.Cli.exe hypo relate 56519b9772164151aa51ff8adcd17127 --relation CompetesWith --to 1723ca1a8a384863a79082484bd8afee --note "same contradiction set"`
- `C:\ctx\bin\Ctx.Cli.exe evidence share 6c46f746ac9b47ad990b9c24f2c6d56f --to hypothesis:820713e2c36b4affa745a01715f673fa`
- `C:\ctx\bin\Ctx.Cli.exe evidence share 29445686d3be4e84a259b03e12088258 --to hypothesis:820713e2c36b4affa745a01715f673fa`
- `C:\ctx\bin\Ctx.Cli.exe hypo merge 56519b9772164151aa51ff8adcd17127 --into 820713e2c36b4affa745a01715f673fa`
- `C:\ctx\bin\Ctx.Cli.exe hypo merge 1723ca1a8a384863a79082484bd8afee --into 820713e2c36b4affa745a01715f673fa`
- `C:\ctx\bin\Ctx.Cli.exe hypo add --statement "Persistent sector behavior and hidden structural constraints should now be treated as the dominant interpretation over the earlier integrative branch." --rationale "V4 review inference: the text's persistence pattern and explicit hidden-variable language justify promoting a structural-constraints reading beyond the earlier integrative summary." --confidence 0.77 --impact 0.78 --evidence-strength 0.72 --cost-to-validate 0.45 --task d5c559decfbf4050981bebab4130518a`
- `C:\ctx\bin\Ctx.Cli.exe hypo update 63890d65ca674bfba45b8c04ffdc5c08 --branch-state Promoted --branch-role Dominant --lineage-group agriculture-conflict-v4`
- `C:\ctx\bin\Ctx.Cli.exe hypo relate 63890d65ca674bfba45b8c04ffdc5c08 --relation DerivedFrom --to 820713e2c36b4affa745a01715f673fa --note "v4 review branch"`
- `C:\ctx\bin\Ctx.Cli.exe evidence add --title "V4 review evidence: persistence favors structural constraints" --summary "The article reports repeated sector persistence across campaigns, which supports treating hidden structural conditions as a stronger interpretation than the earlier integrative branch alone." --source "C:\sources\ctx-open\examples\bootstrap-agriculture-demo-v4\agricultural-systems-contradictions.md" --kind Document --confidence 0.77 --supports hypothesis:63890d65ca674bfba45b8c04ffdc5c08`
- `C:\ctx\bin\Ctx.Cli.exe evidence share 2e8dbec9ddeb4336ad64118d2649bfbd --to hypothesis:63890d65ca674bfba45b8c04ffdc5c08`
- `C:\ctx\bin\Ctx.Cli.exe hypo supersede 820713e2c36b4affa745a01715f673fa --by 63890d65ca674bfba45b8c04ffdc5c08`
- `C:\ctx\bin\Ctx.Cli.exe graph export --format json`

## Observed behavior

- The coexistence-first bootstrap baseline from `v3` carried over correctly: `map/apply` opened three separate hypotheses instead of one synthetic paragraph.
- `branch-state`, `branch-role`, and `lineage-group` updated cleanly on all three baseline hypotheses.
- `competes-with` worked as expected and wrote a reciprocal competing relation between the microbiota-first and microclimate-first branches.
- `evidence share` did not collapse the hypotheses. It expanded the integrative branch's support set while preserving the original evidence ownership on the competing branches.
- `merge` behaved coherently for this case:
  - the two competing branches moved to `Merged`
  - each retained its own identity and history
  - each recorded `mergedIntoHypothesisId = 820713e2c36b4affa745a01715f673fa`
- `supersede` also behaved coherently once a second-order review hypothesis was introduced:
  - the earlier integrative branch was marked `Deprecated`
  - the new dominant branch (`63890d65ca674bfba45b8c04ffdc5c08`) recorded `supersedesHypothesisIds`
  - the graph export showed both `supersedes` and `derived-from`
- `graph export --format json` is now the clearest inspection surface for this feature. It exposes:
  - branch metadata on hypothesis nodes
  - `competes-with`
  - `merged-into`
  - `supersedes`
  - `derived-from`
- The fresh-repo desync seen right after `v4` creation did not persist through the full test. By the end of the run, `ctx status` was showing the seeded goals/tasks/hypotheses correctly.
- One small CLI rough edge remains: the first attempt to create the second-order dominant hypothesis inside a multi-step PowerShell script partly completed and did not print every intermediate success message, so explicit follow-up inspection was still needed.

## Comparison against v3

`v3` proved that bootstrap could preserve multiple competing hypotheses on the first pass.

`v4` adds a new layer on top of that:

- `v3` preserved coexistence
- `v4` preserves coexistence and then lets those interpretations evolve

New validated behavior in `v4`:

- competing interpretations can carry explicit branch-like lifecycle
- evidence can be shared into a stronger integrative or dominant interpretation without deleting the original branch context
- merged branches remain inspectable as cognitive history
- a later dominant interpretation can supersede an earlier integrative branch explicitly instead of silently replacing it

What `v4` still does not validate:

- viewer ergonomics for these relations
- whether `borrowed/shared evidence` should be rendered differently from native supporting evidence
- whether supersede should always imply `Dominant` or remain a separate review choice

## Conclusion

`v4` passes as a product-validation repo for the first branch-like hypothesis layer.

The important result is not just that the commands run. It is that the same contradiction-heavy agricultural article can now move through these phases without collapsing into one premature narrative:

1. coexistence of competing interpretations
2. explicit competition
3. integrative merge
4. second-order dominant interpretation

That means CTX is now capable of storing not just multiple hypotheses, but also part of the lifecycle of how one interpretation absorbs or supersedes another.

The next bottleneck is no longer the core model. It is surface quality:

- viewer rendering
- clearer CLI inspection
- better differentiation between native, shared, and borrowed evidence

