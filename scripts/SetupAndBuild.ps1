param(
    [string]$Configuration = "Release"
)

Write-Host "=== CloudQA Setup: Restore + Build (Configuration=$Configuration) ===" -ForegroundColor Cyan

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

Write-Host "\nRunning: dotnet restore" -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Package restore failed" -ForegroundColor Red
    exit 1
}
Write-Host "Success: Packages restored" -ForegroundColor Green

Write-Host "\nRunning: dotnet build --configuration $Configuration" -ForegroundColor Yellow
dotnet build --configuration $Configuration
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Build failed" -ForegroundColor Red
    exit 1
}
Write-Host "Success: Build successful (Configuration=$Configuration)" -ForegroundColor Green

Write-Host "\nSetup completed. You can now run tests with the RunAndSummarizeTests script." -ForegroundColor Cyan
Write-Host "Example: .\scripts\RunAndSummarizeTests.ps1 -Project .\CloudQAAutomation.csproj -Filter \"Category=FormSubmission\"" -ForegroundColor Gray
