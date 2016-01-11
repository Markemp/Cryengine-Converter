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
            args = new String[] { @"O:\Mods\SC\2.1d\Objects\Vehicles\ships\drak\herald\DRAK_Herald_Hangar.cga", "-objectdir", @"O:\Mods\SC\2.1d", "-tif", "-merge", "-obj", "-outputfile", "DRAK_Herald_Hangar.obj" };
            // args = new String[] { @"Objects\1.3\ORIG_300I.cga", "-objectdir", @"O:\Mods\SC\2.1d", "-tif", "-merge", "-obj", "-outputfile", "ORIG_300I.obj" };
            // args = new String[] { @"O:\Mods\SC\2.1d\Objects\brush\planet\uee\flair_objects\model_spaceships\origin_350r\flair_origin_350r.cgf", "-objectdir", @"O:\Mods\SC\2.1d", "-tif", "-merge", "-obj", "-outputfile", "ORIG_350r_flair.obj" };
            // args = new String[] { @"O:\Mods\SC\2.1d\Objects\Spaceships\Ships\AEGS\Gladius\AEGS_Gladius.cga", "-objectdir", @"O:\Mods\SC\2.1d", "-tif", "-merge", "-obj", "-outputfile", "AEGS_Gladius.obj" };
            // args = new String[] { @"O:\Mods\SC\2.1d\Objects\Spaceships\Ships\VNCL\Glaive\VNCL_Glaive_flightReady.cga", "-objectdir", @"O:\Mods\SC\2.0", "-tif", "-obj", "-outputfile", "VNCL_Glaive_flightReady.obj" };
            // args = new String[] { @"D:\Workspaces\github\Cryengine-Converter\cgf-converter\bin\dev_dolkensp\Objects\2.1\RSI_Aurora.cga", "-objectdir", @"O:\Mods\SC\2.0", "-tif", "-obj", "-outputfile", "RSI_Aurora.obj" };
#endif

#if DEV_MARKEMP
            //args = new String[] { @"C:\Users\Geoff\Documents\Visual Studio 2013\Projects\cgf-converter\cgf-converter\bin\Debug\AEGS_Gladius.cga", "-objectdir", @"e:\blender projects\star citizen", "-dds", "-obj" };
            args = new String[] { @"e:\blender projects\star citizen\objects\ships\spaceships\aegs\AEGS_Gladius.cga", "-objectdir", @"e:\blender projects\star citizen", "-dds", "-obj" };
            //args = new String[] { @"C:\Users\Geoff\Documents\Visual Studio 2013\Projects\cgf-converter\cgf-converter\bin\Debug\RSI_Aurora.cga", "-objectdir", @"e:\blender projects\star citizen", "-dds", "-obj" };
            //args = new String[] { @"C:\Users\Geoff\Documents\Visual Studio 2013\Projects\cgf-converter\cgf-converter\bin\Debug\adder_a_cockpit_standard", "-objectdir", @"e:\blender projects\mechs", "-dds", "-obj" };

#endif
            ArgsHandler argsHandler = new ArgsHandler();
            Int32 result = argsHandler.ProcessArgs(args);

#if !DEBUG
            try
            {
#endif

            if (result == 0)
            {
                // Read CryEngine Files
                CryEngine cryData = new CryEngine(argsHandler.InputFile, argsHandler.DataDir);

                #region Render Output Files

                if (argsHandler.Output_Blender == true)
                {
                    Blender blendFile = new Blender(argsHandler);

                    blendFile.WriteBlend(cryData);
                }

                if (argsHandler.Output_Wavefront == true)
                {
                    Wavefront objFile = new Wavefront(argsHandler);

                    objFile.WriteObjFile(cryData);
                }

                if (argsHandler.Output_Collada == true)
                {
                    COLLADA daeFile = new COLLADA(argsHandler);

                    daeFile.WriteCollada(cryData);
                }

                #endregion
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