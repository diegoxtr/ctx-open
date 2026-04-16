param(
    [ValidateSet("source", "portable")]
    [string]$Mode = "source",
    [string]$InstallRoot = "",
    [string]$SourceRepoPath = "",
    [string]$RepoUrl = "https://github.com/diegoxtr/ctx-open.git",
    [string]$BundlePath = "",
    [string]$ViewerUrl = "",
    [string]$VersionLabel = "dev",
    [ValidateSet("Auto", "User", "Machine", "None")]
    [string]$PathScope = "Auto",
    [switch]$SkipViewer
)

$ErrorActionPreference = "Stop"

$scriptRoot = $PSScriptRoot
$repoRoot = Split-Path -Parent $scriptRoot
$manifestPath = Join-Path $repoRoot "distribution\install-manifest.json"

if (-not (Test-Path $manifestPath)) {
    throw "Install manifest not found: $manifestPath"
}

$manifest = Get-Content $manifestPath -Raw | ConvertFrom-Json

if ([string]::IsNullOrWhiteSpace($InstallRoot)) {
    $InstallRoot = $manifest.installRoots.windows
}

if ([string]::IsNullOrWhiteSpace($ViewerUrl)) {
    $ViewerUrl = $manifest.defaultViewerUrl
}

$binPath = Join-Path $InstallRoot $manifest.paths.bin
$viewerPath = Join-Path $InstallRoot $manifest.paths.viewer
$promptsPath = Join-Path $InstallRoot $manifest.paths.prompts
$docsPath = Join-Path $InstallRoot $manifest.paths.docs
$metadataPath = Join-Path $InstallRoot $manifest.metadataFile

function Reset-InstallLayout {
    param(
        [string]$Root,
        [string]$Bin,
        [string]$Viewer,
        [string]$Prompts,
        [string]$Docs
    )

    New-Item -ItemType Directory -Path $Root -Force | Out-Null

    foreach ($path in @($Bin, $Viewer, $Prompts, $Docs)) {
        if (Test-Path $path) {
            Get-ChildItem -LiteralPath $path -Force -ErrorAction SilentlyContinue |
                Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
        }

        New-Item -ItemType Directory -Path $path -Force | Out-Null
    }
}

function Write-InstallMetadata {
    param(
        [string]$Path,
        [string]$Root,
        [string]$ModeName,
        [string]$SourceRoot,
        [string]$PromptSource,
        [string]$ViewerEndpoint
    )

    $payload = [ordered]@{
        installedAtUtc = [DateTime]::UtcNow.ToString("o")
        installRoot = $Root
        version = $VersionLabel
        mode = $ModeName
        sourceRoot = $SourceRoot
        helperPromptSource = $PromptSource
        viewerUrl = $ViewerEndpoint
    } | ConvertTo-Json -Depth 4

    Set-Content -LiteralPath $Path -Value $payload -Encoding ASCII
}

function Copy-HelperPrompt {
    param(
        [string]$PromptSourcePath,
        [string]$PromptTargetPath
    )

    if (-not (Test-Path $PromptSourcePath)) {
        throw "Helper prompt not found: $PromptSourcePath"
    }

    Copy-Item $PromptSourcePath $PromptTargetPath -Force
}

function Copy-ContextDocs {
    param(
        [string]$SourceRoot,
        [string]$DocsTargetPath,
        [string]$PromptsTargetPath
    )

    $docMappings = @(
        @{ Source = Join-Path $SourceRoot "docs\CTX_VIEWER_GUIDE.md"; Target = Join-Path $DocsTargetPath "CTX_VIEWER_GUIDE.md" },
        @{ Source = Join-Path $SourceRoot "docs\CTX_AUTONOMOUS_OPERATION_PROTOCOL.md"; Target = Join-Path $DocsTargetPath "CTX_AUTONOMOUS_OPERATION_PROTOCOL.md" },
        @{ Source = Join-Path $SourceRoot "prompts\CTX_AGENT_PROMPT.md"; Target = Join-Path $PromptsTargetPath "CTX_AGENT_PROMPT.md" }
    )

    foreach ($mapping in $docMappings) {
        if (-not (Test-Path $mapping.Source)) {
            throw "Required context asset not found: $($mapping.Source)"
        }

        Copy-Item $mapping.Source $mapping.Target -Force
    }
}

