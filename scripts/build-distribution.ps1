param(
    [string]$Configuration = "Release",
    [string]$OutputRoot = "",
    [string]$TargetManifest = "",
    [string[]]$TargetIds = @(),
    [switch]$SkipViewer
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
    $OutputRoot = Join-Path $repoRoot "artifacts\distribution"
}

if ([string]::IsNullOrWhiteSpace($TargetManifest)) {
    $TargetManifest = Join-Path $repoRoot "distribution\targets.json"
}

$cliProject = Join-Path $repoRoot "Ctx.Cli\Ctx.Cli.csproj"
$viewerProject = Join-Path $repoRoot "Ctx.Viewer\Ctx.Viewer.csproj"
$agentLinkPrompt = Join-Path $repoRoot "distribution\agent-link\CTX_AGENT_LINK_PROMPT.txt"

if (-not (Test-Path $TargetManifest)) {
    throw "Target manifest not found: $TargetManifest"
}

if (-not (Test-Path $agentLinkPrompt)) {
    throw "Agent-link prompt not found: $agentLinkPrompt"
}

$manifest = Get-Content $TargetManifest -Raw | ConvertFrom-Json

$targets = $manifest.targets
if ($TargetIds.Count -gt 0) {
    $targets = $targets | Where-Object { $TargetIds -contains $_.id }
}

foreach ($target in $targets) {
    $targetRoot = Join-Path $OutputRoot $target.id
    $bundleRoot = Join-Path $targetRoot "bundle"
    $cliOut = Join-Path $bundleRoot "bin"
    $viewerOut = Join-Path $bundleRoot "viewer"
    $metaOut = Join-Path $bundleRoot "distribution"

    if (Test-Path $targetRoot) {
        Remove-Item -Recurse -Force $targetRoot
    }

    New-Item -ItemType Directory -Path $cliOut -Force | Out-Null
    New-Item -ItemType Directory -Path $metaOut -Force | Out-Null

    dotnet publish $cliProject -c $Configuration -r $target.rid --self-contained true -p:PublishSingleFile=true -o $cliOut

    if (-not $SkipViewer -and $target.includeViewer) {
        New-Item -ItemType Directory -Path $viewerOut -Force | Out-Null
        dotnet publish $viewerProject -c $Configuration -r $target.rid --self-contained true -o $viewerOut
    }

    Copy-Item $agentLinkPrompt (Join-Path $metaOut "CTX_AGENT_LINK_PROMPT.txt")
    $target | ConvertTo-Json -Depth 4 | Set-Content -Path (Join-Path $metaOut "target.json") -Encoding ASCII

    $archiveBase = Join-Path $OutputRoot ("ctx-" + $target.id)
    switch ($target.portableFormat) {
        "zip" {
            Compress-Archive -Path (Join-Path $bundleRoot "*") -DestinationPath ($archiveBase + ".zip") -Force
        }
        "tar.gz" {
            & tar -czf ($archiveBase + ".tar.gz") -C $bundleRoot .
        }
        default {
            throw "Unsupported portable format: $($target.portableFormat)"
        }
    }

    Write-Host "Built portable distribution for $($target.id) at $targetRoot"
}
