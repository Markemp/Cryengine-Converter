# brfl_fps_behr_p4ar.chr Animation Analysis

## File Locations
- **Model**: `{sc41ObjectDir}\Objects\fps_weapons\weapons_v7\behr\rifle\p4ar\brfl_fps_behr_p4ar.chr`
- **Animation DBA**: `{sc41ObjectDir}\Animations\fps_weapons\weapons_v7\behr\rifle\p4ar.dba`

## Overview
- Number of animations: 9
- Number of bones: 26

## Test Animations

### Anim A: Rotation Only fire_trigger_01
- **Name**: `animations/fps_weapons/weapons_v7/behr/rifle/p4ar/brfl_behr_p4ar_fire_trigger_01.caf` (index 2)
- **Animated bone**: `trigger02` (CRC32: 0xC9E2516F)

### Anim B: Mixed (1 pos only + 1 rot only)  fire_01
- **Name**: `animations/fps_weapons/weapons_v7/behr/rifle/p4ar/brfl_behr_p4ar_fire_01.caf` (index 0)
- **Animated bones**:
  - `bolt` (CRC32: 0x6DC0FD07) - position only
  - `coverPlate` (CRC32: 0xD9A6C1AD) - rotation only
  
### Anim C:
- **Name**: `animations/fps_weapons/weapons_v7/behr/rifle/p4ar/stocked_alerted_stand_brfl_behr_p4ar_reload_full_01_add.caf` (index 8)
- ** Animated bones**:
  - `safe` (CRC32: 530851983 / 0x1FA4288F)
  - `bolt` (CRC32: 1841364231 / 0x6DC0FD07)
  - `magAttach` (CRC32: 1909618695 / 0x71D27807)
  - `coverPlate` (CRC32: 3651584429 / 0xD9A6C1AD)

## Bone Rest Poses (from skeleton)

### root
- relative rot: (0, 0, 0, 1) [identity]
- relative pos: (0, 0, 0)
- world rot: (0, 0, 0, 1)
- world pos: (0, 0, 0)

### trigger01 (child of root)
- relative rot: (0, 0, 0, 1) [identity]
- relative pos: (0, 0.044686, 0.042497)
- world rot: (0, 0, 0, 1)
- world pos: (0, 0.044686, 0.042497)

### trigger02 (child of trigger01)
- relative rot: (0, 0, 0, 1) [identity]
- relative pos: (0, 0.011537, 0)
- world rot: (0, 0, 0, 1)
- world pos: (0, 0.056223, 0.042497)

### bolt (child of root)
- relative rot: (0, 0, 0, 1) [identity]
- relative pos: (0, 0.242, 0.132063)
- world rot: (0, 0, 0, 1)
- world pos: (0, 0.242, 0.132063)

### coverPlate (child of root)
- **relative rot: (0, 0.999048, 0, -0.043619)** [NOT identity! ~175° around Y]
- relative pos: (0.02241, 0.15797, 0.10213)
- world rot: (0, 0.999048, 0, -0.043619)
- world pos: (0.02241, 0.15797, 0.10213)

### safe (child of root)
- relative rot: (0, 0, 0, 1) [identity]
- relative pos: 0.000000, 0.033961, 0.057669
- world rot: (0, 0, 0, 1)
- world pos: 0.000000, 0.033961, 0.057669

### magAttach
- relative rot: 0.124774, 0.000000, 0.000000, 0.992185
- relative pos: 0.000000, 0.173156, -0.010600
- world rot: 0.124774, 0.000000, 0.000000, 0.992185
- world pos: 0.000000, 0.173156, -0.010600

---

## Animation A Details

### Metadata
- Start position: (0, 0, 0)
- Start rotation: (0, 0, 0, 1) [identity]

### Animation Data (trigger02)
- 8 rotation keys, format 0x8042
- Bone rest rotation: **identity (0, 0, 0, 1)**

| Frame | Quaternion (x, y, z, w) | Magnitude | Approx Angle |
|-------|-------------------------|-----------|--------------|
| 0 | (0, 0, 0, 1) | 1.0 | 0° |
| 1 | (-0.1908, 0, 0, 0.9816) | 1.0 | ~22° around X |
| 2 | (-0.2322, 0, 0, 0.9727) | 1.0 | ~27° around X |
| 3 | (-0.2084, 0, 0, 0.9780) | 1.0 | ~24° around X |
| 4 | (-0.1513, 0, 0, 0.9885) | 1.0 | ~17° around X |
| 5 | (-0.0824, 0, 0, 0.9966) | 1.0 | ~9° around X |
| 6 | (-0.0244, 0, 0, 0.9997) | 1.0 | ~3° around X |
| 7 | (0, 0, 0, 1) | 1.0 | 0° |

**Interpretation**: Trigger pull animation - rotates around X axis (typical trigger motion)

---

## Animation B Details: 

### Metadata
- Start position: (0, 0, 0)
- Start rotation: (0, 0, 0, 1) [identity]

### Animation Data (bolt) - Position Only
- 4 position keys, times: (0, 1, 2, 3)
- Bone rest position: **(0, 0.242, 0.132063)**
- Format: SNORM (need scale factor)

| Frame | SNORM Values | Notes |
|-------|--------------|-------|
| 0 | (0, -8781, 0) | Bolt back? |
| 1 | (0, 0, 0) | At rest? |
| 2 | (0, 0, 0) | At rest? |
| 3 | (0, -8781, 0) | Bolt back? |

**Missing**: SNORM scale factor to convert -8781 to meters

### Animation Data (coverPlate) - Rotation Only
- 4 rotation keys, format 0x8040, times: (0, 1, 2, 3)
- Bone rest rotation: **(0, 0.999048, 0, -0.043619)** [~175° around Y]

