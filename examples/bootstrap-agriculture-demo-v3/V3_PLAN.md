# Bootstrap Agriculture Demo V3 Plan

## Objective

Validate the implementation change for coexistence-first bootstrap behavior on the same agricultural contradiction text used in `v1` and `v2`.

## Why v3 exists

`v1` proved:

- CTX review can recover multiple hypotheses after manual correction

`v2` proved:

- the current installed bootstrap still collapses the same text into one literal provisional hypothesis

`v3` should prove:

- the implementation change actually altered bootstrap behavior, not just our interpretation of the output

## Source

Use the same contradiction-heavy agricultural article as the baseline comparison case.

## Expected behavior

`bootstrap map` should:

- recognize that the document contains multiple coexisting interpretations
- emit multiple candidate hypotheses instead of one dominant literal paragraph hypothesis
- preserve open tensions or unresolved conflict

`bootstrap apply` should:

- open one provisional line
- promote multiple parallel provisional hypotheses
- keep evidence attached per interpretation where possible
- avoid defaulting immediately to a synthetic “everything interacts” master claim

## Pass condition

The experiment passes only if at least these interpretations survive as separate provisional hypotheses:

1. microbiota-first
2. microclimate-first
3. complex interaction / hidden-variable model

## Failure condition

The experiment fails if the implementation still does either of these:

- promotes only one literal hypothesis based on the article opening
- collapses the contradiction into one global synthetic explanation during `apply`

## Operational rule

Do not close `v3` based on narrative preference.

Close it only after comparing the actual `map` and `apply` output against the `v2` baseline.

