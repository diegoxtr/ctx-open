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
$packagedDocsManifest = Join-Path $repoRoot "distribution\packaged-docs.txt"
$packagedPromptsManifest = Join-Path $repoRoot "distribution\packaged-prompts.txt"
$docsSource = Join-Path $repoRoot "docs"
$viewerGuide = Join-Path $repoRoot "docs\CTX_VIEWER_GUIDE.md"
$cliCommands = Join-Path $repoRoot "docs\CLI_COMMANDS.md"
$technicalIndex = Join-Path $repoRoot "docs\TECHNICAL_INDEX.md"
$agentPrompt = Join-Path $repoRoot "prompts\CTX_AGENT_PROMPT.md"
$autonomousProtocol = Join-Path $repoRoot "docs\CTX_AUTONOMOUS_OPERATION_PROTOCOL.md"

function Read-PackagedAssetList {
    param([string]$Path)

    if (-not (Test-Path $Path)) {
        throw "Packaged asset manifest not found: $Path"
    }

    return @(Get-Content $Path |
        ForEach-Object { $_.Trim() } |
        Where-Object { -not [string]::IsNullOrWhiteSpace($_) -and -not $_.StartsWith("#") })
}

function Copy-PackagedPromptAssets {
    param(
        [string]$SourceRoot,
        [string]$TargetRoot,
        [string[]]$RelativePaths
    )

    foreach ($relativePath in $RelativePaths) {
        $sourcePath = Join-Path $SourceRoot $relativePath
        if (-not (Test-Path $sourcePath)) {
            throw "Required packaged prompt not found: $sourcePath"
        }

        Copy-Item $sourcePath (Join-Path $TargetRoot (Split-Path -Leaf $relativePath)) -Force
    }
}

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

$packagedDocs = Read-PackagedAssetList -Path $packagedDocsManifest
$packagedPrompts = Read-PackagedAssetList -Path $packagedPromptsManifest

foreach ($requiredDoc in @($viewerGuide, $cliCommands, $agentPrompt, $autonomousProtocol)) {
    if (-not (Test-Path $requiredDoc)) {
        throw "Required helper asset not found: $requiredDoc"
    }
}

if (-not (Test-Path $technicalIndex)) {
    throw "Documentation index not found: $technicalIndex"
}

foreach ($requiredPackagedDoc in $packagedDocs) {
    $docPath = Join-Path $repoRoot $requiredPackagedDoc
    if (-not (Test-Path $docPath)) {
        throw "Required packaged documentation not found: $docPath"
    }
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
set "CTX_INSTALL_ROOT=%~dp0.."
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
export CTX_INSTALL_ROOT="$(cd "$DIR/.." && pwd)"
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
    Copy-Item $installManifest (Join-Path $metaOut "install-manifest.json")
    Copy-Item $packagedDocsManifest (Join-Path $metaOut "packaged-docs.txt")
    Copy-Item $packagedPromptsManifest (Join-Path $metaOut "packaged-prompts.txt")
    Copy-Item (Join-Path $docsSource "*") $docsOut -Recurse -Force
    Copy-PackagedPromptAssets -SourceRoot $repoRoot -TargetRoot $promptOut -RelativePaths $packagedPrompts
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

$archivePaths = @(
    Get-ChildItem -Path (Join-Path $OutputRoot "*.zip") -File
    Get-ChildItem -Path (Join-Path $OutputRoot "*.tar.gz") -File
) |
    Sort-Object Name

if ($archivePaths.Count -gt 0) {
    $checksumLines = foreach ($archivePath in $archivePaths) {
        $hash = Get-FileHash -LiteralPath $archivePath.FullName -Algorithm SHA256
        "$($hash.Hash.ToLowerInvariant())  $($archivePath.Name)"
    }

    $checksumLines | Set-Content -Path (Join-Path $OutputRoot "SHA256SUMS.txt") -Encoding ASCII
    Write-Host "Wrote SHA256SUMS.txt for $($archivePaths.Count) portable archive(s)"
}
