using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RasterPaint
{
    internal class MyPoint : MyObject
    {
        public Point Point { get; set; }

        public MyPoint(Point p)
        {
            Point = p;
        }

        public MyPoint(double x, double y)
        {
            Point = new Point(x, y);
        }

        public override MyObject MoveObject(Vector v)
        {
            return new MyPoint(Point.X + v.X, Point.Y + v.Y);
        }
    }
}