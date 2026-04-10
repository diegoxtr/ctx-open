[CmdletBinding()]
param(
    [string]$RepoRoot = "",
    [string]$DemoPath = "",
    [switch]$SkipBuild,
    [switch]$KeepDemo
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
}

if ([string]::IsNullOrWhiteSpace($DemoPath)) {
    $DemoPath = Join-Path $RepoRoot "tmp\ctx-smoke-demo"
}

$cliProject = Join-Path $RepoRoot "Ctx.Cli\Ctx.Cli.csproj"
$testsProject = Join-Path $RepoRoot "Ctx.Tests\Ctx.Tests.csproj"
$solution = Join-Path $RepoRoot "Ctx.sln"

function Invoke-JsonCommand {
    param(
        [string]$WorkingDirectory,
        [string[]]$Arguments
    )

    $output = & dotnet run --project $cliProject -- @Arguments 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "Command failed: ctx $($Arguments -join ' ')`n$output"
    }

    $text = ($output | Out-String).Trim()
    return $text | ConvertFrom-Json
}

function Assert-Success {
    param(
        [object]$Result,
        [string]$CommandName
    )

    if (-not $Result.success) {
        throw "Command '$CommandName' returned success=false."
    }
}

Write-Host "CTX smoke test"
Write-Host "Repository root: $RepoRoot"
Write-Host "Demo path: $DemoPath"

if (-not $SkipBuild) {
    Write-Host "Running build..."
    & dotnet build $solution
    if ($LASTEXITCODE -ne 0) {
        throw "Build failed."
    }

    Write-Host "Running tests..."
    & dotnet test $testsProject
    if ($LASTEXITCODE -ne 0) {
        throw "Tests failed."
    }
}

if (Test-Path $DemoPath) {
    Remove-Item -Recurse -Force $DemoPath
}

New-Item -ItemType Directory -Path $DemoPath -Force | Out-Null

try {
    Push-Location $DemoPath

    $init = Invoke-JsonCommand -WorkingDirectory $DemoPath -Arguments @("init", "--name", "CTX-SMOKE", "--description", "Smoke test demo")
    Assert-Success $init "init"

    $goal = Invoke-JsonCommand -WorkingDirectory $DemoPath -Arguments @("goal", "add", "--title", "Validar CTX", "--description", "Smoke test end-to-end")
    Assert-Success $goal "goal add"
    $goalId = $goal.data.id.value

    $task = Invoke-JsonCommand -WorkingDirectory $DemoPath -Arguments @("task", "add", "--title", "Probar flujo principal", "--description", "Validar artefactos y commit", "--goal", $goalId)
    Assert-Success $task "task add"
    $taskId = $task.data.id.value

    $hypothesis = Invoke-JsonCommand -WorkingDirectory $DemoPath -Arguments @("hypo", "add", "--statement", "CTX permite razonamiento estructurado reutilizable", "--rationale", "Los artefactos quedan persistidos y trazados", "--confidence", "0.82", "--task", $taskId)
    Assert-Success $hypothesis "hypo add"
    $hypothesisId = $hypothesis.data.id.value

    $evidence = Invoke-JsonCommand -WorkingDirectory $DemoPath -Arguments @("evidence", "add", "--title", "Prueba manual", "--summary", "El flujo CLI permite retomar estado y contexto", "--source", "evaluacion local", "--kind", "Observation", "--confidence", "0.9", "--supports", "hypothesis:$hypothesisId")
    Assert-Success $evidence "evidence add"
    $evidenceId = $evidence.data.id.value

    $decision = Invoke-JsonCommand -WorkingDirectory $DemoPath -Arguments @("decision", "add", "--title", "Usar CTX en piloto", "--rationale", "La prueba funcional demuestra trazabilidad y persistencia", "--state", "Accepted", "--hypotheses", $hypothesisId, "--evidence", $evidenceId)
    Assert-Success $decision "decision add"
    $decisionId = $decision.data.id.value

    $conclusion = Invoke-JsonCommand -WorkingDirectory $DemoPath -Arguments @("conclusion", "add", "--summary", "CTX ya tiene una base funcional para piloto interno", "--state", "Accepted", "--decisions", $decisionId, "--evidence", $evidenceId)
    Assert-Success $conclusion "conclusion add"

    $context = Invoke-JsonCommand -WorkingDirectory $DemoPath -Arguments @("context", "--purpose", "Preparar contexto para evaluar la smoke test")
    Assert-Success $context "context"

    $run = Invoke-JsonCommand -WorkingDirectory $DemoPath -Arguments @("run", "--provider", "openai", "--purpose", "Evaluar riesgos y proximos pasos de la smoke test")
    Assert-Success $run "run"
    $runId = $run.data.id.value
    $packetId = $run.data.packetId.value

    $metrics = Invoke-JsonCommand -WorkingDirectory $DemoPath -Arguments @("metrics", "show")
    Assert-Success $metrics "metrics show"

    $commit = Invoke-JsonCommand -WorkingDirectory $DemoPath -Arguments @("commit", "-m", "smoke test completa")
    Assert-Success $commit "commit"
    $commitId = $commit.data.id.value

    $log = Invoke-JsonCommand -WorkingDirectory $DemoPath -Arguments @("log")
    Assert-Success $log "log"

    Write-Host ""
    Write-Host "Smoke test completed successfully"
    Write-Host "Goal ID: $goalId"
    Write-Host "Task ID: $taskId"
    Write-Host "Hypothesis ID: $hypothesisId"
    Write-Host "Evidence ID: $evidenceId"
    Write-Host "Decision ID: $decisionId"
    Write-Host "Run ID: $runId"
    Write-Host "Packet ID: $packetId"
    Write-Host "Commit ID: $commitId"
    Write-Host "Total runs: $($metrics.data.totalRuns)"
    Write-Host "Total tokens: $($metrics.data.totalTokens)"
    Write-Host "ACU cost: $($metrics.data.totalAcuCost)"
}
finally {
    Pop-Location

    if (-not $KeepDemo -and (Test-Path $DemoPath)) {
        Remove-Item -Recurse -Force $DemoPath
        Write-Host "Demo path removed: $DemoPath"
    }
}
