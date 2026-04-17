# Real Testing Notes

Repo under test:

- `C:\sources\ctx-open\examples\bootstrap-agriculture-demo`

Source artifact:

- [agricultural-systems-article.md](C:/sources/ctx-open/examples/bootstrap-agriculture-demo/agricultural-systems-article.md)
- [agricultural-systems-contradictions.md](C:/sources/ctx-open/examples/bootstrap-agriculture-demo/agricultural-systems-contradictions.md)

## Testing objective

Validate whether `ctx bootstrap map` and `ctx bootstrap apply` can reconstruct provisional cognitive lines from a complex domain article without collapsing the article into:

- raw paragraph copying
- generic thread naming
- weak evidence linkage
- false certainty

## Commands used

Initialization:

```powershell
ctx init --name "Bootstrap Agriculture Demo" --description "Bootstrap validation repo for an agricultural systems article" --branch main
```

The installed CLI in `C:\ctx\bin` did not expose `bootstrap` yet, so the working test commands had to run from source:

```powershell
dotnet run --project C:\sources\ctx-open\Ctx.Cli -- bootstrap map --from .\agricultural-systems-article.md
dotnet run --project C:\sources\ctx-open\Ctx.Cli -- bootstrap apply --from .\agricultural-systems-article.md
```

For inspection after `apply`, using the built DLL avoided repeated `dotnet run` rebuild locks:

```powershell
dotnet C:\sources\ctx-open\Ctx.Cli\bin\Debug\net8.0\Ctx.Cli.dll status
dotnet C:\sources\ctx-open\Ctx.Cli\bin\Debug\net8.0\Ctx.Cli.dll audit
```

## What worked

- `bootstrap map` did preserve the central idea that the article is questioning traditional yield explanations and introducing a more complex interaction model.
- `bootstrap apply` stayed conservative.
  It created only:
  - one provisional goal
  - one review task
  - one hypothesis
  - three evidence items
- The resulting CTX line was explicitly marked as provisional/bootstrap in trace tags and wording.
- The article was correctly treated as an `article` source instead of as a multi-file project.

## Real failures observed

### 1. Installed CLI mismatch

The published `ctx` binary under `C:\ctx\bin` returned:

- `Unknown command`

for both:

- `ctx bootstrap map`
- `ctx bootstrap apply`

This means bootstrap is implemented in source but not yet available in the installed local CLI.

### 2. Hypothesis extraction is too literal

The strongest extracted hypothesis was effectively the opening paragraph itself, truncated, instead of a cleaner domain statement such as:

- yield differences may depend on microbiota and microclimate interactions beyond traditional factors

This shows that the current inference still over-weights the first long explanatory block instead of synthesizing its actual hypothesis.

### 3. Thread naming is too generic

`bootstrap apply` opened:

- `Bootstrap map: Bootstrap project map`

That title is mechanically correct but semantically weak.

For this article, a better provisional line would have sounded closer to:

- `Investigate non-traditional drivers of maize yield variability`
- or `Review microbiota and microclimate interaction hypothesis`

### 4. Open-question extraction is too raw

The extracted open questions were long paragraph fragments instead of compact unresolved questions.

For example, the system surfaced nearly raw sentences about:

- microbiological analysis
- microclimate sensor variation

Those should become tighter unresolved forms such as:

- `Are microbiota differences causal or only correlated with yield?`
- `Do microclimate effects depend on interaction terms rather than isolated values?`

### 5. Evidence linkage is structurally weak

After `bootstrap apply`, `ctx audit` in the demo repo reported:

- `MissingEvidence` on the promoted hypothesis

The reason is that the fallback evidence items were attached to the task, not to the hypothesis itself.

So the line looked populated, but the core hypothesis still had no direct supporting evidence.

### 6. Repeated source execution caused file-lock warnings

Running `dotnet run --project C:\sources\ctx-open\Ctx.Cli -- ...` repeatedly produced `MSB3026` copy warnings because `Ctx.Cli.exe` remained locked by previous processes during rebuild.

This does not invalidate the feature, but it does affect the realism and smoothness of local testing.

## Interpretive conclusion

The test confirms that bootstrap is already useful as:

- a provisional line opener
- a controlled first-pass mapper

But it is not yet good enough to claim robust idea-level synthesis on dense scientific or quasi-scientific texts.

The main weakness is not total failure.

The main weakness is partial flattening:

- it captures the existence of a cognitive thread
- but still phrases that thread too close to the source surface
- and under-compresses ambiguity into better reviewable hypotheses/questions

## What should change next

Priority improvements suggested by this test:

