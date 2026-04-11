param(
    [string]$ResultsDirectory = "TestResults/Coverage"
)

$ErrorActionPreference = "Stop"

$rootPath = Split-Path -Parent $PSScriptRoot
$dotnetCliHome = Join-Path $rootPath ".dotnet"
$testProjectPath = Join-Path $rootPath "tests/StudyNotesApi.UnitTests/StudyNotesApi.UnitTests.csproj"
$resolvedResultsDirectory = Join-Path $rootPath $ResultsDirectory
$coverageReportDirectory = Join-Path $rootPath "TestResults/CoverageReport"
$coverageReportPath = Join-Path $resolvedResultsDirectory "coverage.cobertura.xml"
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
    "*/WebApplicationExtensions.cs",
    "*/tests/*"
)

if (-not (Test-Path $dotnetCliHome)) {
    New-Item -ItemType Directory -Path $dotnetCliHome | Out-Null
}

$env:DOTNET_CLI_HOME = $dotnetCliHome

if (Test-Path $resolvedResultsDirectory) {
    Remove-Item -Recurse -Force $resolvedResultsDirectory
}

if (Test-Path $coverageReportDirectory) {
    Remove-Item -Recurse -Force $coverageReportDirectory
}

Write-Host "Restoring local .NET tools..." -ForegroundColor Cyan
& dotnet tool restore

if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

Write-Host "Running unit tests with coverage..." -ForegroundColor Cyan
& dotnet dotnet-coverage collect dotnet test $testProjectPath -c Release --no-restore --output $coverageReportPath --output-format cobertura

if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

if (-not (Test-Path $coverageReportPath)) {
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
        $displayFile = if ($fileName.StartsWith($rootPath, [System.StringComparison]::OrdinalIgnoreCase)) {
            $fileName.Substring($rootPath.Length).TrimStart('\', '/')
        } else {
            $fileName
        }

        $fileCoverage[$fileName] = [PSCustomObject]@{
            File = $displayFile
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
