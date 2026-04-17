# Bootstrap Cognitive Indexing

CTX should not treat bootstrap indexing as raw entity extraction.

The purpose of bootstrap mapping is:

- reconstruct a provisional line of thought
- identify what problem appears to be in play
- infer candidate hypotheses
- collect supporting evidence-like excerpts
- surface open questions
- suggest possible tasks for deliberate follow-up

The bootstrap output is provisional by design.

It is not equivalent to:

- a final CTX graph
- a durable task plan
- a complete import of repository truth

## Why this exists

When CTX starts from an existing directory, project, article, or document set, there may be no prior cognitive state.

A bootstrap surface should help agents recover:

- what the material seems to be trying to solve
- what claims or hypotheses it appears to make
- what evidence supports those claims
- what uncertainty remains

Without this, CTX risks becoming either:

- a blank repository with no starting thread
- or a flat schema dump that preserves entities but loses the idea behind them

## Command shape

Initial command:

```powershell
ctx bootstrap map --from <path> [--mode auto|article|project] [--max-files <n>]
```

Conservative promotion surface:

```powershell
ctx bootstrap apply --from <path> [--mode auto|article|project] [--max-files <n>] [--parent-goal <goalId>]
```

## Output shape

The command should return a provisional map with:

- `projectSummary`
- `candidateThreads`
- `workingProblem`
- `candidateHypotheses`
- `supportingEvidence`
- `possibleTasks`
- `openQuestions`
- `guidance`

For contradiction-heavy material, the provisional map should also be able to return:

- `candidateHypothesisSets`
- `conflicts`
- `sharedEvidence`
- `openTensions`

## Design rule

Bootstrap should preserve the thought, not only the entity.

Bad bootstrap:

- `Goal exists`
- `Task exists`
- `Hypothesis exists`

Good bootstrap:

- `it looks like the material is trying to solve X`
- `it appears to rely on hypothesis Y`
- `these excerpts function as evidence for Y`
- `these uncertainties are still open`

## Persistence rule

The first version should not write final CTX entities automatically.

Bootstrap output should stay reviewable first.

Then a later surface can decide whether to:

- open a work line
- promote a candidate hypothesis
- attach evidence deliberately
- or discard weak bootstrap inference

## Apply rule

`ctx bootstrap apply` exists to bridge provisional inference and durable CTX without pretending the bootstrap already knows the truth.

It should:

- select only the strongest candidate thread
- open one provisional goal
- seed one review task
- promote only a bounded set of hypotheses and evidence
- mark the resulting line as bootstrap/provisional in wording and trace tags

For contradiction-heavy sources, `apply` should not default to a single synthesized hypothesis if the source itself presents multiple live interpretations.

In that case it should be able to:

- promote multiple parallel provisional hypotheses
- keep evidence linked separately per interpretation when possible
- preserve unresolved conflict explicitly
- defer synthesis until review decides to merge or supersede interpretations

It should not:

- import the whole source tree as CTX
- accept decisions automatically
- write conclusions that imply the thread is already validated

The correct follow-up after `apply` is not immediate closeout.

The correct follow-up is:

- inspect the promoted line
- validate or reject hypotheses
- strengthen evidence deliberately
- and only then produce accepted decisions or closure

## Bootstrap v2 direction

The next design direction for bootstrap is `hypothesis isolation`.

That means:

- preserve competing interpretations first
- synthesize later only by explicit review

Bad contradiction handling:

- detect several interpretations
- collapse them into one coherent “best explanation”

Better contradiction handling:

- keep `Hypothesis A`
- keep `Hypothesis B`
- keep `Hypothesis C`
- let each retain its own evidence, score, and later partial conclusion

This design is especially important for:

- scientific or quasi-scientific writing
- ambiguous technical design documents
- postmortems with mutually plausible explanations
- any source where the right output is a stable tension, not a premature synthesis

