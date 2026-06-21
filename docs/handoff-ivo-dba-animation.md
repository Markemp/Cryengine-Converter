# Handoff: Ivo DBA Animation Position Decoding

## Context
Branch `feature/claude-inspection-skills`. Working on USD animation export for Star Citizen Ivo format DBA files. Rotations are working correctly. Positions are partially working — correct relative bone ordering but wrong magnitude for SNORM-encoded tracks.

## Test Assets
- **Landing gear**: `D:\depot\SC4.6\Data\Objects\Spaceships\Ships\AEGS\LandingGear\Avenger\AEGS_Avenger_LandingGear_Back_CHR.chr`
  - ObjectDir: `D:\depot\SC4.6\Data`
  - DBA: `D:\depot\SC4.6\Data\Animations\Spaceships\Ships\AEGS\LandingGear\Avenger.dba`
  - 4 animation blocks: back_compress (15 bones), back_extend (15 bones), front_compress (11 bones), front_extend (11 bones)
  - Flat hierarchy: all bones are children of `LG_Back_Root_Skin_bn`
- **Aloprat animal** (untested): `D:\depot\SC4.6\Data\Objects\Characters\Creatures\aloprat\aloprat_skel.chr`

**To regenerate USD with animations:**
```bash
dotnet run --project cgf-converter -- "D:\depot\SC4.6\Data\Objects\Spaceships\Ships\AEGS\LandingGear\Avenger\AEGS_Avenger_LandingGear_Back_CHR.chr" -objectdir "D:\depot\SC4.6\Data" -usd -animations
```

**To run diagnostic inspection:**
```bash
dotnet run --project CgfConverterTestingConsole -- "D:\depot\SC4.6\Data\Objects\Spaceships\Ships\AEGS\LandingGear\Avenger\AEGS_Avenger_LandingGear_Back_CHR.chr" --objectdir "D:\depot\SC4.6\Data" --custom
```

## What's Confirmed

### Rotations are ABSOLUTE
DBA rotation tracks store the final local rotation, not a delta from rest. Evidence:
- magAttach bone in p4ar rifle: rest rotation is (0.125, 0, 0, 0.992), animation frames 0-11 store the SAME value. If delta, identity would mean "no change" — storing the rest value proves absolute.
- Landing gear bones: animation rotations have the same general pattern as rest rotations but with different angles, consistent with absolute encoding.

### Three position storage formats exist
Determined by the high byte of PosFormatFlags:

| Format | Flag | Storage | Bytes/key | Header |
|--------|------|---------|-----------|--------|
| C0 | 0xC0xx | Raw float Vector3 | 12 | None |
| C1 | 0xC1xx | SNORM int16 x3 | 6 | 24 bytes (2x Vector3) |
| C2 | 0xC2xx | SNORM int16, packed active channels only | 2-6 | 24 bytes (2x Vector3) |

### C0 (raw float) values are ABSOLUTE local positions
Evidence from landing gear Block 0:
- `UpPiston_01` (C0): pos=(-0.001, -0.629, 1.062), rest=(-0.001, -0.604, 1.076) — close to rest, small offset
- `MainWheel` (C0): pos=(0, -0.362, -1.740), rest=(0, -0.402, -1.102) — different from rest but physically reasonable
- Values are clearly in the right ballpark as absolute positions

### C1/C2 SNORM values are NOT absolute positions
Evidence from landing gear Block 1 (extend animation):
- `MainWheel` (C2): pos[0]=(0, 0, 0), rest=(0, -0.402, -1.102) — zero can't be absolute (wheel at origin)
- `SubWheelCompress` (C2): pos[last]=(0, 0.005, -0.023) ≈ zero — last frame of extend = deployed = rest pose
- All bones with C2 format in Block 1 have near-zero last-frame values, consistent with "zero delta = at rest"

### SNORM header structure (24 bytes)
```
Vector3 channelMask;  // 12 bytes — FLT_MAX = channel inactive
Vector3 scale;        // 12 bytes — SNORM range multiplier
```

Observed header values for C2 bones:
- `channelMask.X` = FLT_MAX (3.4e38) — X channel always inactive for these bones
- `channelMask.Y` ≈ 0.000002 — effectively zero
- `channelMask.Z` ≈ 0.000014 to 0.000031 — effectively zero
- `scale` values are in the range of the bone's movement (e.g., scale.Z=-2.030 for SubWheelCompress)

Current decoding: `value = (snorm_int16 / 32767.0) * scale`

