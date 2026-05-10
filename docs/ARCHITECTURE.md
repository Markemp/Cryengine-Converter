# CgfConverter Architecture

## Overview

CgfConverter currently converts CryEngine / Lumberyard-style assets into more portable 3D formats. As implemented, it reads model, material, texture, animation-related, and terrain data from game-like asset layouts, including Star Citizen formats where supported, and writes output through renderer-specific export paths.

## Project layout

| Project | Role | Target / notes |
| --- | --- | --- |
| `cgf-converter` | Command-line executable and conversion entry point. | .NET executable; references the `CgfConverter` library. |
| `CgfConverter` | Core parser, model representation, material handling, texture handling, pack-file access, and renderers. | .NET library; contains most implementation code. |
| `CgfConverterIntegrationTests` | Unit and integration tests for parsing, conversion, math utilities, XML handling, and renderer output. | Integration tests may require local game files that are not included in the repository. |
| `CgfConverterTestingConsole` | Ad hoc developer console for local parser experiments. | Uses local paths and is not the public CLI. |

## Data flow

```text
Input files
  -> CLI argument parsing
  -> CryEngine orchestration
  -> Model / chunk parsing
  -> material and texture resolution
  -> renderer
  -> output files
```

Renderer families currently include Collada, glTF / GLB, Wavefront OBJ, and USD-related code. Existing README text marks OBJ as no longer supported, so it should be treated as deprecated. Existing README text also describes USD as work in progress.

## Key types

| Type | Namespace / path | Role |
| --- | --- | --- |
| `CryEngine` | `CgfConverter/CryEngine/CryEngine.cs` | Orchestrates model loading, sidecar discovery, material creation, node structure, skinning, and animation discovery for model files. |
| `Model` | `CgfConverter/CryEngineCore/Model.cs` | Represents one parsed model or animation container and owns the chunk map for that file. |
| `Chunk` | `CgfConverter/CryEngineCore/Chunks/Chunk.cs` | Base type and factory for versioned chunk readers. |
| `IRenderer` | `CgfConverter/Renderers/IRenderer.cs` | Renderer interface used by output implementations. |
| `ArgsHandler` | `CgfConverter/Services/ArgsHandler.cs` | Current CLI argument parser and holder for conversion options. |
| `CascadedPackFileSystem` | `CgfConverter/PackFileSystem/CascadedPackFileSystem.cs` | Provides layered file lookup over real directories and supported pack-file sources. |
| `ColladaModelRenderer` | `CgfConverter/Renderers/Collada/ColladaModelRenderer.cs` | Writes Collada model output. |
| `GltfModelRenderer` | `CgfConverter/Renderers/Gltf/GltfModelRenderer.cs` | Writes glTF / GLB model output. |

## Chunk system

CryEngine model files are organized as chunks. A chunk is a typed binary record such as mesh data, node hierarchy data, material name data, skinning data, animation controller data, or a game-specific variant of those structures.

Version-specific chunk classes exist because the same conceptual chunk can have different binary layouts across engine versions, games, and Star Citizen format updates. The `Chunk` factory chooses an implementation based on chunk type and version, and falls back to `ChunkUnknown` for unrecognized chunk types.

Chunk classes live under `CgfConverter/CryEngineCore/Chunks`. Shared chunk identifiers and related enums live in `CgfConverter/Enums/Enums.cs`. When an asset format changes, these files are usually the first place to inspect.

## Renderers

Renderers translate the parsed `CryEngine` or terrain data into output formats. The command-line entry point selects renderer implementations based on parsed arguments and then calls `IRenderer.Render()`.

`IRenderer` is the current extension point for output formats. Collada is described in the existing README as the default and most feature-complete path. glTF / GLB output is supported as implemented in the CLI and renderer code.

## CLI parsing

`ArgsHandler` is the current CLI parsing location. This document intentionally does not provide a full CLI reference; a focused CLI reference would be better handled in a later documentation PR.

## Testing

The repository contains unit tests and integration tests. Unit tests cover utility behavior and parser support that can run without a local game asset depot. Integration tests may require local game assets or Star Citizen installation/extraction files; `CONTRIBUTING.md` notes that those files are not included in the repository.

Verified unit-test command:

```powershell
dotnet test --filter TestCategory=unit
```

## Areas under active development

As of this repository state, existing docs and code comments indicate continued work around animation export, including CAF-related support. Existing README text also identifies USD output as work in progress.

## Build output notes

Local build output may include warnings such as an ImageSharp vulnerability advisory and a Microsoft.XmlSerializer.Generator / SGEN warning. These warnings were observed during local build verification and do not prevent the solution from building.
