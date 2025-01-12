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
            var ivoDataStreamType = (IvoDatastreamType)datastreamType;

            switch (ivoDataStreamType)
            {
                case IvoDatastreamType.IVOINDICES:
                    IvoDatastream<uint> indices = new();
                    indices.DatastreamType = IvoDatastreamType.IVOINDICES;
                    indices.BytesPerElement = b.ReadUInt32();
                    indices.NumberOfElements = meshDetails.NumberOfIndices;
                    if (indices.BytesPerElement == 2)
                    {
                        for (int i = 0; i < meshDetails.NumberOfIndices; i++)
                        {
                            indices.Values.Add(b.ReadUInt16()); // casts to uint nicely
                        }
                    }
                    else if (indices.BytesPerElement == 4)
                    {
                        for ( int i = 0; i < meshDetails.NumberOfIndices; i++)
                        {
                            indices.Values.Add(b.ReadUInt32());
                        }
                    }
                    Indices = indices;
                    break;
                case IvoDatastreamType.IVOVERTSUVS:
                case IvoDatastreamType.IVOVERTSUVS2:
                    IvoDatastream<VertUV> vertUVs = new()
                    {
                        DatastreamType = IvoDatastreamType.IVOVERTSUVS2,
                        BytesPerElement = b.ReadUInt32(),
                        NumberOfElements = meshDetails.NumberOfVertices
                    };
                    if (vertUVs.BytesPerElement == 16)
                    {
                        for (int i = 0; i < meshDetails.NumberOfVertices; i++)
                        {
                            vertUVs.Values.Add(new VertUV
                            {
                                Vertex = b.ReadVector3(InputType.SNorm),
                                Skipped = b.ReadBytes(2),
                                Color = b.ReadIRGBA(),
                                UV = b.ReadUV(InputType.Half)
                            });
                        }
                    }
                    else if (vertUVs.BytesPerElement == 20)
                    {
                        for (int i = 0; i < meshDetails.NumberOfVertices; i++)
                        {
                            vertUVs.Values.Add(new VertUV
                            {
                                Vertex = b.ReadVector3(InputType.Single),
                                Color = b.ReadIRGBA(),
                                UV = b.ReadUV(InputType.Half)
                            });
                        }
                    }
                    VertsUvs = vertUVs;
                    break;
                case IvoDatastreamType.IVONORMALS:
                case IvoDatastreamType.IVONORMALS2:
                    IvoDatastream<Vector3> normals = new()
                    {
                        DatastreamType = IvoDatastreamType.IVONORMALS2,
                        BytesPerElement = b.ReadUInt32(),
                        NumberOfElements = meshDetails.NumberOfVertices
                    };
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
                            normals.Values.Add(new Vector3(x, y, z));
                        }
                    }
                    else if (normals.BytesPerElement == 12)
                    {
                        for (int i = 0; i < meshDetails.NumberOfVertices; i++)
                        {
                            normals.Values.Add(b.ReadVector3());
                        }
                    }
                    Normals = normals;
                    break;
                case IvoDatastreamType.IVOTANGENTS:
                    IvoDatastream<Quaternion> tangents = new()
                    {
                        DatastreamType = IvoDatastreamType.IVOTANGENTS,
                        BytesPerElement = b.ReadUInt32(),
                        NumberOfElements = meshDetails.NumberOfVertices
                    };
                    if (tangents.BytesPerElement == 8)
                    {
                        for (int i = 0; i < meshDetails.NumberOfVertices; i++)
                        {
                            Quaternion q = b.ReadQuaternion(InputType.SNorm);
                            tangents.Values.Add(q);
                        }
                    }
                    break;
                case IvoDatastreamType.IVOQTANGENTS:
                    // For Ivo files, these are qtangents using SNORM (int16 I think).  8 bytes.
                    IvoDatastream<Quaternion> qtangents = new()
                    {
                        DatastreamType = IvoDatastreamType.IVOTANGENTS,
                        BytesPerElement = b.ReadUInt32(),
                        NumberOfElements = meshDetails.NumberOfVertices
                    };
                    IvoDatastream<Vector3> normals2 = new()
                    {
                        DatastreamType = IvoDatastreamType.IVONORMALS,
                        BytesPerElement = 12,
                        NumberOfElements = meshDetails.NumberOfVertices
                    };
                    if (qtangents.BytesPerElement == 8)
                    {
                        for (int i = 0; i < meshDetails.NumberOfVertices; i++)
                        {
                            Quaternion q = b.ReadQuaternion(InputType.SNorm);
                            qtangents.Values.Add(q);
                            var normal = q.GetNormalFromQTangent();
                            normals2.Values.Add(normal);
                        }
                    }
                    else if (qtangents.BytesPerElement == 16)
                    {
                        for (int i = 0; i < meshDetails.NumberOfVertices; i++)
                        {
                            // TODO: Finish this or ignore.
                            qtangents.Values.Add(b.ReadQuaternion(InputType.Single));
                        }
                    }
                    QTangents = qtangents;
                    Normals = normals2;
                    //Tangents = qtangents;
                    //ChunkDataStream_900 tangents = new((uint)meshChunk.NumVertices);
                    //tangents._model = _model;
                    //tangents._header = _header;
                    //tangents._header.Offset = (uint)b.BaseStream.Position;
                    //tangents.Read(b);
                    //tangents.DataStreamType = DatastreamType.TANGENTS;
                    //tangents.ChunkType = ChunkType.DataStream;
                    //tangents.ID = 7;
                    //model.ChunkMap.Add(tangents.ID, tangents);

                    //if (!model.ChunkMap.ContainsKey(6))
                    //{
                    //    // Create a normals chunk from Tangents data
                    //    ChunkDataStream_900 norms = new((uint)meshChunk.NumVertices);
                    //    norms._model = _model;
                    //    norms._header = _header;
                    //    norms.ChunkType = ChunkType.DataStream;
                    //    norms.BytesPerElement = 4;
                    //    norms.DataStreamType = DatastreamType.NORMALS;
                    //    norms.Normals = tangents.Normals;
                    //    norms.ID = 6;
                    //    model.ChunkMap.Add(norms.ID, norms);
                    //}
                    break;
                case IvoDatastreamType.IVOBONEMAP32:
                case IvoDatastreamType.IVOBONEMAP:
                    IvoDatastream<MeshBoneMapping> boneMaps = new();
                    boneMaps.DatastreamType = IvoDatastreamType.IVOBONEMAP;
                    boneMaps.BytesPerElement = b.ReadUInt32();
                    boneMaps.NumberOfElements = meshDetails.NumberOfIndices;
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

                            boneMaps.Values.Add(bm);
                        }
                        BoneMappings = boneMaps;
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

                            boneMaps.Values.Add(bm);
                        }
                        BoneMappings = boneMaps;
                    }
                    break;
                case IvoDatastreamType.IVOCOLORS2:
                    //ChunkDataStream_900 colors2 = new((uint)meshChunk.NumVertices);
                    //colors2._model = _model;
                    //colors2._header = _header;
                    //colors2._header.Offset = (uint)b.BaseStream.Position;
                    //colors2.Read(b);
                    //colors2.ChunkType = ChunkType.DataStream;
                    //colors2.BytesPerElement = 4;
                    //colors2.DataStreamType = DatastreamType.COLORS2;
                    //colors2.ID = 10;
                    //model.ChunkMap.Add(colors2.ID, colors2);
                    break;
                default:
                    b.BaseStream.Position = b.BaseStream.Position + 4;
                    break;
            }
        }

        // Create GeometryInfo
        GeometryInfo = new()
        {
            GeometrySubsets = MeshSubsets,
            Vertices = VertsUvs?.Values.Select(a => a.Vertex).ToList() ?? [],
            Normals = Normals?.Values ?? [],
            UVs = VertsUvs?.Values.Select(a => a.UV).ToList() ?? [],
            Colors = VertsUvs?.Values.Select(a => a.Color).ToList() ?? [],
            Indices = Indices.Values,
            BoneMappings = BoneMappings?.Values,
            BoundingBox = meshDetails.BoundingBox
        };
    }
}
