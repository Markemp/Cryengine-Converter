using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using CgfConverter.Models;
using CgfConverter.Utilities;
using Extensions;
using static Extensions.BinaryReaderExtensions;

namespace CgfConverter.CryEngineCore;

internal sealed class ChunkDataStream_801 : ChunkDataStream
{
    public override void Read(BinaryReader b)
    {
        base.Read(b);

        Flags2 = b.ReadUInt32(); // another filler
        uint datastreamType = b.ReadUInt32();
        DataStreamType = (DatastreamType)datastreamType;
        SkipBytes(b, 4);    // data stream index, for multiple streams (not used)
        
        NumElements = b.ReadUInt32(); // number of elements in this chunk
        BytesPerElement = b.ReadUInt32();

        SkipBytes(b, 8);

        switch (DataStreamType)
        {
            case DatastreamType.VERTICES:
                Vertices = new Vector3[NumElements];
                if (BytesPerElement == 12)
                {
                    for (int i = 0; i < NumElements; i++)
                    {
                        Vertices[i] = b.ReadVector3();
                    }
                }
                else
                if (BytesPerElement == 8)
                {
                    for (int i = 0; i < NumElements; i++)
                    {
                        Vertices[i] = b.ReadVector3(InputType.Half);
                        b.ReadUInt16();
                    }
                }

                break;

            case DatastreamType.INDICES:
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

            case DatastreamType.NORMALS:
                Normals = new Vector3[NumElements];
                for (int i = 0; i < NumElements; i++)
                {
                    Normals[i] = b.ReadVector3();
                }
                break;

            case DatastreamType.UVS:
                UVs = new UV[NumElements];
                for (int i = 0; i < NumElements; i++)
                {
                    UVs[i] = b.ReadUV();
                }
                break;

            case DatastreamType.TANGENTS:
                for (int i = 0; i < NumElements; i++)
                {
                    switch (BytesPerElement)
                    {
                        case 0x10:
                            Tangents.Add(b.ReadQuaternion(InputType.SNorm));
                            BiTangents.Add(b.ReadQuaternion(InputType.SNorm));
                            break;
                        case 0x08:
                            QTangents.Add(b.ReadQuaternion(InputType.SNorm));
                            break;
                        default:
                            throw new NotSupportedException($"Unsupported tangents format: {BytesPerElement}"); throw new Exception("Need to add new Tangent Size");
                    }
                }
                break;

            case DatastreamType.COLORS:
                switch (BytesPerElement)
                {
                    case 3:
                        Colors = new IRGBA[NumElements];
                        for (int i = 0; i < NumElements; i++)
                        {
                            Colors[i] = b.ReadIRGBA(0xff);
                        }
                        break;

                    case 4:
                        Colors = new IRGBA[NumElements];
                        for (int i = 0; i < NumElements; i++)
                        {
                            Colors[i] = b.ReadIRGBA();
                        }
                        break;
                    default:
                        HelperMethods.Log("Unknown Color Depth");
                        for (int i = 0; i < NumElements; i++)
                        {
                            SkipBytes(b, BytesPerElement);
                        }
                        break;
                }
                break;

            #region case DataStreamTypeEnum.VERTSUVS:

            case DatastreamType.VERTSUVS:  // 3 half floats for verts, 3 half floats for normals, 2 half floats for UVs
                // Utils.Log(LogLevelEnum.Debug, "In VertsUVs...");
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
                            Colors[i] = b.ReadIRGBA();
                            UVs[i] = b.ReadUV(InputType.Half);
                        }
                        break;
                    case 16:
                        for (int i = 0; i < NumElements; i++)
                        {
                            Vertices[i] = b.ReadVector3(InputType.CryHalf);
                            SkipBytes(b, 2);
                            Colors[i] = b.ReadIRGBA();
                            UVs[i] = b.ReadUV(InputType.Half);
                        }
                        break;
                    default:
                        HelperMethods.Log("Unknown VertUV structure");
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
                skin.BoneMappings = [];

                // Bones should have 4 bone IDs (index) and 4 weights.
                for (int i = 0; i < NumElements; i++)
                {
                    MeshBoneMapping tmpMap = new() { BoneIndex = new int[4], Weight = new float[4] };
                    switch (BytesPerElement)
                    {
                        case 8:     // legacy support
                            for (int j = 0; j < 4; j++)         // read the 4 bone indexes first
                            {
                                tmpMap.BoneIndex[j] = b.ReadByte();
                            }
                            for (int j = 0; j < 4; j++)           // read the weights.
                            {
                                tmpMap.Weight[j] = b.ReadByte() / 255.0f;
                            }
                            skin.BoneMappings.Add(tmpMap);
                            break;
                        case 12:
                            for (int j = 0; j < 4; j++)
                            {
                                tmpMap.BoneIndex[j] = b.ReadUInt16();

                            }
                            for (int j = 0; j < 4; j++) 
                            {
                                tmpMap.Weight[j] = b.ReadByte() / 255.0f;
                            }
                            skin.BoneMappings.Add(tmpMap);

                            break;
                        default:
                            HelperMethods.Log("Unknown BoneMapping structure");
                            break;
                    }
                }
                break;

            #endregion
            #region DataStreamTypeEnum.QTANGENTS
            case DatastreamType.QTANGENTS:
                for (int i = 0; i < NumElements; i++)
                {
                    QTangents.Add(b.ReadQuaternion(InputType.SNorm));
                }
                break;
            #endregion

            default:
                HelperMethods.Log(LogLevelEnum.Debug, "***** Unknown DataStream Type *****");
                break;
        }
    }
}
