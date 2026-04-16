# CTX å®‰è£…ä¸Žä½¿ç”¨æŒ‡å—
如果语言模型及其代理丢失了上下文，这就是你需要的工具。

æœ¬æ–‡æ¡£è¯´æ˜Žå¦‚ä½•åœ¨æœ¬åœ°é¦–æ¬¡å®‰è£…ã€ç¼–è¯‘å¹¶ä½¿ç”¨ CTXã€‚

é€‚ç”¨äººç¾¤ï¼š

- æŠ€æœ¯æµ‹è¯•äººå‘˜
- å¼€å‘è€…
- æž¶æž„å¸ˆ
- V1 æ—©æœŸç”¨æˆ·

## åŸºæœ¬è¦æ±‚

- Windows ç»ˆç«¯
- .NET SDK 8
- ä»“åº“ä»£ç è®¿é—®æƒé™
- å·¥ä½œç›®å½•è¯»å†™æƒé™

å¯é€‰ï¼š

- `OPENAI_API_KEY`
- `ANTHROPIC_API_KEY`

æœªé…ç½®å¯†é’¥ä¹Ÿå¯ä½¿ç”¨ç¦»çº¿ fallback è¿›è¡ŒåŠŸèƒ½æµ‹è¯•ã€‚

## å…³é”®æ­¥éª¤

1. éªŒè¯ .NETï¼š

```powershell
dotnet --version
```

2. æ‹‰å–ä»£ç å¹¶è¿›å…¥æ ¹ç›®å½•ï¼š

```powershell
cd C:\sources\ctx-open
```

3. æ¢å¤ã€æž„å»ºã€æµ‹è¯•ï¼š

```powershell
dotnet restore Ctx.sln
dotnet build Ctx.sln
dotnet test .\Ctx.Tests\Ctx.Tests.csproj
```

4. è¿è¡Œ CLIï¼š

```powershell
dotnet run --project .\Ctx.Cli -- status
```

5. åˆå§‹åŒ–ä»“åº“ï¼š

```powershell
dotnet run --project C:\sources\ctx-open\Ctx.Cli -- init --name "CTX-DEMO" --description "First cognitive repo"
```

## æŽ¨èé¦–ä¸ªæµç¨‹

- `goal add`
- `task add`
- `hypo add`
- `evidence add`
- `decision add`
- `conclusion add`
- `run`
- `commit`

è¯¦ç»†ç¤ºä¾‹è¯·å‚è€ƒè‹±æ–‡ç‰ˆï¼š

- [INSTALLATION_AND_USAGE_GUIDE.md](C:/sources/ctx-open/docs/INSTALLATION_AND_USAGE_GUIDE.md)

