# CTX è¯•ç‚¹æµ‹è¯•æŒ‡å—
如果语言模型及其代理丢失了上下文，这就是你需要的工具。

## 1. ç›®æ ‡

æœ¬æŒ‡å—å®šä¹‰å¦‚ä½•è¿è¡Œåˆå§‹ CTX è¯•ç‚¹ï¼Œä»¥éªŒè¯è¯¥æ–¹æ¡ˆæ˜¯å¦è¶³å¤Ÿæˆç†Ÿï¼Œèƒ½å¤Ÿæˆä¸ºæŠ€æœ¯ç”¨æˆ·çš„ V1 å€™é€‰ã€‚

ç›®æ ‡ä¸ä»…æ˜¯éªŒè¯è½¯ä»¶èƒ½è¿è¡Œï¼Œè¿˜è¦ç¡®è®¤ï¼š

- å·¥ä½œæ¨¡åž‹å¯ç†è§£
- æ“ä½œæµç¨‹æœ‰çœŸå®žä»·å€¼
- è®¤çŸ¥ç»“æž„æ”¹å–„ AI ä½¿ç”¨
- å·¥å…·å‡å°‘è¿”å·¥
- è¿­ä»£æˆæœ¬åˆç†

## 2. è¯•ç‚¹èŒƒå›´

è¯•ç‚¹åº”èšç„¦ï¼š

- æœ¬åœ° CLI ä½¿ç”¨
- çœŸå®žæˆ–æŽ¥è¿‘çœŸå®žçš„æŠ€æœ¯åœºæ™¯
- ç»“æž„åŒ–è®°å½• goalsã€tasksã€hypothesesã€evidenceã€decisionsã€conclusions
- è®¤çŸ¥æäº¤
- ä¸Šä¸‹æ–‡ç”Ÿæˆ
- runs
- diff ä¸Ž merge æ£€æŸ¥
- metricsã€packets ä¸Ž runs çš„æŸ¥çœ‹

æš‚ä¸åŒ…å«ï¼š

- è¿œç¨‹é›†æˆ
- çœŸå®žå¤šç”¨æˆ·ä½¿ç”¨
- å›¾å½¢ç•Œé¢
- ä¼ä¸šåœºæ™¯
- å¤æ‚å¤–éƒ¨è‡ªåŠ¨åŒ–

## 3. æŽ¨èæµ‹è¯•è€…ç”»åƒ

ç†æƒ³æµ‹è¯•è€…ï¼š

- æŠ€æœ¯äººå‘˜
- ç†Ÿæ‚‰ CLI
- èƒ½å°†æŽ¨ç†ç»“æž„åŒ–ä¸ºæ­¥éª¤
- èƒ½è§£é‡Šå†³ç­–å¥½ååŽŸå› 
- æ„¿æ„è®°å½•æ˜Žç¡®åé¦ˆ

æ›´ç†æƒ³ï¼š

- æž¶æž„å¸ˆ
- é«˜çº§å¼€å‘è€…
- å¹³å°å·¥ç¨‹å¸ˆ
- æŠ€æœ¯ç ”ç©¶è€…
- é«˜é¢‘ LLM ä½¿ç”¨è€…

## 4. çŽ¯å¢ƒå‡†å¤‡

### 4.1 è¦æ±‚

- å®‰è£… .NET SDK 8
- CTX ä»“åº“å¯æž„å»º
- å…·å¤‡ç»ˆç«¯è®¿é—®
- å¯é€‰æä¾›çœŸå®ž runs çš„ provider keyï¼š
  - `OPENAI_API_KEY`
  - `ANTHROPIC_API_KEY`

æ—  key ä¹Ÿå¯é€šè¿‡ç¦»çº¿å›žé€€æµ‹è¯•ã€‚

### 4.2 åˆå§‹éªŒè¯

```powershell
dotnet build Ctx.sln
dotnet test .\Ctx.Tests\Ctx.Tests.csproj
```

æœŸæœ›ï¼š

