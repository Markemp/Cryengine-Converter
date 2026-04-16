using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using CgfConverter.PackFileSystem;
using CgfConverter.Utilities;

namespace CgfConverter;

public sealed class ArgsHandler
{
    public Args Args { get; } = new();

    /// <summary>
    /// Parse command line arguments
    /// </summary>
    /// <param name="inputArgs">list of arguments to parse</param>
    /// <returns>0 on success, 1 if anything went wrong</returns>
    public int ProcessArgs(string[] inputArgs)
    {
        var lookupDataDirs = new List<string>();
        var lookupInputs = new List<string>();

        for (int i = 0; i < inputArgs.Length; i++)
        {
            switch (inputArgs[i].ToLowerInvariant())
            {
                // Next item in list will be the Object directory
                case "-datadir":
                case "-objectdir":
                    if (++i > inputArgs.Length)
                    {
                        PrintUsage();
                        return 1;
                    }

                    lookupDataDirs.Add(inputArgs[i]);
                    break;
                // Next item in list will be the output directory
                case "-out":
                case "-outdir":
                case "-outputdir":
                    if (++i > inputArgs.Length)
                    {
                        PrintUsage();
                        return 1;
                    }
                    Args.OutputDir = new DirectoryInfo(inputArgs[i]).FullName;
                    break;
                case "-pp":
                case "-preservepath":
                    Args.PreservePath = true;
                    break;
                case "-mt":
                case "-maxthreads":
                    if (++i > inputArgs.Length)
                    {
                        PrintUsage();
                        return 1;
                    }

                    if (int.TryParse(inputArgs[i], out var mt) && mt >= 0)
                        Args.MaxThreads = mt;
                    else
                    {
                        Console.Error.WriteLine("Invalid number of threads {0}, defaulting to 1.", inputArgs[i]);
                        Args.MaxThreads = 1;
                    }
                    break;
                case "-sl":
                case "-splitlayer":
                case "-splitlayers":
                    Args.SplitLayers = true;
                    break;
                case "-loglevel":
                    if (++i > inputArgs.Length)
                    {
                        PrintUsage();
                        return 1;
                    }

                    if (Enum.TryParse(inputArgs[i], true, out LogLevelEnum level))
                        HelperMethods.LogLevel = level;
                    else
                    {
                        Console.Error.WriteLine("Invalid log level {0}, defaulting to warn", inputArgs[i]);
                        HelperMethods.LogLevel = LogLevelEnum.Warning;
                    }
                    break;
                case "-usage":
                    PrintUsage();
                    return 1;
                case "-smooth":
                    Args.Smooth = true;
                    break;
                case "-obj":
                case "-object":
                case "-wavefront":
                    Args.OutputWavefront = true;
                    break;
                case "-gltf":
                    Args.OutputGLTF = true;
                    break;
                case "-glb":
                    Args.OutputGLB = true;
                    break;
                case "-dae":
                case "-collada":
                    Args.OutputCollada = true;
                    break;
                case "-usd":
                case "-usda":
                    Args.OutputUSD = true;
                    break;
                case "-tif":
                case "-tiff":
                    Args.TiffTextures = true;
                    break;
                case "-png":
                    Args.PngTextures = true;
                    break;
                case "-embedtextures":
                    Args.EmbedTextures = true;
                    break;
                case "-unsplittextures":
                case "-ut":
                    Args.UnsplitTextures = true;
                    break;
                case "-tga":
                    Args.TgaTextures = true;
                    break;
                case "-notex":
                    Args.NoTextures = true;
                    break;
                case "-en":
                case "-excludenode":
                    if (++i > inputArgs.Length)
                    {
                        PrintUsage();
                        return 1;
                    }
                    Args.ExcludeNodeNames.Add(inputArgs[i]);
                    break;
                case "-em":
                case "-excludemat":
                    if (++i > inputArgs.Length)
                    {
                        PrintUsage();
                        return 1;
                    }
                    Args.ExcludeMaterialNames.Add(inputArgs[i]);
                    break;
                case "-es":
                case "-excludeshader":
                    if (++i > inputArgs.Length)
                    {
                        PrintUsage();
                        return 1;
                    }
                    Args.ExcludeShaderNames.Add(inputArgs[i]);
                    break;
                case "-group":
                    Args.GroupMeshes = true;
                    break;
                case "-mtl":
                case "-mat":
                case "-material":
                    if (++i > inputArgs.Length)
                    {
                        PrintUsage();
                        return 1;
                    }
                    Args.MaterialFile = inputArgs[i];
                    break;
                case "-infile":
                case "-inputfile":
                    if (++i > inputArgs.Length)
                    {
                        PrintUsage();
                        return 1;
                    }
                    lookupInputs.Add(inputArgs[i]);
                    break;
                case "-noconflict":
                case "-noconflicts":
                    Args.NoConflicts = true;
                    break;
                case "-anim":
                case "-animations":
                    Args.IncludeAnimations = true;
                    break;
                default:
                    lookupInputs.Add(inputArgs[i]);
                    break;
            }
        }

        // Ensure we have a file to process
        if (!lookupInputs.Any())
        {
            PrintUsage();
            return 1;
        }

        if (Args.MaxThreads == 0)
            Args.MaxThreads = Environment.ProcessorCount;

        if (Args.Smooth)
            HelperMethods.Log(LogLevelEnum.Info, "Smoothing Faces");
        if (Args.GroupMeshes)
            HelperMethods.Log(LogLevelEnum.Info, "Grouping enabled");

        if (Args.NoTextures)
            HelperMethods.Log(LogLevelEnum.Info, "Skipping texture output");
        else if (Args.PngTextures)
            HelperMethods.Log(LogLevelEnum.Info, "Using PNG textures");
        else if (Args.TiffTextures)
            HelperMethods.Log(LogLevelEnum.Info, "Using TIF textures");
        else if (Args.TgaTextures)
            HelperMethods.Log(LogLevelEnum.Info, "Using TGA textures");
        if (Args.MaterialFile is not null)
            HelperMethods.Log(LogLevelEnum.Info, $"Using material file: {Args.MaterialFile}");

        if (Args.OutputWavefront)
            HelperMethods.Log(LogLevelEnum.Info, "Output format set to Wavefront (.obj)");
        if (Args.OutputCollada)
            HelperMethods.Log(LogLevelEnum.Info, "Output format set to COLLADA (.dae)");
        if (Args.OutputGLTF)
            HelperMethods.Log(LogLevelEnum.Info, "Output format set to glTF (.gltf)");
        if (Args.OutputGLB)
            HelperMethods.Log(LogLevelEnum.Info, "Output format set to glTF Binary (.glb)");
        if (Args.OutputUSD)
            HelperMethods.Log(LogLevelEnum.Info, "Output format set to USD (.usda)");

        if (Args.NoConflicts)
            HelperMethods.Log(LogLevelEnum.Info, "Prevent conflicts for mtl files enabled");
        if (Args.ExcludeNodeNames.Any())
            HelperMethods.Log(LogLevelEnum.Info, $"Skipping nodes starting with any of these names: {String.Join(", ", Args.ExcludeNodeNames)}");
        if (Args.ExcludeMaterialNames.Any())
            HelperMethods.Log(LogLevelEnum.Info, $"Skipping meshes using materials named: {String.Join(", ", Args.ExcludeMaterialNames)}");
        if (Args.IncludeAnimations)
            HelperMethods.Log(LogLevelEnum.Info, "Animation loading enabled");

        if (Args.OutputDir != null)
            HelperMethods.Log(LogLevelEnum.Info, "Output directory set to {0}", Args.OutputDir);

        foreach (var dirAndOptions in lookupDataDirs)
        {
            var foundAny = false;

            var dirAndOptionStrings = dirAndOptions.Split("?", 2);
            var dir = dirAndOptionStrings[0].Trim();
            var packFileSystemOptions = dirAndOptionStrings.Length == 2
                ? dirAndOptionStrings[1].Split('&').Select(x => x.Split('=', 2)).ToDictionary(x => x[0].ToLowerInvariant(), x => x.Length == 2 ? x[1] : string.Empty)
                : new Dictionary<string, string>();

            if (Directory.Exists(dir))
            {
                HelperMethods.Log(LogLevelEnum.Info, "Source [Filesystem]: {0}", dir);
                Args.DataDir = dir;
                Args.PackFileSystem.Add(new RealFileSystem(dir));
                foundAny = true;
            }

            foreach (var globbed in Args.PackFileSystem.Glob(dir))
            {
                if (globbed.EndsWith(WiiuStreamPackFileSystem.PackFileNameSuffix,
                        StringComparison.InvariantCultureIgnoreCase))
                {
                    HelperMethods.Log(LogLevelEnum.Info, "Source [Packfile]: {0}", globbed);
                    Args.DataDir = globbed;
                    Args.PackFileSystem.Add(new WiiuStreamPackFileSystem(Args.PackFileSystem.GetStream(globbed), packFileSystemOptions));
                    foundAny = true;
                }
            }

            if (!foundAny)
                HelperMethods.Log(LogLevelEnum.Warning, "No corresponding source directory exist: {0}", dir);
        }

        foreach (var input in lookupInputs)
        {
            var foundAny = false;
            foreach (var globbed in Args.PackFileSystem.Glob(input))
            {
                HelperMethods.Log(LogLevelEnum.Info, "Found input: {0}", globbed);
                Args.InputFiles.Add(globbed);
                foundAny = true;
            }

            if (!foundAny)
                HelperMethods.Log(LogLevelEnum.Warning, "No corresponding input file exist: {0}", input);
        }

        Args.ExcludeNodeNameRegexes.AddRange(Args.ExcludeNodeNames.Select(x => new Regex(x, RegexOptions.Compiled | RegexOptions.IgnoreCase)));
        Args.ExcludeMaterialNameRegexes.AddRange(Args.ExcludeMaterialNames.Select(x => new Regex(x, RegexOptions.Compiled | RegexOptions.IgnoreCase)));
        Args.ExcludeShaderNameRegexes.AddRange(Args.ExcludeShaderNames.Select(x => new Regex(x, RegexOptions.Compiled | RegexOptions.IgnoreCase)));

        // Default to USD format
        if (!Args.OutputCollada && !Args.OutputWavefront && !Args.OutputGLB && !Args.OutputGLTF && !Args.OutputUSD)
            Args.OutputUSD = true;

        return 0;
    }

