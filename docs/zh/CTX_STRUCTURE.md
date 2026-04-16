# `.ctx/` å†…éƒ¨ç»“æž„
如果语言模型及其代理丢失了上下文，这就是你需要的工具。

æœ¬æ–‡æ¡£æè¿° CTX çš„æœ¬åœ°å­˜å‚¨ç»“æž„ã€‚

`.ctx/` æ˜¯æŒä¹…åŒ–åœ¨ç£ç›˜ä¸Šçš„è®¤çŸ¥ä»“åº“ï¼ŒåŒ…å«é…ç½®ã€æ´»åŠ¨çŠ¶æ€ã€å¯å¤çŽ°åŽ†å²ä¸Žè¿è¡Œäº§ç‰©ã€‚

## ç›®æ ‡

`.ctx/` ç»“æž„ç”¨äºŽï¼š

- æŒä¹…åŒ–ç»“æž„åŒ–æŽ¨ç†è€ŒéžåŽŸå§‹å¯¹è¯
- æ”¯æŒå¯å¤çŽ°çš„è®¤çŸ¥æäº¤
- å°†å·¥ä½œæ€ä¸Žä¸å¯å˜å¿«ç…§åˆ†ç¦»
- æ”¯æŒ branchingã€diff ä¸Ž merge
- è®°å½• runsã€packets å’Œ metrics
- æ”¯æŒ export/import/backup/audit

## æ¦‚è§ˆ

```
.ctx/
  version.json
  config.json
  project.json
  HEAD
  branches/
  commits/
  graph/
  working/
  staging/
  runs/
  packets/
  index/
  metrics/
  providers/
  logs/
```

éƒ¨åˆ†ç›®å½•ä¼šåœ¨å¯¹åº”æµç¨‹æ‰§è¡ŒåŽæ‰äº§ç”Ÿå†…å®¹ã€‚

## åŸºç¡€æ–‡ä»¶

- `version.json`: ä»“åº“æ ¼å¼ç‰ˆæœ¬
- `config.json`: ä»“åº“é…ç½®ï¼ˆprovidersã€tokensã€metricsï¼‰
- `project.json`: é¡¹ç›®å®žä½“
- `HEAD`: å½“å‰åˆ†æ”¯ä¸Žæœ€æ–°æäº¤æŒ‡é’ˆ

## ç›®å½•èŒè´£

- `branches/`: åˆ†æ”¯æŒ‡é’ˆ
- `commits/`: ä¸å¯å˜æäº¤å¿«ç…§
- `graph/`: å½“å‰è®¤çŸ¥å›¾æŠ•å½±
- `working/`: å¯å˜å·¥ä½œæ€
- `staging/`: æäº¤å‰å¿«ç…§
- `runs/`: AI è¿è¡Œè®°å½•
- `packets/`: ContextPacket
- `index/`: é¢„ç•™ç´¢å¼•
- `metrics/`: è¿è¡Œä¸Žæˆæœ¬ç»Ÿè®¡
- `providers/`: provider å…ƒæ•°æ®é¢„ç•™
- `logs/`: è¯Šæ–­æ—¥å¿—é¢„ç•™

## å…¸åž‹å†™å…¥æµç¨‹

1. `ctx init` åˆ›å»ºåŸºç¡€æ–‡ä»¶ä¸Žç›®å½•
2. æ—¥å¸¸å‘½ä»¤ä¸»è¦æ›´æ–° `working/` ä¸Ž `graph/`
3. `ctx commit` å†™å…¥ `commits/` å¹¶æ›´æ–° `HEAD`
4. `ctx export`/`ctx import` è¿›è¡Œè¿ç§»

## ç›¸å…³æ–‡æ¡£

- [CLI_COMMANDS.md](C:/sources/ctx-open/docs/CLI_COMMANDS.md)
- [INSTALLATION_AND_USAGE_GUIDE.md](C:/sources/ctx-open/docs/INSTALLATION_AND_USAGE_GUIDE.md)
- [V1_FUNCTIONAL_SPEC.md](C:/sources/ctx-open/docs/V1_FUNCTIONAL_SPEC.md)

