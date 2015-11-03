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
            Point pp1, pp2, pp3, pp4;

            if (p1.X <= p2.X)
            {
                if (p1.Y <= p2.Y)
                {
                    pp1 = p1;
                    pp2 = new Point(p1.X, p2.Y);
                    pp3 = p2;
                    pp4 = new Point(p2.X, p1.Y);
                }
                else // idzie w prawy, górny róg;
                {
                    pp1 = new Point(p1.X, p2.Y);
                    pp2 = p1;
                    pp3 = new Point(p2.X, p1.Y);
                    pp4 = p2;
                }
            }
            else
            {
                if (p1.Y <= p2.Y) // w lewo, w dół
                {
                    pp1 = new Point(p2.X, p1.Y);
                    pp2 = p2;
                    pp3 = new Point(p1.X, p2.Y);
                    pp4 = p1;
                }
                else
                {
                    pp1 = p2;
                    pp2 = new Point(p2.X, p1.Y);
                    pp3 = p1;
                    pp4 = new Point(p1.X, p2.Y);
                }
            }

            return new List<Point> { pp1, pp2, pp3, pp4 };
        } 
    }
}
