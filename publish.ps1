param(
    [string]$Version = "",
    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path

# Version from Directory.Build.props
$propsPath = Join-Path $RepoRoot "Directory.Build.props"
if ([string]::IsNullOrEmpty($Version)) {
    $xml = [xml](Get-Content $propsPath)
    $Version = $xml.Project.PropertyGroup.Version
}

$OutputDir = Join-Path $RepoRoot "publish"
$Rid = "win-x64"

Write-Host "=== MediaVacuum Publisher ===" -ForegroundColor Cyan
Write-Host "Version : $Version"
Write-Host "Output  : $OutputDir"
Write-Host "RID     : $Rid"
Write-Host ""

if (-not $SkipBuild) {
    Write-Host ">>> Restoring packages..." -ForegroundColor Yellow
    dotnet restore $RepoRoot
    if ($LASTEXITCODE -ne 0) { throw "Restore failed" }

    Write-Host ">>> Building + Publishing (single-file, self-contained)..." -ForegroundColor Yellow
    & dotnet publish "$RepoRoot\src\MediaVacuum\MediaVacuum.csproj" `
        -c Release `
        --self-contained true `
        -r $Rid `
        -p:PublishSingleFile=true `
        -p:IncludeNativeLibrariesForSelfExtract=true `
        -p:DebugType=embedded `
        -o $OutputDir
    if ($LASTEXITCODE -ne 0) { throw "Publish failed" }
}

# Create release archive
$archiveName = "MediaVacuum-$Version-$Rid.zip"
$archivePath = Join-Path $RepoRoot $archiveName
Write-Host ">>> Creating archive: $archiveName ..." -ForegroundColor Yellow

# Remove old archive if exists
Remove-Item -LiteralPath $archivePath -Force -ErrorAction SilentlyContinue

Compress-Archive -Path "$OutputDir\*" `
    -DestinationPath $archivePath

Write-Host ""
Write-Host "=== Done ===" -ForegroundColor Green
Write-Host "Archive  : $archivePath"
Write-Host "Size     : $('{0:N2}' -f ((Get-Item $archivePath).Length / 1MB)) MB"
Write-Host ""
Write-Host "To create a GitHub release:" -ForegroundColor Cyan
Write-Host "  gh release create v$Version --title 'v$Version' --notes-file RELEASE_NOTES.md $archivePath"
