# Bootstrap Test Development
If a language model and its agent lose context, this is the tool you need.

This document tracks how bootstrap indexing is being tested as a product surface, not just as a CLI feature.

Its purpose is to preserve:

- what test cases we created
- why each case exists
- what the bootstrap got right
- what it got wrong
- what conclusions changed in the product after each test

It should evolve as a running test-development log.

Public packaging note:

- `ctx-open` publishes the source texts, plans, and testing notes for these bootstrap cases
- the full private `.ctx` workspaces used during development remain in `ctx-private`

## Why this document exists

`ctx bootstrap map` and `ctx bootstrap apply` are not ordinary extraction commands.

They are trying to reconstruct provisional cognitive threads from external material.

That means the most important questions are not:

- did the parser run
- did the command return JSON

The important questions are:

- did the bootstrap preserve the idea being pursued
- did it create reviewable hypotheses instead of copied paragraphs
- did it expose ambiguity honestly
- did it produce a usable provisional line inside CTX

So the test log has to preserve product behavior, interpretive failures, and operational conclusions.

## Test method

Each bootstrap test case should capture five things:

1. Source material
2. Expected cognitive difficulty
3. Observed output quality
4. Failure modes
5. Product conclusions

The output itself is not enough.

We need to know whether the feature:

- flattened the source
- named the thread badly
- attached evidence incorrectly
- missed the actual working problem
- or produced a useful provisional line

## Current test cases

### 1. README bootstrap smoke

Purpose:

- validate that bootstrap can open a provisional line from product-facing material
- confirm the `map/apply` split works end-to-end

What it proved:

- bootstrap can produce a provisional map
- bootstrap can conservatively promote one review line into CTX

Known limitation:

- README material is relatively cooperative
- it does not stress ambiguity or multi-factor reasoning very hard

### 2. Agricultural systems article

Repo:

- [examples/bootstrap-agriculture-demo/README.md](C:/sources/ctx-open/examples/bootstrap-agriculture-demo/README.md)

Source:

- [examples/bootstrap-agriculture-demo/agricultural-systems-article.md](C:/sources/ctx-open/examples/bootstrap-agriculture-demo/agricultural-systems-article.md)

Testing notes:

- [examples/bootstrap-agriculture-demo/REAL_TESTING.md](C:/sources/ctx-open/examples/bootstrap-agriculture-demo/REAL_TESTING.md)

Purpose:

- stress bootstrap with a dense domain article
- validate behavior under ambiguity, non-linear interactions, and mixed scientific/operational framing

What it proved:

- bootstrap can still open a coherent provisional line in a harder text
- bootstrap can be reviewed and closed inside a dedicated cognitive repo

What it exposed:

- first-pass hypothesis extraction can still be too literal
- thread naming can still be too generic
- fallback evidence linkage can still miss direct hypothesis support
- installed local CLI parity still matters for real testing

### 3. Contradiction-heavy agricultural article

Repo:

- [examples/bootstrap-agriculture-demo/README.md](C:/sources/ctx-open/examples/bootstrap-agriculture-demo/README.md)

Source:

- [examples/bootstrap-agriculture-demo/agricultural-systems-contradictions.md](C:/sources/ctx-open/examples/bootstrap-agriculture-demo/agricultural-systems-contradictions.md)

Testing notes:

- [examples/bootstrap-agriculture-demo/REAL_TESTING.md](C:/sources/ctx-open/examples/bootstrap-agriculture-demo/REAL_TESTING.md)

Purpose:

- stress bootstrap with a source whose main structure is contradiction, not one dominant claim
- test whether bootstrap preserves multiple coexisting interpretations instead of flattening them

What it proved:

- CTX review can recover a useful multi-hypothesis line after bootstrap opens the provisional thread
- the example repo is a good real regression harness for ambiguity-heavy text

What it exposed:

- bootstrap is still not contradiction-aware by default
- the first pass still collapses explicit competing interpretations into one paragraph-level hypothesis
- thread naming remains too generic for conflict-heavy sources
- contradiction retention is now a first-class quality target, not a secondary polish issue

### 4. Agricultural contradiction case v2

Repo:

- [examples/bootstrap-agriculture-demo-v2/README.md](C:/sources/ctx-open/examples/bootstrap-agriculture-demo-v2/README.md)

Plan:

- [examples/bootstrap-agriculture-demo-v2/V2_PLAN.md](C:/sources/ctx-open/examples/bootstrap-agriculture-demo-v2/V2_PLAN.md)

