using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboDeck
{
    public struct IntPoint
    {
        public int X
        {
            get;
            private set;
        }
        public int Y
        {
            get;
            private set;
        }
        public IntPoint(int x, int y)
            : this()
        {
            this.X = x;
            this.Y = y;
        }
        public IntPoint(IntPoint p)
            : this()
        {
            this.X = p.X;
            this.Y = p.Y;
        }
        public double Length
        {
            get
            {
                return Math.Sqrt(SquaredLength);
            }
        }
        public int SquaredLength
        {
            get
            {
                return X * X + Y * Y;
            }
        }
        public IntPoint PlanarNormalize()
        {
            return new IntPoint(Math.Sign(X), Math.Sign(Y));
        }

        public IntPoint Rotate(IntPoint _vector, RoboDirection _direction)
        {
            switch (_direction)
            {
                case RoboDirection.Right:
                    return new IntPoint(_vector.Y, -_vector.X);
                case RoboDirection.Down:
                    return -_vector;
                case RoboDirection.Left:
                    return new IntPoint(-_vector.Y, _vector.X);
                case RoboDirection.Up:
                default:
                    return _vector;
            }
        }

        public static readonly IntPoint Zero = new IntPoint(0, 0);
        public static readonly IntPoint Up = new IntPoint(0, -1);
        public static readonly IntPoint Left = new IntPoint(-1, 0);
        public static readonly IntPoint Right = new IntPoint(1, 0);
        public static readonly IntPoint Down = new IntPoint(0, 1);
        public static readonly IntPoint UpLeft = new IntPoint(-1, -1);
        public static readonly IntPoint DownLeft = new IntPoint(-1, 1);
        public static readonly IntPoint UpRight = new IntPoint(1, -1);
        public static readonly IntPoint DownRight = new IntPoint(1, 1);

        public static readonly IntPoint[] Directions4 = new IntPoint[] { Up, Down, Right, Left };
        public static readonly IntPoint[] Directions5 = new IntPoint[] { Zero, Up, Down, Right, Left };
        public static readonly IntPoint[] Directions8 = new IntPoint[] { Up, Down, Right, Left, UpLeft, UpRight, DownLeft, DownRight };
        public static readonly IntPoint[] Directions9 = new IntPoint[] { Zero, Up, Down, Right, Left, UpLeft, UpRight, DownLeft, DownRight };

        public static IntPoint operator +(IntPoint A, IntPoint B)
        {
            return new IntPoint(A.X + B.X, A.Y + B.Y);
        }
        public static IntPoint operator -(IntPoint A, IntPoint B)
        {
            return new IntPoint(A.X - B.X, A.Y - B.Y);
        }
        public static IntPoint operator -(IntPoint A)
        {
            return new IntPoint(-A.X, -A.Y);
        }
        public static int DotProduct (IntPoint A, IntPoint B)
        {
            return A.X * B.X + A.Y * B.Y;
        }
        public static int CrossProduct (IntPoint A, IntPoint B)
        {
            return A.X * B.Y - B.X * A.Y;
        }
        public static IntPoint operator *(IntPoint A, int k)
        {
            return new IntPoint(A.X * k, A.Y * k);
        }
        public static IntPoint operator *(int k, IntPoint A)
        {
            return new IntPoint(A.X * k, A.Y * k);
        }
        public static IntPoint operator /(IntPoint A, int k)
        {
            return new IntPoint(A.X / k, A.Y / k);
        }

        public static FloatPoint operator +(FloatPoint A, IntPoint B)
        {
            return new FloatPoint(A.X + B.X, A.Y + B.Y);
        }
        public static FloatPoint operator +(IntPoint A, FloatPoint B)
        {
            return new FloatPoint(A.X + B.X, A.Y + B.Y);
        }
        public static FloatPoint operator -(FloatPoint A, IntPoint B)
        {
            return new FloatPoint(A.X - B.X, A.Y - B.Y);
        }
        public static FloatPoint operator -(IntPoint A, FloatPoint B)
        {
            return new FloatPoint(A.X - B.X, A.Y - B.Y);
        }
        public static FloatPoint operator *(IntPoint A, float k)
        {
            return new FloatPoint(A.X * k, A.Y * k);
        }
        public static FloatPoint operator *(float k, IntPoint A)
        {
            return new FloatPoint(A.X * k, A.Y * k);
        }
        public static FloatPoint operator /(IntPoint A, float k)
        {
            return new FloatPoint(A.X / k, A.Y / k);
        }

        public static bool operator !=(IntPoint A, IntPoint B)
        {
            return !(A == B);
        }
        public static bool operator ==(IntPoint A, IntPoint B)
        {
            if (System.Object.ReferenceEquals(A, B))
            {
                return true;
            }

            if (((object)A == null) || ((object)B == null))
            {
                return false;
            }

            return Eq(A, B);
        }
        private static bool Eq(IntPoint A, IntPoint B)
        {
            return A.X == B.X && A.Y == B.Y;
        }
        public override bool Equals(object obj)
        {
            if(!(obj is IntPoint))
            {
                return false;
            }
            var other = (IntPoint)obj;

            return Eq(this, other);
        }
        public override int GetHashCode()
        {
            return (new Tuple<int, int>(X, Y)).GetHashCode();
        }

        public override string ToString()
        {
            return String.Format("IntPoint: {0:f2}; {1:f2}", X, Y);
        }

        public static explicit operator Size(IntPoint p)
        {
            return new Size(p.X, p.Y);
        }
        public static explicit operator Point(IntPoint p)
        {
            return new Point(p.X, p.Y);
        }
        public static explicit operator IntPoint(Size s)
        {
            return new IntPoint(s.Width, s.Height);
        }
        public static explicit operator IntPoint(Point p)
        {
            return new IntPoint(p.X, p.Y);
        }

        public static implicit operator FloatPoint(IntPoint p)
        {
            return new FloatPoint(p.X, p.Y);
        }
    }
}
