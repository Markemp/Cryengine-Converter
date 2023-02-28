using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using BCnEncoder.Decoder;
using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using BCnEncoder.Shared.ImageFiles;
using CgfConverter.CryEngineCore;
using Extensions;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace CgfConverter.GLTF;

public class GLTF : BaseRenderer
{
    private readonly GltfRoot _gltfObject;
    private readonly MemoryStream _gltfDataStream;
    private readonly BinaryWriter _gltfDataWriter;
    private readonly Dictionary<uint, int> _controllerIdToNodeIndex;
    private readonly bool _writeText, _writeBinary;

    public GLTF(ArgsHandler argsHandler, CryEngine cryEngine, bool writeText, bool writeBinary) : base(argsHandler,
        cryEngine)
    {
        _gltfObject = new GltfRoot();
        _gltfDataStream = new MemoryStream();
        _gltfDataWriter = new BinaryWriter(_gltfDataStream);
        _controllerIdToNodeIndex = new Dictionary<uint, int>();
        _writeText = writeText;
        _writeBinary = writeBinary;

        _gltfObject.Samplers.Add(new GltfSampler());
        // ^ Just because blender does so.
    }

    public override void Render(string? outputDir = null, bool preservePath = true)
    {
        preservePath = false;
        
        var glbOutputFile = new FileInfo(GetOutputFile("glb", outputDir, preservePath));
        var gltfOutputFile = new FileInfo(GetOutputFile("gltf", outputDir, preservePath));
        var gltfBinOutputFile = new FileInfo(GetOutputFile("bin", outputDir, preservePath));

        _gltfObject.Scenes.Add(new GltfScene
        {
            Name = "Scene",
        });

        if (CryData.Models.Count == 1) // Single file model
            WriteGeometries(CryData.Models[0]);
        else
            WriteGeometries(CryData.Models[1]);

        WriteAnimations();

        while (_gltfDataStream.Position % 4 > 0)
            _gltfDataStream.WriteByte(0);

        var bin = _gltfDataStream.GetBuffer();

        if (_writeBinary)
        {
            _gltfObject.Buffers = new List<GltfBuffer>
            {
                new()
                {
                    ByteLength = _gltfDataStream.Position,
                }
            };

            var json = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_gltfObject));
            if (json.Length % 4 != 0)
            {
                var rv = new byte[(json.Length + 3) / 4 * 4];
                Buffer.BlockCopy(json, 0, rv, 0, json.Length);
                for (var i = json.Length; i < rv.Length; i++)
                    rv[i] = 0x20; // space
                json = rv;
            }

            using var writer = new BinaryWriter(glbOutputFile.Open(FileMode.Create, FileAccess.Write));
            writer.Write(0x46546C67);
            writer.Write(2);
            writer.Write(12 + 8 + json.Length + 8 + bin.Length);
            writer.Write(json.Length);
            writer.Write(0x4E4F534A);
            writer.Write(json);

