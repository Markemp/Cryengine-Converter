using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CgfConverter
{
    class FBX                   //  Class to allow exporting to FBX files.
    {
        FileInfo fbxOutputFile;

        public void WriteFBXFile(CgfData cgfData)
        {
            // The root of the functions to write FBX binary files
            // At this point, we should have a CgfData object, fully populated.
            Console.WriteLine();
            Console.WriteLine("*** Starting WriteFBX() ***");
            Console.WriteLine();

        }
    }
}
