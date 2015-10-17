using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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

        public MyPoint()
        {
            Point = new Point(0, 0);
        }

        public override MyObject MoveObject(Vector v)
        {
            return new MyPoint(Point.X + v.X, Point.Y + v.Y);
        }

        public override MyObject Clone()
        {
            MyPoint clone = new MyPoint(Point) { Color = Color };

            return clone;
        }

        public override void UpdateBoundaries()
        {
            MyBoundary.UpdateBoundary(Point.X, Point.Y);
        }

        public override void DrawObject(WriteableBitmap wb, int width)
        {
            Width = width;
            BitmapExtensions.DrawPoint(wb, Point, Color, Width);
        }

        public override void EraseObject(List<MyObject> list, WriteableBitmap wb)
        {
            BitmapExtensions.DrawPoint(wb, Point, Colors.White, Width);

            if (list.Contains(this))
            {
                list.Remove(this);
            }
        }

        public override void HighlightObject(bool ifHighlight, WriteableBitmap wb)
        {
            Color c = ifHighlight ? Colors.Red : Color;

            BitmapExtensions.DrawPoint(wb, Point, c, Width);
        }

        public void DrawAndAdd(WriteableBitmap wb, Point point, Color value)
        {
            Point = point;
            Color = value;
            BitmapExtensions.DrawPoint(wb, point, value, Width);
        }
    }
}