- æž„å»ºæˆåŠŸ
- æµ‹è¯•é€šè¿‡
- æ— å…³é”®çŽ¯å¢ƒé”™è¯¯

### 4.3 å‡†å¤‡è¯•ç‚¹ä»“åº“

```powershell
dotnet run --project .\Ctx.Cli -- init --name CTX-PILOT --description "Initial validation pilot"
dotnet run --project .\Ctx.Cli -- status
```

æœŸæœ›ï¼š

- åˆ›å»º `.ctx/`
- `status` æ˜¾ç¤º `main`
- æ— åˆå§‹åŒ–é”™è¯¯

## 5. è¯•ç‚¹ç›®æ ‡é—®é¢˜

è¯•ç‚¹æœŸé—´å›žç­”ï¼š

1. ç”¨æˆ·æ˜¯å¦ç†è§£å¦‚ä½•å»ºæ¨¡é—®é¢˜
2. ç»“æž„åŒ–æµç¨‹æ˜¯å¦æ¯”è‡ªç”±èŠå¤©æ›´æœ‰å¸®åŠ©
3. æ˜¯å¦æ›´å®¹æ˜“æ¢å¤å·¥ä½œ
4. æäº¤ä¸Ž diff æ˜¯å¦æœ‰ç”¨
5. åˆå¹¶ä¸Žå†²çªæ˜¯å¦æ˜“äºŽç†è§£
6. context/run æˆæœ¬æ˜¯å¦å¯æŽ¥å—
7. ç”¨æˆ·æ˜¯å¦ä¼šå°† CTX ç”¨äºŽçœŸå®žåœºæ™¯

## 6. æŽ¨èåœºæ™¯

### åœºæ™¯ 1 - æž¶æž„åˆ†æž

ç›®æ ‡ï¼š

- ç”¨å‡è®¾ä¸Žè¯æ®è¯„ä¼°æž¶æž„å†³ç­–

### åœºæ™¯ 2 - æŠ€æœ¯è°ƒæŸ¥

ç›®æ ‡ï¼š

- ç»“æž„åŒ–æ ¹å› è°ƒæŸ¥

### åœºæ™¯ 3 - AI å¼•å¯¼è¿­ä»£

ç›®æ ‡ï¼š

- è¡¡é‡ CTX æ˜¯å¦é™ä½Žå¤šæ¬¡ AI è¿­ä»£çš„è¿”å·¥

## 7. æŽ¨èæµ‹è¯•æµç¨‹

### æ­¥éª¤ 1 - åˆ›å»ºç›®æ ‡

```powershell
dotnet run --project .\Ctx.Cli -- goal add --title "Evaluate module X architecture" --description "Define primary alternative"
```

### æ­¥éª¤ 2 - åˆ›å»ºä»»åŠ¡

```powershell
dotnet run --project .\Ctx.Cli -- task add --title "Analyze option A" --description "Pros and risks"
dotnet run --project .\Ctx.Cli -- task add --title "Analyze option B" --description "Pros and risks"
```

### æ­¥éª¤ 3 - è®°å½•å‡è®¾

```powershell
dotnet run --project .\Ctx.Cli -- hypo add --statement "Option A reduces operational complexity" --rationale "Fewer components"
dotnet run --project .\Ctx.Cli -- hypo add --statement "Option B scales better long term" --rationale "More flexibility"
```

### æ­¥éª¤ 4 - è®°å½•è¯æ®

```powershell
dotnet run --project .\Ctx.Cli -- evidence add --title "Initial benchmark" --summary "A shows lower latency" --source "local test" --kind Benchmark --supports hypothesis:<hypothesisId>
```

### æ­¥éª¤ 5 - åšå‡ºå†³ç­–

```powershell
dotnet run --project .\Ctx.Cli -- decision add --title "Adopt option A for pilot" --rationale "Lower complexity and favorable evidence" --state Accepted --hypotheses <hypothesisId> --evidence <evidenceId>
```