Purpose:

- repeat the same contradiction-heavy source with the same testing shape
- change only the bootstrap strategy
- test whether contradiction can remain alive as parallel hypotheses instead of collapsing into one synthetic explanation

Current design target:

- `microbiota-first`
- `microclimate-first`
- `complex interaction`

must survive as separate provisional hypotheses through the first reviewable output.

### 5. Agricultural contradiction case v3

Repo:

- [examples/bootstrap-agriculture-demo-v3/README.md](C:/sources/ctx-open/examples/bootstrap-agriculture-demo-v3/README.md)

Plan:

- [examples/bootstrap-agriculture-demo-v3/V3_PLAN.md](C:/sources/ctx-open/examples/bootstrap-agriculture-demo-v3/V3_PLAN.md)

Testing notes:

- [examples/bootstrap-agriculture-demo-v3/REAL_TESTING.md](C:/sources/ctx-open/examples/bootstrap-agriculture-demo-v3/REAL_TESTING.md)

Purpose:

- validate the actual implementation change, not just the design intent
- compare the same contradiction-heavy article against the `v2` baseline

What it proved:

- `bootstrap map` can now emit multiple candidate hypotheses for contradiction-heavy material
- `bootstrap apply` can now promote multiple parallel provisional hypotheses into one review line
- useful `openQuestions` can be preserved instead of flattened away

What still needs judgment:

- whether the integrative `complex interaction` hypothesis should always be peer-level with the other two, or sometimes remain a later synthesis candidate
- whether evidence grouping should stay interpretation-specific or gain a clearer notion of shared evidence

### 6. Agricultural contradiction case v4

Repo:

- [examples/bootstrap-agriculture-demo-v4/README.md](C:/sources/ctx-open/examples/bootstrap-agriculture-demo-v4/README.md)

Plan:

- [examples/bootstrap-agriculture-demo-v4/V4_PLAN.md](C:/sources/ctx-open/examples/bootstrap-agriculture-demo-v4/V4_PLAN.md)

Testing notes:

- [examples/bootstrap-agriculture-demo-v4/REAL_TESTING.md](C:/sources/ctx-open/examples/bootstrap-agriculture-demo-v4/REAL_TESTING.md)

Purpose:

- validate the branch-like hypothesis layer on top of the contradiction-aware bootstrap baseline
- test lifecycle, relations, merge, supersede, and evidence-sharing behavior on the same regression article

What it proved:

- competing hypotheses can now carry explicit branch-like metadata inside one lineage group
- evidence can be shared across interpretations without flattening them immediately
- merged hypotheses remain visible as cognitive history
- a later dominant interpretation can explicitly supersede an earlier integrative branch

What it exposed:

- graph export is already a strong inspection surface, but viewer rendering is still the main missing product layer
- CLI behavior is usable for validation, but long review flows still need better inspection ergonomics
- shared or borrowed evidence still needs clearer semantics and presentation

## Current product conclusions

At this stage, bootstrap indexing is validated as:

- a useful provisional mapper
- a viable review-line opener

It is not yet validated as:

- a strong synthesis layer for dense scientific text
- a release-ready indexing surface for ambiguous domain documents
- a contradiction-aware mapper that preserves multiple live interpretations on the first pass
- a coexistence-first bootstrap surface that delays synthesis until review

With `v3`, bootstrap is now provisionally validated as:

- capable of preserving multiple competing interpretations in the first-pass map/apply flow for at least one real contradiction-heavy article

With `v4`, branch-like hypothesis semantics are now provisionally validated as:

- capable of preserving, merging, superseding, and evidentially linking competing interpretations without turning them into repository branches

## What should be logged after every new test

Every new bootstrap test should update this document with:

- a short description of the source material
- why that material was chosen
- what the feature extracted well
- what the feature distorted
- whether the resulting line was usable inside CTX
- what product changes now seem necessary

## Expected next categories of tests

Useful future bootstrap tests include:

- technical design documents with competing hypotheses
- scientific or quasi-scientific papers with ambiguous evidence
- postmortems with mixed narrative and operational content
- long repository docs with multiple overlapping work lines
- articles where the correct output should preserve uncertainty instead of collapsing it
- sources where the right answer is a stable set of competing hypotheses rather than a single summary claim

## Rule for future additions

Do not append raw dumps of bootstrap JSON here.

This document is for:

- behavior
- failure modes
- testing evolution
- product conclusions

The raw extracted output belongs in the test repo or command logs.

