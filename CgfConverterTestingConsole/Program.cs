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
    else if (arg.StartsWith("--dump-") || arg == "--custom" || arg == "--ivo-raw")
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

// --ivo-raw runs without going through CryEngine processing — walks the IVO
// container and DB_DATA blocks directly against StarBreaker whitepaper layout.
if (command == "--ivo-raw")
{
    DumpIvoRaw(filePath);
    return 0;
}

// Parse and process the file
Args argsHandler = new();
var options = objectDir is not null
    ? new CryEngineOptions(ObjectDir: objectDir, IncludeAnimations: true)
    : new CryEngineOptions(IncludeAnimations: true);

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
    // Inspect raw timing chunks and controller data from the direct model
    Console.WriteLine("=== Raw Timing and Controller Data ===\n");
    foreach (var model in cryData.Models)
    {
        Console.WriteLine($"Model: {model.FileName}");
        var timingChunks = model.ChunkMap.Values.OfType<ChunkTimingFormat>().ToList();
        foreach (var tc in timingChunks)
        {
            Console.WriteLine($"  Timing: SecsPerTick={tc.SecsPerTick}, TicksPerFrame={tc.TicksPerFrame}");
            Console.WriteLine($"  GlobalRange: Start={tc.GlobalRange.Start}, End={tc.GlobalRange.End}");
            if (tc.TicksPerFrame > 0)
            {
                int startFrame = tc.GlobalRange.Start / tc.TicksPerFrame;
                int endFrame = tc.GlobalRange.End / tc.TicksPerFrame;
                Console.WriteLine($"  Calculated frame range: [{startFrame}..{endFrame}] ({endFrame - startFrame + 1} frames)");
            }
        }
        Console.WriteLine();
    }

    var bones = cryData.SkinningInfo?.CompiledBones;
    if (bones is null) { Console.WriteLine("No bones"); return; }

    Console.WriteLine("=== Bone Hierarchy with Rest Local Transforms ===");
    Console.WriteLine($"{"Bone",-45} {"Parent",-45} {"Rest Local Translation (X, Y, Z)"}");
    Console.WriteLine(new string('-', 130));

    foreach (var bone in bones)
    {
        var parentName = bone.ParentBone?.BoneName ?? "(ROOT)";
        Matrix4x4 restLocal;
        if (bone.ParentBone is null)
        {
            Matrix4x4.Invert(bone.BindPoseMatrix, out restLocal);
        }
        else
        {
            Matrix4x4.Invert(bone.BindPoseMatrix, out var childWorld);
            restLocal = bone.ParentBone.BindPoseMatrix * childWorld;
        }
        // CryEngine: translation in column 4 (M14, M24, M34)
        // Also decompose rotation
        var transposed = Matrix4x4.Transpose(restLocal);
        Matrix4x4.Decompose(transposed, out _, out var restRot, out _);
        Console.WriteLine($"  {bone.BoneName,-45} {parentName,-45} pos=({restLocal.M14,10:F6}, {restLocal.M24,10:F6}, {restLocal.M34,10:F6})  rot=({restRot.X:F6}, {restRot.Y:F6}, {restRot.Z:F6}, {restRot.W:F6})");
    }

    // Show DBA animation data from chunks
    Console.WriteLine("\n=== DBA Animations ===");
    if (cryData.Animations is null || cryData.Animations.Count == 0)
    {
        Console.WriteLine("  No DBA animations loaded.");
    }
    else
    {
        Console.WriteLine($"  {cryData.Animations.Count} DBA model(s) loaded.");
        foreach (var dbaModel in cryData.Animations)
        {
            Console.WriteLine($"\n  DBA: {dbaModel.FileName}");

            // Find ChunkIvoDBAData in the model chunks
            foreach (var chunk in dbaModel.ChunkMap.Values)
            {
                if (chunk is CgfConverter.CryEngineCore.ChunkIvoDBAData dbaData)
                {
                    Console.WriteLine($"    Animation blocks: {dbaData.AnimationBlocks.Count}");
                    for (int a = 0; a < dbaData.AnimationBlocks.Count; a++)
                    {
                        var block = dbaData.AnimationBlocks[a];
                        Console.WriteLine($"\n    Block[{a}]: BoneCount={block.Header.BoneCount}, Controllers={block.Controllers.Length}");

                        // Show key controllers - read SNORM headers from DBA binary
                        var focusBones = new HashSet<string> {
                            "LG_Back_SubWheelCompress_Skin_bn", "LG_Back_MainWheel_Skin_bn",
                            "LG_Back_MainWheelCompress_Skin_bn", "LG_Back_LowLeg_Skin_bn",
                            "LG_Back_UpPiston_01_Skin_bn", "LG_Back_SubLowLeg_Skin_bn"
                        };

                        for (int c = 0; c < block.Controllers.Length; c++)
                        {
                            var ctrl = block.Controllers[c];
                            uint hash = block.BoneHashes[c];
                            var bone = bones.FirstOrDefault(b => b.ControllerID == hash);
                            var boneName = bone?.BoneName ?? $"unknown_{hash:X8}";
                            bool isFocus = focusBones.Contains(boneName);

                            if (!isFocus) continue;

                            var posFormat = IvoAnimationHelpers.GetPositionFormat(ctrl.PosFormatFlags);
                            Console.Write($"      {boneName,-40} hash=0x{hash:X08} fmt={posFormat,-15} posKeys={ctrl.NumPosKeys}");

                            if (block.Positions.TryGetValue(hash, out var positions) && positions.Count > 0)
                                Console.Write($"  parsed[0]=({positions[0].X:F6}, {positions[0].Y:F6}, {positions[0].Z:F6})");

                            Console.WriteLine();

                            // Show rest position
                            if (bone is not null)
                            {
                                Matrix4x4 restLocal;
                                if (bone.ParentBone is null)
                                    Matrix4x4.Invert(bone.BindPoseMatrix, out restLocal);
                                else
                                {
                                    Matrix4x4.Invert(bone.BindPoseMatrix, out var cw2);
                                    restLocal = bone.ParentBone.BindPoseMatrix * cw2;
                                }
                                Console.WriteLine($"             rest=({restLocal.M14:F6}, {restLocal.M24:F6}, {restLocal.M34:F6})");
                            }

                            // Read SNORM header from DBA binary for C1/C2 formats
                            if ((posFormat == IvoPositionFormat.SNormFull || posFormat == IvoPositionFormat.SNormPacked)
                                && ctrl.PosDataOffset > 0 && block.ControllerOffsets.Length > c)
                            {
                                long controllerStart = block.ControllerOffsets[c];
                                long headerPos = controllerStart + ctrl.PosDataOffset;

                                // Read the DBA file to get the raw header
                                var dbaPath = dbaModel.FileName;
                                if (dbaPath is not null && File.Exists(dbaPath))
                                {
                                    using var fs = File.OpenRead(dbaPath);
                                    using var br = new BinaryReader(fs);
                                    br.BaseStream.Seek(headerPos, SeekOrigin.Begin);
                                    float cx = br.ReadSingle(), cy = br.ReadSingle(), cz = br.ReadSingle();
                                    float sx = br.ReadSingle(), sy = br.ReadSingle(), sz = br.ReadSingle();
                                    Console.WriteLine($"             SNORM header: center=({cx:F6}, {cy:F6}, {cz:F6})  scale=({sx:F6}, {sy:F6}, {sz:F6})");

                                    // Read first SNORM value
                                    if (posFormat == IvoPositionFormat.SNormFull)
                                    {
                                        short v0x = br.ReadInt16(), v0y = br.ReadInt16(), v0z = br.ReadInt16();
                                        Console.WriteLine($"             raw SNORM[0]: ({v0x}, {v0y}, {v0z})");
                                        Console.WriteLine($"             center+snorm*scale: ({cx + v0x / 32767f * sx:F6}, {cy + v0y / 32767f * sy:F6}, {cz + v0z / 32767f * sz:F6})");
                                    }
                                    else // SNormPacked
                                    {
                                        bool xActive = cx < 3.4e38f;
                                        bool yActive = cy < 3.4e38f;
                                        bool zActive = cz < 3.4e38f;
                                        Console.Write($"             active: X={xActive} Y={yActive} Z={zActive}");
                                        float vx = 0, vy = 0, vz = 0;
                                        short rawX = 0, rawY = 0, rawZ = 0;
                                        if (xActive) { rawX = br.ReadInt16(); vx = rawX / 32767f * sx; }
                                        if (yActive) { rawY = br.ReadInt16(); vy = rawY / 32767f * sy; }
                                        if (zActive) { rawZ = br.ReadInt16(); vz = rawZ / 32767f * sz; }
                                        Console.WriteLine($"  raw=({rawX}, {rawY}, {rawZ})");
                                        Console.WriteLine($"             snorm*scale: ({vx:F6}, {vy:F6}, {vz:F6})");
                                        float absX = xActive ? cx + vx : 0;
                                        float absY = yActive ? cy + vy : 0;
                                        float absZ = zActive ? cz + vz : 0;
                                        Console.WriteLine($"             center+snorm*scale: ({absX:F6}, {absY:F6}, {absZ:F6})");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}

static void DumpIvoRaw(string path)
{
    using var fs = File.OpenRead(path);
    using var br = new BinaryReader(fs);

    // IVO header per StarBreaker whitepaper
    string magic = System.Text.Encoding.ASCII.GetString(br.ReadBytes(4));
    uint version = br.ReadUInt32();
    uint chunkCount = br.ReadUInt32();
    uint chunkTableOffset = br.ReadUInt32();
    Console.WriteLine($"IVO header: magic='{magic}' version=0x{version:X} chunks={chunkCount} tableOffset=0x{chunkTableOffset:X}");
    Console.WriteLine($"File size: 0x{fs.Length:X} ({fs.Length})\n");

    if (magic != "#ivo") { Console.WriteLine("Not an #ivo file."); return; }

    // Chunk table: 16 bytes per entry — type(u32), version(u32), fileOffset(u64)
    fs.Seek(chunkTableOffset, SeekOrigin.Begin);
    var chunks = new List<(uint type, uint ver, ulong off)>();
    for (int i = 0; i < chunkCount; i++)
    {
        uint t = br.ReadUInt32();
        uint v = br.ReadUInt32();
        ulong o = br.ReadUInt64();
        chunks.Add((t, v, o));
        Console.WriteLine($"  chunk[{i}]: type=0x{t:X8} version=0x{v:X} fileOffset=0x{o:X}");
    }
    Console.WriteLine();

    // Locate DB_DATA chunk (0x194FBC50)
    var dbData = chunks.Find(c => c.type == 0x194FBC50);
    if (dbData.off == 0) { Console.WriteLine("No DB_DATA chunk in this file."); return; }

    Console.WriteLine($"=== Walking DB_DATA at file offset 0x{dbData.off:X} ===\n");
    fs.Seek((long)dbData.off, SeekOrigin.Begin);

    // First u32 of DB_DATA payload is total_size (per whitepaper)
    long payloadStart = fs.Position;
    uint totalSize = br.ReadUInt32();
    Console.WriteLine($"total_size (first u32): 0x{totalSize:X} ({totalSize})");
    Console.WriteLine($"payload starts at 0x{payloadStart:X}, blocks start at 0x{fs.Position:X}\n");

    // Walk blocks per whitepaper layout:
    //   +0x00 '#dba' (4)  +0x04 bone_count (u16)  +0x06 0xAA55 (u16)  +0x08 data_size (u32)
    int blockIndex = 0;
    while (true)
    {
        long blockStart = fs.Position;
        if (blockStart >= fs.Length - 4) { Console.WriteLine($"Reached EOF at 0x{blockStart:X}."); break; }

        byte[] sigBytes = br.ReadBytes(4);
        string sig = System.Text.Encoding.ASCII.GetString(sigBytes);
        ushort boneCount = br.ReadUInt16();
        ushort magic2 = br.ReadUInt16();
        uint dataSize = br.ReadUInt32();

        Console.WriteLine($"Block[{blockIndex}] @ 0x{blockStart:X}: sig='{sig}' bones={boneCount} magic=0x{magic2:X4} data_size=0x{dataSize:X} ({dataSize})");
        Console.WriteLine($"  raw sig bytes: {sigBytes[0]:X2} {sigBytes[1]:X2} {sigBytes[2]:X2} {sigBytes[3]:X2}");

        if (sig != "#dba")
        {
            Console.WriteLine("  Not '#dba' — terminating loop per whitepaper rule.");
            break;
        }

        long nextStart = blockStart + dataSize;
        Console.WriteLine($"  blockStart + data_size = 0x{nextStart:X}");

        // Peek what's at nextStart
        if (nextStart < fs.Length - 4)
        {
            long savedPos = fs.Position;
            fs.Seek(nextStart, SeekOrigin.Begin);
            byte[] peek = br.ReadBytes(4);
            string peekStr = System.Text.Encoding.ASCII.GetString(peek);
            Console.WriteLine($"  peek at 0x{nextStart:X}: '{peekStr}' (raw: {peek[0]:X2} {peek[1]:X2} {peek[2]:X2} {peek[3]:X2})");

            // Also check a few candidate offsets: alignment + small padding
            foreach (long delta in new long[] { 4, 8, 16, 32, 48 })
            {
                long alt = nextStart + delta;
                if (alt + 4 > fs.Length) continue;
                fs.Seek(alt, SeekOrigin.Begin);
                byte[] altBytes = br.ReadBytes(4);
                string altStr = System.Text.Encoding.ASCII.GetString(altBytes);
                if (altStr == "#dba" || altStr == "#caf")
                    Console.WriteLine($"  >>> '#dba'/'#caf' actually found at 0x{alt:X} (= nextStart + {delta}) <<<");
            }

            fs.Seek(savedPos, SeekOrigin.Begin);
        }

        // Scan whole file for all #dba/#caf magics — print ALL of them
        if (blockIndex == 0)
        {
            Console.WriteLine("  All '#dba'/'#caf' magic occurrences in file:");
            for (long p = 0; p < fs.Length - 4; p++)
            {
                fs.Seek(p, SeekOrigin.Begin);
                byte[] b4 = br.ReadBytes(4);
                bool isDba = b4[0] == (byte)'#' && b4[1] == (byte)'d' && b4[2] == (byte)'b' && b4[3] == (byte)'a';
                bool isCaf = b4[0] == (byte)'#' && b4[1] == (byte)'c' && b4[2] == (byte)'a' && b4[3] == (byte)'f';
                if (isDba || isCaf)
                    Console.WriteLine($"    0x{p:X}  '{(isDba ? "#dba" : "#caf")}'");
            }
        }
        Console.WriteLine();

        // Advance: try ALTERNATE stride — header(12) + boneHashes(4*n) + controllers(24*n)
        long altStride = 12 + 4 * boneCount + 24 * boneCount;
        long altNext = blockStart + altStride;
        Console.WriteLine($"  alt stride (12 + 28*bones): 0x{altStride:X}  → next at 0x{altNext:X}");

        // Use alt stride for actual iteration to walk full chain
        fs.Seek(altNext, SeekOrigin.Begin);
        blockIndex++;
        if (blockIndex > 8) { Console.WriteLine("Stopping after 8 blocks."); break; }
    }

    // Now also try walking via DBA_META to cross-check expected animation count
    Console.WriteLine();
    var dbaMeta = chunks.Find(c => c.type == 0xF7351608);
    if (dbaMeta.off != 0)
    {
        Console.WriteLine($"=== DBA_META at 0x{dbaMeta.off:X} (version 0x{dbaMeta.ver:X}) ===");
        fs.Seek((long)dbaMeta.off, SeekOrigin.Begin);
        uint count = br.ReadUInt32();
        Console.WriteLine($"  count (animations): {count}");
        for (int i = 0; i < count; i++)
        {
            long entryStart = fs.Position;
            uint flags0 = br.ReadUInt32();
            uint flags1 = br.ReadUInt32();
            ushort fps = br.ReadUInt16();
            ushort numCtrls = br.ReadUInt16();
            uint ver2 = br.ReadUInt32();
            uint pad = br.ReadUInt32();
            uint endFrame = br.ReadUInt32();
            // start_rotation (16) + start_position (8) = 24
            fs.Seek(entryStart + 48, SeekOrigin.Begin);
            Console.WriteLine($"  entry[{i}]: flags=({flags0:X8},{flags1:X8}) fps={fps} numControllers={numCtrls} ver=0x{ver2:X} endFrame={endFrame}");
        }

        // Skip 4-byte NUL pad, then string table
        fs.ReadByte(); fs.ReadByte(); fs.ReadByte(); fs.ReadByte();
        long stringStart = fs.Position;
        Console.WriteLine($"  string table at 0x{stringStart:X}:");
        var sb = new System.Text.StringBuilder();
        while (fs.Position < fs.Length)
        {
            byte ch = br.ReadByte();
            if (ch == 0)
            {
                if (sb.Length > 0) { Console.WriteLine($"    \"{sb}\""); sb.Clear(); }
                else break; // double-null = end
            }
            else sb.Append((char)ch);
            if (fs.Position >= fs.Length) break;
        }
    }
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
