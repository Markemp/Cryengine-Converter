using CgfConverter;
using System;
using System.IO;

namespace BinaryReaderExtensions
{
    // This includes changes for 2.6 created by Dymek (byte4/1/2hex, and 20 byte per element vertices).  Thank you!
    public static class BinaryReaderExtensions
    {
        public static float ReadCryHalf(this BinaryReader r)
        {
            // https://docs.microsoft.com/en-us/windows/win32/direct3d11/floating-point-rules#16-bit-floating-point-rules
            var bver = r.ReadUInt16();
            return Byte2HexIntFracToFloat2(bver.ToString("X4")) / 127;
        }

        public static float ReadHalf(this BinaryReader r)
        {
            CgfConverter.Half xshort = new CgfConverter.Half();
            xshort.bits = r.ReadUInt16();
            return xshort.ToSingle();
        }

        public static Quaternion ReadQuaternion(this BinaryReader r)
        {
            var q = new Quaternion()
            {
                x = r.ReadSingle(),
                y = r.ReadSingle(),
                z = r.ReadSingle(),
                w = r.ReadSingle()
            };

            return q;
        }

        public static IRGBA ReadColor(this BinaryReader r)
        {
            IRGBA c = new IRGBA();
            c.r = r.ReadByte();
            c.g = r.ReadByte();
            c.b = r.ReadByte();
            c.a = r.ReadByte();
            return c;
        }

        static float Byte4HexToFloat(string hexString)
        {
            uint num = uint.Parse(hexString, System.Globalization.NumberStyles.AllowHexSpecifier);
            var bytes = BitConverter.GetBytes(num);
            return BitConverter.ToSingle(bytes, 0);
        }

        static int Byte1HexToIntType2(string hexString)
        {
            int value = Convert.ToSByte(hexString, 16);
            return value;
        }

        static float Byte2HexIntFracToFloat2(string hexString)
        {
            string sintPart = hexString.Substring(0, 2);
            string sfracPart = hexString.Substring(2, 2);

            int intPart = Byte1HexToIntType2(sintPart);

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
            float number = (float)intPart + dec;
            return number;
        }
    }
}
