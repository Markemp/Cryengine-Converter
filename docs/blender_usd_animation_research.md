# Blender USD Skeletal Animation Import Research

This document analyzes how Blender imports USD skeletal animations (UsdSkelAnimation) and converts them to Blender Actions. This research is intended for implementing a USD exporter that works well with Blender's importer.

## Executive Summary

Blender's USD importer:
- **Only imports ONE animation per skeleton** - the one referenced by `skel:animationSource`
- Does NOT traverse the stage looking for multiple SkelAnimation prims
- Names the resulting Action after the SkelAnimation prim name
- Relies heavily on USD's `UsdSkelSkeletonQuery` API to resolve animation bindings

---

## 1. SkelAnimation Import Process

### 1.1 How `skel:animationSource` is Read

Blender does **NOT** directly read the `skel:animationSource` relationship. Instead, it uses the USD SDK's `UsdSkelSkeletonQuery` API which internally resolves this relationship.

**Code path:**

```
import_skeleton() [usd_skel_convert.cc:726]
  └─> UsdSkelCache::GetSkelQuery(skel) [line 737]
        └─> Returns UsdSkelSkeletonQuery that internally resolves animationSource
              └─> skel_query.GetAnimQuery() [line 102]
                    └─> Returns UsdSkelAnimQuery for the bound animation
```

**Key code (`usd_skel_convert.cc:102-107`):**
```cpp
const pxr::UsdSkelAnimQuery &anim_query = skel_query.GetAnimQuery();

if (!anim_query) {
  /* No animation is defined. */
  return;
}
```

The `GetAnimQuery()` method follows the `skel:animationSource` relationship automatically. If no animation is bound, the function returns early.

### 1.2 Time Samples to Keyframes Conversion

**Location:** `import_skeleton_curves()` in `source/blender/io/usd/intern/usd_skel_convert.cc:86-320`

**Step 1: Get time samples**
```cpp
std::vector<double> samples;
anim_query.GetJointTransformTimeSamples(&samples);  // line 110
```

**Step 2: Create FCurves (10 per joint: 3 location + 4 rotation + 3 scale)**
```cpp
constexpr int curves_per_joint = 10;  // line 129

// For each joint, create curves for:
// - pose.bones["<name>"].location (indices 0-2)
// - pose.bones["<name>"].rotation_quaternion (indices 3-6)
// - pose.bones["<name>"].scale (indices 7-9)
```

**Step 3: Sample animation at each time code**
```cpp
for (const double frame : samples) {
    pxr::VtMatrix4dArray joint_local_xforms;
    skel_query.ComputeJointLocalTransforms(&joint_local_xforms, frame);  // line 230

    // Decompose each joint's transform
    pxr::UsdSkelDecomposeTransform(bone_xform, &t, &qrot, &s);  // line 252

    // Set keyframe values using set_fcurve_sample()
}
```

**Step 4: Finalize curves**
```cpp
for (FCurve *fcu : fcurves) {
    BKE_fcurve_handles_recalc(fcu);  // line 317
}
```

### 1.3 Transform Computation

The importer computes animation relative to the bind pose:
```cpp
// line 245-246
const pxr::GfMatrix4d bone_xform = joint_local_xforms.AsConst()[i] *
                                   joint_local_bind_xforms[i].GetInverse();
```

This means the USD animation values represent the full joint-local transform, and Blender computes the delta from the bind pose.

---

## 2. Multiple Animation Handling

### 2.1 Current Behavior

**Blender only imports the SINGLE animation bound via `skel:animationSource`.** There is:
- No code to discover multiple SkelAnimation prims in the file
- No import option to select which animation to import
- No code to import multiple animations as separate Actions

### 2.2 Evidence from Code

The stage traversal in `usd_reader_stage.cc` only creates readers for `UsdSkelSkeleton` prims, not `UsdSkelAnimation` prims:

```cpp
// usd_reader_stage.cc:271-272
if (params_.import_skeletons && prim.IsA<pxr::UsdSkelSkeleton>()) {
  return new USDSkeletonReader(prim, params_, settings_);
}
```

There is no `USDSkelAnimationReader` class. Animations are imported as a side effect of importing skeletons.

