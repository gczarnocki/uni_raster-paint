using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RasterPaint
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

        public abstract MyObject MoveObject(Vector v);
        public abstract MyObject Clone();
        public abstract void UpdateBoundaries();
        public abstract void DrawObject(WriteableBitmap wb);
        public abstract void EraseObject(List<MyObject> list, WriteableBitmap wb, Color c);
        public abstract void HighlightObject(bool ifHighlight, WriteableBitmap wb);
    }
}
