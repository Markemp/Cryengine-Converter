using CgfConverter.Models;
using CgfConverter.Utilities;
using Extensions;
using System;
using System.IO;
using System.Numerics;
using static Extensions.BinaryReaderExtensions;

namespace CgfConverter.CryEngineCore.Chunks;

internal sealed class ChunkIvoSkinMesh_900 : ChunkIvoSkinMesh
{
    public override void Read(BinaryReader b)
    {
        base.Read(b);
        SkipBytes(b, 4);  // Flags probably

        IvoGeometryMeshDetails meshDetails = b.ReadMeshDetails();
        MeshDetails = meshDetails;

        SkipBytes(b, 92);  // Unknown data.  All 0x00

        for (int i = 0; i < meshDetails.NumberOfSubmeshes; i++)
        {
            MeshSubsets.Add(b.ReadMeshSubset());
        }
        bool hasReadIndex = false;

        while (b.BaseStream.Position != b.BaseStream.Length)  // Read to end.
        {
            var datastreamType = b.ReadUInt32();
            var ivoDataStreamType = (DatastreamType)datastreamType;
            uint bytesPerElement = 0;

            switch (ivoDataStreamType)
            {
                case DatastreamType.IVOINDICES:
                    if (hasReadIndex) return;
                    hasReadIndex = true;
                    bytesPerElement = b.ReadUInt32();
                    Datastream<uint> indices = new(
                        DatastreamType.IVOINDICES,
                        meshDetails.NumberOfIndices,
                        bytesPerElement,
                        new uint[meshDetails.NumberOfIndices]);
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
                        new VertUV[meshDetails.NumberOfVertices]);
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
                        new Vector3[meshDetails.NumberOfVertices]);
                    if (normals.BytesPerElement == 4)
                    {
                        for (int i = 0; i < meshDetails.NumberOfVertices; i++)
                        {
                            var x = (float)b.ReadCryHalf();
                            var y = (float)b.ReadCryHalf();

                            //if (Math.Abs(x) > 1.05f || Math.Abs(y) > 1.05f)
                            //    throw new InvalidDataException($"Invalid normal components at vertex {i}: ({x}, {y})");

                            //// Check if x²+y² <= 1 (required for valid unit vector)
                            //float sumSquares = x * x + y * y;
                            //if (sumSquares > 1.05f)
                            //    throw new InvalidDataException($"Invalid normal magnitude at vertex {i}: x²+y²={sumSquares}");

                            //float z = (float)Math.Sqrt(1.0f - sumSquares);
                            //normals.Data[i] = new Vector3(x, y, z);
                            normals.Data[i] = new Vector3(x, y, 0.0f);
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
                        meshDetails.NumberOfVertices,
                        bytesPerElement,
                        new MeshBoneMapping[meshDetails.NumberOfVertices]);
                    if (boneMaps.BytesPerElement == 12) // 4 ushort, 4 ubytes
                    {
                        for (int i = 0; i < meshDetails.NumberOfVertices; i++)
                        {
                            MeshBoneMapping bm = new()
                            {
                                BoneIndex = new ushort[4],
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
                    else if (boneMaps.BytesPerElement == 24)
                    {
                        // 8 ushorts, 8 bytes / 255
                        for (int i =0; i < meshDetails.NumberOfVertices; i++)
                        {
                            MeshBoneMapping bm = new()
                            {
                                BoneInfluenceCount = 8,
                                BoneIndex = new ushort[8],
                                Weight = new float[8]
                            };
                            for (int j = 0; j < 8; j++)
                            {
                                bm.BoneIndex[j] = b.ReadUInt16();
                            }
                            for (int j = 0; j < 8; j++)
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
                                BoneIndex = new ushort[4],
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
                    HelperMethods.Log(LogLevelEnum.Warning, $"***** Unknown DataStream Type 0x{(int)ivoDataStreamType:X8} *****");
                    b.BaseStream.Position = b.BaseStream.Position + 4;
                    break;
            }
        }
    }
}
