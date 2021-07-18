using CgfConverter.Structs;
using Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace CgfConverter.CryEngineCore
{
    public abstract class ChunkNode : Chunk          // cccc000b:   Node
    {
        protected float VERTEX_SCALE = 1f / 100;

        public string Name { get; internal set; }
        public int ObjectNodeID { get; internal set; }
        public int ParentNodeID { get; internal set; }  // Parent nodeID
        public int __NumChildren { get; internal set; }
        public int MatID { get; internal set; }
        public bool IsGroupHead { get; internal set; }
        public bool IsGroupMember { get; internal set; }
        public Matrix4x4 Transform { get; internal set; }
        public Vector3 Pos { get; internal set; }
        public Quaternion Rot { get; internal set; }
        public Vector3 Scale { get; internal set; }
        public int PosCtrlID { get; internal set; }  // Obsolete
        public int RotCtrlID { get; internal set; }  // Obsolete
        public int SclCtrlID { get; internal set; }  // Obsolete
        public string Properties { get; internal set; }

        public Matrix4x4 LocalTransform; // Set after all chunks in model are read.

        public void SetLocalTransform()
        {
            if (ParentNode == null)
                LocalTransform = Transform;
            else
            {
                var newRotation = Matrix3x3.Transpose(ParentNode.Transform.GetRotation()) * Transform.GetRotation();
                var newTranslation = ParentNode.Transform.GetRotation() * (Transform.GetTranslation() - ParentNode.Transform.GetTranslation());

                LocalTransform = Matrix4x4Extensions.CreateTransformFromParts(newTranslation, newRotation);
            }
        }

        #region Calculated Properties
        private ChunkNode _parentNode;

        public ChunkNode ParentNode
        {
            get
            {
                if (ParentNodeID == ~0)  // aka 0xFFFFFFFF, or -1
                    return null;

                if (_parentNode == null)
                {
                    if (_model.ChunkMap.ContainsKey(ParentNodeID))
                        _parentNode = _model.ChunkMap[ParentNodeID] as ChunkNode;
                    else
                        _parentNode = _model.RootNode;
                }

                return _parentNode;
            }
            set
            {
                ParentNodeID = value == null ? ~0 : value.ID;
                _parentNode = value;
            }
        }

        private Chunk _objectChunk;
        
        public Chunk ObjectChunk
        {
            get
            {
                if ((_objectChunk == null) && _model.ChunkMap.ContainsKey(ObjectNodeID))
                {
                    _objectChunk = _model.ChunkMap[ObjectNodeID];
                }

                return _objectChunk;
            }
            set { _objectChunk = value; }
        }

        public List<ChunkNode> AllChildNodes
        {
            get
            {
                if (__NumChildren == 0)
                {
                    return null;
                }
                else
                {
                    var node = _model.NodeMap.Values.Where(a => a.ParentNodeID == ID).ToList();
                    return node;
                }
            }
        }
        #endregion

        public override string ToString()
        {
            return $@"Chunk Type: {ChunkType}, ID: {ID:X}, Version: {Version}, Name: {Name}, Object Node ID: {ObjectNodeID:X}, Parent Node ID: {ParentNodeID:X}";
        }
    }
}