1. Add a stronger synthesis step for candidate hypotheses so they become shorter conceptual claims instead of long copied openings.
2. Improve thread-title generation so `apply` names the provisional line after the inferred problem, not after an internal generic label.
3. Convert paragraph-level uncertainty into compact open questions.
4. Ensure bootstrap evidence links to promoted hypotheses, not only to the review task fallback.
5. Publish bootstrap into the installed local CLI so testing no longer depends on the source tree.

## Operational conclusion

This demo should remain in `examples` as a regression case for bootstrap quality.

It is useful precisely because the article is:

- coherent enough to produce a line
- ambiguous enough to expose where current inference still overfits surface wording

## Closure state inside the demo repo

The document-review objective was closed inside the cognitive repo itself.

What happened in CTX:

- the original bootstrap-promoted paragraph hypothesis was archived
- a stronger synthesized hypothesis was added and supported with direct evidence
- an accepted decision recorded that the feature is useful but not release-ready
- an accepted conclusion closed the review task

Final semantic outcome of this demo:

- bootstrap indexing is validated as a useful provisional mapper
- bootstrap indexing is not yet validated as a strong synthesis layer for dense scientific text
- this example should now be used as a regression case whenever bootstrap extraction logic changes

---

## Second case: contradiction-heavy agricultural article

Second source artifact:

- [agricultural-systems-contradictions.md](C:/sources/ctx-open/examples/bootstrap-agriculture-demo/agricultural-systems-contradictions.md)

## Testing objective

Validate whether bootstrap can preserve a source whose central cognitive structure is not one dominant claim, but several coexisting interpretations that remain in tension.

This case is harder than the first one because the text explicitly says:

- microbiota may matter
- microclimate may matter
- neither may be sufficient alone
- the system may be governed by non-linear interactions and hidden variables

The goal of the test was not just to extract a line.

The goal was to see whether CTX bootstrap can keep contradiction alive without collapsing it prematurely into one sentence.

## What worked

- `bootstrap apply` again stayed conservative and did not flood the repo with dozens of entities.
- The review line was easy to inspect and revise inside CTX after bootstrap opened it.
- The article was strong enough for CTX review to recover three meaningful competing hypotheses from one provisional task.

## Real failures observed

### 1. The first pass collapsed multiple interpretations into one paragraph-level hypothesis

The source explicitly presents at least three interpretations:

- microbiota-first
- microclimate-first
- complex interaction / emergent system

But the first bootstrap pass promoted only one literal hypothesis, mostly mirroring the opening paragraph.

That means the mapper recognized a source anchor, but not the document's real cognitive shape.

### 2. Contradiction was not preserved as a first-class structure

The article repeatedly says the available evidence can support different readings depending on which data slice is emphasized.

Bootstrap did not preserve that ambiguity directly.

Instead, contradiction had to be reconstructed manually during review by:

- archiving the literal bootstrap hypothesis
- adding three synthesized hypotheses
- linking evidence to each one

### 3. Naming stayed generic again

The second bootstrap goal still opened as:

- `Bootstrap map: Bootstrap project map`

That is mechanically valid, but weak for review.

For contradiction-heavy texts, naming matters even more because the operator needs to see what kind of conflict is under evaluation.

### 4. The installed CLI gap still matters operationally

This second test still had to run from the source CLI DLL instead of the installed `ctx` binary.

That keeps testing real, but it also means field-style validation remains less smooth than it should be.

## CTX review outcome for the second case

The second article was closed inside the demo repo with this shape:

- the literal bootstrap hypothesis was archived
- three synthesized competing hypotheses were added
- each synthesized hypothesis received direct supporting evidence
- an accepted decision recorded that contradiction-heavy outputs must be treated as multi-hypothesis review inputs
- an accepted conclusion marked the article as a regression case for contradiction-aware bootstrap behavior

Recovered competing hypotheses:

1. Microbiota may influence yield, but not as a dominant standalone driver.
2. Microclimate may contribute, but current measurements do not make it a stable standalone predictor.
3. The stronger reading is a complex interaction model with hidden or structural variables.

## Interpretive conclusion

This second case is more important than the first one for product direction.

The first case showed that bootstrap can open a provisional line but still over-copy the source.

This second case shows a deeper limitation:

- bootstrap is not yet contradiction-aware
- it does not naturally preserve multiple live interpretations
- it still tends to flatten cognitive conflict into one promoted sentence

So the next quality bar is no longer just:

- better synthesis

It is also:

- better contradiction retention
- better coexistence of competing hypotheses
- better extraction of unresolved tension from dense text

## Operational conclusion

This article should remain in `examples` as a standing regression case for:

- multi-hypothesis preservation
- ambiguity handling
- contradiction-aware bootstrap review
- stronger provisional thread naming

If a future bootstrap revision can ingest this article and directly emit a compact multi-hypothesis provisional map without losing the conflict structure, that will represent a real quality jump.

