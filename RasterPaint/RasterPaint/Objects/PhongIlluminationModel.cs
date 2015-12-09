using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Color = System.Windows.Media.Color;

namespace RasterPaint.Objects
{
    public class PhongIlluminationModel
    {
        public PhongMaterial PhongMaterial { get; set; }
        public List<PhongLight> PhongLights { get; set; }
        public int ViewerZ { get; set; }

        public PhongIlluminationModel(PhongMaterial phongMaterial, PhongLight phongLight, int viewerZ)
        {
            ViewerZ = viewerZ;
            PhongMaterial = phongMaterial;
            PhongLights = new List<PhongLight> { phongLight };
        }

        public PhongIlluminationModel(PhongMaterial phongMaterial, int viewerZ)
        {
            ViewerZ = viewerZ;
            PhongMaterial = phongMaterial;
            PhongLights = new List<PhongLight>();
        }

        public Color GetNewIlluminatedPixel(int x, int y, Color c, bool bumpMappingEnabled)
        {
            Vector3D N = new Vector3D();

            if (bumpMappingEnabled)
            {
                N = new Vector3D(c.R / 255.0, c.G / 255.0, c.B / 255.0);
            }
            else
            {
                N = new Vector3D(0, 0, 1);
            }

            Vector3D positionVector = new Vector3D(x, y, 0);

            Vector3D illumination = PhongMaterial.Ambient;

            foreach (var lightSource in PhongLights)
            {
                Vector3D L = lightSource.Position - positionVector;
                Vector3D R = Reflect(lightSource.Position, N);
                Vector3D V = new Vector3D(x, y, ViewerZ);

                L.Normalize();
                R.Normalize();
                V.Normalize();

                illumination += PhongMaterial.Diffuse * Math.Max(Vector3D.DotProduct(L, N), 0);
                illumination += PhongMaterial.Specular * Math.Pow(Math.Max(Vector3D.DotProduct(R, V), 0), PhongMaterial.Shininess);
            }

            if (illumination.X > 1) illumination.X = 1;
            if (illumination.Y > 1) illumination.Y = 1;
            if (illumination.Z > 1) illumination.Z = 1;

            return Color.FromRgb(
                (byte)(illumination.X * 255),
                (byte)(illumination.Y * 255),
                (byte)(illumination.Z * 255));
        }

        private static Vector3D Reflect(Vector3D lightSourceVector, Vector3D normal)
        {
            return 2 * Vector3D.DotProduct(lightSourceVector, normal) * normal - lightSourceVector;
        }
    }
}
