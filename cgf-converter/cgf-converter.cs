using System;
using System.Threading;
using System.Globalization;
using CgfConverter;

namespace CgfConverterConsole;

public class Program
{
    public static int Main(string[] args)
    {
        Utils.LogLevel = LogLevelEnum.Info;
        Utils.DebugLevel = LogLevelEnum.Debug;

        string oldTitle = Console.Title;

        CultureInfo customCulture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
        customCulture.NumberFormat.NumberDecimalSeparator = ".";

        Thread.CurrentThread.CurrentCulture = customCulture;
        var argsHandler = new ArgsHandler();
#if !DEBUG
        try
        {
#endif
            if (argsHandler.ProcessArgs(args) == 0)
            {
                foreach (string inputFile in argsHandler.InputFiles)
                {
                    try
                    {
                        // Read CryEngine Files
                        var cryData = new CryEngine(inputFile, argsHandler.DataDir.FullName);

                        cryData.ProcessCryengineFiles();

                        if (argsHandler.OutputWavefront)
                        {
                            Wavefront objFile = new(argsHandler, cryData);
                            objFile.Render(argsHandler.OutputDir, argsHandler.InputFiles.Count > 1);
                        }

                        if (argsHandler.OutputCollada)
                        {
                            Collada daeFile = new(argsHandler, cryData);
                            daeFile.Render(argsHandler.OutputDir, argsHandler.InputFiles.Count > 1);
                        }

                        if (argsHandler.OutputUsd)
                        {

                        }
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
                        return 1;
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

        Utils.Log(LogLevelEnum.Debug, "Done...");
        
        return 0;
    }
}