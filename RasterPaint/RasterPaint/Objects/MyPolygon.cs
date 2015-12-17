using System.Collections.Generic;
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
        [XmlIgnore] private WriteableBitmap _fillBitmap;
        [XmlIgnore] private WriteableBitmap _initialBitmap;
        [XmlIgnore] private WriteableBitmap _normalBitmap;
        private PhongMaterial _phongMaterial;

        [XmlArray] public List<MyLine> LinesList = new List<MyLine>();

        public Color FillColor { get; set; }

        [XmlIgnore]
        public WriteableBitmap FillBitmap
        {
            get { return _fillBitmap; }
            set { _fillBitmap = value; }
        }

        [XmlIgnore]
        public WriteableBitmap InitialBitmap
        {
            get { return _initialBitmap; }
            set { _initialBitmap = value; }
        }

        [XmlIgnore]
        public WriteableBitmap NormalBitmap
        {
            get { return _normalBitmap; }
            set { _normalBitmap = value;  }
        }

        public PhongMaterial PhongMaterial
        {
            get { return _phongMaterial; }
            set { _phongMaterial = value; }
        }

        public Point[] GetPointsArray => LinesList.Select(x => x.StartPoint).ToArray();
        #endregion

        #region Methods
        public override void DrawObject(WriteableBitmap wb)
        {
            DrawAllEdges(wb);
            FillPolygonScanLine(wb, FillColor, true);
        }

        public void DrawObjectPhong(WriteableBitmap wb, PhongIlluminationModel pim, bool bumpMappingEnabled)
        {
            DrawAllEdges(wb);
            FillPolygonScanLinePhong(wb, FillColor, pim, bumpMappingEnabled);
        }

        private void DrawAllEdges(WriteableBitmap wb)
        {
            foreach (var item in LinesList)
            {
                wb.DrawLine(item.StartPoint, item.EndPoint, Color, Width);
            }
        }

        public override void EraseObject(List<MyObject> list, WriteableBitmap wb, Color c)
        {
            if (list.Contains(this))
            {
                list.Remove(this);
                wb.Clear(c);

                foreach (var item in list)
                {
                    item.DrawObject(wb);
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
                InitialBitmap = InitialBitmap,
                PhongMaterial = PhongMaterial
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
                FillBitmap = FillBitmap,
                InitialBitmap = InitialBitmap,
                PhongMaterial = PhongMaterial
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
        public bool IfBumpMappingCanBeDone => FillBitmap != null && NormalBitmap != null;

        private List<double> GetListOfAllPoints(List<double> ySortedVertices, int i)
        {
            List<double> listOfAllPoints = new List<double>();

            foreach (var line in LinesList)
            {
                var p0 = line.StartPoint;
                var p1 = line.EndPoint;

                if (p0.Y > p1.Y)
                {
                    Static.Swap(ref p0, ref p1);
                }

                var deltaY = p1.Y - p0.Y;

                if (p0.Y <= i && i <= p1.Y)
                {
                    double xPoint;

                    if (p0.X < p1.X)
                    {
                        xPoint = (i - p0.Y) * (p1.X - p0.X) / deltaY;
                        listOfAllPoints.Add(p0.X + xPoint);
                    }
                    else
                    {
                        xPoint = (i - p0.Y) * (p0.X - p1.X) / deltaY;
                        listOfAllPoints.Add(p0.X - xPoint);
                    }
                }
            }

            return listOfAllPoints;
        }

        private void FillLineBetweenPoints(int i, List<double> listOfAllPoints, WriteableBitmap wb)
        {
            if (listOfAllPoints.Count <= 1) return;

            var array = listOfAllPoints.OrderBy(x => x).ToArray();

            var stride = 0;
            var pixels = new byte[1];
            var size = 0;

            GetAllPixelsArray(FillBitmap, out stride, out pixels, out size);

            if (array.Count() % 2 == 0)
            {
                for (var j = 0; j < array.Count(); j += 2)
                {
                    for (var k = (int)array[j]; k <= (int)array[j + 1]; k++)
                    {
                        var x = k - (int)MyBoundary.XMin;
                        var y = i - (int)MyBoundary.YMin;

                        if (x < 0 || y < 0)
                        {
                            x = y = 0;
                        }

                        if (IfToFillWithImage)
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

        private void FillLineBetweenPointsPhong(int i, List<double> listOfAllPoints, WriteableBitmap wb, PhongIlluminationModel pim, PhongMaterial pm, bool bumpMappingEnabled)
        {
            if (listOfAllPoints.Count <= 1) return;

            var array = listOfAllPoints.OrderBy(x => x).ToArray();

            var stride = 0;
            var pixels = new byte[1];
            var size = 0;

            GetAllPixelsArray(FillBitmap, out stride, out pixels, out size);

            if (array.Count() % 2 == 0)
            {
                for (var j = 0; j < array.Count(); j += 2)
                {
                    for (var k = (int)array[j]; k <= (int)array[j + 1]; k++)
                    {
                        var xPolygon = k - (int)MyBoundary.XMin;
                        var yPolygon = i - (int)MyBoundary.YMin;

                        if (xPolygon < 0 || yPolygon < 0)
                        {
                            xPolygon = yPolygon = 0;
                        }

                        Color colorToFill = FillColor;

                        if (IfToFillWithImage)
                        {
                            xPolygon %= FillBitmap.PixelWidth;
                            yPolygon %= FillBitmap.PixelHeight;

                            var color = Static.GetColorFromPixelsArray(pixels, stride, xPolygon, yPolygon);

                            colorToFill = (bumpMappingEnabled && IfBumpMappingCanBeDone)
                                ? pim.GetIlluminatedPixel(k, i, pm, color, true)
                                : pim.GetIlluminatedPixel(k, i, pm, color, false);
                        }
                        else
                        {
                            colorToFill = pim.GetIlluminatedPixel(k, i, pm, FillColor, false);
                        }

                        wb.SetPixel(k, i, colorToFill);
                    }
                }
            }
        }

        private void FillPolygonScanLine(WriteableBitmap wb, Color c, bool ifToDrawBorder)
        {
            FillColor = c;
            var ySortedVertices = GetAllVertices();
            if (!ySortedVertices.Any()) return;

            var yMin = (int)ySortedVertices.First();
            var yMax = (int)ySortedVertices.Last();

            for (var i = yMin; i <= yMax; i++)
            {
                var listOfAllXPoints = GetListOfAllPoints(ySortedVertices, i);
                FillLineBetweenPoints(i, listOfAllXPoints, wb);
            }

            if (ifToDrawBorder) DrawBorder(wb);
        }

        private void FillPolygonScanLinePhong(WriteableBitmap wb, Color c, PhongIlluminationModel pim, bool bumpMappingEnabled)
        {
            var ySortedVertices = GetAllVertices();
            if (!ySortedVertices.Any()) return;

            var yMin = (int)ySortedVertices.First();
            var yMax = (int)ySortedVertices.Last();

            for (var i = yMin; i <= yMax; i++)
            {
                var listOfAllXPoints = GetListOfAllPoints(ySortedVertices, i);
                FillLineBetweenPointsPhong(i, listOfAllXPoints, wb, pim, PhongMaterial, bumpMappingEnabled);
            }

            DrawBorder(wb);
        }

        private void GetAllPixelsArray(WriteableBitmap bitmap, out int stride, out byte[] pixels, out int size)
        {
            stride = 0;
            pixels = new byte[1];
            size = 0;

            if (bitmap != null)
            {
                stride = bitmap.PixelWidth * 4;
                size = bitmap.PixelHeight * stride;
                pixels = new byte[size];
                bitmap.CopyPixels(pixels, stride, 0);
            }
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