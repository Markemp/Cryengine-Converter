---
name: chunk-guide
description: Guide for adding new Cryengine chunk types or versions. Use when implementing support for new chunk versions, understanding the chunk system architecture, or debugging chunk parsing issues.
allowed-tools: Read, Glob, Grep, Edit, Write
---

# Chunk System Implementation Guide

This skill guides you through adding new chunk types or versions to the Cryengine Converter.

## Architecture Overview

The chunk system uses **versioned polymorphism via reflection**:

```
Chunk (abstract base)
  └── ChunkMesh (abstract, type-specific properties)
        ├── ChunkMesh_800 (sealed, version-specific Read())
        ├── ChunkMesh_801
        └── ChunkMesh_802
```

### Key Files

| File | Purpose |
|------|---------|
| `CgfConverter/CryEngineCore/Chunks/Chunk.cs` | Base class + factory methods |
| `CgfConverter/Enums/Enums.cs` | `ChunkType` enum (chunk type IDs) |
| `CgfConverter/CryEngineCore/Chunks/Chunk{Name}.cs` | Abstract base for each chunk type |
| `CgfConverter/CryEngineCore/Chunks/Chunk{Name}_{Version}.cs` | Version implementations |

### Factory Pattern

The factory in `Chunk.cs` uses two methods:

1. **`New(ChunkType, uint version)`** - Switch statement mapping `ChunkType` enum to generic factory
2. **`New<T>(uint version)`** - Reflection-based: finds class named `{BaseTypeName}_{version:X}`

Example: `Chunk.New<ChunkMesh>(0x800)` finds `ChunkMesh_800` via reflection.

---

## Adding a New Version of an Existing Chunk Type

**Scenario**: A game file has `ChunkMesh` version `0x803` that isn't supported.

### Step 1: Create the version file

Create `CgfConverter/CryEngineCore/Chunks/ChunkMesh_803.cs`:

```csharp
using Extensions;
using System.IO;

namespace CgfConverter.CryEngineCore;

internal sealed class ChunkMesh_803 : ChunkMesh
{
    public override void Read(BinaryReader b)
    {
        base.Read(b);  // Always call base first - handles header parsing

        // Parse version-specific fields
        // Use b.ReadInt32(), b.ReadVector3(), etc.
    }
}
```

### Step 2: Verify naming convention

The class name MUST follow `{AbstractTypeName}_{VersionHex}`:
- Version `0x800` → `_800`
- Version `0x901` → `_901`
- Version `2048` (decimal) → `_800` (hex)

### Step 3: No registration needed

The reflection-based factory auto-discovers the class. No changes to `Chunk.cs` switch statement needed for existing chunk types.

### Step 4: Write a test

Add a test in `CgfConverterIntegrationTests` using an asset file that contains this version.

---

## Adding a Completely New Chunk Type

**Scenario**: Supporting a new chunk type like `ChunkFoliage`.

### Step 1: Add to ChunkType enum

In `CgfConverter/Enums/Enums.cs`, add the chunk type ID:

```csharp
public enum ChunkType : uint
{
    // ... existing types ...
    Foliage = 0xCCCC0020,  // Use appropriate hex ID from file format
}
```

### Step 2: Create abstract base class

Create `CgfConverter/CryEngineCore/Chunks/ChunkFoliage.cs`:

```csharp
namespace CgfConverter.CryEngineCore;

public abstract class ChunkFoliage : Chunk
{
    // Type-specific properties shared across all versions
    public int BranchCount { get; set; }
    public float WindStrength { get; set; }
}
```

### Step 3: Create version implementation

Create `CgfConverter/CryEngineCore/Chunks/ChunkFoliage_800.cs`:

```csharp
using System.IO;

namespace CgfConverter.CryEngineCore;

internal sealed class ChunkFoliage_800 : ChunkFoliage
{
    public override void Read(BinaryReader b)
    {
        base.Read(b);

        BranchCount = b.ReadInt32();
        WindStrength = b.ReadSingle();
        // ... parse remaining fields
    }
}
```

### Step 4: Register in factory switch statement

In `CgfConverter/CryEngineCore/Chunks/Chunk.cs`, add to `New(ChunkType, uint)`:

```csharp
public static Chunk New(ChunkType chunkType, uint version)
{
    return chunkType switch
    {
        // ... existing cases ...
        ChunkType.Foliage => Chunk.New<ChunkFoliage>(version),
        _ => new ChunkUnknown(),
    };
}
```

### Step 5: Write tests

Create both unit tests (if possible with mock data) and integration tests with real assets.

---

## Reading Binary Data

### Common patterns in Read() methods

```csharp
public override void Read(BinaryReader b)
{
    base.Read(b);  // ALWAYS call first

    // Primitives
    int count = b.ReadInt32();
    uint flags = b.ReadUInt32();
    float value = b.ReadSingle();
    short shortVal = b.ReadInt16();

    // Vectors (from Extensions namespace)
    Vector3 position = b.ReadVector3();
    Vector4 color = b.ReadVector4();
    Matrix4x4 matrix = b.ReadMatrix4x4();
    Quaternion rotation = b.ReadQuaternion();

    // Arrays
    byte[] data = b.ReadBytes(count);

    // Strings (null-terminated)
    string name = b.ReadCString();

    // Skip padding/unknown bytes
    SkipBytes(b, 16);
}
```

### Endianness

The base `Read()` method handles endianness setup:
- `IsBigEndian` property indicates current endianness
- Wii U files use big-endian
- Most PC files are little-endian

### DataSize vs Size

- `Size` = total chunk size including header
- `DataSize` = data portion only (for Star Citizen files without embedded headers)

---

## Debugging Tips

### When chunks fail to parse

1. Check version matches file: Log `Version` property after `base.Read(b)`
2. Verify stream position: `b.BaseStream.Position` should equal `Offset + Size` after reading
3. Use `SkipBytes(b)` to skip remaining unknown data without failing
4. Compare with working version implementations

### When factory throws NotSupportedException

```
Version 803 of ChunkMesh is not supported
```

This means no `ChunkMesh_803.cs` class exists. Create it following the pattern above.

### Inspecting chunk data

Use hex editor or `CgfConverterTestingConsole` project:
1. Set breakpoint in chunk's `Read()` method
2. Examine `b.BaseStream.Position` and raw bytes
3. Compare against working versions

---

## Star Citizen #ivo Specifics

Star Citizen uses different chunk types with different IDs:

- Traditional: `ChunkType.Mesh = 0xCCCC0000`
- Star Citizen: `ChunkType.MeshIvo = 0x9293B9D8`

Both map to `ChunkMesh` in the factory, but may need different version implementations.

Star Citizen files (0x900+ versions):
- No embedded chunk headers in data
- Use `DataSize` instead of `Size - 16`
- May use bounding-box compressed vertices

---

## Checklist

When adding chunk support:

- [ ] Identify chunk type ID (hex) and version from file
- [ ] Check if abstract base class exists; create if new type
- [ ] Create version implementation with correct naming: `Chunk{Type}_{VersionHex}.cs`
- [ ] Register in `Chunk.New()` switch if new chunk type
- [ ] Add to `ChunkType` enum if new type
- [ ] Call `base.Read(b)` first in `Read()` method
- [ ] Handle all bytes in chunk (use `SkipBytes` for unknowns)
- [ ] Write integration test with real asset
- [ ] Test with `-throw` flag to surface parsing errors
