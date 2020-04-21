using System.IO;

namespace CgfConverter
{
    class FBX                   //  Class to allow exporting to FBX files.
    {
#pragma warning disable CS0169 // The field 'FBX.fbxOutputFile' is never used
        readonly FileInfo fbxOutputFile;
#pragma warning restore CS0169 // The field 'FBX.fbxOutputFile' is never used

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
