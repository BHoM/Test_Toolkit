using System.CommandLine;
using Newtonsoft.Json;
using BH.Engine.Test.CodeCompliance;
using BH.oM.Test;
using BH.oM.Test.Results;

namespace ComplianceRunner;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("BHoM Compliance Checker");

        var checkTypeOption = new Option<string>(
            name: "--check-type",
            description: "Type of check to perform: code, documentation, or project")
        {
            IsRequired = true
        };

        var filesOption = new Option<string[]>(
            name: "--files",
            description: "Files to check")
        {
            IsRequired = true,
            AllowMultipleArgumentsPerToken = true
        };

        var assemblyDescriptionOption = new Option<string>(
            name: "--assembly-description",
            description: "Expected assembly description for project checks");

        rootCommand.AddOption(checkTypeOption);
        rootCommand.AddOption(filesOption);
        rootCommand.AddOption(assemblyDescriptionOption);

        rootCommand.SetHandler(async (checkType, files, assemblyDescription) =>
        {
            await RunComplianceChecks(checkType, files, assemblyDescription);
        }, checkTypeOption, filesOption, assemblyDescriptionOption);

        return await rootCommand.InvokeAsync(args);
    }

    private static async Task RunComplianceChecks(string checkType, string[] files, string? assemblyDescription)
    {
        var results = new ComplianceResults();
        
        try
        {
            foreach (var file in files)
            {
                if (!File.Exists(file))
                {
                    Console.WriteLine($"Warning: File not found: {file}");
                    continue;
                }

                var result = checkType.ToLower() switch
                {
                    "code" => await CheckCodeCompliance(file),
                    "documentation" => await CheckDocumentationCompliance(file),
                    "project" => await CheckProjectCompliance(file, assemblyDescription),
                    _ => throw new ArgumentException($"Unknown check type: {checkType}")
                };

                results.FileResults.Add(file, result);
            }

            // Output results as JSON for GitHub Actions
            var json = JsonConvert.SerializeObject(results, Formatting.Indented);
            Console.WriteLine(json);

            // Set GitHub Actions outputs
            await SetGitHubOutput("results", json);
            await SetGitHubOutput("has-errors", results.HasErrors.ToString().ToLower());
            await SetGitHubOutput("error-count", results.TotalErrors.ToString());

            // Exit with error code if there are compliance issues and fail-on-error is true
            var failOnError = Environment.GetEnvironmentVariable("FAIL_ON_ERROR")?.ToLower() == "true";
            if (results.HasErrors && failOnError)
            {
                Environment.Exit(1);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            Environment.Exit(1);
        }
    }

    private static async Task<FileComplianceResult> CheckCodeCompliance(string filePath)
    {
        var result = new FileComplianceResult();
        
        try
        {
            // Use the existing BH.Engine.Test.CodeCompliance.Compute.RunChecks method
            var testResult = Compute.RunChecks(filePath, "code");
            
            result.Status = ConvertTestStatus(testResult.Status);
            
            // Convert BHoM test results to our format
            if (testResult.Information != null)
            {
                foreach (var info in testResult.Information)
                {
                    if (info is BH.oM.Test.Results.Error error)
                    {
                        var lineNumber = error.Location?.LineSpan?.Start?.Line ?? 0;
                        var message = $"Line {lineNumber}: {error.Message}";
                        
                        if (error.Status == TestStatus.Error)
                        {
                            result.Errors.Add(message);
                        }
                        else if (error.Status == TestStatus.Warning)
                        {
                            result.Warnings.Add(message);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Error processing file: {ex.Message}");
            result.Status = "Error";
        }

        return result;
    }

    private static async Task<FileComplianceResult> CheckDocumentationCompliance(string filePath)
    {
        var result = new FileComplianceResult();
        
        try
        {
            // Use the existing BH.Engine.Test.CodeCompliance.Compute.RunChecks method
            var testResult = Compute.RunChecks(filePath, "documentation");
            
            result.Status = ConvertTestStatus(testResult.Status);
            
            // Convert BHoM test results to our format
            if (testResult.Information != null)
            {
                foreach (var info in testResult.Information)
                {
                    if (info is BH.oM.Test.Results.Error error)
                    {
                        var lineNumber = error.Location?.LineSpan?.Start?.Line ?? 0;
                        var message = $"Line {lineNumber}: {error.Message}";
                        
                        if (error.Status == TestStatus.Error)
                        {
                            result.Errors.Add(message);
                        }
                        else if (error.Status == TestStatus.Warning)
                        {
                            result.Warnings.Add(message);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Error processing file: {ex.Message}");
            result.Status = "Error";
        }

        return result;
    }

    private static async Task<FileComplianceResult> CheckProjectCompliance(string filePath, string? assemblyDescription)
    {
        var result = new FileComplianceResult();
        
        try
        {
            // Use the existing BH.Engine.Test.CodeCompliance.Compute.CheckProjectFile method
            var testResult = Compute.CheckProjectFile(filePath, assemblyDescription);
            
            result.Status = ConvertTestStatus(testResult.Status);
            
            // Convert BHoM test results to our format
            if (testResult.Information != null)
            {
                foreach (var info in testResult.Information)
                {
                    if (info is BH.oM.Test.Results.Error error)
                    {
                        var lineNumber = error.Location?.LineSpan?.Start?.Line ?? 0;
                        var message = $"Line {lineNumber}: {error.Message}";
                        
                        if (error.Status == TestStatus.Error)
                        {
                            result.Errors.Add(message);
                        }
                        else if (error.Status == TestStatus.Warning)
                        {
                            result.Warnings.Add(message);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Error processing file: {ex.Message}");
            result.Status = "Error";
        }

        return result;
    }

    private static string ConvertTestStatus(TestStatus status)
    {
        return status switch
        {
            TestStatus.Pass => "Pass",
            TestStatus.Warning => "Warning",
            TestStatus.Error => "Error",
            _ => "Unknown"
        };
    }

    private static async Task SetGitHubOutput(string name, string value)
    {
        var githubOutput = Environment.GetEnvironmentVariable("GITHUB_OUTPUT");
        if (!string.IsNullOrEmpty(githubOutput))
        {
            await File.AppendAllTextAsync(githubOutput, $"{name}={value}\n");
        }
    }
}

public class ComplianceResults
{
    public Dictionary<string, FileComplianceResult> FileResults { get; set; } = new();
    
    public bool HasErrors => FileResults.Values.Any(r => r.Errors.Count > 0);
    
    public int TotalErrors => FileResults.Values.Sum(r => r.Errors.Count);
    
    public int TotalWarnings => FileResults.Values.Sum(r => r.Warnings.Count);
}

public class FileComplianceResult
{
    public string Status { get; set; } = "Unknown";
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}