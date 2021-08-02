using Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using static Extensions.BinaryReaderExtensions;

namespace CgfConverter.CryEngineCore
{
    public class ChunkDataStream_800 : ChunkDataStream
    {
        private short starCitizenFlag = 0;

        public override void Read(BinaryReader b)
        {
            base.Read(b);

            Flags2 = b.ReadUInt32(); // another filler
            uint dataStreamType = b.ReadUInt32();
            DataStreamType = (DatastreamType)Enum.ToObject(typeof(DatastreamType), dataStreamType);
            NumElements = b.ReadUInt32(); // number of elements in this chunk

            if (_model.FileVersion == FileVersion.CryTek_3_5 || _model.FileVersion == FileVersion.CryTek_3_4)
            {
                BytesPerElement = b.ReadUInt32();
            }
            if (_model.FileVersion == FileVersion.CryTek_3_6)
            {
                BytesPerElement = (uint)b.ReadInt16();  // Star Citizen 2.0 is using an int16 here now.
                starCitizenFlag = b.ReadInt16();        // For Star Citizen files, this is 257.  Other known games (Hunt, Evolve, Prey) are 0.
            }

            SkipBytes(b, 8);

            // Now do loops to read for each of the different Data Stream Types.  If vertices, need to populate Vector3s for example.
            switch (DataStreamType)
            {
                #region case DataStreamTypeEnum.VERTICES:

                case DatastreamType.VERTICES:  // Ref is 0x00000000
                    Vertices = new Vector3[NumElements];

                    switch (BytesPerElement)
                    {
                        case 12:
                            for (int i = 0; i < NumElements; i++)
                            {
                                Vertices[i].X = b.ReadSingle();
                                Vertices[i].Y = b.ReadSingle();
                                Vertices[i].Z = b.ReadSingle();
                            }
                            break;
                        case 8:  // Prey files, and old Star Citizen files
                            for (int i = 0; i < NumElements; i++)
                            {
                                Vertices[i] = b.ReadVector3(InputType.Half);
                                b.ReadUInt16();
                            }
                            break;
                        case 16:
                            for (int i = 0; i < NumElements; i++)
                            {
                                Vertices[i] = b.ReadVector3();
                                SkipBytes(b, 4); // TODO:  Sometimes there's a W to these structures.  Will investigate.
                            }
                            break;
                    }
                    break;

                #endregion
                #region case DataStreamTypeEnum.INDICES:

                case DatastreamType.INDICES:  // Ref is 
                    Indices = new uint[NumElements];

                    if (BytesPerElement == 2)
                    {
                        for (int i = 0; i < NumElements; i++)
                        {
                            Indices[i] = b.ReadUInt16();
                        }
                    }
                    if (BytesPerElement == 4)
                    {
                        for (int i = 0; i < NumElements; i++)
                        {
                            Indices[i] = b.ReadUInt32();
                        }
                    }
                    break;

                #endregion
                #region case DataStreamTypeEnum.NORMALS:

                case DatastreamType.NORMALS:
                    Normals = new Vector3[NumElements];
                    switch (BytesPerElement)
                    {
                        case 12:
                            for (int i = 0; i < NumElements; i++)
                            {
                                Normals[i] = b.ReadVector3();
                            }
                            break;
                        case 4:
                            for (int i = 0; i < NumElements; i++)
                            {
                                Normals[i].X = (float)((b.ReadByte() - 128.0) / 127.5f);
                                Normals[i].Y = (float)((b.ReadByte() - 128.0) / 127.5f);
                                Normals[i].Z = (float)((b.ReadByte() - 128.0) / 127.5f);
                                b.ReadByte();
                            }
                            break;
                        default:
                            break;
                    }
                    break;

                    
                    

                #endregion
                #region case DataStreamTypeEnum.UVS:

                case DatastreamType.UVS:
                    UVs = new UV[NumElements];
                    for (int i = 0; i < NumElements; i++)
                    {
                        UVs[i].U = b.ReadSingle();
                        UVs[i].V = b.ReadSingle();
                    }
                    //Utils.Log(LogLevelEnum.Debug, "Offset is {0:X}", b.BaseStream.Position);
                    break;

                #endregion
                #region case DataStreamTypeEnum.TANGENTS:

                case DatastreamType.TANGENTS:
                    Tangents = new Tangent[NumElements, 2];
                    Normals = new Vector3[NumElements];
                    for (int i = 0; i < NumElements; i++)
                    {
                        switch (BytesPerElement)
                        {
                            case 0x10:
                                // These have to be divided by 32767 to be used properly (value between 0 and 1)
                                Tangents[i, 0].x = b.ReadInt16() / 32767.0f;
                                Tangents[i, 0].y = b.ReadInt16() / 32767.0f;
                                Tangents[i, 0].z = b.ReadInt16() / 32767.0f;
                                Tangents[i, 0].w = b.ReadInt16() / 32767.0f;

                                Tangents[i, 1].x = b.ReadInt16() / 32767.0f;
                                Tangents[i, 1].y = b.ReadInt16() / 32767.0f;
                                Tangents[i, 1].z = b.ReadInt16() / 32767.0f;
                                Tangents[i, 1].w = b.ReadInt16() / 32767.0f;

                                break;
                            case 0x08:
                                // These have to be divided by 127 to be used properly (value between 0 and 1)
                                // Tangent
                                Tangents[i, 0].w = b.ReadSByte() / 127.5f;
                                Tangents[i, 0].x = b.ReadSByte() / 127.5f;
                                Tangents[i, 0].y = b.ReadSByte() / 127.5f;
                                Tangents[i, 0].z = b.ReadSByte() / 127.5f;

                                // Bitangent
                                Tangents[i, 1].w = b.ReadSByte() / 127.5f;
                                Tangents[i, 1].x = b.ReadSByte() / 127.5f;
                                Tangents[i, 1].y = b.ReadSByte() / 127.5f;
                                Tangents[i, 1].z = b.ReadSByte() / 127.5f;

                                // Calculate the normal based on the cross product of the tangents.
                                Vector3 tan = new Vector3(Tangents[i, 0].x, Tangents[i, 0].y, Tangents[i, 0].z);
                                Vector3 bitan = new Vector3(Tangents[i, 1].x, Tangents[i, 1].y, Tangents[i, 1].z);
                                var weight = Tangents[i, 0].z > 0 ? 1 : -1;
                                Normals[i] = Vector3.Cross(tan, bitan) * weight;
                                break;
                            default:
                                throw new Exception("Need to add new Tangent Size");
                        }
                    }
                    // Utils.Log(LogLevelEnum.Debug, "Offset is {0:X}", b.BaseStream.Position);
                    break;

                #endregion
                #region case DataStreamTypeEnum.COLORS:

                case DatastreamType.COLORS:
                    switch (BytesPerElement)
                    {
                        case 3:
                            Colors = new IRGBA[NumElements];
                            for (int i = 0; i < NumElements; i++)
                            {
                                Colors[i].r = b.ReadByte();
                                Colors[i].g = b.ReadByte();
                                Colors[i].b = b.ReadByte();
                                Colors[i].a = 255;
                            }
                            break;

                        case 4:
                            Colors = new IRGBA[NumElements];
                            for (int i = 0; i < NumElements; i++)
                            {
                                Colors[i] = b.ReadColor();
                            }
                            break;
                        default:
                            Utils.Log("Unknown Color Depth");
                            for (int i = 0; i < NumElements; i++)
                            {
                                SkipBytes(b, BytesPerElement);
                            }
                            break;
                    }
                    break;

                #endregion
                #region case DataStreamTypeEnum.VERTSUVS:

                case DatastreamType.VERTSUVS:
                    Vertices = new Vector3[NumElements];
                    Colors = new IRGBA[NumElements];
                    UVs = new UV[NumElements];
                    switch (BytesPerElement)  // new Star Citizen files
                    {
                        case 20:  // 3 floats for vertex position, 4 bytes for colors, 2 halfs for UVs.  Normals are calculated from Tangents
                            Normals = new Vector3[NumElements];
                            for (int i = 0; i < NumElements; i++)
                            {
                                Vertices[i] = b.ReadVector3(); // For some reason, skins are an extra 1 meter in the z direction.

                                Colors[i] = b.ReadColor();

                                UVs[i].U = b.ReadHalf();
                                UVs[i].V = b.ReadHalf();
                            }
                            break;
                        case 16:
                            for (int i = 0; i < NumElements; i++)
                            {
                                if (starCitizenFlag == 257)
                                {
                                    Vertices[i] = b.ReadVector3(InputType.CryHalf);
                                    SkipBytes(b, 2);
                                }
                                else
                                {
                                    Vertices[i] = b.ReadVector3(InputType.Half);
                                    SkipBytes(b, 2);
                                }

                                Colors[i] = b.ReadColor();

                                UVs[i].U = b.ReadHalf();
                                UVs[i].V = b.ReadHalf();
                            }
                            break;
                        default:
                            Utils.Log("Unknown VertUV structure");
                            for (int i = 0; i < NumElements; i++)
                            {
                                SkipBytes(b, BytesPerElement);
                            }
                            break;
                    }
                    break;
                #endregion
                #region case DataStreamTypeEnum.BONEMAP:
                case DatastreamType.BONEMAP:
                    SkinningInfo skin = GetSkinningInfo();
                    skin.HasBoneMapDatastream = true;
                    skin.BoneMapping = new List<MeshBoneMapping>();

                    switch (BytesPerElement)
                    {
                        case 8:
                            for (int i = 0; i < NumElements; i++)
                            {
                                MeshBoneMapping tmpMap = new MeshBoneMapping();
                                tmpMap.BoneIndex = new int[4];
                                tmpMap.Weight = new int[4];

                                for (int j = 0; j < 4; j++)         // read the 4 bone indexes first
                                {
                                    tmpMap.BoneIndex[j] = b.ReadByte();

                                }
                                for (int j = 0; j < 4; j++)           // read the weights.
                                {
                                    tmpMap.Weight[j] = b.ReadByte();
                                }
                                skin.BoneMapping.Add(tmpMap);
                            }
                            break;
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
                            break;

                        default:
                            Utils.Log("Unknown BoneMapping structure");
                            break;
                    }

                    break;

                #endregion
                #region case DataStreamTypeEnum.QTangents
                case DatastreamType.QTANGENTS:
                    QTangents = new Quaternion[NumElements];
                    Normals = new Vector3[NumElements];
                    for (int i = 0; i < NumElements; i++)
                    {
                        QTangents[i] = b.ReadQuaternion(InputType.Int16);
                        Normals[i] = QTangents[i].GetNormal();
                    }
                    break;
                #endregion
                #region default:

                default:
                    Utils.Log(LogLevelEnum.Debug, "***** Unknown DataStream Type *****");
                    break;

                    #endregion
            }
        }
    }
}
