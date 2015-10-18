using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            return new MyPoint(Point.X + v.X, Point.Y + v.Y) { Color = Color, Width = Width, MyBoundary = MyBoundary };
        }

        public override MyObject Clone()
        {
            return new MyPoint(Point) { Color = Color, Width = Width, MyBoundary = MyBoundary };
        }

        public override void UpdateBoundaries()
        {
            MyBoundary.Reset();
            MyBoundary.UpdateBoundary(Point.X, Point.Y);
        }

        public override void DrawObject(WriteableBitmap wb)
        {
            BitmapExtensions.DrawPoint(wb, Point, Color, Width);
        }

        public override void EraseObject(List<MyObject> list, WriteableBitmap wb, Color c)
        {
            BitmapExtensions.DrawPoint(wb, Point, c, Width);

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

        public void DrawAndAdd(WriteableBitmap wb, Point point, Color color, int width)
        {
            Point = point;
            Color = color;
            Width = width;
            MyBoundary = new MyBoundary(Point.X, Point.Y);
            BitmapExtensions.DrawPoint(wb, point, color, Width);
        }
    }
}