# USD Skeletal Animation Architecture Research

This document provides research findings for implementing a USD exporter with skeletal animation support. It covers animation binding, multiple animations, and recommended organization patterns.

---

## 1. Multiple Animations Per Skeleton

### Answer: One Animation Source Per Skeleton Instance

The `skel:animationSource` relationship is **single-target only**. A skeleton can only have one active animation at any given time.

### Evidence

**Schema Definition** (`pxr/usd/usdSkel/schema.usda:162-169`):
```usda
rel skel:animationSource (
    customData = {
        string apiName = "animationSource"
    }
    doc = """Animation source to be bound to Skeleton primitives at or
    beneath the location at which this property is defined.
    """
)
```

**Warning for Multiple Targets** (`pxr/usd/usdSkel/bindingAPI.cpp:361-386`):
```cpp
UsdPrim
_GetFirstTargetPrimForRel(const UsdRelationship& rel,
                          const SdfPathVector& targets)
{
    if (targets.size() > 0) {
        if (targets.size() > 1) {
            TF_WARN("%s -- relationship has more than one target. "
                    "Only the first will be used.",
                    rel.GetPath().GetText());
        }
        const SdfPath& target = targets.front();
        // ...
    }
    return UsdPrim();
}
```

This function explicitly warns and discards additional targets if multiple are specified.

### API Signatures

**bindingAPI.h:328-338**:
```cpp
/// Animation source to be bound to Skeleton primitives at or
/// beneath the location at which this property is defined.
USDSKEL_API
UsdRelationship GetAnimationSourceRel() const;

USDSKEL_API
UsdRelationship CreateAnimationSourceRel() const;
```

**bindingAPI.h:440-450** (Query methods):
```cpp
/// Returns true if an animation source binding is defined, and sets
/// \p prim to the target prim.
USDSKEL_API
bool GetAnimationSource(UsdPrim* prim) const;

/// Returns the animation source bound at this prim, or one of its ancestors.
USDSKEL_API
UsdPrim GetInheritedAnimationSource() const;
```

---

## 2. Animation Switching/Blending Support

### Answer: No Native Blending - Use Value Clips for Sequencing

USD does **not** have native animation blending or clip switching within UsdSkel. However:

1. **Value Clips**: The official USD mechanism for animation sequencing
2. **Future Support**: Architecture anticipates compound animation sources

### No SkelAnimationClip Schema

There is no `SkelAnimationClip`, `AnimationLibrary`, or animation stack concept in UsdSkel. The schema only defines:
- `UsdSkelSkeleton` - The skeleton definition
- `UsdSkelAnimation` - A single animation source
- `UsdSkelBindingAPI` - Binds animations to geometry

### Future Animation Blending (Not Yet Implemented)

**From `pxr/usd/usdSkel/doxygen/objectModel.dox:83-87`**:
```
A UsdSkelAnimQuery is created through a UsdSkelCache instance. This is because
we anticipate adding _compound_ animation sources like animation blenders
in the future, and expect that different instances of blenders may reference
many of the same animations, so requiring a UsdSkelAnimQuery to be constructed
through a UsdSkelCache presents an opportunity to share references to queries
internally.
```

### Value Clips: The Official Approach

**From `pxr/usd/usdSkel/doxygen/schemas.dox:14-16`**:
```
Animations can be published individually and referenced back into scenes.
This not only allows re-use, but also enables sequencing of animations as
Value Clips.
```

Value Clips allow you to sequence multiple USD files containing animation data, switching between them over time.

---

## 3. Recommended Organization for Multiple Animations

### Pattern A: Separate Animation Files with Value Clips (Recommended)

Store each animation (walk, run, idle) as a separate `.usda` file with a `SkelAnimation`, then use Value Clips to sequence them.

**Animation File Structure:**
```
/assets/
  character.usda          # Main character with Skeleton
  animations/
    walk.usda             # SkelAnimation for walk
    run.usda              # SkelAnimation for run
    idle.usda             # SkelAnimation for idle
  instances/
    character_walking.usda   # References character + walk clips
    character_running.usda   # References character + run clips
```

**walk.usda:**
```usda
#usda 1.0

def SkelAnimation "WalkAnim" {
    uniform token[] joints = ["Root", "Root/Spine", "Root/Spine/Head", ...]

    float3[] translations.timeSamples = {
        0: [(0,0,0), (0,1,0), ...],
        1: [(0.1,0,0), (0,1,0), ...],
        // ...
    }

    quatf[] rotations.timeSamples = {
        0: [(1,0,0,0), (1,0,0,0), ...],
        1: [(0.99,0.1,0,0), ...],
        // ...
    }

    half3[] scales.timeSamples = {
        0: [(1,1,1), (1,1,1), ...],
    }
}
```

