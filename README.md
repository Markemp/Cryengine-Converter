<img src="https://www.heffaypresents.com/images/logos/logo-50px-prod.png" align=right alt="Heffay Presents" width="64px" height="64px">

# Cryengine Converter

[Cryengine Converter](https://www.heffaypresents.com/GitHub) is a C# command-line tool that converts Cryengine game assets — `.cgf`, `.cga`, `.chr`, `.skin`, animation files — into portable 3D formats. It supports the major Cryengine variants, including traditional Cryengine games (MechWarrior Online, Crysis, Hunt: Showdown, Kingdom Come Deliverance, ArcheAge) and Star Citizen's proprietary `#ivo` binary format.

## What's new in v2

- **USD is the new default output format** — Universal Scene Description (`.usda` / `.usdc`). Blender imports it natively with materials, skeletons, and animations intact.
- **glTF and GLB** are first-class outputs alongside Collada.
- **Star Citizen `#ivo` format support** — This app only supports the latest versions (currently 4.5+) as the game files change frequently while it's in 'alpha'.
- **Animations** — full support for `.caf`, `.dba`, and `.cal` animation files. Multi-clip assets export as separate files for Blender's NLA editor.
- **Texture pipeline** — DDS combine for split SC textures (`-unsplittextures`, or `-ut` for short), optional conversion to PNG/TIFF/TGA for glTF.
- **Multiple UV layers, vertex colors, full skeletal rigs** all export correctly.

## Installation

1. Grab the latest release from the [Releases page](https://github.com/Markemp/Cryengine-Converter/releases) — `cgf-converter.exe` is a single self-contained Windows executable (~120 MB). No .NET install required.
2. Drop it into a folder that's on your `PATH` (e.g. `D:\scripts\`).  Spend the time to set up the path; it's worth it for the convenience of running `cgf-converter` from any directory.
3. Recommended: use [Windows Terminal](https://aka.ms/terminal) with a PowerShell tab for the best command-line experience.

## Quick start

You'll need a Cryengine game with the `.pak` files extracted (use [7-Zip](https://www.7-zip.org/) — right-click → "7-Zip → Extract Here"). After extracting, you'll have a directory tree with `Objects/`, `Textures/`, `Materials/` at the top — we'll call this the **data directory**.

```powershell
PS> cgf-converter <asset-file> -objectdir <path-to-data-directory>
```

Concrete example, converting an Adder mech body part from MechWarrior Online:

```powershell
PS D:\depot\mwo\Objects\mechs\adder\body> cgf-converter adder_torso.cga -objectdir D:\depot\mwo
```

This produces `adder_torso.usda` next to the source file, ready to import into Blender via **File → Import → Universal Scene Description**.

> **The single most important argument is `-objectdir`.** Cryengine assets reference materials and textures using paths *relative to the data directory*. Without `-objectdir`, the converter can't find them and falls back to default materials. Pass it on every conversion.

## Output formats

| Format    | Flag      | When to use                                                                 |
|-----------|-----------|-----------------------------------------------------------------------------|
| **USD**   | `-usd`    | **Default. Recommended for Blender.** Materials, skeletons, animations.     |
| glTF      | `-gltf`   | Game engines (Unity, Unreal, Godot), web viewers. Folder of files.          |
| GLB       | `-glb`    | Same as glTF, single self-contained file with embedded textures.            |
| Collada   | `-dae`    | Maya / 3DS Max workflows, anything that won't take USD.                     |
| Wavefront | `-obj`    | **Deprecated.** No skinning, no animation, limited materials. Avoid.        |

**Recommendation:** start with USD. Switch to something else only if you hit a specific problem.

## Common workflows

### Convert all assets in a directory

```powershell
PS> cgf-converter *.cga -objectdir D:\depot\mwo
```

### Recursive conversion across a subtree

```powershell
PS> Get-ChildItem -Recurse -Include *.cga,*.cgf,*.chr,*.skin |
      ForEach-Object { cgf-converter $_.FullName -objectdir D:\depot\mwo }
```

> **Heads up:** running this on the entire `Objects` folder of a real Cryengine game will produce tens of gigabytes of output and take an hour or more. Pick the subdirectory you actually need.

### Convert a skeletal asset with animations

```powershell
PS> cgf-converter boar.chr -anim -objectdir D:\depot\KCD2
```

The `-anim` flag pulls in `.chrparams`, `.dba`, `.caf`, and `.cal` animation files. Multi-clip assets emit one USD file per animation, which can be loaded as separate actions in Blender's NLA editor.

### Star Citizen — combining split DDS textures

Star Citizen ships its DDS textures split across multiple files (`*.dds.0`, `*.dds.1`, ...). Pass `-unsplittextures` to combine them before materials are written:

```powershell
PS> cgf-converter AEGS_Avenger.cga -unsplittextures -objectdir D:\depot\SC4.6\Data
```

## Full CLI reference

```
cgf-converter [-usage] | <.cgf file> [options]

Required-ish:
  <input-file>          .cgf, .cga, .chr, .skin, .anim, .dba (wildcards supported)
  -objectdir <path>     Path to the extracted game's data directory (highly recommended)
  -mtl/-mat <file>      Override material file resolution

Output formats:
  -usd / -usda          USD (default)
  -dae                  Collada
  -gltf                 glTF (text + .bin)
  -glb                  glTF binary, single file with embedded textures
  -obj                  Wavefront (deprecated, not supported)

Texture options:
  -notex                Don't include textures in output
  -ut / -unsplittextures Combine split SC DDS files
  -png / -tif / -tga    Reference converted texture format (glTF text only)
  -embedtextures        Embed textures into glTF text output

Filtering:
  -en / -excludenode <regex>     Exclude matching nodes
  -em / -excludemat <regex>      Exclude meshes with matching materials
  -sm / -excludeshader <regex>   Exclude meshes by shader name

Animation:
  -anim / -animations   Include .caf/.dba/.cal animation data

Other:
  -noconflict           Append _out to output filenames
  -pp / -preservepath   Preserve directory hierarchy in output
  -mt / -maxthreads <n> Limit thread count (0 = all cores)
  -loglevel <level>     verbose | debug | info | warn | error | critical | none
```

Run `cgf-converter -usage` for the always-current list.

## Tutorial videos

A new v2 tutorial series is in production. The earlier 2017 series uses an older version of the tool (Collada-only, no animation, no `#ivo` support) and shouldn't be used as a guide for v2 workflows — but the asset extraction and Blender setup parts are still relevant:

- [Original Part 1: Converting CryEngine Files](https://www.youtube.com/watch?v=6WoA2ubTZ0k) (2017)
- [Original Part 2: Bulk Convert and Import Mechs / Prefabs](https://www.youtube.com/watch?v=oBJzNdzFIxM) (2017)

## Reporting bugs and contributing

- File issues at [github.com/Markemp/Cryengine-Converter/issues](https://github.com/Markemp/Cryengine-Converter/issues).
- Pull requests welcome — see [CONTRIBUTING.md](CONTRIBUTING.md) and [DEVNOTES.md](DEVNOTES.md).
- Project structure and architecture are documented in [CLAUDE.md](CLAUDE.md).

## License

See repository for license details.