### 2.3 What This Means for USD File Structure

If your USD file contains multiple SkelAnimation prims (e.g., `walk`, `run`, `idle`):
- Blender will only import the one pointed to by `skel:animationSource` on the Skeleton
- Other animations will be completely ignored
- There is no way to import them without modifying the `skel:animationSource` binding

---

## 3. Action Creation Details

### 3.1 Action Creation Location

**File:** `source/blender/io/usd/intern/usd_skel_convert.cc`
**Function:** `import_skeleton_curves()` starting at line 86

**Action creation (lines 119-120):**
```cpp
bAction *act = blender::animrig::id_action_ensure(bmain, &arm_obj->id);
BKE_id_rename(*bmain, act->id, anim_query.GetPrim().GetName().GetText());
```

### 3.2 Action Naming

The Action is named after the **SkelAnimation prim's name**, not the path:
- USD path `/Root/Skeleton/walk_anim` → Action name: `walk_anim`
- The prim name comes from `anim_query.GetPrim().GetName().GetText()`

### 3.3 FCurve Creation

Curves are created using Blender's new animation system:
```cpp
blender::animrig::Channelbag &channelbag = blender::animrig::action_channelbag_ensure(
    *act, arm_obj->id);

// FCurve paths follow Blender conventions:
// "pose.bones[\"BoneName\"].location"
// "pose.bones[\"BoneName\"].rotation_quaternion"
// "pose.bones[\"BoneName\"].scale"
```

---

## 4. Import Options Affecting Animation

### 4.1 Relevant Import Parameters

From `source/blender/io/usd/usd.hh`:

```cpp
struct USDImportParams {
  bool import_skeletons;     // Must be true to import armatures AND their animations
  bool import_blendshapes;   // For blend shape animation (separate from skeletal)
  // Note: There is NO separate "import_animation" toggle for skeletal animation
};
```

### 4.2 Hidden `import_anim` Parameter

The `import_skeleton()` function accepts an `import_anim` parameter:
```cpp
void import_skeleton(Main *bmain,
                     Object *arm_obj,
                     const pxr::UsdSkelSkeleton &skel,
                     ReportList *reports,
                     bool import_anim = true);  // Defaults to true
```

However, this parameter is **not exposed in the UI** - it always defaults to `true`. There's no user option to import skeleton without animation.

---

## 5. Hardcoded Limitations and TODOs

### 5.1 Negative Determinant Matrices

**Location:** `usd_skel_convert.cc:849-888`

Bones with negative determinant matrices (from mirroring operations) prevent animation import:
```cpp
if (negative_determinant) {
  valid_skeleton = false;
  BKE_reportf(reports, RPT_WARNING,
      "USD Skeleton Import: bone matrices with negative determinants detected..."
      "The skeletal animation won't be imported");
}
```

### 5.2 No Multiple Animation Support

There are no TODOs or FIXMEs related to multiple animation import. This appears to be an intentional design decision rather than missing functionality.

### 5.3 Rest Pose vs Bind Pose Handling

The importer distinguishes between:
- **Bind transforms:** World-space joint transforms at bind time (`GetJointWorldBindTransforms`)
- **Rest transforms:** Optional rest pose that differs from bind (`HasRestPose()`, `ComputeJointLocalTransforms` with `atRest=true`)

---

## 6. Complete Import Code Path

```
USD_import() [usd_capi_import.cc]
  └─> USDStageReader::collect_readers()
        └─> create_reader_if_allowed() for UsdSkelSkeleton
              └─> new USDSkeletonReader(prim, params, settings)

  └─> reader->read_object_data()  [for each reader]
        └─> USDSkeletonReader::read_object_data() [usd_reader_skeleton.cc:24]
              └─> import_skeleton(bmain, object_, skel_, reports())
                    [usd_skel_convert.cc:726]

                    └─> UsdSkelCache::GetSkelQuery(skel)  // Creates skeleton query
                    └─> Creates bones from joint hierarchy
                    └─> Gets bind transforms
                    └─> Sets bone parenting and lengths
                    └─> set_rest_pose()  // If rest pose differs from bind
                    └─> import_skeleton_curves()  // If import_anim && valid_skeleton
                          └─> skel_query.GetAnimQuery()  // Resolves skel:animationSource
                          └─> anim_query.GetJointTransformTimeSamples()
                          └─> Creates Action, names it after SkelAnimation prim
                          └─> Creates FCurves for each joint
                          └─> Samples animation at each time code
                          └─> Recalculates curve handles
```

