# Build script for WinDash2 releases
# Creates portable executable releases

param(
    [ValidateSet('x64', 'x86', 'ARM64', 'All')]
    [string]$Platform = 'x64'
)

$ErrorActionPreference = 'Stop'

Write-Host "Building WinDash2 - Platform: $Platform" -ForegroundColor Cyan

# Platforms to build
$platforms = if ($Platform -eq 'All') { @('x64', 'x86', 'ARM64') } else { @($Platform) }

foreach ($plat in $platforms) {
    $rid = "win-$($plat.ToLower())"
    
    Write-Host ""
    Write-Host "Building Portable for $plat..." -ForegroundColor Green
    dotnet publish -c Release -r $rid --self-contained true /p:Platform=$plat /p:PublishProfile=portable-$rid
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Portable $plat build completed: bin\Release\portable\$rid" -ForegroundColor Green
    } else {
        Write-Error "Portable $plat build failed!"
    }
}

Write-Host ""
Write-Host "=== Build Summary ===" -ForegroundColor Cyan
Write-Host "Portable builds: bin\Release\portable" -ForegroundColor Yellow
