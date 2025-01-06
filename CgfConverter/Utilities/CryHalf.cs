using System;

namespace CgfConverter.Utilities;

/// <summary>
/// CryHalf Max value: 131008, min value: -130944
/// Dymek Half max value: 1.0078433, min value: -1.007874
/// Half max value: 65500, min value: -65500
/// </summary>
public static class CryHalf
{
    public static float ConvertCryHalfToFloat(ushort value)
    {
        uint mantissa;
        uint exponent;
        uint result;

        mantissa = (uint)(value & 0x03FF);

        if ((value & 0x7C00) != 0)  // The value is normalized
            exponent = (uint)(value >> 10 & 0x1F);
        else if (mantissa != 0)     // The value is denormalized
        {
            // Normalize the value in the resulting float
            exponent = 1;

            do
            {
                exponent--;
                mantissa <<= 1;
            } while ((mantissa & 0x0400) == 0);

            mantissa &= 0x03FF;
        }
        else                        // The value is zero
            exponent = 4294967184;  // (uint)-112

        result = ((uint)value & 0x8000) << 16 | // Sign
            exponent + 112 << 23 | // Exponent
            mantissa << 13;          // Mantissa
        return BitConverter.ToSingle(BitConverter.GetBytes(result), 0);
    }

    /// <summary>
    /// A fancy implementation of UNORMs.
    /// </summary>
    public static float ConvertDymekHalfToFloat(ushort value) => Byte2HexIntFracToFloat2(value.ToString("X4")) / 127;

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
            dec += (float)Math.Pow(2, (i + 1) * -1);
        }
        float number = intPart + dec;
        return number;
    }

    static int Byte1HexToIntType2(string hexString) => Convert.ToSByte(hexString, 16);
}
