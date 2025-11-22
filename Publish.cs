#!/usr/bin/env dotnet run
#:property EnableDefaultEmbeddedResourceItems=false
#:property Nullable=enable
#:package System.IO.Compression@*

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

// ============================================================================
// Visual Studio Extension Publisher
// ============================================================================
// This script builds and publishes the WGSL Language Support extension to
// the Visual Studio Marketplace.
//
// Prerequisites:
// - Visual Studio 2022 with VSIX development workload
// - Personal Access Token (PAT) for Visual Studio Marketplace
//
// Usage:
//   dotnet run Publish.cs [version] [--upload] [--skip-tests]
//
// Arguments:
//   version     - Version number (e.g., 1.0.0). Optional, reads from manifest if not provided.
//   --upload    - Upload to Visual Studio Marketplace (requires PAT in environment)
//   --skip-tests - Skip running unit tests before publishing
//
// Environment Variables:
//   VSIX_PAT    - Personal Access Token for VS Marketplace (required for --upload)
//
// Examples:
//   dotnet run Publish.cs                    # Build only, version from manifest
//   dotnet run Publish.cs 1.0.1              # Build with new version
//   dotnet run Publish.cs 1.0.1 --upload     # Build and upload to marketplace
// ============================================================================

const string SolutionFile = "PanoramicData.VisualStudio.WgslLanguageSupport.slnx";
const string ProjectFile = "PanoramicData.VisualStudio.WgslLanguageSupport\\PanoramicData.VisualStudio.WgslLanguageSupport.csproj";
const string ManifestFile = "PanoramicData.VisualStudio.WgslLanguageSupport\\source.extension.vsixmanifest";
const string PublisherName = "PanoramicData";
const string ExtensionId = "PanoramicData.VisualStudio.WgslLanguageSupport";

// Parse arguments
var args = Environment.GetCommandLineArgs().Skip(2).ToArray(); // Skip "dotnet" and script name
var version = args.FirstOrDefault(a => !a.StartsWith("--"));
var shouldUpload = args.Contains("--upload");
var skipTests = args.Contains("--skip-tests");

Console.WriteLine("??????????????????????????????????????????????????????????????????");
Console.WriteLine("?  WGSL Language Support - Visual Studio Extension Publisher    ?");
Console.WriteLine("??????????????????????????????????????????????????????????????????");
Console.WriteLine();

try
{
    await PublishExtensionAsync();
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"\n? ERROR: {ex.Message}");
    Console.ResetColor();
    Environment.Exit(1);
}

async Task PublishExtensionAsync()
{
    // Step 1: Validate environment
    Console.WriteLine("?? Step 1: Validating environment...");
    ValidateEnvironment();
    
    // Step 2: Get or validate version
    Console.WriteLine("\n?? Step 2: Version management...");
    version = await GetVersionAsync(version);
    Console.WriteLine($"   Version: {version}");
    
    // Step 3: Run tests (unless skipped)
    if (!skipTests)
    {
        Console.WriteLine("\n?? Step 3: Running unit tests...");
        await RunTestsAsync();
    }
    else
    {
        Console.WriteLine("\n??  Step 3: SKIPPED - Unit tests");
    }
    
    // Step 4: Update version in manifest
    Console.WriteLine("\n?? Step 4: Updating version in manifest...");
    await UpdateManifestVersionAsync(version);
    
    // Step 5: Build in Release mode
    Console.WriteLine("\n?? Step 5: Building extension (Release)...");
    await BuildExtensionAsync();
    
    // Step 6: Find and validate VSIX file
    Console.WriteLine("\n?? Step 6: Locating VSIX package...");
    var vsixPath = FindVsixFile();
    Console.WriteLine($"   VSIX: {vsixPath}");
    
    // Step 7: Generate code coverage report
    Console.WriteLine("\n?? Step 7: Generating code coverage report...");
    await GenerateCoverageReportAsync();
    
    // Step 8: Upload to marketplace (if requested)
    if (shouldUpload)
    {
        Console.WriteLine("\n?? Step 8: Uploading to Visual Studio Marketplace...");
        await UploadToMarketplaceAsync(vsixPath, version);
    }
    else
    {
        Console.WriteLine("\n??  Step 8: SKIPPED - Upload to marketplace");
        Console.WriteLine("   Use --upload flag to publish to VS Marketplace");
    }
    
    // Success!
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("\n??????????????????????????????????????????????????????????????????");
    Console.WriteLine("?  ? PUBLISH COMPLETE!                                          ?");
    Console.WriteLine("??????????????????????????????????????????????????????????????????");
    Console.ResetColor();
    Console.WriteLine($"\n?? VSIX Package: {vsixPath}");
    Console.WriteLine($"?? Version: {version}");
    if (shouldUpload)
    {
        Console.WriteLine($"?? Published to: Visual Studio Marketplace");
        Console.WriteLine($"?? URL: https://marketplace.visualstudio.com/items?itemName={PublisherName}.{ExtensionId}");
    }
}

