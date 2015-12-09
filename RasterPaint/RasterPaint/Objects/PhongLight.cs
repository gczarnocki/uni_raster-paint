using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace RasterPaint.Objects
{
    public class PhongLight
    {
        public Vector3D Position { get; set; }
        public Color Color { get; set; }

        public PhongLight(Vector3D position, Color color)
        {
            Position = position;
            Color = color;
        }
    }
}
