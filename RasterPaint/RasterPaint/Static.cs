using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using RasterPaint.Objects;

namespace RasterPaint
{
    public static class Static
    {
        public static double DistanceBetweenPoints(Point a, Point b)
        {
            return Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
        }

        public static double DistanceBetweenLineAndPoint(MyLine ml, Point p)
        {
            Point p1 = ml.StartPoint;
            Point p2 = ml.EndPoint;

            return Math.Abs((p2.Y - p1.Y)*p.X - (p2.X - p1.X)*p.Y + p2.X*p1.Y - p2.Y*p1.X)
                   /Math.Sqrt((p2.Y - p1.Y)*(p2.Y - p1.Y) + (p2.X - p1.X)*(p2.X - p1.X));
        }

        public static double Distance = 5.0F;

        public static void Swap<T>(ref T first, ref T second)
        {
            var t = first;
            first = second;
            second = t;
        }
    }
}
