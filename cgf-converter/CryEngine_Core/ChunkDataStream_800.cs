using OpenTK;
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
        // This includes changes for 2.6 created by Dymek (byte4/1/2hex, and 20 byte per element vertices).  Thank you!
        public static float Byte4HexToFloat(string hexString)
        {
            uint num = uint.Parse(hexString, System.Globalization.NumberStyles.AllowHexSpecifier);
            var bytes = BitConverter.GetBytes(num);
            return BitConverter.ToSingle(bytes, 0);
        }

        public static int Byte1HexToIntType2(string hexString)
        {
            int value = Convert.ToSByte(hexString, 16);
            return value;
        }

        public static float Byte2HexIntFracToFloat2(string hexString)
        {
            string sintPart = hexString.Substring(0, 2);
            string sfracPart = hexString.Substring(2, 2);

            int intPart = Byte1HexToIntType2(sintPart);

            short intnum = short.Parse(sfracPart, System.Globalization.NumberStyles.AllowHexSpecifier);
            var intbytes = BitConverter.GetBytes(intnum);
            string intbinary = Convert.ToString(intbytes[0], 2).PadLeft(8, '0');
            string binaryIntPart = intbinary;

            short num = short.Parse(sfracPart, System.Globalization.NumberStyles.AllowHexSpecifier);
            var bytes = BitConverter.GetBytes(num);
            string binary = Convert.ToString(bytes[0], 2).PadLeft(8, '0');
            string binaryFracPart = binary;

            //convert Fractional Part
            float dec = 0;
            for (int i = 0; i < binaryFracPart.Length; i++)
            {
                if (binaryFracPart[i] == '0') continue;
                dec += (float)Math.Pow(2, (i + 1) * (-1));
            }
            float number = 0;
            number = (float)intPart + dec;
            /*if (intPart > 0) { number = (float)intPart + dec; }
            if (intPart < 0) { number = (float)intPart - dec; }
            if (intPart == 0) { number =  dec; }*/
            return number;
        }

         public override void Read(BinaryReader b)
        {
            base.Read(b);

            this.Flags2 = b.ReadUInt32(); // another filler
            UInt32 tmpdataStreamType = b.ReadUInt32();
            this.DataStreamType = (DataStreamTypeEnum)Enum.ToObject(typeof(DataStreamTypeEnum), tmpdataStreamType);
            this.NumElements = b.ReadUInt32(); // number of elements in this chunk

            if (this._model.FileVersion == FileVersionEnum.CryTek_3_5 || this._model.FileVersion == FileVersionEnum.CryTek_3_4)
            {
                this.BytesPerElement = b.ReadUInt32(); // bytes per element
            }
            if (this._model.FileVersion == FileVersionEnum.CryTek_3_6)
            {
                this.BytesPerElement = (UInt32)b.ReadInt16();        // Star Citizen 2.0 is using an int16 here now.
                b.ReadInt16();                                       // unknown value.   Doesn't look like padding though.
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

                                Half xshort = new Half(b.ReadUInt16());
                                //uint xshortbits = b.ReadUInt16();
                                //xshort.bits = b.ReadUInt16();
                                this.Vertices[i].x = xshort.ToSingle();

                                Half yshort = new Half(b.ReadUInt16());
                                //yshort.bits = b.ReadUInt16();
                                this.Vertices[i].y = yshort.ToSingle();

                                Half zshort = new Half(b.ReadUInt16());
                                //zshort.bits = b.ReadUInt16();
                                this.Vertices[i].z = zshort.ToSingle();

                                Half wshort = new Half(b.ReadUInt16());
                                //wshort.bits = b.ReadUInt16();
                                this.Vertices[i].w = wshort.ToSingle();
                            }
                            break;
                        case 16:
                            //Console.WriteLine("method: (3)");
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
                                // These have to be divided by 32767 to be used properly (value between 0 and 1)
                                this.Tangents[i, 0].x = b.ReadSByte();
                                this.Tangents[i, 0].y = b.ReadSByte();
                                this.Tangents[i, 0].z = b.ReadSByte();
                                this.Tangents[i, 0].w = b.ReadSByte();

                                this.Tangents[i, 1].x = b.ReadSByte();
                                this.Tangents[i, 1].y = b.ReadSByte();
                                this.Tangents[i, 1].z = b.ReadSByte();
                                this.Tangents[i, 1].w = b.ReadSByte();
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
                #region case DataStreamTypeEnum.VERTSUVS:

                case DataStreamTypeEnum.VERTSUVS:  // 3 half floats for verts, 3 half floats for normals, 2 half floats for UVs
                    // Utils.Log(LogLevelEnum.Debug, "In VertsUVs...");
                    this.Vertices = new Vector3[this.NumElements];
                    this.Normals = new Vector3[this.NumElements];
                    this.UVs = new UV[this.NumElements];
                    switch (this.BytesPerElement)  // new Star Citizen files
                    {
                        case 20:  // Dymek wrote this
                            for (Int32 i = 0; i < this.NumElements; i++)
                            {
                                uint bver = 0;
                                float ver = 0;

                                bver = b.ReadUInt32();
                                ver = Byte4HexToFloat(bver.ToString("X8"));
                                this.Vertices[i].x = ver;

                                bver = b.ReadUInt32();
                                ver = Byte4HexToFloat(bver.ToString("X8"));
                                this.Vertices[i].y = ver;

                                bver = b.ReadUInt32();
                                ver = Byte4HexToFloat(bver.ToString("X8"));
                                this.Vertices[i].z = ver;

                                Half xnorm = new Half(b.ReadUInt16());
                                //xnorm.bits = b.ReadUInt16();
                                this.Normals[i].x = xnorm.ToSingle();

                                Half ynorm = new Half(b.ReadUInt16());
                                //ynorm.bits = b.ReadUInt16();
                                this.Normals[i].y = ynorm.ToSingle();

                                Half uvu = new Half(b.ReadUInt16());
                                //uvu.bits = b.ReadUInt16();
                                this.UVs[i].U = uvu.ToSingle();

                                Half uvv = new Half(b.ReadUInt16());
                                //uvv.bits = b.ReadUInt16();
                                this.UVs[i].V = uvv.ToSingle();
                            }
                            break;
                        case 16:   // Dymek updated this.
                            //Console.WriteLine("method: (5), 3 half floats for verts, 3 half floats for normals, 2 half floats for UVs");
                            for (Int32 i = 0; i < this.NumElements; i++)
                            {
                                ushort bver = 0;
                                float ver = 0;

                                bver = b.ReadUInt16();
                                ver = Byte2HexIntFracToFloat2(bver.ToString("X4")) / 127;
                                this.Vertices[i].x = ver;

                                bver = b.ReadUInt16();
                                ver = Byte2HexIntFracToFloat2(bver.ToString("X4")) / 127;
                                this.Vertices[i].y = ver;

                                bver = b.ReadUInt16();
                                ver = Byte2HexIntFracToFloat2(bver.ToString("X4")) / 127;
                                this.Vertices[i].z = ver;

                                bver = b.ReadUInt16();
                                ver = Byte2HexIntFracToFloat2(bver.ToString("X4")) / 127;
                                this.Normals[i].x = ver;

                                bver = b.ReadUInt16();
                                ver = Byte2HexIntFracToFloat2(bver.ToString("X4")) / 127;
                                this.Normals[i].y = ver;

                                bver = b.ReadUInt16();
                                ver = Byte2HexIntFracToFloat2(bver.ToString("X4")) / 127;
                                this.Normals[i].z = ver;

                                bver = b.ReadUInt16();
                                ver = Byte2HexIntFracToFloat2(bver.ToString("X4")) / 127;
                                this.UVs[i].U = ver;

                                bver = b.ReadUInt16();
                                ver = Byte2HexIntFracToFloat2(bver.ToString("X4")) / 127;
                                this.UVs[i].V = ver;

                                #region Test version using new Halfs

                                //Half xpos = new Half(b.ReadUInt16());
                                //this.Vertices[i].x = xpos.ToSingle();
                                ////byte[] xbytes = b.ReadBytes(2); //new byte[4];
                                ////this.Vertices[i].x = Half.FromBytes(xbytes, 0);
                                //Half ypos = new Half(b.ReadUInt16());
                                //this.Vertices[i].y = ypos.ToSingle();

                                ////byte[] ybytes = b.ReadBytes(2); //new byte[4];
                                ////this.Vertices[i].y = Half.FromBytes(ybytes, 0);
                                //Half zpos = new Half(b.ReadUInt16());
                                //this.Vertices[i].z = zpos.ToSingle();

                                ////byte[] zbytes = b.ReadBytes(2); //new byte[4];
                                ////this.Vertices[i].z = Half.FromBytes(zbytes, 0); 

                                //Half xnorm = new Half(b.ReadUInt16());
                                //this.Normals[i].x = xnorm.ToSingle();

                                //Half ynorm = new Half(b.ReadUInt16());
                                //this.Normals[i].y = ynorm.ToSingle();

                                //Half znorm = new Half(b.ReadUInt16());
                                //this.Normals[i].z = znorm.ToSingle();

                                //Half uvu = new Half(b.ReadUInt16());
                                //this.UVs[i].U = uvu.ToSingle();

                                //Half uvv = new Half(b.ReadUInt16());
                                //this.UVs[i].V = uvv.ToSingle();

                                #endregion

                                #region Legacy version using Halfs
                                //Half xshort = new Half();
                                //xshort.bits = b.ReadUInt16();
                                //this.Vertices[i].x = xshort.ToSingle();

                                //Half yshort = new Half();
                                //yshort.bits = b.ReadUInt16();
                                //this.Vertices[i].y = yshort.ToSingle();

                                //Half zshort = new Half();
                                //zshort.bits = b.ReadUInt16();
                                //this.Vertices[i].z = zshort.ToSingle();

                                //Half xnorm = new Half();
                                //xnorm.bits = b.ReadUInt16();
                                //this.Normals[i].x = xnorm.ToSingle();

                                //Half ynorm = new Half();
                                //ynorm.bits = b.ReadUInt16();
                                //this.Normals[i].y = ynorm.ToSingle();

                                //Half znorm = new Half();
                                //znorm.bits = b.ReadUInt16();
                                //this.Normals[i].z = znorm.ToSingle();

                                //Half uvu = new Half();
                                //uvu.bits = b.ReadUInt16();
                                //this.UVs[i].U = uvu.ToSingle();

                                //Half uvv = new Half();
                                //uvv.bits = b.ReadUInt16();
                                //this.UVs[i].V = uvv.ToSingle();
                                #endregion
                            }
                            break;
                        default:
                            Utils.Log("Unknown VertUV structure");
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
