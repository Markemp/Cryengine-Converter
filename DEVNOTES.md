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

**Current Status**: USD animation export fully working for MWO, Armored Warfare, and ArcheAge. Both `.dba` (animation databases) and `.caf` (individual clips) are supported.

#### What's Working
- **MWO mech animations**: All power up/down animations from `.dba` files import perfectly into Blender
- **MWO pilot.chr**: Full skeleton with all CAF animations (joystick, throttle, etc.) working correctly
- **Armored Warfare**: CAF animations via ChunkController_829 fully working (chicken walk/idle)
- **ArcheAge**: CAF animations fully working (chicken uses ChunkCompiledBones_801 + .cal file)
- **Kingdom Come Deliverance 2 (KCD2)**: DBA animations working (pig skeleton uses ChunkController_905 in-place streaming mode)
- Animations exported as separate `.usda` files for Blender NLA workflow (one file per animation)
- First animation automatically bound as skeleton's `skel:animationSource`

#### What Needs Testing/Fixing
- **Star Citizen #ivo animations**: Next target for animation support (see section below)

#### Animation File Formats

**DBA Files** (Animation Databases):
- Container format holding multiple animations
- Referenced via `.chrparams` file's `$TracksDatabase` entry (supports wildcards like `*.dba`)
- Parsed by `ChunkController_905` which contains:
  - `NumAnims` animations, each with `MotionParams905` metadata
  - Compressed keyframe data (positions, rotations, times)
  - Per-animation `AssetFlags` in `MotionParams905.AssetFlags`
- **Two storage modes** detected via offset sign:
  - **Standard mode** (positive offsets): Track data → Animation entries → Controllers
  - **In-place streaming mode** (negative offsets): Padding → Animation entries → Controllers → Track data
  - In-place mode: offsets are relative to END of track data block (add negative offset to trackDataEnd)
  - 4-byte alignment required before track data in in-place mode

**CAF Files** (Individual Animation Clips):
- Single animation per file
- Referenced via `.chrparams` `<Animation name="..." path="..."/>` entries
- Support wildcard patterns (e.g., `animations/pilot/*.caf`)
- Animation metadata in `ChunkGlobalAnimationHeaderCAF` chunk (version 971)
- Controller data in `ChunkController_829`, `ChunkController_830`, `ChunkController_831`
- Star Citizen uses `ChunkIvoCAF` for #ivo format animations

#### Animation Asset Flags

Defined in `ChunkController_905.AssetFlags` (also applies to CAF via `ChunkGlobalAnimationHeaderCAF.Flags`):

| Flag | Value | Meaning |
|------|-------|---------|
| `Additive` | 0x001 | Animation stores deltas from rest pose, not absolute transforms |
| `Cycle` | 0x002 | Animation is meant to loop (not used in export - Blender controls this) |
| `Loaded` | 0x004 | Runtime flag |
| `Lmg` | 0x008 | Locomotion group |
| `LmgValid` | 0x020 | Locomotion group valid |
| `Created` | 0x800 | Runtime flag |
| `Aimpose` | 0x4000 | Aim pose animation |
| `BigEndian` | 0x80000000 | File is big-endian |

#### Additive Animation Handling (FIXED)

**Problem**: Additive animations (like `additive_pilot_joystick_down`) store bone transforms as *deltas* from the rest pose. When exported directly to USD, the skeleton collapses because translations are near-zero and rotations are near-identity.

**Identification**:
- CAF files: Check `ChunkGlobalAnimationHeaderCAF.Flags & 0x001`
- DBA files: Check `MotionParams905.AssetFlags.Additive`
- Naming convention: Often prefixed with `additive_`

**Solution** (implemented in `UsdRenderer.Animation.cs`):
```csharp
if (isAdditive)
{
    // Convert additive deltas to absolute transforms
    absolute_rotation = restRotation * additiveRotation;
    absolute_translation = restTranslation + additiveTranslation;
}
```

