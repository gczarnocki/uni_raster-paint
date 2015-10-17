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
    abstract class MyObject
    {
        public Color Color { get; set; }
        public int Width { get; set; }
        public MyBoundary MyBoundary;

        public MyObject()
        {
            MyBoundary = new MyBoundary();
        }

        public abstract MyObject MoveObject(Vector v);
        public abstract MyObject Clone();
        public abstract void UpdateBoundaries();
        public abstract void DrawObject(WriteableBitmap wb, int width);
        public abstract void EraseObject(List<MyObject> list, WriteableBitmap wb);
        public abstract void HighlightObject(bool ifHighlight, WriteableBitmap wb);
    }
}
