using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CgfConverter.Models.Materials;
using Extensions;

namespace CgfConverter.Renderers;

internal static class RendererUtilities
{
    private static int _counter;

    internal static FileInfo FormatOutputFileName(
        this Args args,
        string extension,
        string referenceName,
        string? layerName = null)
    {
        var dirName = Path.GetDirectoryName(referenceName);
        var modelDir = string.IsNullOrEmpty(dirName) ? "." : dirName;
        var outputDir = Path.GetFullPath(string.IsNullOrWhiteSpace(args.OutputDir) ? modelDir : args.OutputDir);

        if (args.PreservePath)
        {
            var preserveDir = Path.GetDirectoryName(referenceName) ?? "";

            // Remove drive letter if necessary
            if (Path.IsPathRooted(preserveDir))
                preserveDir = preserveDir[Path.GetPathRoot(preserveDir)!.Length..];

            // Prevent traversing above output directory
            outputDir = Path.Join(outputDir, FileHandlingExtensions.CombineAndNormalizePath(preserveDir));
        }

        var outputFile = Path.GetFileNameWithoutExtension(referenceName);
        if (string.IsNullOrWhiteSpace(outputFile))
            outputFile = $"CryConv_{DateTime.Now.Ticks:X}_{_counter++}";
        else
            outputFile += args.NoConflicts ? "_out" : "";

        if (!string.IsNullOrWhiteSpace(layerName))
            outputFile += "." + Regex.Replace(layerName, "[<>:\\\\\"/\\|\\?\\*]", "_");

        outputFile += extension;

        Directory.CreateDirectory(outputDir);

        return new FileInfo(Path.Combine(outputDir, outputFile));
    }

    internal static bool IsNodeNameExcluded(this Args args, string nodeName) =>
        args.ExcludeNodeNameRegexes.Any(x => x.IsMatch(nodeName));

    internal static bool IsMaterialExcluded(this Args args, Material material) =>
        (material.Name is not null && args.IsMeshMaterialExcluded(material.Name))
        || (material.Shader is not null && args.IsMeshMaterialShaderExcluded(material.Shader));

    internal static bool IsMeshMaterialExcluded(this Args args, string materialName) =>
        args.ExcludeMaterialNameRegexes.Any(x => x.IsMatch(materialName));

    internal static bool IsMeshMaterialShaderExcluded(this Args args, string shaderName) =>
        args.ExcludeShaderNameRegexes.Any(x => x.IsMatch(shaderName));
}
