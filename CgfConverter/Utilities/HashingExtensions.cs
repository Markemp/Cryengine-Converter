using System.Text;
using System;
using System.IO.Hashing;

namespace CgfConverter.Utilities;

public static class HashingExtensions
{
    // This method is used to calculate the controller id for a bone based on its name.
    public static uint ComputeCrc32(this string input)
    {
        if (string.IsNullOrEmpty(input))
            throw new ArgumentNullException(nameof(input), "Input string cannot be null or empty.");

        byte[] bytes = Encoding.UTF8.GetBytes(input);

        byte[] hash = Crc32.Hash(bytes);

        return (uint)(hash[0] << 24 | hash[1] << 16 | hash[2] << 8 | hash[3]);
    }
}

public static class Crc32CryEngine
{
    private static readonly uint[] Table = InitializeTable();
    private const uint Polynomial = 0xEDB88320; // Reversed polynomial (used in lookup table)

    private static uint[] InitializeTable()
    {
        var table = new uint[256];
        for (uint i = 0; i < 256; i++)
        {
            uint crc = i;
            for (int j = 0; j < 8; j++)
            {
                if ((crc & 1) != 0)
                    crc = (crc >> 1) ^ Polynomial;
                else
                    crc >>= 1;
            }
            table[i] = crc;
        }
        return table;
    }

    public static uint Compute(string input)
    {
        if (string.IsNullOrEmpty(input)) throw new ArgumentNullException(nameof(input));

        byte[] bytes = Encoding.UTF8.GetBytes(input);
        uint crc = 0xFFFFFFFF; // Initial value

        foreach (byte b in bytes)
        {
            crc = (crc >> 8) ^ Table[(crc & 0xFF) ^ b];
        }

        return ~crc; // Final XOR
    }
}
