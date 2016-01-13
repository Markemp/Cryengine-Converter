using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Math;
using System.Xml;
using System.Xml.Schema;
using System.Diagnostics;

namespace CgfConverter
{
    public class Program
    {
        public static Int32 Main(String[] args)
        {
#if DEV_DOLKENSP

            args = new String[] { @"O:\Mods\Models\*.cg?", @"O:\Mods\Models\*.skin", @"O:\Mods\Models\*.chr", "-objectdir", @"O:\Mods\SC\Latest", "-tif", "-merge", "-obj", "-outdir", @"O:\Mods\Models\Export" };
            args = new String[] { @"O:\Mods\SC\Latest\*.cg?", @"O:\Mods\SC\Latest\*.skin", @"O:\Mods\SC\Latest\*.chr", "-objectdir", @"O:\Mods\SC\Latest", "-tif", "-merge", "-obj", "-outdir", @"O:\Mods\Models\Export" };

#endif

#if DEV_MARKEMP
            //args = new String[] { @"C:\Users\Geoff\Documents\Visual Studio 2013\Projects\cgf-converter\cgf-converter\bin\Debug\AEGS_Gladius.cga", "-objectdir", @"e:\blender projects\star citizen", "-dds", "-obj" };
            //args = new String[] { @"E:\Blender Projects\Star Citizen\Objects\Spaceships\Ships\AEGS\Gladius\AEGS_Gladius.cga", "-objectdir", @"e:\blender projects\star citizen", "-dds", "-obj" , "-merge"};
            //args = new String[] { @"C:\Users\Geoff\Documents\Visual Studio 2013\Projects\cgf-converter\cgf-converter\bin\Debug\RSI_Aurora.cga", "-objectdir", @"e:\blender projects\star citizen", "-dds", "-obj" };
            //args = new String[] { @"C:\Users\Geoff\Documents\Visual Studio 2013\Projects\cgf-converter\cgf-converter\bin\Debug\adder_a_cockpit_standard", "-objectdir", @"e:\blender projects\mechs", "-dds", "-obj" };
            //args = new String[] { @"C:\Users\Geoff\Documents\Visual Studio 2013\Projects\cgf-converter\cgf-converter\bin\Debug\candycane_a.chr", "-objectdir", @"e:\blender projects\mechs", "-dds", "-dae" };
            args = new String[] { @"C:\Users\Geoff\Documents\Visual Studio 2013\Projects\cgf-converter\cgf-converter\bin\Debug\raptor.chr", "-dds", "-dae" };
            //args = new String[] { @"C:\Users\Geoff\Documents\Visual Studio 2013\Projects\cgf-converter\cgf-converter\bin\Debug\ar03.chr", "-dds", "-obj" };
#endif

            ArgsHandler argsHandler = new ArgsHandler();
            Int32 result = argsHandler.ProcessArgs(args);

#if !DEBUG
            try
            {
#endif

            if (result == 0)
            {
                foreach (String inputFile in argsHandler.InputFiles)
                {
                    try
                    {
                        // Read CryEngine Files
                        CryEngine cryData = new CryEngine(inputFile, argsHandler.DataDir);

                        #region Render Output Files

                        if (argsHandler.Output_Blender == true)
                        {
                            Blender blendFile = new Blender(argsHandler);

                            blendFile.WriteBlend(cryData);
                        }

                        if (argsHandler.Output_Wavefront == true)
                        {
                            Wavefront objFile = new Wavefront(argsHandler, cryData);

                            objFile.WriteObjFile(argsHandler.OutputDir, argsHandler.InputFiles.Count > 1);
                        }

                        if (argsHandler.Output_Collada == true)
                        {
                            COLLADA daeFile = new COLLADA(argsHandler);

                            daeFile.WriteCollada(cryData);
                        }

                        #endregion
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine();
                        Console.WriteLine("********************************************************************************");
                        Console.WriteLine("There was an error rendering {0}", inputFile);
                        Console.WriteLine();
                        Console.WriteLine(ex.Message);
                        Console.WriteLine();
                        Console.WriteLine(ex.StackTrace);
                        Console.WriteLine("********************************************************************************");
                        Console.WriteLine();
                    }
                }
            }

#if !DEBUG
            }
            catch (Exception)
            {
                if (argsHandler.Throw)
                    throw;
            }
#endif

#if (DEV_DOLKENSP || DEV_MARKEMP)
            Console.WriteLine("Done...");
            Console.ReadKey();
#endif

            return result;
        }
    }
}