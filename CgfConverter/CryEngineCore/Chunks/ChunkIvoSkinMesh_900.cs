using CgfConverter.Models;
using CgfConverter.Models.Structs;
using CgfConverter.Utilities;
using Extensions;
using System;
using System.IO;
using System.Numerics;
using static Extensions.BinaryReaderExtensions;

namespace CgfConverter.CryEngineCore.Chunks;

internal sealed class ChunkIvoSkinMesh_900 : ChunkIvoSkinMesh
{
    /// <summary>
    /// Decode a packed unit vector from a u32 using 15-15-1-1 bit layout.
    /// Bits 0-14: X biased by 0x3FFF, bits 15-29: Y biased by 0x3FFF,
    /// bit 30: Z sign, bit 31: reserved (bitangent sign in caller).
    /// </summary>
    private static Vector3 DecodePackedUnitVector(uint val)
    {
        const float scale = 1.0f / 16383.0f;

        int xRaw = (int)(val & 0x7FFF) - 0x3FFF;
        int yRaw = (int)((val >> 15) & 0x7FFF) - 0x3FFF;
        float x = xRaw * scale;
        float y = yRaw * scale;

        float zSq = MathF.Max(0, 1.0f - x * x - y * y);
        float zUnsigned = MathF.Sqrt(zSq);
        float z = ((val >> 30) & 1) != 0 ? -zUnsigned : zUnsigned;

        float len = MathF.Sqrt(x * x + y * y + z * z);
        return len > 1e-8f
            ? new Vector3(x / len, y / len, z / len)
            : new Vector3(0, 0, 1);
    }

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

        // Calculate chunk end position. Use Size if available, otherwise read to end of file.
        long chunkEndPosition = Size > 0 ? Offset + Size : b.BaseStream.Length;
        while (b.BaseStream.Position < chunkEndPosition)  // Read to end of chunk.
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
                    if (bytesPerElement == 8)
                    {
                        // 8-byte format: 4× i16 SNorm quaternion (XYZW / 32767.0)
                        // Normal = rotation matrix column 2, negated if w < 0.
                        Datastream<Vector3> tangentNormals = new(
                            DatastreamType.NORMALS,
                            meshDetails.NumberOfVertices,
                            bytesPerElement,
                            new Vector3[meshDetails.NumberOfVertices]);

                        for (int i = 0; i < meshDetails.NumberOfVertices; i++)
                        {
                            float qx = b.ReadInt16() / 32767.0f;
                            float qy = b.ReadInt16() / 32767.0f;
                            float qz = b.ReadInt16() / 32767.0f;
                            float qw = b.ReadInt16() / 32767.0f;

                            // Column 2 of rotation matrix = normal direction
                            float nx = 2.0f * (qx * qz + qw * qy);
                            float ny = 2.0f * (qy * qz - qw * qx);
                            float nz = 1.0f - 2.0f * (qx * qx + qy * qy);

                            // CryEngine convention: negate normal if w < 0
                            if (qw < 0.0f)
                            {
                                nx = -nx;
                                ny = -ny;
                                nz = -nz;
                            }

                            float len = MathF.Sqrt(nx * nx + ny * ny + nz * nz);
                            tangentNormals.Data[i] = len > 1e-8f
                                ? new Vector3(nx / len, ny / len, nz / len)
                                : new Vector3(0, 0, 1);
                        }
                        Normals = tangentNormals;
                    }
                    else if (bytesPerElement == 16)
                    {
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

                        for (int i = 0; i < meshDetails.NumberOfVertices; i++)
                        {
                            tangents.Data[i] = b.ReadQuaternion(InputType.SNorm);
                            bitangents.Data[i] = b.ReadQuaternion(InputType.SNorm);
                        }
                        Tangents = tangents;
                        BiTangents = bitangents;
                    }
                    else
                        throw new NotSupportedException($"Unsupported tangents format: {bytesPerElement}");
                    b.AlignTo(8);
                    break;
                case DatastreamType.IVOQTANGENTS:
                    // IVOQTANGENTS 8-byte format: 2× u32 packed unit vectors (15-15-1-1 bit layout).
                    // Despite the name, these are NOT quaternions — they are compressed tangent/bitangent vectors.
                    // Normal = cross(tangent, bitangent) × bitangent_sign.
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
                            uint value1 = b.ReadUInt16() | ((uint)b.ReadUInt16() << 16);
                            uint value2 = b.ReadUInt16() | ((uint)b.ReadUInt16() << 16);

                            Vector3 tangent = DecodePackedUnitVector(value1);
                            Vector3 bitangent = DecodePackedUnitVector(value2);

                            // Bitangent sign from bit 31 of first u32
                            float bitanSign = (value1 & 0x80000000) != 0 ? -1.0f : 1.0f;

                            // Normal = cross(tangent, bitangent) × bitangent_sign
                            Vector3 cross = Vector3.Cross(tangent, bitangent) * bitanSign;
                            float len = cross.Length();
                            normals2.Data[i] = len > 1e-8f
                                ? cross / len
                                : new Vector3(0, 0, 1);
                        }
                    }
                    else if (qtangents.BytesPerElement == 16)
                    {
                        for (int i = 0; i < meshDetails.NumberOfVertices; i++)
                        {
                            // 16-byte format uses direct quaternion
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
                case DatastreamType.IVOSIMPLEBONEMAP:
                    // Simple bone mapping: single ushort bone index per vertex
                    // Used for rigid attachment meshes where each vertex is influenced by a single bone
                    // Weight is implied 1.0 (100%) since there's only one influence
                    bytesPerElement = b.ReadUInt32();
                    if (bytesPerElement == 2)
                    {
                        Datastream<MeshBoneMapping> simpleBoneMaps = new(
                            DatastreamType.IVOSIMPLEBONEMAP,
                            meshDetails.NumberOfVertices,
                            bytesPerElement,
                            new MeshBoneMapping[meshDetails.NumberOfVertices]);
                        for (int i = 0; i < meshDetails.NumberOfVertices; i++)
                        {
                            ushort boneIndex = b.ReadUInt16();
                            MeshBoneMapping bm = new()
                            {
                                BoneInfluenceCount = 1,
                                BoneIndex = [boneIndex, 0, 0, 0],
                                Weight = [1.0f, 0, 0, 0]
                            };
                            simpleBoneMaps.Data[i] = bm;
                        }
                        BoneMappings = simpleBoneMaps;
                    }
                    else
                    {
                        HelperMethods.Log(LogLevelEnum.Warning, $"Unexpected simple bone map bytes per element: {bytesPerElement}");
                        SkipBytes(b, bytesPerElement * meshDetails.NumberOfVertices);
                    }
                    b.AlignTo(8);
                    break;
                default:
                    // Unknown datastream type - skip the 4 bytes we just read (the type) and continue
                    // The chunk size-based loop will handle reaching the end of the chunk
                    HelperMethods.Log(LogLevelEnum.Warning, $"***** Unknown DataStream Type 0x{datastreamType:X8} at position 0x{b.BaseStream.Position - 4:X} - skipping and continuing *****");
                    // Position is already advanced by ReadUInt32(), so we just continue
                    // Align to 8 bytes in case the next datastream expects alignment
                    b.AlignTo(8);
                    break;
            }
        }
    }
}
