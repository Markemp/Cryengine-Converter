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
- 'UsdRenderer' - Experimental USD export (.usd/.usda/.usdc)

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
For quick iteration when developing renderers, use `ManualRenderTests.cs` in `CgfConverterIntegrationTests/IntegrationTests/`. These tests:
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

## Renderer Architecture Pattern

### Partial Classes Pattern (ADOPTED)
**Decision**: Use partial classes pattern for renderer organization, following GltfRenderer precedent.

**Rationale**:
- Maintains single logical type with shared private state
- Easy navigation and IntelliSense support
- Avoids state management complexity of composition
- Consistent with existing GltfRenderer architecture
- Natural fit for tightly coupled rendering operations

**Standard Organization** (apply to new/refactored renderers):
- `{Renderer}.cs` - Main class, constructor, public API, orchestration (Render, GenerateObject, WriteToFile)
- `{Renderer}.Materials.cs` - Material/shader creation, texture handling
- `{Renderer}.Geometry.cs` - Mesh creation, vertex/index processing
- `{Renderer}.Skeleton.cs` - Skeletal animation, skinning (if applicable)
- `{Renderer}.Utilities.cs` - Helper methods (path cleaning, resolvers, etc.)

**Applied to UsdRenderer** (in progress):
- `UsdRenderer.cs` - Main orchestration
- `UsdRenderer.Materials.cs` - CreateMaterials, CreateShaders, shader loading/rules
- `UsdRenderer.Geometry.cs` - CreateMeshPrim, CreateNodeHierarchy, CreateNode
- `UsdRenderer.Skeleton.cs` - CreateSkeleton, AddSkinningAttributes, joint transforms

**Applied to ColladaModelRenderer** (COMPLETED):
- `ColladaModelRenderer.cs` - Main orchestration (144 lines)
- `ColladaModelRenderer.Animation.cs` - Animation export with matrix-based keyframes (322 lines)
- `ColladaModelRenderer.Materials.cs` - Material/texture creation (324 lines)
- `ColladaModelRenderer.Geometry.cs` - Mesh processing (429 lines)
- `ColladaModelRenderer.Skeleton.cs` - Controller/bone/skinning (292 lines)
- `ColladaModelRenderer.Nodes.cs` - Visual scene hierarchy (239 lines)
- `ColladaModelRenderer.Utilities.cs` - String formatting helpers (51 lines)

## USD Export - Active Development

### Shader-Based Material System (IN PROGRESS)

**Goal**: Implement shader definition parsing to accurately interpret material properties based on `StringGenMask` flags, enabling proper material node graphs in USD format.

**Shader File Format (.ext)**:
- Location: `<objectdir>/Shaders/<ShaderName>.ext` (e.g., `d:\depot\mwo\Shaders\MechCockpit.ext`)
- Text-based property definitions with Name, Mask, Description, Dependencies
- 40 shader files total in MWO, 17 use `UsesCommonGlobalFlags`
- Property blocks define how texture channels map to material inputs

**UsesCommonGlobalFlags**: Metadata directive indicating the shader uses standard material properties (diffuse, specular, environment maps, etc.). Not an inheritance mechanism - each .ext file contains complete property definitions. Shaders without this flag are typically post-processing or special effects that don't need material properties. **Decision**: Ignore this directive; parse complete property list from each shader file.

**Example StringGenMask Parsing**:
- Material has `Shader="mechcockpit"` and `StringGenMask="%ALPHAGLOW%ENVIRONMENT_MAP%GLOSS_MAP%SPECULARPOW_GLOSSALPHA%VERTCOLORS"`
- Parser splits flags and looks up definitions in MechCockpit.ext:
  - `%ALPHAGLOW` (Mask 0x2000): "Use alpha channel of diffuse texture for glow" → Connect diffuse.alpha to emissiveColor
  - `%SPECULARPOW_GLOSSALPHA` (Mask 0x800): "Use specular map alpha channel as gloss map" → Connect specular.alpha to roughness
  - `%ENVIRONMENT_MAP` (Mask 0x80): "Use environment map as separate texture" → Enable environment mapping
  - `%GLOSS_MAP` (Mask 0x10): "Use gloss map as separate texture" → Enable gloss texture
  - `%VERTCOLORS` (Mask 0x400000): "Use vertex colors" → Enable vertex color attribute

