using System;
using System.Globalization;

namespace CgfConverter.CryEngineCore.Components
{
    // TODO:  Replace this with System.Numerics Vector3
    public class Vector3
    {
        private float _x;
        private float _y;
        private float _z;

        private ByteArray xBA = new ByteArray();
        private ByteArray yBA = new ByteArray();
        private ByteArray zBA = new ByteArray();

        public float x { get { return xBA.float1; } set { _x = value; xBA.float1 = value; } }
        public float y { get { return yBA.float1; } set { _y = value; yBA.float1 = value; } }
        public float z { get { return zBA.float1; } set { _z = value; zBA.float1 = value; } }

        public float X { get { return xBA.float1; } set { _x = value; xBA.float1 = value; } }
        public float Y { get { return yBA.float1; } set { _y = value; yBA.float1 = value; } }
        public float Z { get { return zBA.float1; } set { _z = value; zBA.float1 = value; } }

        public int xint { get { return xBA.int1; } set { xBA.int1 = value; } }
        public int yint { get { return yBA.int1; } set { yBA.int1 = value; } }
        public int zint { get { return zBA.int1; } set { zBA.int1 = value; } }

        public uint xuint { get { return xBA.uint1; } set { xBA.uint1 = value; } }
        public uint yuint { get { return yBA.uint1; } set { yBA.uint1 = value; } }
        public uint zuint { get { return zBA.uint1; } set { zBA.uint1 = value; } }

        public Vector3(double x, double y, double z)
        {
            this._x = (float)x;
            this._y = (float)y;
            this._z = (float)z;
        }

        public Vector3()
        {
        }

        public Vector3(Vector3 vector)
        {
            this._x = (float)vector.x;
            this._y = (float)vector.y;
            this._z = (float)vector.z;
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

                return hash;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj is Vector3 /*|| obj is Vec4*/)
                return this == (CryEngineCore.Components.Vector3)obj;

            return false;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "{0},{1},{2}", X, Y, Z);
        }
        #endregion

        #region Conversions

        public static implicit operator Vector3(Vector4 vec4)
        {
            return new Vector3(vec4.x, vec4.y, vec4.z);
        }
        #endregion

        #region Operators
        public static bool operator ==(Vector3 left, Vector3 right)
        {
            if ((object)right == null)
                return (object)left == null;

            return ((left.X == right.X) && (left.Y == right.Y) && (left.Z == right.Z));
        }

        public static bool operator !=(Vector3 left, Vector3 right)
        {
            return !(left == right);
        }
        #endregion

        #region Functions
        public bool IsZero(float epsilon = 0)
        {
            return (Math.Abs(x) <= epsilon) && (Math.Abs(y) <= epsilon) && (Math.Abs(z) <= epsilon);
        }
        public float Dot(Vector3 v)
        {
            return X * v.X + Y * v.Y + Z * v.Z;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets individual axes by index
        /// </summary>
        /// <param name="index">Index, 0 - 2 where 0 is X and 2 is Z</param>
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
                    default:
                        throw new ArgumentOutOfRangeException("index", "Indices must run from 0 to 2!");
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
                    default:
                        throw new ArgumentOutOfRangeException("index", "Indices must run from 0 to 2!");
                }
            }
        }
        #endregion

    }
}
