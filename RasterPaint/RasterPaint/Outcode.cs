using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RasterPaint
{
    public struct Outcode
    {
        private uint _all, _left, _right, _bottom, _top;

        public Outcode CompOutcode(float x, float y, float xmin, float xmax, float ymin, float ymax)
        {
            Outcode code;

            code._all = 0;
            code._top = code._bottom = code._left = code._right = 0;

            if (y > ymax)
            {
                code._top = 1;
                code._all += 8;
            }

            if (y < ymin)
            {
                code._bottom = 1;
                code._all += 4;
            }

            if (x > xmax)
            {
                code._right = 1;
                code._all += 2;
            }

            if (x < xmin)
            {
                code._left = 1;
                code._all += 1;
            }

            return code;
        }

        //void CohenSutherlandLineClip(float x1, float y1, float x2, float y2,
        //    float xmin, float xmax, float ymin, float ymax)
        //{
        //    Outcode outcodeOUT;
        //    float x, y;

        //    bool accept = false;
        //    bool done = false;

        //    Outcode outcode1 = CompOutcode(x1, y1, xmin, xmax, ymin, ymax);
        //    Outcode outcode2 = CompOutcode(x2, y2, xmin, xmax, ymin, ymax);

        //    do
        //    {
        //        if ((outcode1._all | outcode2._all) == 0) // trivially accepted;
        //        {
        //            accept = true;
        //            done = true;
        //        }
        //        else if((outcode1._all & outcode2._all) != 0)
        //        {
        //            accept = false;
        //            done = true;
        //        }
        //        else
        //        {
        //            // which point is outside of rectangle?

        //            outcodeOUT = outcode1._all != 0 ? outcode1 : outcode2;

        //            if (outcodeOUT._top == 1) // divide line at top of clip rectangle;
        //            {
        //                x = x1 + (x2 - x1)*(ymax - y1)/(y2 - y1);
        //                y = ymax;
        //            }
        //            else if (outcodeOUT._bottom == 1)
        //            {
                        
        //            }
        //            else if(outcodeOUT.)
        //        }
        //    } while (!done);

        //    if (accept)
        //    {
        //        // DrawLine(...);
        //    }
        //}
    }
}
