using System;
using System.Globalization;

namespace CgfConverter.CryEngineCore.Components
{
    /// <summary>
    /// A 4 value vector (x, y, z, w).  Uses some of Dymek's code to solve Normals for SC models.
    /// </summary>
    public class Vector4
    {
        private float _x;
        private float _y;
        private float _z;
        private float _w;

        private ByteArray xBA = new ByteArray();
        private ByteArray yBA = new ByteArray();
        private ByteArray zBA = new ByteArray();
        private ByteArray wBA = new ByteArray();

        public float x { get { return xBA.float1; } set { _x = value; xBA.float1 = value; } }
        public float y { get { return yBA.float1; } set { _y = value; yBA.float1 = value; } }
        public float z { get { return zBA.float1; } set { _z = value; zBA.float1 = value; } }
        public float w { get { return wBA.float1; } set { _w = value; wBA.float1 = value; } }

        public float X { get { return xBA.float1; } set { _x = value; xBA.float1 = value; } }
        public float Y { get { return yBA.float1; } set { _y = value; yBA.float1 = value; } }
        public float Z { get { return zBA.float1; } set { _z = value; zBA.float1 = value; } }
        public float W { get { return wBA.float1; } set { _w = value; wBA.float1 = value; } }

        public int xint { get { return xBA.int1; } set { xBA.int1 = value; } }
        public int yint { get { return yBA.int1; } set { yBA.int1 = value; } }
        public int zint { get { return zBA.int1; } set { zBA.int1 = value; } }
        public int wint { get { return wBA.int1; } set { wBA.int1 = value; } }

        public uint xuint { get { return xBA.uint1; } set { xBA.uint1 = value; } }
        public uint yuint { get { return yBA.uint1; } set { yBA.uint1 = value; } }
        public uint zuint { get { return zBA.uint1; } set { zBA.uint1 = value; } }
        public uint wuint { get { return wBA.uint1; } set { wBA.uint1 = value; } }

        public Vector4(double x, double y, double z, double w)
        {
            this._x = (float)x;
            this._y = (float)y;
            this._z = (float)z;
            this._w = (float)w;
        }

        public Vector3 ToVector3()
        {
            Vector3 result = new Vector3();
            if (_w == 0)
            {
                result.x = (float)_x;
                result.y = (float)_y;
                result.z = (float)_z;
            }
            else
            {
                result.x = (float)_x / _w;
                result.y = (float)_y / _w;
                result.z = (float)_z / _w;
            }
            return result;
        }

        public void WriteVector4()
        {
            Utils.Log(LogLevelEnum.Debug, "=============================================");
            Utils.Log(LogLevelEnum.Debug, "x:{0:F7}  y:{1:F7}  z:{2:F7} w:{3:F7}", _x, _y, _z, _w);
        }

        #region Overrides
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;

                hash = hash * 23 + X.GetHashCode();
                hash = hash * 23 + Y.GetHashCode();
                hash = hash * 23 + Z.GetHashCode();
                hash = hash * 23 + W.GetHashCode();

                return hash;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj is Vector4 /*|| obj is Vec4*/)
                return this == (Vector4)obj;

            return false;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "{0},{1},{2},{3}", X, Y, Z, W);
        }
        #endregion

        #region Conversions

        public static implicit operator Vector4(Vector3 vec3)
        {
            return new Vector4(vec3.x, vec3.y, vec3.z, 0);
        }
        #endregion

        #region Operators
        public static bool operator ==(Vector4 left, Vector4 right)
        {
            if ((object)right == null)
                return (object)left == null;

            return ((left.X == right.X) && (left.Y == right.Y) && (left.Z == right.Z) && (left.W == right.W));
        }

        public static bool operator !=(Vector4 left, Vector4 right)
        {
            return !(left == right);
        }
        #endregion

        #region Functions
        public bool IsZero(float epsilon = 0)
        {
            return (Math.Abs(x) <= epsilon) && (Math.Abs(y) <= epsilon) && (Math.Abs(z) <= epsilon) && (Math.Abs(w) <= epsilon);
        }
        public float Dot(Vector4 v)
        {
            return X * v.X + Y * v.Y + Z * v.Z + W * v.W;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets individual axes by index
        /// </summary>
        /// <param name="index">Index, 0 - 3 where 0 is X and 3 is W</param>
        /// <returns>The axis value</returns>
        public float this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return x;
                    case 1:
                        return y;
                    case 2:
                        return z;
                    case 3:
                        return W;

                    default:
                        throw new ArgumentOutOfRangeException("index", "Indices must run from 0 to 3!");
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        x = value;
                        break;
                    case 1:
                        y = value;
                        break;
                    case 2:
                        z = value;
                        break;
                    case 3:
                        W = value;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException("index", "Indices must run from 0 to 3!");
                }
            }
        }
        #endregion
    }
}
