# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Cryengine Converter is a C# tool that converts Cryengine game assets (.cgf, .cga, .chr, .skin) into portable 3D formats (Collada .dae, glTF .gltf/.glb). It supports multiple Cryengine variants including traditional Cryengine games (MWO, Crysis) and Star Citizen's proprietary #ivo format.

## Shell Usage

When the working directory is already the repo root, run commands directly without `cd`. Only use `cd` if you need to change to a different directory.

## Common Commands

### Build
```bash
dotnet build
```
Build the entire solution (default configuration: Debug).

### Test
```bash
# Run unit tests only (fast)
dotnet test --filter TestCategory=unit

# Run all tests including integration tests (slow, requires test data)
dotnet test
```

Integration tests are organized by game (StarCitizenTests, MWOIntegrationTests, CrysisIntegrationTests, etc.) and require actual game asset files in specific test data directories.

### Publish
```bash
dotnet publish
```
Creates a self-contained executable at `cgf-converter\bin\Release\net9.0\win-x64\publish\cgf-converter.exe`. The published binary is a single-file executable with all dependencies embedded.

### Run Converter
```bash
# From the cgf-converter project directory
dotnet run -- <cgf-file> [options]

# Example: Convert to Collada
dotnet run -- "C:\GameAssets\ship.cga" -objectdir "C:\GameAssets\Objects"

# Example: Convert to glTF
dotnet run -- ship.cgf -gltf -objectdir "C:\GameAssets\Objects"
```

### Build the Windows Installer

End-user installer is built with **Inno Setup 6** (per-user install by default, with PATH editing).

**Prerequisite (one-time):** install Inno Setup from https://jrsoftware.org/isinfo.php. On this machine, `ISCC.exe` lives at `C:\Users\Geoff\AppData\Local\Programs\Inno Setup 6\ISCC.exe` (per-user install — not on PATH unless explicitly added).

```powershell
# Step 1: publish the self-contained Windows binary (must come first — installer pulls from this path)
dotnet publish cgf-converter\cgf-converter.csproj -c Release

# Step 2: compile the installer
& "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe" installer\cgf-converter.iss

# Output: installer\output\cgf-converter-setup-<version>.exe
```

**Files involved:**
- `installer/cgf-converter.iss` — the Inno Setup script (commented thoroughly)
- `installer/README.txt` — short readme that ships with the install
- `installer/output/` — gitignored, holds the compiled installer

**Critical invariants:**
- The `AppId` GUID in `cgf-converter.iss` (`267AC84D-FD30-4860-A971-393139539822`) **must stay stable across versions**. Changing it will create duplicate Add/Remove Programs entries and break upgrades.
- The version in `cgf-converter.iss` (`MyAppVersion`) is currently hardcoded — **keep it in sync with `Directory.Build.props`** when bumping versions, or template it via `iscc /D` later.
- The publish-output path in the script (`PublishDir`) targets `cgf-converter\bin\Release\net9.0\win-x64\publish` — if the TargetFramework changes, update this path too.

**Testing the installer:**
The safe way is **Windows Sandbox** (Settings → Apps → Optional Features → enable "Windows Sandbox", reboot once). Drag the `.exe` into a sandbox session, install, open a new PowerShell, run `cgf-converter -usage`, then uninstall and verify PATH is clean. Sandbox state is disposable, so installer bugs can't pollute the dev machine. Avoid testing PATH edits on the actual dev machine — uninstall regressions there are tedious to clean up.

**Inno Setup deprecation note:** the script uses `WizardIsTaskSelected` (modern). Older `IsTaskSelected` still works but emits a [Hint] warning at compile time. If a future update emits new deprecation hints, fix them rather than ignoring — Inno's deprecations get removed eventually.

### Release workflow

Releases are produced by `.github/workflows/release.yml`, which has two trigger modes:

**Tag push (final releases):**
```bash
git tag v2.0.0
git push --tags
```
Creates a new GitHub Release at the tag, uploads installer + raw exe + PDBs. The workflow asserts the tag matches the version in `Directory.Build.props` and refuses to release on mismatch — keeps `Directory.Build.props` as the single source of truth.

**Manual dispatch (pre-releases / re-runs):**
- Go to Actions → "Release" → "Run workflow"
- Pick the branch (e.g. `release/v2.0`)
- Provide the existing GitHub Release tag (e.g. `v2.0.0`)
- Workflow builds from HEAD of that branch and **clobbers** assets onto the existing Release entry

