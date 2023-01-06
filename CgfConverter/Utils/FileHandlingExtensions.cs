using System.IO;
using System.Text;
using CgfConverter;

namespace Extensions;

public static class FileHandlingExtensions
{
    /// <summary>Material texture file extensions used to search resolve texture paths to files on disk</summary>
    private static readonly string[] TextureExtensions = {".dds", ".png", ".tif"};
    
    /// <summary>Attempts to resole a material path to the correct file extension, and normalizes the path separators</summary>
    public static string ResolveTextureFile(string mtl, DirectoryInfo dataDir)
    {
        StringBuilder mtlfile = new();
        string cleanName = CleanTextureFileName(mtl);
        if (dataDir.ToString() == ".")
            mtlfile.Append(CleanTextureFileName(mtl)); // Resolve in current directory
        else
            mtlfile.Append($"{dataDir.FullName}/{Path.GetDirectoryName(mtl)}/{cleanName}");

        mtlfile.Replace("\\", "/");

        foreach (string ext in TextureExtensions)
        {
            if (File.Exists($"{mtlfile}{ext}"))
                return $"{mtlfile}{ext}".Replace("\\", "/");
        }
        
        // check in textures sub-directory if we didn't find it
        mtlfile.Replace(cleanName, $"textures/{cleanName}");
        foreach (string ext in TextureExtensions)
        {
            if (File.Exists($"{mtlfile}{ext}"))
                return $"{mtlfile}{ext}".Replace("\\", "/");
        }

        Utilities.Log(LogLevelEnum.Debug, "Could not find extension for material texture \"{0}\". Defaulting to .dds", mtlfile);
        return $"{mtlfile}.dds".Replace("\\", "/");;
    }

    /// <summary>Takes the texture file name and returns just the file name with no extension</summary>
    public static string CleanTextureFileName(string cleanMe) => Path.GetFileNameWithoutExtension(cleanMe);
}