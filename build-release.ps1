param(
    [string]$Version = "1.0.0"
)

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot
$publish = Join-Path $root "publish"
$release = Join-Path $root "release"
$project = Join-Path $root "DiddyFlow.csproj"

Write-Host "==> Publishing Diddy Flow $Version (win-x64, self-contained)"
dotnet publish $project -c Release -r win-x64 --self-contained true -p:PublishReadyToRun=true -o $publish

Write-Host "==> Trimming runtimes we do not ship"
$trim = @(
    (Join-Path $publish "libvlc\win-arm64"),
    (Join-Path $publish "libvlc\win-x86"),
    (Join-Path $publish "runtimes\win-arm64"),
    (Join-Path $publish "runtimes\win-x86"),
    (Join-Path $publish "runtimes\vulkan\linux-x64")
)
foreach ($path in $trim) {
    if (Test-Path -LiteralPath $path) {
        Remove-Item -LiteralPath $path -Recurse -Force
    }
}

New-Item -ItemType Directory -Force -Path $release | Out-Null
$zip = Join-Path $release "DiddyFlow-$Version-win-x64.zip"
if (Test-Path -LiteralPath $zip) {
    Remove-Item -LiteralPath $zip -Force
}

Write-Host "==> Zipping"
Compress-Archive -Path (Join-Path $publish "*") -DestinationPath $zip -CompressionLevel Optimal

$size = [math]::Round((Get-Item -LiteralPath $zip).Length / 1MB, 1)
Write-Host ""
Write-Host "Release ready: $zip ($size MB)"
Write-Host "Attach that zip to your GitHub release."
