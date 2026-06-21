---
name: cryengine-inspect
description: Inspect raw CryEngine file data (bones, nodes, materials, geometry, skinning) using the CgfConverterTestingConsole. Use when debugging conversion issues, verifying chunk data, or comparing source data against renderer output.
allowed-tools: Bash, Read, Edit, Write
---

# CryEngine Data Inspection

Use the `CgfConverterTestingConsole` project to inspect parsed CryEngine data. This console is Claude's tool — modify it freely for any inspection need.

## Quick Reference

```bash
# All commands follow this pattern:
dotnet run --project CgfConverterTestingConsole -- "<file>" [--objectdir "<dir>"] <command>

# Available commands:
--dump-bones             # Bone hierarchy, transforms, bind pose matrices
--dump-nodes             # Node tree with transforms and mesh references
--dump-materials         # Material tree with textures and shader params
--dump-geometry [idx]    # Geometry info (no idx = list meshes, with idx = details)
--dump-chunks            # Chunk table summary for all models
--dump-skinning [count]  # Skinning weights (default: first 10 vertices)
--custom                 # Custom inspection code (edit RunCustom() in Program.cs)
```

## Examples

```bash
# Dump bone hierarchy for adder mech
dotnet run --project CgfConverterTestingConsole -- "d:\depot\mwo\Objects\mechs\adder\body\adder.chr" --objectdir "d:\depot\mwo" --dump-bones

# List all mesh nodes in a CGA file
dotnet run --project CgfConverterTestingConsole -- "d:\depot\mwo\Objects\mechs\adder\body\adr_left_leg_calf.cga" --dump-geometry

# Dump skinning weights for first 20 vertices
dotnet run --project CgfConverterTestingConsole -- "d:\depot\mwo\Objects\mechs\adder\body\adder.chr" --dump-skinning 20

# Dump chunk table
dotnet run --project CgfConverterTestingConsole -- "some_file.cgf" --dump-chunks
```

## Filtering Console Noise

CryEngine logs chunk reading to stdout. Filter it out:

```bash
# Pipe through grep to hide reading lines
dotnet run --project CgfConverterTestingConsole -- "<file>" --dump-bones 2>&1 | grep -v "^\["

# Or redirect output to a file for large dumps
dotnet run --project CgfConverterTestingConsole -- "<file>" --dump-bones > /tmp/bones.txt 2>&1
```

## Custom Inspection (Deep Dives)

For one-off investigation, edit `RunCustom()` in `CgfConverterTestingConsole/Program.cs`:

### Step 1: Edit Program.cs

Modify the `RunCustom()` method at the bottom of Program.cs. The `cryData` parameter gives full access to:

```csharp
static void RunCustom(CryEngine cryData)
{
    // Access bone data
    var bones = cryData.SkinningInfo?.CompiledBones;
    
    // Access node hierarchy  
    var nodes = cryData.Nodes;
    var root = cryData.RootNode;
    
    // Access materials
    var materials = cryData.Materials;
    
    // Access specific chunks from models
    var chunkMap = cryData.Models[0].ChunkMap;
    
    // Access geometry through nodes
    foreach (var node in cryData.Nodes ?? [])
    {
        if (node.MeshData?.GeometryInfo is { } geo)
        {
            // geo.Vertices, geo.Indices, geo.Normals, geo.UVs, geo.BoneMappings
        }
    }
}
```

### Step 2: Build and run

```bash
dotnet build CgfConverterTestingConsole
dotnet run --project CgfConverterTestingConsole -- "<file>" --custom
```

### Step 3: Restore when done

After the custom inspection, restore `RunCustom()` to its default placeholder state so it's clean for next use.

## Key Data Traversal Paths

```
CryEngine
├── .SkinningInfo.CompiledBones[]    → bone hierarchy, transforms, bind poses
│   ├── .BoneName, .ControllerID
│   ├── .WorldTransformMatrix        → Matrix3x4 (pos in M14, M24, M34)
│   ├── .LocalTransformMatrix        → Matrix3x4
│   ├── .BindPoseMatrix              → Matrix4x4 (WorldToBone, inverse bind)
│   ├── .ParentBone                  → CompiledBone? (field)
│   └── .ChildIDs                    → List<int> (controller IDs)
├── .Nodes[]                         → node hierarchy
│   ├── .Name, .Transform (Matrix4x4)
│   ├── .ParentNode, .Children[]
│   ├── .MeshData?.GeometryInfo      → vertices, indices, normals, UVs
│   └── .Materials                   → resolved material
├── .Materials                       → Dictionary<string, Material>
│   └── .SubMaterials[], .Textures[], .Shader
├── .Models[]                        → raw parsed files
│   ├── .ChunkMap                    → all chunks by ID
│   ├── .NodeMap                     → ChunkNode chunks only
│   └── .chunkHeaders               → chunk table with ToString()
└── .Bones                           → shortcut to ChunkCompiledBones
```

## Important Notes

- **CryEngine matrix convention**: Translation is in column 4 (M14, M24, M34), NOT row 4
- **BoneToWorld vs WorldToBone**: `WorldTransformMatrix` is B2W (bone space → world). `BindPoseMatrix` is W2B (world → bone, the inverse bind matrix for skinning)
- **--objectdir is important**: Without it, materials won't resolve and you'll get defaults
- The console project is at `CgfConverterTestingConsole/Program.cs` — it's a top-level statements file
- Build errors in Program.cs won't affect the main CgfConverter build
