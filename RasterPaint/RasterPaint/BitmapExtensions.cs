using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RasterPaint
{
    public static class BitmapExtensions
    {
        internal const int SizeOfArgb = 4;

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
            bool coefficient = Math.Abs(y1 - y0) > Math.Abs(x1 - x0); // delta(y) > delta(x);

            if (coefficient)
            {
                int t = x0; // swap x0 and y0
                x0 = y0;
                y0 = t;

                t = x1; // swap x1 and y1
                x1 = y1;
                y1 = t;
            }

            if (x0 > x1)
            {
                int t = x0; // swap x0 and x1
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
                yield return new Point((coefficient ? y : x), (coefficient ? x : y));
                error = error - dy;
                if (error < 0)
                {
                    y += ystep;
                    error += dx;
                }
            }
        }

        public static void DrawLine(this WriteableBitmap wb, Point startPoint, Point endPoint, Color c, int radius)
        {
            int x1 = (int)startPoint.X;
            int y1 = (int)startPoint.Y;
            int x2 = (int)endPoint.X;
            int y2 = (int)endPoint.Y;

            wb.DrawLine(x1, y1, x2, y2, c); // draw 'core' line;

            var steep = Math.Abs(y2 - y1) > Math.Abs(x2 - x1); // delta(y) > delta(x);

            if (steep && radius > 0)
            {
                for (var i = -radius; i <= radius; i++)
                {
                    wb.DrawLine(x1 + i, y1, x2 + i, y2, c);
                }
            }
            else
            {
                for (var i = -radius; i <= radius; i++)
                {
                    wb.DrawLine(x1, y1 + i, x2, y2 + i, c);
                }
            }
        }

        public static void DrawLineDeprecated(this WriteableBitmap wb, Point startPoint, Point endPoint, Color c, int radius)
        // Other version of DrawLine method - deprecated, bad performance;
        {
            int x1 = (int)startPoint.X;
            int y1 = (int)startPoint.Y;
            int x2 = (int)endPoint.X;
            int y2 = (int)endPoint.Y;
            var steep = Math.Abs(y2 - y1) > Math.Abs(x2 - x1); // delta(y) > delta(x);

            IEnumerable<Point> points = GetPoints(x1, y1, x2, y2);

            if (steep)
            {
                foreach (var p in points)
                {
                    for (var i = -radius; i <= radius; i++)
                    {
                        SetPixel(wb, (int)p.X + i, (int)p.Y, c);
                    }
                }
            }
            else
            {
                foreach (var p in points)
                {
                    for (var i = -radius; i <= radius; i++)
                    {
                        SetPixel(wb, (int)p.X, (int)p.Y + i, c);
                    }
                }
            }
        }

        public static void DrawPoint(WriteableBitmap wb, Point point, Color color, int radius)
        {
            int pX = (int) point.X;
            int pY = (int) point.Y;

            for (int i = pX - radius; i <= pX + radius; i++)
            {
                for (int j = pY - radius; j <= pY + radius; j++)
                {
                    if ((i - pX) * (i - pX) + (j - pY) * (j - pY) <= radius * radius)
                    {
                        wb.SetPixel(i, j, color);
                    }
                }
            }
        }

        // --------------------------------------------------------------------------------------------------------- //
        // --- WriteableBitmapEx: https://writeablebitmapex.codeplex.com/; The NuGet Package: WriteableBitmapEx ---- //

        public static void DrawLine(this WriteableBitmap bmp, int x1, int y1, int x2, int y2, int color)
        // WriteableBitmapEx: https://writeablebitmapex.codeplex.com/;
        // The NuGet Package is added to project as well: Install-Package WriteableBitmapEx;
        {
            unsafe
            {
                using (var context = bmp.GetBitmapContext())
                {
                    // Use refs for faster access (really important!) speeds up a lot!
                    int w = context.Width;
                    int h = context.Height;
                    var pixels = context.Pixels;

                    // Distance start and end point
                    int dx = x2 - x1;
                    int dy = y2 - y1;

                    // Determine sign for direction x
                    int incx = 0;
                    if (dx < 0)
                    {
                        dx = -dx;
                        incx = -1;
                    }
                    else if (dx > 0)
                    {
                        incx = 1;
                    }

                    // Determine sign for direction y
                    int incy = 0;
                    if (dy < 0)
                    {
                        dy = -dy;
                        incy = -1;
                    }
                    else if (dy > 0)
                    {
                        incy = 1;
                    }

                    // Which gradient is larger
                    int pdx, pdy, odx, ody, es, el;
                    if (dx > dy)
                    {
                        pdx = incx;
                        pdy = 0;
                        odx = incx;
                        ody = incy;
                        es = dy;
                        el = dx;
                    }
                    else
                    {
                        pdx = 0;
                        pdy = incy;
                        odx = incx;
                        ody = incy;
                        es = dx;
                        el = dy;
                    }

                    // Init start
                    int x = x1;
                    int y = y1;
                    int error = el >> 1;
                    if (y < h && y >= 0 && x < w && x >= 0)
                    {
                        pixels[y * w + x] = color;
                    }

                    // Walk the line!
                    for (int i = 0; i < el; i++)
                    {
                        // Update error term
                        error -= es;

                        // Decide which coord to use
                        if (error < 0)
                        {
                            error += el;
                            x += odx;
                            y += ody;
                        }
                        else
                        {
                            x += pdx;
                            y += pdy;
                        }

                        // Set pixel
                        if (y < h && y >= 0 && x < w && x >= 0)
                        {
                            pixels[y * w + x] = color;
                        }
                    }
                }
            }
        }

        public static void Clear(this WriteableBitmap bmp, Color color)
        {
        // WriteableBitmapEx: https://writeablebitmapex.codeplex.com/;
        // The NuGet Package is added to project as well: Install-Package WriteableBitmapEx;
            unsafe
            {
                var col = ConvertColor(color);
                using (var context = bmp.GetBitmapContext())
                {
                    var pixels = context.Pixels;
                    var w = context.Width;
                    var h = context.Height;
                    var len = w*SizeOfArgb;

                    // Fill first line
                    for (var x = 0; x < w; x++)
                    {
                        pixels[x] = col;
                    }

                    // Copy first line
                    var blockHeight = 1;
                    var y = 1;
                    while (y < h)
                    {
                        BitmapContext.BlockCopy(context, 0, context, y*len, blockHeight*len);
                        y += blockHeight;
                        blockHeight = Math.Min(2*blockHeight, h - y);
                    }
                }
            }
        }

        public static int ConvertColor(Color color)
        {
        // WriteableBitmapEx: https://writeablebitmapex.codeplex.com/;
        // The NuGet Package is added to project as well: Install-Package WriteableBitmapEx;
            var col = 0;

            if (color.A != 0)
            {
                var a = color.A + 1;
                col = (color.A << 24)
                  | ((byte)((color.R * a) >> 8) << 16)
                  | ((byte)((color.G * a) >> 8) << 8)
                  | ((byte)((color.B * a) >> 8));
            }

            return col;
        }
    }
}
