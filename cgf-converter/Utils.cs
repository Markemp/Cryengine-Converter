using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CgfConverter
{
    public enum StringSizeEnum
    {
        Int8 = 1,
        Int16 = 2,
        Int32 = 4,
    }

    public static class Utils
    {
        public static Version GetVersion()
        {
            AssemblyVersionAttribute assemblyVersion = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyVersionAttribute>();

            if (assemblyVersion != null)
                return new Version(assemblyVersion.Version);

            AssemblyFileVersionAttribute assemblyFileVersion = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>();
            
            if (assemblyFileVersion != null)
                return new Version(assemblyFileVersion.Version);

            return new Version(0, 8, 0, 1);
        }

        /// <summary>
        /// Read a Length-prefixed string from the stream
        /// </summary>
        /// <param name="binaryReader"></param>
        /// <param name="byteLength">Size of the Length representation</param>
        /// <returns></returns>
        public static String ReadPString(this BinaryReader binaryReader, StringSizeEnum byteLength = StringSizeEnum.Int32)
        {
            Int32 stringLength = 0;

            switch (byteLength)
            {
                case StringSizeEnum.Int8:
                    stringLength = binaryReader.ReadByte();
                    break;
                case StringSizeEnum.Int16:
                    stringLength = binaryReader.ReadInt16();
                    break;
                case StringSizeEnum.Int32:
                    stringLength = binaryReader.ReadInt32();
                    break;
                default:
                    throw new NotSupportedException("Only Int8, Int16, and Int32 string sizes are supported");
            }

            // If there is actually a string to read
            if (stringLength > 0)
            {
                return new String(binaryReader.ReadChars(stringLength));
            }

            return null;
        }

        /// <summary>
        /// Read a NULL-Terminated string from the stream
        /// </summary>
        /// <param name="binaryReader"></param>
        /// <returns></returns>
        public static String ReadCString(this BinaryReader binaryReader)
        {
            Int32 stringLength = 0;

            while (binaryReader.ReadByte() != 0)
                stringLength++;

            binaryReader.BaseStream.Seek(0 - stringLength - 1, SeekOrigin.Current);

            Char[] chars = binaryReader.ReadChars(stringLength + 1);

            // If there is actually a string to read
            if (stringLength > 0)
            {
                return new String(chars, 0, stringLength);
            }
            
            return null;
        }

        /// <summary>
        /// Read a Fixed-Length string from the stream
        /// </summary>
        /// <param name="binaryReader"></param>
        /// <param name="stringLength">Size of the String</param>
        /// <returns></returns>
        public static String ReadFString(this BinaryReader binaryReader, Int32 stringLength)
        {
            Char[] chars = binaryReader.ReadChars(stringLength);
                
            for (Int32 i = 0; i < stringLength; i++)
            {
                if (chars[i] == 0)
                {
                    return new String(chars, 0, i);
                }
            }

            return new String(chars);
        }
    }
}
