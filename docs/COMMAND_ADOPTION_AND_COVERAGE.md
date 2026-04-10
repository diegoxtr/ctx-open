# Command Adoption and Coverage
If a language model and its agent lose context, this is the tool you need.

This document summarizes how the CTX CLI is currently used in the authoring workflow and what to do with underused or cold commands.

The goal is not to chase coverage for its own sake.

Rule:

- first adopt what improves daily flow
- then validate what unlocks future work
- only after that cover peripheral or rare surfaces

## Current snapshot

Source:

- `ctx usage summary`
- `ctx usage coverage`

Observed state in this repo:

- `totalKnownCommands`: `49`
- `usedCommandCount`: `31`
- `unusedCommandCount`: `21`
- `coveragePercentage`: `63.27`

## Hot commands

These are already part of the real product flow:

- `evidence add`
- `commit`
- `status`
- `audit`
- `conclusion add`
- `next`
- `hypo add`
- `hypo update`
- `task update`
- `graph lineage`
- `decision add`
- `task add`

Interpretation:

- CTX is already used strongly for traceability, cognitive closure, and consistency
- the dominant loop today is:
  - observe
  - record evidence
  - update state
  - close with a conclusion
  - cognitive commit

## Warm commands

They are used, but not yet part of the dominant loop:

- `log`
- `graph summary`
- `task list`
- `task show`
- `graph show`
- `goal list`
- `hypo rank`
- `hypo show`
- `usage summary`
- `usage coverage`
- `decision list`
- `diff`
- `metrics show`
- `thread reconstruct`
- `version`

Interpretation:

- these surfaces exist and are useful
- but they do not yet define the daily primary flow
- several depend on inspection or debugging needs rather than normal closure

## Cold or unused commands

Surfaces with no real use yet:

- `branch`
- `checkout`
- `conclusion show`
- `context`
- `doctor`
- `evidence list`
- `evidence show`
- `export`
- `goal add`
- `goal show`
- `graph export`
- `hypo list`
- `import`
- `init`
- `merge`
- `packet list`
- `packet show`
- `provider list`
- `run`
- `run list`
- `run show`

Interpretation:

- some are cold because the flow does not need them yet
- some are cold because the product does not push them in the current workflow yet
- some are edge capabilities, not the happy path

## Not all cold commands are equally important

To prioritize them, separate by value.

### Tier 1: high value near current flow

Validate first:

- `goal add`
- `goal show`
- `hypo list`
- `evidence list`
- `evidence show`
- `conclusion show`
- `doctor`
- `context`

Reason:

- very close to the real daily flow
- close inspection and operational gaps
- can improve daily work without heavy new complexity

### Tier 2: structural value for CTX as a full system

Validate later:

- `branch`
- `checkout`
- `merge`
- `graph export`
- `export`
- `import`

Reason:

- critical to the full CTX model
- not yet part of the daily core workflow
- require controlled scenarios and intentional testing

### Tier 3: future or specialized value

Defer:

- `run`
- `run list`
- `run show`
- `provider list`
- `packet list`
- `packet show`

Reason:

- depend on providers, integrations, or advanced workflows
- not the main day-to-day friction today

### Tier 4: do not prioritize now

- `init`

Reason:

- adds little to an already initialized repo
- value is higher in demos, onboarding, or new repos

## Recommended prioritization rule

When choosing which cold command to work on first, use this order:

1. commands that reduce friction in the current daily flow
2. commands that complete a family already used heavily
3. commands that unlock structural system capabilities
4. specialized or integration commands

Do not choose by:

- raw count of unused commands
- desire to raise coverage without operational value

## Families that are currently incomplete

### Goals

Today:

- `goal list` is used
- `goal add` and `goal show` are not

Reading:

- the family exists but is not fully integrated into real flow

### Evidence

Today:

- `evidence add` is the most used command in CTX
- `evidence list` and `evidence show` are not used

Reading:

- this is a clear gap
- writing is adopted, evidence inspection is not

### Conclusions

Today:

- `conclusion add` and `conclusion update` are used
- `conclusion show` is not

Reading:

- the closure family is alive
- but the read surface did not enter the loop

### Branching

Today:

- `branch`, `checkout`, `merge` are unused

Reading:

- branch semantics exist in the viewer and model
- but real flow still does not use cognitive branches daily

### Providers and runs

Today:

- `run*` and `provider list` are unused

Reading:

- CTX operates strongly as a structured cognitive system
- but not yet as a routine model-run orchestrator

## Recommended plan

### Step 1

Validate and document:

- `evidence list`
- `evidence show`
- `goal add`
- `goal show`
- `conclusion show`

### Step 2

Review whether `context` and `doctor` should enter the standard operational protocol.

### Step 3

Build controlled demos or smoke tests for:

- `branch`
- `checkout`
- `merge`
- `export`
- `import`

### Step 4

Only after that push:

- `run`
- `provider list`
- `packet list`
- `packet show`

## Final rule

An unused command is not automatically critical debt.

It is priority debt only if:

- it belongs to a family already central to the real flow
- fixing it reduces daily friction
- or it unlocks a structural capability CTX needs to use itself

