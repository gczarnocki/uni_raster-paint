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
        public static byte[] LockBitsAndGetArgbValuesEx(this Bitmap bmp, out Rectangle rectangle, out BitmapData bmpData, out IntPtr intPtr)
        {
            rectangle = new Rectangle(0, 0, bmp.Width, bmp.Height);
            bmpData = bmp.LockBits(rectangle, ImageLockMode.ReadWrite, bmp.PixelFormat);
            intPtr = bmpData.Scan0;

            int bytes = Math.Abs(bmpData.Stride) * bmp.Height;
            byte[] rgbValues = new byte[bytes];

            System.Runtime.InteropServices.Marshal.Copy(intPtr, rgbValues, 0, bytes);

            return rgbValues;
        }

        public static void UnlockBitsEx(this Bitmap bmp, ref BitmapData bmpData, ref IntPtr intPtr, ref byte[] rgbValues)
        {
            int bytes = Math.Abs(bmpData.Stride) * bmp.Height;

            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, intPtr, bytes);

            bmp.UnlockBits(bmpData);
        }

        public static Bitmap UniformQuantization(this Bitmap bmp, int nR, int nG, int nB)
        {
            Rectangle rectangle;
            BitmapData bmpData;
            IntPtr intPtr;

            var rgbValues = bmp.LockBitsAndGetArgbValuesEx(out rectangle, out bmpData, out intPtr);

            if (bmp.PixelFormat == PixelFormat.Format32bppArgb)
            {
                for (int i = 0; i < rgbValues.Length; i += 4)
                {
                    ReducePixelUQ(nR, ref rgbValues[i + 1]);    // reduce Red;
                    ReducePixelUQ(nG, ref rgbValues[i + 2]);    // reduce Green;
                    ReducePixelUQ(nB, ref rgbValues[i + 3]);    // reduce Blue;
                    // ignore Alpha;
                }
            }
            else if (bmp.PixelFormat == PixelFormat.Format24bppRgb)
            {
                for (int i = 0; i < rgbValues.Length; i += 3)
                {
                    ReducePixelUQ(nR, ref rgbValues[i]);        // reduce Red;
                    ReducePixelUQ(nG, ref rgbValues[i + 1]);    // reduce Green;
                    ReducePixelUQ(nB, ref rgbValues[i + 2]);    // reduce Blue;
                }
            }

            bmp.UnlockBitsEx(ref bmpData, ref intPtr, ref rgbValues);

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