function Write-WindowsLaunchers {
    param(
        [string]$Bin,
        [string]$Viewer,
        [string]$ViewerEndpoint,
        [switch]$NoViewer
    )

    $cliLauncher = @"
@echo off
setlocal
"$Bin\Ctx.Cli.exe" %*
endlocal
"@
    Set-Content -LiteralPath (Join-Path $Bin "ctx.cmd") -Value $cliLauncher -Encoding ASCII

    if ($NoViewer) {
        return
    }

    $viewerLauncher = @"
@echo off
setlocal
pushd "$Viewer"
"$Viewer\Ctx.Viewer.exe" --urls $ViewerEndpoint
popd
endlocal
"@
    Set-Content -LiteralPath (Join-Path $Bin "ctx-viewer.cmd") -Value $viewerLauncher -Encoding ASCII
}

function Test-IsAdministrator {
    try {
        $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
        $principal = New-Object Security.Principal.WindowsPrincipal($identity)
        return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
    }
    catch {
        return $false
    }
}

function Add-BinToPath {
    param(
        [string]$Bin,
        [string]$Scope
    )

    if ($Scope -eq "None") {
        return
    }

    $effectiveScope = $Scope
    if ($effectiveScope -eq "Auto") {
        $effectiveScope = if (Test-IsAdministrator) { "Machine" } else { "User" }
    }

    $target = if ($effectiveScope -eq "Machine") { "Machine" } else { "User" }
    $currentPath = [Environment]::GetEnvironmentVariable("Path", $target)
    $pathEntries = @()
    if (-not [string]::IsNullOrWhiteSpace($currentPath)) {
        $pathEntries = $currentPath.Split(';', [System.StringSplitOptions]::RemoveEmptyEntries)
    }

    if ($pathEntries -contains $Bin) {
        return
    }

    $newPath = (($pathEntries + $Bin) | Select-Object -Unique) -join ';'

    try {
        [Environment]::SetEnvironmentVariable("Path", $newPath, $target)
        $env:Path = "$Bin;$env:Path"
    }
    catch {
        if ($target -eq "Machine") {
            Write-Warning "Unable to persist machine PATH automatically. Falling back to user PATH."
            Add-BinToPath -Bin $Bin -Scope "User"
            return
        }

        Write-Warning "Unable to persist user PATH automatically. Add '$Bin' manually if needed."
    }
}

function Install-FromSource {
    param(
        [string]$RepoPath,
        [string]$CloneUrl,
        [string]$Bin,
        [string]$Viewer,
        [string]$Docs,
        [string]$Prompts,
        [switch]$NoViewer
    )

    $effectiveRepoPath = $RepoPath
    if ([string]::IsNullOrWhiteSpace($effectiveRepoPath)) {
        $effectiveRepoPath = Join-Path ([System.IO.Path]::GetTempPath()) ("ctx-source-" + [Guid]::NewGuid().ToString("N"))
        & git clone $CloneUrl $effectiveRepoPath | Out-Host
        if ($LASTEXITCODE -ne 0) {
            throw "git clone failed with exit code $LASTEXITCODE"
        }
    }

    $cliProject = Join-Path $effectiveRepoPath "Ctx.Cli\Ctx.Cli.csproj"
    $viewerProject = Join-Path $effectiveRepoPath "Ctx.Viewer\Ctx.Viewer.csproj"

    if (-not (Test-Path $cliProject)) {
        throw "Ctx.Cli project not found under source repo: $effectiveRepoPath"
    }

    & dotnet publish $cliProject -c Release -o $Bin | Out-Host
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish failed for Ctx.Cli with exit code $LASTEXITCODE"
    }

    if (-not $NoViewer) {
        if (-not (Test-Path $viewerProject)) {
            throw "Ctx.Viewer project not found under source repo: $effectiveRepoPath"
        }

        & dotnet publish $viewerProject -c Release -o $Viewer | Out-Host
        if ($LASTEXITCODE -ne 0) {
            throw "dotnet publish failed for Ctx.Viewer with exit code $LASTEXITCODE"
        }
    }

    Copy-ContextDocs -SourceRoot $effectiveRepoPath -DocsTargetPath $Docs -PromptsTargetPath $Prompts

    return $effectiveRepoPath
}