The pre-release flag on the GitHub Release entry is preserved across dispatch runs — the workflow never toggles it. To make a Release entry "pre-release," edit it in the GitHub UI once; subsequent clobbers don't change the flag. To promote pre-release → final, uncheck "Set as a pre-release" in the GitHub UI when ready.

**Critical:** dispatch refuses to clobber a non-existent release. If the target tag doesn't have a Release yet, create it manually in the UI first (so you can set the pre-release flag intentionally), then dispatch.

**Pattern in use:** ONE rolling pre-release entry per major version (avoids RC clutter on the Releases page). RC tags can exist locally in git for traceability but the workflow ignores them.

**Commit SHA in builds:** the workflow passes `/p:SourceRevisionId=$GITHUB_SHA` to `dotnet publish`, which stamps the SHA into the assembly's `InformationalVersion`. `cgf-converter -usage` reads this attribute, so CI builds print `v2.0.0+a3f72b1c...` — every reported bug pins to an exact commit. Local builds without a SHA print just `v2.0.0`.

**Code signing:** placeholder commented in the workflow. When a code signing cert is available (SignPath OSS, MS Trusted Signing, or purchased OV/EV), uncomment the `signtool sign` steps and add the cert thumbprint to GitHub repository secrets as `CERT_THUMBPRINT`. Always include `/tr <timestamp-url>` so signatures survive cert expiry.

## Versioning

The application version is managed in `Directory.Build.props` at the repo root. This single file sets the version for all projects in the solution.

When making changes, consider whether a version bump is appropriate:
- **Release branches** (e.g., `release/v2.0`): Do not increment the version. The target version is already set for the release.
- **Bugfixes or features on master**: Ask the user if a version bump is needed before changing `Directory.Build.props`.

## Solution Structure

### Projects
- **CgfConverter** - Core library containing all conversion logic
- **cgf-converter** - CLI executable entry point
- **CgfConverterIntegrationTests** - Test suite (MSTest) with unit and integration tests
- **CgfConverterTestingConsole** - Claude's data inspection console (see `cryengine-inspect` skill). Claude owns this project and can freely modify `Program.cs` to inspect CryEngine data. No tests required — it's a debugging tool, not production code.

### Key Dependencies
- .NET 9.0 target framework
- ImageSharp (texture processing)
- BCnEncoder.Net (DDS texture compression)
- Newtonsoft.Json (glTF serialization)
- XmlSerializer.Generator (Collada performance optimization)

### External Tools
- **OpenUSD CLI tools** at `D:\USD\install\bin` (on PATH): `usdcat`, `usdtree`, `sdffilter`, `sdfdump`. See `usd-inspect` skill for usage reference.

## High-Level Architecture

### Conversion Pipeline

```
Input File (.cgf/.cga/.chr/.skin)
  ↓
Model.FromStream() - Read file header, chunk table, parse all chunks
  ↓
CryEngine.ProcessCryengineFiles() - Build node hierarchy, load materials, create skinning info
  ↓
IRenderer.Render() - Serialize to output format
  ↓
Output File (.dae/.gltf/.glb/.usd)
```

### Core Components

**CryEngine (Facade)**: High-level API for processing Cryengine files. Entry point: `CryEngine.ProcessCryengineFiles()`.

**Model**: Represents a single Cryengine file with its chunks. Contains chunk table, node hierarchy, and file metadata.

**Chunk System**: ~50+ chunk types implementing versioned polymorphism. Each chunk type has version-specific implementations (e.g., `ChunkMesh_800`, `ChunkMesh_801`). The factory pattern uses reflection to instantiate chunks based on type and version: `Chunk.New<T>(version)`.

**Renderers**: Implement `IRenderer` interface for pluggable output formats:
- `ColladaModelRenderer` - Default, fully featured (.dae)
- `GltfModelRenderer` - Modern format (.gltf/.glb)
- `UsdRenderer` - Experimental USD export (.usd/.usda/.usdc)
- `WavefrontModelRenderer` - Deprecated, not supported (.obj)

**Material System**: Loads Cryengine .mtl files (text XML or binary CryXmlB format). Supports hierarchical submaterials, texture maps, and material layers. Resolution cascade: explicit paths → ChunkMtlName references → default materials.

**PackFileSystem**: File system abstraction supporting direct filesystem access (`RealFileSystem`) and .pak archives (`WiiuStreamPackFileSystem`, `CascadedPackFileSystem`).

### Key Data Structures

**GeometryInfo**: Aggregated geometry data including vertices, normals, UVs, indices, material subsets, and vertex colors. Contains `Datastream<T>` for typed data arrays.

