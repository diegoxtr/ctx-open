# CTX æŠ€æœ¯æž¶æž„
如果语言模型及其代理丢失了上下文，这就是你需要的工具。

æœ¬æ–‡æ¡£æè¿° CTX å½“å‰æŠ€æœ¯æž¶æž„ã€‚

æ ¸å¿ƒç›®æ ‡ï¼š

- ç³»ç»Ÿç»„ç»‡æ–¹å¼
- å„å±‚èŒè´£
- ç»„ä»¶è¿žæŽ¥æ–¹å¼
- CLI åˆ°æŒä¹…åŒ–ä¸Ž provider çš„æ‰§è¡Œæµ

CTX åŸºäºŽ `.NET 8`ï¼Œé‡‡ç”¨ Clean Architecture/DDD é£Žæ ¼çš„æ¨¡å—åŒ–ç»“æž„ã€‚

## åˆ†å±‚æ¦‚è§ˆ

- `Ctx.Domain`
- `Ctx.Application`
- `Ctx.Core`
- `Ctx.Persistence`
- `Ctx.Providers`
- `Ctx.Infrastructure`
- `Ctx.Cli`
- `Ctx.Tests`

åŸºæœ¬è§„åˆ™ï¼š

- Domain å®šä¹‰è¯­è¨€ä¸Žç±»åž‹
- Application å®šä¹‰åˆåŒä¸Žç”¨ä¾‹
- Core å®žçŽ°å…³é”®é€»è¾‘
- Persistence å®žçŽ°æœ¬åœ°å­˜å‚¨
- Providers å®žçŽ° LLM é›†æˆ
- Infrastructure è´Ÿè´£ä¾èµ–ç»„è£…
- CLI æš´éœ²ç”¨æˆ·æŽ¥å£

## å…³é”®ç‚¹

- `Ctx.Domain` æ— ä¾èµ–
- `Ctx.Application` ä¾èµ– Domain
- `Ctx.Core` å®žçŽ° Application æŽ¥å£
- `Ctx.Persistence` æä¾›æ–‡ä»¶ç³»ç»Ÿä»“åº“
- `Ctx.Providers` å¯äº’æ¢ providers
- `Ctx.Infrastructure` ç»„è£…å®žçŽ°
- `Ctx.Cli` æ¶ˆè´¹ `ICtxApplicationService`

## ç«¯åˆ°ç«¯æµç¨‹æ‘˜è¦

- `ctx init` åˆå§‹åŒ–ä»“åº“
- `ctx goal add` æ›´æ–° working/graph
- `ctx context` æž„å»º ContextPacket
- `ctx run` æ‰§è¡Œ provider å¹¶è®°å½• metrics
- `ctx commit` ç”Ÿæˆä¸å¯å˜å¿«ç…§ä¸Ž diff
- `ctx diff` æ¯”è¾ƒè®¤çŸ¥çŠ¶æ€
- `ctx merge` åˆå¹¶åˆ†æ”¯å¹¶è¾“å‡ºè®¤çŸ¥å†²çª

## ç›¸å…³å‚è€ƒ

- [DOMAIN_MODEL.md](C:/sources/ctx-open/docs/DOMAIN_MODEL.md)
- [CTX_STRUCTURE.md](C:/sources/ctx-open/docs/CTX_STRUCTURE.md)
- [CLI_COMMANDS.md](C:/sources/ctx-open/docs/CLI_COMMANDS.md)

