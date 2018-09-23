using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public abstract void Render(String outputDir = null, Boolean preservePath = true);

        internal String GetOutputFile(String extension, String outputDir = null, Boolean preservePath = true)
        {
            String outputFile = String.Format("temp.{0}", extension);

            if (String.IsNullOrWhiteSpace(outputDir))
            {
                // Empty output directory means place alongside original models
                // If you want relative path, use "."
                if (Args.NoConflicts )
                {
                    outputFile = Path.Combine(new FileInfo(this.CryData.InputFile).DirectoryName, String.Format("{0}_out.{1}", Path.GetFileNameWithoutExtension(this.CryData.InputFile), extension));
                }
                else
                {
                    outputFile = Path.Combine(new FileInfo(this.CryData.InputFile).DirectoryName, String.Format("{0}.{1}", Path.GetFileNameWithoutExtension(this.CryData.InputFile), extension));
                }
            }
            else
            {
                // If we have an output directory
                String preserveDir = preservePath ? Path.GetDirectoryName(this.CryData.InputFile) : "";

                // Remove drive letter if necessary
                if (!String.IsNullOrWhiteSpace(preserveDir) && !String.IsNullOrWhiteSpace(Path.GetPathRoot(preserveDir)))
                {
                    preserveDir = preserveDir.Replace(Path.GetPathRoot(preserveDir), "");
                }

                outputFile = Path.Combine(outputDir, preserveDir, Path.ChangeExtension(Path.GetFileNameWithoutExtension(this.CryData.InputFile), extension));
            }

            return outputFile;
        }
    }
}
