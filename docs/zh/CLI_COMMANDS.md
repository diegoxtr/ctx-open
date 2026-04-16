# CTX CLI å‘½ä»¤
如果语言模型及其代理丢失了上下文，这就是你需要的工具。

æœ¬æ–‡æ¡£æè¿° `C:\sources\ctx-open` ä¸­å½“å‰ CTX CLI çš„å‘½ä»¤è¡¨é¢ã€‚

CTX è¿”å›žç»“æž„åŒ– JSON è¾“å‡ºï¼ŒåŸºæœ¬æ ¼å¼ä¸ºï¼š

```json
{
  "success": true,
  "message": "short summary",
  "data": {}
}
```

## çº¦å®š

- ä»Žè®¤çŸ¥ä»“åº“ç›®å½•è¿è¡Œå‘½ä»¤ã€‚
- æœ¬åœ°å¼€å‘å¸¸ç”¨æ ¼å¼ï¼š

```powershell
dotnet run --project .\Ctx.Cli -- <command>
```

- å·²å‘å¸ƒæœ¬åœ°å®‰è£…ï¼š

```powershell
ctx <command>
```

- `<goalId>`, `<taskId>`, `<hypothesisId>`, `<commitId>` ç­‰æ¥è‡ªå…ˆå‰å‘½ä»¤è¾“å‡ºã€‚
- å¤šå€¼åˆ—è¡¨ä½¿ç”¨é€—å·åˆ†éš”ã€‚
- å˜æ›´é€šå¸¸å½±å“ `.ctx/working`ã€`.ctx/staging`ã€`.ctx/graph`ï¼Œå¹¶åœ¨åŽç»­è®¤çŸ¥æäº¤ä¸­è½ç›˜ã€‚

## é€šç”¨å‘½ä»¤

### `ctx`

æ— å‚æ•°æ—¶æ˜¾ç¤ºåŸºç¡€å¸®åŠ©ã€‚

```powershell
dotnet run --project .\Ctx.Cli --
```

### `ctx version`

æ˜¾ç¤ºäº§å“ç‰ˆæœ¬ä¸Žä»“åº“æ ¼å¼ç‰ˆæœ¬ã€‚

```powershell
dotnet run --project .\Ctx.Cli -- version
```

### `ctx init`

åœ¨å½“å‰ç›®å½•åˆå§‹åŒ–è®¤çŸ¥ä»“åº“ã€‚

é€‰é¡¹ï¼š
- `--name <project>`
- `--description <text>`
- `--branch <name>`

```powershell
dotnet run --project .\Ctx.Cli -- init --name "CTX Demo" --description "Sample repo" --branch main
```

### `ctx status`

æ˜¾ç¤ºå½“å‰ä»“åº“çŠ¶æ€ï¼š

- å½“å‰ branch
- `HEAD`
- `dirty` çŠ¶æ€
- goals/tasks/hypotheses/decisions/evidence/conclusions/runs è®¡æ•°

```powershell
dotnet run --project .\Ctx.Cli -- status
```

### `ctx doctor`

è¿è¡ŒçŽ¯å¢ƒè¯Šæ–­ï¼š

- äº§å“ç‰ˆæœ¬
- `.ctx/` æ˜¯å¦å­˜åœ¨
- `HEAD`
- working context
- metrics
- provider é…ç½®
- çŽ¯å¢ƒå‡­è¯

```powershell
dotnet run --project .\Ctx.Cli -- doctor
```

### `ctx audit`

è®¤çŸ¥ä¸€è‡´æ€§å®¡è®¡ã€‚ä¼šæ£€æŸ¥ï¼š

- æ—  hypothesis çš„ task
- `Done` ä½†æ—  Accepted conclusion çš„ task
- æ—  evidence çš„ hypothesis
- ä»…å…³è”å·²å…³é—­ task çš„å¼€æ”¾ hypothesis
- `Accepted` ä½†æ—  `rationale`/`evidence` çš„ decision
- å…³è” `Done` task çš„ `Draft` conclusion

```powershell
dotnet run --project .\Ctx.Cli -- audit
```

### `ctx next`

æ ¹æ®å½“å‰çŠ¶æ€æŽ¨èä¸‹ä¸€æ­¥ï¼š

- `Task`
- `Gap`

```powershell
dotnet run --project .\Ctx.Cli -- next
```

## è®¤çŸ¥å›¾è°±

### `ctx graph summary`

```powershell
dotnet run --project .\Ctx.Cli -- graph summary
```

### `ctx graph show <nodeId>`

```powershell
dotnet run --project .\Ctx.Cli -- graph show <hypothesisId>
dotnet run --project .\Ctx.Cli -- graph show Hypothesis:<hypothesisId>
```

### `ctx graph export`

```powershell
dotnet run --project .\Ctx.Cli -- graph export --format json
dotnet run --project .\Ctx.Cli -- graph export --format mermaid
dotnet run --project .\Ctx.Cli -- graph export --format json --commit <commitId>
```

### `ctx graph lineage`

```powershell
dotnet run --project .\Ctx.Cli -- graph lineage --goal <goalId>
dotnet run --project .\Ctx.Cli -- graph lineage --task <taskId>
dotnet run --project .\Ctx.Cli -- graph lineage --hypothesis <hypothesisId> --format mermaid
dotnet run --project .\Ctx.Cli -- graph lineage --decision <decisionId> --output .\tmp\decision-lineage.json
```

## çº¿ç¨‹é‡æž„

### `ctx thread reconstruct --task <id>`

```powershell
dotnet run --project .\Ctx.Cli -- thread reconstruct --task <taskId>
dotnet run --project .\Ctx.Cli -- thread reconstruct --task <taskId> --format markdown
```

## Goals / Tasks / Hypotheses / Evidence / Decisions / Conclusions

è¯·å‚è€ƒè‹±æ–‡ç‰ˆ [CLI_COMMANDS.md](C:/sources/ctx-open/docs/CLI_COMMANDS.md) çš„å®Œæ•´ç¤ºä¾‹ä¸Žé€‰é¡¹ã€‚

## å¯ç§»æ¤æ€§

```powershell
dotnet run --project .\Ctx.Cli -- export --output .\tmp\ctx-export.json
dotnet run --project .\Ctx.Cli -- import --input .\tmp\ctx-export.json
```

