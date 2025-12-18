# Publish script for PanoramicData Visual Studio WGSL Language Support

param(
    [switch]$SkipTests
)

# Exit on any error
$ErrorActionPreference = "Stop"

Write-Host "Starting publish process..." -ForegroundColor Green

# Step 1: Check for git porcelain (clean working directory)
Write-Host "Checking git status..." -ForegroundColor Yellow
$gitStatus = & git status --porcelain
if ($gitStatus) {
    Write-Error "Git working directory is not clean. Please commit or stash changes."
    exit 1
}
Write-Host "Git working directory is clean." -ForegroundColor Green

# Step 2: Determine the Nerdbank git version
Write-Host "Determining version..." -ForegroundColor Yellow
try {
    $versionInfo = & nbgv get-version --format json | ConvertFrom-Json
    $version = $versionInfo.Version
    Write-Host "Version: $version" -ForegroundColor Green
} catch {
    Write-Error "Failed to get version from Nerdbank.GitVersioning: $_"
    exit 1
}

# Step 3: Check that nuget-key.txt exists, has content and is gitignored
Write-Host "Checking nuget-key.txt..." -ForegroundColor Yellow
$nugetKeyPath = "nuget-key.txt"
if (!(Test-Path $nugetKeyPath)) {
    Write-Error "nuget-key.txt does not exist."
    exit 1
}

$keyContent = Get-Content $nugetKeyPath -Raw
if ([string]::IsNullOrWhiteSpace($keyContent)) {
    Write-Error "nuget-key.txt is empty."
    exit 1
}

$gitIgnoreContent = Get-Content .gitignore -Raw
if ($gitIgnoreContent -notmatch "nuget-key\.txt") {
    Write-Error "nuget-key.txt is not in .gitignore."
    exit 1
}
Write-Host "nuget-key.txt is valid and gitignored." -ForegroundColor Green

# Step 4: Run unit tests (unless -SkipTests is specified)
if (!$SkipTests) {
    Write-Host "Running unit tests..." -ForegroundColor Yellow
    & dotnet test --configuration Release --no-build
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Unit tests failed."
        exit 1
    }
    Write-Host "Unit tests passed." -ForegroundColor Green
} else {
    Write-Host "Skipping unit tests." -ForegroundColor Yellow
}

# Step 5: Publish to nuget.org
Write-Host "Publishing to nuget.org..." -ForegroundColor Yellow

# First, pack the main project
& msbuild PanoramicData.VisualStudio.WgslLanguageSupport\PanoramicData.VisualStudio.WgslLanguageSupport.csproj /t:Pack /p:Configuration=Release
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to pack the project."
    exit 1
}

# Find the nupkg file
$nupkgPath = Get-ChildItem -Path "PanoramicData.VisualStudio.WgslLanguageSupport\bin\Release" -Filter "*.nupkg" | Select-Object -First 1
if (!$nupkgPath) {
    Write-Error "No .nupkg file found after packing."
    exit 1
}

# Publish to nuget.org
& dotnet nuget push $nupkgPath.FullName --api-key $keyContent.Trim() --source https://api.nuget.org/v3/index.json
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to publish to nuget.org."
    exit 1
}

Write-Host "Successfully published version $version to nuget.org!" -ForegroundColor Green
exit 0