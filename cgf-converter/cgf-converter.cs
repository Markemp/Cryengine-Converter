using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CgfConverter;
using CgfConverter.Renderers.Gltf;
using Exception = System.Exception;

namespace CgfConverterConsole;

public class Program
{
    private readonly ArgsHandler _argsHandler;

    private Program(ArgsHandler argsHandler)
    {
        _argsHandler = argsHandler;
    }

    public static async Task<int> Main(string[] args)
    {
        Utilities.LogLevel = LogLevelEnum.Info;
        Utilities.DebugLevel = LogLevelEnum.Debug;

        var customCulture = (CultureInfo) Thread.CurrentThread.CurrentCulture.Clone();
        customCulture.NumberFormat.NumberDecimalSeparator = ".";
        Thread.CurrentThread.CurrentCulture = customCulture;

        var argsHandler = new ArgsHandler();
        var numErrorsOccurred = argsHandler.ProcessArgs(args);
        if (numErrorsOccurred != 0)
            return numErrorsOccurred;

        return await new Program(argsHandler).Run();
    }

    private async Task<int> Run()
    {
#if DEBUG
        for (var i = 0; i < _argsHandler.InputFiles.Count; i++)
        {
            var inputFile = _argsHandler.InputFiles[i];
            Utilities.Log(LogLevelEnum.Info,
                $"[{i + 1}/{_argsHandler.InputFiles.Count} {100f * i / _argsHandler.InputFiles.Count:0.00}%] " +
                inputFile);
            
            ExportFile(inputFile);
        }

        Utilities.Log(LogLevelEnum.Info, "Finished.");
        
        return 0;
#else
        var workers = new Dictionary<Task, string>();
        var numErrorsOccurred = 0;

        for (var i = 0; i < _argsHandler.InputFiles.Count || workers.Any();)
        {
            while (workers.Count >= _argsHandler.MaxThreads || (workers.Any() && i == _argsHandler.InputFiles.Count))
            {
                var task = await Task.WhenAny(workers.Keys);
                if (!workers.Remove(task, out var failedFile))
                    continue;
                if (!task.IsFaulted || task.Exception is null)
                    continue;

                Utilities.Log(LogLevelEnum.Critical);
                Utilities.Log(LogLevelEnum.Critical,
                    "********************************************************************************");
                Utilities.Log(LogLevelEnum.Critical, "There was an error rendering {0}", failedFile);
                Utilities.Log(LogLevelEnum.Critical);
                Utilities.Log(LogLevelEnum.Critical, task.Exception.Message);
                Utilities.Log(LogLevelEnum.Critical);
                Utilities.Log(LogLevelEnum.Critical, task.Exception.StackTrace);
                Utilities.Log(LogLevelEnum.Critical,
                    "********************************************************************************");
                Utilities.Log(LogLevelEnum.Critical);
                numErrorsOccurred++;
            }

            if (i >= _argsHandler.InputFiles.Count)
                continue;

            var inputFile = _argsHandler.InputFiles[i];
            Utilities.Log(LogLevelEnum.Info,
                $"[{i + 1}/{_argsHandler.InputFiles.Count} {100f * i / _argsHandler.InputFiles.Count:0.00}%] " +
                inputFile);

            workers.Add(Task.Run(() => ExportFile(inputFile)), inputFile);

            i++;
        }

        Utilities.Log(LogLevelEnum.Info, "Finished.");

        return numErrorsOccurred;
#endif
    }

    private void ExportFile(string inputFile)
    {
        // Read CryEngine Files
        var cryData = new CryEngine(inputFile, _argsHandler.DataDir.FullName);

        cryData.ProcessCryengineFiles();

        if (_argsHandler.OutputWavefront)
        {
            Wavefront objFile = new(_argsHandler, cryData);
            objFile.Render(_argsHandler.OutputDir, _argsHandler.PreservePath);
        }

        if (_argsHandler.OutputCollada)
        {
            Collada daeFile = new(_argsHandler, cryData);
            daeFile.Render(_argsHandler.OutputDir, _argsHandler.PreservePath);
        }

        if (_argsHandler.OutputGLTF || _argsHandler.OutputGLB)
        {
            GltfRenderer gltfFile = new(_argsHandler, cryData, _argsHandler.OutputGLTF, _argsHandler.OutputGLB);
            gltfFile.Render(_argsHandler.OutputDir, _argsHandler.PreservePath);
        }
    }
}