using CgfConverter.Models;
using CgfConverter.Utilities;
using Extensions;
using System;
using System.IO;
using System.Linq;
using System.Numerics;
using static Extensions.BinaryReaderExtensions;

namespace CgfConverter.CryEngineCore.Chunks;

internal sealed class ChunkIvoSkinMesh_900 : ChunkIvoSkinMesh
{
    /* 
     * Node IDs for Ivo models
     * 1: NodeChunk
     * 2: MeshChunk
     * 3: MeshSubsets
     * 4: Indices
     * 5: VertsUVs (contains vertices, UVs and colors)
     * 6: Normals
     * 7: Tangents
     * 8: Bonemap  (assume all #ivo files have armatures)
     * 9: Colors
     * 10: Colors2
     * 11: MtlName
     */

    public override void Read(BinaryReader b)
    {
        var model = _model;

        base.Read(b);
        SkipBytes(b, 4);  // Flags probably

        IvoGeometryMeshDetails meshDetails = b.ReadMeshDetails();
        MeshDetails = meshDetails;

        SkipBytes(b, 92);  // Unknown data.  All 0x00

        for (int i = 0; i < meshDetails.NumberOfSubmeshes; i++)
        {
            MeshSubsets.Add(b.ReadMeshSubset());
        }

        //ChunkMeshSubsets_900 subsetsChunk = new(meshChunk.NumVertSubsets);
        //// Create dummy header info here (ChunkType, version, size, offset)
        //subsetsChunk._model = _model;
        //subsetsChunk._header = _header;
        //subsetsChunk._header.Offset = (uint)b.BaseStream.Position;
        //subsetsChunk.Read(b);
        //subsetsChunk.ChunkType = ChunkType.MeshSubsets;
        //subsetsChunk.ID = 3;
        //model.ChunkMap.Add(subsetsChunk.ID, subsetsChunk);

        // Create dummy mtlName chunk
        //ChunkMtlName_800 mtlName = new();
        //mtlName._model = _model;
        //mtlName._header = _header;
        //mtlName._header.Offset = (uint)b.BaseStream.Position;
        //mtlName.ChunkType = ChunkType.MtlName;
        //mtlName.ID = 11;
        //model.ChunkMap.Add(mtlName.ID, mtlName);

        while (b.BaseStream.Position != b.BaseStream.Length)  // Read to end.
        {
            var datastreamType = b.ReadUInt32();
            var ivoDataStreamType = (DatastreamType)datastreamType;
            uint bytesPerElement = 0;

            switch (ivoDataStreamType)
            {
                case DatastreamType.IVOINDICES:
                    bytesPerElement = b.ReadUInt32();
                    Datastream<uint> indices = new(
                        DatastreamType.IVOINDICES,
                        meshDetails.NumberOfIndices,
                        bytesPerElement,
                        new uint[bytesPerElement]);
                    if (indices.BytesPerElement == 2)
                    {
                        for (int i = 0; i < meshDetails.NumberOfIndices; i++)
                        {
                            indices.Data[i] = b.ReadUInt16(); // casts to uint nicely
                        }
                    }
                    else if (indices.BytesPerElement == 4)
                    {
                        for (int i = 0; i < meshDetails.NumberOfIndices; i++)
                        {
                            indices.Data[i] = b.ReadUInt32();
                        }
                    }
                    Indices = indices;
                    b.AlignTo(8);
                    break;
                case DatastreamType.IVOVERTSUVS:
                case DatastreamType.IVOVERTSUVS2:
                    bytesPerElement = b.ReadUInt32();
                    Datastream<VertUV> vertUVs = new(
                        DatastreamType.IVOVERTSUVS2,
                        meshDetails.NumberOfVertices,
                        bytesPerElement,
                        new VertUV[bytesPerElement]);
                    if (vertUVs.BytesPerElement == 16)
                    {
                        for (int i = 0; i < meshDetails.NumberOfVertices; i++)
                        {
                            vertUVs.Data[i] = new VertUV
                            {
                                Vertex = b.ReadVector3(InputType.SNorm),
                                Skipped = b.ReadBytes(2),
                                Color = b.ReadIRGBA(),
                                UV = b.ReadUV(InputType.Half)
                            };
                        }
                    }
                    else if (vertUVs.BytesPerElement == 20)
                    {
                        for (int i = 0; i < meshDetails.NumberOfVertices; i++)
                        {
                            vertUVs.Data[i] = new VertUV
                            {
                                Vertex = b.ReadVector3(InputType.Single),
                                Color = b.ReadIRGBA(),
                                UV = b.ReadUV(InputType.Half)
                            };
                        }
                    }
                    VertsUvs = vertUVs;
                    b.AlignTo(8);
                    break;
                case DatastreamType.IVONORMALS:
                case DatastreamType.IVONORMALS2:
                    bytesPerElement = b.ReadUInt32();
                    Datastream<Vector3> normals = new(
                        DatastreamType.IVONORMALS2,
                        meshDetails.NumberOfVertices,
                        bytesPerElement,
                        new Vector3[bytesPerElement]);
                    if (normals.BytesPerElement == 4)
                    {
                        for (int i = 0; i < meshDetails.NumberOfVertices; i++)
                        {
                            var x = (float)b.ReadCryHalf();
                            var y = (float)b.ReadCryHalf();

                            if (Math.Abs(x) > 1.05f || Math.Abs(y) > 1.05f)
                                throw new InvalidDataException($"Invalid normal components at vertex {i}: ({x}, {y})");

                            // Check if x²+y² <= 1 (required for valid unit vector)
                            float sumSquares = x * x + y * y;
                            if (sumSquares > 1.05f)
                                throw new InvalidDataException($"Invalid normal magnitude at vertex {i}: x²+y²={sumSquares}");

                            float z = (float)Math.Sqrt(1.0f - sumSquares);
                            normals.Data[i] = new Vector3(x, y, z);
                        }
                    }
                    else if (normals.BytesPerElement == 12)
                    {
                        for (int i = 0; i < meshDetails.NumberOfVertices; i++)
                        {
                            normals.Data[i] = b.ReadVector3();
                        }
                    }
                    Normals = normals;
                    b.AlignTo(8);
                    break;
                case DatastreamType.IVOTANGENTS:
                    bytesPerElement = b.ReadUInt32();
                    Datastream<Quaternion> tangents = new(
                        DatastreamType.IVOTANGENTS,
                        meshDetails.NumberOfVertices,
                        bytesPerElement,
                        new Quaternion[meshDetails.NumberOfVertices]);
                    Datastream<Quaternion> bitangents = new(
                        DatastreamType.IVOTANGENTS,
                        meshDetails.NumberOfVertices,
                        bytesPerElement,
                        new Quaternion[meshDetails.NumberOfVertices]);

                    if (bytesPerElement == 8)
                    {
                        for (int i = 0; i < meshDetails.NumberOfVertices; i++)
                        {
                            tangents.Data[i] = b.ReadQuaternion(InputType.SNorm);
                        }
                    }
                    else if (tangents.BytesPerElement == 16)
                    {
                        for (int i = 0; i < meshDetails.NumberOfVertices; i++)
                        {
                            tangents.Data[i] = b.ReadQuaternion(InputType.SNorm);
                            bitangents.Data[i] = b.ReadQuaternion(InputType.SNorm);
                        }
                    }
                    else
                        throw new NotSupportedException($"Unsupported tangents format: {tangents.BytesPerElement}");
                    Tangents = tangents;
                    BiTangents = bitangents;
                    b.AlignTo(8);
                    break;
                case DatastreamType.IVOQTANGENTS:
                    // For Ivo files, these are qtangents using SNORM (int16 I think).  8 bytes.
                    bytesPerElement = b.ReadUInt32();
                    Datastream<Quaternion> qtangents = new(
                        DatastreamType.IVOTANGENTS,
                        meshDetails.NumberOfVertices,
                        bytesPerElement,
                        new Quaternion[meshDetails.NumberOfVertices]);
                    Datastream<Vector3> normals2 = new(
                        DatastreamType.NORMALS,
                        meshDetails.NumberOfVertices,
                        bytesPerElement,
                        new Vector3[meshDetails.NumberOfVertices]);
                    if (qtangents.BytesPerElement == 8)
                    {
                        for (int i = 0; i < meshDetails.NumberOfVertices; i++)
                        {
                            Quaternion q = b.ReadQuaternion(InputType.SNorm);
                            qtangents.Data[i] = q;
                            normals2.Data[i] = q.GetNormalFromQTangent();
                        }
                    }
                    else if (qtangents.BytesPerElement == 16)
                    {
                        for (int i = 0; i < meshDetails.NumberOfVertices; i++)
                        {
                            // TODO: Finish this or ignore.
                            Quaternion q = b.ReadQuaternion(InputType.Single);
                            qtangents.Data[i] = q;
                            normals2.Data[i] = q.GetNormalFromQTangent();
                        }
                    }
                    QTangents = qtangents;
                    Normals = normals2;
                    b.AlignTo(8);
                    break;
                case DatastreamType.IVOBONEMAP32:
                case DatastreamType.IVOBONEMAP:
                    bytesPerElement = b.ReadUInt32();
                    Datastream<MeshBoneMapping> boneMaps = new(
                        DatastreamType.IVOBONEMAP,
                        meshDetails.NumberOfIndices,
                        bytesPerElement,
                        new MeshBoneMapping[bytesPerElement]);
                    if (boneMaps.BytesPerElement == 12) // 4 ushort, 4 ubytes
                    {
                        for (int i = 0; i < meshDetails.NumberOfVertices; i++)
                        {
                            MeshBoneMapping bm = new()
                            {
                                BoneIndex = new int[4],
                                Weight = new float[4]
                            };
                            for (int j = 0; j < 4; j++)
                            {
                                bm.BoneIndex[j] = b.ReadUInt16();
                            }
                            for (int j = 0; j < 4; j++)
                            {
                                bm.Weight[j] = b.ReadByte() / 255.0f;
                            }

                            boneMaps.Data[i] = bm;
                        }
                        
                    }
                    else if (boneMaps.BytesPerElement == 8) // older format, rare
                    {
                        for (int i = 0; i < meshDetails.NumberOfVertices; i++)
                        {
                            MeshBoneMapping bm = new()
                            {
                                BoneIndex = new int[4],
                                Weight = new float[4]
                            };
                            for (int j = 0; j < 4; j++)
                            {
                                bm.BoneIndex[j] = b.ReadByte();
                            }
                            for (int j = 0; j < 4; j++)
                            {
                                bm.Weight[j] = b.ReadByte() / 255.0f;
                            }

                            boneMaps.Data[i] = bm;
                        }
                    }
                    BoneMappings = boneMaps;
                    b.AlignTo(8);
                    break;
                case DatastreamType.IVOCOLORS2:
                    bytesPerElement = b.ReadUInt32();
                    Datastream<IRGBA> colors = new(
                        DatastreamType.COLORS,
                        meshDetails.NumberOfVertices,
                        bytesPerElement,
                        new IRGBA[meshDetails.NumberOfVertices]);
                    if (bytesPerElement == 4)
                    {
                        for (int i = 0; i < meshDetails.NumberOfVertices; i++)
                        {
                            colors.Data[i] = b.ReadIRGBA();
                        }
                    }
                    else
                    {
                        SkipBytes(b, bytesPerElement * meshDetails.NumberOfVertices);
                    }
                    b.AlignTo(8);
                    Colors = colors;
                    break;
                case DatastreamType.IVOUNKNOWN:
                    bytesPerElement = b.ReadUInt32();
                    SkipBytes(b, bytesPerElement * meshDetails.NumberOfVertices);
                    b.AlignTo(8);
                    break;
                default:
                    HelperMethods.Log(LogLevelEnum.Warning, $"***** Unknown DataStream Type {ivoDataStreamType} *****");
                    b.BaseStream.Position = b.BaseStream.Position + 4;
                    break;
            }
        }

        // Create GeometryInfo
        GeometryInfo = new()
        {
            GeometrySubsets = MeshSubsets,
            Vertices = new Datastream<Vector3[]>(DatastreamType.VERTICES, 0, 0, VertsUvs?.Data.Select(a => a.Vertex).ToArray() ?? []),
            Normals =   Normals?.Data ?? [],
            UVs = VertsUvs?.Data.Select(a => a.UV).ToList() ?? [],
            Colors = VertsUvs?.Data.Select(a => a.Color).ToList() ?? [],
            Indices = Indices.Data,
            BoneMappings = BoneMappings?.Data,
            BoundingBox = meshDetails.BoundingBox
        };
    }
}