For C1, `channelMask` is read but NOT used (discarded). For C2, `channelMask` is only used to determine channel activity (< FLT_MAX = active).

## The Unsolved Problem

### Current implementation (C0=absolute, C1/C2=rest+delta)
This gives **correct relative bone ordering** but **wrong magnitude**:
- SubWheelCompress is correctly below MainWheel (matches rest pose relationship)
- But SubWheelCompress is "a bit too far down" at frame 0
- At max compression (frame 17), SubWheelCompress goes "too far into the wheel"
- Overall: "It travelled too far"

### Numeric example (Block 0 compress, frame 0)

| Bone | Format | Raw DBA | Rest | Current Output (C0=abs, C2=rest+delta) |
|------|--------|---------|------|----------------------------------------|
| MainWheel | C0 | (0, -0.362, -1.740) | (0, -0.402, -1.102) | (0, -0.362, -1.740) |
| SubWheelCompress | C2 | (0, 0.184, -0.832) | (0, 0.328, -1.399) | (0, 0.512, -2.231) |

Rest relationship: SubWheelCompress 0.297m below MainWheel in Z.
Current output: SubWheelCompress 0.491m below MainWheel in Z. Too much.

### Hypotheses to investigate

1. **SNORM header first Vector3 is an offset/center, not just a mask.** The decoding should be `center + (snorm/32767) * scale`. However, observed center values are ≈0, so this gives the same result as current. Unless the center values are wrong because we're reading from the wrong offset.

2. **Scale represents full range, not half range.** If `value = (snorm/32767) * scale / 2`, SubWheelCompress Z would be -0.416 delta instead of -0.832. Final = -1.815, which is 0.075 below MainWheel. Might be too close.

3. **SNORM values encode unsigned [0, 1] range, not signed [-1, 1].** Mapping: `value = ((snorm + 32767) / 65534) * scale`. For SubWheelCompress Z: `(13430 + 32767) / 65534 * (-2.030) = -1.431`, which is very close to rest (-1.399). This would make C1/C2 also absolute, with zero raw SNORM mapping to scale/2 (midpoint of range).

4. **The controller entry field layout may be wrong.** The 010 template shows a different field order for controller entries (single `formatFlags` uint16 instead of separate rot/pos format flags, different offset field ordering). Our parser may be reading offsets incorrectly, which would mean the SNORM headers we're reading are at the wrong position.

### Controller entry discrepancy (010 template vs our parser)

**010 template (24 bytes):**
```
ubyte    numKeys;
ubyte    padding;
uint16   formatFlags;     // single format field
uint32   rotDataOffset;
uint32   rotTimeOffset;
uint32   posDataOffset;
uint32   posTimeOffset;
uint32   reserved;
```

**Our parser (24 bytes):**
```
uint16   NumRotKeys;
uint16   RotFormatFlags;
uint32   RotTimeOffset;
uint32   RotDataOffset;
uint16   NumPosKeys;
uint16   PosFormatFlags;
uint32   PosTimeOffset;
uint32   PosDataOffset;
```

These are different field layouts! The 010 template has a single `formatFlags` and separate rot/pos offsets, while our parser splits into two 12-byte sections with separate format flags. Both sum to 24 bytes but interpret the bytes differently. **This discrepancy should be investigated** — if our parser is reading the wrong fields, the position data offsets (and therefore SNORM headers) would be at wrong positions in the binary.

## Key Source Files

- **Position format handling**: `CgfConverter/Models/Structs/IvoAnimationStructs.cs` — `ReadPositionKeys()`, `DecompressSNorm()`, format enums
- **DBA block parsing**: `CgfConverter/CryEngineCore/Chunks/ChunkIvoDBAData_900.cs` — controller entry reading, offset calculation
- **USD animation export**: `CgfConverter/Renderers/USD/UsdRenderer.Animation.cs` — `CreateSkelAnimationFromIvoDBA()` (line ~340)
- **glTF animation (untested)**: `CgfConverter/Renderers/Gltf/BaseGltfRenderer.Animation.cs` — Ivo DBA section (line ~490)
- **Inspection tool**: `CgfConverterTestingConsole/Program.cs` — `RunCustom()` dumps bone hierarchy, SNORM headers, format details
- **010 template**: `../010-Templates/CryEngineIvo-Animation.bt` — binary format reference

## What's NOT Working Yet
- Ivo DBA position magnitude for SNORM (C1/C2) encoded tracks
- Aloprat animal test asset (not yet attempted)
- glTF Ivo animation export (untested, code exists but Ivo skeletons don't produce glTF skin/nodes)
