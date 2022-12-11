using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CgfConverter;

public sealed class ArgsHandler
{
    public bool Verbose { get; set; }
    /// <summary>Files to process</summary>
    public List<string> InputFiles { get; internal set; }
    /// <summary>Location of the Object Files</summary>
    public DirectoryInfo DataDir { get; internal set; } = new DirectoryInfo(".");
    /// <summary>File to render to</summary>
    public string? OutputFile { get; internal set; }
    /// <summary>Directory to render to</summary>
    public string? OutputDir { get; internal set; }
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
    /// <summary>Render to Pixar's Universal Scene Description format</summary>
    public bool OutputUsd { get; internal set; }
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
    /// <summary>List of node names to skip when rendering</summary>
    public List<string> ExcludeNodeNames { get; internal set; }
    /// <summary>List of material names to skipping the rendering of a mesh that uses the specified material</summary>
    public List<string> ExcludeMaterialNames { get; internal set; }
    public bool Throw { get; internal set; }
    public bool DumpChunkInfo { get; internal set; }

    public ArgsHandler()
    {
        InputFiles = new List<string> { };
        ExcludeNodeNames = new List<string> { };
        ExcludeMaterialNames = new List<string> { };
    }

    // TODO: Make it understand /**/ format, instead of ONLY supporting FileName wildcards
    private static string[] GetFiles(string filter)
    {
        if (File.Exists(filter))
            return new string[] { new FileInfo(filter).FullName };

        string directory = Path.GetDirectoryName(filter);
        if (string.IsNullOrWhiteSpace(directory))
            directory = ".";

        string fileName = Path.GetFileName(filter);
        string extension = Path.GetExtension(filter);

        bool flexibleExtension = extension.Contains('*');

        return Directory.GetFiles(directory, fileName, fileName.Contains('?') || fileName.Contains('*') ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
            .Where(f => flexibleExtension || Path.GetExtension(f).Length == extension.Length)
            .ToArray();
    }

    /// <summary>
    /// Parse command line arguments
    /// </summary>
    /// <param name="inputArgs">list of arguments to parse</param>
    /// <returns>0 on success, 1 if anything went wrong</returns>
    public int ProcessArgs(string[] inputArgs)
    {
        for (int i = 0; i < inputArgs.Length; i++)
        {
            switch (inputArgs[i].ToLowerInvariant())
            {
                #region case "-objectdir" / "-datadir"...

                // Next item in list will be the Object directory
                case "-datadir":
                case "-objectdir":
                    if (++i > inputArgs.Length)
                    {
                        PrintUsage();
                        return 1;
                    }
                    DataDir = new DirectoryInfo(inputArgs[i].Replace("\"", string.Empty));
                    break;
                #endregion
                #region case "-out" / "-outdir" / "-outputdir"...
                // Next item in list will be the output directory
                case "-out":
                case "-outdir":
                case "-outputdir":
                    if (++i > inputArgs.Length)
                    {
                        PrintUsage();
                        return 1;
                    }
                    OutputDir = new DirectoryInfo(inputArgs[i]).FullName;
                    break;
                #endregion
                #region case "-loglevel"...
                case "-loglevel":
                    if (++i > inputArgs.Length)
                    {
                        PrintUsage();
                        return 1;
                    }

                    LogLevelEnum level; 
                    if (LogLevelEnum.TryParse(inputArgs[i], true, out level))
                    {
                        Utils.LogLevel = level;
                    }
                    else
                    {
                        Console.WriteLine("Invalid log level {0}, defaulting to warn", inputArgs[i]);
                        Utils.LogLevel = LogLevelEnum.Warning;
                    }
                    break;
                #endregion
                #region case "-usage"...
                case "-usage":
                    PrintUsage();
                    return 1;
                #endregion
                #region case "-smooth"...
                case "-smooth":
                    Smooth = true;
                    break;
                #endregion
                #region case "-obj" / "-object" / "wavefront"...

                case "-obj":
                case "-object":
                case "-wavefront":
                    OutputWavefront = true;
                    break;
                #endregion
                #region case "-dae" / "-collada"...
                case "-dae":
                case "-collada":
                    OutputCollada = true;
                    break;
                #endregion
                #region case "-tif" / "-tiff"...
                case "-tif":
                case "-tiff":
                    TiffTextures = true;
                    break;
                #endregion
                #region case "-png" ...
                case "-png":
                    PngTextures = true;
                    break;
                #endregion
                #region case "-tga" ...
                case "-tga":
                    TgaTextures = true;
                    break;
                #endregion
                #region case "-notex" ...
                case "-notex":
                    NoTextures = true;
                    break;
                #endregion
                #region case "-en" / "-excludenode"...

                case "-en":
                case "-excludenode":
                    if (++i > inputArgs.Length)
                    {
                        PrintUsage();
                        return 1;
                    }
                    ExcludeNodeNames.Add(inputArgs[i]);
                    break;

                #endregion
                #region case "-em" / "-excludemat"...

                case "-em":
                case "-excludemat":
                    if (++i > inputArgs.Length)
                    {
                        PrintUsage();
                        return 1;
                    }
                    ExcludeMaterialNames.Add(inputArgs[i]);
                    break;

                #endregion
                #region case "-group"...

                case "-group":
                    GroupMeshes = true;
                    break;

                #endregion
                #region case "-throw"...

                case "-throw":
                    Throw = true;

                    break;

                #endregion
                #region case "-infile" / "-inputfile"...

                case "-infile":
                case "-inputfile":
                    if (++i > inputArgs.Length)
                    {
                        PrintUsage();
                        return 1;
                    }
                    InputFiles.AddRange(GetFiles(inputArgs[i]));
                    break;

                #endregion
                #region case "-allowconflict"...
                case "-allowconflicts":
                case "-allowconflict":
                    AllowConflicts = true;
                    break;
                #endregion
                #region case "-noconflict"...
                case "-noconflict":
                case "-noconflicts":
                    NoConflicts = true;
                    break;
                #endregion
                #region case "-dumpchunkinfo"...
                case "-dump":
                case "-dumpchunk":
                case "-dumpchunkinfo":
                    DumpChunkInfo = true;
                    break;
                #endregion
                #region default...

                default:
                    InputFiles.AddRange(GetFiles(inputArgs[i]));
                    break;
                    #endregion
            }
        }

        // Ensure we have a file to process
        if (InputFiles.Count == 0)
        {
            PrintUsage();
            return 1;
        }
        
        // Log info now that loglevel has been set
        if (Smooth)
            Utils.Log(LogLevelEnum.Info, "Smoothing Faces");
        if (GroupMeshes)
            Utils.Log(LogLevelEnum.Info, "Grouping enabled");
        
        if (NoTextures)
            Utils.Log(LogLevelEnum.Info, "Skipping texture output");
        else if (PngTextures)
            Utils.Log(LogLevelEnum.Info, "Using PNG textures");
        else if (TiffTextures)
            Utils.Log(LogLevelEnum.Info, "Using TIF textures");
        else if (TgaTextures)
            Utils.Log(LogLevelEnum.Info, "Using TGA textures");
        
        if (OutputWavefront)
            Utils.Log(LogLevelEnum.Info, "Output format set to Wavefront (.obj)");
        if (OutputCollada)
            Utils.Log(LogLevelEnum.Info, "Output format set to COLLADA (.dae)");
        if (OutputUsd)
            Utils.Log(LogLevelEnum.Info, "Output format set to USD (.usda)");
        if (AllowConflicts)
            Utils.Log(LogLevelEnum.Info, "Allow conflicts for mtl files enabled");
        if (NoConflicts)
            Utils.Log(LogLevelEnum.Info, "Prevent conflicts for mtl files enabled");
        if (ExcludeNodeNames.Any())
            Utils.Log(LogLevelEnum.Info, $"Skipping nodes starting with any of these names: {String.Join(", ", ExcludeNodeNames)}");
        if (ExcludeMaterialNames.Any())
            Utils.Log(LogLevelEnum.Info, $"Skipping meshes using materials named: {String.Join(", ", ExcludeMaterialNames)}");
        if (DumpChunkInfo)
            Utils.Log(LogLevelEnum.Info, "Output chunk info for missing or invalid chunks.");
        if (Throw)
            Utils.Log(LogLevelEnum.Info, "Exceptions thrown to debugger");
        if (DataDir.ToString() != ".")
            Utils.Log(LogLevelEnum.Info, "Data directory set to {0}", DataDir.FullName);
        
        Utils.Log(LogLevelEnum.Info, "Processing input file(s):");
        foreach (var file in InputFiles)
        {
            Utils.Log(LogLevelEnum.Info, file);
        }
        if (OutputDir != null)
            Utils.Log(LogLevelEnum.Info, "Output directory set to {0}", OutputDir);
        
        // Default to Collada (.dae) format
        if (!OutputUsd && !OutputCollada && !OutputWavefront)
            OutputCollada = true;

        return 0;
    }

    public static void PrintUsage()
    {
        Console.WriteLine();
        Console.WriteLine("cgf-converter [-usage] | <.cgf file> [-outputfile <output file>] [-dae] [-obj] [-notex/-png/-tif/-tga] [-group] [-excludenode <nodename>] [-excludemat <matname>] [-loglevel <LogLevel>] [-throw] [-dump] [-objectdir <ObjectDir>]");
        Console.WriteLine();
        Console.WriteLine($"CryEngine Converter v{Assembly.GetExecutingAssembly().GetName().Version}");
        Console.WriteLine();
        Console.WriteLine("-usage:           Prints out the usage statement");
        Console.WriteLine();
        Console.WriteLine("<.cgf file>:      The name of the .cgf, .cga or .skin file to process.");
        Console.WriteLine("-outputfile:      The name of the file to write the output.  Default is [root].dae");
        Console.WriteLine("-objectdir:       The name where the base Objects directory is located.  Used to read mtl file.");
        Console.WriteLine("                  Defaults to current directory.");
        Console.WriteLine("-dae:             Export Collada format files (Default).");
        Console.WriteLine("-obj:             Export Wavefront format files (Not supported).");
        Console.WriteLine("-blend:           Export Blender format files (Not Implemented).");
        Console.WriteLine("-fbx:             Export FBX format files (Not Implemented).");
        Console.WriteLine();
        Console.WriteLine("-smooth:          Smooth Faces.");
        Console.WriteLine("-group:           Group meshes into single model.");
        Console.WriteLine("-en/-excludenode <nodename>:");
        Console.WriteLine("                  Exclude nodes starting with <nodename> from rendering. Can be listed multiple times.");
        Console.WriteLine("-em/-excludemat <material_name>:");
        Console.WriteLine("                  Exclude meshes with the material <material_name> from rendering. Can be listed multiple times.");
        Console.WriteLine("-noconflict:      Use non-conflicting naming scheme (<cgf File>_out.obj)");
        Console.WriteLine("-allowconflict:   Allows conflicts in .mtl file name. (obj exports only, as not an issue in dae.)");
        Console.WriteLine();
        Console.WriteLine("-prefixmatnames:  Prefixes material names with the filename of the source mtl file.");
        Console.WriteLine("-notex:           Do not include textures in outputs");
        Console.WriteLine("-tif:             Change the materials to look for .tif files instead of .dds.");
        Console.WriteLine("-png:             Change the materials to look for .png files instead of .dds.");
        Console.WriteLine("-tga:             Change the materials to look for .tga files instead of .dds.");
        Console.WriteLine();
        Console.WriteLine("-loglevel:        Set the output log level (verbose, debug, info, warn, error, critical, none)");
        Console.WriteLine("-throw:           Throw Exceptions to installed debugger.");
        Console.WriteLine("-dump:            Dump missing/bad chunk info for support.");
        Console.WriteLine();
    }

    public override string ToString() => $@"Input file: {InputFiles}, Obj Dir: {DataDir}, Output file: {OutputFile}";
}