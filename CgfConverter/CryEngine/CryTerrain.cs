using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Numerics;
using CgfConverter.PackFileSystem;
using CgfConverter.Terrain;
using CgfConverter.Terrain.Xml;
using Extensions;
using HoloXPLOR.DataForge;

namespace CgfConverter;

public class CryTerrain
{
    public readonly string InputFile;
    public readonly IPackFileSystem PackFileSystem;
    public readonly string BasePath;
    public readonly LevelData LevelData;
    public readonly LevelInfo LevelInfo;
    public readonly TerrainFile TerrainFile;
    public readonly ImmutableList<Mission> Missions;
    public readonly ImmutableList<SRenderNodeChunk> AllRenderNodes;
    public readonly Dictionary<string, CryEngine> Objects;

    public CryTerrain(string filename, IPackFileSystem packFileSystem)
    {
        InputFile = filename;
        PackFileSystem = packFileSystem;

        BasePath = Path.GetDirectoryName(filename)!;

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

        foreach (var mission in Missions)
        {
            foreach (var entity in mission.ObjectsAndEntities!)
            {
                if (!string.IsNullOrWhiteSpace(entity.Geometry))
                    continue;
                
                if (entity.Properties is {ObjectModel: {} val} properties)
                {
                    if (CryEngine.SupportsFile(val))
                        entity.Geometry = val;
                    else if (val.EndsWith(".cdf", StringComparison.InvariantCultureIgnoreCase))
                        entity.Geometry = val[..^4] + ".chr";  // TODO: AttachmentList

                    if (false && properties is {UseTranslation: "1", Movement: { X: {} xDistance, Y: {} yDistance, Z: {} zDistance }})
                    {
                        // TODO: check if this is correct
                        entity.PosValue = (entity.PosValue ?? Vector3.Zero) +
                                          new Vector3(float.Parse(xDistance),float.Parse(yDistance), float.Parse(zDistance));
                    }

                    if (properties is {UseRotation: "1", Rotation: { X: {} xAngle, Y: {} yAngle, Z: {} zAngle }})
                    {
                        var x = Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)(float.Parse(xAngle) * Math.PI / 180f));
                        var y = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float)(float.Parse(yAngle) * Math.PI / 180f));
                        var z = Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float)(float.Parse(zAngle) * Math.PI / 180f));
                            
                        // TODO: check if we did rotate it the right direction (CW/CCW)
                        entity.RotateValue = (entity.RotateValue ?? Quaternion.Identity) * (x * y * z);
                    }
                        
                    continue;
                }
                
                if (entity.EntityClass?.StartsWith("brb", StringComparison.InvariantCultureIgnoreCase) is true)
                {
                    var path = $"objects/gameplay_assets/{entity.EntityClass[3..]}.cgf";
                    if (packFileSystem.Exists(path))
                    {
                        entity.Geometry = path;
                        continue;
                    }

                    if (entity.EntityClass == "brbJumpPad")
                    {
                        entity.Geometry = "objects/characters/11_ambient/bouncepad_static/bouncepad_static.chr";
                        continue;
                    }
                }
                
                // entity.Geometry = "objects/default/primitive_cube.cgf";
            }
        }

        AllRenderNodes = allObjects.ToImmutableList();

        Objects = TerrainFile.BrushObjects
            .Concat(Missions.SelectMany(x => x.ObjectsAndEntities!)
                .Select(x => x.Geometry)
                .Where(x => x is not null))
            .Select(x => x!.Replace("%level%", Path.GetDirectoryName(filename)))
            .DistinctBy(x => x.ToLowerInvariant())
            .AsParallel()
            .Select(path =>
            {
                var engine = new CryEngine(path, packFileSystem);
                engine.ProcessCryengineFiles();
                return Tuple.Create(path, engine);
            }).ToDictionary(x => x.Item1.ToLowerInvariant(), x => x.Item2);
    }

    public static bool SupportsFile(string name) => Path.GetFileName(name).ToLowerInvariant() == "leveldata.xml";
}