# CTX MCP Server Proposal

## Objetivo

Exponer CTX a agentes mediante Model Context Protocol sin duplicar la logica del CLI, del dominio ni de persistencia.

El servidor MCP debe ser una capa adaptadora:

```text
Agente / IDE / Cliente MCP
        |
        | MCP
        v
Ctx.Mcp
        |
        v
Ctx.Application / Ctx.Infrastructure / Ctx.Persistence
        |
        v
.ctx
```

## Decision propuesta

Crear un proyecto nuevo dentro de la solucion:

```text
Ctx.Mcp/
  Ctx.Mcp.csproj
  Program.cs
  Mcp/
    CtxMcpOptions.cs
    RepositoryGuard.cs
    CtxReadTools.cs
    CtxWriteTools.cs
```

No crear un repositorio separado todavia. El MCP necesita reutilizar los contratos y servicios ya existentes; separarlo antes de estabilizar el contrato aumentaria friccion y duplicacion.

## Paquete base

Usar el SDK oficial C# de MCP:

```xml
<PackageReference Include="ModelContextProtocol" Version="1.2.0" />
```

Este paquete es el recomendado para servidores MCP stdio con hosting, DI y descubrimiento de tools/resources. `ModelContextProtocol.AspNetCore` queda reservado para una etapa posterior con HTTP.

Fuentes:

- https://github.com/modelcontextprotocol/csharp-sdk
- https://csharp.sdk.modelcontextprotocol.io/concepts/getting-started.html
- https://www.nuget.org/packages/ModelContextProtocol/1.2.0

## Alcance del MVP

El MVP debe ser local, stdio y read-first.

Transporte:

```text
stdio
```

Uso esperado:

```powershell
dotnet run --project .\Ctx.Mcp -- --repo C:\\sources\\ctx-open
```

O instalado:

```powershell
C:\ctx\bin\ctx-mcp.cmd --repo C:\\sources\\ctx-open
```

## Tools iniciales

### Read-only

Estas tools no mutan `.ctx` y deberian ser la primera version funcional:

```text
ctx_status(repo?)
ctx_audit(repo?)
ctx_next(repo?)
ctx_context(repo?, goalId?, taskId?, purpose?)
ctx_graph_summary(repo?)
ctx_graph_show(repo?, nodeId)
ctx_thread_reconstruct(repo?, taskId, format?)
ctx_preflight(repo?, operation, goalId?, taskId?)
```

Todas llaman a `ICtxApplicationService`.

Ejemplo:

```text
ctx_status -> ICtxApplicationService.StatusAsync(repositoryPath)
ctx_next -> ICtxApplicationService.NextAsync(repositoryPath)
ctx_graph_summary -> ICtxApplicationService.GraphSummaryAsync(repositoryPath)
```

## Tools de escritura

Las mutaciones ya existen, pero solo se habilitan cuando el servidor se inicia con `--mode write`:

```text
ctx_goal_add
ctx_line_open
ctx_task_add
ctx_task_update
ctx_hypothesis_add
ctx_hypothesis_update
ctx_evidence_add
ctx_evidence_share
ctx_decision_add
ctx_decision_update
ctx_conclusion_add
ctx_conclusion_update
ctx_closeout
ctx_commit
```

Regla operativa: antes de `ctx_commit`, el agente debe llamar `ctx_check` y `ctx_closeout` para verificar que el hilo cognitivo tiene cierre suficiente y que el delta pendiente es coherente.

## Resources

Ademas de tools, exponer recursos MCP para lectura estable:

```text
ctx://status
ctx://audit
ctx://next
ctx://graph/summary
ctx://task/{id}
ctx://hypothesis/{id}
ctx://decision/{id}
ctx://conclusion/{id}
ctx://thread/task/{id}
```

Resources deben ser read-only. Si una operacion muta estado, debe ser tool, no resource.

## Guardrails

### Boundary de repositorio

El servidor debe operar sobre un `RepositoryRoot` fijo.

```text
--repo <path>
```

Si una tool recibe `repo`, solo se acepta si:

1. resuelve a path absoluto;
2. existe;
3. contiene `.ctx` cuando la tool requiere repo inicializado;
4. esta bajo una raiz permitida si se configura `--allow-root`;
5. no usa traversal para escapar del limite.

Para el MVP, preferir un solo repo fijo y no aceptar `repo` dinamico salvo para tools read-only.

### Modos

```text
--mode read-only
--mode write
```

Default:

```text
read-only
```

En `read-only`, las tools mutantes no se registran o devuelven error claro.

### Bootstrap

`bootstrap apply` no entra en el primer MVP. Cuando entre:

```text
ctx_bootstrap_map(repo?, from, mode?, maxFiles?)
ctx_bootstrap_apply(repo?, from, mode?, maxFiles?, parentGoal?)
```

Restricciones:

- `from` debe resolverse dentro del repo;
- `maxFiles` debe tener limite bajo por defecto;
- la respuesta debe advertir si se mapearon archivos de instrucciones operativas.

## Integracion con la solucion actual

