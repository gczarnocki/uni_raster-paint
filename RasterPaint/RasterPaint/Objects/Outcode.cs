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
    }
}
