# Handoff: USD Import Core — Open Issues

## Context
We're on branch `feature/usd-import-core` (off `release/4.0`) upgrading the Cryengine Importer Blender addon from Collada to USD for Blender 5.0+. The core USD import swap is done and committed. The mech import (adder) runs end-to-end — armature imports, components import, materials create, custom bone shapes apply. The AssetImporter operator has been restored for single-file USD imports (tested with KCD2 hen_brown model). Animation import module is working.

## What Was Done
- All `bpy.ops.wm.collada_import()` replaced with `bpy.ops.wm.usd_import()`
- `.dae` → `.usda` throughout
- `bpy.data.objects['Armature']` → `bones.find_armature_in_objects()` (finds by `obj.type == 'ARMATURE'`)
- New object tracking via set diff (`objects_before`/`objects_after`) instead of `bpy.context.selected_objects`
- Fixed Blender 5.0 deprecations: `show_axes`, `ShaderNodeSeparateRGB/CombineRGB/MixRGB`
- **USD hierarchy cleanup**: `utilities.cleanup_usd_import()` deletes all EMPTY objects (root, _materials, Armature/SkelRoot) after each USD import. Component imports also delete redundant per-component skeletons and strip armature modifiers.
- **AssetImporter restored**: Simple single-file `.usda` import operator using native USD materials (no `.mtl` pipeline). Registered in File > Import menu.
- **Animation import**: `animations.py` module discovers animations via CDF skeleton reference, imports as Blender Actions with fake user.
- `import_asset()` simplified — USD handles materials natively via `import_usd_preview=True`

## Resolved Issues

### Issue 1: Orphan `_materials` empties — FIXED
`utilities.cleanup_usd_import()` removes all EMPTY objects after each USD import, including `_materials` scope nodes.

### Issue 3: Orphan `root` objects — FIXED
Same cleanup function handles `root` Xform containers. Component imports also delete the redundant per-component `Skeleton` (armature). Mesh objects are explicitly moved to the Mech collection.

### Issue 2: Mech component positioning — FIXED
**Root cause**: `create_IKs()` modifies the bone hierarchy (reparents Bip01_Pelvis, moves Bip01 tail, etc.). With Collada, geometry was imported first and Blender compensated mesh transforms when bones changed. With USD, geometry imported after bone changes worked correctly.

**Fix**: Moved `create_IKs()` to run BEFORE `import_mech_geometry()` so the bone hierarchy is finalized before meshes are parented to bones. Also added `matrix_world = Identity(4)` to clear residual USD transforms before applying CDF world-space transforms.

## Open Issues

### Issue 4: Leg IK constraints not working correctly with USD bone data

**Symptom**: When using the Foot_IK controls, leg geometry doesn't follow correctly:
- Thigh mesh moves approximately twice as fast as expected
- Foot mesh moves approximately half as fast as expected
- Calf mesh moves at roughly the correct speed
- Upper body IKs (hands, elbows) work fine

**What we've ruled out**:
- NOT a mesh positioning issue — rest pose is perfect, geometry is in the correct place
- NOT an armature modifier double-transform — stripped modifiers, no change
- NOT a bone parenting issue — manually rotating deform bones (without IK) moves geometry correctly
- NOT the ordering change — the symptom is specific to IK posing
- Arms work fine — issue is specific to leg IK setup

**What we know**:
- The leg IK setup in `create_IKs()` is identical code to what worked with Collada
- USD imports bones with different rolls than Collada (e.g. Bip01_R_Calf has 88° roll)
- The bone roll affects how IK constraints solve — different rolls produce different rotation axes
- Arms may work because their bone geometry happens to be compatible, or because their IK setup is simpler (no Copy Rotation, no `use_inherit_rotation = False`)

