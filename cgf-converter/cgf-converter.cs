using CgfConverter.Renderers;
using CgfConverter.Renderers.Collada;
using CgfConverter.Renderers.Gltf;
using CgfConverter.Renderers.Wavefront;
using CgfConverter.Terrain;
using CgfConverter.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CgfConverter;

public class Program
{
    private readonly TaggedLogger Log = new("Program");
    private readonly ArgsHandler _args;

    private Program(ArgsHandler args)
    {
        _args = args;
    }

#if DEBUG
    public static int Main(string[] args)
#else
    public static async Task<int> Main(string[] args)
#endif
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

#if DEBUG
        return new Program(argsHandler).Run();
#else
        return await new Program(argsHandler).Run();
#endif
    }

#if DEBUG
    private int Run()
    {
        for (var i = 0; i < _args.InputFiles.Count; i++)
        {
            var inputFile = _args.InputFiles[i];
            Log.I(
                $"[{i + 1}/{_args.InputFiles.Count} {100f * i / _args.InputFiles.Count:0.00}%] " +
                inputFile);

            if (CryEngine.SupportsFile(inputFile))
                ExportSingleModel(inputFile);
            else if (CryTerrain.SupportsFile(inputFile))
                ExportSingleTerrain(inputFile);
        }

        Log.I("Finished.");

        return 0;
    }
#else
    private async Task<int> Run()
    {
        var workers = new Dictionary<Task, string>();
        var numErrorsOccurred = 0;

        for (var i = 0; i < _args.InputFiles.Count || workers.Any();)
        {
            while (workers.Count >= _args.MaxThreads || (workers.Any() && i == _args.InputFiles.Count))
            {
                var task = await Task.WhenAny(workers.Keys);
                if (!workers.Remove(task, out var failedFile))
                    continue;
                if (!task.IsFaulted || task.Exception is null)
                    continue;
                
                Log.E(task.Exception, "Failed to render: {0}", failedFile);
                numErrorsOccurred++;
            }

            if (i >= _args.InputFiles.Count)
                continue;

            var inputFile = _args.InputFiles[i];
            Log.I("[{0}/{1} {2:0.00}%] {3}",
                i + 1, _args.InputFiles.Count, 100f * i / _args.InputFiles.Count, inputFile);

            if (CryEngine.SupportsFile(inputFile))
                workers.Add(Task.Run(() => ExportSingleModel(inputFile)), inputFile);
            else if (CryTerrain.SupportsFile(inputFile))
                workers.Add(Task.Run(() => ExportSingleTerrain(inputFile)), inputFile);

            i++;
        }

        Log.I("Finished. Rendered {0} file(s)", _args.InputFiles.Count - numErrorsOccurred);
        if (numErrorsOccurred > 0)
            Log.E("Failed to convert {0} file(s).", numErrorsOccurred);

        return numErrorsOccurred;
    }
#endif

    private void ExportSingleModel(string inputFile)
    {
        var data = new CryEngine(
            inputFile,
            _args.PackFileSystem,
            materialFiles: _args.MaterialFile);

        data.ProcessCryengineFiles();

        var renderers = new List<IRenderer>();
        if (_args.OutputWavefront)
            renderers.Add(new WavefrontModelRenderer(_args, data));
        if (_args.OutputCollada)
            renderers.Add(new ColladaModelRenderer(_args, data));
        if (_args.OutputGLB || _args.OutputGLTF)
            renderers.Add(new GltfModelRenderer(_args, data, _args.OutputGLTF, _args.OutputGLB));

        RunRenderersAndThrowAggregateExceptionIfAny(renderers);
    }

    private void ExportSingleTerrain(string inputFile)
    {
        var data = new CryTerrain(inputFile, _args.PackFileSystem);

        var renderers = new List<IRenderer>();
        if (_args.OutputGLB || _args.OutputGLTF)
            renderers.Add(new GltfTerrainRenderer(_args, data, _args.OutputGLTF, _args.OutputGLB));

        RunRenderersAndThrowAggregateExceptionIfAny(renderers);
    }

    private void RunRenderersAndThrowAggregateExceptionIfAny(IEnumerable<IRenderer> renderers)
    {
        var exceptions = new List<Exception>();
        foreach (var renderer in renderers)
        {
            try
            {
                renderer.Render();
            }
            catch (Exception e)
            {
                exceptions.Add(e);
            }
        }

        if (exceptions.Any())
            throw new AggregateException(exceptions);
    }
}
