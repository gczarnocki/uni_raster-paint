using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;

namespace RasterPaint.Objects
{
    public class PhongMaterial
    {
        public Vector3D Ambient { get; set; }
        public Vector3D Diffuse { get; set; }
        public Vector3D Specular { get; set; }
        public double Shininess { get; set; }

        public PhongMaterial(Vector3D ambient, Vector3D diffuse, Vector3D specular, double shininess)
        {
            Ambient = ambient;
            Diffuse = diffuse;
            Specular = specular;
            Shininess = shininess;
        }
    }
}
