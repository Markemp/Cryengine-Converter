using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CgfConverter
{
    public class ArgsHandler
    {
        public bool Verbose { get; set; }
        /// <summary>
        /// Files to process
        /// </summary>
        public List<string> InputFiles { get; internal set; }
        /// <summary>
        /// Location of the Object Files
        /// </summary>
        public DirectoryInfo DataDir { get; internal set; } = new DirectoryInfo(".");
        /// <summary>
        /// File to render to
        /// </summary>
        public string OutputFile { get; internal set; }
        /// <summary>
        /// Directory to render to
        /// </summary>
        public string OutputDir { get; internal set; }

        /// <summary>
        /// Allows naming conflicts for mtl file
        /// </summary>
        public LogLevelEnum LogLevel { get; set; } = LogLevelEnum.Critical;
        /// <summary>
        /// Sets the output log level
        /// </summary>
        public bool AllowConflicts { get; internal set; }
        /// <summary>
        /// For LODs files.  Adds _out onto the output
        /// </summary>
        public bool NoConflicts { get; internal set; }
        /// <summary>
        /// Name to group all meshes under
        /// </summary>
        public bool GroupMeshes { get; internal set; }
        /// <summary>
        /// Render CryTek format files
        /// </summary>
        public bool OutputCryTek { get; internal set; }
        /// <summary>
        /// Render Wavefront format files
        /// </summary>
        public bool OutputWavefront { get; internal set; }
        /// <summary>
        /// Render Blender format files
        /// </summary>
        public bool OutputBlender { get; internal set; }
        /// <summary>
        /// Render COLLADA format files
        /// </summary>
        public bool OutputCollada { get; internal set; }
        /// <summary>
        /// Render FBX
        /// </summary>
        public bool OutputFBX { get; internal set; }
        /// <summary>
        /// Smooth Faces
        /// </summary>
        public bool Smooth { get; internal set; }
        /// <summary>
        /// Flag used to indicate we should convert texture paths to use TIFF instead of DDS
        /// </summary>
        public bool TiffTextures { get; internal set; }
        /// <summary>
        /// Flag used to indicate we should convert texture paths to use PNG instead of DDS
        /// </summary>
        public bool PngTextures { get; internal set; }
        /// <summary>
        /// Flag used to skip the rendering of nodes containing $shield
        /// </summary>
        public bool SkipShieldNodes { get; internal set; }
        /// <summary>
        /// Flag used to skip the rendering of nodes containing $proxy
        /// </summary>
        public bool SkipProxyNodes { get; internal set; }
        /// <summary>
        /// Flag used to pass exceptions to installed debuggers
        /// </summary>
        public bool Throw { get; internal set; }
        public bool DumpChunkInfo { get; internal set; }

        public ArgsHandler()
        {
            this.InputFiles = new List<string> { };
        }

        /// <summary>
        /// Take a string, and expand it into a list of files if it is a file filter
        /// 
        /// TODO: Make it understand /**/ format, instead of ONLY supporting FileName wildcards
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
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
                        this.DataDir = new DirectoryInfo(inputArgs[i].Replace("\"", string.Empty));
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
                        this.OutputDir = new DirectoryInfo(inputArgs[i]).FullName;
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
                        this.Smooth = true;
                        break;
                    #endregion
                    #region case "-blend" / "-blender"...
                    case "-blend":
                    case "-blender":
                        this.OutputBlender = true;
                        break;
                    #endregion
                    #region case "-obj" / "-object" / "wavefront"...

                    case "-obj":
                    case "-object":
                    case "-wavefront":
                        this.OutputWavefront = true;
                        break;
                    #endregion
                    #region case "-fbx"
                    case "-fbx":
                        this.OutputFBX = true;
                        break;
                    #endregion
                    #region case "-dae" / "-collada"...
                    case "-dae":
                    case "-collada":
                        this.OutputCollada = true;
                        break;
                    #endregion
                    #region case "-crytek"...
                    case "-cry":
                    case "-crytek":
                        this.OutputCryTek = true;
                        break;
                    #endregion
                    #region case "-tif" / "-tiff"...
                    case "-tif":
                    case "-tiff":
                        this.TiffTextures = true;
                        break;
                    #endregion
                    #region case "-png" ...
                    case "-png":
                        this.PngTextures = true;
                        break;
                    #endregion
                    #region case "-skipshield" / "-skipshields"...

                    case "-skipshield":
                    case "-skipshields":

                        this.SkipShieldNodes = true;

                        break;

                    #endregion
                    #region case "-skipproxy"...

                    case "-skipproxy":

                        this.SkipProxyNodes = true;

                        break;

                    #endregion
                    #region case "-group"...

                    case "-group":
                        this.GroupMeshes = true;
                        break;

                    #endregion
                    #region case "-throw"...

                    case "-throw":
                        this.Throw = true;

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
                        this.InputFiles.AddRange(GetFiles(inputArgs[i]));
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
                        this.DumpChunkInfo = true;
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
            if (this.Smooth)
                Utils.Log(LogLevelEnum.Info, "Smoothing Faces");
            if (this.GroupMeshes)
                Utils.Log(LogLevelEnum.Info, "Grouping enabled");
            if (this.OutputBlender)
                Utils.Log(LogLevelEnum.Info, "Output format set to Blender (.blend)");
            if (this.OutputCryTek)
                Utils.Log(LogLevelEnum.Info, "Output format set to CryTek (.cga/.cgf/.chr/.skin)");
            if (this.OutputWavefront)
                Utils.Log(LogLevelEnum.Info, "Output format set to Wavefront (.obj)");
            if (this.OutputFBX)
                Utils.Log(LogLevelEnum.Info, "Output format set to FBX (.fbx)");
            if (this.OutputCollada)
                Utils.Log(LogLevelEnum.Info, "Output format set to COLLADA (.dae)");
            if (this.AllowConflicts)
                Utils.Log(LogLevelEnum.Info, "Allow conflicts for mtl files enabled");
            if (this.NoConflicts)
                Utils.Log(LogLevelEnum.Info, "Prevent conflicts for mtl files enabled");
            if (this.DumpChunkInfo)
                Utils.Log(LogLevelEnum.Info, "Output chunk info for missing or invalid chunks.");
            if (this.Throw)
                Utils.Log(LogLevelEnum.Info, "Exceptions thrown to debugger");
            if (this.DataDir.ToString() != ".")
                Utils.Log(LogLevelEnum.Info, "Data directory set to {0}", this.DataDir.FullName);
            
            Utils.Log(LogLevelEnum.Info, "Processing input file(s):");
            foreach (var file in this.InputFiles)
            {
                Utils.Log(LogLevelEnum.Info, file);
            }
            if (this.OutputDir != null)
                Utils.Log(LogLevelEnum.Info, "Output directory set to {0}", this.OutputDir);

            
            // Default to Collada (.dae) format
            if (!OutputBlender && !OutputCollada && !OutputWavefront && !OutputFBX)
                OutputCollada = true;

            return 0;
        }

        public static void PrintUsage()
        {
            Console.WriteLine();
            Console.WriteLine("cgf-converter [-usage] | <.cgf file> [-outputfile <output file>] [-obj] [-blend] [-dae] [-tif/-png] [-group] [-smooth] [-loglevel <LogLevel>] [-throw] [-dump] [-objectdir <ObjectDir>]");
            Console.WriteLine();
            Console.WriteLine("CryEngine Converter v1.3.0");
            Console.WriteLine();
            Console.WriteLine("-usage:           Prints out the usage statement");
            Console.WriteLine();
            Console.WriteLine("<.cgf file>:      The name of the .cgf, .cga or .skin file to process.");
            Console.WriteLine("-outputfile:      The name of the file to write the output.  Default is [root].dae");
            Console.WriteLine("-noconflict:      Use non-conflicting naming scheme (<cgf File>_out.obj)");
            Console.WriteLine("-allowconflict:   Allows conflicts in .mtl file name. (obj exports only, as not an issue in dae.)");
            Console.WriteLine("-objectdir:       The name where the base Objects directory is located.  Used to read mtl file.");
            Console.WriteLine("                  Defaults to current directory.");
            Console.WriteLine("-dae:             Export Collada format files (Default).");
            Console.WriteLine("-blend:           Export Blender format files (Not Implemented).");
            Console.WriteLine("-fbx:             Export FBX format files (Not Implemented).");
            Console.WriteLine("-smooth:          Smooth Faces.");
            Console.WriteLine("-group:           Group meshes into single model.");
            Console.WriteLine("-tif:             Change the materials to look for .tif files instead of .dds.");
            Console.WriteLine("-png:             Change the materials to look for .png files instead of .dds.");
            Console.WriteLine();
            Console.WriteLine("-loglevel:        Set the output log level (verbose, debug, info, warn, error, critical, none)");
            Console.WriteLine("-throw:           Throw Exceptions to installed debugger.");
            Console.WriteLine("-dump:            Dump missing/bad chunk info for support.");
            Console.WriteLine();
        }

        public override string ToString()
        {
            return $@"Input file: {InputFiles}, Obj Dir: {DataDir}, Output file: {OutputFile}";
        }
    }
}