**Sequence with Value Clips:**
```usda
#usda 1.0

def SkelRoot "Character" (
    references = @./character.usda@</Character>
    prepend apiSchemas = ["SkelBindingAPI"]

    clips = {
        dictionary default = {
            asset[] assetPaths = [
                @./animations/idle.usda@,
                @./animations/walk.usda@,
                @./animations/run.usda@
            ]
            string primPath = "/WalkAnim"  # Path within each clip file

            # (stage_time, clip_index) - which clip is active when
            double2[] active = [
                (0, 0),    # idle at frame 0
                (30, 1),   # walk at frame 30
                (60, 2),   # run at frame 60
                (90, 0)    # back to idle at frame 90
            ]

            # (stage_time, clip_time) - time remapping
            double2[] times = [
                (0, 0), (30, 30),      # idle: stage 0-30 maps to clip 0-30
                (30, 0), (60, 30),     # walk: stage 30-60 maps to clip 0-30
                (60, 0), (90, 30),     # run: stage 60-90 maps to clip 0-30
                (90, 0), (120, 30)     # idle: stage 90-120 maps to clip 0-30
            ]
        }
    }
) {
    rel skel:animationSource = </Character/Skeleton/Anim>
    rel skel:skeleton = </Character/Skeleton>
}
```

### Pattern B: Hierarchical Binding Override

Different skeleton instances can use different animations via inheritance override.

**From test `pxr/usd/usdSkel/testenv/testUsdSkelCache/populate.usda`:**
```usda
def SkelRoot "AnimBinding" {
    def Scope "Scope" (prepend apiSchemas = ["SkelBindingAPI"]) {
        rel skel:animationSource = </Anim1>

        def Skeleton "Inherit" {}

        def Skeleton "Override" (prepend apiSchemas = ["SkelBindingAPI"]) {
            rel skel:animationSource = </Anim2>
        }
    }
}
```

- `Skeleton "Inherit"` uses `</Anim1>` (inherited from parent)
- `Skeleton "Override"` uses `</Anim2>` (explicitly overridden)

### Pattern C: Instance-Based Animation

**From `pxr/usd/usdSkel/doxygen/schemas.dox:486-542`:**
```usda
over "A" (prepend apiSchemas = ["SkelBindingAPI"]) {
    rel skel:skeleton = </Skel>
    rel skel:animationSource = </Anim1>

    over "B" (prepend apiSchemas = ["SkelBindingAPI"]) {
        rel skel:animationSource = </Anim2>
    }
    over "C" (prepend apiSchemas = ["SkelBindingAPI"]) {
        rel skel:animationSource = </Anim2>
        rel skel:skeleton = </Skel>
    }
}
```

This creates two skeleton instances:
- One at `</A>` with animation `</Anim1>`
- One at `</A/C>` with animation `</Anim2>`

---

## 4. SkelAnimation Schema Definition

**Full schema from `pxr/usd/usdSkel/schema.usda:92-142`:**

```usda
class SkelAnimation "SkelAnimation" (
    inherits = </Typed>
    doc = """Describes a skel animation, where joint animation is stored in a
    vectorized form."""
    customData = {
        string className = "Animation"
    }
) {
    uniform token[] joints (
        doc = """Array of tokens identifying which joints this animation's
        data applies to. The tokens for joints correspond to the tokens of
        Skeleton primitives. The order of the joints as listed here may
        vary from the order of joints on the Skeleton itself."""
    )

    float3[] translations (
        doc = """Joint-local translations of all affected joints. Array length
        should match the size of the *joints* attribute."""
    )

    quatf[] rotations (
        doc = """Joint-local unit quaternion rotations of all affected joints,
        in 32-bit precision. Array length should match the size of the
        *joints* attribute."""
    )

    half3[] scales (
        doc = """Joint-local scales of all affected joints, in
        16 bit precision. Array length should match the size of the *joints*
        attribute."""
    )

    uniform token[] blendShapes (
         doc = """Array of tokens identifying which blend shapes this
         animation's data applies to."""
    )

    float[] blendShapeWeights (
        doc = """Array of weight values for each blend shape."""
    )
}
```

### Key Characteristics

| Feature | Details |
|---------|---------|
| Sparse Animation | Can animate subset of joints; `joints` array specifies which |
| Joint Order | Can differ from Skeleton's joint order |
| Time-Sampling | `translations`, `rotations`, `scales` support `.timeSamples` |
| Precision | rotations=32-bit float, scales=16-bit half |
| Blend Shapes | Supported via `blendShapes` + `blendShapeWeights` |

---

## 5. Value Clips API Reference

For animation sequencing, use `UsdClipsAPI` from `pxr/usd/usd/clipsAPI.h`.

### Key Metadata

| Metadata | Purpose |
|----------|---------|
| `assetPaths` | Array of clip file paths |
| `primPath` | Path within clips to source data from |
| `active` | `(stage_time, clip_index)` pairs - which clip at what time |
| `times` | `(stage_time, clip_time)` pairs - time remapping |
| `manifestAssetPath` | Optional manifest for optimization |

### Example from `pxr/usd/usd/testenv/testUsdValueClips/multiclip/root.usda`:
```usda
def "Model_1" (
    references = @./ref.usda@</Model>

    clips = {
        dictionary default = {
            asset[] assetPaths = [@./clip1.usda@, @./clip2.usda@]
            string primPath = "/Model"
            double2[] active = [(0.0, 0), (16.0, 1)]
            double2[] times = [(0.0, 0.0), (16.0, 16.0), (16.0, 0.0), (32.0, 16.0)]
        }
    }
)
{
}
```

