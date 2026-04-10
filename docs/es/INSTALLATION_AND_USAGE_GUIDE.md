# CTX - Guia de Instalacion y Uso
Si un modelo de lenguaje y su agente pierden el contexto, esta es la herramienta que necesitas.

## 1. Objetivo

Esta guia explica como instalar, compilar y usar CTX por primera vez en un entorno local.

Esta pensada para:

- testers tecnicos;
- desarrolladores;
- arquitectos;
- usuarios iniciales de la V1 candidata.

## 2. Requisitos

Antes de comenzar, verificar:

- Windows con terminal disponible;
- .NET SDK 8 instalado;
- acceso al codigo fuente del repositorio;
- permisos de lectura y escritura en la carpeta de trabajo.

Opcionalmente:

- `OPENAI_API_KEY`
- `ANTHROPIC_API_KEY`

Si las claves no estan configuradas, CTX puede operar con fallback offline para pruebas funcionales.

## 3. Verificar instalacion de .NET

Ejecutar:

```powershell
dotnet --version
```

Resultado esperado:

- version `8.x`

Tambien se puede validar:

```powershell
dotnet --list-sdks
```

## 4. Obtener el codigo

Si ya tenes el repositorio en tu maquina, ubicate en la carpeta raiz del proyecto.

Ejemplo:

```powershell
cd C:\sources\ctx
```

## 5. Restaurar, compilar y testear

Desde la raiz del proyecto ejecutar:

```powershell
dotnet restore Ctx.sln
dotnet build Ctx.sln
dotnet test .\Ctx.Tests\Ctx.Tests.csproj
```

Resultado esperado:

- restore exitoso;
- build exitoso;
- tests exitosos.

## 6. Ejecutar la CLI

La CLI puede ejecutarse directamente con:

```powershell
dotnet run --project .\Ctx.Cli -- status
```

Si todavia no existe un repositorio cognitivo en la carpeta actual, el comando que debe correrse primero es `init`.

## 7. Crear un repositorio cognitivo

Ubicate en una carpeta de trabajo donde quieras iniciar CTX y ejecutar:

```powershell
dotnet run --project .\\Ctx.Cli -- init --name "CTX-DEMO" --description "Primer repositorio cognitivo"
```

Resultado esperado:

- se crea la carpeta `.ctx/`;
- se generan archivos de configuracion;
- la rama actual queda en `main`.

Verificar:

```powershell
dotnet run --project .\\Ctx.Cli -- status
```

## 8. Primer flujo de uso recomendado

## Paso 1 - Crear un objetivo

```powershell
dotnet run --project .\\Ctx.Cli -- goal add --title "Definir estrategia de prueba" --description "Preparar un piloto tecnico"
```

## Paso 2 - Crear una tarea

```powershell
dotnet run --project .\\Ctx.Cli -- task add --title "Evaluar flujo CLI" --description "Validar comandos principales"
```

## Paso 3 - Crear una hipotesis

```powershell
dotnet run --project .\\Ctx.Cli -- hypo add --statement "El flujo estructurado mejora la trazabilidad" --rationale "El estado queda persistido en artefactos"
```

## Paso 4 - Registrar evidencia

```powershell
dotnet run --project .\\Ctx.Cli -- evidence add --title "Prueba inicial" --summary "La estructura permite retomar contexto" --source "evaluacion manual" --kind Observation --supports hypothesis:<hypothesisId>
```

## Paso 5 - Registrar una decision

```powershell
dotnet run --project .\\Ctx.Cli -- decision add --title "Usar CTX en piloto" --rationale "La trazabilidad es suficiente para un piloto" --state Accepted --hypotheses <hypothesisId> --evidence <evidenceId>
```

## Paso 6 - Registrar una conclusion

```powershell
dotnet run --project .\\Ctx.Cli -- conclusion add --summary "Se aprueba el uso en piloto interno" --state Accepted --decisions <decisionId> --evidence <evidenceId>
```

## Paso 7 - Ejecutar una corrida

```powershell
dotnet run --project .\\Ctx.Cli -- run --provider openai --purpose "Revisar riesgos del piloto"
```

## Paso 8 - Generar commit cognitivo

```powershell
dotnet run --project .\\Ctx.Cli -- commit -m "primer flujo completo de uso"
```

## 9. Comandos utiles

## Estado y navegacion

```powershell
dotnet run --project .\Ctx.Cli -- status
dotnet run --project .\Ctx.Cli -- log
dotnet run --project .\Ctx.Cli -- diff
```

## Artefactos cognitivos

```powershell
dotnet run --project .\Ctx.Cli -- goal list
dotnet run --project .\Ctx.Cli -- task list
dotnet run --project .\Ctx.Cli -- hypo list
dotnet run --project .\Ctx.Cli -- decision list
dotnet run --project .\Ctx.Cli -- evidence list
dotnet run --project .\Ctx.Cli -- conclusion list
```

## Inspeccion operativa

```powershell
dotnet run --project .\Ctx.Cli -- provider list
dotnet run --project .\Ctx.Cli -- run list
dotnet run --project .\Ctx.Cli -- packet list
dotnet run --project .\Ctx.Cli -- metrics show
```

## Branching y merge

```powershell
dotnet run --project .\Ctx.Cli -- branch feature-x
dotnet run --project .\Ctx.Cli -- checkout feature-x
dotnet run --project .\Ctx.Cli -- merge main
```

## 10. Estructura local generada

Al inicializar un repositorio, CTX crea:

- `.ctx/version.json`
- `.ctx/config.json`
- `.ctx/project.json`
- `.ctx/HEAD`
- `.ctx/branches/`
- `.ctx/commits/`
- `.ctx/graph/`
- `.ctx/working/`
- `.ctx/staging/`
- `.ctx/runs/`
- `.ctx/packets/`
- `.ctx/index/`
- `.ctx/metrics/`
- `.ctx/providers/`
- `.ctx/logs/`

## 11. Recomendaciones de uso

- trabajar cada caso de prueba en una carpeta separada;
- hacer commits cognitivos con mensajes claros;
- registrar evidencia antes de aceptar decisiones importantes;
- usar `packet list` y `run list` para revisar iteraciones;
- revisar `metrics show` al final de cada escenario;
- si aparecen conflictos cognitivos, no ignorarlos: revisar el merge antes de continuar.

## 12. Problemas comunes

## Problema: no compila

Verificar:

- que `dotnet --version` sea `8.x`;
- que `global.json` este presente;
- que el restore haya terminado correctamente.

## Problema: no tengo claves de proveedor

No bloquea la prueba:

- CTX puede ejecutarse con fallback offline.

## Problema: no entiendo los IDs

Recomendacion:

- usar `list` para ubicar entidades;
- usar `show` para inspeccionar detalle;
- copiar el `id.value` del artefacto necesario.

## Problema: merge con conflictos

Interpretacion:

- el sistema detecto divergencia sobre el mismo artefacto cognitivo;
- revisar los artefactos antes de consolidar el resultado.

## 13. Criterio de uso correcto

Se considera que el usuario pudo operar CTX correctamente si:

- pudo inicializar un repositorio;
- pudo crear artefactos cognitivos;
- pudo ejecutar al menos un `run`;
- pudo crear al menos un commit cognitivo;
- pudo inspeccionar resultados desde la CLI.

## 14. Siguiente lectura recomendada

Para continuar:

- `docs/V1_PLAN.md`
- `docs/PILOT_TESTING_GUIDE.md`
- `README.md`

