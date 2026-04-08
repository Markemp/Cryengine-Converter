using CgfConverter;
using CgfConverter.CryEngineCore;
using CgfConverter.Models;
using CgfConverter.Models.Materials;
using CgfConverter.Models.Structs;
using System.Numerics;

if (args.Length < 2)
{
    PrintUsage();
    return 1;
}

string filePath = args[0];
string? objectDir = null;
string? command = null;
int commandIndex = -1;

// Parse all args: first arg is file, then scan for --objectdir and the command
for (int i = 1; i < args.Length; i++)
{
    string arg = args[i].ToLowerInvariant();
    if (arg == "--objectdir" && i + 1 < args.Length)
    {
        objectDir = args[++i];
    }
    else if (arg.StartsWith("--dump-") || arg == "--custom")
    {
        command = arg;
        commandIndex = i;
    }
}

if (command is null)
{
    Console.Error.WriteLine("No command specified.");
    PrintUsage();
    return 1;
}

if (!File.Exists(filePath))
{
    Console.Error.WriteLine($"File not found: {filePath}");
    return 1;
}

// Parse and process the file
Args argsHandler = new();
var options = objectDir is not null
    ? new CryEngineOptions(ObjectDir: objectDir)
    : null;

CryEngine cryData = new(filePath, argsHandler.PackFileSystem, options);
cryData.ProcessCryengineFiles();

switch (command)
{
    case "--dump-bones":
        DumpBones(cryData);
        break;
    case "--dump-nodes":
        DumpNodes(cryData);
        break;
    case "--dump-materials":
        DumpMaterials(cryData);
        break;
    case "--dump-geometry":
        int nodeIndex = commandIndex + 1 < args.Length && int.TryParse(args[commandIndex + 1], out var idx) ? idx : -1;
        DumpGeometry(cryData, nodeIndex);
        break;
    case "--dump-chunks":
        DumpChunks(cryData);
        break;
    case "--dump-skinning":
        int vertCount = commandIndex + 1 < args.Length && int.TryParse(args[commandIndex + 1], out var vc) ? vc : 10;
        DumpSkinning(cryData, vertCount);
        break;
    case "--custom":
        RunCustom(cryData);
        break;
    default:
        Console.Error.WriteLine($"Unknown command: {command}");
        PrintUsage();
        return 1;
}

return 0;

// ─── Commands ───────────────────────────────────────────────────────────────

static void DumpBones(CryEngine cryData)
{
    var skinning = cryData.SkinningInfo;
    if (skinning?.CompiledBones is null || skinning.CompiledBones.Count == 0)
    {
        Console.WriteLine("No bones found.");
        return;
    }

    Console.WriteLine($"=== BONES ({skinning.CompiledBones.Count}) ===\n");
    Console.WriteLine($"{"Idx",-4} {"Name",-40} {"Parent",-30} {"CtrlID",10} {"Children",8}");
    Console.WriteLine(new string('-', 96));

    for (int i = 0; i < skinning.CompiledBones.Count; i++)
    {
        var bone = skinning.CompiledBones[i];
        string parentName = bone.ParentBone?.BoneName ?? "(root)";
        Console.WriteLine($"{i,-4} {bone.BoneName,-40} {parentName,-30} {bone.ControllerID,10:X8} {bone.ChildIDs.Count,8}");
    }

    Console.WriteLine($"\n=== BONE TRANSFORMS ===\n");
    Console.WriteLine($"{"Name",-40} {"World Pos (X, Y, Z)",-45} {"Local Pos (X, Y, Z)"}");
    Console.WriteLine(new string('-', 110));

    foreach (var bone in skinning.CompiledBones)
    {
        var w = bone.WorldTransformMatrix;
        var l = bone.LocalTransformMatrix;
        Console.WriteLine($"{bone.BoneName,-40} ({w.M14,10:F4}, {w.M24,10:F4}, {w.M34,10:F4})    ({l.M14,10:F4}, {l.M24,10:F4}, {l.M34,10:F4})");
    }

    Console.WriteLine($"\n=== BIND POSE MATRICES ===\n");
    foreach (var bone in skinning.CompiledBones)
    {
        var b = bone.BindPoseMatrix;
        Console.WriteLine($"{bone.BoneName}:");
        Console.WriteLine($"  [{b.M11,10:F5} {b.M12,10:F5} {b.M13,10:F5} {b.M14,10:F5}]");
        Console.WriteLine($"  [{b.M21,10:F5} {b.M22,10:F5} {b.M23,10:F5} {b.M24,10:F5}]");
        Console.WriteLine($"  [{b.M31,10:F5} {b.M32,10:F5} {b.M33,10:F5} {b.M34,10:F5}]");
        Console.WriteLine($"  [{b.M41,10:F5} {b.M42,10:F5} {b.M43,10:F5} {b.M44,10:F5}]");
    }

    Console.WriteLine($"\n=== BONE HIERARCHY (tree) ===\n");
    var rootBone = skinning.RootBone;
    if (rootBone is not null)
        PrintBoneTree(skinning, rootBone, 0);
}

