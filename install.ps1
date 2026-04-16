param(
    [ValidateSet("install", "update", "repair", "auto")]
    [string]$Action = "auto",
    [ValidateSet("portable", "source", "auto")]
    [string]$Mode = "auto",
    [string]$InstallRoot = "",
    [string]$ManifestPath = "",
    [string]$ReleaseMetadataPath = "",
    [string]$BundlePath = "",
    [string]$SourceRepoPath = "",
    [string]$RepoUrl = "",
    [ValidateSet("Auto", "User", "Machine", "None")]
    [string]$PathScope = "Auto",
    [switch]$SkipViewer
)

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
if ([string]::IsNullOrWhiteSpace($ManifestPath)) {
    $ManifestPath = Join-Path $scriptRoot "distribution\version-manifest.json"
}

if (-not (Test-Path $ManifestPath)) {
    throw "Version manifest not found: $ManifestPath"
}

$manifest = Get-Content $ManifestPath -Raw | ConvertFrom-Json
$installManifestPath = Join-Path $scriptRoot "distribution\install-manifest.json"
$installManifest = Get-Content $installManifestPath -Raw | ConvertFrom-Json

if ([string]::IsNullOrWhiteSpace($InstallRoot)) {
    $InstallRoot = $installManifest.installRoots.windows
}

if ([string]::IsNullOrWhiteSpace($RepoUrl)) {
    $RepoUrl = $manifest.repo.publicCloneUrl
}

function Resolve-WindowsAssetKey {
    $arch = [System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture.ToString().ToLowerInvariant()
    $assetKey = switch ($arch) {
        "x64" { "win-x64" }
        "x86" { "win-x86" }
        default { "win-x64" }
    }

    return $assetKey
}

function Get-ReleaseMetadata {
    param(
        [psobject]$VersionManifest,
        [string]$MetadataPath
    )

    if (-not [string]::IsNullOrWhiteSpace($MetadataPath)) {
        return Get-Content $MetadataPath -Raw | ConvertFrom-Json
    }

    $headers = @{
        "User-Agent" = "ctx-installer"
        "Accept" = "application/vnd.github+json"
    }

    return Invoke-RestMethod -Uri $VersionManifest.repo.latestReleaseApi -Headers $headers -Method Get
}

function Normalize-ReleaseVersion {
    param([string]$TagName)

    if ([string]::IsNullOrWhiteSpace($TagName)) {
        return ""
    }

    return $TagName.TrimStart('v', 'V')
}

function Resolve-PortableDownloadUrl {
    param(
        [psobject]$ReleaseMetadata,
        [string]$AssetName
    )

    $asset = $ReleaseMetadata.assets | Where-Object { $_.name -eq $AssetName } | Select-Object -First 1
    if ($null -eq $asset) {
        throw "Release asset '$AssetName' was not found in GitHub release metadata."
    }

    return $asset.browser_download_url
}

function Download-ReleaseAsset {
    param(
        [string]$AssetUrl,
        [string]$AssetName
    )

    $downloadRoot = Join-Path ([System.IO.Path]::GetTempPath()) "ctx-release-downloads"
    New-Item -ItemType Directory -Path $downloadRoot -Force | Out-Null
    $destination = Join-Path $downloadRoot $AssetName
    $headers = @{
        "User-Agent" = "ctx-installer"
        "Accept" = "application/octet-stream"
    }

    Invoke-WebRequest -Uri $AssetUrl -Headers $headers -OutFile $destination
    return $destination
}

$installMetadataPath = Join-Path $InstallRoot $installManifest.metadataFile
$installedVersion = $null

if (Test-Path $installMetadataPath) {
    try {
        $installedMetadata = Get-Content $installMetadataPath -Raw | ConvertFrom-Json
        $installedVersion = $installedMetadata.version
    }
    catch {
        $installedVersion = $null
    }
}

$releaseMetadata = Get-ReleaseMetadata -VersionManifest $manifest -MetadataPath $ReleaseMetadataPath
$releaseVersion = Normalize-ReleaseVersion -TagName $releaseMetadata.tag_name

$effectiveAction = $Action
if ($effectiveAction -eq "auto") {
    if (-not (Test-Path $installMetadataPath)) {
        $effectiveAction = "install"
    }
    elseif ($installedVersion -ne $releaseVersion) {
        $effectiveAction = "update"
    }
    else {
        $effectiveAction = "repair"
    }
}

$effectiveMode = $Mode
if ($effectiveMode -eq "auto") {
    if (-not [string]::IsNullOrWhiteSpace($BundlePath)) {
        $effectiveMode = "portable"
    }
    else {
        $effectiveMode = "source"
    }
}

if ($effectiveMode -eq "portable" -and [string]::IsNullOrWhiteSpace($BundlePath)) {
    $assetKey = Resolve-WindowsAssetKey
    $assetName = $manifest.assets.$assetKey
    if ([string]::IsNullOrWhiteSpace($assetName)) {
        throw "No $assetKey asset declared in version manifest."
    }
    $assetUrl = Resolve-PortableDownloadUrl -ReleaseMetadata $releaseMetadata -AssetName $assetName
    $BundlePath = Download-ReleaseAsset -AssetUrl $assetUrl -AssetName $assetName
}

$statusLine = switch ($effectiveAction) {
    "install" { "Installing CTX $releaseVersion..." }
    "update" { "Updating CTX from $installedVersion to $releaseVersion..." }
    "repair" { "Repairing CTX $releaseVersion..." }
    default { "Running CTX bootstrap..." }
}

Write-Host $statusLine
Write-Host "Mode: $effectiveMode"
Write-Host "Install root: $InstallRoot"
Write-Host "Release tag: $($releaseMetadata.tag_name)"
if ($effectiveMode -eq "portable") {
    Write-Host "Portable asset: $BundlePath"
}

$installScript = Join-Path $scriptRoot "scripts\install-ctx.ps1"
$params = @{
    Mode = $effectiveMode
    InstallRoot = $InstallRoot
    RepoUrl = $RepoUrl
    VersionLabel = $releaseVersion
    PathScope = $PathScope
    SkipViewer = $SkipViewer
}

if (-not [string]::IsNullOrWhiteSpace($BundlePath)) {
    $params.BundlePath = $BundlePath
}

if (-not [string]::IsNullOrWhiteSpace($SourceRepoPath)) {
    $params.SourceRepoPath = $SourceRepoPath
}

& $installScript @params

Write-Host "CTX bootstrap complete."
