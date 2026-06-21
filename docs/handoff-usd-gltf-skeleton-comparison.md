# Handoff: USD vs glTF Skeleton/Animation Comparison

## Context
Branch `feature/claude-inspection-skills`. Investigating why USD-imported geometry breaks when animations are applied, while glTF import works correctly. This builds on the IK debugging work in `docs/handoff-ik-debugging.md`.

**Critical new finding**: The USD rest pose geometry is CORRECT. The problem only manifests when animations are applied to the armature. This means bind transforms and rest transforms are likely fine — the issue is in how animation transforms interact with bone representation.

## Test Asset
- **File**: `d:\depot\mwo\objects\gamemodes\turret\turret_a.chr`
- **ObjectDir**: `d:\depot\mwo`
- **Generated outputs** (already exist in the turret directory):
  - `turret_a.usda` — USD output
  - `turret_a.gltf` + `turret_a.bin` — glTF output
- **To regenerate**:
  ```bash
  dotnet run --project cgf-converter -- "d:\depot\mwo\objects\gamemodes\turret\turret_a.chr" -objectdir "d:\depot\mwo" -usd
  dotnet run --project cgf-converter -- "d:\depot\mwo\objects\gamemodes\turret\turret_a.chr" -objectdir "d:\depot\mwo" -gltf
  ```

## Focus Chain
Analyze the bone chain: **Root -> turret_a_base -> turret_gyro -> turret_arm**

This chain is interesting because:
- `turret_a_base` has a 90deg Z rotation in its bind pose
- `turret_gyro` has a 90deg rotation (Y/Z swap) in its bind pose  
- `turret_arm` has a non-trivial rotation (pitch angle)
- These accumulated rotations make animation transform errors visible

## What We Proved in This Session

### 1. Converter output is mathematically correct
Both renderers compute identical local transforms from the same source data:
- Root bone: `localTransform = invert(BindPoseMatrix)`
- Child bone: `localTransform = parent.BindPoseMatrix * invert(child.BindPoseMatrix)`

The only difference is coordinate system:
- **USD**: `Transpose(localMatrix)` — keeps CryEngine Z-up
- **glTF**: `SwapAxes(Transpose(localMatrix))` — converts to Y-up

We verified quaternion equivalence: glTF quaternions after Blender's Yup2Zup conversion match USD quaternions exactly for every bone tested.

### 2. Blender importers handle bones differently
**USD importer** (`D:\blender-git\blender\source\blender\io\usd\intern\usd_skel_convert.cc`):
- Line 824: Uses `GetJointWorldBindTransforms()` (world-space)
- Line 873: `ED_armature_ebone_from_mat4(ebone, mat4)` — extracts bone direction from Y column of world-space bind matrix
- No bone orientation heuristic applied
- Line 1008: `set_rest_pose()` applies restTransform-vs-bindLocal difference as pose bone transform

