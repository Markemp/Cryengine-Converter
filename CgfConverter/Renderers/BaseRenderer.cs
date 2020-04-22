using System;
using System.IO;

namespace CgfConverter
{
    public abstract class BaseRenderer
    {
        public ArgsHandler Args { get; internal set; }
        public CryEngine CryData { get; internal set; }

        public BaseRenderer(ArgsHandler argsHandler, CryEngine cryEngine)
        {
            this.Args = argsHandler;
            this.CryData = cryEngine;
        }

        public abstract void Render(string outputDir = null, bool preservePath = true);

        internal string GetOutputFile(string extension, string outputDir = null, bool preservePath = true)
        {
            string outputFile = string.Format("temp.{0}", extension);

            if (string.IsNullOrWhiteSpace(outputDir))
            {
                // Empty output directory means place alongside original models
                // If you want relative path, use "."
                if (Args.NoConflicts)
                {
                    outputFile = Path.Combine(new FileInfo(this.CryData.InputFile).DirectoryName, string.Format("{0}_out.{1}", Path.GetFileNameWithoutExtension(this.CryData.InputFile), extension));
                }
                else
                {
                    outputFile = Path.Combine(new FileInfo(this.CryData.InputFile).DirectoryName, string.Format("{0}.{1}", Path.GetFileNameWithoutExtension(this.CryData.InputFile), extension));
                }
            }
            else
            {
                // If we have an output directory
                string preserveDir = preservePath ? Path.GetDirectoryName(this.CryData.InputFile) : "";

                // Remove drive letter if necessary
                if (!string.IsNullOrWhiteSpace(preserveDir) && !string.IsNullOrWhiteSpace(Path.GetPathRoot(preserveDir)))
                {
                    preserveDir = preserveDir.Replace(Path.GetPathRoot(preserveDir), "");
                }

                outputFile = Path.Combine(outputDir, preserveDir, Path.ChangeExtension(Path.GetFileNameWithoutExtension(this.CryData.InputFile), extension));
            }

            return outputFile;
        }
    }
}