### æ­¥éª¤ 6 - è®°å½•ç»“è®º

```powershell
dotnet run --project .\Ctx.Cli -- conclusion add --summary "Proceed with A for faster validation" --state Accepted --decisions <decisionId> --evidence <evidenceId>
```

### æ­¥éª¤ 7 - æ‰§è¡Œä¸€æ¬¡ run

```powershell
dotnet run --project .\Ctx.Cli -- run --provider openai --purpose "Review decision and propose risks"
```

### æ­¥éª¤ 8 - åˆ›å»ºè®¤çŸ¥æäº¤

```powershell
dotnet run --project .\Ctx.Cli -- commit -m "pilot architecture scenario"
```

### æ­¥éª¤ 9 - æ£€æŸ¥ç»“æžœ

```powershell
dotnet run --project .\Ctx.Cli -- log
dotnet run --project .\Ctx.Cli -- metrics show
dotnet run --project .\Ctx.Cli -- run list
dotnet run --project .\Ctx.Cli -- packet list
```

## 8. æµ‹è¯•è€…æ¸…å•

è¯•ç‚¹ä¸­æ ‡è®°ï¼š

- æˆ‘èƒ½åœ¨æ— å¸®åŠ©ä¸‹åˆå§‹åŒ–ä»“åº“
- æˆ‘ç†è§£å¦‚ä½•åˆ›å»º goalsã€tasksã€hypotheses
- æˆ‘ç†è§£å¦‚ä½•å°† evidence å…³è”åˆ° hypotheses æˆ– decisions
- æˆ‘æ˜Žç¡®å¦‚ä½•è®°å½• decisions å’Œ conclusions
- `status`ã€`log`ã€`diff`ã€`metrics` è¾“å‡ºå¯ç†è§£
- ç”Ÿæˆçš„ packets æœ‰ç”¨
- run è¾“å‡ºå¯å¤ç”¨
- è®¤çŸ¥æäº¤å‡†ç¡®ä»£è¡¨è¾¾æˆçŠ¶æ€
- è¯¥æµç¨‹ä¼˜äºŽéšæ„æç¤º
- æˆ‘ä¼šåœ¨çœŸå®žåœºæ™¯ä¸­ä½¿ç”¨ CTX

## 9. è¯„ä¼°æ ‡å‡†

### 9.1 åŠŸèƒ½

æˆåŠŸæ ‡å‡†ï¼š

- æµç¨‹å¯å®Œæˆä¸”æ— éœ€æ‰‹åŠ¨ç¼–è¾‘ JSON
- æ— é˜»å¡žé”™è¯¯
- å‘½ä»¤è¿”å›žä¸€è‡´ç»“æžœ
- å·¥ä»¶å¯è¿½è¸ª

### 9.2 å¯ç”¨æ€§

å¯æŽ¥å—æ ‡å‡†ï¼š

- åœ¨æœ‰é™è¯´æ˜Žä¸‹æ¨¡åž‹å¯ç†è§£
- CLI ä¸é€ æˆä¸¥é‡å›°æƒ‘
- å‘½ä»¤å‘½ååˆç†
- ç»“æž„åŒ–ä¿ƒè¿›æ›´å¥½æ€è€ƒä¸”æ‘©æ“¦å¯æŽ¥å—

### 9.3 ä»·å€¼

æœ‰å‰æ™¯çš„æ ‡å‡†ï¼š

- ç”¨æˆ·æ„Ÿè§‰ä¸Šä¸‹æ–‡ä¸¢å¤±å‡å°‘
- å†³ç­–æ›´æ˜“è§£é‡Š
- æš‚åœåŽæ˜“äºŽæ¢å¤
- è®¤çŸ¥æäº¤æœ‰ç”¨
- AI é‡å¤è¿­ä»£å‡å°‘

## 10. éœ€è¦è®°å½•çš„æŒ‡æ ‡

æ¯ä¸ªåœºæ™¯è®°å½•ï¼š

