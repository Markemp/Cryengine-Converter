using System;
using System.IO;

namespace CgfConverter
{
    public class Blender : BaseRenderer  //  Class to allow exporting to .blend files.
    {
        FileInfo blendOutputFile;

        public Blender(ArgsHandler argsHandler, CryEngine cryEngine) : base(argsHandler, cryEngine) { }

        public override void Render(String outputDir = null, Boolean preservePath = true)
        {
            // The root of the functions to write Blend files
            // At this point, we should have a CgfData object, fully populated.
            Utilities.Log(LogLevelEnum.Debug);
            Utilities.Log(LogLevelEnum.Debug, "*** Starting WriteBlend() ***");
            Utilities.Log(LogLevelEnum.Debug);

            // File name will be "object name.blend"
            blendOutputFile = new FileInfo(this.GetOutputFile("blend", outputDir, preservePath));
            // using (BinaryWriter b = new BinaryWriter(File.Open(blendOutputFile.FullName, FileMode.Create)))
            // {
            //     WriteHeader(b);
            // }
        }

        public void WriteHeader(BinaryWriter b)
        {
            // Writes the header for the blend file.
            // Header is 12 bytes.  We will generally write the same way each time.  BLENDER-v275.  - is little end, v is 64 bit
            char[] identifier = new char[7];
            char pointersize = '-';             // - is 64 bit, _ is 32 bit
            char endian = 'v';                  // v is little endian, V is big endian
            char[] version = new char[3];       // 275

            identifier = "BLENDER".ToCharArray();
            version = "275".ToCharArray();
            b.Write(identifier);
            b.Write(pointersize);
            b.Write(endian);
            b.Write(version);
        }
        public void WriteFileBlockHeader(BinaryWriter b)
        {
            // Header to each of the file blocks.
        }

    }
}
