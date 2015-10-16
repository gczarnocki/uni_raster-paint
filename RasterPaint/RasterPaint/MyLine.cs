using System.Windows;

namespace RasterPaint
{
    class MyLine : MyObject
    {
        public Point StartPoint { get; set; }
        public Point EndPoint { get; set; }

        public MyLine(Point startPoint, Point endPoint)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
        }

        public MyLine()
        {
            StartPoint = EndPoint = new Point(0, 0);
        }

        public override MyObject MoveObject(Vector v)
        {
            return new MyLine(new Point(StartPoint.X + v.X, StartPoint.Y + v.Y), new Point(EndPoint.X + v.X, EndPoint.Y + v.Y));
        }
    }
}