**Leg-specific IK setup (what's different from arms)**:
- Foot bones have `use_inherit_rotation = False`
- Copy Rotation constraint on feet targeting Foot_IK bones (LOCAL_WITH_PARENT space)
- IK on calf → Foot_IK with `chain_count=2` (covers calf + thigh)
- IK on thigh → Knee_IK with `chain_count=1`
- Hip_Root bone created, Bip01_Pelvis reparented to it

### Investigation Plan: Systematic IK Debugging

**Approach**: Import adder with `debug_import=True` (no IKs), then manually apply each IK step in Blender's Python console to isolate where behavior diverges.

**Prerequisites**: Import adder mech with Debug Import checked. This gives a clean skeleton + geometry with no IK bones or constraints.

**Step 1: Hip/Root bone modifications (EDIT mode)**
```python
import bpy, mathutils
armature = [obj for obj in bpy.data.objects if obj.type == 'ARMATURE'][0]
bpy.context.view_layer.objects.active = armature
bpy.ops.object.mode_set(mode='EDIT')
amt = armature.data

# Copy Pelvis → Hip_Root, flip it, reparent Pelvis
from io_cryengine_importer.bones import copy_bone, flip_bone
hip_root_bone = copy_bone(armature, "Bip01_Pelvis", "Hip_Root")
amt.edit_bones[hip_root_bone].use_connect = False
flip_bone(armature, hip_root_bone)
amt.edit_bones['Bip01_Pelvis'].parent = amt.edit_bones[hip_root_bone]
amt.edit_bones['Bip01_Pitch'].use_inherit_rotation = True

# Modify root bone
root_bone = amt.edit_bones['Bip01']
root_bone.tail.y = root_bone.tail.z
root_bone.tail.z = 0.0
root_bone.use_deform = False
root_bone.use_connect = False
```
**TEST**: Switch to pose mode, rotate Bip01_Pelvis — does geometry follow correctly?

**Step 2: Create right foot IK bone (EDIT mode)**
```python
bpy.ops.object.mode_set(mode='EDIT')
rightFootIKName = copy_bone(armature, "Bip01_R_Foot", "Foot_IK.R")
amt.edit_bones[rightFootIKName].use_connect = False
amt.edit_bones[rightFootIKName].use_deform = False
amt.edit_bones[rightFootIKName].parent = amt.edit_bones["Bip01"]
```
**TEST**: No constraints yet, just verify bone exists.

**Step 3: Create right knee IK bone (EDIT mode)**
```python
from io_cryengine_importer.bones import new_bone
offset = -4  # chickenwalker
rightKneeIKName = new_bone(armature, "Knee_IK.R")
amt.edit_bones[rightKneeIKName].head = amt.edit_bones["Bip01_R_Calf"].head + mathutils.Vector((0, offset, 0))
amt.edit_bones[rightKneeIKName].tail = amt.edit_bones[rightKneeIKName].head + mathutils.Vector((0, offset/4, 0))
amt.edit_bones[rightKneeIKName].use_deform = False
amt.edit_bones[rightKneeIKName].parent = amt.edit_bones["Bip01"]
```
**TEST**: No constraints yet, just verify bone exists.

**Step 4: Add foot Copy Rotation constraint (POSE mode)**
```python
bpy.ops.object.mode_set(mode='POSE')
crc = armature.pose.bones["Bip01_R_Foot"].constraints.new('COPY_ROTATION')
crc.target = armature
crc.subtarget = "Foot_IK.R"
crc.target_space = 'LOCAL_WITH_PARENT'
crc.owner_space = 'LOCAL_WITH_PARENT'
crc.use_offset = True
```
**TEST**: Rotate Foot_IK.R — does the foot follow? Does the foot mesh follow correctly?

**Step 5: Disable foot inherit rotation**
```python
amt.bones['Bip01_R_Foot'].use_inherit_rotation = False
```
**TEST**: Rotate Foot_IK.R again — any change in behavior?

**Step 6: Add IK constraint to calf (chain_count=2)**
```python
bpose = armature.pose
bpose.bones["Bip01_R_Calf"].constraints.new(type='IK')
bpose.bones["Bip01_R_Calf"].constraints['IK'].target = armature
bpose.bones["Bip01_R_Calf"].constraints['IK'].subtarget = 'Foot_IK.R'
bpose.bones["Bip01_R_Calf"].constraints['IK'].chain_count = 2
```
**TEST**: Move Foot_IK.R — do calf and thigh follow? Does geometry track correctly?

**Step 7: Add IK constraint to thigh (chain_count=1)**
```python
bpose.bones["Bip01_R_Thigh"].constraints.new(type='IK')
bpose.bones["Bip01_R_Thigh"].constraints['IK'].target = armature
bpose.bones["Bip01_R_Thigh"].constraints['IK'].subtarget = 'Knee_IK.R'
bpose.bones["Bip01_R_Thigh"].constraints['IK'].chain_count = 1
```
**TEST**: Move Foot_IK.R — does the knee point toward Knee_IK.R? Does geometry still track correctly?

After each step, test by posing and check if geometry follows the bones correctly. The step where behavior breaks tells us exactly which constraint or bone modification is causing the issue.

## Key Files
- `io_cryengine_importer/Cryengine_Importer.py` — `import_mech_geometry()`, `import_geometry()`, `import_asset()`, `create_IKs()`
- `io_cryengine_importer/bones.py` — `import_armature()`, `find_armature_in_objects()`, `copy_bone()`, `new_bone()`
- `io_cryengine_importer/utilities.py` — `cleanup_usd_import()`
- `io_cryengine_importer/animations.py` — `import_animation()`, `import_all_animations()`, `get_skeleton_name_from_cdf()`
- `io_cryengine_importer/__init__.py` — `AssetImporter`, `MechImporter`, `PrefabImporter` operators

## Test Data
- Adder mech: `d:\depot\mwo\Objects\mechs\adder\adder.cdf`
- Hen model (asset import): `d:\depot\KCD2\objects\characters\animals\hen\hen_brown.usda`
- Hen animations: `d:\depot\KCD2\objects\characters\animals\hen\hen_brown_anim_*.usda`