The `BuildRestRotationMapping()` and `BuildRestTranslationMapping()` methods extract rest pose from the skeleton's bind matrices for this conversion.

#### Key Implementation Files

- `CgfConverter/CryEngine/CryEngine.cs`: `LoadCafAnimations()`, `ParseCafModel()` - loads CAF files from chrparams
- `CgfConverter/Models/CafAnimation.cs`: Animation data model with `IsAdditive` property
- `CgfConverter/CryEngineCore/Chunks/ChunkController_905.cs`: DBA animation parsing, `AssetFlags` enum
- `CgfConverter/CryEngineCore/Chunks/ChunkGlobalAnimationHeaderCAF_971.cs`: CAF header with flags
- `CgfConverter/Renderers/USD/UsdRenderer.Animation.cs`: USD export, additive conversion, per-animation file export

#### Blender Import Notes

- USD SkelAnimation has no native looping property - cycle behavior controlled in Blender's NLA editor
- Each animation exported as separate `.usda` file (e.g., `model_anim_walk.usda`) for NLA workflow
- Blender only reads the single bound animation per file, hence separate files needed

---

## Animation Data Flow: CryEngine → USD (Reference for New Chunk Support)

This section documents the vetted animation pipeline to help implement support for new CompiledBones and Controller chunk versions.

### Vetted Chunks (Known Working)

| Chunk Type | Version | Game | Status |
|------------|---------|------|--------|
| `ChunkCompiledBones` | 0x800 | MWO, KCD2 | ✅ Vetted |
| `ChunkCompiledBones` | 0x801 | ArcheAge | ✅ Vetted |
| `ChunkController` | 0x905 | MWO (DBA), KCD2 (DBA in-place streaming) | ✅ Vetted |
| `ChunkController` | 0x829 | Armored Warfare (CAF) | ✅ Vetted |
| `ChunkCompiledBones` | 0x900, 0x901 | Unknown | ⚠️ Untested for animations |

### CryEngine Matrix Convention

**CRITICAL**: CryEngine stores translation in **column 4** (M14, M24, M34), not row 4.

```
CryEngine Matrix3x4 layout (row-major storage):
┌─────────────────────────────────┐
│ M11  M12  M13  M14 (trans X)    │  Row 1
│ M21  M22  M23  M24 (trans Y)    │  Row 2
│ M31  M32  M33  M34 (trans Z)    │  Row 3
└─────────────────────────────────┘
         ↑
    Column 4 = Translation
```

When converted to `System.Numerics.Matrix4x4` via `Matrix3x4.ConvertToTransformMatrix()`:
- Translation stays in M14, M24, M34
- M41, M42, M43 are set to 0
- M44 is set to 1

**WARNING**: `Matrix4x4.Translation` property reads M41, M42, M43 (row 4) - this returns ZEROS for CryEngine matrices! Always extract translation manually:
```csharp
var translation = new Vector3(matrix.M14, matrix.M24, matrix.M34);  // Correct
var translation = matrix.Translation;  // WRONG - returns zeros!
```

### ChunkCompiledBones_800 (Vetted)

**Data per bone** (584 bytes):
- `ControllerID` (uint32): CRC32 hash for animation binding
- `PhysicsGeometry[2]`: Physics data (live/ragdoll)
- `Mass` (float)
- `LocalTransformMatrix` (Matrix3x4): Local bone transform, used directly as BindPoseMatrix
- `WorldTransformMatrix` (Matrix3x4): World space transform
- `BoneName` (256 chars)
- `LimbId`, `OffsetParent`, `NumberOfChildren`, `OffsetChild`

**BindPoseMatrix calculation**:
```csharp
LocalTransformMatrix = b.ReadMatrix3x4();
BindPoseMatrix = LocalTransformMatrix.ConvertToTransformMatrix();
```

### ChunkController_905 (Vetted - DBA Animations)

