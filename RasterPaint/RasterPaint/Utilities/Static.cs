using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using RasterPaint.Objects;

namespace RasterPaint
{
    public static class Static
    {
        public static double Distance = 5.0F;

        public static double DistanceBetweenPoints(Point a, Point b)
        {
            return Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
        }

        public static double DistanceBetweenLineAndPoint(MyLine ml, Point p)
        {
            Point p1 = ml.StartPoint;
            Point p2 = ml.EndPoint;

            return Math.Abs((p2.Y - p1.Y) * p.X - (p2.X - p1.X) * p.Y + p2.X * p1.Y - p2.Y * p1.X)
                   / Math.Sqrt((p2.Y - p1.Y) * (p2.Y - p1.Y) + (p2.X - p1.X) * (p2.X - p1.X));
        }
        
        public static void Swap<T>(ref T first, ref T second)
        {
            var t = first;
            first = second;
            second = t;
        }

        public static void CreateThumbnail(string filename, BitmapSource bs)
        {
            if (filename != string.Empty)
            {
                using (var stream = new FileStream(filename, FileMode.Create))
                {
                    PngBitmapEncoder encoder5 = new PngBitmapEncoder();
                    encoder5.Frames.Add(BitmapFrame.Create(bs));
                    encoder5.Save(stream);
                    stream.Close();
                }
            }
        }

        public static System.Windows.Media.Color GetColorFromPixelsArray(byte[] pixels, int stride, int x, int y)
        {
            var index = y * stride + 4 * x;

            return System.Windows.Media.Color.FromArgb(pixels[index + 3], pixels[index + 2], pixels[index + 1], pixels[index + 0]);
        }
    }
}