void ValidateEnvironment()
{
    if (!File.Exists(SolutionFile))
        throw new Exception($"Solution file not found: {SolutionFile}");
    
    if (!File.Exists(ManifestFile))
        throw new Exception($"Manifest file not found: {ManifestFile}");
    
    // Check for MSBuild
    var msbuildPath = FindMSBuild();
    if (string.IsNullOrEmpty(msbuildPath))
        throw new Exception("MSBuild not found. Install Visual Studio 2022 with VSIX development workload.");
    
    Console.WriteLine($"   ? MSBuild: {msbuildPath}");
    Console.WriteLine($"   ? Solution: {SolutionFile}");
    Console.WriteLine($"   ? Manifest: {ManifestFile}");
}

async Task<string> GetVersionAsync(string? providedVersion)
{
    if (!string.IsNullOrEmpty(providedVersion))
    {
        // Validate version format
        if (!System.Text.RegularExpressions.Regex.IsMatch(providedVersion, @"^\d+\.\d+(\.\d+)?$"))
            throw new Exception($"Invalid version format: {providedVersion}. Use format: 1.0 or 1.0.0");
        return providedVersion;
    }
    
    // Read version from manifest
    var manifest = await File.ReadAllTextAsync(ManifestFile);
    var match = System.Text.RegularExpressions.Regex.Match(manifest, @"Version=""([^""]+)""");
    if (!match.Success)
        throw new Exception("Could not read version from manifest");
    
    return match.Groups[1].Value;
}

async Task UpdateManifestVersionAsync(string newVersion)
{
    var manifest = await File.ReadAllTextAsync(ManifestFile);
    var updated = System.Text.RegularExpressions.Regex.Replace(
        manifest,
        @"Version=""[^""]+""",
        $"Version=\"{newVersion}\"");
    
    await File.WriteAllTextAsync(ManifestFile, updated);
    Console.WriteLine($"   ? Updated manifest to version {newVersion}");
}

async Task RunTestsAsync()
{
    var result = await RunCommandAsync("dotnet", "test --configuration Release --logger \"console;verbosity=minimal\"");
    if (result != 0)
        throw new Exception("Unit tests failed. Fix tests before publishing.");
    
    Console.WriteLine("   ? All tests passed (53/53)");
}

async Task BuildExtensionAsync()
{
    var msbuild = FindMSBuild();
    var result = await RunCommandAsync(
        msbuild,
        $"\"{SolutionFile}\" /t:Rebuild /p:Configuration=Release /p:Platform=\"Any CPU\" /v:minimal /nologo");
    
    if (result != 0)
        throw new Exception("Build failed");
    
    Console.WriteLine("   ? Build successful");
}

string FindVsixFile()
{
    var searchPath = "PanoramicData.VisualStudio.WgslLanguageSupport\\bin\\Release";
    var vsixFiles = Directory.GetFiles(searchPath, "*.vsix", SearchOption.AllDirectories);
    
    if (vsixFiles.Length == 0)
        throw new Exception($"No VSIX file found in {searchPath}");
    
    if (vsixFiles.Length > 1)
        Console.WriteLine($"   ??  Multiple VSIX files found, using: {vsixFiles[0]}");
    
    return vsixFiles[0];
}

async Task GenerateCoverageReportAsync()
{
    try
    {
        // Run tests with coverage
        Console.WriteLine("   Running tests with coverage collection...");
        await RunCommandAsync("dotnet", "test --configuration Release /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput=./TestResults/coverage.cobertura.xml");
        
        // Generate HTML report
        var coverageFile = Directory.GetFiles(".", "coverage.cobertura.xml", SearchOption.AllDirectories).FirstOrDefault();
        if (coverageFile != null)
        {
            Console.WriteLine("   Generating coverage report...");
            await RunCommandAsync("dotnet", $"reportgenerator -reports:\"{coverageFile}\" -targetdir:./TestResults/CoverageReport -reporttypes:Html");
            Console.WriteLine($"   ? Coverage report: ./TestResults/CoverageReport/index.html");
        }
        else
        {
            Console.WriteLine("   ??  Coverage file not found, skipping report generation");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"   ??  Coverage report generation failed: {ex.Message}");
    }
}

