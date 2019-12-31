using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CgfConverter.CryEngineCore
{
    public abstract class ChunkNode : Chunk          // cccc000b:   Node
    {
        #region Chunk Properties

        /// <summary>
        /// Chunk Name (String[64])
        /// </summary>
        public String Name { get; internal set; }
        /// <summary>
        /// Mesh or Helper Object ID
        /// </summary>
        public int ObjectNodeID { get; internal set; }
        /// <summary>
        /// Node parent.  if 0xFFFFFFFF, it's the top node.  Maybe...
        /// </summary>
        public int ParentNodeID { get; internal set; }  // Parent nodeID
        public int __NumChildren;
        /// <summary>
        /// Material ID for this chunk
        /// </summary>
        public int MatID { get; internal set; }
        public Boolean IsGroupHead { get; internal set; }
        public Boolean IsGroupMember { get; internal set; }
        /// <summary>
        /// Transformation Matrix
        /// </summary>
        public Matrix44 Transform { get; internal set; }
        /// <summary>
        /// Position vector of Transform
        /// </summary>
        public Vector3 Pos { get; internal set; }
        /// <summary>
        /// Rotation component of Transform
        /// </summary>
        public Quat Rot { get; internal set; }
        /// <summary>
        /// Scalar component of Transform
        /// </summary>
        public Vector3 Scale { get; internal set; }
        /// <summary>
        /// Position Controller ID - Obsolete
        /// </summary>
        public int PosCtrlID { get; internal set; }
        /// <summary>
        /// Rotation Controller ID - Obsolete
        /// </summary>
        public int RotCtrlID { get; internal set; }
        /// <summary>
        /// Scalar Controller ID - Obsolete
        /// </summary>
        public int SclCtrlID { get; internal set; }
        /// <summary>
        /// Appears to be a Blob of properties, separated by new lines
        /// </summary>
        public String Properties { get; internal set; }

        #endregion

        #region Calculated Properties
        public Matrix44 LocalTransform = new Matrix44();            // Because Cryengine tends to store transform relative to world, we have to add all the transforms from the node to the root.  Calculated, row major.
        public Vector3 LocalTranslation = new Vector3();            // To hold the local rotation vector
        public Matrix33 LocalRotation = new Matrix33();             // to hold the local rotation matrix
        public Vector3 LocalScale = new Vector3();                  // to hold the local scale matrix

        private ChunkNode _parentNode;

        public ChunkNode ParentNode
        {
            get
            {
                // Turns out chunk IDs are ints, not uints.  ~0 is shorthand for -1, or 0xFFFFFFFF in the uint world.
                //if (this.ParentNodeID == 0xFFFFFFFF)
                if (this.ParentNodeID == ~0)
                    return null;

                if (this._parentNode == null)
                {
                    if (this._model.ChunkMap.ContainsKey(this.ParentNodeID))
                        this._parentNode = this._model.ChunkMap[this.ParentNodeID] as ChunkNode;
                    else
                        this._parentNode = this._model.RootNode;
                }

                return this._parentNode;
            }
            set
            {
                //this.ParentNodeID = value == null ? 0xFFFFFFFF : value.ID;
                this.ParentNodeID = value == null ? ~0 : value.ID;
                this._parentNode = value;
            }
        }

        public List<ChunkNode> ChildNodes { get; set; }

        private Chunk _objectChunk;
        public Chunk ObjectChunk
        {
            get
            {
                if ((this._objectChunk == null) && this._model.ChunkMap.ContainsKey(this.ObjectNodeID))
                {
                    this._objectChunk = this._model.ChunkMap[this.ObjectNodeID];
                }

                return this._objectChunk;
            }
            set { this._objectChunk = value; }
        }

        public Vector3 TransformSoFar
        {
            get
            {
                if (this.ParentNode != null)
                {
                    return this.ParentNode.TransformSoFar.Add(this.Transform.GetTranslation());
                }
                else
                {
                    // TODO: What should this be?
                    // return this._model.RootNode.Transform.GetTranslation();
                    return this.Transform.GetTranslation();
                }
            }
        }

        public Matrix33 RotSoFar
        {
            get
            {
                if (this.ParentNode != null)
                {
                    return this.Transform.GetRotation().Mult(this.ParentNode.RotSoFar);
                }
                else
                {
                    return this._model.RootNode.Transform.GetRotation();
                    // TODO: What should this be?
                    // return this.Transform.To3x3();
                }
            }
        }

        public List<ChunkNode> AllChildNodes
        {
            get
            {
                if (this.__NumChildren == 0)
                {
                    return null;
                }
                else
                {
                    var node = this._model.NodeMap.Values.Where(a => a.ParentNodeID == this.ID).ToList();
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
            Vector3 vec3 = transform;

            // if (this.id != 0xFFFFFFFF)
            // {

            // Apply the local transforms (rotation and translation) to the vector
            // Do rotations.  Rotations must come first, then translate.
            vec3 = this.RotSoFar.Mult3x1(vec3);
            // Do translations.  I think this is right.  Objects in right place, not rotated right.
            vec3 = vec3.Add(this.TransformSoFar);
            //}

            return vec3;
        }

        public override void Read(BinaryReader b)
        {
            base.Read(b);

            // Read the Name string
            this.Name = b.ReadFString(64);
            this.ObjectNodeID = b.ReadInt32(); // Object reference ID
            this.ParentNodeID = b.ReadInt32();
            this.__NumChildren = b.ReadInt32();
            this.MatID = b.ReadInt32();  // Material ID?
            this.SkipBytes(b, 4);

            // Read the 4x4 transform matrix.  Should do a couple of for loops, but data structures...
            this.Transform = new Matrix44
            {
                m11 = b.ReadSingle(),
                m12 = b.ReadSingle(),
                m13 = b.ReadSingle(),
                m14 = b.ReadSingle(),
                m21 = b.ReadSingle(),
                m22 = b.ReadSingle(),
                m23 = b.ReadSingle(),
                m24 = b.ReadSingle(),
                m31 = b.ReadSingle(),
                m32 = b.ReadSingle(),
                m33 = b.ReadSingle(),
                m34 = b.ReadSingle(),
                m41 = b.ReadSingle(),
                m42 = b.ReadSingle(),
                m43 = b.ReadSingle(),
                m44 = b.ReadSingle(),
            };

            // Read the position Pos Vector3
            this.Pos = new Vector3
            {
                x = b.ReadSingle() / 100,
                y = b.ReadSingle() / 100,
                z = b.ReadSingle() / 100,
            };

            // Read the rotation Rot Quad
            this.Rot = new Quat
            {
                w = b.ReadSingle(),
                x = b.ReadSingle(),
                y = b.ReadSingle(),
                z = b.ReadSingle(),
            };

            // Read the Scale Vector 3
            this.Scale = new Vector3
            {
                x = b.ReadSingle(),
                y = b.ReadSingle(),
                z = b.ReadSingle(),
            };

            // read the controller pos/rot/scale
            this.PosCtrlID = b.ReadInt32();
            this.RotCtrlID = b.ReadInt32();
            this.SclCtrlID = b.ReadInt32();

            this.Properties = b.ReadPString();
            // Good enough for now.
        }

        public override string ToString()
        {
            return $@"Chunk Type: {ChunkType}, ID: {ID:X}, Version: {Version}, Name: {Name}, Object Node ID: {ObjectNodeID}, Parent Node ID: {ParentNodeID}";
        }
    }
}
