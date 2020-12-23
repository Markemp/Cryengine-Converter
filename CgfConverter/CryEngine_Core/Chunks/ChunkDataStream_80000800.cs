using System;
using System.Collections.Generic;
using System.IO;

namespace CgfConverter.CryEngineCore
{
    public class ChunkDataStream_80000800 : ChunkDataStream
    {
        // Reversed endian class of x0800 for console games

        public override void Read(BinaryReader b)
        {
            base.Read(b);

            this.Flags2 = Utils.SwapUIntEndian(b.ReadUInt32()); // another filler
            uint tmpdataStreamType = Utils.SwapUIntEndian(b.ReadUInt32());
            this.DataStreamType = (DataStreamTypeEnum)Enum.ToObject(typeof(DataStreamTypeEnum), tmpdataStreamType);
            this.NumElements = Utils.SwapUIntEndian(b.ReadUInt32()); // number of elements in this chunk
            this.BytesPerElement = Utils.SwapUIntEndian(b.ReadUInt32());

            this.SkipBytes(b, 8);

            // Now do loops to read for each of the different Data Stream Types.  If vertices, need to populate Vector3s for example.
            switch (this.DataStreamType)
            {
                #region case DataStreamTypeEnum.VERTICES:

                case DataStreamTypeEnum.VERTICES:  // Ref is 0x00000000
                    this.Vertices = new Vector3[this.NumElements];

                    switch (BytesPerElement)
                    {
                        case 12:
                            for (Int32 i = 0; i < this.NumElements; i++)
                            {
                                this.Vertices[i].x = b.ReadSingle();
                                this.Vertices[i].y = b.ReadSingle();
                                this.Vertices[i].z = b.ReadSingle();
                            }
                            break;
                        case 8:  // Prey files, and old Star Citizen files
                            for (Int32 i = 0; i < NumElements; i++)
                            {
                                Half xshort = new Half();
                                xshort.bits = b.ReadUInt16();
                                this.Vertices[i].x = xshort.ToSingle();

                                Half yshort = new Half();
                                yshort.bits = b.ReadUInt16();
                                this.Vertices[i].y = yshort.ToSingle();

                                Half zshort = new Half();
                                zshort.bits = b.ReadUInt16();
                                this.Vertices[i].z = zshort.ToSingle();
                                b.ReadUInt16();
                            }
                            break;
                        case 16:
                            for (Int32 i = 0; i < this.NumElements; i++)
                            {
                                this.Vertices[i].x = b.ReadSingle();
                                this.Vertices[i].y = b.ReadSingle();
                                this.Vertices[i].z = b.ReadSingle();
                                this.Vertices[i].w = b.ReadSingle(); // TODO:  Sometimes there's a W to these structures.  Will investigate.
                            }
                            break;
                    }
                    break;

                #endregion
                #region case DataStreamTypeEnum.INDICES:

                case DataStreamTypeEnum.INDICES:  // Ref is 
                    this.Indices = new UInt32[NumElements];

                    if (this.BytesPerElement == 2)
                    {
                        for (Int32 i = 0; i < this.NumElements; i++)
                        {
                            this.Indices[i] = (UInt32)b.ReadUInt16();
                            //Console.WriteLine("Indices {0}: {1}", i, this.Indices[i]);
                        }
                    }
                    if (this.BytesPerElement == 4)
                    {
                        for (Int32 i = 0; i < this.NumElements; i++)
                        {
                            this.Indices[i] = b.ReadUInt32();
                        }
                    }
                    //Utils.Log(LogLevelEnum.Debug, "Offset is {0:X}", b.BaseStream.Position);
                    break;

                #endregion
                #region case DataStreamTypeEnum.NORMALS:

                case DataStreamTypeEnum.NORMALS:
                    this.Normals = new Vector3[this.NumElements];
                    for (Int32 i = 0; i < NumElements; i++)
                    {
                        this.Normals[i].x = b.ReadSingle();
                        this.Normals[i].y = b.ReadSingle();
                        this.Normals[i].z = b.ReadSingle();
                    }
                    //Utils.Log(LogLevelEnum.Debug, "Offset is {0:X}", b.BaseStream.Position);
                    break;

                #endregion
                #region case DataStreamTypeEnum.UVS:

                case DataStreamTypeEnum.UVS:
                    this.UVs = new UV[this.NumElements];
                    for (Int32 i = 0; i < this.NumElements; i++)
                    {
                        this.UVs[i].U = b.ReadSingle();
                        this.UVs[i].V = b.ReadSingle();
                    }
                    //Utils.Log(LogLevelEnum.Debug, "Offset is {0:X}", b.BaseStream.Position);
                    break;

                #endregion
                #region case DataStreamTypeEnum.TANGENTS:

                case DataStreamTypeEnum.TANGENTS:
                    this.Tangents = new Tangent[this.NumElements, 2];
                    this.Normals = new Vector3[this.NumElements];
                    for (Int32 i = 0; i < this.NumElements; i++)
                    {
                        switch (this.BytesPerElement)
                        {
                            case 0x10:
                                // These have to be divided by 32767 to be used properly (value between 0 and 1)
                                this.Tangents[i, 0].x = b.ReadInt16();
                                this.Tangents[i, 0].y = b.ReadInt16();
                                this.Tangents[i, 0].z = b.ReadInt16();
                                this.Tangents[i, 0].w = b.ReadInt16();

                                this.Tangents[i, 1].x = b.ReadInt16();
                                this.Tangents[i, 1].y = b.ReadInt16();
                                this.Tangents[i, 1].z = b.ReadInt16();
                                this.Tangents[i, 1].w = b.ReadInt16();

                                break;
                            case 0x08:
                                // These have to be divided by 127 to be used properly (value between 0 and 1)
                                // Tangent
                                this.Tangents[i, 0].w = b.ReadSByte() / 127.0;
                                this.Tangents[i, 0].x = b.ReadSByte() / 127.0;
                                this.Tangents[i, 0].y = b.ReadSByte() / 127.0;
                                this.Tangents[i, 0].z = b.ReadSByte() / 127.0;

                                // Binormal
                                this.Tangents[i, 1].w = b.ReadSByte() / 127.0;
                                this.Tangents[i, 1].x = b.ReadSByte() / 127.0;
                                this.Tangents[i, 1].y = b.ReadSByte() / 127.0;
                                this.Tangents[i, 1].z = b.ReadSByte() / 127.0;

                                // Calculate the normal based on the cross product of the tangents.
                                //this.Normals[i].x = (Tangents[i,0].y * Tangents[i,1].z - Tangents[i,0].z * Tangents[i,1].y);
                                //this.Normals[i].y = 0 - (Tangents[i,0].x * Tangents[i,1].z - Tangents[i,0].z * Tangents[i,1].x); 
                                //this.Normals[i].z = (Tangents[i,0].x * Tangents[i,1].y - Tangents[i,0].y * Tangents[i,1].x);
                                //Console.WriteLine("Tangent: {0:F6} {1:F6} {2:F6}", Tangents[i,0].x, Tangents[i, 0].y, Tangents[i, 0].z);
                                //Console.WriteLine("Binormal: {0:F6} {1:F6} {2:F6}", Tangents[i, 1].x, Tangents[i, 1].y, Tangents[i, 1].z);
                                //Console.WriteLine("Normal: {0:F6} {1:F6} {2:F6}", Normals[i].x, Normals[i].y, Normals[i].z);
                                break;
                            default:
                                throw new Exception("Need to add new Tangent Size");
                        }
                    }
                    // Utils.Log(LogLevelEnum.Debug, "Offset is {0:X}", b.BaseStream.Position);
                    break;
                #endregion

                #region case DataStreamTypeEnum.COLORS:
                case DataStreamTypeEnum.COLORS:
                    switch (this.BytesPerElement)
                    {
                        case 3:
                            this.RGBColors = new IRGB[this.NumElements];
                            for (Int32 i = 0; i < NumElements; i++)
                            {
                                this.RGBColors[i].r = b.ReadByte();
                                this.RGBColors[i].g = b.ReadByte();
                                this.RGBColors[i].b = b.ReadByte();
                            }
                            break;

                        case 4:
                            this.RGBAColors = new IRGBA[this.NumElements];
                            for (Int32 i = 0; i < this.NumElements; i++)
                            {
                                this.RGBAColors[i].r = b.ReadByte();
                                this.RGBAColors[i].g = b.ReadByte();
                                this.RGBAColors[i].b = b.ReadByte();
                                this.RGBAColors[i].a = b.ReadByte();
                            }
                            break;
                        default:
                            Utils.Log("Unknown Color Depth");
                            for (Int32 i = 0; i < this.NumElements; i++)
                            {
                                this.SkipBytes(b, this.BytesPerElement);
                            }
                            break;
                    }
                    break;
                #endregion

                #region case DataStreamTypeEnum.BONEMAP:
                case DataStreamTypeEnum.BONEMAP:
                    SkinningInfo skin = GetSkinningInfo();
                    skin.HasBoneMapDatastream = true;

                    skin.BoneMapping = new List<MeshBoneMapping>();

                    // Bones should have 4 bone IDs (index) and 4 weights.
                    for (int i = 0; i < NumElements; i++)
                    {
                        MeshBoneMapping tmpMap = new MeshBoneMapping();
                        switch (this.BytesPerElement)
                        {
                            case 8:
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
                                break;
                            case 12:
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

                                break;
                            default:
                                Utils.Log("Unknown BoneMapping structure");
                                break;

                        }
                    }

                    break;


                #endregion
                #region DataStreamTypeEnum.Unknown1
                case DataStreamTypeEnum.UNKNOWN1:
                    this.Tangents = new Tangent[this.NumElements, 2];
                    this.Normals = new Vector3[this.NumElements];
                    for (Int32 i = 0; i < NumElements; i++)
                    {
                        this.Tangents[i, 0].w = b.ReadSByte() / 127.0;
                        this.Tangents[i, 0].x = b.ReadSByte() / 127.0;
                        this.Tangents[i, 0].y = b.ReadSByte() / 127.0;
                        this.Tangents[i, 0].z = b.ReadSByte() / 127.0;

                        // Binormal
                        this.Tangents[i, 1].w = b.ReadSByte() / 127.0;
                        this.Tangents[i, 1].x = b.ReadSByte() / 127.0;
                        this.Tangents[i, 1].y = b.ReadSByte() / 127.0;
                        this.Tangents[i, 1].z = b.ReadSByte() / 127.0;

                        // Calculate the normal based on the cross product of the tangents.
                        this.Normals[i].x = (Tangents[i, 0].y * Tangents[i, 1].z - Tangents[i, 0].z * Tangents[i, 1].y);
                        this.Normals[i].y = 0 - (Tangents[i, 0].x * Tangents[i, 1].z - Tangents[i, 0].z * Tangents[i, 1].x);
                        this.Normals[i].z = (Tangents[i, 0].x * Tangents[i, 1].y - Tangents[i, 0].y * Tangents[i, 1].x);
                    }
                    break;
                #endregion // Prey normals?
                #region default:

                default:
                    Utils.Log(LogLevelEnum.Debug, "***** Unknown DataStream Type *****");
                    break;

                    #endregion
            }
        }
    }
}
