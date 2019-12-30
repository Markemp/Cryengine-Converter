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

                        this.DataDir = new DirectoryInfo(inputArgs[i].Replace("\"", string.Empty ));

                        Console.WriteLine("Data directory set to {0}", inputArgs[i]);

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

                        Console.WriteLine("Output directory set to {0}", inputArgs[i]);

                        break;

                    #endregion
                    #region case "-usage"...

                    case "-usage":
                        PrintUsage();
                        return 1;

                    #endregion
                    #region case "-smooth"...

                    case "-smooth":
                        Console.WriteLine("Smoothing Faces");
                        this.Smooth = true;

                        break;

                    #endregion
                    #region case "-blend" / "-blender"...

                    case "-blend":
                    case "-blender":
                        Console.WriteLine("Output format set to Blender (.blend)");
                        this.OutputBlender = true;

                        break;

                    #endregion
                    #region case "-obj" / "-object" / "wavefront"...

                    case "-obj":
                    case "-object":
                    case "-wavefront":
                        Console.WriteLine("Output format set to Wavefront (.obj)");
                        this.OutputWavefront = true;

                        break;

                    #endregion
                    #region case "-fbx"
                    case "-fbx":
                        Console.WriteLine("Output format set to FBX (.fbx)");
                        this.OutputFBX = true;
                        break;
                    #endregion
                    #region case "-dae" / "-collada"...
                    case "-dae":
                    case "-collada":
                        Console.WriteLine("Output format set to COLLADA (.dae)");
                        this.OutputCollada = true;

                        break;

                    #endregion
                    #region case "-crytek"...
                    case "-cry":
                    case "-crytek":
                        Console.WriteLine("Output format set to CryTek (.cga/.cgf/.chr/.skin)");
                        this.OutputCryTek = true;

                        break;

                    #endregion
                    #region case "-tif" / "-tiff"...

                    case "-tif":
                    case "-tiff":

                        this.TiffTextures = true;

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

                        Console.WriteLine("Grouping set to {0}", this.GroupMeshes);

                        break;

                    #endregion
                    #region case "-throw"...

                    case "-throw":
                        Console.WriteLine("Exceptions thrown to debugger");
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

                        Console.WriteLine("Input file set to {0}", inputArgs[i]);

                        break;

                    #endregion
                    #region case "-allowconflict"...
                    case "-allowconflicts":
                    case "-allowconflict":
                        AllowConflicts = true;
                        Console.WriteLine("Allow conflicts for mtl files enabled");
                        break;
                    #endregion
                    #region case "-noconflict"...
                    case "-noconflict":
                    case "-noconflicts":
                        NoConflicts = true;
                        Console.WriteLine("Prevent conflicts for mtl files enabled");
                        break;
                    #endregion


                    #region default...

                    default:
                        this.InputFiles.AddRange(GetFiles(inputArgs[i]));

                        Console.WriteLine("Input file set to {0}", inputArgs[i]);

                        break;

                    #endregion
                }
            }

            // Ensure we have a file to process
            if (this.InputFiles.Count == 0)
            {
                PrintUsage();
                return 1;
            }

            // Default to Collada (.dae) format
            if (!this.OutputBlender && !this.OutputCollada && !this.OutputWavefront && !this.OutputFBX)
                this.OutputCollada = true;

            return 0;
        }

        public static void PrintUsage()
        {
            Console.WriteLine();
            Console.WriteLine("cgf-converter [-usage] | <.cgf file> [-outputfile <output file>] [-objectdir <ObjectDir>] [-obj] [-blend] [-dae] [-smooth] [-throw]");
            Console.WriteLine();
            Console.WriteLine("-usage:           Prints out the usage statement");
            Console.WriteLine();
            Console.WriteLine("<.cgf file>:      The name of the .cgf, .cga or .skin file to process.");
            Console.WriteLine("-outputfile:      The name of the file to write the output.  Default is [root].dae");
            Console.WriteLine("-noconflict:      Use non-conflicting naming scheme (<cgf File>_out.obj)");
            Console.WriteLine("-allowconflict:   Allows conflicts in .mtl file name. (obj exports only, as not an issue in dae.)");
            Console.WriteLine("-objectdir:       The name where the base Objects directory is located.  Used to read mtl file");
            Console.WriteLine("                  Defaults to current directory.");
            Console.WriteLine("-dae:             Export Collada format files (Default)"); 
            Console.WriteLine("-blend:           Export Blender format files (Not Implemented)");
            Console.WriteLine("-fbx:             Export FBX format files (Not Implemented)");
            Console.WriteLine("-smooth:          Smooth Faces");
            Console.WriteLine("-group:           Group meshes into single model");
            Console.WriteLine();
            Console.WriteLine("-throw:           Throw Exceptions to installed debugger");
            Console.WriteLine();
        }

        public override string ToString()
        {
            return $@"Input file: {InputFiles}, Obj Dir: {DataDir}, Output file: {OutputFile}";
        }
    }
}