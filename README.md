# PanoramicData.VisualStudio.WgslLanguageSupport

WGSL Language Support for Visual Studio by Panoramic Data Limited

A Visual Studio 2022 extension that provides comprehensive language support for WebGPU Shading Language (WGSL) files.

## Features

- ?? **Syntax Highlighting** - Comprehensive WGSL syntax coloring via TextMate grammar
- ?? **Real-Time Linting** - Comprehensive error detection with 8 validation categories:
  - Missing semicolons
  - Unmatched braces (`{}`, `()`, `[]`)
  - Invalid attributes (`@vertex`, `@fragment`, `@compute`, etc.)
  - Stage function validation (proper return types)
  - Undefined type detection (validates all WGSL built-in types)
  - Duplicate binding detection
  - Workgroup size validation
  - Variable declaration validation
- ?? **File Association** - Automatic recognition of `.wgsl` files
- ? **Fast & Lightweight** - In-process extension with minimal overhead
- ?? **Comment-Aware** - Skips validation inside comments and strings
- ?? **Complete Type Coverage** - Recognizes all WGSL scalar, vector, matrix, texture, and sampler types

## Installation

### From Visual Studio Marketplace (Coming Soon)

Once published, you'll be able to install directly:

1. Open Visual Studio 2022
2. Go to **Extensions ? Manage Extensions**
3. Search for **"WGSL Language Support"**
4. Click **Download** and restart Visual Studio

### From Source

1. Clone this repository
2. Open `PanoramicData.VisualStudio.WgslLanguageSupport.slnx` in Visual Studio 2022
3. Build the solution (F6)
4. Press F5 to launch the Experimental Instance of Visual Studio
5. Create or open a `.wgsl` file to test the extension

### From VSIX

1. Download the latest `.vsix` file from the releases page
2. Double-click the `.vsix` file to install
3. Restart Visual Studio

## Development

### Prerequisites

- Visual Studio 2022 (17.0 or later)
- Visual Studio extension development workload
- .NET 8 SDK

### Project Structure

```
PanoramicData.VisualStudio.WgslLanguageSupport/
??? Grammars/
?   ??? wgsl.tmLanguage.json          # TextMate syntax grammar
??? WgslContentDefinition.cs          # Content type registration
??? WgslErrorTagger.cs                # Linting logic
??? WgslErrorTaggerProvider.cs        # Tagger provider
??? WgslLanguagePackage.cs            # VS Package registration
??? source.extension.vsixmanifest     # Extension manifest
```

### Building

```bash
dotnet build
```

### Testing

Press F5 in Visual Studio to launch the Experimental Instance with the extension loaded.

## Linting Rules

Currently implemented (prototype):
- Detection of `deprecated` keyword

### Planned WGSL-Specific Rules

- Missing semicolons
- Invalid type usage
- Undefined variables/functions
- Shader entry point validation (`@vertex`, `@fragment`, `@compute`)
- Attribute validation (`@binding`, `@group`, `@location`, etc.)
- Built-in function validation
- Type compatibility checks

## Attribution

This extension uses the WGSL TextMate grammar from [PolyMeilex/vscode-wgsl](https://github.com/PolyMeilex/vscode-wgsl) (MIT License).

## License

MIT License - see LICENSE.txt for details

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.
