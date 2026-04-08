# Handoff: IK Debugging — Bone Roll Investigation

## Context
Branch `feature/usd-import-core`. Debugging Issue 4 from `docs/handoff-usd-import-issues.md` — leg IK constraints don't work correctly with USD-imported bone data.

## What We Did

### Systematic IK Debugging (Manual Steps in Blender Console)
Imported adder mech with `debug_import=True` (clean skeleton + geometry, no IK bones or constraints), then manually applied each IK step from the investigation plan in `handoff-usd-import-issues.md`.

**Results by step:**
1. **Hip/Root bone modifications** — geometry follows correctly. Tested rotating Bip01_Pelvis and Bip01_L_Hip, all children move properly.
2. **Create Foot_IK.R** — bone exists, no issues.
3. **Create Knee_IK.R** — bone exists, no issues.
4. **Add Copy Rotation constraint to foot** — setting `target_space = 'LOCAL_WITH_PARENT'` caused foot to rotate 90deg around Y, then setting `owner_space = 'LOCAL_WITH_PARENT'` rotated it back (they cancel out). Rotating Foot_IK.R rotates foot geometry correctly on all axes.
5. **Disable foot inherit rotation** — no change, still works.
6. **Add IK to calf (chain_count=2)** — **THIS IS WHERE IT BREAKS.** Setting subtarget without chain_count caused entire mech to fall on its right side (IK solving up the full chain). Setting chain_count=2 brought it upright but the leg flies backwards. Foot detaches from IK target.

### Bone Roll Discovery
Checked bone rolls after USD import:
```
Bip01_R_Thigh: roll=1.5406  (~88 deg)
Bip01_R_Calf:  roll=1.5376  (~88 deg)
Bip01_R_Foot:  roll=1.5708  (~90 deg, exactly pi/2)
Bip01_R_UpperArm: roll=-1.9552
Bip01_R_Forearm:  roll=-1.8175
Bip01_R_Hand:     roll=-1.8175
```
Leg bones all have ~pi/2 rolls. Arms have different rolls (~-1.82 to -1.96) that happen to work with their IK setup.

### Recalculating Rolls — Partial Success
1. Selected leg bones (R_Thigh, R_Calf, R_Foot) and ran `bpy.ops.armature.calculate_roll(type='GLOBAL_POS_Z')`
2. Rolls went to ~0: Thigh=0.0082, Calf=0.0005, Foot=0.0000
3. IK rotation axes were swapped (X mapped to Z, Z to X) — because Foot_IK.R still had old roll
4. **Also recalculated Foot_IK.R roll** — IK solving then worked correctly. Calf and thigh followed foot IK, rotations matched on all axes.
5. **But**: geometry was offset (foot stuck up and behind) because mesh was skinned to bones with original rolls, then rolls changed after binding.

### Attempted Fixes (Both Reverted)
**Attempt 1**: Add roll recalculation in `import_armature()` (bones.py) right after USD import, before geometry import. Debug import worked (geometry aligned, bones rotated correctly). Full import (with IKs) still broken — because `create_IKs()` modifies bone hierarchy after the recalculation (reparents pelvis, changes root bone tail, creates Hip_Root via flip_bone).

**Attempt 2**: Added second roll recalculation at end of `create_IKs()` edit mode section, after all hierarchy changes. Still didn't work — Foot_IK.R rotated geometry around a point near the knee IK, Knee_IK had a dashed line to a point 7m in -X direction.

**Both changes were reverted.** Code is back to the state before this debugging session.

## Key Findings

1. **The IK solver is sensitive to bone rolls** — ~90deg rolls from USD cause wrong bend directions.
2. **Recalculating rolls fixes IK solving direction** — but only when done after ALL bones (including IK targets) exist and hierarchy is finalized.
3. **Order-of-operations problem**: Recalculating in `import_armature()` gets invalidated by `create_IKs()` hierarchy changes. Recalculating after `create_IKs()` hierarchy changes still doesn't produce correct results for the full import pipeline.
4. **The manual step-by-step approach worked**, but the integrated pipeline doesn't — suggesting something about the full import flow interacts differently than manual application.

## Unresolved Theory: USD Skeleton May Be Fundamentally Wrong

Geoff's suspicion: the bones may be "in the right place, but for the wrong reason." The USD skeleton could have correct bone positions but wrong orientations/transforms baked in from the USD export. Recalculating rolls is treating a symptom, not the root cause.

**Evidence supporting this:**
- The 90deg rolls are suspiciously uniform across all leg bones — this isn't random, it's a systematic orientation difference in how USD represents the skeleton vs Collada
- The Copy Rotation constraint snapping 90deg when setting target_space (Step 4) suggests the bone local spaces are fundamentally rotated
- Arms working could be coincidental rather than correct — their IK setup is simpler (no Copy Rotation, no `use_inherit_rotation = False`)
- Recalculating rolls "fixes" IK but breaks geometry binding, suggesting the rolls and the skin weights are coupled — you can't fix one without fixing the other

**Next investigation**: Compare a turret model imported as both glTF and USD to see if the armature and skinning differ between formats. This would confirm whether the USD exporter (Cryengine Converter) is producing skeletons with different bone orientations than glTF/Collada.

## Key Files
- `io_cryengine_importer/Cryengine_Importer.py` — `create_IKs()` at line 69
- `io_cryengine_importer/bones.py` — `import_armature()` at line 17, `copy_bone()`, `new_bone()`, `flip_bone()`
- `docs/handoff-usd-import-issues.md` — parent issue tracker with full investigation plan

## Test Data
- Adder mech: `d:\depot\mwo\Objects\mechs\adder\adder.cdf`
