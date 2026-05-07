param(
    [Parameter(Mandatory = $true)]
    [ValidatePattern('^\d+\.\d+\.\d+$')]
    [string]$Version,

    [switch]$UpdateReleaseNotes
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$assemblyVersion = "$Version.0"
$utf8NoBom = [System.Text.UTF8Encoding]::new($false)

function Update-RequiredText {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,

        [Parameter(Mandatory = $true)]
        [string]$Pattern,

        [Parameter(Mandatory = $true)]
        [string]$Replacement,

        [Parameter(Mandatory = $true)]
        [string]$Description
    )

    $absolutePath = Join-Path $repoRoot $Path
    if (-not (Test-Path $absolutePath)) {
        throw "Missing file for $Description`: $Path"
    }

    $content = [System.IO.File]::ReadAllText($absolutePath, $utf8NoBom)
    if ($content -notmatch $Pattern) {
        throw "Could not find $Description in $Path"
    }

    $updated = [regex]::Replace($content, $Pattern, $Replacement, 1)
    [System.IO.File]::WriteAllText($absolutePath, $updated, $utf8NoBom)
}

Update-RequiredText `
    -Path "Directory.Build.props" `
    -Pattern '(<Version>)[^<]+(</Version>)' `
    -Replacement "`${1}$Version`${2}" `
    -Description "package version"

Update-RequiredText `
    -Path "Directory.Build.props" `
    -Pattern '(<AssemblyVersion>)[^<]+(</AssemblyVersion>)' `
    -Replacement "`${1}$assemblyVersion`${2}" `
    -Description "assembly version"

Update-RequiredText `
    -Path "Directory.Build.props" `
    -Pattern '(<FileVersion>)[^<]+(</FileVersion>)' `
    -Replacement "`${1}$assemblyVersion`${2}" `
    -Description "file version"

Update-RequiredText `
    -Path "Directory.Build.props" `
    -Pattern '(<InformationalVersion>)[^<]+(</InformationalVersion>)' `
    -Replacement "`${1}$Version`${2}" `
    -Description "informational version"

Update-RequiredText `
    -Path "Ctx.Domain\Model.cs" `
    -Pattern '(ProductVersion\s*=\s*")[^"]+(";\s*// CTX_RELEASE_VERSION)' `
    -Replacement "`${1}$Version`${2}" `
    -Description "DomainConstants.ProductVersion"

Update-RequiredText `
    -Path "distribution\windows\ctx.iss" `
    -Pattern '(#define AppVersion\s+")[^"]+(")' `
    -Replacement "`${1}$Version`${2}" `
    -Description "Windows installer version"

Update-RequiredText `
    -Path "README.md" `
    -Pattern '(Current version:\s*`)[^`]+(`)' `
    -Replacement "`${1}$Version`${2}" `
    -Description "README current version"

if ($UpdateReleaseNotes) {
    $releaseName = "RELEASE_$($Version.Replace('.', '_')).md"
    $releasePath = Join-Path "docs" $releaseName
    Update-RequiredText `
        -Path $releasePath `
        -Pattern '(# CTX )\d+\.\d+\.\d+( Release Notes)' `
        -Replacement "`${1}$Version`${2}" `
        -Description "release notes title"
}

Write-Host "Updated CTX release version surfaces to $Version"
