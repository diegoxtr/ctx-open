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
$mcpProject = Join-Path $repoRoot "Ctx.Mcp\Ctx.Mcp.csproj"
$acpProject = Join-Path $repoRoot "Ctx.Agent.Acp\Ctx.Agent.Acp.csproj"
$viewerProject = Join-Path $repoRoot "Ctx.Viewer\Ctx.Viewer.csproj"
$agentLinkPrompt = Join-Path $repoRoot "distribution\agent-link\CTX_AGENT_LINK_PROMPT.txt"
$helperPrompt = Join-Path $repoRoot "prompts\CTX_HELPER_PROMPT.md"
$installManifest = Join-Path $repoRoot "distribution\install-manifest.json"
$docsSource = Join-Path $repoRoot "docs"
$viewerGuide = Join-Path $repoRoot "docs\CTX_VIEWER_GUIDE.md"
$cliCommands = Join-Path $repoRoot "docs\CLI_COMMANDS.md"
$technicalIndex = Join-Path $repoRoot "docs\TECHNICAL_INDEX.md"
$agentPrompt = Join-Path $repoRoot "prompts\CTX_AGENT_PROMPT.md"
$autonomousProtocol = Join-Path $repoRoot "docs\CTX_AUTONOMOUS_OPERATION_PROTOCOL.md"

if (-not (Test-Path $TargetManifest)) {
    throw "Target manifest not found: $TargetManifest"
}

if (-not (Test-Path $agentLinkPrompt)) {
    throw "Agent-link prompt not found: $agentLinkPrompt"
}

if (-not (Test-Path $helperPrompt)) {
    throw "Helper prompt not found: $helperPrompt"
}

if (-not (Test-Path $installManifest)) {
    throw "Install manifest not found: $installManifest"
}

foreach ($requiredDoc in @($viewerGuide, $cliCommands, $agentPrompt, $autonomousProtocol)) {
    if (-not (Test-Path $requiredDoc)) {
        throw "Required helper asset not found: $requiredDoc"
    }
}

if (-not (Test-Path $technicalIndex)) {
    throw "Documentation index not found: $technicalIndex"
}

if (-not (Test-Path $mcpProject)) {
    throw "Ctx.Mcp project not found: $mcpProject"
}

if (-not (Test-Path $acpProject)) {
    throw "Ctx.Agent.Acp project not found: $acpProject"
}

$manifest = Get-Content $TargetManifest -Raw | ConvertFrom-Json

$targets = $manifest.targets
if ($TargetIds.Count -gt 0) {
    $targets = $targets | Where-Object { $TargetIds -contains $_.id }
}

function Write-PortableLaunchers {
    param(
        [Parameter(Mandatory = $true)]
        [string]$BundleRoot,

        [Parameter(Mandatory = $true)]
        [string]$TargetOs,

        [Parameter(Mandatory = $true)]
        [bool]$IncludeViewerLauncher
    )

    $binPath = Join-Path $BundleRoot "bin"

    if ($TargetOs -eq "windows") {
        Set-Content -Path (Join-Path $binPath "ctx.cmd") -Value '@echo off
"%~dp0Ctx.Cli.exe" %*' -Encoding ASCII

        Set-Content -Path (Join-Path $binPath "ctx-mcp.cmd") -Value '@echo off
"%~dp0..\mcp\Ctx.Mcp.exe" %*' -Encoding ASCII

        Set-Content -Path (Join-Path $binPath "ctx-agent-acp.cmd") -Value '@echo off
"%~dp0..\acp\Ctx.Agent.Acp.exe" %*' -Encoding ASCII

        if ($IncludeViewerLauncher) {
            Set-Content -Path (Join-Path $binPath "ctx-viewer.cmd") -Value '@echo off
"%~dp0..\viewer\Ctx.Viewer.exe" %*' -Encoding ASCII
        }

        return
    }

    Set-Content -Path (Join-Path $binPath "ctx") -Value '#!/usr/bin/env bash
DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
exec "$DIR/Ctx.Cli" "$@"' -Encoding ASCII

    Set-Content -Path (Join-Path $binPath "ctx-mcp") -Value '#!/usr/bin/env bash
DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
exec "$DIR/../mcp/Ctx.Mcp" "$@"' -Encoding ASCII

    Set-Content -Path (Join-Path $binPath "ctx-agent-acp") -Value '#!/usr/bin/env bash
DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
exec "$DIR/../acp/Ctx.Agent.Acp" "$@"' -Encoding ASCII

    if ($IncludeViewerLauncher) {
        Set-Content -Path (Join-Path $binPath "ctx-viewer") -Value '#!/usr/bin/env bash
DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
exec "$DIR/../viewer/Ctx.Viewer" "$@"' -Encoding ASCII
    }
}

foreach ($target in $targets) {
    $targetRoot = Join-Path $OutputRoot $target.id
    $bundleRoot = Join-Path $targetRoot "bundle"
    $cliOut = Join-Path $bundleRoot "bin"
    $mcpOut = Join-Path $bundleRoot "mcp"
    $acpOut = Join-Path $bundleRoot "acp"
    $viewerOut = Join-Path $bundleRoot "viewer"
    $metaOut = Join-Path $bundleRoot "distribution"
    $promptOut = Join-Path $bundleRoot "prompts"
    $docsOut = Join-Path $bundleRoot "docs"

    if (Test-Path $targetRoot) {
        Remove-Item -Recurse -Force $targetRoot
    }

    New-Item -ItemType Directory -Path $cliOut -Force | Out-Null
    New-Item -ItemType Directory -Path $mcpOut -Force | Out-Null
    New-Item -ItemType Directory -Path $acpOut -Force | Out-Null
    New-Item -ItemType Directory -Path $metaOut -Force | Out-Null
    New-Item -ItemType Directory -Path $promptOut -Force | Out-Null
    New-Item -ItemType Directory -Path $docsOut -Force | Out-Null

    dotnet publish $cliProject -c $Configuration -r $target.rid --self-contained true -p:PublishSingleFile=true -o $cliOut
    dotnet publish $mcpProject -c $Configuration -r $target.rid --self-contained true -p:PublishSingleFile=true -o $mcpOut
    dotnet publish $acpProject -c $Configuration -r $target.rid --self-contained true -p:PublishSingleFile=true -o $acpOut

    if (-not $SkipViewer -and $target.includeViewer) {
        New-Item -ItemType Directory -Path $viewerOut -Force | Out-Null
        dotnet publish $viewerProject -c $Configuration -r $target.rid --self-contained true -o $viewerOut
    }

    Copy-Item $agentLinkPrompt (Join-Path $metaOut "CTX_AGENT_LINK_PROMPT.txt")
    Copy-Item $helperPrompt (Join-Path $promptOut "CTX_HELPER_PROMPT.md")
    Copy-Item $installManifest (Join-Path $metaOut "install-manifest.json")
    Copy-Item (Join-Path $docsSource "*") $docsOut -Recurse -Force
    Copy-Item $agentPrompt (Join-Path $promptOut "CTX_AGENT_PROMPT.md")
    $target | ConvertTo-Json -Depth 4 | Set-Content -Path (Join-Path $metaOut "target.json") -Encoding ASCII

    Write-PortableLaunchers `
        -BundleRoot $bundleRoot `
        -TargetOs $target.os `
        -IncludeViewerLauncher ((-not $SkipViewer) -and [bool]$target.includeViewer)

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
