using CgfConverter.CryEngineCore;
using CgfConverter.Models;
using CgfConverter.Models.Shaders;
using CgfConverter.Parsers;
using CgfConverter.Renderers.USD.Models;
using CgfConverter.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CgfConverter.Renderers.USD;

/// <summary>
/// USD renderer for exporting CryEngine assets to Universal Scene Description format.
/// Organized as partial classes: Main, Materials, Geometry, Skeleton.
/// </summary>
public partial class UsdRenderer : IRenderer
{
    protected readonly Args _args;
    protected readonly CryEngine _cryData;

    private readonly FileInfo usdOutputFile;
    private UsdSerializer usdSerializer;
    protected readonly TaggedLogger Log;

    // Shader system
    protected readonly Dictionary<string, ShaderDefinition> _shaderDefinitions;
    protected readonly ShaderRulesEngine _shaderRules;

    public UsdRenderer(Args argsHandler, CryEngine cryEngine)
    {
        _args = argsHandler;
        _cryData = cryEngine;
        usdOutputFile = _args.FormatOutputFileName(".usda", _cryData.InputFile);
        usdSerializer = new UsdSerializer();
        Log = _cryData.Log;

        // Initialize shader system
        _shaderRules = new ShaderRulesEngine(Log);
        _shaderDefinitions = LoadShaderDefinitions();
    }

    /// <summary>
    /// Load shader definitions from the Shaders directory.
    /// </summary>
    private Dictionary<string, ShaderDefinition> LoadShaderDefinitions()
    {
        // Try to find Shaders directory in datadir (objectdir)
        if (string.IsNullOrEmpty(_args.DataDir))
        {
            Log.D("No datadir specified, shader-based materials disabled");
            return new Dictionary<string, ShaderDefinition>(System.StringComparer.OrdinalIgnoreCase);
        }

        var shadersDir = Path.Combine(_args.DataDir, "Shaders");
        if (!Directory.Exists(shadersDir))
        {
            Log.D($"Shaders directory not found: {shadersDir}, shader-based materials disabled");
            return new Dictionary<string, ShaderDefinition>(System.StringComparer.OrdinalIgnoreCase);
        }

        var parser = new ShaderExtParser(Log);
        var shaders = parser.LoadShadersFromDirectory(shadersDir);

        if (shaders.Count > 0)
        {
            Log.D($"Loaded {shaders.Count} shader definitions from {shadersDir}");
        }

        return shaders;
    }

    // Cached skeleton data for multi-file animation export
    private Dictionary<uint, string>? _controllerIdToJointPath;
    private List<string>? _jointPaths;
    private Dictionary<CompiledBone, string>? _bonePathMap;
    private int[]? _compiledBoneIndexToJointIndex;  // Maps CompiledBones array index to jointPaths array index

    public int Render()
    {
        var usdDoc = GenerateUsdObject();

        Log.D();
        Log.D("*** Starting Write USD ***");
        Log.D();

        WriteUsdToFile(usdDoc);

        // Export individual animation files for Blender NLA workflow
        // (Blender only imports single bound animation, so we export each separately)
        if (_controllerIdToJointPath is not null && _jointPaths is not null && _bonePathMap is not null)
        {
            var animCount = ExportAnimationFiles(_controllerIdToJointPath, _jointPaths, _bonePathMap);
            if (animCount > 0)
            {
                Log.I($"Exported {animCount} separate animation files for Blender NLA workflow");
            }
        }

        return 1;
    }

    public void WriteUsdToFile(UsdDoc usdDoc)
    {
        TextWriter writer = new StreamWriter(usdOutputFile.FullName);
        usdSerializer.Serialize(usdDoc, writer);
        writer.Close();
    }

    public UsdDoc GenerateUsdObject()
    {
        Log.D("Number of models: {0}", _cryData.Models.Count);
        for (int i = 0; i < _cryData.Models.Count; i++)
        {
            Log.D("\tNumber of nodes in model: {0}", _cryData.Models[i].NodeMap.Count);
        }

        // Create the usd doc
        var usdDoc = new UsdDoc { Header = new UsdHeader() };
        usdDoc.Prims.Add(new UsdXform("root", "/"));
        var rootPrim = usdDoc.Prims[0];

        // Check if this model has skeletal animation
        bool hasSkeleton = _cryData.SkinningInfo?.HasSkinningInfo ?? false;

        if (hasSkeleton)
        {
            Log.D("Model has skeleton with {0} bones", _cryData.SkinningInfo.CompiledBones.Count);

            var skelRoot = CreateSkeleton(out _controllerIdToJointPath, out _jointPaths, out _bonePathMap, out _compiledBoneIndexToJointIndex);

            if (HasIvoSkeletonBinding())
            {
                // Ivo format with node-to-bone mapping (.cga): meshes go inside SkelRoot
                // Skeleton drives all mesh positioning via per-vertex bone binding
                skelRoot.Children.AddRange(CreateIvoSkinnedMeshes());
            }
            else
            {
                // Skeleton without node mapping (.skin/.chr): meshes go inside SkelRoot
                // with skinning from IntVertices/Ext2IntMap or BoneMappings
                skelRoot.Children.AddRange(CreateSkinnedMeshes());
            }

            // Add animation to the main file only when there's exactly one.
            // Multiple animations get exported as separate files (each with their own skeleton)
            // due to USD's single animation binding per skeleton limitation.
            var animPrims = CreateAnimations(_controllerIdToJointPath, usdDoc.Header);
            if (animPrims.Count == 1)
            {
                skelRoot.Children.Add(animPrims[0]);

                var skeleton = skelRoot.Children.OfType<UsdSkeleton>().FirstOrDefault();
                if (skeleton is not null)
                {
                    var animPath = $"</root/Armature/{animPrims[0].Name}>";
                    skeleton.Attributes.Add(new UsdRelationship("skel:animationSource", animPath));
                }
            }

            rootPrim.Children.Add(skelRoot);
        }
        else
        {
            // No skeleton: just Xform hierarchy
            rootPrim.Children.AddRange(CreateNodeHierarchy());
        }

        rootPrim.Children.Add(CreateMaterials());

        return usdDoc;
    }

    /// <summary>
    /// Check if the model has Ivo format skeleton binding (bones with ObjectNodeIndex mappings).
    /// </summary>
    private bool HasIvoSkeletonBinding()
    {
        var bones = _cryData.SkinningInfo?.CompiledBones;
        if (bones is null || bones.Count == 0)
            return false;

        return bones.Any(b => b.ObjectNodeIndex >= 0);
    }

    /// <summary>
    /// Clean a string to be a valid USD prim name.
    /// USD prim names must start with letter/underscore and contain only letters, digits, and underscores.
    /// </summary>
    protected string CleanPathString(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "_";

        // Replace invalid characters with underscore
        var cleaned = new StringBuilder();
        foreach (char c in value)
        {
            if (char.IsLetterOrDigit(c) || c == '_')
                cleaned.Append(c);
            else
                cleaned.Append('_');
        }

        // Ensure it starts with letter or underscore
        var result = cleaned.ToString();
        if (char.IsDigit(result[0]))
            result = "_" + result;

        return result;
    }
}
