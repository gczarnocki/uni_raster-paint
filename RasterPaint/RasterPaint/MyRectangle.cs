using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RasterPaint
{
    class MyRectangle
    {
        Point p1;
        Point p2;

        public MyRectangle(Point p1, Point p2)
        {
            this.p1 = p1;
            this.p2 = p2;
        }

        public List<Point> TwoPointsList()
        {
            return new List<Point> { p1, p2 };
        }

        public List<Point> FourPointsList()
        {
            List<Point> returnList = new List<Point>();

            returnList.Add(p1);
            returnList.Add(new Point(p1.X + p2.X, p1.Y));
            returnList.Add(p2);
            returnList.Add(new Point(p1.X, p1.Y + p2.Y));

            return returnList;
        }
    }
}