function Install-FromPortable {
    param(
        [string]$ArchivePath,
        [string]$Bin,
        [string]$Viewer,
        [string]$Prompts,
        [string]$Docs
    )

    if (-not (Test-Path $ArchivePath)) {
        throw "Portable archive not found: $ArchivePath"
    }

    $extractRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("ctx-portable-" + [Guid]::NewGuid().ToString("N"))
    New-Item -ItemType Directory -Path $extractRoot -Force | Out-Null

    if ($ArchivePath.EndsWith(".zip", [StringComparison]::OrdinalIgnoreCase)) {
        Expand-Archive -LiteralPath $ArchivePath -DestinationPath $extractRoot -Force
    }
    elseif ($ArchivePath.EndsWith(".tar.gz", [StringComparison]::OrdinalIgnoreCase)) {
        & tar -xzf $ArchivePath -C $extractRoot
    }
    else {
        throw "Unsupported archive format: $ArchivePath"
    }

    Copy-Item (Join-Path $extractRoot "bin\*") $Bin -Recurse -Force

    if (Test-Path (Join-Path $extractRoot "viewer")) {
        Copy-Item (Join-Path $extractRoot "viewer\*") $Viewer -Recurse -Force
    }

    if (Test-Path (Join-Path $extractRoot "prompts\CTX_HELPER_PROMPT.md")) {
        Copy-Item (Join-Path $extractRoot "prompts\CTX_HELPER_PROMPT.md") (Join-Path $Prompts "CTX_HELPER_PROMPT.md") -Force
    }

    if (Test-Path (Join-Path $extractRoot "prompts\CTX_AGENT_PROMPT.md")) {
        Copy-Item (Join-Path $extractRoot "prompts\CTX_AGENT_PROMPT.md") (Join-Path $Prompts "CTX_AGENT_PROMPT.md") -Force
    }

    if (Test-Path (Join-Path $extractRoot "docs\CTX_VIEWER_GUIDE.md")) {
        Copy-Item (Join-Path $extractRoot "docs\CTX_VIEWER_GUIDE.md") (Join-Path $Docs "CTX_VIEWER_GUIDE.md") -Force
    }

    if (Test-Path (Join-Path $extractRoot "docs\CTX_AUTONOMOUS_OPERATION_PROTOCOL.md")) {
        Copy-Item (Join-Path $extractRoot "docs\CTX_AUTONOMOUS_OPERATION_PROTOCOL.md") (Join-Path $Docs "CTX_AUTONOMOUS_OPERATION_PROTOCOL.md") -Force
    }

    return $extractRoot
}

Reset-InstallLayout -Root $InstallRoot -Bin $binPath -Viewer $viewerPath -Prompts $promptsPath -Docs $docsPath

$promptSource = ""
$sourceRoot = ""

if ($Mode -eq "source") {
    $sourceRoot = Install-FromSource -RepoPath $SourceRepoPath -CloneUrl $RepoUrl -Bin $binPath -Viewer $viewerPath -Docs $docsPath -Prompts $promptsPath -NoViewer:$SkipViewer
    $promptSource = Join-Path $sourceRoot $manifest.helperPrompt
    Copy-HelperPrompt -PromptSourcePath $promptSource -PromptTargetPath (Join-Path $promptsPath "CTX_HELPER_PROMPT.md")
}
else {
    $sourceRoot = Install-FromPortable -ArchivePath $BundlePath -Bin $binPath -Viewer $viewerPath -Prompts $promptsPath -Docs $docsPath
    $promptSource = Join-Path $promptsPath "CTX_HELPER_PROMPT.md"
    if (-not (Test-Path $promptSource)) {
        Copy-HelperPrompt -PromptSourcePath (Join-Path $repoRoot $manifest.helperPrompt) -PromptTargetPath $promptSource
    }
}

Write-WindowsLaunchers -Bin $binPath -Viewer $viewerPath -ViewerEndpoint $ViewerUrl -NoViewer:$SkipViewer
Write-InstallMetadata -Path $metadataPath -Root $InstallRoot -ModeName $Mode -SourceRoot $sourceRoot -PromptSource $promptSource -ViewerEndpoint $ViewerUrl
Add-BinToPath -Bin $binPath -Scope $PathScope

Write-Host "CTX installed to $InstallRoot via $Mode mode."
Write-Host "CLI launcher: $(Join-Path $binPath 'ctx.cmd')"
if (-not $SkipViewer) {
    Write-Host "Viewer launcher: $(Join-Path $binPath 'ctx-viewer.cmd')"
}
Write-Host "Context docs copied to: $docsPath"
Write-Host "PATH scope: $PathScope"
Write-Host "Open a new shell if your PATH has not refreshed yet."
