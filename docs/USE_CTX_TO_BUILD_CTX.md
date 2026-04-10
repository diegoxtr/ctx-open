# Use CTX to Build CTX
If a language model and its agent lose context, this is the tool you need.

## Goal

Use CTX as the official cognitive repository for the evolution of the product itself.

## What CTX should store in this process

- product objectives
- structured backlog
- value and architecture hypotheses
- evidence from real tests
- technical and product decisions
- per-iteration conclusions
- cognitive snapshots per commit

## What is missing today

- visual comparison of commits in the UI
- richer parent/child navigation in the timeline
- better capture of relations between runs and artifacts
- explicit hypothesis scoring model
- tighter operational flow for daily CTX use

## Recommended operational workflow

### 1. Open the cycle

```powershell
ctx status
ctx graph summary
ctx log
```

### 2. Create an iteration goal

Examples:

- harden viewer
- improve cognitive diff
- define hypothesis scoring
- prepare V1 pilot

### 3. Break down into tasks and hypotheses

Each iteration should have:

- 1 clear goal
- 2 to 5 tasks
- 1 to 3 relevant hypotheses

### 4. Record evidence while working

Examples:

- test result
- discovered limitation
- viewer feedback
- benchmark
- usage friction

### 5. Close with decision and conclusion

Do not leave cognitive commits without answering:

- what we learned
- what we changed
- why we changed it
- what remains open

### 6. Cognitive commit

Each useful block should end with:

```powershell
ctx commit -m "<block result>"
```

### 7. Autonomous continuation

If the operator or model only receives `continua`, it should:

1. re-read `ctx status`, `ctx graph summary`, and `ctx log`
2. choose the dominant active goal
3. select the most blocking or highest-value task
4. produce real evidence
5. record a conclusion
6. close a cognitive commit

Do not wait for manual direction if the CTX repository already makes the next step clear.

## Suggested cognitive work structure

### Goals

- usable local V1
- operational viewer
- cognitive capture method
- validation pilot

### Tasks

- improve UI
- package release
- install CLI
- test demos
- define scoring

### Hypotheses

- visual traceability reduces rework
- the viewer speeds up history comprehension
- simple scoring improves decision quality

## Recommended workspace

Use a specific CTX repository for product development:

- `<path-to-your-ctx-repository>`

## Expected outcome

If CTX works to build CTX, it should demonstrate:

- continuity across iterations
- less context loss
- more explicit decisions
- better backlog quality
- traceability of why the product took its current shape
- autonomous work continuation without relying on chat

