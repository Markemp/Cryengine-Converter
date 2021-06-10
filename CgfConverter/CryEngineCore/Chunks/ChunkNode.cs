using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CgfConverter.CryEngineCore
{
    public abstract class ChunkNode : Chunk          // cccc000b:   Node
    {
        protected double VERTEX_SCALE = 1d / 100;
        #region Chunk Properties

        public string Name { get; internal set; }
        public int ObjectNodeID { get; internal set; }
        public int ParentNodeID { get; internal set; }  // Parent nodeID
        public int __NumChildren { get; internal set; }
        public int MatID { get; internal set; }
        public bool IsGroupHead { get; internal set; }
        public bool IsGroupMember { get; internal set; }
        public Matrix44 Transform { get; internal set; }
        public Vector3 Pos { get; internal set; }
        public Quaternion Rot { get; internal set; }
        public Vector3 Scale { get; internal set; }
        public int PosCtrlID { get; internal set; }  // Obsolete
        public int RotCtrlID { get; internal set; }  // Obsolete
        public int SclCtrlID { get; internal set; }  // Obsolete
        public string Properties { get; internal set; }

        #endregion

        #region Calculated Properties
        public Matrix44 LocalTransform { get; set; }  = new Matrix44();            // Because Cryengine tends to store transform relative to world, we have to add all the transforms from the node to the root.  Calculated, row major.
        public Vector3 LocalTranslation { get; set; } = new Vector3();            // To hold the local rotation vector
        public Matrix33 LocalRotation { get; set; } = new Matrix33();             // to hold the local rotation matrix
        public Vector3 LocalScale { get; set; } = new Vector3();                  // to hold the local scale matrix

        private ChunkNode _parentNode;

        public ChunkNode ParentNode
        {
            get
            {
                // Turns out chunk IDs are ints, not uints.  ~0 is shorthand for -1, or 0xFFFFFFFF in the uint world.
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

        public Matrix44 TransformSoFar
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

        public Matrix33 RotSoFar
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

        /// <summary>
        /// Gets the transform of the vertex.  This will be both the rotation and translation of this vertex, plus all the parents.
        /// The transform matrix is a 4x4 matrix.  Vector3 is a 3x1.  We need to convert vector3 to vector4, multiply the matrix, then convert back to vector3.
        /// </summary>
        /// <param name="transform"></param>
        /// <returns></returns>
        public Vector3 GetTransform(Vector3 transform)
        {
            // Apply the transforms (rotation and translation) to the vector.
            // Work on the single matrix
            Vector3 vec3 = TransformSoFar*transform;
            
            return vec3;
        }

        public override string ToString()
        {
            return $@"Chunk Type: {ChunkType}, ID: {ID:X}, Version: {Version}, Name: {Name}, Object Node ID: {ObjectNodeID:X}, Parent Node ID: {ParentNodeID:X}";
        }
    }
}