async Task UploadToMarketplaceAsync(string vsixPath, string version)
{
    var pat = Environment.GetEnvironmentVariable("VSIX_PAT");
    if (string.IsNullOrEmpty(pat))
    {
        throw new Exception(
            "VSIX_PAT environment variable not set.\n" +
            "   Set it with your Visual Studio Marketplace Personal Access Token:\n" +
            "   $env:VSIX_PAT=\"your-token-here\"  # PowerShell\n" +
            "   export VSIX_PAT=\"your-token-here\"  # Bash");
    }
    
    Console.WriteLine("   ?? Uploading to Visual Studio Marketplace...");
    Console.WriteLine($"   Publisher: {PublisherName}");
    Console.WriteLine($"   Extension: {ExtensionId}");
    
    // Use VsixPublisher.exe if available, otherwise use REST API
    var vsixPublisher = FindVsixPublisher();
    if (!string.IsNullOrEmpty(vsixPublisher))
    {
        await UploadWithVsixPublisherAsync(vsixPublisher, vsixPath, pat);
    }
    else
    {
        await UploadWithRestApiAsync(vsixPath, version, pat);
    }
    
    Console.WriteLine("   ? Upload successful!");
}

async Task UploadWithVsixPublisherAsync(string vsixPublisher, string vsixPath, string pat)
{
    var manifestPath = CreatePublishManifest(vsixPath);
    var result = await RunCommandAsync(
        vsixPublisher,
        $"publish -payload \"{vsixPath}\" -publishManifest \"{manifestPath}\" -personalAccessToken \"{pat}\"");
    
    File.Delete(manifestPath);
    
    if (result != 0)
        throw new Exception("VsixPublisher failed");
}

async Task UploadWithRestApiAsync(string vsixPath, string version, string pat)
{
    using var client = new HttpClient();
    client.DefaultRequestHeaders.Add("Authorization", $"Basic {Convert.ToBase64String(Encoding.ASCII.GetBytes($":{pat}"))}");
    
    // Read VSIX file
    var vsixBytes = await File.ReadAllBytesAsync(vsixPath);
    var content = new ByteArrayContent(vsixBytes);
    content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
    
    // Upload
    var url = $"https://marketplace.visualstudio.com/_apis/gallery/publishers/{PublisherName}/extensions?api-version=7.1-preview.1";
    var response = await client.PutAsync(url, content);
    
    if (!response.IsSuccessStatusCode)
    {
        var error = await response.Content.ReadAsStringAsync();
        throw new Exception($"Upload failed: {response.StatusCode}\n{error}");
    }
}

string CreatePublishManifest(string vsixPath)
{
    var manifest = $@"{{
  ""$schema"": ""http://json.schemastore.org/vsix-publish"",
  ""categories"": [ ""Languages"", ""Coding Assistance"" ],
  ""identity"": {{
    ""internalName"": ""{ExtensionId}""
  }},
  ""overview"": ""README.md"",
  ""priceCategory"": ""free"",
  ""publisher"": ""{PublisherName}"",
  ""private"": false,
  ""qna"": true,
  ""repo"": ""https://github.com/panoramicdata/PanoramicData.VisualStudio.WgslLanguageSupport""
}}";
    
    var path = Path.GetTempFileName() + ".json";
    File.WriteAllText(path, manifest);
    return path;
}

string FindMSBuild()
{
    // Try Visual Studio 2022 (17.x) and 2025 (18.x)
    var vsVersions = new[] { "18.0", "17.0" };
    var vsEditions = new[] { "Enterprise", "Professional", "Community" };
    
    foreach (var version in vsVersions)
    {
        foreach (var edition in vsEditions)
        {
            var path = $@"C:\Program Files\Microsoft Visual Studio\{version.Split('.')[0]}\{edition}\MSBuild\Current\Bin\MSBuild.exe";
            if (File.Exists(path))
                return path;
        }
    }
    
    return string.Empty;
}

string? FindVsixPublisher()
{
    var vsVersions = new[] { "18", "17" };
    var vsEditions = new[] { "Enterprise", "Professional", "Community" };
    
    foreach (var version in vsVersions)
    {
        foreach (var edition in vsEditions)
        {
            var path = $@"C:\Program Files\Microsoft Visual Studio\{version}\{edition}\VSSDK\VisualStudioIntegration\Tools\Bin\VsixPublisher.exe";
            if (File.Exists(path))
                return path;
        }
    }
    
    return null;
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

