# Development Notes

Active development notes, debugging history, and in-progress feature work for Cryengine Converter.

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

4. **UsdRenderer Integration** (in `UsdRenderer.Materials.cs`)
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

5. **Texture Support Extensions**
   - Specular textures: Connect to `inputs:specularColor` or create channel router nodes
   - Bumpmap textures: Connect to normal input (may need UsdNormalMap node)
   - Environment textures: Skip (USD PreviewSurface doesn't support cubemaps)
   - Detail textures: Handle with blending where applicable
   - Unknown shader properties: Log at Debug level, continue processing

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

### Animation Support

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

#### Fixed Issues
- ~~**GeomSubset indices**: "invalid indices" warning in Blender~~ - FIXED: Convert vertex indices to face indices for elementType="face"
- ~~**Normal count mismatch**: "Loop normal count mismatch" warning~~ - FIXED: Expand normals array to match faceVertexIndices for faceVarying interpolation
- ~~**Ivo format file size explosion**: 700MB+ output files~~ - FIXED: Extract only per-subset vertices and remap indices (same fix as Collada/glTF renderers)
- ~~**Skeleton infinite recursion**: Stack overflow in BuildJointPaths~~ - FIXED: Added cycle detection to skip already-processed bones
- ~~**USD Skinning bone-to-vertex mapping incorrect for Star Citizen .skin files**~~ - FIXED: `BoneIndex` values in skinning data are indices into `CompiledBones` array order, but USD's `skel:jointIndices` expects indices into the `joints` array built from `jointPaths`. Since `BuildJointPaths()` walks the bone hierarchy depth-first, it produces different ordering than `CompiledBones`. Fix adds `_compiledBoneIndexToJointIndex` mapping in `CreateSkeleton()` and uses it in `AddSkinningAttributes()`.

#### Open Issues
- **Node transforms incorrect**: Child nodes not positioned correctly in complex models (e.g., Avenger spaceship). Currently using `node.Transform` directly.
  - Attempted: `node.LocalTransform` (full transpose) - Translations lost, all objects at origin
  - Attempted: Transpose 3x3 rotation only + move translation to row 4 - Translations wrong
  - **Priority**: Fix before armature/skinning work on complex multi-node models

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

---

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

---

## Reference Data

### Mechanic.chr Bone Matrices (for skeleton debugging)

Cryengine Matrix3x4 format, column 4 is translation, row major. Z up, Y forward.

**Bip01**:
```
worldToBone: [[-0.000000, 1.000000, 0.000000, -0.000000] [-1.000000, -0.000000, -0.000000, -0.000000] [-0.000000, -0.000000, 1.000000, -0.000000]]
boneToWorld: [[-0.000000, -1.000000, -0.000000, -0.000000] [1.000000, -0.000000, -0.000000, 0.000000] [0.000000, -0.000000, 1.000000, 0.000000]]
```

**Bip01_Pelvis**:
```
worldToBone: [[0.000000, 0.000000, 1.000000, -0.950611] [-0.000003, 1.000000, -0.000000, -0.000000] [-1.000000, -0.000003, 0.000000, -0.000000]]
boneToWorld: [[0.000000, -0.000003, -1.000000, 0.000000] [0.000000, 1.000000, -0.000003, 0.000000] [1.000000, -0.000000, 0.000000, 0.950611]]
```

**Bip01_L_Thigh**:
```
worldToBone: [[-0.141242, -0.078662, -0.986845, 0.920858] [-0.011145, 0.996901, -0.077868, 0.072651] [0.989912, 0.000000, -0.141681, 0.232642]]
boneToWorld: [[-0.141242, -0.011145, 0.989912, -0.099421] [-0.078662, 0.996901, 0.000000, 0.000010] [-0.986845, -0.077868, -0.141681, 0.947363]]
```

Use this data to verify restTransforms and bindTransforms calculations for skeleton export.
