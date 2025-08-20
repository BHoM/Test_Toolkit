# BHoM Compliance GitHub Actions

This directory contains custom GitHub Actions for running BHoM compliance checks on your repositories. These actions are designed to be reusable across different BHoM projects and external organizations.

## Available Actions

### 1. Code Compliance (`code-compliance`)
Runs BHoM code compliance checks on C# files using the `RunChecks` method with `checktype="code"`.

### 2. Documentation Compliance (`documentation-compliance`)
Runs BHoM documentation compliance checks on C# files using the `RunChecks` method with `checktype="documentation"`.

### 3. Project Compliance (`project-compliance`)
Runs BHoM project compliance checks on `.csproj` files using the `CheckProjectFile` method.

## Usage

### Basic Usage

Add any of these actions to your workflow:

```yaml
- name: Code Compliance Check
  uses: BHoM/Test_Toolkit/.github/actions/code-compliance@main
  with:
    github-token: ${{ secrets.GITHUB_TOKEN }}
    fail-on-error: 'true'
```

### Complete Example

```yaml
name: 'BHoM Compliance'
on:
  pull_request:
    branches: [ main ]
    paths: ['**.cs', '**.csproj']

jobs:
  compliance:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
        with:
          fetch-depth: 0  # Required for detecting changed files
      
      - name: Code Compliance
        uses: BHoM/Test_Toolkit/.github/actions/code-compliance@main
        with:
          fail-on-error: 'true'
      
      - name: Documentation Compliance  
        uses: BHoM/Test_Toolkit/.github/actions/documentation-compliance@main
        with:
          fail-on-error: 'false'  # Warning only
      
      - name: Project Compliance
        uses: BHoM/Test_Toolkit/.github/actions/project-compliance@main
        with:
          fail-on-error: 'true'
          assembly-description: 'https://github.com/YourOrganization'
```

## Input Parameters

### Common Parameters (all actions)

| Parameter | Description | Required | Default |
|-----------|-------------|----------|---------|
| `files` | Space-separated list of files to check | No | Auto-detect changed files |
| `github-token` | GitHub token for repository access | No | `${{ github.token }}` |
| `fail-on-error` | Whether to fail the action on compliance errors | No | `'true'` |

### Project Compliance Specific

| Parameter | Description | Required | Default |
|-----------|-------------|----------|---------|
| `assembly-description` | Expected assembly description URL | No | `''` |

## Output Parameters

All actions provide the following outputs:

| Output | Description |
|--------|-------------|
| `results` | JSON string containing detailed compliance results |
| `has-errors` | Boolean indicating if any errors were found |
| `error-count` | Number of compliance errors found |

### Using Outputs

```yaml
- name: Code Compliance Check
  id: compliance
  uses: BHoM/Test_Toolkit/.github/actions/code-compliance@main

- name: Handle Results
  if: always()
  run: |
    echo "Has errors: ${{ steps.compliance.outputs.has-errors }}"
    echo "Error count: ${{ steps.compliance.outputs.error-count }}"
    echo "Results: ${{ steps.compliance.outputs.results }}"
```

## File Detection

When no specific files are provided via the `files` parameter, the actions automatically detect changed files:

- **Pull Requests**: Compares files changed between base branch and head
- **Push Events**: Compares files changed in the last commit

### File Patterns
- **Code/Documentation Compliance**: Checks `*.cs` files (excludes `AssemblyInfo.cs`)
- **Project Compliance**: Checks `*.csproj` files

## Advanced Usage

### Check Specific Files

```yaml
- name: Check Specific Files
  uses: BHoM/Test_Toolkit/.github/actions/code-compliance@main
  with:
    files: 'src/MyClass.cs src/AnotherClass.cs'
```

### Matrix Strategy

```yaml
jobs:
  compliance:
    strategy:
      matrix:
        check-type: ['code-compliance', 'documentation-compliance', 'project-compliance']
    steps:
      - uses: actions/checkout@v4
        with:
          fetch-depth: 0
      - uses: BHoM/Test_Toolkit/.github/actions/${{ matrix.check-type }}@main
```

### Conditional Execution

```yaml
- name: Code Compliance (PRs only)
  if: github.event_name == 'pull_request'
  uses: BHoM/Test_Toolkit/.github/actions/code-compliance@main
```

## Architecture

These actions use a pre-compiled .NET 8 console application running in a Docker container for:
- **Performance**: Pre-compiled binaries run faster than building from source
- **Consistency**: Same compliance rules across all repositories
- **Maintenance**: Centralized updates and bug fixes

## Versioning

- `@main` - Latest version (recommended for most users)
- `@v1` - Major version (when available)
- `@v1.2.3` - Specific version (for production stability)

## Contributing

To modify these actions:
1. Make changes to the source code in this repository
2. Test with the example workflows
3. Create a pull request with your changes
4. After merging, the actions will be available at `@main`

## Troubleshooting

### Common Issues

1. **"No files to check"**: Ensure your workflow triggers on the correct file paths and that you have `fetch-depth: 0` in your checkout action.

2. **Permission errors**: Make sure the `github-token` has appropriate permissions to access the repository.

3. **Docker build failures**: Check that all required files are present and the Dockerfile syntax is correct.

### Getting Help

- Check the [example workflows](../workflows/examples/) for working configurations
- Review action logs for detailed error messages
- Open an issue in this repository for bugs or feature requests

## License

These actions are part of the BHoM Test Toolkit and are subject to the same license terms.
