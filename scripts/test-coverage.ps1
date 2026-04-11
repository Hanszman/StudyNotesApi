param(
    [string]$ResultsDirectory = "TestResults/Coverage"
)

$ErrorActionPreference = "Stop"

$rootPath = Split-Path -Parent $PSScriptRoot
$testProjectPath = Join-Path $rootPath "tests/StudyNotesApi.UnitTests/StudyNotesApi.UnitTests.csproj"
$resolvedResultsDirectory = Join-Path $rootPath $ResultsDirectory
$coverageReportDirectory = Join-Path $rootPath "TestResults/CoverageReport"
$excludedPatterns = @(
    "*/Program.cs",
    "*/AssemblyReference.cs",
    "*/DTOs/*",
    "*/*Configuration.cs",
    "*/Data/Migrations/*",
    "*/EnvironmentFileLoader.cs",
    "*/ApplicationDbContext.cs",
    "*/ApplicationDbContextFactory.cs",
    "*/ServiceCollectionExtensions.cs",
    "*/WebApplicationExtensions.cs"
)

if (Test-Path $resolvedResultsDirectory) {
    Remove-Item -Recurse -Force $resolvedResultsDirectory
}

if (Test-Path $coverageReportDirectory) {
    Remove-Item -Recurse -Force $coverageReportDirectory
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

& dotnet tool run reportgenerator "-reports:$coverageReportPath" "-targetdir:$coverageReportDirectory" "-reporttypes:Html;TextSummary"

[xml]$coverageXml = Get-Content -Path $coverageReportPath
$fileCoverage = @{}

foreach ($classNode in $coverageXml.coverage.packages.package.classes.class) {
    $fileName = $classNode.filename

    if ([string]::IsNullOrWhiteSpace($fileName)) {
        continue
    }

    $normalizedFileName = $fileName.Replace('\', '/')
    $isExcluded = $false

    foreach ($pattern in $excludedPatterns) {
        if ($normalizedFileName -like $pattern) {
            $isExcluded = $true
            break
        }
    }

    if ($isExcluded) {
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

$includedFiles = $fileCoverage.Values | Where-Object { $_.TotalLines -gt 0 }
$coveredLines = ($includedFiles | Measure-Object -Property CoveredLines -Sum).Sum
$totalLines = ($includedFiles | Measure-Object -Property TotalLines -Sum).Sum
$overallCoverage = if ($totalLines -eq 0) { 1 } else { $coveredLines / $totalLines }
$filesBelowThreshold = $includedFiles | Where-Object { $_.CoveredLines -lt $_.TotalLines }

Write-Host ""
Write-Host "Coverage summary by file" -ForegroundColor Green
$summary | Format-Table -AutoSize

Write-Host ""
Write-Host ("Overall included-file line coverage: {0:P2}" -f $overallCoverage) -ForegroundColor Green
Write-Host "HTML report: TestResults/CoverageReport/index.html" -ForegroundColor DarkGray
Write-Host "Raw report file: $coverageReportPath" -ForegroundColor DarkGray

if ($filesBelowThreshold.Count -gt 0) {
    Write-Host ""
    Write-Host "Files below 100% coverage:" -ForegroundColor Red
    $filesBelowThreshold |
        Sort-Object File |
        Select-Object File, CoveredLines, TotalLines |
        Format-Table -AutoSize

    throw "Coverage validation failed because one or more included files are below 100%."
}
