#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Build ShiftSharp release DLL and NuGet package

.DESCRIPTION
    This script builds the ShiftSharp project in Release configuration,
    creating the DLL and NuGet package.

.PARAMETER Clean
    Clean before building

.PARAMETER SkipTests
    Skip running unit tests

.EXAMPLE
    .\build-release.ps1
    .\build-release.ps1 -Clean
    .\build-release.ps1 -SkipTests
#>

param(
    [switch]$Clean,
    [switch]$SkipTests
)

$ErrorActionPreference = "Stop"

# Script directory
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectPath = Join-Path $scriptDir "ShiftSharp\ShiftSharp.csproj"
$testProjectPath = Join-Path $scriptDir "TestShiftSharp\TestShiftSharp.csproj"
$outputDir = Join-Path $scriptDir "ShiftSharp\bin\Release\net8.0"

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  ShiftSharp Release Build Script" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Clean if requested
if ($Clean) {
    Write-Host "Cleaning solution..." -ForegroundColor Yellow
    dotnet clean $solutionPath --configuration Release
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    Write-Host "Clean complete." -ForegroundColor Green
    Write-Host ""
}

# Restore NuGet packages
Write-Host "Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore $solutionPath
if ($LASTEXITCODE -ne 0) { 
    Write-Host "Restore failed!" -ForegroundColor Red
    exit $LASTEXITCODE 
}
Write-Host "Restore complete." -ForegroundColor Green
Write-Host ""

# Build in Release configuration
Write-Host "Building solution in Release configuration..." -ForegroundColor Yellow
dotnet build $solutionPath --configuration Release --no-restore
if ($LASTEXITCODE -ne 0) { 
    Write-Host "Build failed!" -ForegroundColor Red
    exit $LASTEXITCODE 
}
Write-Host "Build complete." -ForegroundColor Green
Write-Host ""

# Run tests unless skipped
if (-not $SkipTests) {
    Write-Host "Running unit tests..." -ForegroundColor Yellow
    dotnet test $testProjectPath --configuration Release --no-build --verbosity normal
    if ($LASTEXITCODE -ne 0) { 
        Write-Host "Tests failed!" -ForegroundColor Red
        exit $LASTEXITCODE 
    }
    Write-Host "All tests passed." -ForegroundColor Green
    Write-Host ""
}

# Display output information
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  Build Summary" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

if (Test-Path $outputDir) {
    $dllPath = Join-Path $outputDir "ShiftSharp.dll"
    $nupkgPath = Get-ChildItem -Path (Join-Path $scriptDir "ShiftSharp\bin\Release") -Filter "*.nupkg" -Recurse | Sort-Object LastWriteTime -Descending | Select-Object -First 1
    
    if (Test-Path $dllPath) {
        $fileInfo = Get-Item $dllPath
        Write-Host "DLL Location: " -NoNewline -ForegroundColor Yellow
        Write-Host $dllPath -ForegroundColor White
        Write-Host "DLL Size:     " -NoNewline -ForegroundColor Yellow
        Write-Host "$([math]::Round($fileInfo.Length / 1KB, 2)) KB" -ForegroundColor White
        Write-Host "Built:        " -NoNewline -ForegroundColor Yellow
        Write-Host $fileInfo.LastWriteTime -ForegroundColor White
    }
    
    if ($nupkgPath) {
        Write-Host ""
        Write-Host "NuGet Package: " -NoNewline -ForegroundColor Yellow
        Write-Host $nupkgPath.FullName -ForegroundColor White
        Write-Host "Package Size:  " -NoNewline -ForegroundColor Yellow
        Write-Host "$([math]::Round($nupkgPath.Length / 1KB, 2)) KB" -ForegroundColor White
    }
}

Write-Host ""
Write-Host "Build completed successfully!" -ForegroundColor Green