            writer.Write(bin.Length);
            writer.Write(0x004E4942);
            writer.Write(bin);
        }

        if (_writeText)
        {
            _gltfObject.Buffers = new List<GltfBuffer>
            {
                new()
                {
                    ByteLength = _gltfDataStream.Position,
                    Uri = gltfBinOutputFile.Name,
                }
            };
            var json = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_gltfObject));

            using var gltfWrite = gltfOutputFile.Open(FileMode.Create, FileAccess.Write);
            gltfWrite.Write(json);

            using var gltfBinWrite = gltfBinOutputFile.Open(FileMode.Create, FileAccess.Write);
            gltfBinWrite.Write(bin);
        }
    }
    
    private int CreateTexture<TPixel>(string name, int width, int height, TPixel[] raw) where TPixel : unmanaged, IPixel<TPixel>
    {
        using var ms = new MemoryStream();
        using (var image = Image.LoadPixelData(raw, width, height))
            image.SaveAsPng(ms);
        return CreateTexture(name + ".png", "image/png", ms.ToArray());
    }
    
    private int CreateTexture(string name, int width, int height, ColorRgba32[] raw, bool useAlpha)
    {
        using var ms = new MemoryStream();
        if (useAlpha)
        {
            var res = new Rgba32[raw.Length];
            for (var i = 0; i < raw.Length; i++)
            {
                res[i].R = raw[i].r;
                res[i].G = raw[i].g;
                res[i].B = raw[i].b;
                res[i].A = raw[i].a;
            }

            using var image = Image.LoadPixelData(res, width, height);
            image.SaveAsPng(ms);
        }
        else
        {
            var res = new Rgb24[raw.Length];
            for (var i = 0; i < raw.Length; i++)
            {
                res[i].R = raw[i].r;
                res[i].G = raw[i].g;
                res[i].B = raw[i].b;
            }

            using var image = Image.LoadPixelData(res, width, height);
            image.SaveAsPng(ms);
        }
        return CreateTexture(name + ".png", "image/png", ms.ToArray());
    }

    private int CreateTexture(string name, string mimeType, byte[] textureBytes)
    {
        _gltfObject.Textures.Add(new GltfTexture
        {
            Name = name,
            Source = _gltfObject.Images.Count,
        });

        _gltfObject.Accessors.Add(new GltfAccessor
        {
            Name = $"TextureAccessor:{Path.GetFileName(name)}",
            BufferView = _gltfObject.BufferViews.Count,
            ComponentType = GltfAccessorComponentTypes.u8,
            Count = textureBytes.Length,
            Type = GltfAccessorTypes.Scalar,
        });
        AlignData(_gltfObject.Accessors.Last().ComponentType);
        _gltfObject.BufferViews.Add(new GltfBufferView
        {
            ByteOffset = _gltfDataStream.Position,
            ByteLength = _gltfObject.Accessors.Last().RequiredByteLength,
        });
        _gltfDataWriter.Write(textureBytes);
        Debug.Assert(_gltfDataStream.Position == _gltfObject.BufferViews.Last().ByteOffsetTo);

        _gltfObject.Images.Add(new GltfImage
        {
            MimeType = mimeType,
            Name = name,
            BufferView = _gltfObject.BufferViews.Count - 1,
        });

        return _gltfObject.Textures.Count - 1;
    }

    private int WriteMaterial(ChunkNode nodeChunk)
    {
        var decoder = new BcDecoder();
        
        var baseMaterialIndex = _gltfObject.Materials.Count;
        foreach (var m in nodeChunk.Materials.SubMaterials)
        {
            _gltfObject.Materials.Add(new GltfMaterial
            {
                Name = m.Name,
                DoubleSided = true,
            });

            var diffuse = -1;
            var diffuseDetail = -1;
            var normal = -1;
            var specular = -1;
            var metallicRoughness = -1;

            foreach (var texture in m.Textures!)
            {
                var texturePath = FileHandlingExtensions.ResolveTextureFile(texture.File, Args.DataDir);
                if (!File.Exists(texturePath))
                {
                    Utilities.Log(LogLevelEnum.Warning, $"Skipping {texture.File} because the file could not be found.");
                    continue;
                }

                DdsFile ddsFile;
                using (var ddsfs = new FileStream(texturePath, FileMode.Open, FileAccess.Read))
                    ddsFile = DdsFile.Load(ddsfs);

                var width = (int)ddsFile.header.dwWidth;
                var height = (int)ddsFile.header.dwHeight;
                var raw = decoder.Decode(ddsFile);
                
                var name = Path.GetFileNameWithoutExtension(texturePath);

                switch (texture.Map)
                {
                    case Texture.MapTypeEnum.Bumpmap:
                    {
                        if (ddsFile.header.ddsPixelFormat.DxgiFormat == DxgiFormat.DxgiFormatBc3Unorm)
                        {
                            // https://docs.cryengine.com/display/SDKDOC2/Detail+Maps
                            // Red: Diffuse
                            // Green: Normal Red
                            // Blue: Gloss
                            // Alpha: Normal Green
                            var rawNormal = new Rgb24[raw.Length];
                            var rawMetallicRoughness = new Rgb24[raw.Length];
                            var rawDiffuseDetail = new Rgb24[raw.Length];
                            for (var i = 0; i < raw.Length; i++)
                            {
                                var r = (rawNormal[i].R = raw[i].g) / 255f;
                                var g = (rawNormal[i].G = raw[i].a) / 255f;
                                var b = Math.Sqrt(1 - Math.Pow(r * 2 - 1, 2) - Math.Pow(g * 2 - 1, 2)) / 2 + 0.5f;
                                rawNormal[i].B = (byte) (255 * b);

                                // Its green channel contains roughness values
                                rawMetallicRoughness[i].G = (byte) (255 - raw[i].b);
                                // and its blue channel contains metalness values.

                                rawDiffuseDetail[i].R = rawDiffuseDetail[i].G = rawDiffuseDetail[i].B = raw[i].r;
                            }

                            normal = CreateTexture(name, width, height, rawNormal);
                            metallicRoughness = CreateTexture(name, width, height, rawMetallicRoughness);
                            diffuseDetail = CreateTexture(name, width, height, rawDiffuseDetail);
                        }
                        else
                        {
                            normal = CreateTexture(name, width, height, raw, false);
                        }

                        break;
                    }
                    case Texture.MapTypeEnum.Diffuse:
                    {
                        var noAlpha = true;
                        for (var i = 0; i < raw.Length && noAlpha; i++)
                            noAlpha &= raw[i].a == 0;

                        diffuse = CreateTexture(name, width, height, raw, !noAlpha);
                        break;
                    }
                    case Texture.MapTypeEnum.Specular:
                    {
                        var noAlpha = true;
                        for (var i = 0; i < raw.Length && noAlpha; i++)
                            noAlpha &= raw[i].a == 0;

                        specular = CreateTexture(name, width, height, raw, !noAlpha);
                        break;
                    }
                    default:
                        Utilities.Log($"Ignoring texture type {texture.Map}");
                        break;
                }
            }

            if (normal != -1)
            {
                _gltfObject.Materials.Last().NormalTexture = new GltfMaterialTextureSpecifier
                {
                    Index = normal,
                };
            }

            if (diffuse != -1 || metallicRoughness != -1)
            {
                _gltfObject.Materials.Last().PbrMetallicRoughness = new GltfMaterialPbrMetallicRoughness
                {
                    BaseColorTexture = diffuse == -1
                        ? null
                        : new GltfMaterialTextureSpecifier
                        {
                            Index = diffuse,
                        },
                    MetallicFactor = 0f,
                    RoughnessFactor = 1f,
                    MetallicRoughnessTexture = metallicRoughness == -1
                        ? null
                        : new GltfMaterialTextureSpecifier
                        {
                            Index = metallicRoughness,
                        },
                };
            }

            if (specular != -1)
            {
                _gltfObject.ExtensionsUsed.Add("KHR_materials_specular");
                _gltfObject.Materials.Last().Extensions = new GltfExtensions
                {
                    KhrMaterialsSpecular = new GltfExtensionKhrMaterialsSpecular
                    {
                        SpecularColorTexture = new GltfMaterialTextureSpecifier
                        {
                            Index = specular,
                        },
                    },
                };
            }
        }

        return baseMaterialIndex;
    }

    private static Vector3 SwapAxes(Vector3 val) => new(val.X, val.Z, val.Y);

    private static Quaternion SwapAxes(Quaternion val) => new(val.X, val.Z, val.Y, -val.W);

    private static Matrix4x4 SwapAxes(Matrix4x4 val) => new(
        val.M11, val.M13, val.M12, val.M14,
        val.M31, val.M33, val.M32, val.M34,
        val.M21, val.M23, val.M22, val.M24,
        val.M41, val.M43, val.M42, val.M44);

    private void WriteGeometries(Model model)
    {
        foreach (var nodeChunk in model.ChunkMap.Values.OfType<ChunkNode>())
        {
            if (IsNodeNameExcluded(nodeChunk.Name))
            {
                Utilities.Log(LogLevelEnum.Debug, $"Excluding node {nodeChunk.Name}");
                continue;
            }

            if (nodeChunk.ObjectChunk is null)
            {
                Utilities.Log(LogLevelEnum.Warning, "Skipped node with missing Object {0}", nodeChunk.Name);
                continue;
            }

            if (nodeChunk._model.ChunkMap[nodeChunk.ObjectNodeID] is not ChunkMesh meshChunk)
                continue;

            var vertices = nodeChunk._model.ChunkMap.GetValueOrDefault(meshChunk.VerticesData) as ChunkDataStream;
            var vertsUvs = nodeChunk._model.ChunkMap.GetValueOrDefault(meshChunk.VertsUVsData) as ChunkDataStream;
            var normals = nodeChunk._model.ChunkMap.GetValueOrDefault(meshChunk.NormalsData) as ChunkDataStream;
            var uvs = nodeChunk._model.ChunkMap.GetValueOrDefault(meshChunk.UVsData) as ChunkDataStream;
            var indices = nodeChunk._model.ChunkMap.GetValueOrDefault(meshChunk.IndicesData) as ChunkDataStream;
            var colors = nodeChunk._model.ChunkMap.GetValueOrDefault(meshChunk.ColorsData) as ChunkDataStream;
            var tangents = nodeChunk._model.ChunkMap.GetValueOrDefault(meshChunk.TangentsData) as ChunkDataStream;
            var meshSubsets =
                nodeChunk._model.ChunkMap.GetValueOrDefault(meshChunk.MeshSubsetsData) as ChunkMeshSubsets;

            Debug.Assert(indices != null);
            Debug.Assert(meshSubsets != null);
            Debug.Assert(nodeChunk.Materials != null);
            Debug.Assert(nodeChunk.Materials.SubMaterials != null);

            var baseMaterialIndex = WriteMaterial(nodeChunk);

            if (vertices is not null) // Will be null if it's using VertsUVs.
            {
                var axisSwappedVertices = vertices.Vertices.Select(SwapAxes).ToArray();
                var vertexBufferAccessor = _gltfObject.Accessors.Count;
                _gltfObject.Accessors.Add(new GltfAccessor
                {
                    Name = "VertexAccessor",
                    BufferView = _gltfObject.BufferViews.Count,
                    ComponentType = GltfAccessorComponentTypes.f32,
                    Count = axisSwappedVertices.Length,
                    Type = GltfAccessorTypes.Vec3,
                    Min = new List<float>
                    {
                        axisSwappedVertices.Min(vec3 => vec3.X),
                        axisSwappedVertices.Min(vec3 => vec3.Y),
                        axisSwappedVertices.Min(vec3 => vec3.Z),
                    },
                    Max = new List<float>
                    {
                        axisSwappedVertices.Max(vec3 => vec3.X),
                        axisSwappedVertices.Max(vec3 => vec3.Y),
                        axisSwappedVertices.Max(vec3 => vec3.Z),
                    },
                });
                AlignData(_gltfObject.Accessors.Last().ComponentType);
                _gltfObject.BufferViews.Add(new GltfBufferView
                {
                    ByteOffset = _gltfDataStream.Position,
                    ByteLength = _gltfObject.Accessors.Last().RequiredByteLength,
                });
                foreach (var vec3 in axisSwappedVertices)
                    _gltfDataWriter.Write(vec3);
                Debug.Assert(_gltfDataStream.Position == _gltfObject.BufferViews.Last().ByteOffsetTo);

                int? colorBufferAccessor = null;
                // TODO: does this do anything?
                /*
                if (colors is not null)
                {
                    colorBufferAccessor = _gltfObject.Accessors.Count;
                    _gltfObject.Accessors.Add(new GltfAccessor
                    {
                        Name = "ColorAccessor",
                        BufferView = _gltfObject.BufferViews.Count,
                        ComponentType = GltfAccessorComponentTypes.f32,
                        Count = colors.Colors.Length,
                        Type = GltfAccessorTypes.Vec4,
                    });
                    AlignData(_gltfObject.Accessors.Last().ComponentType);
                    _gltfObject.BufferViews.Add(new GltfBufferView
                    {
                        ByteOffset = _gltfDataStream.Position,
                        ByteLength = _gltfObject.Accessors.Last().RequiredByteLength,
                    });
                    foreach (var rgba in colors.Colors)
                        _gltfDataWriter.Write(rgba);
                    Debug.Assert(_gltfDataStream.Position == _gltfObject.BufferViews.Last().ByteOffsetTo);
                }
                //*/

                int? normalBufferAccessor = null;
                if (normals is not null)
                {
                    normalBufferAccessor = _gltfObject.Accessors.Count;
                    _gltfObject.Accessors.Add(new GltfAccessor
                    {
                        Name = "NormalAccessor",
                        BufferView = _gltfObject.BufferViews.Count,
                        ComponentType = GltfAccessorComponentTypes.f32,
                        Count = normals.Normals.Length,
                        Type = GltfAccessorTypes.Vec3,
                    });
                    AlignData(_gltfObject.Accessors.Last().ComponentType);
                    _gltfObject.BufferViews.Add(new GltfBufferView
                    {
                        ByteOffset = _gltfDataStream.Position,
                        ByteLength = _gltfObject.Accessors.Last().RequiredByteLength,
                    });
                    foreach (var vec3 in normals.Normals)
                        _gltfDataWriter.Write(vec3);
                    Debug.Assert(_gltfDataStream.Position == _gltfObject.BufferViews.Last().ByteOffsetTo);
                }

                int? tangentBufferAccessor = null;
                if (tangents is not null)
                {
                    tangentBufferAccessor = _gltfObject.Accessors.Count;
                    _gltfObject.Accessors.Add(new GltfAccessor
                    {
                        Name = "TangentAccessor",
                        BufferView = _gltfObject.BufferViews.Count,
                        ComponentType = GltfAccessorComponentTypes.f32,
                        Count = tangents.Tangents.GetLength(0),
                        Type = GltfAccessorTypes.Vec4,
                    });
                    AlignData(_gltfObject.Accessors.Last().ComponentType);
                    _gltfObject.BufferViews.Add(new GltfBufferView
                    {
                        ByteOffset = _gltfDataStream.Position,
                        ByteLength = _gltfObject.Accessors.Last().RequiredByteLength,
                    });
                    for (var i = 0; i < tangents.Tangents.GetLength(0); i++)
                        _gltfDataWriter.Write(tangents.Tangents[i, 0]);
                    Debug.Assert(_gltfDataStream.Position == _gltfObject.BufferViews.Last().ByteOffsetTo);
                }

                int? uvBufferAccessor = null;
                if (uvs is not null)
                {
                    uvBufferAccessor = _gltfObject.Accessors.Count;
                    _gltfObject.Accessors.Add(new GltfAccessor
                    {
                        Name = "UVAccessor",
                        BufferView = _gltfObject.BufferViews.Count,
                        ComponentType = GltfAccessorComponentTypes.f32,
                        Count = uvs.UVs.Length,
                        Type = GltfAccessorTypes.Vec2,
                    });
                    AlignData(_gltfObject.Accessors.Last().ComponentType);
                    _gltfObject.BufferViews.Add(new GltfBufferView
                    {
                        ByteOffset = _gltfDataStream.Position,
                        ByteLength = _gltfObject.Accessors.Last().RequiredByteLength,
                    });
                    foreach (var uv in uvs.UVs)
                        _gltfDataWriter.Write(uv);
                    Debug.Assert(_gltfDataStream.Position == _gltfObject.BufferViews.Last().ByteOffsetTo);
                }

                var skinningInfo = nodeChunk.GetSkinningInfo();

                var boneWeightAccessor = _gltfObject.Accessors.Count;
                if (skinningInfo.IntVertices == null)
                {
                    _gltfObject.Accessors.Add(new GltfAccessor
                    {
                        Name = "BoneWeightAccessor",
                        BufferView = _gltfObject.BufferViews.Count,
                        ComponentType = GltfAccessorComponentTypes.f32,
                        Count = skinningInfo.BoneMapping.Count,
                        Type = GltfAccessorTypes.Vec4,
                    });
                    AlignData(_gltfObject.Accessors.Last().ComponentType);
                    _gltfObject.BufferViews.Add(new GltfBufferView
                    {
                        ByteOffset = _gltfDataStream.Position,
                        ByteLength = _gltfObject.Accessors.Last().RequiredByteLength,
                    });
                    foreach (var v in skinningInfo.BoneMapping)
                    {
                        _gltfDataWriter.Write(v.Weight[0] / 255f);
                        _gltfDataWriter.Write(v.Weight[1] / 255f);
                        _gltfDataWriter.Write(v.Weight[2] / 255f);
                        _gltfDataWriter.Write(v.Weight[3] / 255f);
                    }
                }
                else
                {
                    _gltfObject.Accessors.Add(new GltfAccessor
                    {
                        Name = "BoneWeightAccessor",
                        BufferView = _gltfObject.BufferViews.Count,
                        ComponentType = GltfAccessorComponentTypes.f32,
                        Count = skinningInfo.Ext2IntMap.Count,
                        Type = GltfAccessorTypes.Vec4,
                    });
                    AlignData(_gltfObject.Accessors.Last().ComponentType);
                    _gltfObject.BufferViews.Add(new GltfBufferView
                    {
                        ByteOffset = _gltfDataStream.Position,
                        ByteLength = _gltfObject.Accessors.Last().RequiredByteLength,
                    });
                    foreach (var v in skinningInfo.Ext2IntMap)
                    {
                        _gltfDataWriter.Write(skinningInfo.IntVertices[v].Weights[0]);
                        _gltfDataWriter.Write(skinningInfo.IntVertices[v].Weights[1]);
                        _gltfDataWriter.Write(skinningInfo.IntVertices[v].Weights[2]);
                        _gltfDataWriter.Write(skinningInfo.IntVertices[v].Weights[3]);
                    }
                }

                Debug.Assert(_gltfDataStream.Position == _gltfObject.BufferViews.Last().ByteOffsetTo);

                var armatureNode = new GltfNode
                {
                    Name = "MyArmature",
                };
                var meshNode = new GltfNode
                {
                    Name = "MyMesh",
                };

                // if (bone.parentID == 0) m4x4 = Matrix4x4.Transform(m4x4, AxisChangeQuat);

                var boneIdToBindPoseMatrices = new Dictionary<uint, Matrix4x4>();
                foreach (var bone in skinningInfo.CompiledBones)
                {
                    _gltfObject.Nodes.Add(new GltfNode
                    {
                        Name = bone.boneName,
                    });
                    _controllerIdToNodeIndex[bone.ControllerID] = _gltfObject.Nodes.Count - 1;

                    boneIdToBindPoseMatrices[bone.ControllerID] = bone.BindPoseMatrix;
                    var m4x4 = bone.BindPoseMatrix;

                    if (bone.parentID == 0)
                    {
                        armatureNode.Children.Add(_gltfObject.Nodes.Count - 1);
                    }
                    else
                    {
                        _gltfObject.Nodes[_controllerIdToNodeIndex[bone.parentID]].Children
                            .Add(_gltfObject.Nodes.Count - 1);

                        if (!Matrix4x4.Invert(boneIdToBindPoseMatrices[bone.parentID], out var pm4x4))
                            throw new Exception();
                        m4x4 = m4x4 * pm4x4;
                    }

                    if (!Matrix4x4.Invert(m4x4, out m4x4))
                        throw new Exception();

                    m4x4 = Matrix4x4.Transpose(m4x4);
                    m4x4 = SwapAxes(m4x4);

                    if (!Matrix4x4.Decompose(m4x4, out var scale, out var rotation, out var translation))
                        throw new Exception();

                    if ((scale - Vector3.One).LengthSquared() > 0.000001)
                        _gltfObject.Nodes.Last().Scale = new List<float> {scale.X, scale.Y, scale.Z};
                    if (translation != Vector3.Zero)
                        _gltfObject.Nodes.Last().Translation = new List<float>
                            {translation.X, translation.Y, translation.Z};
                    if (rotation != Quaternion.Identity)
                        _gltfObject.Nodes.Last().Rotation = new List<float>
                            {rotation.X, rotation.Y, rotation.Z, rotation.W};
                }

                var jointAccessor = _gltfObject.Accessors.Count;
                if (!skinningInfo.HasIntToExtMapping)
                {
                    _gltfObject.Accessors.Add(new GltfAccessor
                    {
                        Name = "BoneIndexAccessor",
                        BufferView = _gltfObject.BufferViews.Count,
                        ComponentType = GltfAccessorComponentTypes.u16,
                        Count = skinningInfo.BoneMapping.Count,
                        Type = GltfAccessorTypes.Vec4,
                    });
                    AlignData(_gltfObject.Accessors.Last().ComponentType);
                    _gltfObject.BufferViews.Add(new GltfBufferView
                    {
                        ByteOffset = _gltfDataStream.Position,
                        ByteLength = _gltfObject.Accessors.Last().RequiredByteLength,
                    });
                    foreach (var v in skinningInfo.BoneMapping)
                    {
                        _gltfDataWriter.Write((ushort) v.BoneIndex[0]);
                        _gltfDataWriter.Write((ushort) v.BoneIndex[1]);
                        _gltfDataWriter.Write((ushort) v.BoneIndex[2]);
                        _gltfDataWriter.Write((ushort) v.BoneIndex[3]);
                    }
                }
                else
                {
                    _gltfObject.Accessors.Add(new GltfAccessor
                    {
                        Name = "BoneIndexAccessor",
                        BufferView = _gltfObject.BufferViews.Count,
                        ComponentType = GltfAccessorComponentTypes.u16,
                        Count = skinningInfo.Ext2IntMap.Count,
                        Type = GltfAccessorTypes.Vec4,
                    });
                    AlignData(_gltfObject.Accessors.Last().ComponentType);
                    _gltfObject.BufferViews.Add(new GltfBufferView
                    {
                        ByteOffset = _gltfDataStream.Position,
                        ByteLength = _gltfObject.Accessors.Last().RequiredByteLength,
                    });
                    foreach (var v in skinningInfo.Ext2IntMap)
                    {
                        _gltfDataWriter.Write((ushort) skinningInfo.IntVertices[v].BoneIDs[0]);
                        _gltfDataWriter.Write((ushort) skinningInfo.IntVertices[v].BoneIDs[1]);
                        _gltfDataWriter.Write((ushort) skinningInfo.IntVertices[v].BoneIDs[2]);
                        _gltfDataWriter.Write((ushort) skinningInfo.IntVertices[v].BoneIDs[3]);
                    }
                }

                Debug.Assert(_gltfDataStream.Position == _gltfObject.BufferViews.Last().ByteOffsetTo);

                var inverseBindMatricesAccessor = _gltfObject.Accessors.Count;
                _gltfObject.Accessors.Add(new GltfAccessor
                {
                    Name = "InverseBindMatricesAccessor",
                    BufferView = _gltfObject.BufferViews.Count,
                    ComponentType = GltfAccessorComponentTypes.f32,
                    Count = skinningInfo.CompiledBones.Count,
                    Type = GltfAccessorTypes.Mat4,
                });
                AlignData(_gltfObject.Accessors.Last().ComponentType);
                _gltfObject.BufferViews.Add(new GltfBufferView
                {
                    ByteOffset = _gltfDataStream.Position,
                    ByteLength = _gltfObject.Accessors.Last().RequiredByteLength,
                });
                foreach (var bone in skinningInfo.CompiledBones)
                    _gltfDataWriter.Write(SwapAxes(Matrix4x4.Transpose(bone.BindPoseMatrix)));

                Debug.Assert(_gltfDataStream.Position == _gltfObject.BufferViews.Last().ByteOffsetTo);

                _gltfObject.Skins.Add(new GltfSkin
                {
                    InverseBindMatrices = inverseBindMatricesAccessor,
                    Joints = skinningInfo.CompiledBones.Select(x => _controllerIdToNodeIndex[x.ControllerID]).ToList(),
                    Name = armatureNode.Name,
                });
                meshNode.Skin = _gltfObject.Skins.Count - 1;

                var indexBufferView = _gltfObject.BufferViews.Count;
                AlignData(GltfAccessorComponentTypes.u32);
                _gltfObject.BufferViews.Add(new GltfBufferView
                {
                    ByteOffset = _gltfDataStream.Position,
                    ByteLength = indices.Indices.Length * 4,
                });
                foreach (var u32 in indices.Indices)
                    _gltfDataWriter.Write(u32);
                Debug.Assert(_gltfDataStream.Position == _gltfObject.BufferViews.Last().ByteOffsetTo);

                var gltfMesh = new GltfMesh();
                foreach (var v in meshSubsets.MeshSubsets)
                {
                    var indicesBufferAccessor = _gltfObject.Accessors.Count;
                    _gltfObject.Accessors.Add(new GltfAccessor
                    {
                        Name = $"MeshAccessor{v.FirstIndex}",
                        BufferView = indexBufferView,
                        ByteOffset = v.FirstIndex * 4,
                        ComponentType = GltfAccessorComponentTypes.u32,
                        Count = v.NumIndices,
                        Type = GltfAccessorTypes.Scalar,
                    });

                    gltfMesh.Primitives.Add(new GltfMeshPrimitive
                    {
                        Attributes = new GltfMeshPrimitiveAttributes
                        {
                            Position = vertexBufferAccessor,
                            Normal = normalBufferAccessor,
                            Tangent = tangentBufferAccessor,
                            TexCoord0 = uvBufferAccessor,
                            Color0 = colorBufferAccessor,
                            Joints0 = jointAccessor,
                            Weights0 = boneWeightAccessor,
                        },
                        Indices = indicesBufferAccessor,
                        Material = baseMaterialIndex + v.MatID,
                    });
                }

                _gltfObject.Meshes.Add(gltfMesh);
                meshNode.Mesh = _gltfObject.Meshes.Count - 1;

                _gltfObject.Nodes.Add(meshNode);
                armatureNode.Children.Insert(0, _gltfObject.Nodes.Count - 1);
                // ^ Insert at index 0 just because blender does so. Probably unnecessary.

                _gltfObject.Nodes.Add(armatureNode);
                _gltfObject.Scenes[_gltfObject.Scene].Nodes.Add(_gltfObject.Nodes.Count - 1);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }

    private void WriteAnimation(
        ChunkController_905.Animation anim,
        List<int> keyTimeAccessors,
        List<int> keyPositionAccessors,
        List<int> keyRotationAccessors)
    {
        var animationNode = new GltfAnimation
        {
            Name = anim.Name,
        };
        _gltfObject.Animations.Add(animationNode);

        foreach (var con in anim.Controllers)
        {
            if (con.HasPosTrack)
            {
                animationNode.Samplers.Add(new GltfAnimationSampler
                {
                    Input = keyTimeAccessors[con.PosKeyTimeTrack],
                    Output = keyPositionAccessors[con.PosTrack],
                    Interpolation = GltfAnimationSamplerInterpolation.Linear,
                });
                animationNode.Channels.Add(new GltfAnimationChannel
                {
                    Sampler = animationNode.Samplers.Count - 1,
                    Target = new GltfAnimationChannelTarget
                    {
                        Node = _controllerIdToNodeIndex[con.ControllerID],
                        Path = GltfAnimationChannelTargetPath.Translation,
                    },
                });
            }

            if (con.HasRotTrack)
            {
                animationNode.Samplers.Add(new GltfAnimationSampler
                {
                    Input = keyTimeAccessors[con.RotKeyTimeTrack],
                    Output = keyRotationAccessors[con.RotTrack],
                    Interpolation = GltfAnimationSamplerInterpolation.Linear,
                });
                animationNode.Channels.Add(new GltfAnimationChannel
                {
                    Sampler = animationNode.Samplers.Count - 1,
                    Target = new GltfAnimationChannelTarget
                    {
                        Node = _controllerIdToNodeIndex[con.ControllerID],
                        Path = GltfAnimationChannelTargetPath.Rotation,
                    },
                });
            }
        }
    }

    private void WriteAnimations()
    {
        var animChunks = CryData.Animations
            .SelectMany(x => x.ChunkMap.Values.OfType<ChunkController_905>())
            .ToList();
        foreach (var animChunk in animChunks)
        {
            var keyTimeAccessors = new List<int>();
            for (var i = 0; i < animChunk.KeyTimes.Count; i++)
            {
                var values = animChunk.KeyTimes[i];
                keyTimeAccessors.Add(_gltfObject.Accessors.Count);
                _gltfObject.Accessors.Add(new GltfAccessor
                {
                    Name = $"KeyTimeAccessor{i}",
                    BufferView = _gltfObject.BufferViews.Count,
                    ComponentType = GltfAccessorComponentTypes.f32,
                    Count = values.Count,
                    Type = GltfAccessorTypes.Scalar,
                    Min = new List<float> {0f},
                    Max = new List<float> {(values.Last() - values.First()) / 30f},
                });
                AlignData(_gltfObject.Accessors.Last().ComponentType);
                _gltfObject.BufferViews.Add(new GltfBufferView
                {
                    ByteOffset = _gltfDataStream.Position,
                    ByteLength = _gltfObject.Accessors.Last().RequiredByteLength,
                });
                foreach (var v in values)
                {
                    // CryEngine animations are always saved at 30 frames.
                    _gltfDataWriter.Write((v - values.First()) / 30f);
                }
            }

            var keyPositionAccessors = new List<int>();
            for (var i = 0; i < animChunk.KeyPositions.Count; i++)
            {
                var values = animChunk.KeyPositions[i].Select(SwapAxes).ToArray();
                keyPositionAccessors.Add(_gltfObject.Accessors.Count);
                _gltfObject.Accessors.Add(new GltfAccessor
                {
                    Name = $"KeyPositionAccessor{i}",
                    BufferView = _gltfObject.BufferViews.Count,
                    ComponentType = GltfAccessorComponentTypes.f32,
                    Count = values.Length,
                    Type = GltfAccessorTypes.Vec3,
                    Min = new List<float>
                    {
                        values.Min(vec3 => vec3.X),
                        values.Min(vec3 => vec3.Y),
                        values.Min(vec3 => vec3.Z),
                    },
                    Max = new List<float>
                    {
                        values.Max(vec3 => vec3.X),
                        values.Max(vec3 => vec3.Y),
                        values.Max(vec3 => vec3.Z),
                    },
                });
                AlignData(_gltfObject.Accessors.Last().ComponentType);
                _gltfObject.BufferViews.Add(new GltfBufferView
                {
                    ByteOffset = _gltfDataStream.Position,
                    ByteLength = _gltfObject.Accessors.Last().RequiredByteLength,
                });
                foreach (var v in values)
                    _gltfDataWriter.Write(v);
            }

            var keyRotationAccessors = new List<int>();
            for (var i = 0; i < animChunk.KeyRotations.Count; i++)
            {
                var values = animChunk.KeyRotations[i].Select(SwapAxes).ToArray();
                keyRotationAccessors.Add(_gltfObject.Accessors.Count);
                _gltfObject.Accessors.Add(new GltfAccessor
                {
                    Name = $"KeyRotationAccessor{i}",
                    BufferView = _gltfObject.BufferViews.Count,
                    ComponentType = GltfAccessorComponentTypes.f32,
                    Count = values.Length,
                    Type = GltfAccessorTypes.Vec4,
                    Min = new List<float>
                    {
                        values.Min(quat => quat.X),
                        values.Min(quat => quat.Y),
                        values.Min(quat => quat.Z),
                        values.Min(quat => quat.W),
                    },
                    Max = new List<float>
                    {
                        values.Max(quat => quat.X),
                        values.Max(quat => quat.Y),
                        values.Max(quat => quat.Z),
                        values.Max(quat => quat.W),
                    },
                });
                AlignData(_gltfObject.Accessors.Last().ComponentType);
                _gltfObject.BufferViews.Add(new GltfBufferView
                {
                    ByteOffset = _gltfDataStream.Position,
                    ByteLength = _gltfObject.Accessors.Last().RequiredByteLength,
                });
                foreach (var v in values)
                    _gltfDataWriter.Write(v);
            }

            foreach (var anim in animChunk.Animations.OrderBy(x => x.Name.ToLowerInvariant()))
                WriteAnimation(anim, keyTimeAccessors, keyPositionAccessors, keyRotationAccessors);
        }
    }

    private void AlignData(GltfAccessorComponentTypes type)
    {
        var s = type.GetSize();
        if (s % 4 == 2)
            s *= 2;
        else if (s % 4 != 0)
            s *= 4;

        var i = _gltfDataStream.Position % s;
        if (i == 0)
            return;
        for (; i < type.GetSize(); i++)
            _gltfDataStream.WriteByte(0xCC);
    }

    public class GltfExtensionKhrMaterialsSpecular
    {
        [JsonProperty("specularColorTexture", NullValueHandling = NullValueHandling.Ignore)]
        public GltfMaterialTextureSpecifier? SpecularColorTexture;
    }

    public class GltfExtensionMsftTextureDds
    {
        [JsonProperty("source")] public int Source;
    }

    public class GltfExtensions
    {
        [JsonProperty("KHR_materials_specular", NullValueHandling = NullValueHandling.Ignore)]
        public GltfExtensionKhrMaterialsSpecular? KhrMaterialsSpecular;

        [JsonProperty("MSFT_texture_dds", NullValueHandling = NullValueHandling.Ignore)]
        public GltfExtensionMsftTextureDds? MsftTextureDds;
    }

    private class GltfAsset
    {
        [JsonProperty("generator", NullValueHandling = NullValueHandling.Ignore)]
        public string? Generator = "Cryengine Converter";

        [JsonProperty("version")] public string Version = "2.0";
    }

    private class GltfScene
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string? Name;

        [JsonProperty("nodes")] public List<int> Nodes = new();
    }

    public class GltfNode
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string? Name;

        [JsonProperty("mesh", NullValueHandling = NullValueHandling.Ignore)]
        public int? Mesh;

        [JsonProperty("skin", NullValueHandling = NullValueHandling.Ignore)]
        public int? Skin;

        [JsonProperty("children", NullValueHandling = NullValueHandling.Ignore)]
        public List<int> Children = new();

        [JsonProperty("rotation", NullValueHandling = NullValueHandling.Ignore)]
        public List<float>? Rotation;

        [JsonProperty("scale", NullValueHandling = NullValueHandling.Ignore)]
        public List<float>? Scale;

        [JsonProperty("translation", NullValueHandling = NullValueHandling.Ignore)]
        public List<float>? Translation;

        public bool ShouldSerializeChildren() => Children.Any();
    }

    public class GltfMaterialTextureSpecifier
    {
        [JsonProperty("index")] public int Index;
    }

    public class GltfMaterialPbrMetallicRoughness
    {
        [JsonProperty("baseColorFactor", NullValueHandling = NullValueHandling.Ignore)]
        public Vector4? BaseColorFactor;

        [JsonProperty("baseColorTexture", NullValueHandling = NullValueHandling.Ignore)]
        public GltfMaterialTextureSpecifier? BaseColorTexture;

        [JsonProperty("metallicFactor", NullValueHandling = NullValueHandling.Ignore)]
        public float? MetallicFactor;

        [JsonProperty("metallicTexture", NullValueHandling = NullValueHandling.Ignore)]
        public GltfMaterialTextureSpecifier? MetallicTexture;

        [JsonProperty("roughnessFactor", NullValueHandling = NullValueHandling.Ignore)]
        public float? RoughnessFactor;

        [JsonProperty("metallicRoughnessTexture", NullValueHandling = NullValueHandling.Ignore)]
        public GltfMaterialTextureSpecifier? MetallicRoughnessTexture;
    }

    public class GltfMaterial
    {
        [JsonProperty("doubleSided", NullValueHandling = NullValueHandling.Ignore)]
        public bool? DoubleSided;

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string? Name;

        [JsonProperty("normalTexture", NullValueHandling = NullValueHandling.Ignore)]
        public GltfMaterialTextureSpecifier? NormalTexture;

        [JsonProperty("pbrMetallicRoughness", NullValueHandling = NullValueHandling.Ignore)]
        public GltfMaterialPbrMetallicRoughness? PbrMetallicRoughness;

        [JsonProperty("extensions", NullValueHandling = NullValueHandling.Ignore)]
        public GltfExtensions? Extensions;
    }

    public class GltfMeshPrimitiveAttributes
    {
        [JsonProperty("POSITION", NullValueHandling = NullValueHandling.Ignore)]
        public int? Position;

        [JsonProperty("NORMAL", NullValueHandling = NullValueHandling.Ignore)]
        public int? Normal;

        [JsonProperty("TANGENT", NullValueHandling = NullValueHandling.Ignore)]
        public int? Tangent;

        [JsonProperty("TEXCOORD_0", NullValueHandling = NullValueHandling.Ignore)]
        public int? TexCoord0;

        [JsonProperty("COLOR_0", NullValueHandling = NullValueHandling.Ignore)]
        public int? Color0;

        [JsonProperty("JOINTS_0", NullValueHandling = NullValueHandling.Ignore)]
        public int? Joints0;

        [JsonProperty("WEIGHTS_0", NullValueHandling = NullValueHandling.Ignore)]
        public int? Weights0;
    }

    public class GltfMeshPrimitive
    {
        [JsonProperty("attributes")] public GltfMeshPrimitiveAttributes Attributes = new();

        [JsonProperty("indices", NullValueHandling = NullValueHandling.Ignore)]
        public int? Indices;

        [JsonProperty("material", NullValueHandling = NullValueHandling.Ignore)]
        public int? Material;
    }

    public class GltfMesh
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string? Name;

        [JsonProperty("primitives")] public List<GltfMeshPrimitive> Primitives = new();
    }

    public enum GltfAccessorComponentTypes
    {
        s8 = 5120,
        u8 = 5121,
        s16 = 5122,
        u16 = 5123,
        u32 = 5125,
        f32 = 5126,
    }

    public enum GltfSamplerFilters
    {
        Nearest = 9728,
        Linear = 9729,
        NearestMipmapNearest = 9984,
        LinearMipmapNearest = 9985,
        NearestMipmapLinear = 9986,
        LinearMipmapLinear = 9987,
    }

    public enum GltfAccessorTypes
    {
        Scalar,
        Vec2,
        Vec3,
        Vec4,
        Mat2,
        Mat3,
        Mat4,
    }

    public enum GltfAnimationChannelTargetPath
    {
        Translation,
        Rotation,
        Scale,
        Weights,
    }

    public enum GltfAnimationSamplerInterpolation
    {
        Linear,
        Step,
        CubicSpline,
    }

    public class GltfAccessor
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string? Name;

        [JsonProperty("bufferView")] public int BufferView;

        [JsonProperty("byteOffset")] public int ByteOffset;

        [JsonProperty("componentType")] public GltfAccessorComponentTypes ComponentType;

        [JsonProperty("count")] public int Count;

        [JsonIgnore] public GltfAccessorTypes Type;

        [JsonProperty("min", NullValueHandling = NullValueHandling.Ignore)]
        public object? Min;

        [JsonProperty("max", NullValueHandling = NullValueHandling.Ignore)]
        public object? Max;

        [JsonProperty("type")]
        public string TypeString
        {
            get => Type switch
            {
                GltfAccessorTypes.Scalar => "SCALAR",
                GltfAccessorTypes.Vec2 => "VEC2",
                GltfAccessorTypes.Vec3 => "VEC3",
                GltfAccessorTypes.Vec4 => "VEC4",
                GltfAccessorTypes.Mat2 => "MAT2",
                GltfAccessorTypes.Mat3 => "MAT3",
                GltfAccessorTypes.Mat4 => "MAT4",
                _ => throw new ArgumentOutOfRangeException(),
            };
            set => Type = value switch
            {
                "SCALAR" => GltfAccessorTypes.Scalar,
                "VEC2" => GltfAccessorTypes.Vec2,
                "VEC3" => GltfAccessorTypes.Vec3,
                "VEC4" => GltfAccessorTypes.Vec4,
                "MAT2" => GltfAccessorTypes.Mat2,
                "MAT3" => GltfAccessorTypes.Mat3,
                "MAT4" => GltfAccessorTypes.Mat4,
                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        [JsonIgnore] public int RequiredByteLength => Type.GetScalarCount() * ComponentType.GetSize() * Count;
    }

    public class GltfBufferView
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string? Name;

        [JsonProperty("buffer")] public int Buffer;

        [JsonProperty("byteLength")] public long ByteLength;

        [JsonProperty("byteOffset")] public long ByteOffset;

        [JsonIgnore] public long ByteOffsetTo => ByteLength + ByteOffset;
    }

    public class GltfBuffer
    {
        [JsonProperty("byteLength")] public long ByteLength;

        [JsonProperty("uri", NullValueHandling = NullValueHandling.Ignore)]
        public string? Uri;
    }

    public class GltfImage
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string? Name;

        [JsonProperty("uri", NullValueHandling = NullValueHandling.Ignore)]
        public string? Uri;

        [JsonProperty("mimeType", NullValueHandling = NullValueHandling.Ignore)]
        public string? MimeType;

        [JsonProperty("bufferView", NullValueHandling = NullValueHandling.Ignore)]
        public int? BufferView;
    }

    public class GltfTexture
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string? Name;

        [JsonProperty("source", NullValueHandling = NullValueHandling.Ignore)]
        public int? Source;

        [JsonProperty("extensions", NullValueHandling = NullValueHandling.Ignore)]
        public GltfExtensions? Extensions;
    }

    public class GltfSkin
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string? Name;

        /// <summary>
        /// The index of the accessor containing the floating-point 4x4 inverse-bind matrices.
        /// </summary>
        /// <remarks>
        /// Its `accessor.count` property **MUST** be greater than or equal to the number of elements of the `joints`
        /// array. When undefined, each matrix is a 4x4 identity matrix.
        /// </remarks>
        [JsonProperty("inverseBindMatrices")] public int? InverseBindMatrices;

        [JsonProperty("joints")] public List<int> Joints = new();
    }

    public class GltfAnimationChannelTarget
    {
        /// <summary>
        /// The index of the node to animate. When undefined, the animated object **MAY** be defined by an extension.
        /// </summary>
        [JsonProperty("node", NullValueHandling = NullValueHandling.Ignore)]
        public int? Node;

        /// <summary>
        /// The name of the node's TRS property to animate, or the `"weights"` of the Morph Targets it instantiates.
        /// </summary>
        /// <remarks>
        /// For the `"translation"` property, the values that are provided by the sampler are the translation along the
        /// X, Y, and Z axes.
        /// For the `"rotation"` property, the values are a quaternion in the order (x, y, z, w), where w is the scalar.
        /// For the `"scale"` property, the values are the scaling factors along the X, Y, and Z axes.
        /// </remarks>
        [JsonIgnore] public GltfAnimationChannelTargetPath Path;

        [JsonProperty("path")]
        public string PathString
        {
            get => Path switch
            {
                GltfAnimationChannelTargetPath.Translation => "translation",
                GltfAnimationChannelTargetPath.Rotation => "rotation",
                GltfAnimationChannelTargetPath.Scale => "scale",
                GltfAnimationChannelTargetPath.Weights => "weights",
                _ => throw new ArgumentOutOfRangeException(),
            };
            set => Path = value switch
            {
                "translation" => GltfAnimationChannelTargetPath.Translation,
                "rotation" => GltfAnimationChannelTargetPath.Rotation,
                "scale" => GltfAnimationChannelTargetPath.Scale,
                "weights" => GltfAnimationChannelTargetPath.Weights,
                _ => throw new ArgumentOutOfRangeException(),
            };
        }
    }

    public class GltfAnimationChannel
    {
        /// <summary>
        /// The index of a sampler in this animation used to compute the value for the target.
        /// </summary>
        /// <remarks>
        /// e.g., a node's translation, rotation, or scale (TRS).
        /// </remarks>
        [JsonProperty("sampler")] public int Sampler;

        /// <summary>
        /// The descriptor of the animated property.
        /// </summary>
        [JsonProperty("target")] public GltfAnimationChannelTarget Target = null!;
    }

    public class GltfAnimationSampler
    {
        /// <summary>
        /// The index of an accessor containing keyframe timestamps.
        /// </summary>
        /// <remarks>
        /// The accessor **MUST** be of scalar type with floating-point components.
        /// The values represent time in seconds with `time[0] >= 0.0`, and strictly increasing values,
        /// i.e., `time[n + 1] > time[n]`.
        /// </remarks>
        [JsonProperty("input")] public int Input;

        /// <summary>
        /// The index of an accessor, containing keyframe output values.
        /// </summary>
        [JsonProperty("output")] public int Output;

        /// <summary>
        /// Interpolation algorithm.
        /// </summary>
        [JsonIgnore] public GltfAnimationSamplerInterpolation Interpolation;

        [JsonProperty("interpolation")]
        public string InterpolationString
        {
            get => Interpolation switch
            {
                GltfAnimationSamplerInterpolation.Linear => "LINEAR",
                GltfAnimationSamplerInterpolation.Step => "STEP",
                GltfAnimationSamplerInterpolation.CubicSpline => "CUBICSPLINE",
                _ => throw new ArgumentOutOfRangeException(),
            };
            set => Interpolation = value switch
            {
                "LINEAR" => GltfAnimationSamplerInterpolation.Linear,
                "STEP" => GltfAnimationSamplerInterpolation.Step,
                "CUBICSPLINE" => GltfAnimationSamplerInterpolation.CubicSpline,
                _ => throw new ArgumentOutOfRangeException(),
            };
        }
    }

    public class GltfAnimation
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string? Name;

        /// <summary>
        /// An array of animation channels.
        /// </summary>
        /// <remarks>
        /// An animation channel combines an animation sampler with a target property being animated.
        /// Different channels of the same animation **MUST NOT** have the same targets.
        /// </remarks>
        [JsonProperty("channels")] public List<GltfAnimationChannel> Channels = new();

        /// <summary>
        /// An array of animation samplers.
        /// </summary>
        /// <remarks>
        /// An animation sampler combines timestamps with a sequence of output values and defines an interpolation
        /// algorithm.
        /// </remarks>
        [JsonProperty("samplers")] public List<GltfAnimationSampler> Samplers = new();
    }

    public class GltfSampler
    {
        [JsonProperty("magFilter")] public GltfSamplerFilters MagFilter = GltfSamplerFilters.Linear;

        [JsonProperty("minFilter")] public GltfSamplerFilters MinFilter = GltfSamplerFilters.LinearMipmapLinear;
    }

    private class GltfRoot
    {
        [JsonProperty("asset")] public GltfAsset Asset = new();

        [JsonProperty("extensionsUsed")] public HashSet<string> ExtensionsUsed = new();

        [JsonProperty("scene")] public int Scene;

        [JsonProperty("scenes")] public List<GltfScene> Scenes = new();

        [JsonProperty("nodes")] public List<GltfNode> Nodes = new();

        [JsonProperty("animations")] public List<GltfAnimation> Animations = new();

        [JsonProperty("materials")] public List<GltfMaterial> Materials = new();

        [JsonProperty("meshes")] public List<GltfMesh> Meshes = new();

        [JsonProperty("textures")] public List<GltfTexture> Textures = new();

        [JsonProperty("images")] public List<GltfImage> Images = new();

        [JsonProperty("skins")] public List<GltfSkin> Skins = new();

        [JsonProperty("accessors")] public List<GltfAccessor> Accessors = new();

        [JsonProperty("bufferViews")] public List<GltfBufferView> BufferViews = new();

        [JsonProperty("samplers")] public List<GltfSampler> Samplers = new();

        [JsonProperty("buffers")] public List<GltfBuffer> Buffers = new();

        public bool ShouldSerializeExtensionsUsed() => ExtensionsUsed.Any();

        public bool ShouldSerializeAnimations() => Animations.Any();

        public bool ShouldSerializeSkins() => Skins.Any();
    }
}

