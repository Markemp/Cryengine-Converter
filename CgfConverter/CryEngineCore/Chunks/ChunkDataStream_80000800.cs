using Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace CgfConverter.CryEngineCore;

// Reversed endian class of x0800 for console games
internal sealed class ChunkDataStream_80000800 : ChunkDataStream
{
    public override void Read(BinaryReader b)
    {
        base.Read(b);

        Flags2 = Utils.SwapUIntEndian(b.ReadUInt32()); // another filler
        uint tmpdataStreamType = Utils.SwapUIntEndian(b.ReadUInt32());
        DataStreamType = (DatastreamType)tmpdataStreamType;
        NumElements = Utils.SwapUIntEndian(b.ReadUInt32()); // number of elements in this chunk
        BytesPerElement = Utils.SwapUIntEndian(b.ReadUInt32());

        SkipBytes(b, 8);

        // Now do loops to read for each of the different Data Stream Types.  If vertices, need to populate Vector3s for example.
        switch (DataStreamType)
        {
            #region case DataStreamTypeEnum.VERTICES:

            case DatastreamType.VERTICES:  // Ref is 0x00000000
                this.Vertices = new Vector3[this.NumElements];

                switch (BytesPerElement)
                {
                    case 12:
                        for (int i = 0; i < NumElements; i++)
                        {
                            Vertices[i].X = Utils.SwapSingleEndian(b.ReadSingle());
                            Vertices[i].Y = Utils.SwapSingleEndian(b.ReadSingle());
                            Vertices[i].Z = Utils.SwapSingleEndian(b.ReadSingle());
                        }
                        break;
                }
                break;

            #endregion
            #region case DataStreamTypeEnum.INDICES:

            case DatastreamType.INDICES:  // Ref is 
                this.Indices = new UInt32[NumElements];

                if (this.BytesPerElement == 2)
                {
                    for (Int32 i = 0; i < this.NumElements; i++)
                    {
                        this.Indices[i] = (UInt32)Utils.SwapUInt16Endian(b.ReadUInt16());
                    }
                }
                if (this.BytesPerElement == 4)
                {
                    for (Int32 i = 0; i < this.NumElements; i++)
                    {
                        this.Indices[i] = Utils.SwapUIntEndian(b.ReadUInt32());
                    }
                }
                break;

            #endregion
            #region case DataStreamTypeEnum.NORMALS:

            case DatastreamType.NORMALS:
                Normals = new Vector3[NumElements];
                for (int i = 0; i < NumElements; i++)
                {
                    Normals[i].X = Utils.SwapSingleEndian(b.ReadSingle());
                    Normals[i].Y = Utils.SwapSingleEndian(b.ReadSingle());
                    Normals[i].Z = Utils.SwapSingleEndian(b.ReadSingle());
                }
                //Utils.Log(LogLevelEnum.Debug, "Offset is {0:X}", b.BaseStream.Position);
                break;

            #endregion
            #region case DataStreamTypeEnum.UVS:

            case DatastreamType.UVS:
                this.UVs = new UV[this.NumElements];
                for (Int32 i = 0; i < this.NumElements; i++)
                {
                    this.UVs[i].U = Utils.SwapSingleEndian(b.ReadSingle());
                    this.UVs[i].V = Utils.SwapSingleEndian(b.ReadSingle());
                }
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
                            Tangents[i, 0].x = Utils.SwapIntEndian(b.ReadInt16());
                            Tangents[i, 0].y = Utils.SwapIntEndian(b.ReadInt16());
                            Tangents[i, 0].z = Utils.SwapIntEndian(b.ReadInt16());
                            Tangents[i, 0].w = Utils.SwapIntEndian(b.ReadInt16());
                                               
                            Tangents[i, 1].x = Utils.SwapIntEndian(b.ReadInt16());
                            Tangents[i, 1].y = Utils.SwapIntEndian(b.ReadInt16());
                            Tangents[i, 1].z = Utils.SwapIntEndian(b.ReadInt16());
                            Tangents[i, 1].w = Utils.SwapIntEndian(b.ReadInt16());

                            break;
                        case 0x08:
                            // These have to be divided by 127 to be used properly (value between 0 and 1)
                            // Tangent
                            this.Tangents[i, 0].w = b.ReadSByte() / 127f;
                            this.Tangents[i, 0].x = b.ReadSByte() / 127f;
                            this.Tangents[i, 0].y = b.ReadSByte() / 127f;
                            this.Tangents[i, 0].z = b.ReadSByte() / 127f;

                            // Binormal
                            this.Tangents[i, 1].w = b.ReadSByte() / 127f;
                            this.Tangents[i, 1].x = b.ReadSByte() / 127f;
                            this.Tangents[i, 1].y = b.ReadSByte() / 127f;
                            this.Tangents[i, 1].z = b.ReadSByte() / 127f;

                           break;
                        default:
                            throw new Exception("Need to add new Tangent Size");
                    }
                }
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
            #region case DataStreamTypeEnum.BONEMAP:
            case DatastreamType.BONEMAP:
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
                                tmpMap.BoneIndex[j] = Utils.SwapUInt16Endian(b.ReadUInt16());

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
            case DatastreamType.QTANGENTS:
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
