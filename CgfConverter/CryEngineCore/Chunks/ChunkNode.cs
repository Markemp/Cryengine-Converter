using CgfConverter.Models.Materials;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace CgfConverter.CryEngineCore;

public abstract class ChunkNode : Chunk
{
    protected float VERTEX_SCALE = 1f / 100;

    public string Name { get; internal set; } = string.Empty;
    public int ObjectNodeID { get; internal set; } // Doesn't exist in Ivo
    public int ParentNodeID { get; internal set; }  // Parent nodeID
    public int ParentNodeIndex { get; internal set; } // Parent node index for Ivo files
    public int NumChildren { get; internal set; }
    public int MaterialID { get; internal set; }    // Chunk Id of the material for this node
    public Matrix4x4 Transform { get; internal set; }
    [Obsolete("Use Transform")]
    public Vector3 Pos { get; internal set; }
    [Obsolete("Use Transform")]
    public Quaternion Rot { get; internal set; }
    [Obsolete("Use Transform")]
    public Vector3 Scale { get; internal set; }
    public int PosCtrlID { get; internal set; }
    public int RotCtrlID { get; internal set; }
    public int SclCtrlID { get; internal set; }
    public string Properties { get; internal set; } = string.Empty;
    public int PropertyStringLength { get; internal set; }

    // Computed properties
    public ChunkHelper? ChunkHelper { get; set; }      // Only set if object node id is helper object
    public ChunkMesh? MeshData { get; set; }           // Only set if object node id is ChunkMesh

    /// <summary>Computed from material file. Not set for helper nodes, etc.</summary>
    public Material Materials { get; internal set; } = new();

    /// <summary>Name of the material library file.</summary>
    public string MaterialFileName { get; internal set; } = string.Empty;

    public Matrix4x4 LocalTransform => Matrix4x4.Transpose(Transform);

    private ChunkNode? _parentNode;
    public ChunkNode? ParentNode {
        get {
            if (ParentNodeID == ~0)  // aka 0xFFFFFFFF, or -1
                return null;

            if (_parentNode is null)
            {
                if (_model.ChunkMap.TryGetValue(ParentNodeID, out Chunk? node))
                    _parentNode = node as ChunkNode;
                else
                    _parentNode = _model.RootNode;
            }

            return _parentNode;
        }
        set {
            ParentNodeID = value == null ? ~0 : value.ID;
            _parentNode = value;
        }
    }

    public List<ChunkNode> Children { get; set; } = [];

    public override string ToString() => $@"Chunk Type: {ChunkType}, ID: {ID:X}, Version: {Version}, Name: {Name}, Object Node ID: {ObjectNodeID:X}, Parent Node ID: {ParentNodeID:X}, Mat: {MaterialID:X}";
}