static void PrintBoneTree(SkinningInfo skinning, CompiledBone bone, int depth)
{
    string indent = new string(' ', depth * 2);
    string prefix = depth == 0 ? "" : "|- ";
    var w = bone.WorldTransformMatrix;
    Console.WriteLine($"{indent}{prefix}{bone.BoneName}  [pos: ({w.M14:F3}, {w.M24:F3}, {w.M34:F3})]");

    foreach (var child in skinning.GetChildBones(bone))
    {
        PrintBoneTree(skinning, child, depth + 1);
    }
}

static void DumpNodes(CryEngine cryData)
{
    if (cryData.Nodes is null || cryData.Nodes.Count == 0)
    {
        Console.WriteLine("No nodes found.");
        return;
    }

    Console.WriteLine($"=== NODES ({cryData.Nodes.Count}) ===\n");

    foreach (var node in cryData.Nodes)
    {
        string parentName = node.ParentNode?.Name ?? "(root)";
        string meshInfo = node.MeshData?.GeometryInfo is not null
            ? $"verts={node.MeshData.NumVertices}, idx={node.MeshData.NumIndices}"
            : "no mesh";
        string matInfo = node.Materials?.Name ?? "no material";

        Console.WriteLine($"Node: {node.Name}");
        Console.WriteLine($"  Parent: {parentName}  |  Children: {node.Children?.Count ?? 0}");
        Console.WriteLine($"  Mesh: {meshInfo}");
        Console.WriteLine($"  Material: {matInfo}  |  MaterialID: {node.MaterialID}");
        Console.WriteLine($"  ObjectNodeID: {node.ObjectNodeID}");

        var t = node.Transform;
        Console.WriteLine($"  Transform:");
        Console.WriteLine($"    [{t.M11,10:F5} {t.M12,10:F5} {t.M13,10:F5} {t.M14,10:F5}]");
        Console.WriteLine($"    [{t.M21,10:F5} {t.M22,10:F5} {t.M23,10:F5} {t.M24,10:F5}]");
        Console.WriteLine($"    [{t.M31,10:F5} {t.M32,10:F5} {t.M33,10:F5} {t.M34,10:F5}]");
        Console.WriteLine($"    [{t.M41,10:F5} {t.M42,10:F5} {t.M43,10:F5} {t.M44,10:F5}]");
        Console.WriteLine();
    }
}

static void DumpMaterials(CryEngine cryData)
{
    if (cryData.Materials is null || cryData.Materials.Count == 0)
    {
        Console.WriteLine("No materials found.");
        return;
    }

    Console.WriteLine($"=== MATERIALS ({cryData.Materials.Count} files) ===\n");

    foreach (var (name, material) in cryData.Materials)
    {
        Console.WriteLine($"Material File: {name}");
        PrintMaterial(material, 1);
        Console.WriteLine();
    }
}

