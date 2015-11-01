using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Windows.Shell;
using RasterPaint.Objects;

namespace RasterPaint
{
    /// <summary>
    /// The Cohen Sutherland line clipping algorithm
    /// http://en.wikipedia.org/wiki/Cohen%E2%80%93Sutherland_algorithm
    /// </summary>
    public static class CohenSutherland
    {
        /// <summary>
        /// Bitfields used to partition the space into 9 regiond
        /// </summary>
        private const byte Inside = 0; // 0000
        private const byte Left = 1;   // 0001
        private const byte Right = 2;  // 0010
        private const byte Bottom = 4; // 0100
        private const byte Top = 8;    // 1000

        /// <summary>
        /// Compute the bit code for a point (x, y) using the clip rectangle
        /// bounded diagonally by (xmin, ymin), and (xmax, ymax)
        /// ASSUME THAT xmax, xmin, ymax and ymin are global constants.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private static byte ComputeOutCode(MyBoundary boundary, double x, double y)
        {
            // initialised as being inside of clip window
            byte code = Inside;

            if (y < boundary.YMin)          // above the clip window
                code |= Top;
            else if (y > boundary.YMax)     // below the clip window
                code |= Bottom;
            if (x < boundary.XMin)          // to the left of clip window
                code |= Left;
            else if (x > boundary.XMax)     // to the right of clip window
                code |= Right;

            return code;
        }

        /// <summary>
        /// Cohen–Sutherland clipping algorithm clips a line from
        /// P0 = (x0, y0) to P1 = (x1, y1) against a rectangle with
        /// diagonal from (xmin, ymin) to (xmax, ymax).
        /// </summary>
        /// <param name="boundary"></param>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <returns>a list of two points in the resulting clipped line, or zero</returns>
        public static List<Point> CohenSutherlandLineClip(MyBoundary boundary, Point p0, Point p1)
        {
            var x0 = p0.X;
            var y0 = p0.Y;
            var x1 = p1.X;
            var y1 = p1.Y;

            // compute outcodes for P0, P1, and whatever point lies outside the clip rectangle
            var outcode0 = ComputeOutCode(boundary, x0, y0);
            var outcode1 = ComputeOutCode(boundary, x1, y1);
            var accept = false;

            while (true)
            {
                // Bitwise OR is 0. Trivially accept and get out of loop
                if ((outcode0 | outcode1) == 0)
                {
                    accept = true;
                    break;
                }
                // Bitwise AND is not 0. Trivially reject and get out of loop
                else if ((outcode0 & outcode1) != 0)
                {
                    break;
                }
                else
                {
                    // failed both tests, so calculate the line segment to clip
                    // from an outside point to an intersection with clip edge
                    double x, y;

                    // At least one endpoint is outside the clip rectangle; pick it.
                    byte outcodeOut = (outcode0 != 0) ? outcode0 : outcode1;

                    // Now find the intersection point;
                    // use formulas y = y0 + slope * (x - x0), x = x0 + (1 / slope) * (y - y0)
                    if ((outcodeOut & Top) != 0)
                    {   // point is above the clip rectangle
                        // x = x0 + (x1 - x0) * (boundary.YMax - y0) / (y1 - y0);
                        x = x0 + (x1 - x0) * (boundary.YMin - y0) / (y1 - y0);
                        y = boundary.YMin;
                    }
                    else if ((outcodeOut & Bottom) != 0)
                    { // point is below the clip rectangle
                      // x = x0 + (x1 - x0) * (boundary.YMin - y0) / (y1 - y0);
                        x = x0 + (x1 - x0) * (boundary.YMax - y0) / (y1 - y0);
                        y = boundary.YMax;
                    }
                    else if ((outcodeOut & Right) != 0)
                    {  // point is to the right of clip rectangle
                        y = y0 + (y1 - y0) * (boundary.XMax - x0) / (x1 - x0);
                        x = boundary.XMax;
                    }
                    else if ((outcodeOut & Left) != 0)
                    {   // point is to the left of clip rectangle
                        y = y0 + (y1 - y0) * (boundary.XMin - x0) / (x1 - x0);
                        x = boundary.XMin;
                    }
                    else
                    {
                        x = double.NaN;
                        y = double.NaN;
                    }

                    // Now we move outside point to intersection point to clip
                    // and get ready for next pass.
                    if (outcodeOut == outcode0)
                    {
                        x0 = x;
                        y0 = y;
                        outcode0 = ComputeOutCode(boundary, x0, y0);
                    }
                    else
                    {
                        x1 = x;
                        y1 = y;
                        outcode1 = ComputeOutCode(boundary, x1, y1);
                    }
                }
            }

            // return the clipped line
            return (accept) ? new List<Point>()
            {
            new Point(x0, y0),
            new Point(x1, y1),
            } : null;
        }
    }
}
