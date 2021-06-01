using System;
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
            }
        }
    }
}