    public static void PrintUsage()
    {
        Console.WriteLine();
        Console.WriteLine("cgf-converter [-usage] | <.cgf file> [-dae] [-obj] [-glb] [-gltf] [-usd] [-notex/-png/-tif/-tga] [-excludenode <nodename>] [-excludemat <matname>] [-loglevel <LogLevel>] [-objectdir <ObjectDir>] [-anim]");
        Console.WriteLine();
        Console.WriteLine($"CryEngine Converter v{Assembly.GetEntryAssembly()?.GetName().Version}");
        Console.WriteLine();
        Console.WriteLine("-usage:            Prints out the usage statement");
        Console.WriteLine();
        Console.WriteLine("<.cgf file>:       The name of the .cgf, .cga, .chr, .anim, .dba or .skin file to process.");
        Console.WriteLine("-objectdir:        (Optional but highly recommended) The name where the base Objects directory is located (i.e. where the .pak files were extracted).");
        Console.WriteLine("                   Defaults to current directory. Some packfile formats may accept additional options in the form of some.pack.file?key=value&key2=value2.");
        Console.WriteLine("-mtl/mat/material:  (Optional) The material file to use.");
        Console.WriteLine();
        Console.WriteLine(" Export formats.   By default -usd is used.");
        Console.WriteLine("-usd/-usda:        Export USD format files (default).");
        Console.WriteLine("-dae:              Export Collada format files.");
        Console.WriteLine("-glb:              Export glb (glTF binary) files. Embeds textures by default so expect large files!");
        Console.WriteLine("-gltf:             Export file pairs of glTF and bin files.");
        Console.WriteLine("-obj:              Export Wavefront format files (Not supported).");
        Console.WriteLine();
        Console.WriteLine("  Texture Options.");
        Console.WriteLine("-notex:            Do not include textures in outputs.");
        Console.WriteLine("-ut/-unsplittextures:");
        Console.WriteLine("                   Use DDS Unsplitter to combine split DDS texture files into a single file.");
        Console.WriteLine();
        Console.WriteLine("-en/-excludenode   <regular expression for node names>:");
        Console.WriteLine("                   Exclude matching nodes from rendering. Can be listed multiple times.");
        Console.WriteLine("-em/-excludemat    <regular expression for material names>");
        Console.WriteLine("                   Exclude meshes with matching materials from rendering. Can be listed multiple times.");
        Console.WriteLine("-sm/-excludeshader  <material_name>:");
        Console.WriteLine("                   Exclude meshes with the material using matching shader from rendering. Can be listed multiple times.");
        Console.WriteLine("-noconflict:       Append _out to output filename to avoid naming conflicts.");
        Console.WriteLine();
        Console.WriteLine("-pp/-preservepath:");
        Console.WriteLine("                   Preserve the path hierarchy.");
        Console.WriteLine("-mt/-maxthreads <number>");
        Console.WriteLine("                   Set maximum number of threads to use. Specify 0 to use all cores.");
        Console.WriteLine();
        Console.WriteLine("  glTF/GLB Options.");
        Console.WriteLine("-tif:              Reference .tif files instead of converting to PNG (glTF text mode only).");
        Console.WriteLine("-png:              Reference .png files instead of converting to PNG (glTF text mode only).");
        Console.WriteLine("-tga:              Reference .tga files instead of converting to PNG (glTF text mode only).");
        Console.WriteLine("-embedtextures:    Embed textures into the glTF text output instead of external references.");
        Console.WriteLine("                   GLB output always embeds textures as PNG regardless of these flags.");
        Console.WriteLine("-sl/-splitlayer(s)");
        Console.WriteLine("                   Split into multiple layers (terrain only).");
        Console.WriteLine();
        Console.WriteLine("  Wavefront Options (obj export only, not supported).");
        Console.WriteLine("-smooth:           Smooth faces.");
        Console.WriteLine("-group:            Group meshes into single model.");
        Console.WriteLine();
        Console.WriteLine("  Animation Options.");
        Console.WriteLine("-anim/-animations: Include animations in the conversion output.");
        Console.WriteLine("                   Loads .chrparams, .dba, .caf, and .cal animation files.");
        Console.WriteLine();
        Console.WriteLine("-loglevel:         Set the output log level (verbose, debug, info, warn, error, critical, none)");
        Console.WriteLine();
    }
}
