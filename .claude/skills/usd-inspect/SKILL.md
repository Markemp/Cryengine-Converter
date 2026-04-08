---
name: usd-inspect
description: Reference for inspecting USD/USDA/USDC files using OpenUSD CLI tools. Use when you need to query prim hierarchies, attribute values, material bindings, skeleton data, or compare USD outputs.
allowed-tools: Bash, Read
---

# USD Inspection Reference

> **Environment dependency**: This skill requires OpenUSD CLI tools (`usdcat`, `usdtree`, `sdffilter`, `sdfdump`) to be built and on PATH. On this machine they're installed at `D:\USD\install\bin` from the OpenUSD source at `C:\Users\Geoff\Source\Repos\OpenUSD`. These tools won't be available on other machines unless OpenUSD is built and installed there.

OpenUSD CLI tools are installed at `D:\USD\install\bin` and on PATH. Use these instead of writing ad-hoc Python scripts.

## Tools Overview

| Tool | Best For |
|------|----------|
| `usdcat` | Dump full USDA text, extract specific prim subtrees via `--mask` |
| `usdtree` | Quick hierarchy overview, see all prims with optional attributes |
| `sdffilter` | **Primary query tool** — filter by path regex, field regex, time ranges |
| `sdfdump` | Raw layer data with path/field filtering, good for debugging |

---

## Common Recipes

### See the full prim hierarchy
```bash
usdtree file.usda
```

### See hierarchy with all authored attributes
```bash
usdtree -a file.usda
```

### See hierarchy with metadata (kind, active, etc.)
```bash
usdtree -a -m file.usda
```

### Extract a specific prim subtree as USDA
```bash
usdcat --mask /root/skeleton file.usda
```

### Extract multiple prims
```bash
usdcat --mask /root/skeleton,/root/mesh file.usda
```

### Flatten compositions and dump
```bash
usdcat -f file.usda
```

---

## sdffilter — The Query Powerhouse

### Filter by prim path (regex)
```bash
# Everything under /root/skeleton
sdffilter -p "/root/skeleton" --outputType pseudoLayer file.usda

# All prims with "Bip01" in the path
sdffilter -p "Bip01" --outputType pseudoLayer file.usda

# Specific bone
sdffilter -p "/root/skeleton/Bip01_R_Calf$" --outputType pseudoLayer file.usda
```

### Filter by field name (regex)
```bash
# Only jointNames attributes anywhere
sdffilter -f "jointNames" --outputType pseudoLayer file.usda

# Only transform-related fields
sdffilter -f "xformOp" --outputType pseudoLayer file.usda

# Binding-related fields
sdffilter -f "skel:(jointNames|bindTransforms|restTransforms)" --outputType pseudoLayer file.usda
```

### Combine path + field filters
```bash
# Get bind transforms on a specific skeleton
sdffilter -p "/root/skeleton" -f "bindTransforms" --outputType pseudoLayer file.usda

# Get all material bindings on meshes
sdffilter -p "mesh" -f "material:binding" --outputType pseudoLayer file.usda
```

### Output types
```bash
# Quick outline (default) — paths and field names, truncated values
sdffilter --outputType outline file.usda

# pseudoLayer — USDA-like but with truncated arrays (human-readable)
sdffilter --outputType pseudoLayer file.usda

# Full layer — complete USDA, no truncation
sdffilter --outputType layer file.usda

# Summary — high-level stats
sdffilter --outputType summary file.usda

# Validity check
sdffilter --outputType validity file.usda
```

### Control array truncation
```bash
# Show up to 20 elements per array (default is 8 for pseudoLayer)
sdffilter --arraySizeLimit 20 --outputType pseudoLayer file.usda

# Show ALL array elements (careful with large meshes)
sdffilter --arraySizeLimit -1 --outputType pseudoLayer file.usda
```

### Time samples (animation data)
```bash
# Show only frame 0
sdffilter -t 0 --outputType pseudoLayer file.usda

# Show frames 0 through 30
sdffilter -t 0..30 --outputType pseudoLayer file.usda

# Limit timeSamples output
sdffilter --timeSamplesSizeLimit 5 --outputType pseudoLayer file.usda
```

---

## sdfdump — Raw Layer Inspection

```bash
# High-level summary (chunk count, etc.)
sdfdump -s file.usda

# Full dump with all array values
sdfdump --fullArrays file.usda

# Filter to specific path
sdfdump -p "/root/skeleton" file.usda

# Filter to specific field
sdfdump -f "bindTransforms" file.usda

# Validate all data values are readable
sdfdump --validate file.usda
```

---

## Skeleton-Specific Queries

These are the most common queries for debugging armature/skinning issues:

```bash
# Joint names list
sdffilter -f "joints" -p "Skeleton" --outputType pseudoLayer --arraySizeLimit -1 file.usda

# Bind transforms (inverse bind matrices)
sdffilter -f "bindTransforms" -p "Skeleton" --outputType pseudoLayer --arraySizeLimit -1 file.usda

# Rest transforms (rest pose)
sdffilter -f "restTransforms" -p "Skeleton" --outputType pseudoLayer --arraySizeLimit -1 file.usda

# Joint indices and weights on a mesh
sdffilter -f "skel:(jointIndices|jointWeights)" --outputType pseudoLayer --arraySizeLimit 40 file.usda

# Skeleton binding on mesh prims
sdffilter -f "skel:" --outputType pseudoLayer file.usda

# Xform ops on skeleton root
sdffilter -p "Skeleton$" -f "xformOp" --outputType pseudoLayer file.usda
```

## Material Queries

```bash
# All material bindings
sdffilter -f "material:binding" --outputType pseudoLayer file.usda

# Material definitions (shader params, textures)
sdffilter -p "_materials" --outputType pseudoLayer file.usda

# Specific material inputs
sdffilter -p "_materials" -f "inputs:" --outputType pseudoLayer file.usda
```

## Geometry Queries

```bash
# Mesh point counts and extents
sdffilter -f "(points|extent|faceVertexCounts|faceVertexIndices)" --outputType pseudoLayer --arraySizeLimit 10 file.usda

# Full vertex positions (careful — large!)
sdffilter -p "mesh" -f "points" --outputType pseudoLayer --arraySizeLimit -1 file.usda

# Normals
sdffilter -f "normals" --outputType pseudoLayer --arraySizeLimit 10 file.usda

# UV coordinates
sdffilter -f "primvars:st" --outputType pseudoLayer --arraySizeLimit 10 file.usda
```

---

## Tips

- **Pipe to file** for large outputs: `sdffilter ... file.usda > output.txt` then use Read tool
- **Pipe to head** for quick peek: `sdffilter ... file.usda | head -50`
- **pseudoLayer is almost always the right outputType** — it's readable and concise
- **arraySizeLimit** defaults: 0 for outline, 8 for pseudoLayer, -1 for layer
- Path regexes are standard regex — use `$` for exact match, `.*` for wildcards
- These tools work on both `.usda` (text) and `.usdc` (binary crate) files
- For `.usd` files, the tools auto-detect the underlying format