`Ctx.Mcp` referencia las mismas capas que `Ctx.Cli`:

```xml
<ProjectReference Include="..\Ctx.Domain\Ctx.Domain.csproj" />
<ProjectReference Include="..\Ctx.Application\Ctx.Application.csproj" />
<ProjectReference Include="..\Ctx.Core\Ctx.Core.csproj" />
<ProjectReference Include="..\Ctx.Persistence\Ctx.Persistence.csproj" />
<ProjectReference Include="..\Ctx.Providers\Ctx.Providers.csproj" />
<ProjectReference Include="..\Ctx.Infrastructure\Ctx.Infrastructure.csproj" />
```

Y usa:

```csharp
var runtime = Bootstrapper.Create();
var service = runtime.ApplicationService;
```

La diferencia con CLI es solo el adaptador de entrada/salida:

```text
CLI -> args -> DispatchAsync -> CommandResult -> stdout JSON
MCP -> tool schema -> ICtxApplicationService -> tool result JSON
```

## Contrato de respuesta

Cada tool debe devolver un objeto simple:

```json
{
  "success": true,
  "message": "...",
  "data": {}
}
```

Cuando `CommandResult.Success` sea `false`, la tool no debe lanzar excepcion salvo que haya un error del servidor. Debe devolver el resultado de CTX para que el agente pueda razonar sobre el fallo.

## Observabilidad

Registrar telemetria similar al CLI:

```text
mcp:ctx_status
mcp:ctx_next
mcp:ctx_audit
```

Esto puede reutilizar `IMetricsRepository.RecordCommandUsage`, pero no debe romper la tool si falla.

## Seguridad

El MCP local no debe exponer red por defecto.

Etapa 1:

```text
stdio solamente
sin HTTP
sin auth remota
sin acceso fuera del repo
read-only por defecto
```

Etapa 3, si se expone HTTP:

```text
ModelContextProtocol.AspNetCore
auth obligatoria
allowlist de repos
auditoria de tools mutantes
rate limits
modo read-only por usuario
```

## Configuracion de cliente MCP local

Ejemplo conceptual:

```json
{
  "mcpServers": {
    "ctx": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "C:\\sources\\ctx-open\\Ctx.Mcp",
        "--",
        "--repo",
        "C:\\sources\\ctx-open",
        "--mode",
        "read-only"
      ]
    }
  }
}
```

Cuando exista instalador:

```json
{
  "mcpServers": {
    "ctx": {
      "command": "C:\\ctx\\bin\\ctx-mcp.cmd",
      "args": [
        "--repo",
        "C:\\sources\\ctx-open",
        "--mode",
        "read-only"
      ]
    }
  }
}
```

## Roadmap

### Fase 1: MVP local read-only

- crear `Ctx.Mcp`; **hecho**
- registrar servidor stdio; **hecho**
- implementar tools read-only; **hecho**
- validar con un repo CTX real; **hecho**
- documentar configuracion de cliente MCP. **hecho**

Estado implementado:

```text
Ctx.Mcp
  Program.cs
  Mcp/CtxMcpOptions.cs
  Mcp/RepositoryGuard.cs
  Mcp/CtxReadTools.cs
```

Tools disponibles:

```text
ctx_status
ctx_audit
ctx_next
ctx_context
ctx_graph_summary
ctx_graph_show
ctx_thread_reconstruct
ctx_preflight
```

Publicacion local:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\publish-local.ps1
C:\ctx\bin\ctx-mcp.cmd --repo C:\\sources\\ctx-open --mode read-only
```

Configuracion de cliente MCP local:

```json
{
  "mcpServers": {
    "ctx": {
      "command": "C:\\ctx\\bin\\ctx-mcp.cmd",
      "args": [
        "--repo",
        "C:\\sources\\ctx-open",
        "--mode",
        "read-only"
      ]
    }
  }
}
```

### Fase 2: mutaciones controladas

- agregar tools write para goal, line, task, hypothesis, evidence, decision, conclusion y commit; **hecho**
- rechazar mutaciones si el servidor no esta en `--mode write`; **hecho**
- validar flujo write en repositorio descartable; **hecho**
- registrar telemetria MCP. **pendiente**

### Fase 3: bootstrap y demo

- agregar `bootstrap map`;
- agregar `bootstrap apply`;
- probar con `examples/ctx/almacen-barrial-reglas`;
- documentar flujo de charla.

### Fase 4: HTTP remoto

- agregar `Ctx.Mcp.Http` o modo HTTP dentro de `Ctx.Mcp`;
- usar `ModelContextProtocol.AspNetCore`;
- auth;
- allowlist de repos;
- preparar compatibilidad para ChatGPT remoto.

## Criterio de exito

Un agente conectado por MCP debe poder responder:

```text
Que trabajo cognitivo esta abierto?
Que deberia hacer despues?
Que evidencia sostiene esta decision?
Como se ve el grafo actual?
Que falta antes de commitear CTX?
```

sin que el usuario tenga que pegar `ctx status`, `ctx next`, `ctx graph summary` o partes de `.ctx` en el chat.