---

## 7. Recommendations for USD Exporter

### 7.1 File Structure for Best Blender Compatibility

```
/Root
  /Armature                    (Xform - becomes SkelRoot)
    /Skeleton                  (UsdSkelSkeleton)
      skel:animationSource -> </Root/Armature/Skeleton/Animation>

      /Animation               (UsdSkelAnimation - CHILD of skeleton)
        joints: [...]
        translations: [...] (time-sampled)
        rotations: [...] (time-sampled)
        scales: [...] (time-sampled)

    /SkinnedMesh               (UsdGeomMesh with skel binding)
```

### 7.2 Key Points

1. **Single animation per skeleton**: Place ONE SkelAnimation as a child of the skeleton and bind it via `skel:animationSource`. Blender will ignore any other animations.

2. **Animation prim naming**: The SkelAnimation prim name becomes the Blender Action name. Choose descriptive names.

3. **Use relative paths for animationSource**: Blender's exporter uses relative paths:
   ```cpp
   // usd_writer_armature.cc:115-116
   usd_skel_api.CreateAnimationSourceRel().SetTargets(
       pxr::SdfPathVector({pxr::SdfPath(skel_anim.GetPath().GetName())}));
   ```

4. **Avoid negative scale/mirroring**: Bone matrices with negative determinants will cause animation import to fail.

5. **Store decomposed transforms**: Blender expects translations, rotations (as quaternions), and scales. Using `SetTransforms()` on UsdSkelAnimation will automatically decompose matrices.

6. **Joint ordering must match**: The joint order in the SkelAnimation must match the Skeleton's joint order.

### 7.3 For Multiple Animations Workflow

Since Blender only imports one animation, if you need multiple animations:

1. **Export separate USD files** per animation, each with the animation bound via `skel:animationSource`

2. **Or** modify the USD file's `skel:animationSource` relationship between imports

3. **Or** wait for future Blender versions that may add multi-animation support

### 7.4 Blender Export Structure Reference

When Blender exports, it creates this structure (from `usd_writer_armature.cc`):

```cpp
// Skeleton path: /Root/Armature/Skeleton
// Animation path: /Root/Armature/Skeleton/<ActionName>

pxr::SdfPath anim_path = usd_export_context_.usd_path.AppendChild(anim_name);
skel_anim = pxr::UsdSkelAnimation::Define(stage, anim_path);
```

The animation is always a direct child of the skeleton, and the action name (sanitized) is used as the prim name.

---

## 8. Blend Shape Animation Notes

Blend shape animation follows a similar pattern but uses:
- `GetInheritedAnimationSource()` on the skeleton's binding API
- Falls back to `GetAnimationSource()` if inherited is not available
- Looks for `blendShapeWeights` attribute on the SkelAnimation

**Location:** `usd_skel_convert.cc:555-678`

```cpp
pxr::UsdPrim anim_prim = skel_api.GetInheritedAnimationSource();

if (!anim_prim) {
  skel_api.GetAnimationSource(&anim_prim);
}
```

This means blend shape and skeletal animation should use the **same** SkelAnimation prim for Blender to import both correctly.

---

## File References

| File | Description |
|------|-------------|
| `source/blender/io/usd/intern/usd_skel_convert.cc` | Main skeletal animation import logic |
| `source/blender/io/usd/intern/usd_skel_convert.hh` | Header with function declarations |
| `source/blender/io/usd/intern/usd_reader_skeleton.cc` | Skeleton prim reader |
| `source/blender/io/usd/intern/usd_armature_utils.cc` | FCurve creation utilities |
| `source/blender/io/usd/intern/usd_writer_armature.cc` | Export counterpart (for reference) |
| `source/blender/io/usd/intern/usd_reader_stage.cc` | Stage traversal and reader creation |
| `source/blender/io/usd/usd.hh` | Import/export parameters |