**SkinningInfo**: Consolidated bone hierarchy and vertex skinning weights from multiple chunks (`ChunkCompiledBones`, `ChunkCompiledIntSkinVertices`, `ChunkCompiledExtToIntMap`).

**Material**: Hierarchical material system with submaterials, textures (diffuse, normal, specular), colors, and shader parameters.

### File Format Variants

**Traditional Cryengine**: Node hierarchy using `ChunkNode → ChunkMesh → ChunkDataStream`. May have companion geometry files (.cgam/.skinm) auto-detected and merged.

**#ivo Format (Star Citizen 3.23+)**: Uses `ChunkNodeMeshCombo` + `ChunkIvoSkinMesh`. Consolidated mesh data in single chunk with `VertUV` structure. Vertices are bounding-box compressed and require decompression.

### Version Handling

Cryengine versions (0x744, 0x745, 0x746, 0x900) are handled via naming convention: `ChunkName_VERSION.cs`. The factory uses reflection to find and instantiate the correct version at runtime. When adding support for a new chunk version, create a new file following this pattern.

## Important Implementation Details

### Coordinate Systems
Cryengine uses Z-up coordinate system. Collada output preserves Z-up. Transform matrices are transposed when needed for format compatibility.

### Binary XML (CryXmlB)
Star Citizen uses binary XML for material files. Parser in `CryXmlSerializer.cs` handles magic signature "CryXmlB\0" and deserializes node/reference/content tables. Legacy "pbxml\0" format is also supported.

### Multithreading
Release builds support parallel file processing via `-maxthreads` argument. Debug builds are single-threaded for easier debugging. Use `#if DEBUG` guards around threading logic.

### Test Categories
Integration tests use `[TestCategory("unit")]` or `[TestCategory("integration")]` attributes. Unit tests run fast without external dependencies. Integration tests require game asset files and validate XML schema compliance.

### Manual Render Tests (Fast Iteration)
For quick iteration when developing renderers, use `ManualRenderTests.cs` in `CgfConverterIntegrationTests/ManualTests/`. These tests:
- Run directly from Visual Studio Test Explorer (no publish/command-line needed)
- Output files to the source asset's directory (e.g., `.usda` next to `.cga`)
- Are excluded from CI via `[TestCategory("manual")]`

**To add a new test file:**
```csharp
[TestMethod]
public void MWO_YourAsset_USD()
{
    RenderToUsd($@"{mwoObjectDir}\path\to\asset.cga", mwoObjectDir);
}
```

Helper methods available: `RenderToUsd()`, `RenderToCollada()`, `RenderToGltf()`

### Material File Resolution
The `-objectdir` argument is critical for correct material loading. Without it, materials may not be found and defaults will be generated. The resolver caches paths and tries multiple locations (as-provided, same directory, ObjectDir).

## Namespace Organization

- **CgfConverter.CryEngineCore**: Core chunk reading (`Chunks/`, `Model.cs`)
- **CgfConverter.Models**: Domain models (`Materials/`, `GeometryInfo.cs`, `SkinningInfo.cs`)
- **CgfConverter.Renderers**: Output renderers (`Collada/`, `Gltf/`, `Wavefront/`, `USD/`)
- **CgfConverter.PackFileSystem**: File system abstraction
- **CgfConverter.CryXmlB**: Binary XML parser
- **CgfConverter.Utilities**: Helper classes and utilities
- **CgfConverter.Services**: Service classes like `ArgsHandler`

## Critical Files

- `cgf-converter/cgf-converter.cs` - CLI entry point and argument parsing
- `CgfConverter/CryEngine/CryEngine.cs` - Main conversion orchestration
- `CgfConverter/CryEngineCore/Model.cs` - File reading and chunk parsing
- `CgfConverter/CryEngineCore/Chunks/Chunk.cs` - Chunk factory and base class
- `CgfConverter/Renderers/Collada/ColladaModelRenderer.cs` - Collada output
- `CgfConverter/Renderers/Gltf/GltfModelRenderer.cs` - glTF output
- `CgfConverter/Renderers/USD/UsdRenderer.cs` - USD output
- `CgfConverter/Utilities/MaterialUtilities.cs` - Material loading logic
- `CgfConverter/CryXmlB/CryXmlSerializer.cs` - Binary XML parser
- `CgfConverter/Models/GeometryInfo.cs` - Geometry aggregation
- `CgfConverter/Models/SkinningInfo.cs` - Bone/skinning data

## Common Development Patterns

