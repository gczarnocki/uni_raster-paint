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
        public List<PhongLight> PhongLights { get; set; }
        public int ViewerZ { get; set; }

        public PhongIlluminationModel(PhongLight phongLight, int viewerZ)
        {
            ViewerZ = viewerZ;
            PhongLights = new List<PhongLight> { phongLight };
        }

        public PhongIlluminationModel(int viewerZ)
        {
            ViewerZ = viewerZ;
            PhongLights = new List<PhongLight>();
        }

        public Color GetIlluminatedPixel(int x, int y, PhongMaterial pm, Color c, bool bumpMappingEnabled)
        {
            Vector3D N = new Vector3D();

            if (bumpMappingEnabled)
            {
                N = new Vector3D(c.R / 255.0, c.G / 255.0, c.B / 255.0);
                N = 2 * N - new Vector3D(1, 1, 1); // [0, 255] -> [-1, 1];
            }
            else
            {
                N = new Vector3D(0, 0, 1);
            }


            Vector3D positionVector = new Vector3D(x, y, 0);
            Vector3D illumination = pm.Ambient;

            foreach (var lightSource in PhongLights)
            {
                Vector3D L = lightSource.Position - positionVector;
                Vector3D R = Reflect(lightSource.Position, N);
                Vector3D V = new Vector3D(x, y, ViewerZ);

                L.Normalize();
                R.Normalize();
                V.Normalize();

                illumination += pm.Diffuse * Math.Max(Vector3D.DotProduct(L, N), 0);
                illumination += pm.Specular * Math.Pow(Math.Max(Vector3D.DotProduct(R, V), 0), pm.Shininess);
            }

            Vector3D illuminatedColor = new Vector3D(
                illumination.X * c.R,
                illumination.Y * c.G,
                illumination.Z * c.B);

            ClampRGBVector3DTo01(ref illuminatedColor);

            var color = Color.FromRgb((byte)illuminatedColor.X, (byte)illuminatedColor.Y, (byte)illuminatedColor.Z);

            return color;
        }

        private void ClampRGBVector3DTo01(ref Vector3D vector3D)
        {
            if (vector3D.X > 255) vector3D.X = 255;
            if (vector3D.Y > 255) vector3D.Y = 255;
            if (vector3D.Z > 255) vector3D.Z = 255; // clamp to 1;

            if (vector3D.X < 0) vector3D.Y = 0;
            if (vector3D.Y < 0) vector3D.Z = 0;
            if (vector3D.Z < 0) vector3D.X = 0; // or to 0;
        }

        private static Vector3D Reflect(Vector3D lightSourceVector, Vector3D normal)
        {
            return 2 * Vector3D.DotProduct(lightSourceVector, normal) * normal - lightSourceVector;
        }
    }
}
