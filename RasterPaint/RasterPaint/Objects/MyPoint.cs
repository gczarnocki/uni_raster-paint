using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RasterPaint.Objects
{
    public class MyPoint : MyObject
    {
        #region Constructors
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
        #endregion

        #region Methods
        public void DrawAndAdd(WriteableBitmap wb, Point point, Color color, int width)
        {
            Point = point;
            Color = color;
            Width = width;
            MyBoundary = new MyBoundary(Point.X, Point.Y);
            wb.DrawPoint(point, color, Width);
        }

        public override void DrawObject(WriteableBitmap wb)
        {
            wb.DrawPoint(Point, Color, Width);
        }

        public override void EraseObject(List<MyObject> list, WriteableBitmap wb, Color c)
        {
            wb.DrawPoint(Point, c, Width);

            if (list.Contains(this))
            {
                list.Remove(this);
            }
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

        public override void HighlightObject(bool ifHighlight, WriteableBitmap wb, Color c)
        {
            Color color = ifHighlight ? c : Color;

            wb.DrawPoint(Point, color, Width);
        }

        public override bool IfPointCloseToBoundary(Point p)
        {
            return Static.DistanceBetweenPoints(this.Point, p) <= Static.Distance;
        }

        #endregion
    }
}