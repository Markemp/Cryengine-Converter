using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CgfConverter.CryEngine_Core.Components
{
    public class Vector2
    {
        private float _x;
        private float _y;

        private ByteArray xBA = new ByteArray();
        private ByteArray yBA = new ByteArray();

        public float x { get { return xBA.float1; } set { _x = value; xBA.float1 = value; } }
        public float y { get { return yBA.float1; } set { _y = value; yBA.float1 = value; } }

        public float X { get { return xBA.float1; } set { _x = value; xBA.float1 = value; } }
        public float Y { get { return yBA.float1; } set { _y = value; yBA.float1 = value; } }

        public int xint { get { return xBA.int1; } set { xBA.int1 = value; } }
        public int yint { get { return yBA.int1; } set { yBA.int1 = value; } }

        public uint xuint { get { return xBA.uint1; } set { xBA.uint1 = value; } }
        public uint yuint { get { return yBA.uint1; } set { yBA.uint1 = value; } }

        public Vector2(double x, double y)
        {
            this._x = (float)x;
            this._y = (float)y;
        }

        public Vector2()
        {
        }

        public Vector2(Vector2 vector)
        {
            this._x = (float)vector.x;
            this._y = (float)vector.y;
        }


        #region Overrides
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;

                hash = hash * 23 + X.GetHashCode();
                hash = hash * 23 + Y.GetHashCode();

                return hash;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj is Vector2 /*|| obj is Vec4*/)
                return this == (Vector2)obj;

            return false;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "{0},{1}", X, Y);
        }
        #endregion

        #region Operators
        public static bool operator ==(Vector2 left, Vector2 right)
        {
            if ((object)right == null)
                return (object)left == null;

            return ((left.X == right.X) && (left.Y == right.Y));
        }

        public static bool operator !=(Vector2 left, Vector2 right)
        {
            return !(left == right);
        }
        #endregion

        #region Functions
        public bool IsZero(float epsilon = 0)
        {
            return (Math.Abs(x) <= epsilon) && (Math.Abs(y) <= epsilon);
        }
        public float Dot(Vector2 v)
        {
            return X * v.X + Y * v.Y;
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
                     default:
                        throw new ArgumentOutOfRangeException("index", "Indices must run from 0 to 1.");
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
                    default:
                        throw new ArgumentOutOfRangeException("index", "Indices must run from 0 to 1.");
                }
            }
        }
        #endregion

    }
}
