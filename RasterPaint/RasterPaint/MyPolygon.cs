using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RasterPaint
{
    class MyPolygon : MyObject
    {
        public List<MyLine> LinesList = new List<MyLine>();

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

        public override void DrawObject(WriteableBitmap wb)
        {
            foreach(MyLine item in LinesList)
            {
                wb.DrawLine(item.StartPoint, item.EndPoint, Color, Width);
            }
        }

        public override void EraseObject(List<MyObject> list, WriteableBitmap wb, Color c)
        {
            if (list.Contains(this))
            {
                foreach (var item in LinesList)
                {
                    // wb.DrawLine(item.StartPoint, item.EndPoint, Colors.White, Width);
                    wb.DrawLine(item.StartPoint, item.EndPoint, c, Width);
                }

                list.Remove(this);
            }
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

        public override void HighlightObject(bool ifHighlight, WriteableBitmap wb)
        {
            Color c = ifHighlight ? Colors.Red : Color;

            foreach (var item in LinesList)
            {
                wb.DrawLine(item.StartPoint, item.EndPoint, c, Width);
            }
        }

        public override MyObject MoveObject(Vector v)
        {
            MyPolygon mo = new MyPolygon { Color = Color, Width = Width, MyBoundary = MyBoundary };

            foreach (var item in LinesList)
            {
                Point newStartPoint = new Point(item.StartPoint.X + v.X, item.StartPoint.Y + v.Y);
                Point newEndPoint = new Point(item.EndPoint.X + v.X, item.EndPoint.Y + v.Y);
                mo.AddLine(new MyLine(newStartPoint, newEndPoint));
            }

            return mo;
        }
    }
}
