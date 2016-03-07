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
            Utils.LogLevel = LogLevelEnum.Warning;
            Utils.DebugLevel = LogLevelEnum.Debug;

            String oldTitle = Console.Title;

#if DEV_DOLKENSP
            Utils.LogLevel = LogLevelEnum.None;      // Display NO error logs
            Utils.DebugLevel = LogLevelEnum.Debug;

            args = new String[] { @"O:\Mods\Models\*.cg?", @"O:\Mods\Models\*.skin", @"O:\Mods\Models\*.chr", "-objectdir", @"O:\Mods\SC\Latest", "-tif", "-merge", "-obj", "-outdir", @"O:\Mods\Models\Export" };
            args = new String[] { @"O:\Mods\SC\Latest\*.cg?", @"O:\Mods\SC\Latest\*.skin", @"O:\Mods\SC\Latest\*.chr", "-objectdir", @"O:\Mods\SC\Latest", "-tif", "-merge", "-obj", "-outdir", @"O:\Mods\Assets_Out" };
            args = new String[] { @"Objects\*.cg?", @"Objects\*.skin", @"Objects\*.chr", "-objectdir", @"O:\Mods\SC\Latest", "-tif", "-merge", "-obj", "-outdir", @"Export" };
            // args = new String[] { @"O:\Mods\Assets\*.cg?", "-objectdir", @"O:\Mods\SC\Latest", "-tif", "-merge", "-dae", "-outdir", @"O:\Mods\Assets_Out" };
            args = new String[] { @"Objects\*.cg?", @"Objects\*.skin", @"Objects\*.chr", "-objectdir", @"O:\Mods\SC\Latest", "-tif", "-merge", "-obj", "-cry", "-dae", "-outdir", @"Export", "-skipshield", "-skipproxy" };

#endif

#if DEV_MARKEMP
            Utils.LogLevel = LogLevelEnum.Verbose; // Display ALL error logs in the console
            Utils.DebugLevel = LogLevelEnum.Debug;  // Send all to the IDE Output window

            //args = new String[] { @"C:\Users\Geoff\Documents\Visual Studio 2013\Projects\cgf-converter\cgf-converter\bin\Debug\AEGS_Gladius.cga", "-objectdir", @"e:\blender projects\star citizen", "-dds", "-obj" };
            //args = new String[] { @"E:\Blender Projects\Star Citizen\Objects\Spaceships\Ships\ORIG\300I\ORIG_300I.cga", "-objectdir", @"e:\blender projects\star citizen", "-dds", "-obj" };
            //args = new String[] { @"E:\Blender Projects\Star Citizen\Objects\Spaceships\Ships\AEGS\Gladius\AEGS_Gladius.cga", "-objectdir", @"e:\blender projects\star citizen", "-dds", "-obj" , "-merge"};
            //args = new String[] { @"C:\Users\Geoff\Documents\Visual Studio 2013\Projects\cgf-converter\cgf-converter\bin\Debug\RSI_Aurora.cga", "-objectdir", @"e:\blender projects\star citizen", "-dds", "-obj" };
            //args = new String[] { @"C:\Users\Geoff\Documents\Visual Studio 2013\Projects\cgf-converter\cgf-converter\bin\Debug\hulagirl_a.cga", "-objectdir", @"e:\blender projects\mechs", "-dds", "-dae" };
            //args = new String[] { @"C:\Users\Geoff\Documents\Visual Studio 2013\Projects\cgf-converter\cgf-converter\bin\Debug\atlas_leg_left.cga", "-objectdir", @"e:\blender projects\mechs", "-dds", "-dae" };
            args = new String[] { @"C:\Users\Geoff\Documents\Visual Studio 2013\Projects\cgf-converter\cgf-converter\bin\Debug\adder_a_cockpit_standard.cga", "-dds", "-dae" };
            //args = new String[] { @"C:\Users\Geoff\Documents\Visual Studio 2013\Projects\cgf-converter\cgf-converter\bin\Debug\candycane_a.chr", "-objectdir", @"e:\blender projects\mechs", "-dds", "-dae" };
            //args = new String[] { @"C:\Users\Geoff\Documents\Visual Studio 2013\Projects\cgf-converter\cgf-converter\bin\Debug\raptor.chr", "-dds", "-dae" };
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
                            Blender blendFile = new Blender(argsHandler, cryData);

                            blendFile.Render(argsHandler.OutputDir, argsHandler.InputFiles.Count > 1);
                        }

                        if (argsHandler.Output_Wavefront == true)
                        {
                            Wavefront objFile = new Wavefront(argsHandler, cryData);

                            objFile.Render(argsHandler.OutputDir, argsHandler.InputFiles.Count > 1);
                        }

                        if (argsHandler.Output_CryTek == true)
                        {
                            CryRender cryFile = new CryRender(argsHandler, cryData);

                            cryFile.Render(argsHandler.OutputDir, argsHandler.InputFiles.Count > 1);
                        }

                        if (argsHandler.Output_Collada == true)
                        {
                            COLLADA daeFile = new COLLADA(argsHandler, cryData);

                            daeFile.Render(argsHandler.OutputDir, argsHandler.InputFiles.Count > 1);
                        }

                        #endregion
                    }
                    catch (Exception ex)
                    {
                        Utils.Log(LogLevelEnum.Critical);
                        Utils.Log(LogLevelEnum.Critical, "********************************************************************************");
                        Utils.Log(LogLevelEnum.Critical, "There was an error rendering {0}", inputFile);
                        Utils.Log(LogLevelEnum.Critical);
                        Utils.Log(LogLevelEnum.Critical, ex.Message);
                        Utils.Log(LogLevelEnum.Critical);
                        Utils.Log(LogLevelEnum.Critical, ex.StackTrace);
                        Utils.Log(LogLevelEnum.Critical, "********************************************************************************");
                        Utils.Log(LogLevelEnum.Critical);
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

            Console.Title = oldTitle;

#if (DEV_DOLKENSP || DEV_MARKEMP)
            Console.WriteLine("Done...");
            Console.ReadKey();
#endif

            return result;
        }
    }
}