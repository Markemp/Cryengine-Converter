using System.IO;

namespace CgfConverter;

public abstract class BaseRenderer
{
    public ArgsHandler Args { get; internal set; }
    public CryEngine CryData { get; internal set; }

    public BaseRenderer(ArgsHandler argsHandler, CryEngine cryEngine)
    {
        Args = argsHandler;
        CryData = cryEngine;
    }

    public abstract void Render(string outputDir = null, bool preservePath = true);

    internal string GetOutputFile(string extension, string outputDir = null, bool preservePath = true)
    {
        var outputFile = string.Format("temp.{0}", extension);

        if (string.IsNullOrWhiteSpace(outputDir))
        {
            // Empty output directory means place alongside original models
            // If you want relative path, use "."
            if (Args.NoConflicts)
            {
                outputFile = Path.Combine(new FileInfo(CryData.InputFile).DirectoryName, string.Format("{0}_out.{1}", Path.GetFileNameWithoutExtension(CryData.InputFile), extension));
            }
            else
            {
                outputFile = Path.Combine(new FileInfo(CryData.InputFile).DirectoryName, string.Format("{0}.{1}", Path.GetFileNameWithoutExtension(CryData.InputFile), extension));
            }
        }
        else
        {
            // If we have an output directory
            var preserveDir = preservePath ? Path.GetDirectoryName(CryData.InputFile) : "";

            // Remove drive letter if necessary
            if (!string.IsNullOrWhiteSpace(preserveDir) && !string.IsNullOrWhiteSpace(Path.GetPathRoot(preserveDir)))
            {
                preserveDir = preserveDir.Replace(Path.GetPathRoot(preserveDir), "");
            }

            outputFile = Path.Combine(outputDir, preserveDir, Path.ChangeExtension(Path.GetFileNameWithoutExtension(CryData.InputFile), extension));
        }

        return outputFile;
    }

    public bool IsNodeNameExcluded(string nodeName)
    {
        foreach (var excname in Args.ExcludeNodeNames)
        {
            if (nodeName.ToLower().StartsWith(excname.ToLower()))
            {
                Utilities.Log(LogLevelEnum.Debug, $"Node matched excludename '{excname}'");
                return true;
            }
        }

        return false;
    }

    public bool IsMeshMaterialExcluded(string materialName)
    {
        foreach (var excname in Args.ExcludeMaterialNames)
        {
            if (materialName.ToLower().StartsWith(excname.ToLower()))
            {
                Utilities.Log(LogLevelEnum.Debug, $"Material name matched excludemat '{excname}'");
                return true;
            }
        }
        return false;
    }
}
