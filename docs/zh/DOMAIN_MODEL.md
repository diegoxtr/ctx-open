# CTX é¢†åŸŸæ¨¡åž‹
如果语言模型及其代理丢失了上下文，这就是你需要的工具。

æœ¬æ–‡æ¡£æè¿° CTX å½“å‰çš„é¢†åŸŸæ¨¡åž‹ã€‚

ç›®æ ‡æ˜¯æ˜Žç¡®ï¼š

- å®žä½“
- å¼ºæ ‡è¯†ç¬¦
- å…³ç³»
- ç”Ÿå‘½å‘¨æœŸçŠ¶æ€
- å¯è¿½æº¯æ€§è§„åˆ™
- å…³é”®èšåˆä¸Žè¿è¡Œäº§ç‰©

CTX ä¸æŠŠå¯¹è¯ä½œä¸ºä¸»è¦æ¥æºï¼Œè€Œæ˜¯å»ºæ¨¡ç»“æž„åŒ–è®¤çŸ¥äº§ç‰©ã€‚

## å…³é”®åŽŸåˆ™

- ä¿¡æ¯å¿…é¡»ç»“æž„åŒ–
- é‡è¦äº§ç‰©å¿…é¡»æœ‰å¼ºèº«ä»½
- å†³ç­–å¿…é¡»æ˜¾å¼
- è¯æ®å¯å¼•ç”¨
- æäº¤å¯å¤çŽ°
- æŽ¨ç†æ¼”åŒ–å¯æ¯”è¾ƒ
- å·¥ä½œæ€ä¸ŽåŽ†å²åˆ†ç¦»

## å¼ºæ ‡è¯†ç¬¦

æ ¸å¿ƒå®žä½“ä½¿ç”¨ typed IDï¼ˆ`record struct`ï¼‰ï¼Œå¦‚ï¼š

- `ProjectId`, `GoalId`, `TaskId`, `HypothesisId`
- `DecisionId`, `EvidenceId`, `ConclusionId`
- `RunId`, `ContextCommitId`, `ContextPacketId`, `WorkingContextId`

## Traceability

æ ¸å¿ƒå­—æ®µï¼š

- `CreatedBy`, `CreatedAtUtc`
- `UpdatedBy`, `UpdatedAtUtc`
- `Tags`, `RelatedIds`

## æ ¸å¿ƒå®žä½“ï¼ˆæ‘˜è¦ï¼‰

- `Project`: ä»“åº“æ ¹èº«ä»½
- `Goal`: é«˜å±‚ç›®æ ‡
- `Task`: å…·ä½“å·¥ä½œå•å…ƒï¼ˆ`Draft/Ready/InProgress/Blocked/Done`ï¼‰
- `Hypothesis`: å¯éªŒè¯å‡è®¾ï¼ˆ`Proposed/UnderEvaluation/Supported/Refuted/Archived`ï¼‰
- `Decision`: æ˜¾å¼å†³ç­–ï¼ˆ`Proposed/Accepted/Rejected/Superseded`ï¼‰
- `Evidence`: å¯è¿½æº¯è¯æ®ï¼ˆå« `Kind`ï¼‰
- `Conclusion`: æ”¶æ•›ç»“è®ºï¼ˆ`Draft/Accepted/Superseded`ï¼‰
- `Run`: AI æ‰§è¡Œè®°å½•
- `ContextPacket`: å‘é€ç»™ provider çš„ä¸Šä¸‹æ–‡åŒ…
- `WorkingContext`: å¯å˜å·¥ä½œæ€
- `ContextCommit`: ä¸å¯å˜å¿«ç…§

## é‡è¦å…³ç³»

- `Goal` èšåˆ `Task`
- `Task` é“¾æŽ¥ `Hypothesis`
- `Hypothesis` ç”± `Evidence` æ”¯æ’‘
- `Decision` é“¾æŽ¥ `Hypothesis` ä¸Ž `Evidence`
- `Conclusion` é“¾æŽ¥ `Decision` ä¸Ž `Evidence`
- `Run` å…³è” `ContextPacket`

## å®Œæ•´ç»†èŠ‚

æ›´å¤šå­—æ®µã€çŠ¶æ€ä¸Ž diff/merge ç»“æž„è¯·è§è‹±æ–‡ç‰ˆï¼š

- [DOMAIN_MODEL.md](C:/sources/ctx-open/docs/DOMAIN_MODEL.md)

