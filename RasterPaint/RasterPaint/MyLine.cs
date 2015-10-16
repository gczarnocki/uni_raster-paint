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
    }
}
