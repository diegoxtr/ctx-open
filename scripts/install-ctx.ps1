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
$mcpPath = Join-Path $InstallRoot $manifest.paths.mcp
$acpPath = Join-Path $InstallRoot $manifest.paths.acp
$viewerPath = Join-Path $InstallRoot $manifest.paths.viewer
$promptsPath = Join-Path $InstallRoot $manifest.paths.prompts
$docsPath = Join-Path $InstallRoot $manifest.paths.docs
$metadataPath = Join-Path $InstallRoot $manifest.metadataFile

function Reset-InstallLayout {
    param(
        [string]$Root,
        [string]$Bin,
        [string]$Mcp,
        [string]$Acp,
        [string]$Viewer,
        [string]$Prompts,
        [string]$Docs
    )

    New-Item -ItemType Directory -Path $Root -Force | Out-Null

    foreach ($path in @($Bin, $Mcp, $Acp, $Viewer, $Prompts, $Docs)) {
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

    $docsSource = Join-Path $SourceRoot "docs"
    if (-not (Test-Path $docsSource)) {
        throw "Docs folder not found: $docsSource"
    }

    Copy-Item (Join-Path $docsSource "*") $DocsTargetPath -Recurse -Force

    $agentPromptSource = Join-Path $SourceRoot "prompts\CTX_AGENT_PROMPT.md"
    if (-not (Test-Path $agentPromptSource)) {
        throw "Required context asset not found: $agentPromptSource"
    }

    Copy-Item $agentPromptSource (Join-Path $PromptsTargetPath "CTX_AGENT_PROMPT.md") -Force
}

function Write-WindowsLaunchers {
    param(
        [string]$Bin,
        [string]$Mcp,
        [string]$Acp,
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

    $mcpLauncher = @"
@echo off
setlocal
"$Mcp\Ctx.Mcp.exe" %*
endlocal
"@
    Set-Content -LiteralPath (Join-Path $Bin "ctx-mcp.cmd") -Value $mcpLauncher -Encoding ASCII

    $acpLauncher = @"
@echo off
setlocal
"$Acp\Ctx.Agent.Acp.exe" %*
endlocal
"@
    Set-Content -LiteralPath (Join-Path $Bin "ctx-agent-acp.cmd") -Value $acpLauncher -Encoding ASCII

    if ($NoViewer) {
        return
    }

    $viewerLauncher = @"
@echo off
setlocal
set "CTX_INSTALL_ROOT=$([System.IO.Path]::GetFullPath((Split-Path -Parent $Bin)))"
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
        [string]$Mcp,
        [string]$Acp,
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
    $mcpProject = Join-Path $effectiveRepoPath "Ctx.Mcp\Ctx.Mcp.csproj"
    $acpProject = Join-Path $effectiveRepoPath "Ctx.Agent.Acp\Ctx.Agent.Acp.csproj"
    $viewerProject = Join-Path $effectiveRepoPath "Ctx.Viewer\Ctx.Viewer.csproj"

    if (-not (Test-Path $cliProject)) {
        throw "Ctx.Cli project not found under source repo: $effectiveRepoPath"
    }

    if (-not (Test-Path $mcpProject)) {
        throw "Ctx.Mcp project not found under source repo: $effectiveRepoPath"
    }

    if (-not (Test-Path $acpProject)) {
        throw "Ctx.Agent.Acp project not found under source repo: $effectiveRepoPath"
    }

    & dotnet publish $cliProject -c Release -o $Bin | Out-Host
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish failed for Ctx.Cli with exit code $LASTEXITCODE"
    }

    & dotnet publish $mcpProject -c Release -o $Mcp | Out-Host
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish failed for Ctx.Mcp with exit code $LASTEXITCODE"
    }

    & dotnet publish $acpProject -c Release -o $Acp | Out-Host
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish failed for Ctx.Agent.Acp with exit code $LASTEXITCODE"
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
        [string]$Mcp,
        [string]$Acp,
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

    if (Test-Path (Join-Path $extractRoot "mcp")) {
        Copy-Item (Join-Path $extractRoot "mcp\*") $Mcp -Recurse -Force
    }
    elseif (Test-Path (Join-Path $extractRoot "bin\Ctx.Mcp.exe")) {
        Copy-Item (Join-Path $extractRoot "bin\Ctx.Mcp.exe") (Join-Path $Mcp "Ctx.Mcp.exe") -Force
    }

    if (Test-Path (Join-Path $extractRoot "acp")) {
        Copy-Item (Join-Path $extractRoot "acp\*") $Acp -Recurse -Force
    }
    elseif (Test-Path (Join-Path $extractRoot "bin\Ctx.Agent.Acp.exe")) {
        Copy-Item (Join-Path $extractRoot "bin\Ctx.Agent.Acp.exe") (Join-Path $Acp "Ctx.Agent.Acp.exe") -Force
    }

    if (Test-Path (Join-Path $extractRoot "viewer")) {
        Copy-Item (Join-Path $extractRoot "viewer\*") $Viewer -Recurse -Force
    }

    if (Test-Path (Join-Path $extractRoot "prompts\CTX_HELPER_PROMPT.md")) {
        Copy-Item (Join-Path $extractRoot "prompts\CTX_HELPER_PROMPT.md") (Join-Path $Prompts "CTX_HELPER_PROMPT.md") -Force
    }

    if (Test-Path (Join-Path $extractRoot "prompts\CTX_AGENT_PROMPT.md")) {
        Copy-Item (Join-Path $extractRoot "prompts\CTX_AGENT_PROMPT.md") (Join-Path $Prompts "CTX_AGENT_PROMPT.md") -Force
    }

    if (Test-Path (Join-Path $extractRoot "docs")) {
        Copy-Item (Join-Path $extractRoot "docs\*") $Docs -Recurse -Force
    }

    return $extractRoot
}