**glTF importer** (`D:\blender-git\blender\scripts\addons_core\io_scene_gltf2\blender\imp\`):
- `vnode.py:536`: `prettify_bones()` applies TEMPERANCE heuristic — rotates bones to point toward children
- `vnode.py:618`: `rotate_edit_bone()` compensates children's transforms after rotation
- `node.py:248`: Pose bone compensation: `pose_bone.rotation_quaternion = er.conjugated() @ r`
- Result: edit bone orientation is "pretty" (pointing toward children), pose bone compensates so net TRS matches original

### 3. Key difference: how animation transforms are applied
This is the untested hypothesis. The different bone representations (edit bone orientation + pose bone compensation) mean that **animation transforms get interpreted in different local coordinate frames**:

- **glTF**: Animation rotations are applied to pose bones that already have a compensating rotation baked in. The edit bone is oriented toward children, so the animation rotation axes align with the bone's visual direction.
- **USD**: Animation rotations are applied to pose bones whose rest state comes from `set_rest_pose()` (restTransform * inverse(bindLocal)). The edit bone orientation is the raw bind transform Y column, which may be 90deg off from the bone's spatial direction.

**If the animation channel applies a rotation in a local space that's 90deg rotated between USD and glTF, that would explain geometry breaking only during animation.**

## Investigation Plan for Next Session

### Prerequisites
- Blender MCP server running (addon: `blender-mcp`, Claude Code MCP configured)
- Both turret_a.usda and turret_a.gltf available

### Step 1: Import both formats side-by-side
```python
# Import glTF
bpy.ops.import_scene.gltf(filepath=r"d:\depot\mwo\objects\gamemodes\turret\turret_a.gltf")
# Rename armature to distinguish
gltf_arma = [obj for obj in bpy.data.objects if obj.type == 'ARMATURE'][0]
gltf_arma.name = "GLTF_Armature"

# Import USD
bpy.ops.wm.usd_import(filepath=r"d:\depot\mwo\objects\gamemodes\turret\turret_a.usda")
usd_arma = [obj for obj in bpy.data.objects if obj.type == 'ARMATURE' and obj != gltf_arma][0]
usd_arma.name = "USD_Armature"
```

### Step 2: Compare rest pose bone data
For each bone in the focus chain (Root, turret_a_base, turret_gyro, turret_arm), compare:

```python
for bone_name in ['Root', 'turret_a_base', 'turret_gyro', 'turret_arm']:
    for arma_name in ['GLTF_Armature', 'USD_Armature']:
        arma = bpy.data.objects[arma_name]
        bone = arma.data.bones[bone_name]
        pose_bone = arma.pose.bones[bone_name]
        
        print(f"\n{arma_name} / {bone_name}:")
        print(f"  Edit bone head: {bone.head_local}")
        print(f"  Edit bone tail: {bone.tail_local}")
        print(f"  Edit bone roll: {bone.roll}")
        print(f"  Edit bone matrix: {bone.matrix_local}")
        print(f"  Pose bone location: {pose_bone.location}")
        print(f"  Pose bone rotation: {pose_bone.rotation_quaternion}")
        print(f"  Pose bone matrix: {pose_bone.matrix}")
        print(f"  Pose bone matrix_basis: {pose_bone.matrix_basis}")
```

**What to look for**: The `pose_bone.matrix` (final world-space transform) should be identical between USD and glTF for the same bone. If it's not, that's the bug. The `bone.matrix_local` (edit bone) will differ, and the `pose_bone.matrix_basis` (pose compensation) should compensate — verify this.

### Step 3: Apply a simple rotation animation and compare
Apply a small rotation to `turret_arm` on both armatures and see if geometry follows differently:

```python
import mathutils

for arma_name in ['GLTF_Armature', 'USD_Armature']:
    arma = bpy.data.objects[arma_name]
    pose_bone = arma.pose.bones['turret_arm']
    
    # Apply a 30deg rotation around local X
    rot = mathutils.Quaternion((1, 0, 0), 0.523)  # 30 degrees
    pose_bone.rotation_quaternion = pose_bone.rotation_quaternion @ rot
    
    print(f"\n{arma_name} / turret_arm (after rotation):")
    print(f"  Pose matrix: {pose_bone.matrix}")
```

**What to look for**: After applying the same local rotation, do the final world-space matrices match? If they diverge, the local coordinate frames are different between the two imports.

### Step 4: Check animation import specifically
If the turret has animations (.caf/.dba), import them on both armatures and compare the pose bone transforms at a specific frame.

## Source Code References

### CryEngine Converter
- **USD skeleton**: `CgfConverter/Renderers/USD/UsdRenderer.Skeleton.cs` — `GetBindTransforms()` (line 152), `GetRestTransforms()` (line 189)
- **glTF skeleton**: `CgfConverter/Renderers/Gltf/BaseGltfRenderer.Geometry.cs` — bone creation (line 638), inverse bind matrices (line 821)
- **Axis swap**: `CgfConverter/Renderers/Gltf/BaseGltfRenderer.SwapAxes.cs` — `SwapAxes(Matrix4x4)` (line 62)

### Blender USD importer (C++)
- `D:\blender-git\blender\source\blender\io\usd\intern\usd_skel_convert.cc`
  - `import_skeleton()` (line 729) — creates bones from world-space bind transforms
  - `set_rest_pose()` (line 684) — applies restTransform vs bindLocal as pose bone offset
  - `ED_armature_ebone_from_mat4()` (armature_utils.cc:236) — extracts bone direction from Y column
  - `mat3_to_vec_roll()` (armature.cc:2600) — extracts direction and roll from rotation matrix

### Blender glTF importer (Python)
- `D:\blender-git\blender\scripts\addons_core\io_scene_gltf2\blender\imp\vnode.py`
  - `pick_bind_pose()` (line 509) — solves local bind from inverse bind matrices
  - `prettify_bones()` (line 536) — TEMPERANCE heuristic rotates bones toward children
  - `rotate_edit_bone()` (line 618) — applies rotation + compensates children
  - `calc_bone_matrices()` (line 633) — computes armature-space transforms
- `D:\blender-git\blender\scripts\addons_core\io_scene_gltf2\blender\imp\node.py`
  - `create_bones()` (line 184) — creates edit bones with heuristic orientation
  - Lines 244-251 — pose bone compensation: `er.conjugated() @ r`

## Hypothesis Test Results (2026-04-08)

### Hypothesis: DISPROVEN

> "The animation transform is applied in a different local space between USD and glTF imports, because the edit bone orientations differ."

This was tested by importing both `turret_a.gltf` and `turret_a.usda` into Blender side-by-side, then comparing bone data and deformed vertex positions after applying identical rotations.

### What we observed

**Edit bone orientations DO differ** between the two importers:
- glTF: TEMPERANCE heuristic rotates edit bones to point toward children
- USD: edit bones oriented by raw bind transform Y column
- Example — `turret_arm` Y-axis: glTF `[0, -0.586, 0.810]` vs USD `[0, 0.810, 0.586]`

**World-space pose matrices also differ** for the same bone:
- `turret_arm` glTF: `[[1,0,0], [0,-0.586,-0.810], [0,0.810,-0.586]]`
- `turret_arm` USD: `[[1,0,0], [0,0.810,-0.586], [0,0.586,0.810]]`

**But the deformation matrices are IDENTICAL:**
- `deform = pose_bone.matrix @ bone.matrix_local.inverted()`
- Max difference across all focus chain bones: 7.7e-7 (floating point noise)

**And deformed vertex positions match exactly after applying the same rotation:**
- Applied `rotation_quaternion = Quaternion((1,0,0), 30deg)` to `turret_arm` on both armatures
- Compared all 19,855 deformed vertex positions
- Max diff: 6.0e-6 (floating point noise)
- Zero vertices with diff > 0.001

### Why the hypothesis was wrong

The edit bone orientation is purely cosmetic for skinning. The deformation math is:

```
deformed_vertex = (pose_matrix @ rest_matrix_inverse) @ vertex
```

Both `pose_matrix` and `rest_matrix` differ by the same bone-orientation rotation, so it cancels out. The same `rotation_quaternion` value on a pose bone produces identical vertex deformation regardless of which importer created the armature.

### Additional verified facts
- Rest pose geometry: identical (vertex diff ~1e-6)
- Vertex counts: identical (19,855)
- Vertex weights: identical (spot-checked turret_arm group)
- Child bone propagation: identical (turret_a_gun_attach position matches within 2e-6)

### Revised fix options (a), (b), (c) from original hypothesis

All three are **unnecessary** — bone orientation is not the problem.

## Resolution (2026-04-08)

### Root cause: non-animated bones got Identity rotation in USD SkelAnimation

USD `SkelAnimation` *replaces* `restTransforms` entirely — every bone needs explicit rotation/translation values at every time sample. Unlike glTF, where bones without animation channels simply keep their rest values, USD bones without rotation tracks were getting `Quaternion.Identity`, zeroing out their rest rotation.

For `turret_a_gun_attach` (which has no rotation track in the DBA), this replaced a ~35° rest rotation with identity, visibly tilting the turret top geometry.

### Fix applied (commit 355fed2)

Two changes in `UsdRenderer.Animation.cs`:

1. **`BuildRestRotationMapping()` convention fix**: The function was decomposing the non-transposed CryEngine matrix, producing quaternions that were the *conjugate* of the skeleton's `restTransforms` (which are transposed). Fixed by transposing before decomposing:
   ```csharp
   // Before: Matrix4x4.Decompose(restMatrix, ...)
   // After:
   Matrix4x4.Decompose(Matrix4x4.Transpose(restMatrix), out _, out var rotation, out _)
   ```

2. **Rest rotation fallback for non-animated bones**: DBA and CAF paths now use rest rotation instead of `Quaternion.Identity` when a bone has no rotation track.

### Verification
- Imported both `turret_a.usda` and `turret_a.gltf` into Blender side-by-side
- Deformation matrices match within floating point noise for all bones at frame 0
- Geometry visually matches between USD and glTF during animation playback
