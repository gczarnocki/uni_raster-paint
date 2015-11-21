using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RasterPaint.Objects;
using RasterPaint.Utilities;

namespace RasterPaintTests
{
    [TestClass()]
    public class ColorReductionTests
    {
        /// <summary>
        /// Test for value = 0;
        /// </summary>
        [TestMethod]
        public void CheckIfZeroReturnsZero() // początek przedziału;
        {
            byte pixelValue = 141;
            var value = ColorReduction.ReducePixelUqForTests(0, pixelValue);

            Assert.AreEqual(pixelValue, value);
        }

        [TestMethod]
        public void CheckReducing0Within2Intervals()
        {
            byte pixelValue = 0;
            byte expected = 64;
            var value = ColorReduction.ReducePixelUqForTests(2, pixelValue);

            Assert.AreEqual(expected, value);
        }

        [TestMethod]
        public void CheckReducing0Within4Intervals()
        {
            byte pixelValue = 0;
            byte expected = 32;
            var value = ColorReduction.ReducePixelUqForTests(4, pixelValue);

            Assert.AreEqual(expected, value);
        }

        [TestMethod]
        public void CheckReducing255Within2Intervals()
        {
            byte pixelValue = 255;
            byte expected = 191;
            var value = ColorReduction.ReducePixelUqForTests(2, pixelValue);

            Assert.AreEqual(expected, value);
        }

        [TestMethod]
        public void CheckReducing255Within4Intervals()
        {
            byte pixelValue = 255;
            byte expected = 223;
            var value = ColorReduction.ReducePixelUqForTests(4, pixelValue);

            Assert.AreEqual(expected, value);
        }

        [TestMethod]
        public void CheckReducing0Within6Intervals()
        {
            byte pixelValue = 0;
            byte expected = 21;
            var value = ColorReduction.ReducePixelUqForTests(6, pixelValue);

            Assert.AreEqual(expected, value);
        }

        [TestMethod]
        public void CheckReducing255Within6Intervals()
        {
            byte pixelValue = 255;
            byte expected = 232;
            var value = ColorReduction.ReducePixelUqForTests(6, pixelValue);

            Assert.AreEqual(expected, value);
        }

        [TestMethod]
        public void CheckReducingIntoSecondIntervalWithin3Intervals()
        {
            byte pixelValue = 127; // 256 / 3 = ~ 85; within second interval;
            byte expected = 85 + (85 - 1)/2;

            var value = ColorReduction.ReducePixelUqForTests(3, pixelValue);
            Assert.AreEqual(expected, value);
        }

        [TestMethod]
        public void CheckReducingIntoThirdIntervalWithin3Intervals()
        {
            byte pixelValue = 253; // 256 / 3 = ~ 85; 255 - (85 * 2) = 85; 85 / 2 = ~ 42;
            byte expected = 85 + 85 + 42;

            Trace.WriteLine(Math.Round((double)85 / 2));

            var value = ColorReduction.ReducePixelUqForTests(3, pixelValue);
            Assert.AreEqual(expected, value);
        }
        
        [TestMethod]
        public void CheckReducingIntoFourthIntervalWithin11Intervals()
        {
            byte fraction = (byte)Math.Round((double)256/11);
            byte pixelValue = (byte) (3*fraction + 1);
            byte expected = (byte)Math.Floor((double)3*fraction + fraction/2);

            var value = ColorReduction.ReducePixelUqForTests(11, pixelValue);
            Assert.AreEqual(expected, value);
        }

        [TestMethod]
        public void CheckReducingIntoLastIntervalWithin20Intervals()
        {
            byte pixelValue = 253;
            byte expected = 241;

            var value = ColorReduction.ReducePixelUqForTests(20, pixelValue);
            Assert.AreEqual(expected, value);
        }

        [TestMethod]
        public void CheckReducing255Within38Intervals()
        {
            byte pixelValue = 255;
            byte expected = 238;

            var value = ColorReduction.ReducePixelUqForTests(38, pixelValue);
            Assert.AreEqual(expected, value);
        }

        [TestMethod]
        public void _CheckLinqTakeWhenDictionaryHasLessElements()
        {
            Dictionary<int, int> exampleDictionary = new Dictionary<int, int>();

            for (int i = 0; i < 1; i++)
            {
                exampleDictionary.Add(i, i);
            }

            var collection = exampleDictionary.Select(b => b.Key).Take(2).ToArray();

            Assert.AreEqual(collection.Length, 1);
            Assert.AreEqual(collection[0], 0);
        }
    }
}