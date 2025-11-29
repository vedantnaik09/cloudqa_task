param(
    [string]$Project = ".\CloudQAAutomation.csproj",
    [string]$Filter = ""
)

# Ensure TestResults folder exists
$testResultsDir = Join-Path -Path (Get-Item .).FullName -ChildPath "TestResults"
if (-not (Test-Path $testResultsDir)) { New-Item -Path $testResultsDir -ItemType Directory | Out-Null }

# Remove old results.trx if present
$trxFileName = "results.trx"
$oldTrx = Get-ChildItem -Path $testResultsDir -Filter $trxFileName -ErrorAction SilentlyContinue
if ($oldTrx) { Remove-Item -Path $oldTrx.FullName -Force -ErrorAction SilentlyContinue }

# Build dotnet test arguments
$dotnetArgs = @('test', $Project, '--logger', "trx;LogFileName=$trxFileName")
if ($Filter -ne "") { $dotnetArgs = @('test', $Project, '--filter', $Filter, '--logger', "trx;LogFileName=$trxFileName") }

Write-Host "Running: dotnet $($dotnetArgs -join ' ')`n"

# Run tests
$proc = Start-Process -FilePath dotnet -ArgumentList $dotnetArgs -NoNewWindow -Wait -PassThru
$exitCode = $proc.ExitCode

# Locate most recent TRX file (prefer our TestResults folder)
$trx = Get-ChildItem -Path $testResultsDir -Filter *.trx -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending | Select-Object -First 1
if (-not $trx) {
    # fallback: search workspace
    $trx = Get-ChildItem -Path . -Recurse -Filter *.trx -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending | Select-Object -First 1
}

if (-not $trx) {
    Write-Host "No .trx file found. dotnet test exit code: $exitCode"
    exit $exitCode
}

Write-Host "Found TRX: $($trx.FullName)`n"

# Parse TRX XML
try {
    [xml]$doc = Get-Content $trx.FullName
} catch {
    Write-Host "Failed to read TRX XML: $_"
    exit $exitCode
}

# UnitTestResult nodes are typically at TestRun/Results/UnitTestResult
$unitResults = @()
if ($doc.TestRun -and $doc.TestRun.Results -and $doc.TestRun.Results.UnitTestResult) {
    $unitResults = $doc.TestRun.Results.UnitTestResult
} elseif ($doc.TestRun -and $doc.TestRun.Results) {
    # handle cases where Results may have other structure
    $unitResults = $doc.SelectNodes('//UnitTestResult')
}

$passed = @()
$failed = @()
$skipped = @()

foreach ($r in $unitResults) {
    $name = $r.testName
    $outcome = $r.outcome
    switch ($outcome) {
        'Passed' { $passed += $name }
        'Failed' { $failed += [PSCustomObject]@{ Name = $name; Node = $r } }
        default  { $skipped += $name }
    }
}

# Print summary and lists
Write-Host "=== TRX Summary ==="
Write-Host "Passed: $($passed.Count)  Failed: $($failed.Count)  Skipped/Other: $($skipped.Count)  Total: $($unitResults.Count)`n"

if ($passed.Count -gt 0) {
    Write-Host "Passed tests:" -ForegroundColor Green
    foreach ($p in $passed) { Write-Host " - $p" }
    Write-Host ""
}

if ($failed.Count -gt 0) {
    Write-Host "Failed tests:" -ForegroundColor Red
    foreach ($f in $failed) {
        $n = $f.Name
        Write-Host " - $n"
        # Attempt to show failure message if present
        $msg = $null
        try {
            $msg = $f.Node.Output.ErrorInfo.Message
        } catch { }
        if (-not $msg) {
            try { $msg = $f.Node.Output.StdOut } catch { }
        }
        if ($msg) { Write-Host "    Message: $msg`n" }
    }
    Write-Host ""
}

if ($skipped.Count -gt 0) {
    Write-Host "Skipped/Other tests:" -ForegroundColor Yellow
    foreach ($s in $skipped) { Write-Host " - $s" }
    Write-Host ""
}

# Exit with original dotnet test exit code
exit $exitCode