**Structure**:
- Header: `NumKeyPos`, `NumKeyRot`, `NumKeyTime`, `NumAnims`
- Track data: Compressed keyframes (positions as Vector3, rotations as Quaternion)
- Per-animation: `MotionParams905` + `CControllerInfo[]`

**Key fields in CControllerInfo**:
- `ControllerID`: CRC32 of bone name for skeleton binding
- `PosKeyTimeTrack`, `PosTrack`: Indices into KeyTimes/KeyPositions arrays
- `RotKeyTimeTrack`, `RotTrack`: Indices into KeyTimes/KeyRotations arrays

**Position data**: Stored as **absolute local transforms**, not deltas from rest pose.

### USD SkelAnimation Requirements

USD `SkelAnimation` needs these attributes for Blender import:

```usda
def SkelAnimation "AnimationName"
{
    uniform token[] joints = ["Bip01", "Bip01/Pelvis", ...]  # Joint paths

    float3[] translations.timeSamples = {
        0: [(x,y,z), (x,y,z), ...],  # One Vector3 per joint per frame
        1: [...],
    }

    quatf[] rotations.timeSamples = {
        0: [(w,x,y,z), (w,x,y,z), ...],  # One Quaternion per joint per frame
        1: [...],
    }

    half3[] scales.timeSamples = {
        0: [(1,1,1), (1,1,1), ...],  # Usually uniform scale
        1: [...],
    }
}
```

**Joint path format**: Hierarchical with `/` separator (e.g., `Bip01/bip_01_Pelvis/bip_01_Spine`)

### Translation Handling (The Critical Fix)

