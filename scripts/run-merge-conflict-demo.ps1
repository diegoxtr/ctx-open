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
    $DemoPath = Join-Path $RepoRoot "tmp\ctx-merge-demo"
}

$cliProject = Join-Path $RepoRoot "Ctx.Cli\Ctx.Cli.csproj"
$testsProject = Join-Path $RepoRoot "Ctx.Tests\Ctx.Tests.csproj"
$solution = Join-Path $RepoRoot "Ctx.sln"

function Invoke-JsonCommand {
    param(
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

function Save-Json {
    param(
        [string]$Path,
        [object]$Data
    )

    $json = $Data | ConvertTo-Json -Depth 20
    Set-Content -Path $Path -Value $json -Encoding UTF8
}

Write-Host "CTX merge conflict demo"
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

    $init = Invoke-JsonCommand -Arguments @("init", "--name", "CTX-MERGE-DEMO", "--description", "Branch and conflict demo")
    Assert-Success $init "init"

    $goal = Invoke-JsonCommand -Arguments @("goal", "add", "--title", "Explorar conflicto cognitivo", "--description", "Forzar divergencia entre ramas")
    Assert-Success $goal "goal add"
    $goalId = $goal.data.id.value

    $task = Invoke-JsonCommand -Arguments @("task", "add", "--title", "Evaluar politica de merge", "--description", "Descripcion base", "--goal", $goalId)
    Assert-Success $task "task add"
    $taskId = $task.data.id.value

    $initialCommit = Invoke-JsonCommand -Arguments @("commit", "-m", "estado base")
    Assert-Success $initialCommit "commit base"

    $branch = Invoke-JsonCommand -Arguments @("branch", "feature-conflict")
    Assert-Success $branch "branch"

    $featureCheckout = Invoke-JsonCommand -Arguments @("checkout", "feature-conflict")
    Assert-Success $featureCheckout "checkout feature-conflict"

    $workingPath = Join-Path $DemoPath ".ctx\working\working-context.json"
    $featureContext = Get-Content $workingPath -Raw | ConvertFrom-Json
    $featureContext.tasks[0].description = "Descripcion divergente en feature"
    $featureContext.dirty = $true
    Save-Json -Path $workingPath -Data $featureContext

    $featureCommit = Invoke-JsonCommand -Arguments @("commit", "-m", "cambio en feature")
    Assert-Success $featureCommit "commit feature"

    $mainCheckout = Invoke-JsonCommand -Arguments @("checkout", "main")
    Assert-Success $mainCheckout "checkout main"

    $mainContext = Get-Content $workingPath -Raw | ConvertFrom-Json
    $mainContext.tasks[0].description = "Descripcion divergente en main"
    $mainContext.dirty = $true
    Save-Json -Path $workingPath -Data $mainContext

    $mainCommit = Invoke-JsonCommand -Arguments @("commit", "-m", "cambio en main")
    Assert-Success $mainCommit "commit main"

    $merge = Invoke-JsonCommand -Arguments @("merge", "feature-conflict")
    Assert-Success $merge "merge"

    $conflicts = $merge.data.conflicts
    if (-not $conflicts -or $conflicts.Count -lt 1) {
        throw "Expected at least one cognitive conflict, but none were returned."
    }

    Write-Host ""
    Write-Host "Merge conflict demo completed successfully"
    Write-Host "Task ID: $taskId"
    Write-Host "Base commit: $($initialCommit.data.id.value)"
    Write-Host "Feature commit: $($featureCommit.data.id.value)"
    Write-Host "Main commit: $($mainCommit.data.id.value)"
    Write-Host "Conflict count: $($conflicts.Count)"
    Write-Host "First conflict entity type: $($conflicts[0].entityType)"
    Write-Host "First conflict entity id: $($conflicts[0].entityId)"
}
finally {
    Pop-Location

    if (-not $KeepDemo -and (Test-Path $DemoPath)) {
        Remove-Item -Recurse -Force $DemoPath
        Write-Host "Demo path removed: $DemoPath"
    }
}
