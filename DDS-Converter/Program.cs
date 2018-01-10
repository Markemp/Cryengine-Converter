using CgfConverter;
using System;
using System.Threading.Tasks;

namespace DDS_Converter
{
    public class Program
    {
        public static Int32 Main(string[] args)
        {
            Utils.LogLevel = LogLevelEnum.Warning;
            Utils.DebugLevel = LogLevelEnum.Debug;
            String oldTitle = Console.Title;

            ArgsHandler argsHandler = new ArgsHandler();
            Int32 result = argsHandler.ProcessArgs(args);

            if (result == 0)
            {
                foreach (String inputFile in argsHandler.InputFiles)
                {
                    try
                    {
                        // Read CryEngine Files
                        CryEngine cryData = new CryEngine(inputFile, argsHandler.DataDir.FullName);

                        #region Render Output Files

                        //if (argsHandler.Output_Blender == true)
                        //{
                        //    Blender blendFile = new Blender(argsHandler, cryData);

                        //    blendFile.Render(argsHandler.OutputDir, argsHandler.InputFiles.Count > 1);
                        //}

                        //if (argsHandler.Output_Wavefront == true)
                        //{
                        //    Wavefront objFile = new Wavefront(argsHandler, cryData);

                        //    objFile.Render(argsHandler.OutputDir, argsHandler.InputFiles.Count > 1);
                        //}

                        //if (argsHandler.Output_CryTek == true)
                        //{
                        //    CryRender cryFile = new CryRender(argsHandler, cryData);

                        //    cryFile.Render(argsHandler.OutputDir, argsHandler.InputFiles.Count > 1);
                        //}

                        //if (argsHandler.Output_Collada == true)
                        //{
                        //    COLLADA daeFile = new COLLADA(argsHandler, cryData);

                        //    daeFile.Render(argsHandler.OutputDir, argsHandler.InputFiles.Count > 1);
                        //}

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
            Console.Title = oldTitle;

#if (DEV_DOLKENSP || DEV_MARKEMP)
            Console.WriteLine("Done...");
            Console.ReadKey();
#endif

            return result;


        }
    }
}
