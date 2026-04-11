param(
    [string]$ResultsDirectory = "TestResults/Coverage"
)

$ErrorActionPreference = "Stop"

$rootPath = Split-Path -Parent $PSScriptRoot
$testProjectPath = Join-Path $rootPath "tests/StudyNotesApi.UnitTests/StudyNotesApi.UnitTests.csproj"
$resolvedResultsDirectory = Join-Path $rootPath $ResultsDirectory

if (Test-Path $resolvedResultsDirectory) {
    Remove-Item -Recurse -Force $resolvedResultsDirectory
}

Write-Host "Running unit tests with coverage..." -ForegroundColor Cyan
& dotnet test $testProjectPath --collect:"XPlat Code Coverage" --results-directory $resolvedResultsDirectory

if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$coverageReportPath = Get-ChildItem -Path $resolvedResultsDirectory -Recurse -Filter "coverage.cobertura.xml" |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1 -ExpandProperty FullName

if (-not $coverageReportPath) {
    throw "Coverage report was not generated."
}

[xml]$coverageXml = Get-Content -Path $coverageReportPath
$fileCoverage = @{}

foreach ($classNode in $coverageXml.coverage.packages.package.classes.class) {
    $fileName = $classNode.filename

    if ([string]::IsNullOrWhiteSpace($fileName)) {
        continue
    }

    if (-not $fileCoverage.ContainsKey($fileName)) {
        $fileCoverage[$fileName] = [PSCustomObject]@{
            File = $fileName
            CoveredLines = 0
            TotalLines = 0
        }
    }

    foreach ($lineNode in $classNode.lines.line) {
        $fileCoverage[$fileName].TotalLines++
        if ([int]$lineNode.hits -gt 0) {
            $fileCoverage[$fileName].CoveredLines++
        }
    }
}

$summary = $fileCoverage.Values |
    Sort-Object File |
    Select-Object @{
            Name = "File"
            Expression = { $_.File }
        }, @{
            Name = "Coverage"
            Expression = {
                if ($_.TotalLines -eq 0) {
                    "n/a"
                } else {
                    "{0:P2}" -f ($_.CoveredLines / $_.TotalLines)
                }
            }
        }, @{
            Name = "CoveredLines"
            Expression = { $_.CoveredLines }
        }, @{
            Name = "TotalLines"
            Expression = { $_.TotalLines }
        }

$overallCoverage = [double]$coverageXml.coverage.'line-rate'

Write-Host ""
Write-Host "Coverage summary by file" -ForegroundColor Green
$summary | Format-Table -AutoSize

Write-Host ""
Write-Host ("Overall line coverage: {0:P2}" -f $overallCoverage) -ForegroundColor Green
Write-Host "Coverage report file: $coverageReportPath" -ForegroundColor DarkGray
