using System.IO;
using System.Numerics;
using CgfConverter.Models;
using CgfConverter.Models.Structs;
using CgfConverter.Utilities;
using Extensions;
using static Extensions.BinaryReaderExtensions;

namespace CgfConverter.CryEngineCore;

internal sealed class ChunkDataStream_801 : ChunkDataStream
{
    private short starCitizenFlag = 0;

    public override void Read(BinaryReader b)
    {
        base.Read(b);

        Flags2 = b.ReadUInt32(); // another filler
        uint datastreamType = b.ReadUInt32();
        DataStreamType = (DatastreamType)datastreamType;
        SkipBytes(b, 4);    // data stream index, for multiple streams (not used)
        
        NumElements = b.ReadUInt32(); // number of elements in this chunk

        BytesPerElement = b.ReadUInt16();
        starCitizenFlag = b.ReadInt16();

        SkipBytes(b, 8);

        switch (DataStreamType)
        {
            case DatastreamType.VERTICES:
                var vertices = new Vector3[NumElements];
                switch (BytesPerElement)
                {
                    case 12:
                        for (int i = 0; i < NumElements; i++)
                        {
                            vertices[i] = b.ReadVector3();
                        }
                        break;
                    case 8:  // Prey files, and old Star Citizen files, Evolve
                        for (int i = 0; i < NumElements; i++)
                        {
                            vertices[i] = b.ReadVector3(InputType.Half);
                            b.ReadUInt16();
                        }
                        break;
                    case 16:
                        for (int i = 0; i < NumElements; i++)
                        {
                            vertices[i] = b.ReadVector3();
                            SkipBytes(b, 4);
                        }
                        break;
                    default:
                        throw new UnsupportedDataFormatException(DataStreamType, BytesPerElement,
                            $"Unsupported bytes per element {BytesPerElement} for vertices data");
                }
                Data = new Datastream<Vector3>(DataStreamType, NumElements, BytesPerElement, vertices);
                break;

            case DatastreamType.INDICES:
                var indices = new uint[NumElements];
                switch (BytesPerElement)
                {
                    case 2:
                        for (int i = 0; i < NumElements; i++)
                        {
                            indices[i] = b.ReadUInt16();
                        }
                        break;
                    case 4:
                        for (int i = 0; i < NumElements; i++)
                        {
                            indices[i] = b.ReadUInt32();
                        }
                        break;
                    default:
                        throw new UnsupportedDataFormatException(DataStreamType, BytesPerElement,
                            $"Unsupported bytes per element {BytesPerElement} for indices data");
                }
                Data = new Datastream<uint>(DataStreamType, NumElements, BytesPerElement, indices);
                break;

            case DatastreamType.NORMALS:
                var normals = new Vector3[NumElements];
                switch (BytesPerElement)
                {
                    case 4:
                        for (int i = 0; i < NumElements; i++)
                        {
                            // TODO:   Finish this. This is wrong.
                            normals[i].X = b.ReadCryHalf();
                            normals[i].Y = b.ReadCryHalf();
                        }
                        break;
                    case 12:
                        for (int i = 0; i < NumElements; i++)
                        {
                            normals[i] = b.ReadVector3();
                        }
                        break;
                    default:
                        throw new UnsupportedDataFormatException(DataStreamType, BytesPerElement,
                            $"Unsupported bytes per element {BytesPerElement} for normal data");
                }

                Data = new Datastream<Vector3>(DataStreamType, NumElements, BytesPerElement, normals);
                break;

            case DatastreamType.UVS:
                var uvs = new UV[NumElements];
                for (int i = 0; i < NumElements; i++)
                {
                    uvs[i] = b.ReadUV();
                }
                Data = new Datastream<UV>(DataStreamType, NumElements, BytesPerElement, uvs);
                break;

            case DatastreamType.TANGENTS:
                var tangents = new Quaternion[NumElements];
                var bitangents = new Quaternion[NumElements];
                for (int i = 0; i < NumElements; i++)
                {
                    switch (BytesPerElement)
                    {
                        case 0x10:  // 16 bytes
                            tangents[i] = b.ReadQuaternion(InputType.SNorm);
                            tangents[i] = b.ReadQuaternion(InputType.SNorm); // not really using these
                            break;
                        case 0x08:
                            tangents[i] = b.ReadQuaternion(InputType.SNorm);
                            break;
                        default:
                            throw new UnsupportedDataFormatException(DataStreamType, BytesPerElement,
                                $"Unsupported bytes per element {BytesPerElement} for Tangent data");
                    }
                }
                Data = new Datastream<Quaternion>(DataStreamType, NumElements, BytesPerElement, tangents);
                break;

            case DatastreamType.COLORS:
                var colors = new IRGBA[NumElements];
                switch (BytesPerElement)
                {
                    case 3:
                        for (int i = 0; i < NumElements; i++)
                        {
                            colors[i] = b.ReadIRGBA(0xff);
                        }
                        break;

                    case 4:
                        for (int i = 0; i < NumElements; i++)
                        {
                            colors[i] = b.ReadIRGBA();
                        }
                        break;
                    default:
                        throw new UnsupportedDataFormatException(DataStreamType, BytesPerElement,
                            $"Unsupported bytes per element {BytesPerElement} for Color data");
                }
                Data = new Datastream<IRGBA>(DataStreamType, NumElements, BytesPerElement, colors);
                break;

            case DatastreamType.VERTSUVS:
                var vertsUVs = new VertUV[NumElements];
                for (int i = 0; i < NumElements; i++)
                {
                    vertsUVs[i] = b.ReadVertUV(BytesPerElement, starCitizenFlag == 257);
                }
                Data = new Datastream<VertUV>(DataStreamType, NumElements, BytesPerElement, vertsUVs);
                break;

            case DatastreamType.BONEMAP:
                var bonemap = new MeshBoneMapping[NumElements];
                for (int i = 0; i < NumElements; i++)
                {
                    bonemap[i] = b.ReadBoneMap(BytesPerElement);
                }
                Data = new Datastream<MeshBoneMapping>(DataStreamType, NumElements, BytesPerElement, bonemap);
                break;

            case DatastreamType.QTANGENTS:
                var qtans = new Quaternion[NumElements];
                for (int i = 0; i < NumElements; i++)
                {
                    qtans[i] = b.ReadQuaternion(InputType.SNorm);
                }
                Data = new Datastream<Quaternion>(DataStreamType, NumElements, BytesPerElement, qtans);
                break;

            default:
                HelperMethods.Log(LogLevelEnum.Debug, "***** Unknown DataStream Type *****");
                break;
        }
    }
}
