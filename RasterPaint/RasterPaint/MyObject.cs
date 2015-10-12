using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RasterPaint
{
    class MyObject
    {
        public List<MyLine> linesList = new List<MyLine>(); // linie, które wchodzą w skład obiektu;
        public List<Point> pointsList = new List<Point>(); // wszystkie wierzchołki danego obiektu;
        public Boundary objectBoundary = new Boundary();
        public Color Color { get; set; } // kolor obiektu;

        public void DrawAndAdd(WriteableBitmap wb, MyLine ml, Color c)
        {
            BitmapExtensions.DrawLine(wb, ml.StartPoint, ml.EndPoint, c);
            Color = c;

            AddLinesAndPoints(ml);
        }

        public void AddLinesAndPoints(MyLine ml) // dodanie linii i punktów do list wewn. oraz aktualizacja ograniczenia;
        {
            if (!ml.Equals(null) && !linesList.Contains(ml))
            {
                linesList.Add(ml);
                pointsList.Add(ml.StartPoint);
                pointsList.Add(ml.EndPoint);
            }
        }

        public void AddPoint(Point p) // dla metody Clone();
        {
            if (!p.Equals(null) && !pointsList.Contains(p))
            {
                pointsList.Add(p);
            }
        }

        public void AddLine(MyLine ml) // dla metody Clone();
        {
            if (!ml.Equals(null) && !linesList.Contains(ml))
            {
                linesList.Add(ml);
                UpdateBoundaries(ml);
            }
        }

        public MyObject Clone()
        {
            MyObject clone = new MyObject();

            foreach (var item in linesList)
            {
                clone.AddLine(item);
            }

            foreach (var item in pointsList)
            {
                clone.AddPoint(item);
            }

            clone.Color = Color;

            return clone;
        }

        private void UpdateBoundaries(MyLine ml)
        {
            objectBoundary.UpdateBoundary(ml.StartPoint.X, ml.StartPoint.Y);
            objectBoundary.UpdateBoundary(ml.EndPoint.X, ml.EndPoint.Y);
        }

        public void HighlightObject(bool ifHighlight, WriteableBitmap wb)
        {
            Color c = ifHighlight ? Colors.Red : Color;

            foreach (var item in this.linesList)
            {
                BitmapExtensions.DrawLine(wb, item.StartPoint, item.EndPoint, c);
            }
        }
    }
}
