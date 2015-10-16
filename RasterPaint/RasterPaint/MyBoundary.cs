using System.Windows;

namespace RasterPaint
{
    class MyBoundary
    {
        double _xMin = int.MaxValue;
        double _xMax = int.MinValue;
        double _yMin = int.MaxValue;
        double _yMax = int.MinValue;

        public void UpdateBoundary(double x, double y)
        {
            if (x <= _xMin) _xMin = x;
            if (x >= _xMax) _xMax = x;
            if (y <= _yMin) _yMin = y;
            if (y >= _yMax) _yMax = y;
        }

        public bool Contains(Point p)
        {
            if ((p.X > _xMax) || (p.X < _xMin))
            {
                return false;
            }

            return p.Y <= _yMax && p.Y >= _yMin;
        }
    }
}
