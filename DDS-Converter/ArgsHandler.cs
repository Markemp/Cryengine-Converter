using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DDS_Converter
{
    public class ArgsHandler
    {
        /// <summary>
        /// Files to process
        /// </summary>
        public List<string> InputFiles { get; internal set; }
        /// <summary>
        /// Location of the Object Files
        /// </summary>
        public DirectoryInfo DataDir { get; internal set; }

        /// <summary>
        /// Convert the dds files to TIF
        /// </summary>
        public bool TiffTextures { get; internal set; }

        /// <summary>
        /// Flag used to pass exceptions to installed debuggers
        /// </summary>
        public Boolean Throw { get; internal set; }

        public bool Verbose { get; set; }



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
        private string[] GetFiles(String filter)
        {
            if (File.Exists(filter))
                return new string[] { new FileInfo(filter).FullName };

            String directory = Path.GetDirectoryName(filter);
            if (String.IsNullOrWhiteSpace(directory))
                directory = ".";

            String fileName = Path.GetFileName(filter);
            String extension = Path.GetExtension(filter);

            Boolean flexibleExtension = extension.Contains('*');

            return Directory.GetFiles(directory, fileName, fileName.Contains('?') || fileName.Contains('*') ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                .Where(f => flexibleExtension || Path.GetExtension(f).Length == extension.Length)
                .ToArray();
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
                    #region case "-basedir"

                    // Next item in list will be the Object directory
                    case "-basedir":
                        if (++i > inputArgs.Length)
                        {
                            this.PrintUsage();
                            return 1;
                        }

                        this.DataDir = new DirectoryInfo(inputArgs[i].Replace("\"", string.Empty));

                        Console.WriteLine("Base directory set to {0}", inputArgs[i]);

                        break;

                    #endregion
                    #region case "-usage"...

                    case "-usage":
                        this.PrintUsage();
                        return 1;

                    #endregion

                    #region case "-tif" / "-tiff"...
                    case "-tif":
                    case "-tiff":

                        this.TiffTextures = true;

                        break;

                    #endregion

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

                        this.InputFiles.AddRange(this.GetFiles(inputArgs[i]));

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

            }

            // Ensure we have a file to process
            if (this.InputFiles.Count == 0)
            {
                this.PrintUsage();
                return 1;
            }

            // Default to TIF files 
            this.TiffTextures = true;

            return 0;
        }

        /// <summary>
        /// Print the usage syntax of the executable
        /// </summary>
        public void PrintUsage()
        {
            Console.WriteLine();
            Console.WriteLine("dds-converter [-usage] |[-basedir <BaseDir>] [-tif] [-dds]");
            Console.WriteLine();
            Console.WriteLine("-usage:           Prints out the usage statement");
            Console.WriteLine();
            Console.WriteLine("-basedir:         The root of the game files, where all the dds files are found.");
            Console.WriteLine("                  Defaults to current directory.");
            Console.WriteLine("-tif:             Export the DDS file to TIF");
            Console.WriteLine("-dds:             For Star Citizen.  Converts their new DDS format to a combined format.");
            Console.WriteLine("                  WARNING:  This overwrites the .dds files the program finds!");
            Console.WriteLine();
        }

    }


}
