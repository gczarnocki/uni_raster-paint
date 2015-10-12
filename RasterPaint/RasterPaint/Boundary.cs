using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RasterPaint
{
    class Boundary
    {
        double x_min = int.MaxValue;
        double x_max = int.MinValue;
        double y_min = int.MaxValue;
        double y_max = int.MinValue;

        public void UpdateBoundary(double x, double y)
        {
            if (x <= x_min) x_min = x;
            if (x >= x_max) x_max = x;
            if (y <= y_min) y_min = y;
            if (y >= y_max) y_max = y;
        }

        public bool Contains(Point p)
        {
            if (p.X <= x_max && p.X >= x_min)
            {
                if (p.Y <= y_max && p.Y >= y_min)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