internal static class GltfRendererInternalExtensions
{
    internal static int GetScalarCount(this GLTF.GltfAccessorTypes t) => t switch
    {
        GLTF.GltfAccessorTypes.Scalar => 1,
        GLTF.GltfAccessorTypes.Vec2 => 2,
        GLTF.GltfAccessorTypes.Vec3 => 3,
        GLTF.GltfAccessorTypes.Vec4 => 4,
        GLTF.GltfAccessorTypes.Mat2 => 4,
        GLTF.GltfAccessorTypes.Mat3 => 9,
        GLTF.GltfAccessorTypes.Mat4 => 16,
        _ => throw new ArgumentOutOfRangeException(nameof(t)),
    };

    internal static int GetSize(this GLTF.GltfAccessorComponentTypes t) => t switch
    {
        GLTF.GltfAccessorComponentTypes.s8 => 1,
        GLTF.GltfAccessorComponentTypes.u8 => 1,
        GLTF.GltfAccessorComponentTypes.s16 => 2,
        GLTF.GltfAccessorComponentTypes.u16 => 2,
        GLTF.GltfAccessorComponentTypes.u32 => 4,
        GLTF.GltfAccessorComponentTypes.f32 => 4,
        _ => throw new ArgumentOutOfRangeException(nameof(t)),
    };

