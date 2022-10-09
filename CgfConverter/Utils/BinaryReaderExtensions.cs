using CgfConverter;
using CgfConverter.Structs;
using System;
using System.IO;
using System.Numerics;

namespace Extensions;

// This includes changes for 2.6 created by Dymek (byte4/1/2hex, and 20 byte per element vertices).  Thank you!
public static class BinaryReaderExtensions
{
    public static float ReadCryHalf(this BinaryReader r)
    {
        // https://docs.microsoft.com/en-us/windows/win32/direct3d11/floating-point-rules#16-bit-floating-point-rules
        var bver = r.ReadUInt16();
        return Byte2HexIntFracToFloat2(bver.ToString("X4")) / 127;
    }

    public static Vector3 ReadVector3(this BinaryReader r, InputType inputType = InputType.Single)
    {
        Vector3 v;
        switch (inputType)
        {
            case InputType.Single:
                v = new()
                {
                    X = r.ReadSingle(),
                    Y = r.ReadSingle(),
                    Z = r.ReadSingle()
                };
                break;
            case InputType.Half:
                v = new()
                {
                    X = (float)r.ReadHalf(),
                    Y = (float)r.ReadHalf(),
                    Z = (float)r.ReadHalf()
                };
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return v;
    }

    public static Quaternion ReadQuaternion(this BinaryReader r, InputType inputType = InputType.Single)
    {
        Quaternion q;

        switch (inputType)
        {
            case InputType.Single:
                q = new Quaternion()
                {
                    X = r.ReadSingle(),
                    Y = r.ReadSingle(),
                    Z = r.ReadSingle(),
                    W = r.ReadSingle()
                };
                break;
            case InputType.Half:
                q = new Quaternion()
                {
                    X = (float)r.ReadHalf(),
                    Y = (float)r.ReadHalf(),
                    Z = (float)r.ReadHalf(),
                    W = (float)r.ReadHalf()
                };
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return q;
    }

    public static IRGBA ReadColor(this BinaryReader r)
    {
        var c = new IRGBA()
        {
            r = r.ReadByte(),
            g = r.ReadByte(),
            b = r.ReadByte(),
            a = r.ReadByte()
        };
        return c;
    }

    public static IRGBA ReadColorBGRA(this BinaryReader r)
    {
        var c = new IRGBA()
        {
            b = r.ReadByte(),
            g = r.ReadByte(),
            r = r.ReadByte(),
            a = r.ReadByte()
        };
        return c;
    }

    public static Matrix3x3 ReadMatrix3x3(this BinaryReader reader)
    {
        // Reads a Matrix33 structure
        if (reader == null)
            throw new ArgumentNullException(nameof(reader));

        Matrix3x3 m = new()
        {
            M11 = reader.ReadSingle(),
            M12 = reader.ReadSingle(),
            M13 = reader.ReadSingle(),
            M21 = reader.ReadSingle(),
            M22 = reader.ReadSingle(),
            M23 = reader.ReadSingle(),
            M31 = reader.ReadSingle(),
            M32 = reader.ReadSingle(),
            M33 = reader.ReadSingle()
        };

        return m;
    }

    public static Matrix3x4 ReadMatrix3x4(this BinaryReader r)
    {
        if (r == null)
            throw new ArgumentNullException(nameof(r));

        Matrix3x4 m = new()
        {
            M11 = r.ReadSingle(),
            M12 = r.ReadSingle(),
            M13 = r.ReadSingle(),
            M14 = r.ReadSingle(),
            M21 = r.ReadSingle(),
            M22 = r.ReadSingle(),
            M23 = r.ReadSingle(),
            M24 = r.ReadSingle(),
            M31 = r.ReadSingle(),
            M32 = r.ReadSingle(),
            M33 = r.ReadSingle(),
            M34 = r.ReadSingle()
        };

        return m;
    }

    public enum InputType
    {
        Half,
        CryHalf,
        Single,
        Double
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
