#!/usr/bin/env dotnet run
#:property Nullable=enable
#:package System.IO.Compression@*

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

// Find the VSIX file
var vsixFiles = Directory.GetFiles("PanoramicData.VisualStudio.WgslLanguageSupport/bin", "*.vsix", SearchOption.AllDirectories)
    .OrderByDescending(f => new FileInfo(f).LastWriteTime)
    .ToArray();

if (vsixFiles.Length == 0)
{
    Console.WriteLine("[X] No VSIX file found. Build the project first.");
    Environment.Exit(1);
}

var vsixPath = vsixFiles[0];
Console.WriteLine($"Checking VSIX: {Path.GetFileName(vsixPath)}");
Console.WriteLine($"Path: {vsixPath}");
Console.WriteLine($"Built: {new FileInfo(vsixPath).LastWriteTime}");
Console.WriteLine();

// Extract and read the manifest
using var zip = ZipFile.OpenRead(vsixPath);
var manifestEntry = zip.Entries.FirstOrDefault(e => e.Name == "extension.vsixmanifest");

if (manifestEntry == null)
{
    Console.WriteLine("[X] extension.vsixmanifest not found in VSIX");
    Environment.Exit(1);
}

using var stream = manifestEntry.Open();
var xml = XDocument.Load(stream);

// Get namespace
var ns = xml.Root?.Name.Namespace ?? XNamespace.None;

// Find Identity element and get Version attribute
var identity = xml.Descendants(ns + "Identity").FirstOrDefault();
if (identity != null)
{
    var version = identity.Attribute("Version")?.Value;
    if (version != null)
    {
        Console.WriteLine($"[OK] VSIX Version: {version}");
        
        // Also show NBGV version for comparison
        try
        {
            var nbgvVersion = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "nbgv",
                Arguments = "get-version -v Version",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });
            
            if (nbgvVersion != null)
            {
                var nbgvOutput = nbgvVersion.StandardOutput.ReadToEnd().Trim();
                nbgvVersion.WaitForExit();
                Console.WriteLine($"[OK] NBGV Version: {nbgvOutput}");
                
                if (version == nbgvOutput)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\n[OK] Versions match! NBGV integration is working correctly.");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"\n[!] Version mismatch!");
                    Console.WriteLine($"    VSIX:  {version}");
                    Console.WriteLine($"    NBGV:  {nbgvOutput}");
                    Console.ResetColor();
                }
            }
        }
        catch
        {
            // NBGV not available, skip comparison
            Console.WriteLine("[!] NBGV not available for comparison");
        }
    }
    else
    {
        Console.WriteLine("[X] Could not find Version attribute in Identity element");
        Environment.Exit(1);
    }
}
else
{
    Console.WriteLine("[X] Could not find Identity element in manifest");
    Environment.Exit(1);
}
