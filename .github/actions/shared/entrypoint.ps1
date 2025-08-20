#!/usr/bin/env pwsh

Write-Host "Starting BHoM Compliance Check..."
Write-Host "Check Type: $env:CHECK_TYPE"
Write-Host "Input Files: $env:INPUT_FILES"

# Function to get changed files if no specific files provided
function Get-ChangedFiles {
    param([string]$FilePattern)
    
    if ($env:INPUT_FILES) {
        return $env:INPUT_FILES
    }
    
    # Get changed files from git
    $changedFiles = @()
    
    if ($env:GITHUB_EVENT_NAME -eq "pull_request") {
        # For pull requests, get files changed between base and head
        git fetch origin $env:GITHUB_BASE_REF --depth=1
        $changedFiles = git diff --name-only "origin/$env:GITHUB_BASE_REF...HEAD" | Where-Object { $_ -match $FilePattern }
    } else {
        # For push events, get files changed in the last commit
        $changedFiles = git diff --name-only HEAD~1 HEAD | Where-Object { $_ -match $FilePattern }
    }
    
    return $changedFiles -join " "
}

# Determine file pattern and get files to check
$filePattern = if ($env:CHECK_TYPE -eq "project") { "\.csproj$" } else { "\.cs$" }
$filesToCheck = Get-ChangedFiles -FilePattern $filePattern

if (-not $filesToCheck -or $filesToCheck.Trim() -eq "") {
    Write-Host "No files to check for $env:CHECK_TYPE compliance."
    $emptyResult = @{
        FileResults = @{}
        HasErrors = $false
        TotalErrors = 0
        TotalWarnings = 0
    } | ConvertTo-Json -Compress
    
    Write-Host $emptyResult
    
    # Set GitHub Actions outputs
    if ($env:GITHUB_OUTPUT) {
        Add-Content -Path $env:GITHUB_OUTPUT -Value "results=$emptyResult"
        Add-Content -Path $env:GITHUB_OUTPUT -Value "has-errors=false"
        Add-Content -Path $env:GITHUB_OUTPUT -Value "error-count=0"
    }
    
    exit 0
}

Write-Host "Files to check:"
Write-Host $filesToCheck

# Convert space-separated files to array and filter existing files
$filesArray = @()
$filesToCheck.Split(' ', [System.StringSplitOptions]::RemoveEmptyEntries) | ForEach-Object {
    $file = $_.Trim()
    if ($file -and (Test-Path $file)) {
        $filesArray += $file
    }
}

# Run the compliance checker
if ($filesArray.Count -eq 0) {
    Write-Host "No valid files found to check."
    $emptyResult = @{
        FileResults = @{}
        HasErrors = $false
        TotalErrors = 0
        TotalWarnings = 0
    } | ConvertTo-Json -Compress
    
    Write-Host $emptyResult
    
    # Set GitHub Actions outputs
    if ($env:GITHUB_OUTPUT) {
        Add-Content -Path $env:GITHUB_OUTPUT -Value "results=$emptyResult"
        Add-Content -Path $env:GITHUB_OUTPUT -Value "has-errors=false"
        Add-Content -Path $env:GITHUB_OUTPUT -Value "error-count=0"
    }
} else {
    Write-Host "Running compliance checks on $($filesArray.Count) files..."
    
    # Build the command arguments
    $cmdArgs = @(
        "--check-type", $env:CHECK_TYPE,
        "--files"
    )
    $cmdArgs += $filesArray
    
    # Add assembly description if provided for project checks
    if ($env:CHECK_TYPE -eq "project" -and $env:ASSEMBLY_DESCRIPTION) {
        $cmdArgs += @("--assembly-description", $env:ASSEMBLY_DESCRIPTION)
    }
    
    # Run the compliance runner
    try {
        & "ComplianceRunner.exe" @cmdArgs
    } catch {
        Write-Host "Error running compliance checks: $_"
        exit 1
    }
}

Write-Host "Compliance check completed."
