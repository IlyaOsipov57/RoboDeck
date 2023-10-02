using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboDeck
{
    public struct FloatPoint
    {
        public float X
        {
            get;
            private set;
        }
        public float Y
        {
            get;
            private set;
        }
        public FloatPoint(float x, float y)
            : this()
        {
            this.X = x;
            this.Y = y;
        }
        public FloatPoint(IntPoint p)
            : this()
        {
            this.X = p.X;
            this.Y = p.Y;
        }

        public static FloatPoint operator +(FloatPoint A, FloatPoint B)
        {
            return new FloatPoint(A.X + B.X, A.Y + B.Y);
        }
        public static FloatPoint operator -(FloatPoint A, FloatPoint B)
        {
            return new FloatPoint(A.X - B.X, A.Y - B.Y);
        }
        public static FloatPoint operator -(FloatPoint A)
        {
            return new FloatPoint(-A.X, -A.Y);
        }
        public static float DotProduct(FloatPoint A, FloatPoint B)
        {
            return A.X * B.X + A.Y * B.Y;
        }
        public static float CrossProduct(FloatPoint A, FloatPoint B)
        {
            return A.X * B.Y - B.X * A.Y;
        }
        public static FloatPoint operator *(FloatPoint A, float k)
        {
            return new FloatPoint(A.X * k, A.Y * k);
        }
        public static FloatPoint operator *(float k, FloatPoint A)
        {
            return new FloatPoint(A.X * k, A.Y * k);
        }
        public static FloatPoint operator /(FloatPoint A, float k)
        {
            return new FloatPoint(A.X / k, A.Y / k);
        }

        public override string ToString()
        {
            return String.Format("IntPoint: {0:f2}; {1:f2}", X, Y);
        }

        public static explicit operator Point(FloatPoint p)
        {
            return new Point((int)p.X, (int)p.Y);
        }
    }
}