    internal static void Write(this BinaryWriter bw, Vector3 v)
    {
        bw.Write(v.X);
        bw.Write(v.Y);
        bw.Write(v.Z);
    }
        
    internal static void Write(this BinaryWriter bw, Quaternion v)
    {
        bw.Write(v.X);
        bw.Write(v.Y);
        bw.Write(v.Z);
        bw.Write(v.W);
    }

    internal static void Write(this BinaryWriter bw, Tangent v)
    {
        bw.Write(v.x / 32767f);
        bw.Write(v.y / 32767f);
        bw.Write(v.z / 32767f);
        bw.Write(v.w / 32767f);
    }

    internal static void Write(this BinaryWriter bw, IRGBA v)
    {
        bw.Write(v.r / 255f);
        bw.Write(v.g / 255f);
        bw.Write(v.b / 255f);
        bw.Write(v.a / 255f);
    }

    internal static void Write(this BinaryWriter bw, UV v)
    {
        bw.Write(v.U);
        bw.Write(v.V);
    }

    internal static void Write(this BinaryWriter bw, Matrix4x4 v)
    {
        bw.Write(v.M11);
        bw.Write(v.M12);
        bw.Write(v.M13);
        bw.Write(v.M14);
        bw.Write(v.M21);
        bw.Write(v.M22);
        bw.Write(v.M23);
        bw.Write(v.M24);
        bw.Write(v.M31);
        bw.Write(v.M32);
        bw.Write(v.M33);
        bw.Write(v.M34);
        bw.Write(v.M41);
        bw.Write(v.M42);
        bw.Write(v.M43);
        bw.Write(v.M44);
    }
}