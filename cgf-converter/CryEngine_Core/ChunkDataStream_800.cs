using OpenTK.Math;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CgfConverter.CryEngine_Core
{
    public class ChunkDataStream_800 : ChunkDataStream
    {
        public override void Read(BinaryReader b)
        {
            base.Read(b);

            this.Flags2 = b.ReadUInt32(); // another filler
            UInt32 tmpdataStreamType = b.ReadUInt32();
            this.DataStreamType = (DataStreamTypeEnum)Enum.ToObject(typeof(DataStreamTypeEnum), tmpdataStreamType);
            this.NumElements = b.ReadUInt32(); // number of elements in this chunk

            if (CryEngine.Model.FILE_VERSION == FileVersionEnum.CryTek_3_5 || CryEngine.Model.FILE_VERSION == FileVersionEnum.CryTek_3_4)
            {
                this.BytesPerElement = b.ReadUInt32(); // bytes per element
            }
            if (CryEngine.Model.FILE_VERSION == FileVersionEnum.CryTek_3_6)
            {
                this.BytesPerElement = (UInt32)b.ReadInt16();        // Star Citizen 2.0 is using an int16 here now.
                b.ReadInt16();                                  // unknown value.   Doesn't look like padding though.
            }

            this.SkipBytes(b, 8);

            // Now do loops to read for each of the different Data Stream Types.  If vertices, need to populate Vector3s for example.
            switch (this.DataStreamType)
            {
                #region case DataStreamTypeEnum.VERTICES:

                case DataStreamTypeEnum.VERTICES:  // Ref is 0x00000000
                    this.Vertices = new Vector3[this.NumElements];

                    switch (this.BytesPerElement)
                    {
                        case 12:
                            for (Int32 i = 0; i < this.NumElements; i++)
                            {
                                this.Vertices[i].x = b.ReadSingle();
                                this.Vertices[i].y = b.ReadSingle();
                                this.Vertices[i].z = b.ReadSingle();
                            }
                            break;
                        case 8:  // Old Star Citizen files
                            for (Int32 i = 0; i < NumElements; i++)
                            {
                                // 2 byte floats.  Use the Half structure from TK.Math

                                Half xshort = new Half();
                                xshort.bits = b.ReadUInt16();
                                this.Vertices[i].x = xshort.ToSingle();

                                Half yshort = new Half();
                                yshort.bits = b.ReadUInt16();
                                this.Vertices[i].y = yshort.ToSingle();

                                Half zshort = new Half();
                                zshort.bits = b.ReadUInt16();
                                this.Vertices[i].z = zshort.ToSingle();

                                Half wshort = new Half();
                                wshort.bits = b.ReadUInt16();
                                this.Vertices[i].w = wshort.ToSingle();
                            }
                            break;
                        case 16:  // new Star Citizen files
                            for (Int32 i = 0; i < this.NumElements; i++)
                            {
                                this.Vertices[i].x = b.ReadSingle();
                                this.Vertices[i].y = b.ReadSingle();
                                this.Vertices[i].z = b.ReadSingle();
                                this.Vertices[i].w = b.ReadSingle(); // Sometimes there's a W to these structures.  Will investigate.
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
                    for (Int32 i = 0; i < this.NumElements; i++)
                    {
                        // These have to be divided by 32767 to be used properly (value between 0 and 1)
                        this.Tangents[i, 0].x = b.ReadInt16();
                        this.Tangents[i, 0].y = b.ReadInt16();
                        this.Tangents[i, 0].z = b.ReadInt16();
                        this.Tangents[i, 0].w = b.ReadInt16();

                        this.Tangents[i, 1].x = b.ReadInt16();
                        this.Tangents[i, 1].y = b.ReadInt16();
                        this.Tangents[i, 1].z = b.ReadInt16();
                        this.Tangents[i, 1].w = b.ReadInt16();
                    }
                    // Utils.Log(LogLevelEnum.Debug, "Offset is {0:X}", b.BaseStream.Position);
                    break;

                #endregion
                #region case DataStreamTypeEnum.COLORS:

                case DataStreamTypeEnum.COLORS:
                    if (this.BytesPerElement == 3)
                    {
                        this.RGBColors = new IRGB[this.NumElements];
                        for (Int32 i = 0; i < NumElements; i++)
                        {
                            this.RGBColors[i].r = b.ReadByte();
                            this.RGBColors[i].g = b.ReadByte();
                            this.RGBColors[i].b = b.ReadByte();
                        }
                    }
                    if (this.BytesPerElement == 4)
                    {
                        this.RGBAColors = new IRGBA[this.NumElements];
                        for (Int32 i = 0; i < this.NumElements; i++)
                        {
                            this.RGBAColors[i].r = b.ReadByte();
                            this.RGBAColors[i].g = b.ReadByte();
                            this.RGBAColors[i].b = b.ReadByte();
                            this.RGBAColors[i].a = b.ReadByte();
                        }
                    }
                    break;

                #endregion
                #region case DataStreamTypeEnum.VERTSUVS:

                case DataStreamTypeEnum.VERTSUVS:  // 3 half floats for verts, 6 unknown, 2 half floats for UVs
                    // Utils.Log(LogLevelEnum.Debug, "In VertsUVs...");
                    this.Vertices = new Vector3[this.NumElements];
                    this.Normals = new Vector3[this.NumElements];
                    this.UVs = new UV[this.NumElements];
                    if (this.BytesPerElement == 16)  // new Star Citizen files
                    {
                        for (Int32 i = 0; i < this.NumElements; i++)
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

                            Half xnorm = new Half();
                            xnorm.bits = b.ReadUInt16();
                            this.Normals[i].x = xnorm.ToSingle();

                            Half ynorm = new Half();
                            ynorm.bits = b.ReadUInt16();
                            this.Normals[i].y = ynorm.ToSingle();

                            Half znorm = new Half();
                            znorm.bits = b.ReadUInt16();
                            this.Normals[i].z = znorm.ToSingle();

                            Half uvu = new Half();
                            uvu.bits = b.ReadUInt16();
                            this.UVs[i].U = uvu.ToSingle();

                            Half uvv = new Half();
                            uvv.bits = b.ReadUInt16();
                            this.UVs[i].V = uvv.ToSingle();

                            //short w = b.ReadInt16();  // dump this as not needed.  Last 2 bytes are surplus...sort of.
                            //if (i < 20)
                            //{
                            //    Utils.Log(LogLevelEnum.Debug, "{0:F7} {1:F7} {2:F7} {3:F7} {4:F7}",
                            //        Vertices[i].x, Vertices[i].y, Vertices[i].z,
                            //        UVs[i].U, UVs[i].V);
                            //}
                        }
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
