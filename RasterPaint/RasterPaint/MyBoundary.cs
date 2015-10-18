using System.Windows;

namespace RasterPaint
{
    public class MyBoundary
    {
        public double XMin { get; set; } = int.MaxValue;
        public double XMax { get; set; } = int.MinValue;
        public double YMin { get; set; } = int.MaxValue;
        public double YMax { get; set; } = int.MinValue;

        public MyBoundary()
        {
            
        }

        public MyBoundary(double x, double y)
        {
            XMin = XMax = x;
            YMin = YMax = y;
        }

        public MyBoundary(double xMin, double yMin, double xMax, double yMax)
        {
            XMin = xMin;
            XMax = xMax;
            YMin = yMin;
            YMax = yMax;
        }

        public void UpdateBoundary(double x, double y)
        {
            if (x <= XMin) XMin = x;
            if (x >= XMax) XMax = x;
            if (y <= YMin) YMin = y;
            if (y >= YMax) YMax = y;
        }

        public bool Contains(Point p)
        {
            if ((p.X > XMax) || (p.X < XMin))
            {
                return false;
            }

            return p.Y <= YMax && p.Y >= YMin;
        }
    }
}