### Adding a New Chunk Type
1. Create `Chunks/ChunkNewType_VERSION.cs` inheriting from `Chunk`
2. Override `Read(BinaryReader)` to parse chunk data
3. Add chunk type ID to `ChunkType` enum in `Enums.cs`
4. Factory will auto-discover via reflection

### Adding a New Output Format
1. Create renderer class implementing `IRenderer`
2. Implement `Render(CryEngine, string)` method
3. Add command-line argument in `ArgsHandler.cs`
4. Add renderer instantiation in `cgf-converter.cs`

### Debugging Chunk Reading Issues
1. Use `CgfConverterTestingConsole` project for focused debugging
2. Enable `-throw` argument to surface exceptions in debugger
3. Check `Model.ChunkMap` to see what chunks were parsed
4. Verify chunk version matches expected format

### Working with Materials
Materials are loaded lazily during `CreateMaterials()`. Check `MaterialUtilities.LoadMaterial()` for resolution logic. Binary XML materials require `CryXmlSerializer` - add breakpoints there if materials aren't loading correctly.

## Renderer Architecture Pattern

### Partial Classes Pattern
Renderers use partial classes for organization, following the GltfRenderer precedent.

**Rationale**:
- Maintains single logical type with shared private state
- Easy navigation and IntelliSense support
- Avoids state management complexity of composition
- Natural fit for tightly coupled rendering operations

**Standard Organization** (apply to new/refactored renderers):
- `{Renderer}.cs` - Main class, constructor, public API, orchestration
- `{Renderer}.Materials.cs` - Material/shader creation, texture handling
- `{Renderer}.Geometry.cs` - Mesh creation, vertex/index processing
- `{Renderer}.Skeleton.cs` - Skeletal animation, skinning
- `{Renderer}.Animation.cs` - Animation export
- `{Renderer}.Utilities.cs` - Helper methods

**Current implementations**:
- **ColladaModelRenderer**: `.cs`, `.Animation.cs`, `.Materials.cs`, `.Geometry.cs`, `.Skeleton.cs`, `.Nodes.cs`, `.Utilities.cs`
- **UsdRenderer**: `.cs`, `.Animation.cs`, `.Materials.cs`, `.Geometry.cs`, `.Skeleton.cs`
- **GltfModelRenderer**: Uses partial classes in `Renderers/Gltf/` with Models subdirectory for data types

## Animation Support

### Supported Animation Formats

**DBA Files** (Animation Databases):
- Container format holding multiple animations
- Referenced via `.chrparams` file's `$TracksDatabase` entry (supports wildcards like `*.dba`)
- Parsed by `ChunkController_905` with two storage modes:
  - **Standard mode**: Positive offsets relative to start of track data
  - **In-place streaming mode**: Negative offsets relative to end of track data (detected by `keyTimeOffsets[0] < 0`)

**CAF Files** (Individual Animation Clips):
- Single animation per file
- Referenced via `.chrparams` `<Animation name="..." path="..."/>` entries
- Support wildcard patterns (e.g., `animations/pilot/*.caf`)
- Controller versions: `ChunkController_829`, `ChunkController_830`, `ChunkController_831`

**CAL Files** (Animation Lists):
- XML files defining animation sets (used by ArcheAge)
- Reference `.caf` files with additional metadata
- Loaded via `LoadCalAnimations()` in `CryEngine.cs`

### Critical Animation Implementation Details

**CryEngine Matrix Convention**: Translation stored in column 4 (M14, M24, M34), NOT row 4. **Never use `Matrix4x4.Translation` property** - it reads M41/M42/M43 which are zeros.

**Rest Translation Fallback**: Bones without position animation must use rest translation from skeleton bind pose. Otherwise skeleton collapses.

**Additive Animations**: Detected via `AssetFlags.Additive` (0x001). Must convert deltas to absolute transforms: `absolute = rest * additive`

### Key Animation Files

- `CgfConverter/CryEngine/CryEngine.cs`: `LoadCafAnimations()`, `LoadCalAnimations()`, `ExpandCafWildcard()`, `ExpandDbaWildcard()`
- `CgfConverter/Models/CafAnimation.cs`: Animation data model
- `CgfConverter/CryEngineCore/Chunks/ChunkController_905.cs`: DBA parsing (both storage modes)
- `CgfConverter/CryEngineCore/Chunks/ChunkController_829.cs`: CAF parsing
- `CgfConverter/Renderers/USD/UsdRenderer.Animation.cs`: USD export with additive conversion

## Active Development Notes

See `DEVNOTES.md` for:
- USD shader-based material system planning
- Known issues and debugging history
- In-progress feature work
- Star Citizen #ivo animation format (next target)
