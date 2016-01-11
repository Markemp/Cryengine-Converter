﻿using System;
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
            args = new String[] { @"O:\Mods\SC\2.1d\Objects\Vehicles\ships\drak\herald\DRAK_Herald_Hangar.cga", "-objectdir", @"O:\Mods\SC\2.1d", "-tif", "-merge", "-obj", "-outputfile", "DRAK_Herald_Hangar.obj", "-group" };
            
            args = new String[] { @"O:\Mods\SC\2.1d\Objects\Spaceships\Ships\RSI\Constellation\RSI_Constellation.cga", "-objectdir", @"O:\Mods\SC\2.1d", "-tif", "-merge", "-obj", "-outputfile", "RSI_Constellation.obj", "-group" };
            args = new String[] { @"O:\Mods\SC\2.1d\Objects\Spaceships\Ships\RSI\Constellation\RSI_Constellation_Exterior.cga", "-objectdir", @"O:\Mods\SC\2.1d", "-tif", "-merge", "-obj", "-outputfile", "RSI_Constellation_Exterior.obj", "-group" };
            args = new String[] { @"O:\Mods\SC\2.1d\Objects\Spaceships\Ships\RSI\Constellation\RSI_Constellation_Aquarius.cga", "-objectdir", @"O:\Mods\SC\2.1d", "-tif", "-merge", "-obj", "-outputfile", "RSI_Constellation_Aquarius.obj", "-group" };
            // args = new String[] { @"O:\Mods\SC\2.1d\Objects\Spaceships\Ships\RSI\Constellation\RSI_Constellation_Cygnus.cga", "-objectdir", @"O:\Mods\SC\2.1d", "-tif", "-merge", "-obj", "-outputfile", "RSI_Constellation_Cygnus.obj", "-group" };

            args = new String[] { @"O:\Mods\SC\2.1d\Objects\Spaceships\Ships\MISC\Freelancer\MISC_Freelancer.cga", "-objectdir", @"O:\Mods\SC\2.1d", "-tif", "-merge", "-obj", "-outputfile", "MISC_Freelancer.obj", "-group" };
            args = new String[] { @"O:\Mods\SC\2.1d\Objects\Spaceships\Ships\MISC\Freelancer\MISC_Freelancer_DUR.cga", "-objectdir", @"O:\Mods\SC\2.1d", "-tif", "-merge", "-obj", "-outputfile", "MISC_Freelancer_DUR.obj", "-group" };
            args = new String[] { @"O:\Mods\SC\2.1d\Objects\Spaceships\Ships\MISC\Freelancer\MISC_Freelancer_MAX.cga", "-objectdir", @"O:\Mods\SC\2.1d", "-tif", "-merge", "-obj", "-outputfile", "MISC_Freelancer_MAX.obj", "-group" };
            // args = new String[] { @"O:\Mods\SC\2.1d\Objects\Spaceships\Ships\MISC\Freelancer\MISC_Freelancer_MIS.cga", "-objectdir", @"O:\Mods\SC\2.1d", "-tif", "-merge", "-obj", "-outputfile", "MISC_Freelancer_MIS.obj", "-group" };

            args = new String[] { @"O:\Mods\SC\2.1d\Objects\Spaceships\Ships\ORIG\M50\ORIG_M50.cga", "-objectdir", @"O:\Mods\SC\2.1d", "-tif", "-merge", "-obj", "-outputfile", "ORIG_M50.obj", "-group" };

            args = new String[] { @"O:\Mods\SC\2.1d\Objects\Spaceships\Ships\ANVL\Hornet\ANVL_Hornet.cga", "-objectdir", @"O:\Mods\SC\2.1d", "-tif", "-merge", "-obj", "-outputfile", "ANVL_Hornet.obj", "-group" };
            args = new String[] { @"O:\Mods\SC\2.1d\Objects\Spaceships\Ships\ANVL\Hornet\F7C_M\ANVL_Hornet_F7C_M.cga", "-objectdir", @"O:\Mods\SC\2.1d", "-tif", "-merge", "-obj", "-outputfile", "ANVL_Hornet_F7C_M.obj", "-group" };
            // args = new String[] { @"O:\Mods\SC\2.1d\Objects\Spaceships\Ships\ANVL\Hornet\MISC_Freelancer_MIS.cga", "-objectdir", @"O:\Mods\SC\2.1d", "-tif", "-merge", "-obj", "-outputfile", "MISC_Freelancer_MIS.obj", "-group" };
            // args = new String[] { @"O:\Mods\SC\2.1d\Objects\Spaceships\Ships\MISC\Freelancer\MISC_Freelancer_MIS.cga", "-objectdir", @"O:\Mods\SC\2.1d", "-tif", "-merge", "-obj", "-outputfile", "MISC_Freelancer_MIS.obj", "-group" };
            // args = new String[] { @"O:\Mods\SC\2.1d\Objects\Spaceships\Ships\MISC\Freelancer\MISC_Freelancer_MIS.cga", "-objectdir", @"O:\Mods\SC\2.1d", "-tif", "-merge", "-obj", "-outputfile", "MISC_Freelancer_MIS.obj", "-group" };

            // args = new String[] { @"Objects\1.3\ORIG_300I.cga", "-objectdir", @"O:\Mods\SC\2.1d", "-tif", "-merge", "-obj", "-outputfile", "ORIG_300I.obj", "-group" };
            // args = new String[] { @"O:\Mods\SC\2.1d\Objects\brush\planet\uee\flair_objects\model_spaceships\origin_350r\flair_origin_350r.cgf", "-objectdir", @"O:\Mods\SC\2.1d", "-tif", "-merge", "-obj", "-outputfile", "ORIG_350r_flair.obj", "-group" };
            // args = new String[] { @"O:\Mods\SC\2.1d\Objects\Spaceships\Ships\AEGS\Gladius\AEGS_Gladius.cga", "-objectdir", @"O:\Mods\SC\2.1d", "-tif", "-merge", "-obj", "-outputfile", "AEGS_Gladius.obj", "-group" };
            // args = new String[] { @"O:\Mods\SC\2.1d\Objects\Spaceships\Ships\VNCL\Glaive\VNCL_Glaive_flightReady.cga", "-objectdir", @"O:\Mods\SC\2.0", "-tif", "-obj", "-outputfile", "VNCL_Glaive_flightReady.obj", "-group" };
            // args = new String[] { @"D:\Workspaces\github\Cryengine-Converter\cgf-converter\bin\dev_dolkensp\Objects\2.1\RSI_Aurora.cga", "-objectdir", @"O:\Mods\SC\2.0", "-tif", "-obj", "-outputfile", "RSI_Aurora.obj", "-group" };

            args = new String[] { @"C:\Projects\Cryengine-Converter\cgf-converter\bin\dev_dolkensp\Objects\2.1\DRAK_Herald_Hangar.cga", "-objectdir", @"O:\Mods\SC\2.1d", "-tif", "-merge", "-obj", "-outputfile", "DRAK_Herald_Hangar_2_1.obj", "-group" };
            // args = new String[] { @"C:\Projects\Cryengine-Converter\cgf-converter\bin\dev_dolkensp\Objects\1.3\AEGS_Gladius.cga", "-objectdir", @"O:\Mods\SC\2.1d", "-tif", "-merge", "-obj", "-outputfile", "AEGS_Gladius_1_3.obj", "-group" };
            // args = new String[] { @"C:\Projects\Cryengine-Converter\cgf-converter\bin\dev_dolkensp\Objects\1.3\ORIG_300I.cga", "-objectdir", @"O:\Mods\SC\2.1d", "-tif", "-merge", "-obj", "-outputfile", "ORIG_300I_1_3.obj", "-group" };
            // args = new String[] { @"C:\Projects\Cryengine-Converter\cgf-converter\bin\dev_dolkensp\Objects\bush_small_leaf.cgf", "-objectdir", @"O:\Mods\SC\2.1d", "-tif", "-merge", "-obj", "-outputfile", "bush_small_leaf.obj", "-group" };
            // args = new String[] { @"C:\Projects\Cryengine-Converter\cgf-converter\bin\dev_dolkensp\Objects\abrams\m1a1_abrams.cga", "-objectdir", @"O:\Mods\SC\2.1d", "-tif", "-merge", "-obj", "-outputfile", "m1a1_abrams.obj", "-group" };
            // args = new String[] { @"C:\Projects\Cryengine-Converter\cgf-converter\bin\dev_dolkensp\Objects\abrams_old\abrams.cga", "-objectdir", @"O:\Mods\SC\2.1d", "-tif", "-merge", "-obj", "-outputfile", "m1a1_abrams_old.obj", "-group" };
            // args = new String[] { @"C:\Projects\Cryengine-Converter\cgf-converter\bin\dev_dolkensp\Objects\raptor_3_4_3\raptor.chr", "-objectdir", @"O:\Mods\SC\2.1d", "-tif", "-merge", "-obj", "-outputfile", "raptor.obj", "-group" };
