using Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

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
                #region IVOINDICES
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
                        {
                            var peek = Convert.ToChar(b.ReadByte()); // Sometimes the next Ivo chunk has a 4 byte filler, sometimes it doesn't.
                            b.BaseStream.Position -= 1;
                            if (peek == 0)
                                SkipBytes(b, 4);
                        }

                    }
                    else if (BytesPerElement == 4)
                    {
                        for (int i = 0; i < NumElements; i++)
                        {
                            Indices[i] = b.ReadUInt32();
                        }
                    }
                    break;
                #endregion
                #region IVOVERTSUVS
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
                                Vertices[i] = b.ReadVector3(); // For some reason, skins are an extra 1 meter in the z direction.

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
                #endregion
                #region IVONORMALS
                case DatastreamType.IVONORMALS:
                case DatastreamType.IVONORMALS2:
                    switch (BytesPerElement)
                    {
                        case 4:
                            Normals = new Vector3[NumElements];
                            for (int i = 0; i < NumElements; i++)
                            {
                                var x = b.ReadSByte() / 128.0f;
                                var y = b.ReadSByte() / 128.0f;
                                var z = b.ReadSByte() / 128.0f;
                                var w = b.ReadSByte() / 128.0f;
                                Normals[i].X = 2.0f * (x * z + y * w);
                                Normals[i].Y = 2.0f * (y * z - x * w);
                                Normals[i].Z = (2.0f * (z * z + w * w)) - 1.0f;
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
                #endregion
                #region COLORS2
                case DatastreamType.IVOCOLORS2:
                    switch (BytesPerElement)
                    {
                        case 4:
                            for (int i = 0; i < NumElements; i++)
                            {
                                SkipBytes(b, 4);        // TODO:  If colors2 data is needed, capture here.
                            }
                            if (NumElements % 2 == 1)
                            {
                                SkipBytes(b, 4);
                            }
                            break;
                    }
                    break;
                #endregion
                #region IVOTANGENTS
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
                        Normals[i].X = (Tangents[i, 0].y * Tangents[i, 1].z - Tangents[i, 0].z * Tangents[i, 1].y);
                        Normals[i].Y = 0 - (Tangents[i, 0].x * Tangents[i, 1].z - Tangents[i, 0].z * Tangents[i, 1].x);
                        Normals[i].Z = (Tangents[i, 0].x * Tangents[i, 1].y - Tangents[i, 0].y * Tangents[i, 1].x);

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
                #endregion
                #region IVOBONEMAP
                case DatastreamType.IVOBONEMAP:
                    SkinningInfo skin = GetSkinningInfo();
                    skin.HasBoneMapDatastream = true;
                    skin.BoneMapping = new List<MeshBoneMapping>();

                    switch (BytesPerElement)
                    {
                        case 12:
                            for (int i = 0; i < NumElements; i++)
                            {
                                MeshBoneMapping tmpMap = new MeshBoneMapping();
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
                #endregion
                case DatastreamType.IVOUNKNOWN2:
                    break;
            }
        }
    }
}
