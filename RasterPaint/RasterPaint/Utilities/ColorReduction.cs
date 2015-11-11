using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RasterPaint.Utilities
{
    static class ColorReduction
    {
        public static Bitmap UniformQuantization(Bitmap bmp, int nR, int nG, int nB)
        {
            Rectangle rectangle = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData bmpData = bmp.LockBits(rectangle, ImageLockMode.ReadWrite, bmp.PixelFormat);
            IntPtr intPtr = bmpData.Scan0;

            int bytes = Math.Abs(bmpData.Stride) * bmp.Height;
            byte[] rgbValues = new byte[bytes];

            System.Runtime.InteropServices.Marshal.Copy(intPtr, rgbValues, 0, bytes);

            if (bmp.PixelFormat == PixelFormat.Format32bppArgb)
            {
                for (int i = 0; i < rgbValues.Length; i += 4)
                {
                    ReducePixelUQ(nR, ref rgbValues[i + 1]);    // reduce Red canal;
                    ReducePixelUQ(nG, ref rgbValues[i + 2]);    // reduce Green canal;
                    ReducePixelUQ(nB, ref rgbValues[i + 3]);    // reduce Blue canal;
                }
            }
            else if (bmp.PixelFormat == PixelFormat.Format24bppRgb)
            {
                for (int i = 0; i < rgbValues.Length; i += 3)
                {
                    ReducePixelUQ(nR, ref rgbValues[i]);        // reduce Red canal;
                    ReducePixelUQ(nG, ref rgbValues[i + 1]);    // reduce Green canal;
                    ReducePixelUQ(nB, ref rgbValues[i + 2]);    // reduce Blue canal;
                }
            }

            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, intPtr, bytes);

            bmp.UnlockBits(bmpData);

            return bmp;
        }

        private static void ReducePixelUQ(int n, ref byte pixelValue)
        {
            int fraction = (int)Math.Floor((double)(256 / n));
            int part = (int) Math.Floor((double)(pixelValue / fraction));

            pixelValue = (byte)(part * fraction + fraction / 2);
        }
    }
}
