param(
    [string]$InstallRoot = "C:\ctx",
    [string]$ViewerUrl = "http://127.0.0.1:5271"
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$cliProject = Join-Path $repoRoot "Ctx.Cli\Ctx.Cli.csproj"
$viewerProject = Join-Path $repoRoot "Ctx.Viewer\Ctx.Viewer.csproj"
$binPath = Join-Path $InstallRoot "bin"
$viewerPath = Join-Path $InstallRoot "viewer"
$installedViewerExe = Join-Path $viewerPath "Ctx.Viewer.exe"
$restartInstalledViewer = $false

if (Test-Path $installedViewerExe) {
    $runningInstalledViewer = Get-Process |
        Where-Object { $_.Path -eq $installedViewerExe } |
        Select-Object -First 1

    if ($null -ne $runningInstalledViewer) {
        $restartInstalledViewer = $true
        Stop-Process -Id $runningInstalledViewer.Id -Force
        Start-Sleep -Milliseconds 500
    }
}

New-Item -ItemType Directory -Path $InstallRoot -Force | Out-Null

if (Test-Path $binPath) {
    Remove-Item -Recurse -Force $binPath
}

if (Test-Path $viewerPath) {
    Remove-Item -Recurse -Force $viewerPath
}

New-Item -ItemType Directory -Path $binPath -Force | Out-Null
New-Item -ItemType Directory -Path $viewerPath -Force | Out-Null

dotnet publish $cliProject -c Release -o $binPath
dotnet publish $viewerProject -c Release -o $viewerPath

$cliLauncher = @"
@echo off
setlocal
"$binPath\Ctx.Cli.exe" %*
endlocal
"@
Set-Content -Path (Join-Path $binPath "ctx.cmd") -Value $cliLauncher -Encoding ASCII

$viewerLauncher = @"
@echo off
setlocal
start "" "$ViewerUrl/"
pushd "$viewerPath"
"$viewerPath\Ctx.Viewer.exe" --urls $ViewerUrl
popd
endlocal
"@
Set-Content -Path (Join-Path $binPath "ctx-viewer.cmd") -Value $viewerLauncher -Encoding ASCII

$currentUserPath = [Environment]::GetEnvironmentVariable("Path", "User")
$pathEntries = @()
if (-not [string]::IsNullOrWhiteSpace($currentUserPath)) {
    $pathEntries = $currentUserPath.Split(';', [System.StringSplitOptions]::RemoveEmptyEntries)
}

if ($pathEntries -notcontains $binPath) {
    $newUserPath = (($pathEntries + $binPath) | Select-Object -Unique) -join ';'
    [Environment]::SetEnvironmentVariable("Path", $newUserPath, "User")
    $env:Path = "$binPath;$env:Path"
}

Write-Host "CTX published to $InstallRoot"
Write-Host "CLI path: $binPath"
Write-Host "Viewer path: $viewerPath"
Write-Host "Use 'ctx version' after opening a new shell."
Write-Host "Use 'ctx-viewer' to launch the bundled viewer."

if ($restartInstalledViewer) {
    Start-Process -FilePath (Join-Path $binPath "ctx-viewer.cmd")
    Write-Host "Restarted installed viewer."
}
