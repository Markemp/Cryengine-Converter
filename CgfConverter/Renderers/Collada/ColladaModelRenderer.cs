using CgfConverter.CryEngineCore;
using CgfConverter.Models.Structs;
using CgfConverter.Renderers.Collada.Collada;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Metadata;
using CgfConverter.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Xml.Serialization;

namespace CgfConverter.Renderers.Collada;

/// <summary>
/// ColladaModelRenderer - Main orchestration class for Collada (.dae) export.
/// Split into partial classes for maintainability:
/// - ColladaModelRenderer.cs (this file) - Main orchestration, constructor, Render()
/// - ColladaModelRenderer.Animation.cs - Animation export
/// - ColladaModelRenderer.Materials.cs - Material and texture handling
/// - ColladaModelRenderer.Geometry.cs - Mesh/geometry creation
/// - ColladaModelRenderer.Skeleton.cs - Controller/bone/skinning
/// - ColladaModelRenderer.Nodes.cs - Visual scene and node hierarchy
/// - ColladaModelRenderer.Utilities.cs - String formatting helpers
/// </summary>
public partial class ColladaModelRenderer : IRenderer
{
    protected readonly ArgsHandler _args;
    protected readonly CryEngine _cryData;
    public readonly ColladaDoc DaeObject = new();

    private readonly CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
    private const string colladaVersion = "1.4.1";
    private readonly XmlSerializer serializer = new(typeof(ColladaDoc));
    private readonly FileInfo daeOutputFile;

    private static readonly Vector3 DefaultNormal = new(0.0f, 0.0f, 0.0f);
    private static readonly UV DefaultUV = new(0.0f, 0.0f);
    private static readonly IRGBA DefaultColor = new(1.0f, 1.0f, 1.0f, 1.0f);

    private readonly Dictionary<uint, string> controllerIdToBoneName = new();

    private readonly TaggedLogger Log;

    public ColladaModelRenderer(ArgsHandler argsHandler, CryEngine cryEngine)
    {
        _args = argsHandler;
        _cryData = cryEngine;
        Log = _cryData.Log;

        daeOutputFile = _args.FormatOutputFileName(".dae", _cryData.InputFile);
    }

    public int Render()
    {
        GenerateDaeObject();

        // At this point, we should have a cryData.Asset object, fully populated.
        Log.D();
        Log.D("*** Starting WriteCOLLADA() ***");
        Log.D();

        TextWriter writer = new StreamWriter(daeOutputFile.FullName);
        serializer.Serialize(writer, DaeObject);

        writer.Close();
        Log.D("End of Write Collada.  Export complete.");
        return 1;
    }

    public void GenerateDaeObject()
    {
        Log.D("Number of models: {0}", _cryData.Models.Count);
        for (int i = 0; i < _cryData.Models.Count; i++)
        {
            Log.D("\tNumber of nodes in model: {0}", _cryData.Models[i].NodeMap.Count);
        }

        WriteColladaRoot(colladaVersion);
        WriteAsset();
        WriteScene();

        DaeObject.Library_Materials = new();
        DaeObject.Library_Images = new();
        DaeObject.Library_Effects = new();

        CreateMaterials();
        WriteLibrary_Geometries();

        // If there is Skinning info, create the controller library and set up visual scene to refer to it.  Otherwise just write the Visual Scene
        var extension = Path.GetExtension(_cryData.InputFile);
        if (extension == ".chr" || extension == ".skin")
        {
            WriteLibrary_Controllers();
            WriteLibrary_VisualScenesWithSkeleton();
        }
        else
            WriteLibrary_VisualScenes();

        // Write animations (requires skeleton to be created first for bone name mapping)
        if (_cryData.Animations is not null && _cryData.Animations.Count > 0)
            WriteLibrary_Animations();
    }

    protected void WriteColladaRoot(string version)
    {
        // Blender doesn't like 1.5. :(
        DaeObject.Collada_Version = version;
    }

    protected void WriteAsset()
    {
        var fileCreated = DateTime.Now;
        var fileModified = DateTime.Now;
        ColladaAsset asset = new()
        {
            Revision = Assembly.GetExecutingAssembly().GetName().Version.ToString()
        };
        ColladaAssetContributor[] contributors = new ColladaAssetContributor[1];
        // Get the actual file creators from the Source Chunk
        contributors[0] = new ColladaAssetContributor();
        foreach (ChunkSourceInfo sourceInfo in _cryData.Chunks.Where(a => a.ChunkType == ChunkType.SourceInfo))
        {
            contributors[0].Author = sourceInfo.Author;
            contributors[0].Source_Data = sourceInfo.SourceFile;
            contributors[0].Source_Data = _cryData.RootNode?.Name ?? Path.GetFileNameWithoutExtension(_cryData.InputFile);
        }
        asset.Created = fileCreated;
        asset.Modified = fileModified;
        asset.Up_Axis = "Z_UP";
        asset.Unit = new ColladaAssetUnit()
        {
            Meter = 1.0,
            Name = "meter"
        };
        asset.Title = _cryData.RootNode?.Name ?? Path.GetFileNameWithoutExtension(_cryData.InputFile);
        DaeObject.Asset = asset;
        DaeObject.Asset.Contributor = contributors;
    }
}
