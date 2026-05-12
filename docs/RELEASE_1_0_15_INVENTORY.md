# CTX 1.0.15 Public Sync Inventory

Source inventory: release draft, reviewed before public release preparation.

Scope rule: promote shipped items into public code, docs, changelog, release notes, and packaged documentation. Keep local CTX state, unpublished planning notes, and machine-specific paths out of the public repository.

## Inventory Matrix

| Private inventory line | Public status | Public surface |
|---|---|---|
| `ctx update` and alias `ctx -update` | Added in 1.0.15 | `Ctx.Cli`, `docs/CLI_COMMANDS.md`, localized CLI docs, tests |
| GitHub Release comparison fields | Added in 1.0.15 | `ReleaseUpdateChecker`, release notes, tests |
| Update command owner/repository overrides | Added in 1.0.15 | `ctx update --owner`, `--repo`, `--repository`, env defaults |
| Viewer native ES module entrypoint | Added in 1.0.15 | `/app.mjs`, `index.html`, contract tests |
| Viewer API client extraction | Added in 1.0.15 | `/js/viewer-api.mjs`, contract tests |
| Viewer storage extraction | Added in 1.0.15 | `/js/viewer-storage.mjs`, contract tests |
| Viewer workspace-tab helpers | Added in 1.0.15 | `/js/viewer-workspaces.mjs`, contract tests |
| Viewer history helpers | Added in 1.0.15 | `/js/viewer-history.mjs`, contract tests |
| Viewer utility helpers | Added in 1.0.15 | `/js/viewer-utils.mjs`, contract tests |
| Compare from History rows | Added in 1.0.15 | Viewer History controls and `ViewerAppContractTests` |
| Compare Graph dedicated surface | Added in 1.0.15 | Viewer graph canvas, tests, release notes |
| Compare Graph zoom stability | Added in 1.0.15 | Viewer contract tests |
| Compare Graph Back to Trace Graph action | Added in 1.0.15 | Viewer contract tests |
| Compare Graph selectable nodes | Added in 1.0.15 | Details-panel contract tests |
| Diff summary strip | Added in 1.0.15 | Viewer contract tests |
| Single graph scroll surface | Added in 1.0.15 | Viewer contract tests |
| Resizable graph viewport | Added in 1.0.15 | Viewer contract tests |
| Public docs for `ctx update` | Added in 1.0.15 | English, Spanish, and Chinese CLI references |
| Public release version surfaces | Added in 1.0.15 | `Directory.Build.props`, `ProductVersion`, README, installer metadata |
| Public release notes | Added in 1.0.15 | `docs/RELEASE_1_0_15.md` |
| Changelog entry | Added in 1.0.15 | `CHANGELOG.md` |
| Static live-demo version references | Added in 1.0.15 | `docs/live-demo/*`, `docs/LIVE_DEMO.md`, `docs/TECHNICAL_INDEX.md` |
| Viewer MCP packaged server detection | Already public in 1.0.14 | `docs/RELEASE_1_0_14.md`, packaged MCP fix |
| Explicit Viewer MCP Start/Stop controls | Already public in 1.0.14 | Viewer header and MCP local docs |
| Release bell and `/api/release-status` | Already public before 1.0.15 | Viewer guide and release history |
| Epics, roadmap, and gaps command surface | Already public before 1.0.15 | README, CLI docs, MCP parity docs |
| `ctx prompt list` / `ctx prompts` | Already public before 1.0.15 | CLI docs and command coverage |
| Runbook selection and `ctx plan` contract | Already public before 1.0.15 | README, CLI docs, tests |
| Working-context lock retry behavior | Already public before 1.0.15 | operational docs and tests |
| Viewer progressive graph rendering | Already public before 1.0.15 | Viewer release history and tests |
| Viewer zoom in-place updates | Already public before 1.0.15 | Viewer release history and tests |
| Parked epics rail and origin navigation | Already public before 1.0.15 | Viewer release history |
| Repository Browse behavior | Already public before 1.0.15 | README and Viewer docs |
| Packaged helper/doc asset guardrail | Already public before 1.0.15 | installer and distribution docs |
| Unpublished multilingual documentation audit notes | Excluded from 1.0.15 public release | release planning only |
| Local self-hosting validation paths | Excluded from public release | replaced by placeholders or public demo paths |
| Local `.ctx` state | Excluded from public release | verified by Git status and tracked-file scan |
| Machine-specific checkout paths | Excluded from public release | verified by text scan |

## Documentation Verification

Release documentation was checked across:

- `README.md`
- `CHANGELOG.md`
- `docs/RELEASE_1_0_15.md`
- `docs/CLI_COMMANDS.md`
- `docs/es/CLI_COMMANDS.md`
- `docs/zh/CLI_COMMANDS.md`
- `docs/LIVE_DEMO.md`
- `docs/TECHNICAL_INDEX.md`
- `docs/live-demo/index.html`
- `docs/live-demo/mcp-local.html`
- `docs/live-demo/notes.html`
- `docs/live-demo/talk-unju.html`

Expected historical references:

- `1.0.14` remains only in historical changelog and `docs/RELEASE_1_0_14.md`.
- Older `app.js` references remain only in historical release notes or older diagram examples, not in the current Viewer entrypoint.

Release blockers:

- Do not publish if `.ctx` state is staged.
- Do not publish if machine-specific paths appear in current release docs or source.
- Do not publish if `ctx version`, `ctx update`, build, or tests fail on the release branch.
