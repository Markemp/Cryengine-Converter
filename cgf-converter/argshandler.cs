using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CgfConverter
{
    public class ArgsHandler
    {
        /// <summary>
        /// Files to process
        /// </summary>
        public List<String> InputFiles { get; private set; }
        /// <summary>
        /// Location of the Object Files
        /// </summary>
        public String DataDir { get; private set; }
        /// <summary>
        /// File to render to
        /// </summary>
        // public String OutputFile { get; private set; }
        /// <summary>
        /// Directory to render to
        /// </summary>
        public String OutputDir { get; private set; }
        /// <summary>
        /// Name to group all meshes under
        /// </summary>
        public Boolean GroupMeshes { get; private set; }
        /// <summary>
        /// Render Wavefront format files
        /// </summary>
        public Boolean Output_Wavefront { get; private set; }
        /// <summary>
        /// Render Blender format files
        /// </summary>
        public Boolean Output_Blender { get; private set; }
        /// <summary>
        /// Render COLLADA format files
        /// </summary>
        public Boolean Output_Collada { get; private set; }
        /// <summary>
        /// Smooth Faces
        /// </summary>
        public Boolean Smooth { get; private set; }
        /// <summary>
        /// Flag used to indicate we should convert texture paths to use TIFF instead of DDS
        /// </summary>
        public Boolean TiffTextures { get; private set; }
        /// <summary>
        /// Flag used to pass exceptions to installed debuggers
        /// </summary>
        public Boolean Throw { get; private set; }

        public ArgsHandler()
        {
            this.InputFiles = new List<String> { };
        }

        /// <summary>
        /// Take a string, and expand it into a list of files if it is a file filter
        /// 
        /// TODO: Make it understand /**/ format, instead of ONLY supporting FileName wildcards
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        private String[] GetFiles(String filter)
        {
            if (File.Exists(filter))
                return new String[] { new FileInfo(filter).FullName };

            String directory = Path.GetDirectoryName(filter);
            if (String.IsNullOrWhiteSpace(directory))
                directory = ".";

            String fileName = Path.GetFileName(filter);

            return Directory.GetFiles(directory, fileName, fileName.Contains('?') || fileName.Contains('*') ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        }

        /// <summary>
        /// Parse command line arguments
        /// </summary>
        /// <param name="inputArgs">list of arguments to parse</param>
        /// <returns>0 on success, 1 if anything went wrong</returns>
        public Int32 ProcessArgs(String[] inputArgs)
        {
            for (int i = 0; i < inputArgs.Length; i++)
            {
                #region Parse Arguments

                switch (inputArgs[i].ToLowerInvariant())
                {
                    #region case "-objectdir" / "-datadir"...

                    // Next item in list will be the Object directory
                    case "-datadir":
                    case "-objectdir":
                        if (++i > inputArgs.Length)
                        {
                            this.PrintUsage();
                            return 1;
                        }

                        this.DataDir = new DirectoryInfo(inputArgs[i]).FullName;

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
                            this.PrintUsage();
                            return 1;
                        }

                        this.OutputDir = new DirectoryInfo(inputArgs[i]).FullName;

                        Console.WriteLine("Output directory set to {0}", inputArgs[i]);

                        break;

                    #endregion
                    #region case "-usage"...

                    case "-usage":
                        this.PrintUsage();
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
                        this.Output_Blender = true;

                        break;

                    #endregion
                    #region case "-obj" / "-object" / "wavefront"...

                    case "-obj":
                    case "-object":
                    case "-wavefront":
                        Console.WriteLine("Output format set to Wavefront (.obj)");
                        this.Output_Wavefront = true;

                        break;

                    #endregion
                    #region case "-dae" / "-collada"...
                    case "-dae":
                    case "-collada":
                        Console.WriteLine("Output format set to COLLADA (.dae)");
                        this.Output_Collada = true;

                        break;

                    #endregion
                    #region case "-tif" / "-tiff"...

                    case "-tif":
                    case "-tiff":

                        this.TiffTextures = true;

                        break;

                    #endregion
                    #region case "-noconflict"...

                    case "-noconflict":

                        // TODO: Add support
                        // Output file is based on first file name
                        // this.OutputFile = new FileInfo(Path.GetFileNameWithoutExtension(this.InputFile) + "_out.obj").FullName;

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

                    // Next item in list will be the output filename
                    case "-infile":
                    case "-inputfile":
                        if (++i > inputArgs.Length)
                        {
                            this.PrintUsage();
                            return 1;
                        }

                        this.InputFiles.AddRange(this.GetFiles(inputArgs[0]));

                        Console.WriteLine("Input file set to {0}", inputArgs[i]);

                        break;

                    #endregion
                    #region default...

                    default:
                        this.InputFiles.AddRange(this.GetFiles(inputArgs[i]));

                        Console.WriteLine("Input file set to {0}", inputArgs[i]);

                        break;

                    #endregion
                }

                #endregion
            }

            // Ensure we have a file to process
            if (this.InputFiles.Count == 0)
            {
                this.PrintUsage();
                return 1;
            }

            // Default to Wavefront (.obj) format
            if (!this.Output_Blender && !this.Output_Collada && !this.Output_Wavefront)
                this.Output_Wavefront = true;

            return 0;
        }

        /// <summary>
        /// Print the usage syntax of the executable
        /// </summary>
        public void PrintUsage()
        {
            Console.WriteLine();
            Console.WriteLine("cgf-converter [-usage] | <.cgf file> [-outputfile <output file>] [-objectdir <ObjectDir>] [-obj] [-blend] [-dae] [-smooth] [-throw]");
            Console.WriteLine();
            Console.WriteLine("-usage:           Prints out the usage statement");
            Console.WriteLine();
            Console.WriteLine("<.cgf file>:      Mandatory.  The name of the .cgf, .cga or .skin file to process");
            Console.WriteLine("-outputfile:      The name of the file to write the output.  Default is [root].obj");
            Console.WriteLine("-noconflict:      Use non-conflicting naming scheme (<cgf File>_out.obj)");
            Console.WriteLine("-objectdir:       The name where the base Objects directory is located.  Used to read mtl file");
            Console.WriteLine("                  Defaults to current directory.");
            Console.WriteLine("-obj:             Export Wavefront format files (Default: true)");
            Console.WriteLine("-blend:           Export Blender format files (Not Implemented)");
            Console.WriteLine("-dae:             Export Collada format files (Not Implemented)");
            Console.WriteLine("-smooth:          Smooth Faces");
            Console.WriteLine("-group:           Group meshes into single model");
            Console.WriteLine();
            Console.WriteLine("-throw:           Throw Exceptions to installed debugger");
            Console.WriteLine();
        }

        /// <summary>
        /// Print the current arguments of the executable
        /// </summary>
        public void WriteArgs()
        {
            Console.WriteLine();
            Console.WriteLine("*** Submitted args ***");
            // Console.WriteLine("    Input files:            {0}", this.InputFile);
            if (!String.IsNullOrWhiteSpace(this.DataDir))
            {
                Console.WriteLine("    Object dir:             {0}", this.DataDir);
            }
            if (!String.IsNullOrWhiteSpace(this.OutputDir))
            {
                Console.WriteLine("    Output file:            {0}", this.OutputDir);
            }
            Console.WriteLine("    Smooth Faces:           {0}", this.Smooth);
            Console.WriteLine("    Output to .obj:         {0}", this.Output_Wavefront);
            Console.WriteLine("    Output to .blend:       {0}", this.Output_Blender);
            Console.WriteLine("    Output to .dae:         {0}", this.Output_Collada);
            Console.WriteLine();
        }
    }
}