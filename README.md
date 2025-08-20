# BHoM Test Toolkit

The BHoM Test Toolkit provides comprehensive testing and compliance checking tools for BHoM projects. This toolkit includes engines for code compliance, documentation compliance, project structure validation, unit testing, and interoperability testing.

## 🚀 GitHub Actions (New!)

This repository now provides reusable GitHub Actions for automated compliance checking that can be used across BHoM projects and external repositories.

### Available Actions

- **[Code Compliance](.github/actions/code-compliance)**: Validates C# code against BHoM coding standards
- **[Documentation Compliance](.github/actions/documentation-compliance)**: Ensures proper documentation coverage
- **[Project Compliance](.github/actions/project-compliance)**: Validates `.csproj` file configurations

### Quick Start

Add to your workflow:

```yaml
name: 'BHoM Compliance Check'
on: [pull_request]

jobs:
  compliance:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
        with:
          fetch-depth: 0
      
      - name: Code Compliance
        uses: BHoM/Test_Toolkit/.github/actions/code-compliance@main
      
      - name: Documentation Compliance  
        uses: BHoM/Test_Toolkit/.github/actions/documentation-compliance@main
        with:
          fail-on-error: 'false'  # Warning only
      
      - name: Project Compliance
        uses: BHoM/Test_Toolkit/.github/actions/project-compliance@main
        with:
          assembly-description: 'https://github.com/YourOrganization'
```

📖 **[Full Documentation](.github/actions/README.md)** | 💡 **[Example Workflows](.github/workflows/examples/)**

---

## Components

### Code Compliance Engine
Validates C# code files against BHoM coding standards and conventions.

### Documentation Compliance Engine  
Ensures proper documentation coverage and formatting.

### Project Compliance Engine
Validates project file configurations, references, and build settings.

### Unit Test Engine
Provides infrastructure for running and managing unit tests.

### Interoperability Test Engine
Tests data exchange and compatibility between different BHoM adapters.

### NUnit Integration
Supports NUnit-based testing workflows.

## Local Usage

The toolkit can also be used locally for development and testing:

```bash
# Build the solution
msbuild Test_Toolkit.sln /p:Configuration=Release

# Run compliance checks (using the built assemblies)
# See individual engine documentation for specific usage
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test with the provided workflows
5. Submit a pull request

## License

This project is part of the BHoM ecosystem and follows the same licensing terms.
