using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CgfConverter
{
    public class ArgsHandler
    {
        /// <summary>
        /// File extensions processed by the handler
        /// </summary>
        private static HashSet<String> _validExtensions = new HashSet<String>
        {
            ".cgf",
            ".cga",
            ".skin",
        };

        /// <summary>
        /// List of files to process
        /// </summary>
        public List<FileInfo> InputFiles { get; private set; }
        /// <summary>
        /// Location of the Object Files
        /// </summary>
        public DirectoryInfo ObjectDir { get; private set; }
        /// <summary>
        /// File to render to
        /// </summary>
        public FileInfo OutputFile { get; private set; }
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
        /// Reverse UVs
        /// </summary>
        public Boolean FlipUVs { get; private set; }
        /// <summary>
        /// Flag used to indicate we should convert texture paths to use TIFF instead of DDS
        /// </summary>
        public Boolean TiffTextures { get; private set; }
        /// <summary>
        /// Flag used to pass exceptions to installed debuggers
        /// </summary>
        public Boolean Throw { get; private set; }


        /// <summary>
        /// Parse command line arguments
        /// </summary>
        /// <param name="inputArgs">list of arguments to parse</param>
        /// <returns>0 on success, 1 if anything went wrong</returns>
        public Int32 ProcessArgs(String[] inputArgs)
        {
            this.InputFiles = new List<FileInfo> { };

            Boolean inputFileMode = true;

            for (int i = 0; i < inputArgs.Length; i++)
            {
                #region Parse Input Files

                if (inputFileMode)
                {
                    FileInfo newFile = new FileInfo(inputArgs[i]);

                    if (newFile.Exists)
                    {
                        // Validate file extension - handles .cgam / skinm
                        if (!ArgsHandler._validExtensions.Contains(newFile.Extension))
                        {
                            Console.WriteLine("Warning: Unsupported file extension - please use a cga or cgf file");
                            return 1;
                        }

                        // Add to list of files to process
                        this.InputFiles.Add(newFile);

                        #region m-File Auto-Detection

                        FileInfo mFile = new FileInfo(Path.ChangeExtension(inputArgs[i], String.Format("{0}m", Path.GetExtension(inputArgs[i]))));

                        if (mFile.Exists)
                        {
                            Console.WriteLine("Found mFile file {0}", mFile.Name);

                            // Add to list of files to process
                            this.InputFiles.Add(mFile);
                        }

                        #endregion

                    }
                    else
                    {
                        inputFileMode = false;
                    }
                }

                #endregion

                #region Parse Other Arguments

                if (!inputFileMode)
                {
                    switch (inputArgs[i].ToLowerInvariant())
                    {
                        #region case "-objectdir"...

                        // Next item in list will be the Object directory
                        case "-objectdir":
                            if (++i > inputArgs.Length)
                            {
                                this.PrintUsage();
                                return 1;
                            }

                            this.ObjectDir = new DirectoryInfo(inputArgs[i]);

                            if (!this.ObjectDir.Exists)
                                throw new DirectoryNotFoundException("Object Directory not found");

                            Console.WriteLine("Object directory set to {0}", inputArgs[i]);

                            break;

                        #endregion
                        #region case "-outfile" / "-outputfile"...

                        // Next item in list will be the output filename
                        case "-outfile":
                        case "-outputfile":
                            if (++i > inputArgs.Length)
                            {
                                this.PrintUsage();
                                return 1;
                            }

                            this.OutputFile = new FileInfo(inputArgs[i]);

                            Console.WriteLine("Output file set to {0}", inputArgs[i]);

                            break;

                        #endregion
                        #region case "-usage"...

                        case "-usage":
                            this.PrintUsage();
                            return 1;

                        #endregion
                        #region case "-flipuv"...

                        case "-flipuv":
                            Console.WriteLine("Flipping UVs.");
                            this.FlipUVs = true;

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
                        #region case "-tif" / "-tiff""...

                        case "-tif":
                        case "-tiff":

                            this.TiffTextures = true;

                            break;

                        #endregion
                        #region case "-noconflict"...

                        case "-noconflict":

                            // Output file is based on first file name
                            if (i == 0) this.OutputFile = new FileInfo(Path.GetFileNameWithoutExtension(this.InputFiles.First().FullName) + "_out.obj");

                            break;

                        #endregion
                        #region case "-throw"...

                        case "-throw":
                            Console.WriteLine("Exceptions thrown to debugger");
                            this.Throw = true;

                            break;

                        #endregion
                    }
                }

                #endregion
            }

            // Ensure we have files to process
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
            Console.WriteLine("cgf-converter [-usage] | <.cgf file> [-outputfile <output file>] [-objectdir <ObjectDir>] [-obj] [-blend] [-dae] [-flipUVs] [-throw]");
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
            Console.WriteLine("-flipUVs:         Flip the UVs");
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
            Console.WriteLine("    Input files:            {0}", this.InputFiles.First().Name);
            for (Int32 i = 1, j = this.InputFiles.Count; i < j; i++)
            {
                Console.WriteLine("                            {0}", this.InputFiles[i].Name);
            }
            if (this.ObjectDir != null)
            {
                Console.WriteLine("    Object dir:             {0}", this.ObjectDir.FullName);
            }
            if (this.OutputFile != null)
            {
                Console.WriteLine("    Output file:            {0}", this.OutputFile.FullName);
            }
            Console.WriteLine("    Flip UVs:               {0}", this.FlipUVs);
            Console.WriteLine("    Output to .obj:         {0}", this.Output_Wavefront);
            Console.WriteLine("    Output to .blend:       {0}", this.Output_Blender);
            Console.WriteLine("    Output to .dae:         {0}", this.Output_Collada);
            Console.WriteLine();
        }
    }
}