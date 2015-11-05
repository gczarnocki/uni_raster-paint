using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xaml.Schema;

namespace RasterPaint.Objects
{
    abstract public class MyObject
    {
        public Color Color { get; set; }
        public int Width { get; set; } = 1;
        public MyBoundary MyBoundary;

        protected MyObject()
        {
            MyBoundary = new MyBoundary();
        }

        public double XCenter => MyBoundary.XMin + (MyBoundary.XMax - MyBoundary.XMin) / 2;

        public double YCenter => MyBoundary.YMin + (MyBoundary.YMax - MyBoundary.YMin) / 2;

        public abstract MyObject MoveObject(Vector v);
        public abstract MyObject Clone();
        public abstract void UpdateBoundaries();
        public abstract void DrawObject(WriteableBitmap wb);
        public abstract void EraseObject(List<MyObject> list, WriteableBitmap wb, Color c);
        public abstract void HighlightObject(bool ifHighlight, WriteableBitmap wb, Color c);
        public abstract bool IfPointCloseToBoundary(Point p);
    }
}
