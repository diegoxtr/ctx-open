# Hypothesis Scoring
If a language model and its agent lose context, this is the tool you need.

## Goal

Provide a simple, operational way to prioritize hypotheses by probability, impact, and support.

## Current problem

Today CTX stores `confidence`, but that is not enough to decide:

- which hypothesis to validate first
- which has the most potential value
- which is already weakened by contradictory evidence

## Proposed v1 model

Each hypothesis should be evaluated on four dimensions:

1. `probability`
   how likely it seems to be true

2. `impact`
   how much the product changes if it is true

3. `evidenceStrength`
   how much structured evidence supports it today

4. `costToValidate`
   how expensive it is to validate

## Suggested simple formula

```text
hypothesisScore = (probability * 0.35) + (impact * 0.35) + (evidenceStrength * 0.20) - (costToValidate * 0.10)
```

Suggested scale:

- `0.0` to `1.0`

## Score interpretation

- `0.80 - 1.00` highest priority
- `0.60 - 0.79` important
- `0.40 - 0.59` secondary
- `< 0.40` low priority or needs redefinition

## How to estimate each component

### probability

- expert intuition
- prior evidence
- pattern repetition

### impact

- token savings
- time savings
- traceability improvement
- reduced rework

### evidenceStrength

- evidence count
- source quality
- consistency across evidence

### costToValidate

- time
- technical complexity
- need for real users

## Next product step

Implement in CTX:

- new scoring fields for `Hypothesis`
- simple recalculation from CLI or core
- score visualization in the viewer
- filters for high-priority hypotheses

