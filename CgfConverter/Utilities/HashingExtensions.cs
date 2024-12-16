using System.Text;
using System;
using System.IO.Hashing;

namespace CgfConverter.Utilities;

internal static class HashingExtensions
{
    public static uint ComputeCrc32(this string input)
    {
        if (string.IsNullOrEmpty(input))
            throw new ArgumentNullException(nameof(input), "Input string cannot be null or empty.");

        byte[] bytes = Encoding.UTF8.GetBytes(input);

        byte[] hash = Crc32.Hash(bytes);

        // Convert the first 4 bytes of the hash to a uint (big-endian to match CRC conventions)
        return (uint)(hash[0] << 24 | hash[1] << 16 | hash[2] << 8 | hash[3]);
    }
}