| Frame | Quaternion (x, y, z, w) | Magnitude |
|-------|-------------------------|-----------|
| 0 | (0, 0, 0, 1) | 1.0 |
| 1 | (0, 0, 0, 1) | 1.0 |
| 2 | (0, 0, 0, 1) | 1.0 |
| 3 | (0, 0, 0, 1) | 1.0 |

**Note**: All identity quaternions - either this animation doesn't rotate coverPlate, or this is a test case for delta vs absolute interpretation.

---

## Animation C Details:
- Additive animation?  Not sure if this makes a difference
- Time data: 111 ticks. Both rot and pos time header has start anim at 0, end anim at 110

### Animation data (magRelease) - Rotation and position
Controller 0 (safe) is unanimated.  
Controller 1 (bolt) pos Only
Controller 2 (magAttach) Both
Controller 3 (coverPlate) Both

sample pos data (50 total positions, scale (?) = -0.375232, -0.224156, -0.374677)

magAttach animation (controller index 2)
Rot=111 keys (flags=0x8042), Pos=50 keys (flags=0xC142)
Pos info: X=ON Y=ON Z=ON | Scale: (-0.375232, -0.224156, -0.374677)
Sample rots:
0-11: (0.1248, 0.0000, 0.0000, 0.9922) mag=1.0000 [OK]
12-14: (0.3215, 0.2606, 0.0855, 0.9063) mag=1.0000 [OK]

Sample positions
0-1: (0.068847, 0.146881, 0.147506)
2: (0.024621, 0.110255, 0.000011)
3: (0.021277, 0.110125, 0.002390)
4: (0.019582, 0.110255, 0.004917)


coverPlate Animation (controller index 3)
Rot=111 keys (flags=0x8042), Pos=3 keys (flags=0xC240)
Pos info:  X=OFF Y=OFF Z=OFF | Scale: (0.029439, 0.157912, 0.102597). 3 pos keys all 0,0,0
Rot info:  111 rotation keys, all set to 0,0,0,1.


---

## Analysis Notes (Claude Code - 2025-12-27)

### CRITICAL FINDING: magAttach Proves Rotations are ABSOLUTE

The magAttach bone in Animation C provides definitive proof:

**magAttach rest rotation**: `(0.124774, 0, 0, 0.992185)` ≈ 14.3° around X axis

**Animation frames 0-11**: `(0.1248, 0, 0, 0.9922)` - **SAME as rest rotation!**

This proves rotations are **ABSOLUTE**, not deltas:

| Interpretation | Calculation | Result | Matches Rest? |
|----------------|-------------|--------|---------------|
| **ABSOLUTE** | `final = animValue` | 14.3° around X | ✓ YES |
| **DELTA** | `final = rest * delta = rest * rest` | ~28.6° around X | ✗ NO (doubled!) |

If the animation were storing deltas, identity (0,0,0,1) would mean "no change from rest". But the animation stores the rest rotation value itself, proving it's absolute.

### Bolt Position Confirms Delta Pattern

From Animation B (fire_01):
- Scale: `(0, 0.155398, 0.132063)` with only Y active
- Animation value after SNORM decode: `(0, -0.041644, 0)`
- Rest position: `(0, 0.242, 0.132063)`

The bolt retracts ~4.2cm backwards. This matches `rest + delta` pattern:
- Final Y = 0.242 + (-0.041644) = 0.200356m ✓

### CONFIRMED CONVENTION

**Ivo DBA uses DIFFERENT conventions for positions vs rotations:**

| Component | Convention | Formula | Rationale |
|-----------|------------|---------|-----------|
| **Positions** | DELTA | `final = rest + animDelta` | SNORM compression works well for small deltas |
| **Rotations** | ABSOLUTE | `final = animValue` | Quaternions are already normalized, no benefit from delta encoding |

### Why trigger02/coverPlate Tests Were Inconclusive

1. **trigger02**: Rest rotation = identity, so `absolute` and `rest * delta` give same result
2. **coverPlate**: Animation shows identity quaternions - likely means "straighten to identity" (absolute interpretation), not "no change from rest"

The coverPlate having all identity rotations in animations B and C, while having a ~175° rest rotation, suggests those animations intentionally reset it to identity (dust cover opens).

### Previous Working Theory (SUPERSEDED)

- **Positions**: `final = rest + animDelta` (CONFIRMED)
- **Rotations**: `final = rest * animDelta` (ASSUMED, needs verification)

The trigger02 animation is consistent with both absolute and delta interpretations because rest = identity.

### What Would Be Ideal Test Case

Find an animation where:
1. A bone has non-identity rest rotation (like coverPlate with ~175° Y)
2. The animation has non-identity rotation values
3. We can verify visually in Blender what the expected result should be

### Checking Other Animations in the DBA

The DBA has 9 animations. Worth checking:
- Index 1, 3, 4, 5, 6, 7, 8 for animations that might have more revealing rotation data
- Particularly reload animations which likely animate more bones

---

## Raw Data Reference

### Anim A Bone Hierarchy
```
root -> trigger01 -> trigger02
```

### Anim B Bone Hierarchy
```
root -> bolt
root -> coverPlate
```

### Anim C Bone Hierarchy
```
root -> safe
root -> magAttach
root -> bolt
root -> coverPlate
```

### Coordinate System
- Rifle facing +Y direction
- Rifle dimensions: ~0.38m in Y, ~0.18m in Z (height)
- trigger02 head position: Y=0.056223m, Z=0.042497m
- magAttach only bone with slight -z pos