function Test-InstallLayout {
    param(
        [string]$Bin,
        [string]$Mcp,
        [string]$Acp,
        [string]$Viewer,
        [switch]$NoViewer
    )

    $cliExe = Join-Path $Bin "Ctx.Cli.exe"
    $mcpExe = Join-Path $Mcp "Ctx.Mcp.exe"
    $acpExe = Join-Path $Acp "Ctx.Agent.Acp.exe"
    $viewerExe = Join-Path $Viewer "Ctx.Viewer.exe"

    if (-not (Test-Path $cliExe)) {
        throw "Installed CLI executable not found: $cliExe"
    }

    if (-not (Test-Path $mcpExe)) {
        throw "Installed MCP executable not found: $mcpExe"
    }

    if (-not (Test-Path $acpExe)) {
        throw "Installed ACP executable not found: $acpExe"
    }

    if (-not $NoViewer -and -not (Test-Path $viewerExe)) {
        throw "Installed viewer executable not found: $viewerExe"
    }

    foreach ($requiredDoc in @("CTX_VIEWER_GUIDE.md", "CLI_COMMANDS.md", "CTX_AUTONOMOUS_OPERATION_PROTOCOL.md", "TECHNICAL_INDEX.md")) {
        $docPath = Join-Path (Split-Path -Parent $Bin) "docs\$requiredDoc"
        if (-not (Test-Path $docPath)) {
            throw "Installed console-referenced documentation not found: $docPath"
        }
    }
}

Reset-InstallLayout -Root $InstallRoot -Bin $binPath -Mcp $mcpPath -Acp $acpPath -Viewer $viewerPath -Prompts $promptsPath -Docs $docsPath

$promptSource = ""
$sourceRoot = ""

if ($Mode -eq "source") {
    $sourceRoot = Install-FromSource -RepoPath $SourceRepoPath -CloneUrl $RepoUrl -Bin $binPath -Mcp $mcpPath -Acp $acpPath -Viewer $viewerPath -Docs $docsPath -Prompts $promptsPath -NoViewer:$SkipViewer
    $promptSource = Join-Path $sourceRoot $manifest.helperPrompt
    Copy-HelperPrompt -PromptSourcePath $promptSource -PromptTargetPath (Join-Path $promptsPath "CTX_HELPER_PROMPT.md")
}
else {
    $sourceRoot = Install-FromPortable -ArchivePath $BundlePath -Bin $binPath -Mcp $mcpPath -Acp $acpPath -Viewer $viewerPath -Prompts $promptsPath -Docs $docsPath
    $promptSource = Join-Path $promptsPath "CTX_HELPER_PROMPT.md"
    if (-not (Test-Path $promptSource)) {
        Copy-HelperPrompt -PromptSourcePath (Join-Path $repoRoot $manifest.helperPrompt) -PromptTargetPath $promptSource
    }
}

Write-WindowsLaunchers -Bin $binPath -Mcp $mcpPath -Acp $acpPath -Viewer $viewerPath -ViewerEndpoint $ViewerUrl -NoViewer:$SkipViewer
Test-InstallLayout -Bin $binPath -Mcp $mcpPath -Acp $acpPath -Viewer $viewerPath -NoViewer:$SkipViewer
Write-InstallMetadata -Path $metadataPath -Root $InstallRoot -ModeName $Mode -SourceRoot $sourceRoot -PromptSource $promptSource -ViewerEndpoint $ViewerUrl
Add-BinToPath -Bin $binPath -Scope $PathScope

Write-Host "CTX installed to $InstallRoot via $Mode mode."
Write-Host "CTX_INSTALL_ROOT=$InstallRoot"
Write-Host "CTX_BIN_PATH=$binPath"
Write-Host "CTX_MCP_PATH=$mcpPath"
Write-Host "CTX_ACP_PATH=$acpPath"
Write-Host "CLI launcher: $(Join-Path $binPath 'ctx.cmd')"
Write-Host "MCP launcher: $(Join-Path $binPath 'ctx-mcp.cmd')"
Write-Host "ACP launcher: $(Join-Path $binPath 'ctx-agent-acp.cmd')"
if (-not $SkipViewer) {
    Write-Host "Viewer launcher: $(Join-Path $binPath 'ctx-viewer.cmd')"
}
Write-Host "Context docs copied to: $docsPath"
Write-Host "PATH scope: $PathScope"
Write-Host "Open a new shell if your PATH has not refreshed yet."