**For bones WITH position animation**: Use animation data directly (it's absolute local position)

**For bones WITHOUT position animation**: Use rest pose translation from skeleton

```csharp
Vector3 position = SamplePosition(track, frame, restTranslation);
// If track has position data → returns interpolated animation position
// If track has NO position data → returns restTranslation
```

**Rest translation extraction** (from `BuildRestTranslationMapping()`):
```csharp
// Compute local transform relative to parent
if (bone.ParentBone == null)
{
    // Root: invert BindPoseMatrix to get boneToWorld
    Matrix4x4.Invert(bone.BindPoseMatrix, out var boneToWorld);
    restMatrix = boneToWorld;
}
else
{
    // Child: localTransform = parentWorldToBone * childBoneToWorld
    Matrix4x4.Invert(bone.BindPoseMatrix, out var childBoneToWorld);
    restMatrix = bone.ParentBone.BindPoseMatrix * childBoneToWorld;
}

// Extract translation from column 4 (NOT .Translation property!)
var translation = new Vector3(restMatrix.M14, restMatrix.M24, restMatrix.M34);
```

### Adding Support for New Chunk Versions

When implementing a new `ChunkCompiledBones_XXX`:

1. **Identify how BindPoseMatrix is stored/computed**
   - Is it stored directly as Matrix3x4?
   - Is it computed from quaternion + translation?
   - Where is translation stored (column 4 or row 4)?

2. **Ensure translation ends up in M14, M24, M34**
   - Use `Matrix3x4.ConvertToTransformMatrix()` if reading Matrix3x4
   - If building from quat+translation: `m.M14 = t.X; m.M24 = t.Y; m.M34 = t.Z;`

3. **Test with non-additive animation first** (simpler code path)

4. **Verify rest translations are non-zero** for child bones

When implementing a new `ChunkController_XXX`:

1. **Identify position data format**
   - Absolute local transforms? (like 905) → Use directly
   - Deltas from rest pose? → Add to rest translation

2. **Identify key time format**
   - Float frames? UInt16? Byte?
   - Separate times for position vs rotation?

3. **Map ControllerID to skeleton bones**
   - Usually CRC32 of bone name
   - Check case sensitivity (some games use lowercase)

### Debugging Animation Issues

**Symptom: All bones collapse to one point**
- Check rest translations - are they all zeros?
- Likely cause: extracting translation from wrong matrix elements

**Symptom: Animation floats/offset from rest pose**
- Check if position data is being treated as delta when it's absolute (or vice versa)
- Compare animation position values to rest translation values

**Symptom: Bones don't match between skeleton and animation**
- Check ControllerID matching (CRC32 hash)
- Check case sensitivity of bone names in hash
- Log which controllers couldn't find matching bones

### Star Citizen #ivo Animation Format (IN PROGRESS)

**Status**: Rotation animation parsing implemented and working. Position animation skipped (compression not decoded). Armature still has issues - translations correct in edit mode but wrong in pose/object mode.

**Test Asset**: `aloprat_skel.chr` with 13 CAF animations from `aloprat/*.caf`

#### What's Implemented

**ChunkIvoCAF_900** (`CgfConverter/CryEngineCore/Chunks/ChunkIvoCAF_900.cs`):
- Parses `#caf` signature block (magic 0xAA55 for DBA, 0xFFFF for CAF)
- Reads bone hash array (CRC32 identifiers for each bone)
- Reads controller entries (24 bytes each): rotation track (12 bytes) + position track (12 bytes)
- Parses rotation keyframe data (supports formats 0x40, 0x42, 0x43)
- Parses rotation time keys (uint16 normalized to frame indices)

**Controller Entry Structure** (per 010 template `Animation.bt`):
```
// 24 bytes per bone
struct AnimControllerEntry_Ivo {
    // Rotation track (12 bytes)
    uint16 numRotKeys;        // Number of rotation keyframes
    uint16 rotFormatFlags;    // e.g., 0x8042
    uint32 rotTimeOffset;     // Offset from controller start to time data
    uint32 rotDataOffset;     // Offset from controller start to rotation data

    // Position track (12 bytes)
    uint16 numPosKeys;        // Meaning varies by format - NOT always a count
    uint16 posFormatFlags;    // e.g., 0xC040, 0xC142, 0xC242
    uint32 posTimeOffset;     // Offset from controller start to time data
    uint32 posDataOffset;     // Offset from controller start to position data
};
```

**IMPORTANT**: All offsets are relative to the **start of each controller**, not the start of the controllers array.

**Rotation Format Flags** (0x80xx) - determines time structure:
| Flag | Time Format | Data Format |
|------|-------------|-------------|
| `0x8040` | ubyte array[numRotKeys] (padded to 4 bytes) | numRotKeys × Quaternion (16 bytes each) |
| `0x8042` | 8-byte header (uint16 × 2 + uint32 marker) | numRotKeys × Quaternion (16 bytes each) |

**Rotation Time Header** (for 0x8042):
- 8-byte header: `uint16 startTime`, `uint16 endTime`, `uint32 marker`
- Followed by `numRotKeys` quaternions (16 bytes each, uncompressed)

**Position Format Flags** (0xC0xx) - determines structure:
| Flag | Time Format | Data Format | numPosKeys Meaning |
|------|-------------|-------------|-------------------|
| `0xC040` | ubyte array[numPosKeys] | numPosKeys × Vector3, no header | Actual position count |
| `0xC142` | 8-byte header (uint16 × 2 + uint32 marker) | 2 × Vector3, no header | Unknown (not count) |
| `0xC242` | 8-byte header (uint16 × 2 + uint32 marker) | 8-byte header + 1 × Vector3 | Unknown (not count) |

**Position Time Header** (for 0xC142, 0xC242):
```
struct PosTimeBlock {
    uint16 timeStart;    // Usually 0
    uint16 timeEnd;      // Usually 30 (0x1E)
    uint32 marker;       // e.g., 0x65294A55
};
```

**Position Data Header** (for 0xC242 only):
- 8 bytes including a NaN marker (0x7F7FFFFF pattern observed)
- Followed by 1 Vector3

**Format Flags Breakdown** (Unvetted - Under Investigation):

Flag Structure: `0xABCD` (16-bit)

| Nibble | Position | Values Observed | Meaning |
|--------|----------|-----------------|---------|
| A | 1st | 8, C | Data type: 8 = Quaternion (rotation), C = Vec3 (position) |
| B | 2nd | 0, 1, 2 | Compression/format variant (see below) |
| C | 3rd | 4 (always) | Likely indicates uncompressed/raw data |
| D | 4th | 0, 2 | Time format: 0 = ubyte, 2 = ushort |

**Rotation Flags (Nibble A = 8)**:

| Flag | Time Format | Data Format |
|------|-------------|-------------|
| 0x8040 | ubyte | Uncompressed Quat (4 floats, 16 bytes/key) |
| 0x8042 | ushort | Uncompressed Quat (4 floats, 16 bytes/key) |

**Position Flags (Nibble A = C)**:

| Flag | Time Format | Data Format | Notes |
|------|-------------|-------------|-------|
| 0xC040 | ubyte | Full Vec3 (3 floats, 12 bytes/key) | Standard uncompressed |
| 0xC042 | ushort | Full Vec3 (3 floats, 12 bytes/key) | Standard uncompressed |
| 0xC140 | ubyte | Unknown variant | Possibly CryHalf3? |
| 0xC142 | ushort | Unknown variant | Possibly CryHalf3? |
| 0xC240 | ubyte | Compressed - possibly SNORM16×2 + shared Z | See notes below |
| 0xC242 | ushort | Compressed - base Vec3 + delta indices? | See notes below |

**2nd Nibble Hypothesis (Position Compression Level)**:

| Value | Likely Meaning |
|-------|----------------|
| 0 | Full precision (3 × float32 = 12 bytes/key) |
| 1 | Half precision (3 × float16 = 6-8 bytes/key)? |
| 2 | Delta/indexed compression (base Vec3 + uint16 offsets) |

**0xC240 Format (Observed)**:

For a 3-key static position:
- `[SNORM16 x][SNORM16 y]` × 3 keys = 12 bytes
- `[float32 Z or scale]` = 4 bytes
- `0xFFFF` as SNORM16 ≈ 0.0
- `0x7F7F` as SNORM16 ≈ 0.996 (near 1.0)

**0xC242 Format (Under Investigation)**:

For 14 keys, data structure appears to be:
- `[4 bytes header?]` 00 00 1E 00
- `[unknown 8 bytes]`
- `[Base Vec3]` 3 floats (12 bytes)
- `[possible 4th float]`
- `[14 × uint16 delta/index values]`

The repeating pattern `03 02 01 00 06 02 00 00` suggests indexed or delta-encoded positions referencing the base Vec3.

**Reference: Lumberyard SNORM16 Packing** (from `VertexFormats.h` PackingSNorm namespace):
```cpp
int16 tPackF2B(float f) { return (int16)(f * 32767.0f); }
float tPackB2F(int16 i) { return (float)i / 32767.0f; }
```
- `0x7FFF` (32767) = +1.0
- `0x8001` (-32767) = -1.0
- `0x0000` = 0.0

**Reference: Lumberyard Time Formats** (`EKeyTimesFormat`):

| Value | Name | Description |
|-------|------|-------------|
| 0 | eF32 | 32-bit float |
| 1 | eUINT16 | 16-bit unsigned |
| 2 | eByte | 8-bit unsigned |
| 6 | eBitset | Bitset encoded |

Note: Star Citizen may have remapped these values (0=byte, 2=ushort observed)

**Lumberyard Reference Files**:
- `Code/CryEngine/CryCommon/CryCharAnimationParams.h` - Animation flag enums
- `Code/CryEngine/CryCommon/CryHalf.inl` - CryHalf implementation
- `Code/CryEngine/CryCommon/VertexFormats.h` - SNORM packing (PackingSNorm)
- `Code/Tools/RC/ResourceCompilerPC/CGA/ControllerPQ.h` - Controller formats
- `Gems/CryLegacy/Code/Source/CryAnimation/ControllerOpt.h` - Runtime controller

**DONE**: Warnings are now logged in `ChunkIvoCAF_900.cs` when encountering format flags not in the known set (0x8040/0x8042 for rotation, 0xC040/0xC142/0xC242 for position).

#### Current Issues

**Position data not usable**:
- Format `0x40` positions appear to be root motion direction vectors (unit length ~1.0), not bone translations
- Format `0x42` positions use SmallTree compression (6 bytes) which we can't decode as 12-byte Vector3
- Reading compressed data as uncompressed produces garbage that looks valid (e.g., `(0.619, 0.778, -0.178)`)
- **Current workaround**: Skip all CAF position tracks, use rest translations from skeleton

**Armature display issue**:
- Edit mode: Armature looks correct (restTransforms working)
- Pose/Object mode: Armature positions are wrong (animation translations issue)
- Translations in USD are all rest pose values (constant across frames) since we skip position tracks
- Rotations are being read and vary per frame
- Issue may be in rotation data interpretation or bone ordering

**Observed symptoms in Blender** (walk animation):
- World bone at (0,0,0) - correct
- Lower leg/tail/foot bones clustered near origin with small offsets
- Spine2 significantly offset (was reading garbage before position skip fix)
- Spine3 ~1m off with another cluster (arms, head, etc.)
- Animation plays but skeleton structure is wrong

#### Research Needed

1. **SmallTree position compression**: The 6-byte position format (0x42) needs reverse engineering
   - May be similar to SmallTree48BitQuat but for Vector3
   - Could be quantized/normalized positions requiring scale factor

2. **Root motion data**: Format 0x40 positions look like direction vectors
   - Need to understand what this data represents
   - May need to extract root motion separately from bone animation

3. **Rotation data validation**: Even with position fix, armature is wrong
   - Verify quaternion decoding for all formats
   - Check if rotation data is local or world space
   - Verify bone hash to joint path mapping is correct

4. **Bone ordering**: Animation joints array order may matter
   - Currently built from `cafAnim.BoneTracks` dictionary iteration order
   - May need to match skeleton's joint order

#### Key Files

- `CgfConverter/CryEngineCore/Chunks/ChunkIvoCAF_900.cs` - Main #ivo CAF parser
- `CgfConverter/CryEngineCore/Chunks/ChunkIvoCAF.cs` - Base class with dictionaries for rotation/position data
- `CgfConverter/Models/Structs/IvoAnimationStructs.cs` - Controller entry struct
- `CgfConverter/Renderers/USD/UsdRenderer.Animation.cs` - USD export (CreateSkelAnimationFromCaf)
- `010-Templates/chunks/Animation.bt` - 010 Editor template with format documentation

#### Test Commands

```bash
# Run aloprat CAF animation test
dotnet test --filter "FullyQualifiedName~Aloprat_Skel_USD_WithCAF"

# Output files generated at:
# D:\depot\SC4.1\Data\Objects\Characters\Creatures\aloprat\aloprat_skel_anim_*.usda
```

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

- **glTF: ArcheAge models skeleton/geometry incorrect**: ArcheAge uses `ChunkCompiledBones_801` which stores B2W (boneToWorld) matrix and computes BindPoseMatrix (W2B) by inversion. The glTF bone hierarchy building produces incorrect results - skeleton and geometry are malformed in Blender. USD export works correctly for the same files.
  - Tested with: `archeage/game/objects/characters/animals/chicken/chicken.chr`
  - Multiple fix attempts: changed root bone detection, rewrote bone transform computation to match USD approach, tried two-pass bone index mapping
  - Symptoms: Geometry appears but skeleton is collapsed/wrong, bone transforms incorrect
  - **Workaround**: Use USD export for ArcheAge models (works correctly with animations)

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