This sequences `clip1.usda` (frames 0-16) then `clip2.usda` (frames 16-32).

---

## 6. Implementation Recommendations for USD Exporter

### Critical Requirements

1. **Single Animation Per Skeleton Instance**
   - Do NOT attempt multiple targets on `skel:animationSource`
   - Export one `SkelAnimation` per binding location
   - If source has multiple animations, export to separate files

2. **Animation Source Validation**
   ```cpp
   // Validate before export
   if (!UsdSkelIsSkelAnimationPrim(animPrim)) {
       // Invalid animation source
   }
   ```

3. **Array Size Consistency**
   - `joints.size() == translations.size() == rotations.size() == scales.size()`
   - All must match or export will fail silently

4. **Sparse Animation Support**
   - Animation can reference subset of skeleton joints
   - Order doesn't need to match skeleton's joint order
   - UsdSkel handles remapping internally

5. **Joint Order Note**
   - **Skeleton** joints: Must be topologically sorted (parent before children)
   - **Animation** joints: No ordering requirement

### Recommended Export Strategy

```
For each character with multiple animations (walk, run, idle):

1. Export skeleton definition once:
   /Character/Skeleton  (SkelSkeleton with joints, bindTransforms, restTransforms)

2. Export each animation to separate prim or file:
   /Character/Animations/Walk  (SkelAnimation)
   /Character/Animations/Run   (SkelAnimation)
   /Character/Animations/Idle  (SkelAnimation)

3. For single-animation use:
   - Bind directly: rel skel:animationSource = </Character/Animations/Walk>

4. For animation sequencing:
   - Use Value Clips metadata to sequence animation files
   - Each animation in separate .usda file
   - Clips metadata specifies timing
```

### Code Example (Pseudocode)

```cpp
// Create animation prim
UsdSkelAnimation anim = UsdSkelAnimation::Define(stage, SdfPath("/Anim"));

// Set joints (can be sparse subset)
VtTokenArray joints = {"Root", "Root/Spine", "Root/Spine/Head"};
anim.GetJointsAttr().Set(joints);

// Set time-sampled transforms
for (double time : keyframeTimes) {
    VtVec3fArray translations = GetTranslationsAtTime(time);
    VtQuatfArray rotations = GetRotationsAtTime(time);
    VtVec3hArray scales = GetScalesAtTime(time);

    anim.GetTranslationsAttr().Set(translations, UsdTimeCode(time));
    anim.GetRotationsAttr().Set(rotations, UsdTimeCode(time));
    anim.GetScalesAttr().Set(scales, UsdTimeCode(time));
}

// Bind animation to skeleton
UsdSkelBindingAPI binding = UsdSkelBindingAPI::Apply(skelRootPrim);
binding.CreateAnimationSourceRel().SetTargets({anim.GetPath()});
```

---

## 7. Limitations and Caveats

### Current Limitations

1. **No Native Blending**: Cannot blend walk+run animations simultaneously
2. **Single Active Animation**: Only one animation per skeleton instance at runtime
3. **No Animation Layers**: Unlike Maya/Blender, no layered animation system
4. **Value Clips Overhead**: Runtime clip switching has performance implications

### Workarounds

| Need | Solution |
|------|----------|
| Multiple animations | Separate files + Value Clips |
| Blending | Pre-bake blended result, or custom runtime code |
| Animation layers | Flatten to single animation on export |
| Runtime switching | Value Clips or application-level logic |

### Future Considerations

The UsdSkel team anticipates adding "compound animation sources" including blenders. The current architecture (UsdSkelAnimQuery through UsdSkelCache) is designed to support this future expansion without breaking existing code.

---

## 8. File References

| File | Content |
|------|---------|
| `pxr/usd/usdSkel/schema.usda` | Schema definitions for Skeleton, Animation |
| `pxr/usd/usdSkel/bindingAPI.h/cpp` | Animation binding API |
| `pxr/usd/usdSkel/animQuery.h` | Animation query interface |
| `pxr/usd/usdSkel/doxygen/schemaOverview.dox` | Architecture documentation |
| `pxr/usd/usdSkel/doxygen/schemas.dox` | Detailed schema docs |
| `pxr/usd/usdSkel/doxygen/objectModel.dox` | Object model & future plans |
| `pxr/usd/usd/clipsAPI.h` | Value Clips API |
| `pxr/usd/usdSkel/testenv/testUsdSkelCache/populate.usda` | Binding examples |
| `pxr/usd/usd/testenv/testUsdValueClips/multiclip/root.usda` | Clip sequencing example |

---

## Summary

- **One animation per skeleton instance** - `skel:animationSource` is single-target
- **Use Value Clips for sequencing** - Official USD approach for walk/run/idle switching
- **Export animations to separate files** - Enables reuse and Value Clips composition
- **No native blending** - Pre-bake or handle at application level
- **Sparse animations supported** - Can animate subset of joints in any order
