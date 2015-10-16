using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RasterPaint
{
    class MyPolygon : MyObject
    {
        public List<MyLine> LinesList = new List<MyLine>();
        public MyBoundary ObjectMyBoundary = new MyBoundary();

        public void DrawAndAdd(WriteableBitmap wb, MyLine ml, Color c)
        {
            Color = c;
            AddLine(ml);

            BitmapExtensions.DrawLine(wb, ml.StartPoint, ml.EndPoint, c);
        }

        public void AddLine(MyLine ml)
        {
            if (!ml.Equals(null) && !LinesList.Contains(ml))
            {
                LinesList.Add(ml);
                UpdateBoundaries(ml);
            }
        }

        public MyPolygon MoveObject(Vector v)
        {
            MyPolygon mo = new MyPolygon {Color = Color};

            foreach(var item in LinesList)
            {
                Point newStartPoint = new Point(item.StartPoint.X + v.X, item.StartPoint.Y + v.Y);
                Point newEndPoint = new Point(item.EndPoint.X + v.X, item.EndPoint.Y + v.Y);
                mo.AddLine(new MyLine(newStartPoint, newEndPoint));
            }

            return mo;
        }

        public MyPolygon Clone()
        {
            MyPolygon clone = new MyPolygon { Color = Color };

            foreach (var item in LinesList)
            {
                clone.AddLine(item);
            }

            return clone;
        }

        private void UpdateBoundaries(MyLine ml)
        {
            ObjectMyBoundary.UpdateBoundary(ml.StartPoint.X, ml.StartPoint.Y);
            ObjectMyBoundary.UpdateBoundary(ml.EndPoint.X, ml.EndPoint.Y);
        }

        public void HighlightObject(bool ifHighlight, WriteableBitmap wb)
        {
            Color c = ifHighlight ? Colors.Red : Color;

            foreach (var item in LinesList)
            {
                BitmapExtensions.DrawLine(wb, item.StartPoint, item.EndPoint, c);
            }
        }
    }
}
