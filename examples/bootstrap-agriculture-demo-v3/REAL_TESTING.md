# Bootstrap Agriculture Demo V3 Real Testing

Repo under test:

- `C:\sources\ctx-open\examples\bootstrap-agriculture-demo-v3`

Source artifact:

- [agricultural-systems-contradictions.md](C:/sources/ctx-open/examples/bootstrap-agriculture-demo-v3/agricultural-systems-contradictions.md)

## Objective

Validate whether the coexistence-first implementation actually changes bootstrap behavior on the same contradiction-heavy agricultural text used in `v1` and `v2`.

The pass condition for `v3` is not “does bootstrap open a line”.

That was already true.

The real pass condition is:

- do multiple competing interpretations survive directly from `map`
- and do they remain separate after `apply`

## Commands used

Published local CLI:

```powershell
C:\ctx\bin\ctx.cmd bootstrap map --from .\agricultural-systems-contradictions.md
C:\ctx\bin\ctx.cmd bootstrap apply --from .\agricultural-systems-contradictions.md
```

## Observed behavior

`bootstrap map` now emits:

- a microbiota-centered hypothesis
- a microclimate-centered hypothesis
- a complex-interaction hypothesis

It also emits useful open questions, including:

- whether microbiota is dominant or only conditional
- whether microclimate predicts directly or only through interaction
- whether contradiction-heavy material should preserve multiple live interpretations

`bootstrap apply` now promotes all three hypotheses into the provisional task instead of collapsing the text into one paragraph-level bootstrap statement.

## Comparison against v2

`v2` baseline behavior:

- one literal hypothesis
- one provisional review line
- no meaningful open questions

`v3` implementation behavior:

- three parallel provisional hypotheses
- one provisional review line
- open questions preserved
- evidence attached per interpretation

## Conclusion

The implementation changed the behavior in the intended direction.

CTX now behaves more like:

- preserve contradiction first

and less like:

- force coherence immediately

This does not mean the problem is fully solved.

The third hypothesis is still an integrative reading, so later review still has to decide whether that synthesis should stay coequal with the other two or be treated as a later-stage interpretation.

But the core v3 objective is satisfied:

- contradiction no longer collapses immediately into one promoted hypothesis during bootstrap

