# CloudQA Automation - Quick Start Script

Write-Host "=== CloudQA Automation Test Suite ===" -ForegroundColor Cyan
Write-Host ""

# Check .NET installation
Write-Host "Checking .NET installation..." -ForegroundColor Yellow
$dotnetVersion = dotnet --version 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "Success: .NET SDK installed: $dotnetVersion" -ForegroundColor Green
} else {
    Write-Host "Error: .NET SDK not found. Please install .NET 8.0 SDK" -ForegroundColor Red
    Write-Host "Download from: https://dotnet.microsoft.com/download" -ForegroundColor Yellow
    exit 1
}

Write-Host ""
Write-Host "Step 1: Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Package restore failed" -ForegroundColor Red
    exit 1
}
Write-Host "Success: Packages restored" -ForegroundColor Green

Write-Host ""
Write-Host "Step 2: Building project..." -ForegroundColor Yellow
dotnet build --configuration Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Build failed" -ForegroundColor Red
    exit 1
}
Write-Host "Success: Build successful" -ForegroundColor Green

Write-Host ""
Write-Host "Step 3: Running tests..." -ForegroundColor Yellow
Write-Host ""

# Create logs directory
if (!(Test-Path "logs/html-snippets")) {
    New-Item -ItemType Directory -Path "logs/html-snippets" -Force | Out-Null
}

# Run tests with detailed output
dotnet test --configuration Release --logger "console;verbosity=detailed"

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "=== All Tests Passed! ===" -ForegroundColor Green
    Write-Host ""
    Write-Host "HTML snapshots saved to: logs/html-snippets/" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "To run specific test categories:" -ForegroundColor Yellow
    Write-Host "  dotnet test --filter Category=Analysis" -ForegroundColor Gray
    Write-Host "  dotnet test --filter Category=FirstName" -ForegroundColor Gray
    Write-Host "  dotnet test --filter Category=Email" -ForegroundColor Gray
    Write-Host "  dotnet test --filter Category=State" -ForegroundColor Gray
    Write-Host ""
} else {
    Write-Host ""
    Write-Host "=== Some Tests Failed ===" -ForegroundColor Red
    Write-Host "Check the output above for details" -ForegroundColor Yellow
    Write-Host "Review HTML snapshots in: logs/html-snippets/" -ForegroundColor Cyan
    exit 1
}
