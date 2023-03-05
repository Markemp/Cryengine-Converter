using System;
using System.Collections.Generic;
using System.Threading;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CgfConverter;
using CgfConverter.Renderers.Gltf;

namespace CgfConverterConsole;

public class Program
{
    private readonly ArgsHandler _argsHandler;

    private Program(ArgsHandler argsHandler)
    {
        _argsHandler = argsHandler;
    }

#if DEBUG
    public static int Main(string[] args)
#else
    public static async Task<int> Main(string[] args)
#endif
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

#if DEBUG
        return new Program(argsHandler).Run();
#else
        return await new Program(argsHandler).Run();
#endif
    }

#if DEBUG
    private int Run()
    {
        for (var i = 0; i < _argsHandler.InputFiles.Count; i++)
        {
            var inputFile = _argsHandler.InputFiles[i];
            Utilities.Log(LogLevelEnum.Info,
                $"[{i + 1}/{_argsHandler.InputFiles.Count} {100f * i / _argsHandler.InputFiles.Count:0.00}%] " +
                inputFile);
            
            if (CryEngine.SupportsFile(inputFile))
                ExportSingleModel(inputFile);
            else if (CryTerrain.SupportsFile(inputFile))
                ExportSingleTerrain(inputFile);
        }

        Utilities.Log(LogLevelEnum.Info, "Finished.");
        
        return 0;
    }
#else
    private async Task<int> Run()
    {
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
                if (task.Exception.InnerExceptions.Any())
                {
                    foreach (var inner in task.Exception.InnerExceptions)
                    {
                        Utilities.Log(LogLevelEnum.Critical);
                        Utilities.Log(LogLevelEnum.Critical, inner.Message);
                        Utilities.Log(LogLevelEnum.Critical);
                        Utilities.Log(LogLevelEnum.Critical, inner.StackTrace);
                    }
                }
                else
                {
                    Utilities.Log(LogLevelEnum.Critical);
                    Utilities.Log(LogLevelEnum.Critical, task.Exception.Message);
                    Utilities.Log(LogLevelEnum.Critical);
                    Utilities.Log(LogLevelEnum.Critical, task.Exception.StackTrace);
                }

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

            if (CryEngine.SupportsFile(inputFile))
                workers.Add(Task.Run(() => ExportSingleModel(inputFile)), inputFile);
            else if (CryTerrain.SupportsFile(inputFile))
                workers.Add(Task.Run(() => ExportSingleTerrain(inputFile)), inputFile);

            i++;
        }

        Utilities.Log(LogLevelEnum.Info, "Finished.");

        return numErrorsOccurred;
    }
#endif

    private void ExportSingleModel(string inputFile)
    {
        // Read CryEngine Files
        var cryData = new CryEngine(inputFile, _argsHandler.PackFileSystem);

        cryData.ProcessCryengineFiles();

        if (_argsHandler.OutputWavefront)
        {
            Wavefront objFile = new(_argsHandler, cryData);
            try
            {
                objFile.Render(_argsHandler.OutputDir, _argsHandler.PreservePath);
            }
            catch (NotSupportedException)
            {
                Utilities.Log(LogLevelEnum.Error, "Not supported [.obj]: {0}", inputFile);
            }
        }

        if (_argsHandler.OutputCollada)
        {
            Collada daeFile = new(_argsHandler, cryData);
            try
            {
                daeFile.Render(_argsHandler.OutputDir, _argsHandler.PreservePath);
            }
            catch (NotSupportedException)
            {
                Utilities.Log(LogLevelEnum.Error, "Not supported [.dae]: {0}", inputFile);
            }
        }

        if (_argsHandler.OutputGLTF || _argsHandler.OutputGLB)
        {
            GltfRenderer gltfFile = new(_argsHandler, cryData, _argsHandler.OutputGLTF, _argsHandler.OutputGLB);
            try
            {
                gltfFile.Render(_argsHandler.OutputDir, _argsHandler.PreservePath);
            }
            catch (NotSupportedException)
            {
                Utilities.Log(LogLevelEnum.Error, "Not supported [.gltf]: {0}", inputFile);
            }
        }
    }

    private void ExportSingleTerrain(string inputFile)
    {
        var terrain = new CryTerrain(inputFile, _argsHandler.PackFileSystem);
        var outBasePath = _argsHandler.OutputDir;
        if (outBasePath == null)
            outBasePath = Path.TrimEndingDirectorySeparator(Path.GetDirectoryName(inputFile)!);
        else
            outBasePath = Path.Join(outBasePath, Path.GetFileName(Path.GetDirectoryName(inputFile)!));
        // TODO: bunch of args processing
        
        if (_argsHandler.OutputGLTF || _argsHandler.OutputGLB)
        {
            var renderer = new GltfRendererCommon(_argsHandler.PackFileSystem,
                    new List<Regex>
                {
                    new(".*_shadows?[_.].*", RegexOptions.IgnoreCase),
                    new(".*shadow_caster.*", RegexOptions.IgnoreCase),
                });
            if (renderer.RenderSingleTerrain(terrain, outBasePath, _argsHandler.OutputGLB, _argsHandler.OutputGLTF, true) > 1)
                renderer.RenderSingleTerrain(terrain, outBasePath, _argsHandler.OutputGLB, _argsHandler.OutputGLTF, false);
        }
    }
}