**Implementation Plan**:

1. **Data Models** (`CgfConverter/Models/Shaders/`)
   - `ShaderProperty.cs` - Single property from .ext (Name, Mask, Description, Dependencies)
   - `ShaderDefinition.cs` - Complete shader (ShaderName, Properties dictionary)
   - `MaterialRule.cs` - Applied rule (PropertyName, TextureSlot, TargetChannel, OutputTarget)

2. **Parser** (`CgfConverter/Parsers/ShaderExtParser.cs`)
   - Parse .ext text format property blocks
   - Build `ShaderDefinition` objects with property dictionaries
   - Cache parsed shaders for reuse

3. **Rules Engine** (`CgfConverter/Renderers/USD/ShaderRulesEngine.cs`)
   - Takes Material + ShaderDefinition
   - Parses StringGenMask into flags (split by '%')
   - Generates MaterialRule list to apply
   - Handles channel routing (e.g., diffuse.alpha → emissive vs opacity)

4. **UsdRenderer Refactoring** (Partial Classes - following GltfRenderer pattern)
   - `UsdRenderer.cs` - Main class, orchestration (Render, GenerateUsdObject, WriteUsdToFile)
   - `UsdRenderer.Materials.cs` - Material/shader creation (CreateMaterials, CreateShaders, shader loading)
   - `UsdRenderer.Geometry.cs` - Mesh creation (CreateMeshPrim, CreateNodeHierarchy, CreateNode)
   - `UsdRenderer.Skeleton.cs` - Skeletal animation (CreateSkeleton, AddSkinningAttributes, etc.)
   - Shared state via private fields accessible to all partials

