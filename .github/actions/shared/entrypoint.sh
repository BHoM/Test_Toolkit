#!/bin/bash
set -e

echo "Starting BHoM Compliance Check..."
echo "Check Type: $CHECK_TYPE"
echo "Input Files: $INPUT_FILES"

# Function to get changed files if no specific files provided
get_changed_files() {
    local file_pattern="$1"
    
    if [ -n "$INPUT_FILES" ]; then
        echo "$INPUT_FILES"
        return
    fi
    
    # Get changed files from git
    if [ "$GITHUB_EVENT_NAME" = "pull_request" ]; then
        # For pull requests, get files changed between base and head
        git fetch origin "$GITHUB_BASE_REF" --depth=1
        changed_files=$(git diff --name-only "origin/$GITHUB_BASE_REF"...HEAD | grep "$file_pattern" || true)
    else
        # For push events, get files changed in the last commit
        changed_files=$(git diff --name-only HEAD~1 HEAD | grep "$file_pattern" || true)
    fi
    
    echo "$changed_files"
}

# Determine file pattern and get files to check
if [ "$CHECK_TYPE" = "project" ]; then
    file_pattern="\.csproj$"
else
    file_pattern="\.cs$"
fi

files_to_check=$(get_changed_files "$file_pattern")

if [ -z "$files_to_check" ]; then
    echo "No files to check for $CHECK_TYPE compliance."
    echo "{\"FileResults\": {}, \"HasErrors\": false, \"TotalErrors\": 0, \"TotalWarnings\": 0}" > /tmp/results.json
    
    # Set GitHub Actions outputs
    if [ -n "$GITHUB_OUTPUT" ]; then
        echo "results={\"FileResults\": {}, \"HasErrors\": false, \"TotalErrors\": 0, \"TotalWarnings\": 0}" >> "$GITHUB_OUTPUT"
        echo "has-errors=false" >> "$GITHUB_OUTPUT"
        echo "error-count=0" >> "$GITHUB_OUTPUT"
    fi
    
    exit 0
fi

echo "Files to check:"
echo "$files_to_check"

# Convert space-separated files to array
files_array=()
while IFS= read -r file; do
    if [ -n "$file" ] && [ -f "$file" ]; then
        files_array+=("$file")
    fi
done <<< "$files_to_check"

# Run the compliance checker
if [ ${#files_array[@]} -eq 0 ]; then
    echo "No valid files found to check."
    echo "{\"FileResults\": {}, \"HasErrors\": false, \"TotalErrors\": 0, \"TotalWarnings\": 0}"
    
    # Set GitHub Actions outputs
    if [ -n "$GITHUB_OUTPUT" ]; then
        echo "results={\"FileResults\": {}, \"HasErrors\": false, \"TotalErrors\": 0, \"TotalWarnings\": 0}" >> "$GITHUB_OUTPUT"
        echo "has-errors=false" >> "$GITHUB_OUTPUT"
        echo "error-count=0" >> "$GITHUB_OUTPUT"
    fi
else
    echo "Running compliance checks on ${#files_array[@]} files..."
    
    # Build the command arguments
    cmd_args=("--check-type" "$CHECK_TYPE")
    cmd_args+=("--files")
    cmd_args+=("${files_array[@]}")
    
    # Add assembly description if provided for project checks
    if [ "$CHECK_TYPE" = "project" ] && [ -n "$ASSEMBLY_DESCRIPTION" ]; then
        cmd_args+=("--assembly-description" "$ASSEMBLY_DESCRIPTION")
    fi
    
    # Run the compliance runner
    dotnet ComplianceRunner.dll "${cmd_args[@]}"
fi

echo "Compliance check completed."
