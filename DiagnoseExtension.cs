#!/usr/bin/env dotnet run
#:property EnableDefaultEmbeddedResourceItems=false

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

// ============================================================================
// WGSL Extension Diagnostic & Fix Script
// ============================================================================
// This script helps diagnose why the WGSL extension isn't working
// ============================================================================

Console.WriteLine("??????????????????????????????????????????????????????????????????");
Console.WriteLine("?  WGSL Extension Diagnostic Tool                               ?");
Console.WriteLine("??????????????????????????????????????????????????????????????????");
Console.WriteLine();

// Step 1: Check if extension is built
Console.WriteLine("?? Step 1: Checking if extension is built...");
var vsixPath = "PanoramicData.VisualStudio.WgslLanguageSupport\\bin\\Release\\PanoramicData.VisualStudio.WgslLanguageSupport.vsix";
if (File.Exists(vsixPath))
{
    var vsixInfo = new FileInfo(vsixPath);
    Console.WriteLine($"   ? VSIX found: {vsixInfo.Length / 1024} KB");
    Console.WriteLine($"   ? Modified: {vsixInfo.LastWriteTime}");
}
else
{
    Console.WriteLine("   ? VSIX not found! Build the project first.");
    return 1;
}

// Step 2: Check Experimental Instance
Console.WriteLine("\n?? Step 2: Checking Experimental Instance...");
var expPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "Microsoft\\VisualStudio");

var expDirs = Directory.GetDirectories(expPath, "18.0_*Exp");
if (expDirs.Length > 0)
{
    Console.WriteLine($"   ? Found {expDirs.Length} Experimental Instance(s)");
    foreach (var dir in expDirs)
    {
        var dirName = Path.GetFileName(dir);
        Console.WriteLine($"     • {dirName}");
    }
}
else
{
    Console.WriteLine("   ? No Experimental Instance found");
}

// Step 3: Check if extension is deployed
Console.WriteLine("\n?? Step 3: Checking if extension is deployed...");
var deployed = false;
foreach (var expDir in expDirs)
{
    var extPath = Path.Combine(expDir, "Extensions");
    if (Directory.Exists(extPath))
    {
        var wgslDirs = Directory.GetDirectories(extPath, "*", SearchOption.AllDirectories)
            .Where(d => d.Contains("WGSL") || d.Contains("PanoramicData"));
        
        if (wgslDirs.Any())
        {
            deployed = true;
            Console.WriteLine($"   ? Extension deployed to: {Path.GetFileName(expDir)}");
            foreach (var dir in wgslDirs)
            {
                var dllPath = Path.Combine(dir, "PanoramicData.VisualStudio.WgslLanguageSupport.dll");
                if (File.Exists(dllPath))
                {
                    var dllInfo = new FileInfo(dllPath);
                    Console.WriteLine($"     • DLL: {dllInfo.LastWriteTime}");
                }
            }
        }
    }
}

if (!deployed)
{
    Console.WriteLine("   ? Extension not deployed to Experimental Instance");
}

// Step 4: Check if VS Experimental Instance is running
Console.WriteLine("\n?? Step 4: Checking if Experimental Instance is running...");
var processes = Process.GetProcessesByName("devenv");
var expProcess = processes.FirstOrDefault(p => 
{
    try
    {
        return p.MainWindowTitle.Contains("Experimental") || 
               p.MainWindowTitle.Contains("example.wgsl");
    }
    catch
    {
        return false;
    }
});

if (expProcess != null)
{
    Console.WriteLine($"   ??  Experimental Instance IS running (PID: {expProcess.Id})");
    Console.WriteLine($"      Title: {expProcess.MainWindowTitle}");
}
else
{
    Console.WriteLine("   ? No Experimental Instance running");
}

// Recommendations
Console.WriteLine("\n??????????????????????????????????????????????????????????????????");
Console.WriteLine("?  ?? RECOMMENDATIONS                                            ?");
Console.WriteLine("??????????????????????????????????????????????????????????????????");
Console.WriteLine();

if (expProcess != null)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("??  ISSUE: Extension may be cached or not loaded");
    Console.ResetColor();
    Console.WriteLine("\n? FIX:");
    Console.WriteLine("   1. Close the Experimental Instance window");
    Console.WriteLine("   2. Clear MEF cache:");
    Console.WriteLine("      devenv /clearcache /rootsuffix Exp");
    Console.WriteLine("   3. Press F5 in your main Visual Studio");
}
else
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("? Ready to test!");
    Console.ResetColor();
    Console.WriteLine("\n?? To launch Experimental Instance:");
    Console.WriteLine("   • Press F5 in Visual Studio");
    Console.WriteLine("   • Or run: devenv /rootsuffix Exp");
}

Console.WriteLine("\n?? Alternative: Reset Experimental Instance");
Console.WriteLine("   devenv /resetSettings /rootsuffix Exp");

return 0;
