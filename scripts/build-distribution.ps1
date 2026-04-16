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
$helperPrompt = Join-Path $repoRoot "prompts\CTX_HELPER_PROMPT.md"
$installManifest = Join-Path $repoRoot "distribution\install-manifest.json"
$viewerGuide = Join-Path $repoRoot "docs\CTX_VIEWER_GUIDE.md"
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

foreach ($requiredDoc in @($viewerGuide, $agentPrompt, $autonomousProtocol)) {
    if (-not (Test-Path $requiredDoc)) {
        throw "Required helper asset not found: $requiredDoc"
    }
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
    $promptOut = Join-Path $bundleRoot "prompts"
    $docsOut = Join-Path $bundleRoot "docs"

    if (Test-Path $targetRoot) {
        Remove-Item -Recurse -Force $targetRoot
    }

    New-Item -ItemType Directory -Path $cliOut -Force | Out-Null
    New-Item -ItemType Directory -Path $metaOut -Force | Out-Null
    New-Item -ItemType Directory -Path $promptOut -Force | Out-Null
    New-Item -ItemType Directory -Path $docsOut -Force | Out-Null

    dotnet publish $cliProject -c $Configuration -r $target.rid --self-contained true -p:PublishSingleFile=true -o $cliOut

    if (-not $SkipViewer -and $target.includeViewer) {
        New-Item -ItemType Directory -Path $viewerOut -Force | Out-Null
        dotnet publish $viewerProject -c $Configuration -r $target.rid --self-contained true -o $viewerOut
    }

    Copy-Item $agentLinkPrompt (Join-Path $metaOut "CTX_AGENT_LINK_PROMPT.txt")
    Copy-Item $helperPrompt (Join-Path $promptOut "CTX_HELPER_PROMPT.md")
    Copy-Item $installManifest (Join-Path $metaOut "install-manifest.json")
    Copy-Item $viewerGuide (Join-Path $docsOut "CTX_VIEWER_GUIDE.md")
    Copy-Item $agentPrompt (Join-Path $promptOut "CTX_AGENT_PROMPT.md")
    Copy-Item $autonomousProtocol (Join-Path $docsOut "CTX_AUTONOMOUS_OPERATION_PROTOCOL.md")
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
