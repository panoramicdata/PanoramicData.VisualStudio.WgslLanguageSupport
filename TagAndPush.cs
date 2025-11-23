#!/usr/bin/env dotnet run
#:property Nullable=enable

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

// Parse arguments
var scriptArgs = Environment.GetCommandLineArgs().Skip(2).ToArray();
var isDryRun = scriptArgs.Contains("--dry-run");
var isForce = scriptArgs.Contains("--force");
var allowDirty = scriptArgs.Contains("--allow-dirty");

Console.WriteLine("================================================================");
Console.WriteLine("  Git Tag & Push - Semantic Versioning with NBGV");
Console.WriteLine("================================================================");
Console.WriteLine();

try
{
    await TagAndPushAsync();
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"\n[X] ERROR: {ex.Message}");
    Console.ResetColor();
    Environment.Exit(1);
}

async Task TagAndPushAsync()
{
    Console.WriteLine(">> Step 1: Validating environment...");
    ValidateEnvironment();
    
    Console.WriteLine("\n>> Step 2: Checking git repository status...");
    await CheckGitStatusAsync();
    
    Console.WriteLine("\n>> Step 3: Getting version from NBGV...");
    var version = await GetNbgvVersionAsync();
    Console.WriteLine($"   Version: {version}");
    
    Console.WriteLine("\n>> Step 4: Checking if tag exists...");
    var tagExists = await CheckTagExistsAsync(version);
    
    if (tagExists && !isForce)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"   [!] Tag 'v{version}' already exists!");
        Console.WriteLine("   Use --force to recreate the tag");
        Console.ResetColor();
        return;
    }
    
    Console.WriteLine("\n>> Step 5: Creating git tag...");
    await CreateGitTagAsync(version, isForce, isDryRun);
    
    Console.WriteLine("\n>> Step 6: Pushing tag to remote...");
    await PushTagAsync(version, isForce, isDryRun);
    
    Console.ForegroundColor = isDryRun ? ConsoleColor.Cyan : ConsoleColor.Green;
    Console.WriteLine(isDryRun ? "\n[i] DRY RUN COMPLETE" : "\n[OK] TAG AND PUSH COMPLETE!");
    Console.ResetColor();
    Console.WriteLine($"\nTag: v{version}");
}

void ValidateEnvironment()
{
    if (!IsCommandAvailable("git"))
        throw new Exception("Git not found");
    if (!IsCommandAvailable("nbgv"))
        throw new Exception("nbgv not found. Install: dotnet tool install -g nbgv");
    if (!File.Exists("version.json"))
        throw new Exception("version.json not found");
    Console.WriteLine("   [OK] Environment validated");
}

async Task CheckGitStatusAsync()
{
    var result = await RunCommandAsync("git", "status --porcelain", captureOutput: true);
    if (!string.IsNullOrWhiteSpace(result.Output))
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("   [!] Uncommitted changes detected");
        Console.ResetColor();
        
        if (!allowDirty)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("   [X] Working directory not clean! Use --allow-dirty to override");
            Console.ResetColor();
            throw new Exception("Uncommitted changes detected");
        }
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("   [!] Proceeding (--allow-dirty)");
        Console.ResetColor();
    }
    else
    {
        Console.WriteLine("   [OK] Working directory clean");
    }
}

async Task<string> GetNbgvVersionAsync()
{
    var result = await RunCommandAsync("nbgv", "get-version -f json", captureOutput: true);
    
    // Try to get SemVer2 version first (includes build metadata like 1.0.123)
    var semVer2Match = Regex.Match(result.Output, @"""SemVer2""\s*:\s*""([^""]+)""");
    if (semVer2Match.Success)
    {
        var semVer2 = semVer2Match.Groups[1].Value;
        Console.WriteLine($"   SemVer2: {semVer2}");
        
        // Extract just the version part (remove any +metadata suffix if present)
        var versionMatch = Regex.Match(semVer2, @"^([0-9]+\.[0-9]+\.[0-9]+)");
        if (versionMatch.Success)
            return versionMatch.Groups[1].Value;
    }
    
    // Fallback to Version field
    var versionMatch2 = Regex.Match(result.Output, @"""Version""\s*:\s*""([^""]+)""");
    if (!versionMatch2.Success)
        throw new Exception("Could not parse version from NBGV");
    
    return versionMatch2.Groups[1].Value;
}

async Task<bool> CheckTagExistsAsync(string version)
{
    var result = await RunCommandAsync("git", $"tag -l v{version}", captureOutput: true);
    return !string.IsNullOrWhiteSpace(result.Output);
}

async Task CreateGitTagAsync(string version, bool force, bool dryRun)
{
    var command = $"tag {(force ? "-f " : "")}-a v{version} -m \"Release {version}\"";
    if (dryRun)
    {
        Console.WriteLine($"   [DRY RUN] git {command}");
    }
    else
    {
        await RunCommandAsync("git", command, captureOutput: false);
        Console.WriteLine($"   [OK] Created tag: v{version}");
    }
}

async Task PushTagAsync(string version, bool force, bool dryRun)
{
    var command = $"push {(force ? "--force " : "")}origin v{version}";
    if (dryRun)
    {
        Console.WriteLine($"   [DRY RUN] git {command}");
    }
    else
    {
        await RunCommandAsync("git", command, captureOutput: false);
        Console.WriteLine($"   [OK] Pushed tag: v{version}");
    }
}

bool IsCommandAvailable(string command)
{
    try
    {
        var p = Process.Start(new ProcessStartInfo
        {
            FileName = command,
            Arguments = "--version",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        });
        p?.WaitForExit(5000);
        return p?.ExitCode == 0;
    }
    catch { return false; }
}

async Task<(int ExitCode, string Output)> RunCommandAsync(string fileName, string arguments, bool captureOutput)
{
    var p = Process.Start(new ProcessStartInfo
    {
        FileName = fileName,
        Arguments = arguments,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
    }) ?? throw new Exception($"Failed to start: {fileName}");
    
    var output = await p.StandardOutput.ReadToEndAsync();
    var error = await p.StandardError.ReadToEndAsync();
    await p.WaitForExitAsync();
    
    if (!captureOutput && !string.IsNullOrWhiteSpace(output))
        Console.WriteLine(output);
    if (p.ExitCode != 0 && !string.IsNullOrWhiteSpace(error))
        Console.Error.WriteLine(error);
    
    return (p.ExitCode, output);
}