static void PrintMaterial(Material mat, int depth)
{
    string indent = new string(' ', depth * 2);
    Console.WriteLine($"{indent}Name: {mat.Name ?? "(unnamed)"}");
    Console.WriteLine($"{indent}Shader: {mat.Shader ?? "none"}");

    if (mat.Textures is not null)
    {
        foreach (var tex in mat.Textures)
        {
            Console.WriteLine($"{indent}  Tex [{tex.Map}]: {tex.File}");
        }
    }

    if (mat.DiffuseValue is not null)
        Console.WriteLine($"{indent}  Diffuse: ({mat.DiffuseValue.Red:F3}, {mat.DiffuseValue.Green:F3}, {mat.DiffuseValue.Blue:F3})");
    if (mat.SpecularValue is not null)
        Console.WriteLine($"{indent}  Specular: ({mat.SpecularValue.Red:F3}, {mat.SpecularValue.Green:F3}, {mat.SpecularValue.Blue:F3})");
    if (mat.OpacityValue is not null)
        Console.WriteLine($"{indent}  Opacity: {mat.OpacityValue:F3}");

    if (mat.SubMaterials is not null)
    {
        Console.WriteLine($"{indent}  SubMaterials ({mat.SubMaterials.Length}):");
        for (int i = 0; i < mat.SubMaterials.Length; i++)
        {
            Console.WriteLine($"{indent}    [{i}]:");
            PrintMaterial(mat.SubMaterials[i], depth + 3);
        }
    }
}

static void DumpGeometry(CryEngine cryData, int nodeIndex)
{
    var meshNodes = cryData.Nodes?
        .Where(n => n.MeshData?.GeometryInfo is not null)
        .ToList() ?? [];

    if (meshNodes.Count == 0)
    {
        Console.WriteLine("No geometry found.");
        return;
    }

    if (nodeIndex == -1)
    {
        // List all mesh nodes
        Console.WriteLine($"=== MESH NODES ({meshNodes.Count}) ===\n");
        for (int i = 0; i < meshNodes.Count; i++)
        {
            var n = meshNodes[i];
            var geo = n.MeshData!.GeometryInfo!;
            Console.WriteLine($"  [{i}] {n.Name}: {geo.Vertices?.NumElements ?? 0} verts, {geo.Indices?.NumElements ?? 0} indices, {geo.GeometrySubsets?.Count ?? 0} subsets");
        }
        Console.WriteLine("\nUse --dump-geometry <index> to see details for a specific mesh.");
        return;
    }

    if (nodeIndex >= meshNodes.Count)
    {
        Console.Error.WriteLine($"Node index {nodeIndex} out of range (0-{meshNodes.Count - 1})");
        return;
    }

    var node = meshNodes[nodeIndex];
    var g = node.MeshData!.GeometryInfo!;

    Console.WriteLine($"=== GEOMETRY: {node.Name} ===\n");
    Console.WriteLine($"Vertices: {g.Vertices?.NumElements ?? 0}");
    Console.WriteLine($"Indices: {g.Indices?.NumElements ?? 0}");
    Console.WriteLine($"Normals: {g.Normals?.NumElements ?? 0}");
    Console.WriteLine($"UVs: {g.UVs?.NumElements ?? 0}");
    Console.WriteLine($"Colors: {g.Colors?.NumElements ?? 0}");
    Console.WriteLine($"BoneMappings: {g.BoneMappings?.NumElements ?? 0}");
    Console.WriteLine($"BoundingBox: min=({g.BoundingBox.Min.X:F3},{g.BoundingBox.Min.Y:F3},{g.BoundingBox.Min.Z:F3}) max=({g.BoundingBox.Max.X:F3},{g.BoundingBox.Max.Y:F3},{g.BoundingBox.Max.Z:F3})");

    if (g.GeometrySubsets is not null)
    {
        Console.WriteLine($"\n  Subsets ({g.GeometrySubsets.Count}):");
        foreach (var sub in g.GeometrySubsets)
        {
            Console.WriteLine($"    MatID={sub.MatID}, FirstIdx={sub.FirstIndex}, NumIdx={sub.NumIndices}, FirstVert={sub.FirstVertex}, NumVerts={sub.NumVertices}");
        }
    }

    // Print first 10 vertices as sample
    if (g.Vertices?.Data is not null)
    {
        int sampleCount = Math.Min(10, g.Vertices.Data.Length);
        Console.WriteLine($"\n  First {sampleCount} vertices:");
        for (int i = 0; i < sampleCount; i++)
        {
            var v = g.Vertices.Data[i];
            Console.WriteLine($"    [{i}] ({v.X:F5}, {v.Y:F5}, {v.Z:F5})");
        }
    }
}

