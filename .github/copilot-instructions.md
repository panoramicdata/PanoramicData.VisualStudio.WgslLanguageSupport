# Copilot Instructions for WGSL Language Support Project

## Documentation Management

**IMPORTANT**: This project uses a **SINGLE SOURCE OF TRUTH** for all project documentation.

### Documentation Rules

1. **MASTER_PLAN.md** is the **ONLY** project documentation file (besides README.md)
   - All project status, progress, implementation details, testing guides, publishing instructions, and future plans go here
   - Update MASTER_PLAN.md when adding features, fixing issues, or changing project status
   - **DO NOT** create separate documentation files like:
     - IMPLEMENTATION_SUMMARY.md
     - TESTING_GUIDE.md
     - NEXT_STEPS.md
     - PROJECT_STATUS.md
     - PUBLISHING_GUIDE.md
     - PRE_PUBLISHING_CHECKLIST.md
     - etc.

2. **README.md** is for **end-users and GitHub visitors**
   - Keep it concise and focused on what the extension does
   - Installation instructions
   - Features overview
   - Basic usage
   - Links to marketplace (when published)

3. **CHANGELOG.md** is the **ONLY** other allowed documentation file
   - Version history only
   - What changed in each release
   - Follow semantic versioning

### When User Asks for Documentation Updates

- Update the appropriate section in MASTER_PLAN.md
- Do NOT create new .md files
- Do NOT create summary documents
- Do NOT duplicate information across multiple files

### Project Structure

- **Source Code**: Keep clean, well-commented C# code
- **Tests**: Maintain comprehensive unit tests
- **Documentation**: MASTER_PLAN.md + README.md + CHANGELOG.md only

### Making Changes

When implementing new features or fixing bugs:

1. Update the code
2. Update tests
3. Update MASTER_PLAN.md status section
4. Update CHANGELOG.md if releasing
5. That's it - no other documentation needed

## Project-Specific Guidelines

### Technology Stack

- **Target Framework**: .NET Framework 4.8 (required for VS extensions)
- **VS SDK**: Traditional VSIX SDK (in-process MEF)
- **NOT** using the new VisualStudio.Extensibility SDK (out-of-process)

### Automation & Scripting

- **PREFER C# single-file scripts** (`.cs` files) over PowerShell for automation
- Use `dotnet run Script.cs` pattern (see `Publish.cs`, `TagAndPush.cs`, `Coverage.cs`)
- C# scripts are:
  - Cross-platform (Windows/Linux/macOS)
  - Type-safe
  - Easy to debug
  - Consistent with project technology stack
- Only use PowerShell/batch files for simple, Windows-specific tasks
- Existing C# scripts in project:
  - `Publish.cs` - Build and publish VSIX
  - `TagAndPush.cs` - Create and push git tags with NBGV
  - `Coverage.cs` - Generate code coverage reports

### Common Issues

1. **Always target .NET Framework 4.8**, not .NET 8
2. **Include System.ComponentModel.Composition** reference for MEF
3. **Base content type** should be "code", not CodeRemoteContentDefinition
4. **Test in Experimental Instance** (F5) before considering anything done

### Build Requirements

- Visual Studio 2022 with VS extension development workload
- .NET Framework 4.8 Developer Pack
- All packages from NuGet restore automatically

## Response Style

- Be concise
- Focus on the user's immediate question
- Don't create documentation just for the sake of documenting
- MASTER_PLAN.md already has everything - just update it

## Publishing

When ready to publish:
- Follow publishing section in MASTER_PLAN.md
- Don't create separate publishing guides
- Update MASTER_PLAN.md with publishing status
