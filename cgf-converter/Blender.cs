using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CgfConverter
{
    public class Blender  //  Class to allow exporting to .blend files.
    {
        FileInfo blendOutputFile;
        public ArgsHandler Args { get; internal set; }
        public CryEngine CryData { get; set; }

        public Blender(ArgsHandler argsHandler)
        {
            this.Args = argsHandler;
        }

        public void WriteBlend(CryEngine cryEngine)
        {
            this.CryData = cryEngine;
            // The root of the functions to write Blend files
            // At this point, we should have a CgfData object, fully populated.
            Utils.Log(LogLevelEnum.Debug);
            Utils.Log(LogLevelEnum.Debug, "*** Starting WriteBlend() ***");
            Utils.Log(LogLevelEnum.Debug);

            // File name will be "object name.blend"
            // blendOutputFile = new FileInfo(cgfData.RootNode.Name + ".blend");
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
