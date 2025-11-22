# Changelog

All notable changes to the WGSL Language Support extension will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2024-11-22

### Added
- Initial release of WGSL Language Support for Visual Studio
- File association for `.wgsl` files
- Syntax highlighting using TextMate grammar (from PolyMeilex/vscode-wgsl)
- Comprehensive WGSL linting with 8 validation rule categories:
  1. Missing semicolons detection
  2. Unmatched braces detection (handles `{}`, `()`, `[]`)
  3. Invalid attribute validation (`@vertex`, `@fragment`, `@compute`, `@binding`, etc.)
  4. Stage function validation (proper return types for vertex/fragment shaders)
  5. Undefined type detection (validates against all WGSL built-in types)
  6. Duplicate binding detection
  7. Workgroup size validation (1-256 per dimension, warning for >256 total invocations)
  8. Variable declaration validation (requires type or initializer)
- Real-time error highlighting with red squiggles
- Support for Visual Studio 2022-2026 (versions 17.0-19.0)
- Support for all VS editions (Community, Professional, Enterprise)
- Comment and string-aware parsing (skips line comments, block comments, strings)
- Comprehensive built-in type coverage:
  - Scalar types: `bool`, `i32`, `u32`, `f32`, `f16`
  - Vector types: `vec2/3/4` with typed variants
  - Matrix types: all `mat2x2` through `mat4x4` variants
  - Texture types: all 1D, 2D, 3D, cube, array, multisampled, storage, depth variants
  - Sampler types and special types (`array`, `ptr`, `atomic`)

### Technical
- Built with .NET Framework 4.8
- Traditional VSIX SDK (in-process MEF)
- Old-style .csproj format (required for VSIX packaging)
- MEF-based content type and tagger providers
- TextMate grammar integration via pkgdef registration

### Known Limitations
- Linting is regex-based; full AST parsing not yet implemented
- IntelliSense (auto-completion) not yet available
- No semantic analysis (variable scope, type inference, etc.)
- No code formatting or refactoring tools

## [Unreleased]

### Planned for v1.1.0
- Enhanced semantic analysis
- IntelliSense (auto-completion for keywords, types, built-ins)
- Signature help for built-in functions
- Improved TextMate grammar integration
- Performance optimizations for large files

### Planned for v2.0.0
- Full AST-based parsing
- Go to Definition
- Find All References
- Rename refactoring
- Code formatting
- Symbol search
