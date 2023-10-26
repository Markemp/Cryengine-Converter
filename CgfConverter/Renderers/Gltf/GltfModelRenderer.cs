using CgfConverter.Renderers.Gltf.Models;
using Extensions;
using System;
using System.IO;
using System.Linq;
using System.Numerics;

namespace CgfConverter.Renderers.Gltf;

public class GltfModelRenderer : BaseGltfRenderer, IRenderer
{
    public GltfModelRenderer(ArgsHandler argsHandler, CryEngine cryEngine, bool writeText, bool writeBinary)
        : base(argsHandler, Path.GetFileName(cryEngine.InputFile), writeText, writeBinary)
    {
        _cryData = cryEngine;
    }

    public int Render()
    {
        // Create the root object
        Reset("Scene");

        var rootIndex = _gltfRoot.Nodes.Count;
        var root = new GltfNode()
        {
            Name = _cryData.RootNode.Name,
            Rotation = Quaternion.CreateFromYawPitchRoll((float)Math.PI, -(float)Math.PI / 2.0f, 0).ToGltfList(),
            Translation = Vector3.Zero.ToGltfList(),
            Scale = Vector3.One.ToGltfList()
        };
        _gltfRoot.Nodes.Add(root);

        CurrentScene.Nodes.Add(rootIndex);

        // For each root node in the crydata, add to the scene nodes.
        foreach (var cryNode in _cryData.Models[0].NodeMap.Values.Where(a => a.ParentNodeID == ~0).ToList())
        {
            // CurrentScene.Nodes has the index for the nodes in GltfRoot.Nodes
            // This is only for node chunks.  Bones are handled next.
            root.Children.Add(CreateGltfNode(cryNode));
        }

        Save(_cryData.InputFile);
        return 1;
    }


    public GltfRoot GenerateGltfObject()
    {
        Reset("Scene");
        var rootIndex = _gltfRoot.Nodes.Count;
        var root = new GltfNode()
        {
            Name = _cryData.RootNode.Name,
            Rotation = Quaternion.CreateFromYawPitchRoll((float)Math.PI, -(float)Math.PI / 2.0f, 0).ToGltfList(),
            Translation = Vector3.Zero.ToGltfList(),
            Scale = Vector3.One.ToGltfList()
        };
        _gltfRoot.Nodes.Add(root);

        CurrentScene.Nodes.Add(rootIndex);
        // For each root node in the crydata, add to the scene nodes.
        foreach (var cryNode in _cryData.Models[0].NodeMap.Values.Where(a => a.ParentNodeID == ~0).ToList())
        {
            root.Children.Add(CreateGltfNode(cryNode));
        }

        return _gltfRoot;
    }
}