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
        Point _p1;
        Point _p2;

        public MyRectangle(Point p1, Point p2)
        {
            this._p1 = p1;
            this._p2 = p2;
        }

        public List<Point> FourPointsList()
        {
            Point pp1, pp2, pp3, pp4;

            if (_p1.X <= _p2.X)
            {
                if (_p1.Y <= _p2.Y)
                {
                    pp1 = _p1;
                    pp2 = new Point(_p1.X, _p2.Y);
                    pp3 = _p2;
                    pp4 = new Point(_p2.X, _p1.Y);
                }
                else // idzie w prawy, górny róg;
                {
                    pp1 = new Point(_p1.X, _p2.Y);
                    pp2 = _p1;
                    pp3 = new Point(_p2.X, _p1.Y);
                    pp4 = _p2;
                }
            }
            else
            {
                if (_p1.Y <= _p2.Y) // w lewo, w dół
                {
                    pp1 = new Point(_p2.X, _p1.Y);
                    pp2 = _p2;
                    pp3 = new Point(_p1.X, _p2.Y);
                    pp4 = _p1;
                }
                else
                {
                    pp1 = _p2;
                    pp2 = new Point(_p2.X, _p1.Y);
                    pp3 = _p1;
                    pp4 = new Point(_p1.X, _p2.Y);
                }
            }

            return new List<Point> { pp1, pp2, pp3, pp4 };
        } 
    }
}
