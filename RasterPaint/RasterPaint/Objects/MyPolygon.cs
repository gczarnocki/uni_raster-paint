using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace RasterPaint.Objects
{
    public class MyPolygon : MyObject
    {
        [XmlArray]
        public List<MyLine> LinesList = new List<MyLine>();
        public Color FillColor { get; set; }

        #region Methods
        public override void DrawObject(WriteableBitmap wb)
        {
            foreach (MyLine item in LinesList)
            {
                wb.DrawLine(item.StartPoint, item.EndPoint, Color, Width);
            }

            this.FillPolygonScanLine(true, wb, FillColor);
        }

        public override void EraseObject(List<MyObject> list, WriteableBitmap wb, Color c)
        {
            if (list.Contains(this))
            {
                this.FillPolygonScanLine(false, wb, c);

                foreach (var item in LinesList)
                {
                    // wb.DrawLine(item.StartPoint, item.EndPoint, Colors.White, Width);
                    wb.DrawLine(item.StartPoint, item.EndPoint, c, Width);
                }

                list.Remove(this);
            }
        }

        public void DrawAndAddLine(WriteableBitmap wb, MyLine ml, Color c)
        {
            Color = c;
            AddLine(ml);

            wb.DrawLine(ml.StartPoint, ml.EndPoint, c, Width);
        }

        public void AddLine(MyLine ml)
        {
            if (!ml.Equals(null) && !LinesList.Contains(ml))
            {
                LinesList.Add(ml);
                UpdateBoundaries();
            }
        }

        public override MyObject Clone()
        {
            MyPolygon clone = new MyPolygon { Color = Color, Width = Width, MyBoundary = MyBoundary };

            foreach (var item in LinesList)
            {
                clone.AddLine(item);
            }

            return clone;
        }

        public override MyObject MoveObject(Vector v)
        {
            MyPolygon mo = new MyPolygon { Color = Color, Width = Width, MyBoundary = MyBoundary, FillColor = FillColor };

            foreach (var item in LinesList)
            {
                Point newStartPoint = new Point(item.StartPoint.X + v.X, item.StartPoint.Y + v.Y);
                Point newEndPoint = new Point(item.EndPoint.X + v.X, item.EndPoint.Y + v.Y);
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
            Color color = ifToHighlight ? c : Color;

            foreach (var item in LinesList)
            {
                wb.DrawLine(item.StartPoint, item.EndPoint, color, Width);
            }
        }

        public void FillPolygonScanLine(bool ifToFill, WriteableBitmap wb, Color c)
        {
            FillColor = c;

            List<double> ySortedVertices = GetAllVertices();

            if (!ySortedVertices.Any()) return;

            int yMin = (int)ySortedVertices.First();
            int yMax = (int)ySortedVertices.Last(); // w dół na układzie współrzędnych;

            List<double> listOfAllPoints = new List<double>(); // lista wszystkich x-ów, między nimi będziemy wypełniać;

            for (int i = yMin; i <= yMax; i++)
            {
                // i - współrzędna y = i (pozioma linia);

                foreach (var line in LinesList)
                {
                    Point p0 = line.StartPoint;
                    Point p1 = line.EndPoint;

                    if (p0.Y > p1.Y)
                    {
                        Swap(ref p0, ref p1);
                    } // gwarant, że p0.Y <= p1.Y;

                    double deltaY = p1.Y - p0.Y;

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

                if (listOfAllPoints.Count > 1)
                {
                    var array = listOfAllPoints.OrderBy(x => x).ToArray();

                    if (array.Count() % 2 == 0)
                    {
                        for (int j = 0; j < array.Count(); j += 2)
                        {
                            wb.DrawLine((int)array[j], i, (int)array[j + 1], i, FillColor);
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Tutaj nie weszliśmy: i = " + i);
                        Debug.WriteLine("Count: " + array.Count());
                    }
                }

                listOfAllPoints.RemoveAll(x => true);
            }

            if(ifToFill) this.DrawBorder(wb);
        }

        public override bool IfPointCloseToBoundary(Point p)
        {
            return this.LinesList.Any(item => Static.DistanceBetweenPoints(item.StartPoint, p) <= Static.Distance || Static.DistanceBetweenLineAndPoint(item, p) <= Static.Distance);
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