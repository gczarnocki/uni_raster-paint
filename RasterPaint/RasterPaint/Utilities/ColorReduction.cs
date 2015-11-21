using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;

namespace RasterPaint.Utilities
{
    public static class ColorReduction
    {
        #region Algorithms
        public static WriteableBitmap UniformQuantization(WriteableBitmap wbm, byte nR, byte nG, byte nB)
        {
            var result = wbm.Clone();

            unsafe
            {
                using (var context = result.GetBitmapContext())
                {
                    for (int i = 0; i < wbm.PixelWidth; i++)
                    {
                        for (int j = 0; j < wbm.PixelHeight; j++)
                        {
                            var c = context.Pixels[j * context.Width + i];
                            var a = (byte)(c >> 24);

                            // Prevent division by zero
                            int ai = a;
                            if (ai == 0)
                            {
                                ai = 1;
                            }

                            ai = ((255 << 8) / ai);

                            var color = Color.FromArgb(a,
                                                 (byte)((((c >> 16) & 0xFF) * ai) >> 8),
                                                 (byte)((((c >> 8) & 0xFF) * ai) >> 8),
                                                 (byte)((((c & 0xFF) * ai) >> 8)));

                            var r = ReducePixelUq(nR, color.R);
                            var g = ReducePixelUq(nG, color.G);
                            var b = ReducePixelUq(nB, color.B);

                            context.Pixels[j * context.Width + i] = (a << 24) | (r << 16) | (g << 8) | b;
                        }
                    }
                }

                return result;
            }
        }

        private static Color ReduceColor(System.Drawing.Color color, byte nR, byte nG, byte nB)
        {
            return Color.FromArgb(color.A, ReducePixelUq(nR, color.R), ReducePixelUq(nG, color.G), ReducePixelUq(nB, color.B));
        }

        private static void ReducePixelUq(byte n, ref byte pixelValue)
        {
            pixelValue = ReducePixelUq(n, pixelValue);
        }

        private static byte ReducePixelUq(byte n, byte pixelValue)
        {
            if (n == 0) return pixelValue;
            if (n == 1) pixelValue = 128;
            int result = 0;

            int fraction = (int) Math.Floor((double) 256 / n);
            int part = (int) Math.Ceiling((double) pixelValue / fraction); // ilość części == n;

            if (pixelValue == 0) part++; // because 0 is in the first part;
            if (pixelValue == 255) part = n; // because 0 is in the last part;

            if (part > n)
            {
                part = n;
            }

            if (part == n)
            {
                var offset = (part - 1) * fraction;
                result = (byte) Math.Floor((double)(offset + ((255 - offset) / 2)));
            }
            else
            {
                result = (byte)(Math.Floor((double)((part - 1) * fraction + fraction / 2)));
            }

            return (byte)result;
        }

        public static byte ReducePixelUqForTests(byte n, byte pixelValue)
        {
            return ReducePixelUq(n, pixelValue);
        }
    }
}