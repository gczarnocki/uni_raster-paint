using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace RasterPaint
{
    abstract class MyObject
    {
        public Color Color { get; set; }
        public int Width { get; set; }

        public abstract MyObject MoveObject(Vector v);
    }
}
