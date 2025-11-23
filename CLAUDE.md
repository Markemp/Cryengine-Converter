# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Cryengine Converter is a C# tool that converts Cryengine game assets (.cgf, .cga, .chr, .skin) into portable 3D formats (Collada .dae, glTF .gltf/.glb). It supports multiple Cryengine variants including traditional Cryengine games (MWO, Crysis) and Star Citizen's proprietary #ivo format.

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

## Solution Structure

### Projects
- **CgfConverter** - Core library containing all conversion logic
- **cgf-converter** - CLI executable entry point
- **CgfConverterTests** - Integration test suite (MSTest)
- **CgfConverterTestingConsole** - Testing/debugging console

### Key Dependencies
- .NET 9.0 target framework
- ImageSharp (texture processing)
- BCnEncoder.Net (DDS texture compression)
- Newtonsoft.Json (glTF serialization)
- XmlSerializer.Generator (Collada performance optimization)

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
Output File (.dae/.gltf/.glb/.obj)
```

### Core Components

**CryEngine (Facade)**: High-level API for processing Cryengine files. Entry point: `CryEngine.ProcessCryengineFiles()`.

**Model**: Represents a single Cryengine file with its chunks. Contains chunk table, node hierarchy, and file metadata.

**Chunk System**: ~50+ chunk types implementing versioned polymorphism. Each chunk type has version-specific implementations (e.g., `ChunkMesh_800`, `ChunkMesh_801`). The factory pattern uses reflection to instantiate chunks based on type and version: `Chunk.New<T>(version)`.

**Renderers**: Implement `IRenderer` interface for pluggable output formats:
- `ColladaModelRenderer` - Default, fully featured (.dae)
- `GltfModelRenderer` - Modern format (.gltf/.glb)
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

### Material File Resolution
The `-objectdir` argument is critical for correct material loading. Without it, materials may not be found and defaults will be generated. The resolver caches paths and tries multiple locations (as-provided, same directory, ObjectDir).

## Namespace Organization

- **CgfConverter.CryEngineCore**: Core chunk reading (`Chunks/`, `Model.cs`)
- **CgfConverter.Models**: Domain models (`Materials/`, `GeometryInfo.cs`, `SkinningInfo.cs`)
- **CgfConverter.Renderers**: Output renderers (`Collada/`, `Gltf/`, `Wavefront/`)
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

## TODO - Renderer Architecture & Maintainability

### Refactor Renderer Organization
**Problem**: Renderer classes are becoming large monolithic files that are hard to maintain:
- `ColladaModelRenderer.cs`: **1585 lines** - single file
- `UsdRenderer.cs`: **463 lines** - growing quickly
- `GltfRenderer`: Uses partial classes to split concerns (Animation, Material, Buffers, Geometry, SwapAxes)

**Options to Consider**:

1. **Partial Classes** (GltfRenderer approach)
   - `UsdRenderer.Materials.cs` - Material and shader creation
   - `UsdRenderer.Geometry.cs` - Mesh and geometry processing
   - `UsdRenderer.Hierarchy.cs` - Node hierarchy and transforms
   - `UsdRenderer.cs` - Main coordination and public API
   - **Pros**: Keeps everything in one logical type, easy navigation, shared private state
   - **Cons**: Still technically one class, can feel like hiding complexity

2. **Component Classes with Composition**
   - `UsdMaterialBuilder.cs` - Creates materials/shaders
   - `UsdGeometryBuilder.cs` - Handles mesh creation
   - `UsdHierarchyBuilder.cs` - Builds node trees
   - `UsdRenderer.cs` - Orchestrates components
   - **Pros**: Better separation of concerns, easier to test components, clearer dependencies
   - **Cons**: More files, need to manage state sharing, potential over-engineering

3. **Hybrid Approach**
   - Keep UsdRenderer partial classes for closely coupled logic
   - Extract independent utilities/builders as separate classes (e.g., `UsdMaterialBuilder`, `UsdTextureResolver`)
   - **Pros**: Balance of organization without over-abstraction
   - **Cons**: Need to decide what stays vs. what gets extracted

**Current UsdRenderer responsibilities**:
- Materials: `CreateMaterials()`, `CreateShaders()`, `CreateUsdImageTextureShader()`, `GetMaterialName()`
- Hierarchy: `CreateNodeHierarchy()`, `CreateNode()`
- Geometry: `CreateMeshPrim()`
- Utilities: `CleanPathString()`, `ResolveTextureFile()`
- Orchestration: `GenerateUsdObject()`, `Render()`, `WriteUsdToFile()`

**Action Items**:
- Evaluate which approach best fits the codebase philosophy
- Consider applying same pattern to ColladaModelRenderer (needs refactoring most urgently)
- Ensure test coverage before refactoring
- Document chosen pattern in this file for consistency across renderers

## TODO - USD Export

### Material System Improvements
- **Metallic detection heuristic**: Investigate using `Shader`, `MtlFlags`, or specular properties to auto-detect metallic materials
  - Check if `Shader="Metal"` exists in any materials
  - Consider using `Glossiness` property (currently unused)
  - Analyze specular color patterns (colored specular might indicate metallic)
- **Texture support**: Add diffuse/normal/specular texture mapping to USD materials
- **Material validation**: Ensure all MTL properties map correctly to USD PBR workflow

### Known Issues
- ~~**GeomSubset indices**: "invalid indices" warning in Blender~~ - FIXED: Convert vertex indices to face indices for elementType="face"
- ~~**Normal count mismatch**: "Loop normal count mismatch" warning~~ - FIXED: Expand normals array to match faceVertexIndices for faceVarying interpolation
