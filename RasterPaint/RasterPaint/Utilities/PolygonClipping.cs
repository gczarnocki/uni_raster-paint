using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using RasterPaint.Objects;

namespace RasterPaint.Utilities
{
    public static class PolygonClipping
    {
        #region Edge
        /// <summary>
        /// This represents a line segment
        /// </summary>
        private class Edge
        {
            public Edge(Point from, Point to)
            {
                this.From = from;
                this.To = to;
            }

            public readonly Point From;
            public readonly Point To;
        }
        #endregion

        /// <summary>
        /// Bitfields used to partition the space into 9 regiond
        /// </summary>
        private const byte Inside = 0; // 0000
        private const byte Left = 1;   // 0001
        private const byte Right = 2;  // 0010
        private const byte Bottom = 4; // 0100
        private const byte Top = 8;    // 1000

        /// <summary>
        /// Compute the bit code for a point (x, y) using the clip rectangle bounded diagonally.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>Returns CompOut Code.</returns>
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
        /// Cohen–Sutherland clipping algorithm clips a line from P0 = (x0, y0) to P1 = (x1, y1) against a rectangle.
        /// </summary>
        /// <param name="boundary"></param>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <returns>Returns a list of two points in the resulting clipped line, or zero.</returns>
        public static List<Point> CohenSutherlandLineClip(MyBoundary boundary, Point p0, Point p1)
        {
            var x0 = p0.X;
            var y0 = p0.Y;
            var x1 = p1.X;
            var y1 = p1.Y;
            
            var outcode0 = ComputeOutCode(boundary, x0, y0);
            var outcode1 = ComputeOutCode(boundary, x1, y1);
            var accept = false;

            while (true)
            {
                // Bitwise OR is 0. Trivially accept and get out of loop;
                if ((outcode0 | outcode1) == 0)
                {
                    accept = true;
                    break;
                }
                // Bitwise AND is not 0. Trivially reject and get out of loop;
                else if ((outcode0 & outcode1) != 0)
                {
                    break;
                }
                else
                {
                    double x, y;

                    byte outcodeOut = (outcode0 != 0) ? outcode0 : outcode1; // At least one endpoint is outside the clip rectangle; pick it.

                    // Now find the intersection point;

                    if ((outcodeOut & Top) != 0)
                    {
                        x = x0 + (x1 - x0) * (boundary.YMin - y0) / (y1 - y0);
                        y = boundary.YMin;
                    }
                    else if ((outcodeOut & Bottom) != 0)
                    {
                        x = x0 + (x1 - x0) * (boundary.YMax - y0) / (y1 - y0);
                        y = boundary.YMax;
                    }
                    else if ((outcodeOut & Right) != 0)
                    {
                        y = y0 + (y1 - y0) * (boundary.XMax - x0) / (x1 - x0);
                        x = boundary.XMax;
                    }
                    else if ((outcodeOut & Left) != 0)
                    {  
                        y = y0 + (y1 - y0) * (boundary.XMin - x0) / (x1 - x0);
                        x = boundary.XMin;
                    }
                    else
                    {
                        x = double.NaN;
                        y = double.NaN;
                    }

                    // Now we move outside point to intersection point to clip and get ready for next pass.

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

            return (accept) ? new List<Point>()
            {
            new Point(x0, y0),
            new Point(x1, y1),
            } : null;
        }

        /// <summary>
        /// This clips the subject polygon against the clip polygon (gets the intersection of the two polygons).
        /// </summary>
        /// <remarks>
        /// Based on the psuedocode from: http://en.wikipedia.org/wiki/Sutherland%E2%80%93Hodgman
        /// </remarks>
        /// <param name="subjectPoly">Can be concave or convex.</param>
        /// <param name="clipPoly">Must be convex.</param>
        /// <returns>The intersection of the two polygons (or null).</returns>
        public static Point[] GetIntersectedPolygon(Point[] subjectPoly, Point[] clipPoly)
        {
            if (subjectPoly.Length < 3 || clipPoly.Length < 3)
            {
                throw new ArgumentException($"The polygons passed in must have at least 3 points: Subject: {subjectPoly.Length}, Clip: {clipPoly.Length}");
            }

            List<Point> outputList = subjectPoly.ToList();

            // Make sure it's clockwise;
            if (!IsClockwise(subjectPoly))
            {
                outputList.Reverse();
            }

            // Walk around the clip polygon clockwise;
            foreach (Edge clipEdge in IterateEdgesClockwise(clipPoly))
            {
                List<Point> inputList = outputList.ToList(); //	clone it;
                outputList.Clear();

                if (inputList.Count == 0)
                {
                    break;
                }

                Point s = inputList[inputList.Count - 1];

                foreach (Point e in inputList)
                {
                    if (IsInside(clipEdge, e))
                    {
                        if (!IsInside(clipEdge, s))
                        {
                            Point? point = GetIntersect(s, e, clipEdge.From, clipEdge.To);

                            if (point == null)
                            {
                                throw new ApplicationException("Line segments don't intersect"); //	may be colinear, or may be a bug;
                            }
                            else
                            {
                                outputList.Add(point.Value);
                            }
                        }

                        outputList.Add(e);
                    }
                    else if (IsInside(clipEdge, s))
                    {
                        Point? point = GetIntersect(s, e, clipEdge.From, clipEdge.To);
                        if (point == null)
                        {
                            throw new ApplicationException("Line segments don't intersect");
                            // may be colinear, or may be a bug;
                        }
                        else
                        {
                            outputList.Add(point.Value);
                        }
                    }

                    s = e;
                }
            }

            return outputList.ToArray();
        }

        private static IEnumerable<Edge> IterateEdgesClockwise(Point[] polygon)
        {
            if (IsClockwise(polygon))
            {
                #region Already clockwise

                for (int cntr = 0; cntr < polygon.Length - 1; cntr++)
                {
                    yield return new Edge(polygon[cntr], polygon[cntr + 1]);
                }

                yield return new Edge(polygon[polygon.Length - 1], polygon[0]);

                #endregion
            }
            else
            {
                #region Reverse

                for (int cntr = polygon.Length - 1; cntr > 0; cntr--)
                {
                    yield return new Edge(polygon[cntr], polygon[cntr - 1]);
                }

                yield return new Edge(polygon[0], polygon[polygon.Length - 1]);

                #endregion
            }
        }

        /// <summary>
        /// Returns the intersection of the two lines (line segments are passed in, but they are treated like infinite lines).
        /// </summary>
        /// <remarks>
        /// Got this here:
        /// http://stackoverflow.com/questions/14480124/how-do-i-detect-triangle-and-rectangle-intersection
        /// </remarks>
        private static Point? GetIntersect(Point line1From, Point line1To, Point line2From, Point line2To)
        {
            Vector direction1 = line1To - line1From;
            Vector direction2 = line2To - line2From;
            double dotPerp = (direction1.X * direction2.Y) - (direction1.Y * direction2.X);

            if (IsNearZero(dotPerp))
            {
                return null;
            }

            Vector c = line2From - line1From;
            double t = (c.X * direction2.Y - c.Y * direction2.X) / dotPerp;

            return line1From + (t * direction1);
        }

        private static bool IsInside(Edge edge, Point test)
        {
            bool? isLeft = IsLeftOf(edge, test);
            if (isLeft == null)
            {
                return true; //	Colinear points should be considered inside
            }

            return !isLeft.Value;
        }

        private static bool IsClockwise(Point[] polygon)
        {
            for (int cntr = 2; cntr < polygon.Length; cntr++)
            {
                bool? isLeft = IsLeftOf(new Edge(polygon[0], polygon[1]), polygon[cntr]);
                if (isLeft != null)		//	some of the points may be colinear.  That's ok as long as the overall is a polygon
                {
                    return !isLeft.Value;
                }
            }

            throw new ArgumentException("All the points in the polygon are colinear");
        }

        private static bool? IsLeftOf(Edge edge, Point test)
        {
            Vector tmp1 = edge.To - edge.From;
            Vector tmp2 = test - edge.To;

            double x = (tmp1.X * tmp2.Y) - (tmp1.Y * tmp2.X); // dot product of perpendicular?

            if (x < 0)
            {
                return false;
            }
            else if (x > 0)
            {
                return true;
            }
            else
            {
                // Colinear points;
                return null;
            }
        }

        private static bool IsNearZero(double testValue)
        {
            return Math.Abs(testValue) <= .000000001d;
        }
    }
}