5. **Texture Support Extensions**
   - Specular textures: Connect to `inputs:specularColor` or create channel router nodes
   - Bumpmap textures: Connect to normal input (may need UsdNormalMap node)
   - Environment textures: Skip (USD PreviewSurface doesn't support cubemaps)
   - Detail textures: Handle with blending where applicable
   - Unknown shader properties: Log at Debug level, continue processing

6. **Material Creation Flow** (in `UsdRenderer.Materials.cs`)
   ```
   Startup:
     - Load all .ext files from <objectdir>/Shaders
     - Parse and cache ShaderDefinition objects

   Per Material:
     1. Look up ShaderDefinition from material.Shader property (case-insensitive)
     2. Parse StringGenMask into active flags
     3. Use ShaderRulesEngine to generate MaterialRules
     4. Create texture shader nodes for all supported types (Diffuse, Specular, Bumpmap)
     5. Apply MaterialRules to configure connections (channel routing, alpha handling)
     6. Create PrincipledBSDF with all connections based on rules
   ```

**Test Case**: Adder cockpit (`d:\depot\mwo\objects\mechs\cockpit_standard\adder_a_cockpit_standard.cga`)
- Material file: `d:\depot\mwo\objects\mechs\adder\cockpit_standard\adder_a_cockpit_standard.mtl`
- "cockpit_shared" material uses MechCockpit shader with %ALPHAGLOW flag
- Verify diffuse.alpha connects to emissiveColor (not opacity) in exported USD

**Current Status**: Planning complete, ready to implement data models and parser.

### Future Material Improvements
- **Metallic detection heuristic**: Investigate using `Shader`, `MtlFlags`, or specular properties to auto-detect metallic materials
  - Check if `Shader="Metal"` exists in any materials
  - Consider using `Glossiness` property (currently unused)
  - Analyze specular color patterns (colored specular might indicate metallic)
- **Material validation**: Ensure all MTL properties map correctly to USD PBR workflow

### Animation Support (IMPLEMENTED for .dba, TODO for .caf)

**Current Status**: USD animation export implemented for `.dba` animation databases via `UsdRenderer.Animation.cs`.

**How it works**:
- Animations loaded from `.chrparams` file's `$TracksDatabase` entry pointing to a `.dba` file
- Animation data stored as USD `SkelAnimation` prims with time-sampled translations/rotations
- First animation automatically bound as skeleton's `skel:animationSource`

**TODO - Individual .caf file support**:
- `.caf` files are individual animation clips (vs `.dba` which is a database of many animations)
- `.chrparams` lists individual `.caf` files with animation names (e.g., `<Animation name="walk" path="walk.caf"/>`)
- Currently only `$TracksDatabase`/`#filepath` entries are processed
- Need to extend `CryEngine.CreateAnimations()` to also load individual `.caf` files
- `.caf` files use the same `ChunkController` chunk format as `.dba`

### Known Issues
- ~~**GeomSubset indices**: "invalid indices" warning in Blender~~ - FIXED: Convert vertex indices to face indices for elementType="face"
- ~~**Normal count mismatch**: "Loop normal count mismatch" warning~~ - FIXED: Expand normals array to match faceVertexIndices for faceVarying interpolation
- ~~**Ivo format file size explosion**: 700MB+ output files~~ - FIXED: Extract only per-subset vertices and remap indices (same fix as Collada/glTF renderers)
- ~~**Skeleton infinite recursion**: Stack overflow in BuildJointPaths~~ - FIXED: Added cycle detection to skip already-processed bones
- **Node transforms incorrect**: Child nodes not positioned correctly in complex models (e.g., Avenger spaceship). Currently using `node.Transform` directly. **Attempted fixes that didn't work:**
  - `node.LocalTransform` (full transpose): Translations lost, all objects at origin
  - Transpose 3x3 rotation only + move translation to row 4: Translations wrong
  - **Priority: Fix before armature/skinning work.**

- **USD Skinning: Bone-to-vertex mapping incorrect for Star Citizen .skin files**: When exporting `aloprat.skin` to USD, the armature imports correctly but bones influence wrong vertices. Examples observed:
  - `R_UpperArm` bone influences vertices in the head
  - `Tail1` bone influences back right toe vertices
  - This suggests the joint indices in `skel:jointIndices` don't match the joint order in the skeleton's `joints` array
  - **Likely causes to investigate:**
    1. Joint index remapping issue: The bone indices stored in `ChunkCompiledIntSkinVertices` may use a different ordering than the skeleton joint list
    2. Ext2Int mapping: Star Citizen uses `ChunkCompiledExtToIntMap` to map external vertex indices to internal - this mapping may need to be applied to skinning weights too
    3. Bone hash vs bone index: CAF animations use bone hashes; skinning may need similar hash-based lookup instead of direct indices
  - **Files to investigate:** `UsdRenderer.Skeleton.cs` (AddSkinningAttributes), `SkinningInfo.cs`, `ChunkCompiledIntSkinVertices`
  - **Test file:** `d:\depot\SC4.1\Data\Objects\Characters\Creatures\aloprat\aloprat.skin`

### Multiple UV Layer Support (IN PROGRESS)

**Status**: USD renderer infrastructure complete. Parsing not yet implemented.

**USD Support**: Fully supported via primvars. Each UV set is a separate named primvar:
- Primary UV: `primvars:{nodeName}_UV` (texCoord2f[])
- Secondary UV: `primvars:{nodeName}_UV2` (texCoord2f[])
- Shaders reference UV sets via `UsdPrimvarReader_float2` with `varname` input

**Implementation**:
- `GeometryInfo.UVs2` property added for second UV layer
- `UsdRenderer.Geometry.cs` outputs `_UV2` primvar when `UVs2` is populated
- Collada multi-UV support: TODO (uses multiple `<source>` elements with `TEXCOORD` semantic and `set` indices)

**CryEngine Data Sources** (research needed):
- Vertex format `eVF_P3S_C4B_T2S_T2S` exists in `Enums.cs` with comment "For UV2 support"
- Vertex format `eVF_P3F_C4B_T2F_T2F` also supports dual texture coordinates
- These formats are defined but **not yet parsed** in `ChunkDataStream`
- Need to identify which games/assets actually use dual-UV vertex formats
- Parsing would occur in `ChunkDataStream_800.cs` / `ChunkDataStream_801.cs` in the `VERTSUVS` case

**Next Steps**:
1. Find sample assets that use dual-UV vertex formats
2. Extend `ChunkDataStream` parsing to extract second UV set
3. Populate `GeometryInfo.UVs2` during geometry aggregation
4. Test with USD export to verify primvar output

## Collada Animation Export - Blender Compatibility (FIXED)

### Problem
Animations exported to Collada were not importing into Blender. Blender reported "removed X unused curves".

### Root Causes (Two Issues)

1. **Wrong SID**: Joint nodes used `sid="matrix"` instead of `sid="transform"`
2. **Wrong element order**: Animation XML had `<channel>` before `<source>` elements

Blender's Collada importer expects:
- Joint nodes with `<matrix sid="transform">` (not decomposed transforms or `sid="matrix"`)
- Animation channels targeting `BoneName/transform`
- Animation elements in order: `source`, `sampler`, `channel` (not `channel` first)

### Solution Implemented
Changed joint nodes and animations to use matrix-based transforms with SID `transform`:

**Joint nodes** (`ColladaModelRenderer.Skeleton.cs`):
```xml
<node id="Bip01" name="Bip01" sid="Bip01" type="JOINT">
  <matrix sid="transform">...</matrix>
</node>
```

**Animation channels** (`ColladaModelRenderer.Animation.cs`):
```xml
<source id="anim_Bip01_output">
  <float_array count="...">/* 16 floats per keyframe */</float_array>
  <accessor stride="16"><param type="float4x4"/></accessor>
</source>
<channel source="#sampler" target="Bip01/transform"/>
```

### Key Technical Details
- Animation output uses `float4x4` matrices (16 values per keyframe)
- Channel targets use `/transform` suffix to match joint's matrix SID
- Removed `library_animation_clips` (Blender doesn't require it)
- Matrices built from position (Vector3) + rotation (Quaternion) at each keyframe
- **Element order fix** in `ColladaAnimation.cs`: Reordered property declarations so XML serializer outputs `source`, `sampler`, `channel` in correct Collada spec order

### Mechanic.chr bone matrices for Bip01, Bip01_Pelvis, Bip01_L_Thigh.  

In Cryengine Matrix3x4 format, where column 4 is translation.  These are row major form.  This information should
be all that is needed to recreate the restTransforms and bindTransforms for the skeleton.  Z up, Y forward.

- Bip01
struct MATRIX3x4 worldToBone		32DCh	30h	Fg: Bg:0x000080	[[-0.000000, 1.000000, 0.000000, -0.000000] [-1.000000, -0.000000, -0.000000, -0.000000] [-0.000000, -0.000000, 1.000000, -0.000000]]
struct MATRIX3x4 boneToWorld		330Ch	30h	Fg: Bg:0x000080	[[-0.000000, -1.000000, -0.000000, -0.000000] [1.000000, -0.000000, -0.000000, 0.000000] [0.000000, -0.000000, 1.000000, 0.000000]]

- Bip01_Pelvis
struct MATRIX3x4 worldToBone		3524h	30h	Fg: Bg:0x000080	[[0.000000, 0.000000, 1.000000, -0.950611] [-0.000003, 1.000000, -0.000000, -0.000000] [-1.000000, -0.000003, 0.000000, -0.000000]]
struct MATRIX3x4 boneToWorld		3554h	30h	Fg: Bg:0x000080	[[0.000000, -0.000003, -1.000000, 0.000000] [0.000000, 1.000000, -0.000003, 0.000000] [1.000000, -0.000000, 0.000000, 0.950611]]

- Bip01_L_Thigh
struct MATRIX3x4 worldToBone		376Ch	30h	Fg: Bg:0x000080	[[-0.141242, -0.078662, -0.986845, 0.920858] [-0.011145, 0.996901, -0.077868, 0.072651] [0.989912, 0.000000, -0.141681, 0.232642]]
struct MATRIX3x4 boneToWorld		379Ch	30h	Fg: Bg:0x000080	[[-0.141242, -0.011145, 0.989912, -0.099421] [-0.078662, 0.996901, 0.000000, 0.000010] [-0.986845, -0.077868, -0.141681, 0.947363]]

