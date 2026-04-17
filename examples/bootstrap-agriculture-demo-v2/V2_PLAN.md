# Bootstrap Agriculture Demo V2 Plan

## Purpose

Repeat the agricultural contradiction test with the same overall workflow as v1, but explicitly target `hypothesis isolation`.

The product question is no longer:

- can bootstrap open a provisional line from a hard article?

That is already validated.

The new question is:

- can bootstrap preserve multiple live interpretations as separate provisional hypotheses instead of collapsing them into one coherent synthesis?

## Why v2 exists

The v1 contradiction test showed:

- CTX can detect conflict
- CTX can open a reviewable provisional line
- CTX review can manually recover multiple hypotheses

But it also showed:

- bootstrap apply still resolves tension too early
- the strongest surviving line becomes an over-coherent synthesis
- the system still behaves more like an analyst than a scientist

## Design target

V2 should try a different resolution path:

1. Detect that the source contains explicit competing interpretations.
2. Preserve each interpretation as a separate provisional hypothesis.
3. Attach evidence to each competing hypothesis directly.
4. Delay synthesis unless the review explicitly chooses to synthesize later.

## Planned workflow

### Step 1

Use the same contradiction-heavy agricultural source as the semantic stress case.

### Step 2

Run a v2 bootstrap mapping strategy that tries to emit:

- one working problem
- multiple candidate interpretations
- evidence grouped by interpretation
- unresolved tensions or open conflicts

### Step 3

Apply conservatively into CTX, but promote:

- parallel hypotheses
- not a single merged explanation

### Step 4

Review whether the resulting graph:

- shows coexistence
- preserves conflict
- avoids premature synthesis

### Step 5

Document failures in testing terms:

- did conflict collapse again?
- did evidence cross-wire incorrectly?
- did the graph remain reviewable?
- did naming improve?

## Success criteria

The v2 experiment counts as successful if:

- at least two or three competing hypotheses remain alive after apply
- each has direct evidence
- no synthetic “everything interaction model” becomes the only promoted hypothesis by default
- the resulting line is still usable in CTX without overwhelming the graph

## Expected product implications

If v2 works, bootstrap should gain a new design rule:

- preserve contradiction first
- synthesize later

If v2 fails, the next change is likely not prompt-only.
It will probably require a deeper change in the bootstrap output model itself.