static void DumpChunks(CryEngine cryData)
{
    Console.WriteLine($"=== MODELS ({cryData.Models.Count}) ===\n");

    for (int m = 0; m < cryData.Models.Count; m++)
    {
        var model = cryData.Models[m];
        Console.WriteLine($"Model[{m}]: {model.FileName}");
        Console.WriteLine($"  Signature: {model.FileSignature}  Version: {model.FileVersion}  Chunks: {model.NumChunks}");
        Console.WriteLine($"  HasBones: {model.HasBones}  HasGeometry: {model.HasGeometry}  IsIvo: {model.IsIvoFile}");
        Console.WriteLine();

        // Use chunk headers which have ToString() with all the info
        Console.WriteLine($"  Chunk Headers:");
        foreach (var header in model.chunkHeaders)
        {
            Console.WriteLine($"   {header}");
        }

        Console.WriteLine();
    }
}

static void DumpSkinning(CryEngine cryData, int vertCount)
{
    var skinning = cryData.SkinningInfo;
    if (skinning is null || !skinning.HasSkinningInfo)
    {
        Console.WriteLine("No skinning info found.");
        return;
    }

    Console.WriteLine($"=== SKINNING INFO ===\n");
    Console.WriteLine($"Bones: {skinning.CompiledBones?.Count ?? 0}");
    Console.WriteLine($"BoneMappings: {skinning.BoneMappings?.Count ?? 0}");
    Console.WriteLine($"Ext2IntMap: {skinning.Ext2IntMap?.Count ?? 0}");
    Console.WriteLine($"IntVertices: {skinning.IntVertices?.Count ?? 0}");

    if (skinning.BoneMappings is not null && skinning.BoneMappings.Count > 0)
    {
        int count = Math.Min(vertCount, skinning.BoneMappings.Count);
        Console.WriteLine($"\n  First {count} vertex bone mappings:");
        for (int i = 0; i < count; i++)
        {
            var mapping = skinning.BoneMappings[i];
            Console.Write($"    [{i}] ");
            for (int w = 0; w < mapping.BoneIndex.Length; w++)
            {
                if (mapping.Weight[w] > 0)
                {
                    string boneName = skinning.CompiledBones is not null && mapping.BoneIndex[w] < skinning.CompiledBones.Count
                        ? skinning.CompiledBones[mapping.BoneIndex[w]].BoneName ?? "?"
                        : $"bone#{mapping.BoneIndex[w]}";
                    Console.Write($"{boneName}={mapping.Weight[w]:F3} ");
                }
            }
            Console.WriteLine();
        }
    }

    if (skinning.Ext2IntMap is not null && skinning.Ext2IntMap.Count > 0)
    {
        int count = Math.Min(vertCount, skinning.Ext2IntMap.Count);
        Console.WriteLine($"\n  First {count} ext-to-int mappings:");
        for (int i = 0; i < count; i++)
        {
            Console.WriteLine($"    ext[{i}] -> int[{skinning.Ext2IntMap[i]}]");
        }
    }
}

static void RunCustom(CryEngine cryData)
{
    // ─── Custom inspection code goes here ───
    // Modify this method to inspect specific data during debugging.
    // This is the escape hatch for one-off deep dives.

    Console.WriteLine("Custom inspection mode. Edit RunCustom() in Program.cs for your needs.");
    Console.WriteLine($"File: {cryData.InputFile}");
    Console.WriteLine($"Models: {cryData.Models.Count}");
    Console.WriteLine($"Nodes: {cryData.Nodes?.Count ?? 0}");
    Console.WriteLine($"Has Bones: {cryData.Bones is not null}");
    Console.WriteLine($"Has Skinning: {cryData.SkinningInfo?.HasSkinningInfo ?? false}");
}

static void PrintUsage()
{
    Console.WriteLine("""
        CgfConverterTestingConsole - CryEngine file inspector

        Usage: CgfConverterTestingConsole <file> [--objectdir <dir>] <command>

        Commands:
          --dump-bones             Print bone hierarchy with transforms
          --dump-nodes             Print node tree with transforms and mesh refs
          --dump-materials         Print material tree with textures
          --dump-geometry [idx]    Print geometry info (no idx = list meshes)
          --dump-chunks            Print chunk table summary for all models
          --dump-skinning [count]  Print skinning weights (default: first 10 verts)
          --custom                 Run custom inspection code (edit RunCustom())
        """);
}