#endif

#if DEV_MARKEMP
            //args = new String[] { @"C:\Users\Geoff\Documents\Visual Studio 2013\Projects\cgf-converter\cgf-converter\bin\Debug\AEGS_Gladius.cga", "-objectdir", @"e:\blender projects\star citizen", "-dds", "-obj" };
            //args = new String[] { @"E:\Blender Projects\Star Citizen\Objects\Spaceships\Ships\AEGS\Gladius\AEGS_Gladius.cga", "-objectdir", @"e:\blender projects\star citizen", "-dds", "-obj" , "-merge"};
            //args = new String[] { @"C:\Users\Geoff\Documents\Visual Studio 2013\Projects\cgf-converter\cgf-converter\bin\Debug\RSI_Aurora.cga", "-objectdir", @"e:\blender projects\star citizen", "-dds", "-obj" };
            //args = new String[] { @"C:\Users\Geoff\Documents\Visual Studio 2013\Projects\cgf-converter\cgf-converter\bin\Debug\adder_a_cockpit_standard", "-objectdir", @"e:\blender projects\mechs", "-dds", "-obj" };
            args = new String[] { @"C:\Users\Geoff\Documents\Visual Studio 2013\Projects\cgf-converter\cgf-converter\bin\Debug\candycane_a.chr", "-objectdir", @"e:\blender projects\mechs", "-dds", "-dae" };
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