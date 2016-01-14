using OpenTK.Math;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CgfConverter.CryEngine_Core
{
    public abstract class ChunkDataStream : Chunk // cccc0016:  Contains data such as vertices, normals, etc.
    {
        public UInt32 Flags; // not used, but looks like the start of the Data Stream chunk
        public UInt32 Flags1; // not used.  UInt32 after Flags that looks like offsets
        public UInt32 Flags2; // not used, looks almost like a filler.
        public DataStreamTypeEnum DataStreamType; // type of data (vertices, normals, uv, etc)
        public UInt32 NumElements; // Number of data entries
        public UInt32 BytesPerElement; // Bytes per data entry
        public UInt32 Reserved1;
        public UInt32 Reserved2;
        // Need to be careful with using float for Vertices and normals.  technically it's a floating point of length BytesPerElement.  May need to fix this.
        public Vector3[] Vertices;  // For dataStreamType of 0, length is NumElements. 
        public Vector3[] Normals;   // For dataStreamType of 1, length is NumElements.

        public UV[] UVs;            // for datastreamType of 2, length is NumElements.
        public IRGB[] RGBColors;    // for dataStreamType of 3, length is NumElements.  Bytes per element of 3
        public IRGBA[] RGBAColors;  // for dataStreamType of 4, length is NumElements.  Bytes per element of 4
        public UInt32[] Indices;    // for dataStreamType of 5, length is NumElements.
        // For Tangents on down, this may be a 2 element array.  See line 846+ in cgf.xml
        public Tangent[,] Tangents;  // for dataStreamType of 6, length is NumElements,2.  
        public Byte[,] ShCoeffs;     // for dataStreamType of 7, length is NumElement,BytesPerElements.
        public Byte[,] ShapeDeformation; // for dataStreamType of 8, length is NumElements,BytesPerElement.
        public Byte[,] BoneMap;      // for dataStreamType of 9, length is NumElements,BytesPerElement.
        public Byte[,] FaceMap;      // for dataStreamType of 10, length is NumElements,BytesPerElement.
        public Byte[,] VertMats;     // for dataStreamType of 11, length is NumElements,BytesPerElement.

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

        public override void WriteChunk()
        {
            //string tmpDataStream = new string(Name);
            Utils.Log(LogLevelEnum.Verbose, "*** START DATASTREAM ***");
            Utils.Log(LogLevelEnum.Verbose, "    ChunkType:                       {0}", ChunkType);
            Utils.Log(LogLevelEnum.Verbose, "    Version:                         {0:X}", Version);
            Utils.Log(LogLevelEnum.Verbose, "    DataStream chunk starting point: {0:X}", Flags);
            Utils.Log(LogLevelEnum.Verbose, "    Chunk ID:                        {0:X}", ID);
            Utils.Log(LogLevelEnum.Verbose, "    DataStreamType:                  {0}", DataStreamType);
            Utils.Log(LogLevelEnum.Verbose, "    Number of Elements:              {0}", NumElements);
            Utils.Log(LogLevelEnum.Verbose, "    Bytes per Element:               {0}", BytesPerElement);
            Utils.Log(LogLevelEnum.Verbose, "*** END DATASTREAM ***");

        }
    }
}
