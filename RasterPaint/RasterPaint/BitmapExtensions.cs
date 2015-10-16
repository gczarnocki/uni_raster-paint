using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RasterPaint
{
    public static class BitmapExtensions
    {
        public static void SetPixel(this WriteableBitmap wb, int x, int y, Color c)
        {
            if (y > wb.PixelHeight - 1 || x > wb.PixelWidth - 1)
            {
                return;
            }

            if (y < 0 || x < 0)
            {
                return;
            }

            if (!wb.Format.Equals(PixelFormats.Bgra32))
            {
                return;
            }

            wb.Lock();
            IntPtr buff = wb.BackBuffer;
            int stride = wb.BackBufferStride;

            unsafe
            {
                byte* pbuff = (byte*)buff.ToPointer();
                int loc = y * stride + x * 4;
                pbuff[loc]     = c.B;
                pbuff[loc + 1] = c.G;
                pbuff[loc + 2] = c.R;
                pbuff[loc + 3] = c.A;
            }

            wb.AddDirtyRect(new Int32Rect(x, y, 1, 1));
            wb.Unlock();
        }

        public static Color GetPixel(this WriteableBitmap wbm, int x, int y)
        {
            if ((y > wbm.PixelHeight - 1 || x > wbm.PixelWidth - 1) || (y < 0 || x < 0))
            {
                return Color.FromArgb(0, 0, 0, 0);
            }

            if (!wbm.Format.Equals(PixelFormats.Bgra32))
            {
                return Color.FromArgb(0, 0, 0, 0);
            }

            IntPtr buff = wbm.BackBuffer;
            int stride = wbm.BackBufferStride;
            Color c;

            unsafe
            {
                byte* pbuff = (byte*)buff.ToPointer();
                int loc = y * stride + x * 4;
                c = Color.FromArgb(pbuff[loc + 3], pbuff[loc + 2], pbuff[loc + 1], pbuff[loc]);
            }

            return c;
        }

        public static IEnumerable<Point> GetPoints(int x0, int y0, int x1, int y1)
        {
            bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);

            if (steep)
            {
                int t;
                t = x0; // swap x0 and y0
                x0 = y0;
                y0 = t;
                t = x1; // swap x1 and y1
                x1 = y1;
                y1 = t;
            }

            if (x0 > x1)
            {
                int t;
                t = x0; // swap x0 and x1
                x0 = x1;
                x1 = t;
                t = y0; // swap y0 and y1
                y0 = y1;
                y1 = t;
            }

            int dx = x1 - x0;
            int dy = Math.Abs(y1 - y0);
            int error = dx / 2;
            int ystep = (y0 < y1) ? 1 : -1;
            int y = y0;

            for (int x = x0; x <= x1; x++)
            {
                yield return new Point((steep ? y : x), (steep ? x : y));
                error = error - dy;
                if (error < 0)
                {
                    y += ystep;
                    error += dx;
                }
            }
        }

        public static void DrawLine(WriteableBitmap wb, Point startPoint, Point endPoint, Color c)
        {
            IEnumerable<Point> points = GetPoints((int)startPoint.X, (int)startPoint.Y, (int)endPoint.X, (int)endPoint.Y);

            foreach (var p in points)
            {
                SetPixel(wb, (int)p.X, (int)p.Y, c);
            }
        }
    }
}
