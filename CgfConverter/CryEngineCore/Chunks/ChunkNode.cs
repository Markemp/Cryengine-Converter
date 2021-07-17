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

        #region Calculated Properties
        public Matrix4x4 LocalTransform { get; set; }  = new Matrix4x4();            // Because Cryengine tends to store transform relative to world, we have to add all the transforms from the node to the root.  Calculated, row major.
        public Vector3 LocalTranslation { get; set; } = new Vector3();            // To hold the local rotation vector
        public Matrix3x3 LocalRotation { get; set; } = new Matrix3x3();             // to hold the local rotation matrix
        public Vector3 LocalScale { get; set; } = new Vector3();                  // to hold the local scale matrix

        private ChunkNode _parentNode;

        public ChunkNode ParentNode
        {
            get
            {
                // ~0 is shorthand for -1, or 0xFFFFFFFF in the uint world.
                if (ParentNodeID == ~0)  // aka 0xFFFFFFFF
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

        public Matrix4x4 TransformSoFar
        {
            get
            {
                if (ParentNode != null)
                {
                    return Transform * ParentNode.TransformSoFar;
                }
                else
                {
                    // TODO: What should this be?
                    return Transform;
                }
            }
        }

        public Matrix3x3 RotSoFar
        {
            get
            {
                if (ParentNode != null)
                {
                    return Transform.GetRotation().Mult(ParentNode.RotSoFar);
                }
                else
                {
                    return _model.RootNode.Transform.GetRotation();
                    // TODO: What should this be?
                }
            }
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
