using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CgfConverter.CryEngine_Core
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
        /// Position Controller ID
        /// </summary>
        public int PosCtrlID { get; internal set; }
        /// <summary>
        /// Rotation Controller ID
        /// </summary>
        public int RotCtrlID { get; internal set; }
        /// <summary>
        /// Scalar Controller ID
        /// </summary>
        public int SclCtrlID { get; internal set; }
        /// <summary>
        /// Appears to be a Blob of properties, separated by new lines
        /// </summary>
        public String Properties { get; internal set; }

        #endregion

        #region Calculated Properties

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
                    return this.Transform.To3x3().Mult(this.ParentNode.RotSoFar);
                }
                else
                {
                    return this._model.RootNode.Transform.To3x3();
                    // TODO: What should this be?
                    // return this.Transform.To3x3();
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the transform of the vertex.  This will be both the rotation and translation of this vertex, plus all the parents.
        /// 
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

        public override void WriteChunk()
        {
            Utils.Log(LogLevelEnum.Verbose, "*** START Node Chunk ***");
            Utils.Log(LogLevelEnum.Verbose, "    ChunkType:           {0}", ChunkType);
            Utils.Log(LogLevelEnum.Verbose, "    Node ID:             {0:X}", ID);
            Utils.Log(LogLevelEnum.Verbose, "    Node Name:           {0}", Name);
            Utils.Log(LogLevelEnum.Verbose, "    Object ID:           {0:X}", ObjectNodeID);
            Utils.Log(LogLevelEnum.Verbose, "    Parent ID:           {0:X}", ParentNodeID);
            Utils.Log(LogLevelEnum.Verbose, "    Number of Children:  {0}", __NumChildren);
            Utils.Log(LogLevelEnum.Verbose, "    Material ID:         {0:X}", MatID); // 0x1 is mtllib w children, 0x10 is mtl no children, 0x18 is child
            Utils.Log(LogLevelEnum.Verbose, "    Position:            {0:F7}   {1:F7}   {2:F7}", Pos.x, Pos.y, Pos.z);
            Utils.Log(LogLevelEnum.Verbose, "    Scale:               {0:F7}   {1:F7}   {2:F7}", Scale.x, Scale.y, Scale.z);
            Utils.Log(LogLevelEnum.Verbose, "    Transformation:      {0:F7}  {1:F7}  {2:F7}  {3:F7}", Transform.m11, Transform.m12, Transform.m13, Transform.m14);
            Utils.Log(LogLevelEnum.Verbose, "                         {0:F7}  {1:F7}  {2:F7}  {3:F7}", Transform.m21, Transform.m22, Transform.m23, Transform.m24);
            Utils.Log(LogLevelEnum.Verbose, "                         {0:F7}  {1:F7}  {2:F7}  {3:F7}", Transform.m31, Transform.m32, Transform.m33, Transform.m34);
            Utils.Log(LogLevelEnum.Verbose, "                         {0:F7}  {1:F7}  {2:F7}  {3:F7}", Transform.m41 / 100, Transform.m42 / 100, Transform.m43 / 100, Transform.m44);
            Utils.Log(LogLevelEnum.Verbose, "    Transform_sum:       {0:F7}  {1:F7}  {2:F7}", TransformSoFar.x, TransformSoFar.y, TransformSoFar.z);
            Utils.Log(LogLevelEnum.Verbose, "    Rotation_sum:");
            this.RotSoFar.WriteMatrix33();
            Utils.Log(LogLevelEnum.Verbose, "*** END Node Chunk ***");
        }

        #endregion
    }
}
