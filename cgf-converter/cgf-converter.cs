using System.Threading;
using System.Globalization;
using CgfConverter;
using CgfConverter.Renderers.Gltf;

namespace CgfConverterConsole;

public class Program
{
    public static int Main(string[] args)
    {
        Utilities.LogLevel = LogLevelEnum.Info;
        Utilities.DebugLevel = LogLevelEnum.Debug;

        var customCulture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
        customCulture.NumberFormat.NumberDecimalSeparator = ".";
        Thread.CurrentThread.CurrentCulture = customCulture;
        
        var argsHandler = new ArgsHandler();
        var numErrorsOccurred = argsHandler.ProcessArgs(args);
        if (numErrorsOccurred != 0)
            return numErrorsOccurred;
    
        for (var i = 0; i < argsHandler.InputFiles.Count; i++)
        {
            var inputFile = argsHandler.InputFiles[i];
            Utilities.Log(LogLevelEnum.Info,
                $"[{i + 1}/{argsHandler.InputFiles.Count} {100f * i / argsHandler.InputFiles.Count:0.00}%] " +
                inputFile);
#if !DEBUG
            try
            {
#endif
                // Read CryEngine Files
                var cryData = new CryEngine(inputFile, argsHandler.DataDir.FullName);

                cryData.ProcessCryengineFiles();

                if (argsHandler.OutputWavefront)
                {
                    Wavefront objFile = new(argsHandler, cryData);
                    objFile.Render(argsHandler.OutputDir, argsHandler.PreservePath);
                }

                if (argsHandler.OutputCollada)
                {
                    Collada daeFile = new(argsHandler, cryData);
                    daeFile.Render(argsHandler.OutputDir, argsHandler.PreservePath);
                }
                
                if (argsHandler.OutputGLTF || argsHandler.OutputGLB)
                {
                    GltfRenderer gltfFile = new(argsHandler, cryData, argsHandler.OutputGLTF, argsHandler.OutputGLB);
                    gltfFile.Render(argsHandler.OutputDir, argsHandler.PreservePath);
                }
#if !DEBUG
            }
            catch (Exception ex)
            {
                Utilities.Log(LogLevelEnum.Critical);
                Utilities.Log(LogLevelEnum.Critical, "********************************************************************************");
                Utilities.Log(LogLevelEnum.Critical, "There was an error rendering {0}", inputFile);
                Utilities.Log(LogLevelEnum.Critical);
                Utilities.Log(LogLevelEnum.Critical, ex.Message);
                Utilities.Log(LogLevelEnum.Critical);
                Utilities.Log(LogLevelEnum.Critical, ex.StackTrace);
                Utilities.Log(LogLevelEnum.Critical, "********************************************************************************");
                Utilities.Log(LogLevelEnum.Critical);
                numErrorsOccurred++;
            }
#endif
        }

        Utilities.Log(LogLevelEnum.Info, "Finished.");
        
        return numErrorsOccurred;
    }
}