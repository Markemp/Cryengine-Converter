using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CgfConverter.PackFileSystem;
using CgfConverter.Utilities;

namespace CgfConverter;

public sealed class Args
{
    public readonly CascadedPackFileSystem PackFileSystem = new();
    public readonly List<Regex> ExcludeNodeNameRegexes = [];
    public readonly List<Regex> ExcludeMaterialNameRegexes = [];
    public readonly List<Regex> ExcludeShaderNameRegexes = [];
    
    public bool Verbose { get; set; }
    /// <summary>Files to process</summary>
    public List<string> InputFiles { get; internal set; }
    /// <summary>Location of the Game files</summary>
    public string DataDir { get; internal set; }
    /// <summary>File to render to</summary>
    public string? OutputFile { get; internal set; }
    /// <summary>Directory to render to</summary>
    public string? OutputDir { get; internal set; }
    /// <summary>Material file override</summary>
    public string? MaterialFile { get; internal set; }
    /// <summary>Whether to preserve path, if OutputDir is set.</summary>
    public bool PreservePath { get; internal set; }
    /// <summary>Maximum number of threads to use.</summary>
    public int MaxThreads { get; internal set; }
    /// <summary>Sets the output log level</summary>
    public LogLevelEnum LogLevel { get; set; } = LogLevelEnum.Critical;
    /// <summary>Allows naming conflicts for mtl file</summary>
    public bool AllowConflicts { get; internal set; }
    /// <summary>LODs files.  Adds _out onto the output</summary>
    public bool NoConflicts { get; internal set; }
    /// <summary>Name to group all meshes under</summary>
    public bool GroupMeshes { get; internal set; }
    /// <summary>Render Wavefront format files</summary>
    public bool OutputWavefront { get; internal set; }
    /// <summary>Render Collada format files</summary>
    public bool OutputCollada { get; internal set; }
    /// <summary>Render glTF</summary>
    public bool OutputGLTF { get; internal set; }
    /// <summary>Render glTF binary (default behavior)</summary>
    public bool OutputGLB { get; internal set; }
    /// <summary>Smooth Faces</summary>
    public bool Smooth { get; internal set; }
    /// <summary>Flag used to indicate we should convert texture paths to use TIFF instead of DDS</summary>
    public bool TiffTextures { get; internal set; }
    /// <summary>Flag used to indicate we should convert texture paths to use PNG instead of DDS</summary>
    public bool PngTextures { get; internal set; }
    /// <summary>Flag used to indicate we should convert texture paths to use TGA instead of DDS</summary>
    public bool TgaTextures { get; internal set; }
    /// <summary>Flag used to indicate that textures should not be included in the output file</summary>
    public bool NoTextures { get; internal set; }
    /// <summary>For glTF exports, embed textures into the glTF file instead of external references.</summary>
    public bool EmbedTextures { get; internal set; }
    /// <summary>Split each layer into different files, if a file does contain multiple layers.</summary>
    public bool SplitLayers { get; internal set; }
    /// <summary>List of node names to skip when rendering</summary>
    public List<string> ExcludeNodeNames { get; internal set; }
    /// <summary>List of material names to skipping the rendering of a mesh that uses the specified material</summary>
    public List<string> ExcludeMaterialNames { get; internal set; }
    /// <summary>List of shader names to skip when rendering</summary>
    public List<string> ExcludeShaderNames { get; internal set; }
    public bool Throw { get; internal set; }
    public bool DumpChunkInfo { get; internal set; }

    public Args()
    {
        InputFiles = [];
        ExcludeNodeNames = [];
        ExcludeMaterialNames = [];
        ExcludeShaderNames = [];
    }

    public override string ToString() => $@"Input file: {InputFiles}, Object Dir: {string.Join(',', DataDir)}, Output file: {OutputFile}";
}
