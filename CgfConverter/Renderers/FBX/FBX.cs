using System.IO;

namespace CgfConverter
{
    class FBX                   //  Class to allow exporting to FBX files.
    {
        readonly FileInfo fbxOutputFile;

        public void WriteFBXFile(CryEngine cryData)
        {
            // The root of the functions to write FBX binary files
            // At this point, we should have a CgfData object, fully populated.
            Utils.Log(LogLevelEnum.Verbose);
            Utils.Log(LogLevelEnum.Verbose, "*** Starting WriteFBX() ***");
            Utils.Log(LogLevelEnum.Verbose);
        }
    }
}
