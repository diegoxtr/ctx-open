param(
    [string]$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path,
    [int]$StaleAfterSeconds = 30,
    [switch]$Force
)

$ErrorActionPreference = "Stop"

$gitDir = Join-Path $RepoRoot ".git"
$lockPath = Join-Path $gitDir "index.lock"

if (-not (Test-Path $gitDir)) {
    throw "No .git directory was found at '$gitDir'."
}

if (-not (Test-Path $lockPath)) {
    Write-Host "No index.lock is present in $RepoRoot"
    exit 0
}

$lockFile = Get-Item $lockPath
$lockAgeSeconds = [int]((Get-Date) - $lockFile.LastWriteTime).TotalSeconds
$gitProcesses = Get-Process git -ErrorAction SilentlyContinue | Select-Object Id, ProcessName, StartTime

Write-Host "Detected $lockPath"
Write-Host "Lock age: $lockAgeSeconds seconds"

if ($gitProcesses) {
    Write-Host "Active git.exe processes:"
    $gitProcesses | Format-Table -AutoSize | Out-String | Write-Host
}

if ($gitProcesses -and -not $Force) {
    throw "Refusing to remove index.lock while git.exe processes are still running. Re-run after they exit, or use -Force if you have already verified they are unrelated."
}

if (($lockAgeSeconds -lt $StaleAfterSeconds) -and -not $Force) {
    throw "index.lock is only $lockAgeSeconds seconds old. Re-run later or use -Force if you have verified it is orphaned."
}

Remove-Item -LiteralPath $lockPath -Force
Write-Host "Removed orphaned $lockPath"
