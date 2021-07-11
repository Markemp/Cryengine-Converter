using BinaryReaderExtensions;
using System;
using System.Collections.Generic;
using System.IO;

namespace CgfConverter.CryEngineCore
{
    class ChunkDataStream_900 : ChunkDataStream
    {
        public ChunkDataStream_900(uint numberOfElements)
        {
            NumElements = numberOfElements;
        }

        public override void Read(BinaryReader b)
        {
            base.Read(b);

            uint dataStreamType = b.ReadUInt32();
            DataStreamType = (DatastreamType)Enum.ToObject(typeof(DatastreamType), dataStreamType);
            BytesPerElement = b.ReadUInt32();

            switch (DataStreamType)
            {
                case DatastreamType.IVOINDICES:
                    Indices = new uint[NumElements];
                    if (BytesPerElement == 2)
                    {
                        for (int i = 0; i < NumElements; i++)
                        {
                            Indices[i] = b.ReadUInt16();
                        }
                        if (NumElements % 2 == 1)
                            SkipBytes(b, 2);
                        else
                            SkipBytes(b, 4);
                    }
                    else if (BytesPerElement == 4)
                    {
                        for (int i = 0; i < NumElements; i++)
                        {
                            Indices[i] = b.ReadUInt32();
                        }
                    }
                    break;
                case DatastreamType.IVOVERTSUVS:
                    Vertices = new Vector3[NumElements];
                    Normals = new Vector3[NumElements];
                    Colors = new IRGBA[NumElements];
                    UVs = new UV[NumElements];
                    switch (BytesPerElement)  // new Star Citizen files
                    {
                        case 20:
                            for (int i = 0; i < NumElements; i++)
                            {
                                Vertices[i].x = b.ReadSingle();
                                Vertices[i].y = b.ReadSingle();
                                Vertices[i].z = b.ReadSingle(); // For some reason, skins are an extra 1 meter in the z direction.

                                Colors[i] = b.ReadColor();

                                Half uvu = new Half();
                                uvu.bits = b.ReadUInt16();
                                UVs[i].U = uvu.ToSingle();

                                Half uvv = new Half();
                                uvv.bits = b.ReadUInt16();
                                UVs[i].V = uvv.ToSingle();
                            }
                            if (NumElements % 2 == 1)
                            {
                                SkipBytes(b, 4);
                            }
                            break;
                    }
                    break;
                case DatastreamType.IVONORMALS:
                    switch (BytesPerElement)
                    {
                        case 4:
                            Normals = new Vector3[NumElements];
                            for (int i = 0; i < NumElements; i++)
                            {
                                var x = b.ReadSByte() / 128.0;
                                var y = b.ReadSByte() / 128.0;
                                var z = b.ReadSByte() / 128.0;
                                var w = b.ReadSByte() / 128.0;
                                Normals[i].x = (2.0 * (x * z + y * w));
                                Normals[i].y = (2.0 * (y * z - x * w));
                                Normals[i].z = (2.0 * (z * z + w * w)) - 1.0;
                            }
                            if (NumElements % 2 == 1)
                            {
                                SkipBytes(b, 4);
                            }
                            break;
                        default:
                            Utils.Log("Unknown Normals Format");
                            for (int i = 0; i < NumElements; i++)
                            {
                                SkipBytes(b, BytesPerElement);
                            }
                            break;
                    }
                    break;
                case DatastreamType.IVOTANGENTS:
                    Tangents = new Tangent[NumElements, 2];
                    Normals = new Vector3[NumElements];
                    for (int i = 0; i < NumElements; i++)
                    {
                        Tangents[i, 0].w = b.ReadSByte() / 127f;
                        Tangents[i, 0].x = b.ReadSByte() / 127f;
                        Tangents[i, 0].y = b.ReadSByte() / 127f;
                        Tangents[i, 0].z = b.ReadSByte() / 127f;

                        // Binormal
                        Tangents[i, 1].w = b.ReadSByte() / 127f;
                        Tangents[i, 1].x = b.ReadSByte() / 127f;
                        Tangents[i, 1].y = b.ReadSByte() / 127f;
                        Tangents[i, 1].z = b.ReadSByte() / 127f;

                        // Calculate the normal based on the cross product of the tangents.
                        Normals[i].x = (Tangents[i, 0].y * Tangents[i, 1].z - Tangents[i, 0].z * Tangents[i, 1].y);
                        Normals[i].y = 0 - (Tangents[i, 0].x * Tangents[i, 1].z - Tangents[i, 0].z * Tangents[i, 1].x);
                        Normals[i].z = (Tangents[i, 0].x * Tangents[i, 1].y - Tangents[i, 0].y * Tangents[i, 1].x);

                        //// These have to be divided by 32767 to be used properly (value between -1 and 1)
                        //// Tangent
                        //Tangents[i, 0].x = b.ReadInt16() / 32767;
                        //Tangents[i, 0].y = b.ReadInt16() / 32767;
                        //Tangents[i, 0].z = b.ReadInt16() / 32767;
                        //Tangents[i, 0].w = b.ReadInt16() / 32767;

                        //Normals[i].x = (2.0 * (Tangents[i, 0].x * Tangents[i, 0].z + Tangents[i, 0].y * Tangents[i, 0].w));
                        //Normals[i].y = (2.0 * (Tangents[i, 0].y * Tangents[i, 0].z - Tangents[i, 0].x * Tangents[i, 0].w));
                        //Normals[i].z = (2.0 * (Tangents[i, 0].z * Tangents[i, 0].z + Tangents[i, 0].w * Tangents[i, 0].w)) - 1.0;

                        //// Binormal
                        ////Tangents[i, 1].x = b.ReadSByte() / 127;
                        ////Tangents[i, 1].y = b.ReadSByte() / 127;
                        ////Tangents[i, 1].z = b.ReadSByte() / 127;
                        ////Tangents[i, 1].w = b.ReadSByte() / 127;
                    }
                    break;
                case DatastreamType.IVOBONEMAP:
                    SkinningInfo skin = GetSkinningInfo();
                    skin.HasBoneMapDatastream = true;

                    skin.BoneMapping = new List<MeshBoneMapping>();
                    MeshBoneMapping tmpMap = new MeshBoneMapping();
                    switch (BytesPerElement)
                    {
                        case 12:
                            for (int i = 0; i < NumElements; i++)
                            {
                                tmpMap.BoneIndex = new int[4];
                                tmpMap.Weight = new int[4];

                                for (int j = 0; j < 4; j++)         // read the 4 bone indexes first
                                {
                                    tmpMap.BoneIndex[j] = b.ReadUInt16();

                                }
                                for (int j = 0; j < 4; j++)           // read the weights.
                                {
                                    tmpMap.Weight[j] = b.ReadByte();
                                }
                                skin.BoneMapping.Add(tmpMap);
                            }
                            if (NumElements % 2 == 1)
                            {
                                SkipBytes(b, 4);
                            }
                            break;
                        default:
                            Utils.Log("Unknown BoneMapping structure");
                            break;
                    }

                    break;

            }
        }
    }
}
