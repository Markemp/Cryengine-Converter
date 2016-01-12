using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Math;
using System.Xml;
using System.Xml.Schema;

namespace CgfConverter
{
    public class Program
    {
        public static Int32 Main(String[] args)
        {
#if DEV_DOLKENSP

            // var ships1 = Directory.GetFiles(@"O:\Mods\SC\2.1d\Objects\Spaceships\Ships", "*.cga", SearchOption.AllDirectories);
            // var ships2 = Directory.GetFiles(@"O:\Mods\SC\2.1d\Objects\Spaceships\Ships", "*.cgf", SearchOption.AllDirectories);
            // var ships3 = Directory.GetFiles(@"O:\Mods\SC\2.1d\Objects\Spaceships\Ships", "*.cgf", SearchOption.AllDirectories);
            // var ships4 = Directory.GetFiles(@"O:\Mods\SC\2.1d\Objects\Vehicles\ships", "*.cga", SearchOption.AllDirectories);
            // var ships5 = Directory.GetFiles(@"O:\Mods\SC\2.1d\Objects\Vehicles\ships", "*.cgf", SearchOption.AllDirectories);
            // var ships6 = Directory.GetFiles(@"O:\Mods\SC\2.1d\Objects\Vehicles\ships", "*.cgf", SearchOption.AllDirectories);
            // var ships7 = Directory.GetFiles(@"Objects", "*.cga", SearchOption.AllDirectories);
            // var ships8 = Directory.GetFiles(@"Objects", "*.cgf", SearchOption.AllDirectories);
            // var ships9 = Directory.GetFiles(@"Objects", "*.cgf", SearchOption.AllDirectories);

            args = new String[] { @"Objects\3.4\*.chr", "-objectdir", @"O:\Mods\SC\2.1d", "-tif", "-merge", "-obj", "-outdir", "export", "-group" };
            args = new String[] { @"Objects\3.4\*.chr", "-objectdir", @"O:\Mods\SC\2.1d", "-tif", "-merge", "-dae", "-outdir", "export", "-group" };

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