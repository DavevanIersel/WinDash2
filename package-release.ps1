# Package releases for GitHub distribution
param(
    [ValidateSet('x64', 'x86', 'ARM64', 'All')]
    [string]$Platform = 'x64'
)

$ErrorActionPreference = 'Stop'

$platforms = if ($Platform -eq 'All') { @('x64', 'x86', 'ARM64') } else { @($Platform) }

# Create release folder if it doesn't exist
if (-not (Test-Path "release")) {
    New-Item -ItemType Directory -Path "release" | Out-Null
}

Write-Host "Packaging WinDash2 for GitHub Release" -ForegroundColor Cyan

foreach ($plat in $platforms) {
    $rid = "win-$($plat.ToLower())"
    $sourcePath = "bin\Release\portable\$rid"
    $zipName = "release\WinDash2-portable-$rid.zip"
    
    if (Test-Path $sourcePath) {
        Write-Host "`nPackaging $plat..." -ForegroundColor Green
        
        # Remove old zip if exists
        if (Test-Path $zipName) {
            Remove-Item $zipName -Force
        }
        
        # Create zip
        Compress-Archive -Path "$sourcePath\*" -DestinationPath $zipName -CompressionLevel Optimal
        
        $size = (Get-Item $zipName).Length / 1MB
        $sizeRounded = [math]::Round($size, 2)
        Write-Host "Created $zipName ($sizeRounded MB)" -ForegroundColor Green
    } else {
        Write-Warning "Build not found for $plat. Run: .\build-release.ps1 -Platform $plat"
    }
}

Write-Host "`n=== Ready for GitHub Release ===" -ForegroundColor Cyan
Write-Host "Files in release/ folder:" -ForegroundColor Yellow
Get-ChildItem -Path "release" -Filter "WinDash2-portable-*.zip" | ForEach-Object {
    $sizeMB = [math]::Round($_.Length / 1MB, 2)
    Write-Host "  $($_.Name) - $sizeMB MB" -ForegroundColor Yellow
}