- æ€»æ‰§è¡Œæ—¶é—´
- å‘½ä»¤æ•°é‡
- goals/tasks/hypotheses/evidence/decisions/conclusions æ•°é‡
- run æ•°é‡
- token ä½¿ç”¨é‡
- æˆæœ¬ä¼°ç®—
- é‡å¤è¿­ä»£æ¬¡æ•°
- æ£€æµ‹åˆ°çš„è®¤çŸ¥å†²çª
- ä¸»è§‚æœ‰ç”¨æ€§

## 11. æµ‹è¯•è€…åé¦ˆæ¨¡æ¿

é€šç”¨ä¿¡æ¯ï¼š

- æµ‹è¯•è€…å§“å
- æ—¥æœŸ
- åœºæ™¯
- æ—¶é•¿

è¯„ä¼°ï¼š

- é—®é¢˜æ˜¯å¦æœ‰æ•ˆ
- ç›®æ ‡æ˜¯å¦è¾¾æˆ
- æœ€æœ‰ç”¨çš„å‘½ä»¤
- æœ€ä»¤äººå›°æƒ‘çš„å‘½ä»¤
- æµç¨‹ä¸­æœ€æœ‰ä»·å€¼çš„éƒ¨åˆ†
- æµç¨‹ä¸­æ‘©æ“¦æœ€å¤§å¤„
- ç¼ºå¤±ä¿¡æ¯
- ç¼ºå¤±å‘½ä»¤/å¸®åŠ©
- æ˜¯å¦ä¼šåœ¨çœŸå®žåœºæ™¯ä½¿ç”¨ CTXï¼šæ˜¯/å¦

å»ºè®® 1-5 åˆ†ï¼š

- æ¨¡åž‹æ¸…æ™°åº¦
- ä½¿ç”¨æ˜“ç”¨æ€§
- ç»“æž„åŒ–ä¸Šä¸‹æ–‡ä»·å€¼
- è®¤çŸ¥æäº¤ä»·å€¼
- diff ä»·å€¼
- merge ä»·å€¼
- metrics ä»·å€¼
- å¤ç”¨å¯èƒ½æ€§

## 12. è¯•ç‚¹åŽå†³ç­–

### è¿›å…¥ V1 å€™é€‰

è‹¥ï¼š

- æ— ä¸¥é‡é˜»å¡ž
- æµ‹è¯•è€…ç†è§£æµç¨‹
- ä»·å€¼æ„Ÿé«˜
- æˆæœ¬åˆç†

### ä¿æŒå†…éƒ¨è¿­ä»£

è‹¥ï¼š

- æ¦‚å¿µå—æ¬¢è¿Ž
- CLI/UX æ‘©æ“¦ä»åé«˜

### V1 å‰é‡è¯„

è‹¥ï¼š

- ä»·å€¼æ„Ÿä½Ž
- æ¨¡åž‹ä¸æ¸…æ™°
- æ“ä½œæˆæœ¬è¿‡é«˜
- å·¥ä»¶ä¸èƒ½æ”¹è¿›å†³ç­–

## 13. å®žé™…å»ºè®®

åˆå§‹è¯•ç‚¹å»ºè®®ï¼š

- 3 ä¸ªåœºæ™¯
- 3-5 ä½æŠ€æœ¯æµ‹è¯•è€…
- çŸ­å‘¨æœŸ
- å¿…é¡»ä¹¦é¢åé¦ˆ
- æœ€ç»ˆå¤ç›˜ä¼šè®®

## 14. é¢„æœŸç»“æžœ

æ‰§è¡Œå¾—å½“å¯èŽ·å¾—ï¼š

- ä»·å€¼ä¸Žæ‘©æ“¦çš„å…·ä½“è¯æ®
- V1 å‰çœŸå®žæ”¹è¿›æ¸…å•
- åˆ¤æ–­ CTX æ˜¯å¦è¿›å…¥å—æŽ§äº§å“æµ‹è¯•çš„å®¢è§‚ä¾æ®

