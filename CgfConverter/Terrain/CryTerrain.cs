using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Numerics;
using CgfConverter.CryXmlB;
using CgfConverter.PackFileSystem;
using CgfConverter.Terrain.Xml;
using CgfConverter.Utils;
using Extensions;

namespace CgfConverter.Terrain;

public class CryTerrain
{
    protected readonly TaggedLogger Log;
    
    public readonly IPackFileSystem PackFileSystem;
    public readonly string BasePath;
    public readonly string BaseName;
    public readonly LevelData LevelData;
    public readonly LevelInfo LevelInfo;
    public readonly TerrainFile TerrainFile;
    public readonly ImmutableList<Mission> Missions;
    public readonly ImmutableList<SRenderNodeChunk> AllRenderNodes;
    public readonly Dictionary<string, CryEngine> Objects;

    public readonly CryTerrainLayer RootLayer;
    public readonly CryTerrainEntity RootEntity;

    public CryTerrain(string filename, IPackFileSystem packFileSystem)
    {
        PackFileSystem = packFileSystem;

        BasePath = Path.TrimEndingDirectorySeparator(Path.GetDirectoryName(filename)!);
        BaseName = Path.GetFileName(BasePath);
        Log = new TaggedLogger(BaseName);

        LevelData = CryXmlSerializer.Deserialize<LevelData>(packFileSystem.GetStream(
            FileHandlingExtensions.CombineAndNormalizePath(
                BasePath, "leveldata.xml")), true);

        LevelInfo = CryXmlSerializer.Deserialize<LevelInfo>(
            packFileSystem.GetStream(
                FileHandlingExtensions.CombineAndNormalizePath(
                    BasePath, "levelinfo.xml")), true);

        TerrainFile = new TerrainFile(new BinaryReader(packFileSystem.GetStream(
            FileHandlingExtensions.CombineAndNormalizePath(
                BasePath, "terrain", "terrain.dat"))), true);

        Missions = LevelInfo.Missions == null
            ? ImmutableList<Mission>.Empty
            : LevelInfo.Missions.Select(x => CryXmlSerializer.Deserialize<Mission>(
                packFileSystem.GetStream(
                    FileHandlingExtensions.CombineAndNormalizePath(
                        BasePath, $"mission_{x.Name}.xml")), true)).ToImmutableList();

        var allObjects = new List<SRenderNodeChunk>();
        var stack = new List<SOcTreeNodeChunk> {TerrainFile.OcTreeNode};
        while (stack.Any())
        {
            var chunk = stack.Last();
            stack.RemoveAt(stack.Count - 1);
            allObjects.AddRange(chunk.Objects);
            stack.AddRange(chunk.Children.Where(x => x is not null)!);
        }

        RootLayer = new CryTerrainLayer(
            null,
            0,
            LevelData.Layers?.ToImmutableList() ?? ImmutableList<Layer>.Empty,
            allObjects);

        RootEntity = new CryTerrainEntity(0, new ObjectOrEntity(),
            Missions.SelectMany(x => x.ObjectsAndEntities!).ToImmutableList());

        var attachedModels = new List<string?>();
        var entityClasses = new HashSet<string>();
        foreach (var mission in Missions)
        {
            foreach (var entity in mission.ObjectsAndEntities!)
            {
                if (entity.EntityClass is not null)
                    entityClasses.Add(entity.EntityClass);
                attachedModels.Clear();
                attachedModels.Add(entity.Geometry);

                if (entity.Properties is { } properties)
                {
                    attachedModels.Add(properties.BladesModel);
                    attachedModels.Add(properties.ObjectModel);
                    attachedModels.Add(properties.ObjectMesh);
                    attachedModels.Add(properties.ObjectMeshDrc);

                    if (properties is
                        {UseTranslation: "1", Movement: {X: { } xDistance, Y: { } yDistance, Z: { } zDistance}})
                    {
                        // TODO: check if this is correct
                        entity.PosValue = (entity.PosValue ?? Vector3.Zero) +
                                          new Vector3(float.Parse(xDistance), float.Parse(yDistance),
                                              float.Parse(zDistance));
                    }

                    if (properties is {UseRotation: "1", Rotation: {X: { } xAngle, Y: { } yAngle, Z: { } zAngle}})
                    {
                        var x = Quaternion.CreateFromAxisAngle(Vector3.UnitY,
                            -(float) (float.Parse(xAngle) * Math.PI / 180f));
                        var y = Quaternion.CreateFromAxisAngle(Vector3.UnitZ,
                            -(float) (float.Parse(yAngle) * Math.PI / 180f));
                        var z = Quaternion.CreateFromAxisAngle(Vector3.UnitX,
                            -(float) (float.Parse(zAngle) * Math.PI / 180f));

                        // TODO: check if we did rotate it the right direction (CW/CCW)
                        entity.RotateValue = (entity.RotateValue ?? Quaternion.Identity) * (x * y * z);
                    }
                }

                switch (entity.EntityClass?.ToLowerInvariant())
                {
                    // Couldn't find where are these object name constants defined, thus hardcoding.
                    case "brbjumppad":
                        attachedModels.Add(entity.Properties?.JumpPadType == "1"
                            ? "objects/characters/11_ambient/bouncepad_rotate/bouncepad_rotate.chr"
                            : "objects/characters/11_ambient/bouncepad_static/bouncepad_static.chr");
                        break;

                    case "brbpartchest":
                        attachedModels.Add("objects/characters/11_ambient/treasure_chest/treasure_chest.chr");
                        break;

                    case "brbcrystallock":
                        // TODO: use .cdf file
                        attachedModels.Add("objects/characters/11_ambient/crystal_lock/crystal_lock.chr");
                        attachedModels.Add(new[]
                        {
                            "objects/characters/11_ambient/crystal_lock/crystals_amber.chr",
                            "objects/characters/11_ambient/crystal_lock/crystals_aqua.chr",
                            "objects/characters/11_ambient/crystal_lock/crystals_black.chr",
                            "objects/characters/11_ambient/crystal_lock/crystals_gold.chr",
                            "objects/characters/11_ambient/crystal_lock/crystals_blue.chr",
                            "objects/characters/11_ambient/crystal_lock/crystals_green.chr",
                            "objects/characters/11_ambient/crystal_lock/crystals_purple.chr",
                            "objects/characters/11_ambient/crystal_lock/crystals_red.chr",
                        }[int.Parse(entity.Properties?.CrystalType ?? "0")]);
                        break;

                    default:
                        if (entity.EntityClass?.StartsWith("brb", StringComparison.InvariantCultureIgnoreCase) is not
                            true)
                            break;

                        var path = $"objects/gameplay_assets/{entity.EntityClass[3..]}.cgf";
                        if (packFileSystem.Exists(path))
                            attachedModels.Add(path);
                        break;
                }

                entity.AllAttachedModelPaths = attachedModels
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Cast<string>()
                    .Select(x => x.Replace("%level%", BasePath).ToLowerInvariant().Replace('\\', '/'))
                    .ToList();
                attachedModels.Clear();

                for (var i = 0; i < entity.AllAttachedModelPaths.Count; i++)
                {
                    var val = entity.AllAttachedModelPaths[i];
                    if (CryEngine.SupportsFile(val))
                        continue;
                    if (val.EndsWith(".cdf", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // TODO: use AttachmentList inside .cdf file
                        entity.AllAttachedModelPaths[i] = val[..^4] + ".chr";
                        continue;
                    }

                    entity.AllAttachedModelPaths.RemoveAt(i--);
                }

                // entity.Geometry = "objects/default/primitive_cube.cgf";
            }
        }

        Log.V("The file contains the following types of EntityClass:\n" +
              string.Join("\n", entityClasses.Select(x => $" - {x}")));

        AllRenderNodes = allObjects.ToImmutableList();

        Objects = TerrainFile.BrushObjects
            .Concat(TerrainFile.StaticInstanceGroups.Select(x => x.FileName))
            .Concat(Missions.SelectMany(x => x.ObjectsAndEntities!)
                .SelectMany(x => x.AllAttachedModelPaths))
            .Select(x => x.Replace("%level%", BasePath).ToLowerInvariant().Replace('\\', '/'))
            .Distinct()
            .AsParallel()
            .Select(path =>
            {
                try
                {
                    var engine = new CryEngine(path, packFileSystem, Log);
                    engine.ProcessCryengineFiles();
                    return Tuple.Create(path, engine);
                }
                catch (FileNotFoundException)
                {
                    Log.W("Model file was expected at {0} but could not be found.", path);
                    return null;
                }
                catch (Exception e)
                {
                    Log.E(e, "Unexpected error loading: {0}", path);
                    return null;
                }
            })
            .Where(x => x != null)
            .ToDictionary(x => x!.Item1.ToLowerInvariant(), x => x!.Item2);
    }

    public static bool SupportsFile(string name) => Path.GetFileName(name).ToLowerInvariant() == "leveldata.xml";
}