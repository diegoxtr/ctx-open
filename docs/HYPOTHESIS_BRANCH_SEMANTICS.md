# Hypothesis Branch Semantics
If a language model and its agent lose context, this is the tool you need.

This document defines the proposed branch-like semantics for competing hypotheses inside CTX.

The goal is not to turn hypotheses into repository branches immediately.
The goal is to let competing interpretations behave like branches:

- they start from shared context
- they evolve independently
- they accumulate their own evidence
- they can weaken, merge, supersede, or become dominant

This proposal is driven by the bootstrap agriculture regression cases, where contradiction-heavy material must preserve competing interpretations instead of collapsing too early into a synthetic single narrative.

## Design rule

The core rule is:

`preserve competing hypotheses first; synthesize only by explicit review`

This means:

- CTX should allow multiple active interpretations of the same problem
- contradiction is not an error state by default
- synthesis is a later operation, not the initial behavior

## Status

Current status:

- model fields are implemented
- CLI operations are implemented
- help text is updated
- viewer surfaces are implemented
- the main graph keeps relation overlays optional to preserve readability

## Scope

This proposal is intentionally limited.

In scope:

- branch-like lifecycle for hypotheses
- explicit relations between competing hypotheses
- hypothesis-scoped evidence behavior
- minimal CLI and viewer surfaces to inspect and operate on those semantics

Out of scope for the first version:

- mapping hypotheses directly to repository branches
- reusing Git-like merge mechanics at repository level
- changing commit or branch storage semantics for the whole repo

## Minimal model

The first functional version should extend `Hypothesis` with additional semantics instead of replacing the current model.

### Proposed fields

- `branchState`
- `branchRole`
- `lineageGroupId`
- `parentHypothesisIds`
- `mergedIntoHypothesisId`
- `supersedesHypothesisIds`

### `branchState`

Suggested values:

- `active`
- `weakening`
- `merged`
- `deprecated`
- `promoted`

Intent:

- `active`: hypothesis is still alive as a serious interpretation
- `weakening`: evidence is reducing confidence but not closing the line yet
- `merged`: the interpretation has been absorbed into another hypothesis
- `deprecated`: no longer useful as a live explanatory path
- `promoted`: the current dominant interpretation for the lineage group

### `branchRole`

Suggested values:

- `competing`
- `integrative`
- `dominant`

Intent:

- `competing`: one of several rival interpretations
- `integrative`: combines parts of other branches without erasing their history
- `dominant`: currently favored interpretation for operational use

### `lineageGroupId`

Groups competing hypotheses that belong to the same unresolved interpretation space.

Example:

- `microbiota-first`
- `microclimate-first`
- `complex interaction`

These should remain separate hypotheses inside one lineage group instead of collapsing into a single narrative too early.

### Parent and resolution fields

- `parentHypothesisIds`
  - lets a hypothesis derive from one or more earlier interpretations
- `mergedIntoHypothesisId`
  - records where a hypothesis ended up when merged
- `supersedesHypothesisIds`
  - records which earlier hypotheses were replaced by a stronger formulation

## Required relations

The first usable version needs explicit relations between hypotheses.

Suggested relations:

- `competes-with`
- `merged-into`
- `supersedes`
- `derived-from`
- `borrows-evidence-from`

### Meaning

- `competes-with`
  - two interpretations are live and incompatible or partially incompatible
- `merged-into`
  - one interpretation has been absorbed into another
- `supersedes`
  - a stronger hypothesis replaces an older one
- `derived-from`
  - a new branch was formed from a previous interpretation
- `borrows-evidence-from`
  - evidence first attached to one hypothesis is explicitly reused by another

## Evidence behavior

Evidence should not be treated as global by default when branch-like semantics matter.

The minimum usable policy is:

- evidence can support a specific hypothesis directly
- evidence can be shared across hypotheses
- evidence can be borrowed without collapsing the interpretations into one

This is necessary because contradiction-heavy material often contains observations that:

- support one hypothesis strongly
- weaken another
- or remain ambiguous across several competing readings

## CLI surface

The first implementation stays compact.

Implemented commands:

- `ctx hypo update <id> --branch-state <state>`
- `ctx hypo relate <a> --relation <type> --to <b>`
- `ctx hypo merge <from> --into <to>`
- `ctx hypo supersede <old> --by <new>`
- `ctx evidence share <evidenceId> --to hypothesis:<id>`

The point is not to provide every branch operation immediately.
The point is to make the state and relationships inspectable and explicit.

## Viewer surface

The viewer should not hide losing interpretations.

Planned behavior:

- show a badge for `branchState`
- visually group hypotheses by `lineageGroup`
- render parallel competing hypotheses side by side when useful
- render merge and supersede relations explicitly
- preserve merged or deprecated branches as visible history

## Agriculture contradiction example

A single lineage group could contain:

- `microbiota-first`
- `microclimate-first`
- `complex interaction`

Possible evolution:

1. all three begin as `active`
2. later evidence weakens the first two
3. the integrative hypothesis absorbs part of both
4. the first two become `merged`
5. the integrative hypothesis becomes `promoted`

That sequence preserves contradiction, evolution, and resolution.

## Why this is preferable to direct repository branching

Repository branches today are repo-level work lines.
Competing hypotheses are interpretation-level lines.

They are related ideas, but not the same thing.

If CTX maps hypotheses to real repository branches too early, it risks mixing:

- repository history
- working context
- commit semantics
- interpretation semantics

The first version should therefore keep branch-like behavior inside the hypothesis model.

## Implementation order

Recommended order:

1. add model fields and relations
2. expose them in CLI
3. render minimal inspection surfaces in the viewer
4. validate on contradiction-heavy regression cases
5. only then consider whether any part should map to repository branching

