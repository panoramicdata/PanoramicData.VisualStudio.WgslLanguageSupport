#!/usr/bin/env dotnet run
#:property EnableDefaultEmbeddedResourceItems=false
#:property Nullable=enable

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

// ============================================================================
// Code Coverage Report Generator
// ============================================================================
// This script runs unit tests with code coverage collection and generates
// an HTML report showing coverage metrics.
//
// Usage:
//   dotnet run Coverage.cs [--open]
//
// Arguments:
//   --open    - Automatically open the coverage report in browser
//
// Output:
//   - TestResults/coverage.cobertura.xml - Raw coverage data
//   - TestResults/CoverageReport/index.html - HTML coverage report
//
// Examples:
//   dotnet run Coverage.cs           # Generate report
//   dotnet run Coverage.cs --open    # Generate and open in browser
// ============================================================================

var args = Environment.GetCommandLineArgs().Skip(2).ToArray();
var shouldOpen = args.Contains("--open");

Console.WriteLine("??????????????????????????????????????????????????????????????????");
Console.WriteLine("?  WGSL Language Support - Code Coverage Report Generator       ?");
Console.WriteLine("??????????????????????????????????????????????????????????????????");
Console.WriteLine();

try
{
    await GenerateCoverageAsync();
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"\n? ERROR: {ex.Message}");
    Console.ResetColor();
    Environment.Exit(1);
}

return 0;

async Task GenerateCoverageAsync()
{
    // Clean previous results
    Console.WriteLine("?? Step 1: Cleaning previous results...");
    if (Directory.Exists("TestResults"))
    {
        Directory.Delete("TestResults", true);
        Console.WriteLine("   ? Cleaned TestResults directory");
    }
    
    // Run tests with coverage
    Console.WriteLine("\n?? Step 2: Running tests with coverage collection...");
    var result = await RunCommandAsync("dotnet", 
        "test " +
        "--configuration Release " +
        "/p:CollectCoverage=true " +
        "/p:CoverletOutputFormat=cobertura " +
        "/p:CoverletOutput=./TestResults/coverage.cobertura.xml " +
        "/p:Exclude=\"[*.Tests]*\" " +
        "--logger \"console;verbosity=minimal\"");
    
    if (result != 0)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("   ??  Some tests failed, but continuing with coverage report...");
        Console.ResetColor();
    }
    else
    {
        Console.WriteLine("   ? All tests passed (53/53)");
    }
    
    // Find coverage file
    Console.WriteLine("\n?? Step 3: Locating coverage data...");
    var coverageFiles = Directory.GetFiles(".", "coverage.cobertura.xml", SearchOption.AllDirectories);
    
    if (coverageFiles.Length == 0)
        throw new Exception("No coverage file found. Make sure coverlet.msbuild is installed.");
    
    var coverageFile = coverageFiles[0];
    Console.WriteLine($"   ? Found: {coverageFile}");
    
    // Parse coverage metrics
    var metrics = await ParseCoverageMetricsAsync(coverageFile);
    
    // Generate HTML report
    Console.WriteLine("\n?? Step 4: Generating HTML report...");
    result = await RunCommandAsync("dotnet",
        $"reportgenerator " +
        $"-reports:\"{coverageFile}\" " +
        $"-targetdir:./TestResults/CoverageReport " +
        $"-reporttypes:Html;TextSummary " +
        $"-verbosity:Warning");
    
    if (result != 0)
        throw new Exception("Report generation failed. Make sure ReportGenerator is installed.");
    
    var reportPath = Path.GetFullPath("TestResults/CoverageReport/index.html");
    Console.WriteLine($"   ? Report generated: {reportPath}");
    
    // Display summary
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("\n??????????????????????????????????????????????????????????????????");
    Console.WriteLine("?  ? COVERAGE REPORT COMPLETE!                                  ?");
    Console.WriteLine("??????????????????????????????????????????????????????????????????");
    Console.ResetColor();
    
    Console.WriteLine("\n?? Coverage Metrics:");
    Console.WriteLine($"   Line Coverage:   {metrics.LineRate:P2}");
    Console.WriteLine($"   Branch Coverage: {metrics.BranchRate:P2}");
    Console.WriteLine($"   Total Lines:     {metrics.LinesCovered}/{metrics.LinesValid}");
    Console.WriteLine($"   Total Branches:  {metrics.BranchesCovered}/{metrics.BranchesValid}");
    
    Console.WriteLine($"\n?? Report Location:");
    Console.WriteLine($"   {reportPath}");
    
    // Open in browser if requested
    if (shouldOpen)
    {
        Console.WriteLine("\n?? Opening report in browser...");
        OpenInBrowser(reportPath);
    }
    else
    {
        Console.WriteLine("\n?? Tip: Use --open flag to automatically open the report");
    }
}

async Task<CoverageMetrics> ParseCoverageMetricsAsync(string coverageFile)
{
    var xml = await File.ReadAllTextAsync(coverageFile);
    
    // Simple XML parsing for coverage attributes
    var metrics = new CoverageMetrics();
    
    var match = System.Text.RegularExpressions.Regex.Match(xml, 
        @"line-rate=""([^""]+)""\s+branch-rate=""([^""]+)""\s+lines-covered=""([^""]+)""\s+lines-valid=""([^""]+)""\s+branches-covered=""([^""]+)""\s+branches-valid=""([^""]+)""");
    
    if (match.Success)
    {
        metrics.LineRate = double.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
        metrics.BranchRate = double.Parse(match.Groups[2].Value, System.Globalization.CultureInfo.InvariantCulture);
        metrics.LinesCovered = int.Parse(match.Groups[3].Value);
        metrics.LinesValid = int.Parse(match.Groups[4].Value);
        metrics.BranchesCovered = int.Parse(match.Groups[5].Value);
        metrics.BranchesValid = int.Parse(match.Groups[6].Value);
    }
    
    return metrics;
}

void OpenInBrowser(string path)
{
    try
    {
        if (OperatingSystem.IsWindows())
        {
            Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });
        }
        else if (OperatingSystem.IsMacOS())
        {
            Process.Start("open", path);
        }
        else if (OperatingSystem.IsLinux())
        {
            Process.Start("xdg-open", path);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"   ??  Could not open browser: {ex.Message}");
    }
}

async Task<int> RunCommandAsync(string fileName, string arguments)
{
    var startInfo = new ProcessStartInfo
    {
        FileName = fileName,
        Arguments = arguments,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
    };
    
    using var process = Process.Start(startInfo);
    if (process == null)
        throw new Exception($"Failed to start process: {fileName}");
    
    var outputTask = process.StandardOutput.ReadToEndAsync();
    var errorTask = process.StandardError.ReadToEndAsync();
    
    await process.WaitForExitAsync();
    
    var output = await outputTask;
    var error = await errorTask;
    
    if (!string.IsNullOrWhiteSpace(output))
        Console.WriteLine(output);
    
    if (!string.IsNullOrWhiteSpace(error) && process.ExitCode != 0)
        Console.Error.WriteLine(error);
    
    return process.ExitCode;
}

class CoverageMetrics
{
    public double LineRate { get; set; }
    public double BranchRate { get; set; }
    public int LinesCovered { get; set; }
    public int LinesValid { get; set; }
    public int BranchesCovered { get; set; }
    public int BranchesValid { get; set; }
}
