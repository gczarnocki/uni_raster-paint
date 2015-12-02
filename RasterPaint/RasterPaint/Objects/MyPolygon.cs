using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace RasterPaint.Objects
{
    public class MyPolygon : MyObject
    {
        #region Fields and Properties
        [NonSerialized]
        private WriteableBitmap _fillBitmap;
        private WriteableBitmap _initialBitmap;

        [XmlArray]
        public List<MyLine> LinesList = new List<MyLine>();

        public Color FillColor { get; set; }

        public WriteableBitmap FillBitmap
        {
            get { return _fillBitmap; }
            set { _fillBitmap = value; }
        }

        public WriteableBitmap InitialBitmap
        {
            get { return _initialBitmap; }

            set
            {
                _initialBitmap = value;
                // Trace.WriteLine("Changed");
            }
        }

        public Point[] GetPointsArray => LinesList.Select(x => x.StartPoint).ToArray();
        #endregion

        #region Methods
        public override void DrawObject(WriteableBitmap wb)
        {
            foreach (var item in LinesList)
            {
                wb.DrawLine(item.StartPoint, item.EndPoint, Color, Width);
            }

            FillPolygonScanLine(true, wb, FillColor);
        }

        public override void EraseObject(List<MyObject> list, WriteableBitmap wb, Color c)
        {
            if (list.Contains(this))
            {
                if (IfToFillWithImage)
                {
                    wb.Clear(c);
                }
                else
                {
                    FillPolygonScanLine(false, wb, c);

                    foreach (var item in LinesList)
                    {
                        wb.DrawLine(item.StartPoint, item.EndPoint, c, Width);
                    }
                }

                list.Remove(this);

                if (IfToFillWithImage)
                {
                    foreach (var item in list)
                    {
                        item.DrawObject(wb);
                    }
                }
            }
        }

        public void DrawAndAddLine(WriteableBitmap wb, MyLine ml, Color c)
        {
            Color = c;
            AddLine(ml);

            wb.DrawLine(ml.StartPoint, ml.EndPoint, c, Width);
        }

        private void AddLine(MyLine ml)
        {
            if (!ml.Equals(null) && !LinesList.Contains(ml))
            {
                LinesList.Add(ml);
                UpdateBoundaries();
            }
        }

        public override MyObject Clone()
        {
            var clone = new MyPolygon
            {
                Color = Color,
                Width = Width,
                MyBoundary = MyBoundary,
                FillBitmap = FillBitmap,
                FillColor = FillColor,
                InitialBitmap = InitialBitmap
            };

            foreach (var item in LinesList)
            {
                clone.AddLine(item);
            }

            return clone;
        }

        public override MyObject MoveObject(Vector v)
        {
            var mo = new MyPolygon
            {
                Color = Color,
                Width = Width,
                MyBoundary = MyBoundary,
                FillColor = FillColor,
                FillBitmap = FillBitmap
            };

            foreach (var item in LinesList)
            {
                var newStartPoint = new Point(item.StartPoint.X + v.X, item.StartPoint.Y + v.Y);
                var newEndPoint = new Point(item.EndPoint.X + v.X, item.EndPoint.Y + v.Y);
                mo.AddLine(new MyLine(newStartPoint, newEndPoint));
            }

            return mo;
        }

        public override void UpdateBoundaries()
        {
            MyBoundary.Reset();

            foreach (var ml in LinesList)
            {
                MyBoundary.UpdateBoundary(ml.StartPoint.X, ml.StartPoint.Y);
                MyBoundary.UpdateBoundary(ml.EndPoint.X, ml.EndPoint.Y);
            }
        }

        public override void HighlightObject(bool ifToHighlight, WriteableBitmap wb, Color c)
        {
            var color = ifToHighlight ? c : Color;

            foreach (var item in LinesList)
            {
                wb.DrawLine(item.StartPoint, item.EndPoint, color, Width);
            }
        }

        public bool IfToFillWithImage => FillBitmap != null;

        private void FillPolygonScanLine(bool ifToFill, WriteableBitmap wb, Color c)
        {
            FillColor = c;

            var ySortedVertices = GetAllVertices();

            if (!ySortedVertices.Any()) return;

            var yMin = (int) ySortedVertices.First();
            var yMax = (int) ySortedVertices.Last(); // w dół na układzie współrzędnych;

            var listOfAllPoints = new List<double>(); // lista wszystkich x-ów, między nimi będziemy wypełniać;

            for (var i = yMin; i <= yMax; i++)
            {
                // i - współrzędna y = i (pozioma linia);

                foreach (var line in LinesList)
                {
                    var p0 = line.StartPoint;
                    var p1 = line.EndPoint;

                    if (p0.Y > p1.Y)
                    {
                        Swap(ref p0, ref p1);
                    } // gwarant, że p0.Y <= p1.Y;

                    var deltaY = p1.Y - p0.Y;

                    if (p0.Y <= i && i <= p1.Y)
                    {
                        double xPoint;

                        if (p0.X < p1.X)
                        {
                            xPoint = (i - p0.Y)*(p1.X - p0.X)/deltaY;
                            listOfAllPoints.Add(p0.X + xPoint);
                        }
                        else
                        {
                            xPoint = (i - p0.Y)*(p0.X - p1.X)/deltaY;
                            listOfAllPoints.Add(p0.X - xPoint);
                        }
                    }
                }

                if (listOfAllPoints.Count > 1)
                {
                    var array = listOfAllPoints.OrderBy(x => x).ToArray();

                    var stride = 0;
                    var pixels = new byte[1];

                    if (IfToFillWithImage)
                    {
                        stride = FillBitmap.PixelWidth * 4;
                        var size = FillBitmap.PixelHeight * stride;
                        pixels = new byte[size];
                        FillBitmap.CopyPixels(pixels, stride, 0);
                    }

                    if (array.Count() % 2 == 0)
                    {
                        for (var j = 0; j < array.Count(); j += 2)
                        {
                            for (var k = (int) array[j]; k <= (int) array[j + 1]; k++)
                            {
                                int x = k - (int)MyBoundary.XMin;
                                int y = i - (int)MyBoundary.YMin;

                                if (x < 0 || y < 0)
                                {
                                    x = y = 0;
                                }

                                if(IfToFillWithImage)
                                {
                                    x %= FillBitmap.PixelWidth;
                                    y %= FillBitmap.PixelHeight;
                                }

                                var colorToFill = IfToFillWithImage
                                    ? Static.GetColorFromPixelsArray(pixels, stride, x, y)
                                    : FillColor;

                                wb.SetPixel(k, i, colorToFill);
                            }
                        }
                    }
                }

                listOfAllPoints.RemoveAll(x => true);
            }

            if (ifToFill) DrawBorder(wb);
        }

        public override bool IfPointCloseToBoundary(Point p)
        {
            return
                LinesList.Any(item =>
                        Static.DistanceBetweenPoints(item.StartPoint, p) <= Static.Distance ||
                        Static.DistanceBetweenLineAndPoint(item, p) <= Static.Distance);
        }

        public bool PolygonIsConvex()
        {
            // For each set of three adjacent points A, B, C,
            // find the cross product AB · BC. If the sign of
            // all the cross products is the same, the angles
            // are all positive or negative (depending on the
            // order in which we visit them) so the polygon
            // is convex.

            var Points = LinesList.Select(x => x.StartPoint).ToList();

            var gotNegative = false;
            var gotPositive = false;
            var numPoints = Points.Count();
            int B, C;
            for (var a = 0; a < numPoints; a++)
            {
                B = (a + 1)%numPoints;
                C = (B + 1)%numPoints;

                var crossProduct = CrossProductLength(
                        Points[a].X, Points[a].Y,
                        Points[B].X, Points[B].Y,
                        Points[C].X, Points[C].Y);

                if (crossProduct < 0)
                {
                    gotNegative = true;
                }
                else if (crossProduct > 0)
                {
                    gotPositive = true;
                }

                if (gotNegative && gotPositive) return false;
            }

            // If we got this far, the polygon is convex.
            return true;
        }

        // Return the cross product AB x BC. The cross product is a vector perpendicular to AB
        // and BC having length |AB| * |BC| * Sin(theta) and with direction given by the right-
        // hand rule. For two vectors in the X-Y plane, the result is a vector with X and Y 
        // components 0 so the Z component gives the vector's length and direction.
        public static double CrossProductLength(double Ax, double Ay, double Bx, double By, double Cx, double Cy)
        {
            // Get the vectors' coordinates.
            var BAx = Ax - Bx;
            var BAy = Ay - By;
            var BCx = Cx - Bx;
            var BCy = Cy - By;

            // Calculate the Z coordinate of the cross product.
            return (BAx * BCy - BAy * BCx);
        }

        #endregion

        #region Additional Methods

        private List<double> GetAllVertices()
        {
            return (from p in
                (from q in LinesList
                    select q.EndPoint)
                orderby p.Y ascending
                select p.Y).ToList();
        }

        private static void Swap<T>(ref T first, ref T second)
        {
            var t = first;
            first = second;
            second = t;
        }

        private void DrawBorder(WriteableBitmap wb)
        {
            foreach (var item in LinesList)
            {
                wb.DrawLine(item.StartPoint, item.EndPoint, Color, Width);
            }
        }

        #endregion
    }
}