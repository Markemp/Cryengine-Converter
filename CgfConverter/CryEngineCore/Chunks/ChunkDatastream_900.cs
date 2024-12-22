using CgfConverter.Models;
using CgfConverter.Utilities;
using Extensions;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using static Extensions.BinaryReaderExtensions;

namespace CgfConverter.CryEngineCore;

internal sealed class ChunkDataStream_900 : ChunkDataStream
{
    public ChunkDataStream_900(uint numberOfElements)
    {
        NumElements = numberOfElements;
    }

    public override void Read(BinaryReader b)
    {
        base.Read(b);
        // Datastreams are aligned on 8 byte boundaries.  After reading a datastream,
        // set the position to the next 8 byte boundary.

        uint dataStreamType = b.ReadUInt32();
        var ivoDataStreamType = (IvoDatastreamType)dataStreamType;
        
        BytesPerElement = b.ReadUInt32();

        switch (ivoDataStreamType)
        {
            #region Indices
            case IvoDatastreamType.IVOINDICES:
                Indices = new uint[NumElements];
                if (BytesPerElement == 2)
                {
                    for (int i = 0; i < NumElements; i++)
                    {
                        Indices[i] = b.ReadUInt16();
                    }
                }
                else if (BytesPerElement == 4)
                {
                    for (int i = 0; i < NumElements; i++)
                    {
                        Indices[i] = b.ReadUInt32();
                    }
                }
                b.AlignTo(8);
                break;
            #endregion
            #region VertsUVs
            case IvoDatastreamType.IVOVERTSUVS:
            case IvoDatastreamType.IVOVERTSUVS2:
                Vertices = new Vector3[NumElements];
                Colors = new IRGBA[NumElements];
                UVs = new UV[NumElements];
                switch (BytesPerElement)
                {
                    case 16:
                        for (int i = 0; i < NumElements; i++)
                        {
                            Vertices[i] = b.ReadVector3(InputType.CryHalf);
                            Colors[i] = b.ReadColor();
                            UVs[i].U = (float)b.ReadHalf();
                            UVs[i].V = (float)b.ReadHalf();
                        }
                        break;
                    case 20:
                        for (int i = 0; i < NumElements; i++)
                        {
                            Vertices[i] = b.ReadVector3(); // For some reason, skins are an extra 1 meter in the z direction.
                            Colors[i] = b.ReadColor();
                            UVs[i].U = (float)b.ReadHalf();
                            UVs[i].V = (float)b.ReadHalf();
                        }
                        
                        break;
                }
                b.AlignTo(8);
                break;
            #endregion
            #region Normals
            case IvoDatastreamType.IVONORMALS:
            case IvoDatastreamType.IVONORMALS2:
                Normals = new Vector3[NumElements];
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
                        HelperMethods.Log("Unknown Normals Format");
                        for (int i = 0; i < NumElements; i++)
                        {
                            SkipBytes(b, BytesPerElement);
                        }
                        break;
                }
                b.AlignTo(8);
                break;
            #endregion
            #region Colors
            case IvoDatastreamType.IVOCOLORS2:
                Colors2 = new IRGBA[NumElements];
                for (int i = 0; i < NumElements; i++)
                {

                }
                b.AlignTo(8);
                break;
            #endregion
            #region Tangents
            case IvoDatastreamType.IVOTANGENTS:
            case IvoDatastreamType.IVOTANGENTS2:
                Tangents = new Tangent[NumElements, 2];
                //Normals = new Vector3[NumElements];
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
                    //Normals[i].X = (Tangents[i, 0].y * Tangents[i, 1].z - Tangents[i, 0].z * Tangents[i, 1].y);
                    //Normals[i].Y = 0 - (Tangents[i, 0].x * Tangents[i, 1].z - Tangents[i, 0].z * Tangents[i, 1].x);
                    //Normals[i].Z = (Tangents[i, 0].x * Tangents[i, 1].y - Tangents[i, 0].y * Tangents[i, 1].x);

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
                b.AlignTo(8);
                break;
            #endregion
            #region BoneMap
            case IvoDatastreamType.IVOBONEMAP32:
            case IvoDatastreamType.IVOBONEMAP:
                SkinningInfo skin = GetSkinningInfo();
                skin.BoneMapping = new List<MeshBoneMapping>();

                switch (BytesPerElement)
                {
                    case 12:
                        for (int i = 0; i < NumElements; i++)
                        {
                            MeshBoneMapping tmpMap = new();
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
                    case 24:
                        for (int i = 0; i < NumElements; i++)
                        {
                            MeshBoneMapping tmpMap = new();
                            tmpMap.BoneIndex = new int[4];
                            tmpMap.Weight = new int[4];

                            for (int j = 0; j < 4; j++)         // read the 4 bone indexes first
                            {
                                tmpMap.BoneIndex[j] = (int)b.ReadInt32();
                            }
                            for (int j = 0; j < 4; j++)           // read the weights.
                            {
                                tmpMap.Weight[j] = b.ReadUInt16();
                            }
                            skin.BoneMapping.Add(tmpMap);
                        }
                        break;
                    default:
                        HelperMethods.Log("Unknown BoneMapping structure");
                        break;
                }
                b.AlignTo(8);
                break;
            #endregion
        }
    }
}
