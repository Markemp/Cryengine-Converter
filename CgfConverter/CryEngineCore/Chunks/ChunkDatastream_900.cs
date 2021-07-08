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
                        {
                            SkipBytes(b, 2);
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
                case DatastreamType.IVOVERTSUVS:
                    Vertices = new Vector3[NumElements];
                    Normals = new Vector3[NumElements];
                    RGBColors = new IRGB[NumElements];
                    UVs = new UV[NumElements];
                    switch (BytesPerElement)  // new Star Citizen files
                    {
                        case 20:
                            for (int i = 0; i < NumElements; i++)
                            {
                                Vertices[i].x = b.ReadSingle();
                                Vertices[i].y = b.ReadSingle();
                                Vertices[i].z = b.ReadSingle(); // For some reason, skins are an extra 1 meter in the z direction.

                                // Normals are stored in a signed byte, prob div by 127.
                                Normals[i].x = (float)b.ReadSByte() / 127;
                                Normals[i].y = (float)b.ReadSByte() / 127;
                                Normals[i].z = (float)b.ReadSByte() / 127;
                                b.ReadSByte(); // Should be FF.

                                Half uvu = new Half();
                                uvu.bits = b.ReadUInt16();
                                UVs[i].U = uvu.ToSingle();

                                Half uvv = new Half();
                                uvv.bits = b.ReadUInt16();
                                UVs[i].V = uvv.ToSingle();
                            }
                            break;
                            
                    }
                    break;
                case DatastreamType.IVOCOLORS:
                    switch (BytesPerElement)
                    {
                        case 3:
                            RGBColors = new IRGB[NumElements];
                            for (int i = 0; i < NumElements; i++)
                            {
                                RGBColors[i].r = b.ReadByte();
                                RGBColors[i].g = b.ReadByte();
                                RGBColors[i].b = b.ReadByte();
                            }
                            break;
                        case 4:
                            RGBAColors = new IRGBA[NumElements];
                            for (int i = 0; i < NumElements; i++)
                            {
                                RGBAColors[i].r = b.ReadByte();
                                RGBAColors[i].g = b.ReadByte();
                                RGBAColors[i].b = b.ReadByte();
                                RGBAColors[i].a = b.ReadByte();
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
                case DatastreamType.IVOTANGENTS:
                    Tangents = new Tangent[NumElements, 2];
                    for (int i = 0; i < NumElements; i++)
                    {
                        switch (BytesPerElement)
                        {
                            case 0x10:
                                // These have to be divided by 32767 to be used properly (value between 0 and 1)
                                Tangents[i, 0].x = b.ReadInt16();
                                Tangents[i, 0].y = b.ReadInt16();
                                Tangents[i, 0].z = b.ReadInt16();
                                Tangents[i, 0].w = b.ReadInt16();

                                Tangents[i, 1].x = b.ReadInt16();
                                Tangents[i, 1].y = b.ReadInt16();
                                Tangents[i, 1].z = b.ReadInt16();
                                Tangents[i, 1].w = b.ReadInt16();

                                break;
                            case 0x08:
                                // These have to be divided by 127 to be used properly (value between 0 and 1)
                                // Tangent
                                Tangents[i, 0].w = b.ReadSByte() / 127.0;
                                Tangents[i, 0].x = b.ReadSByte() / 127.0;
                                Tangents[i, 0].y = b.ReadSByte() / 127.0;
                                Tangents[i, 0].z = b.ReadSByte() / 127.0;

                                // Binormal
                                Tangents[i, 1].w = b.ReadSByte() / 127.0;
                                Tangents[i, 1].x = b.ReadSByte() / 127.0;
                                Tangents[i, 1].y = b.ReadSByte() / 127.0;
                                Tangents[i, 1].z = b.ReadSByte() / 127.0;

                                break;
                            default:
                                throw new Exception("Need to add new Tangent Size");
                        }
                    }
                    break;
                case DatastreamType.IVOBONEMAP:   
                    SkinningInfo skin = GetSkinningInfo();
                    skin.HasBoneMapDatastream = true;

                    skin.BoneMapping = new List<MeshBoneMapping>();
                    MeshBoneMapping tmpMap = new MeshBoneMapping();
                    switch (BytesPerElement)
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
