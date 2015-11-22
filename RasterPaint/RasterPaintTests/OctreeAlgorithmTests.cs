using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Octree = RasterPaint.Objects.Octree;

namespace RasterPaintTests
{
    [TestClass()]
    public class OctreeAlgorithmTests
    {
        [TestMethod]
        public void CheckIndexesFor_255_255_255_()
        {
            Color c = Color.FromArgb(255, 255, 255);
            var output = Octree.GetIndexesForPixel(c);
            var expected = Enumerable.Repeat(7, 8).ToArray();

            for (int i = 0; i < 7; i++)
            {
                Assert.AreEqual(expected[i], output[i]);
            }
        }

        [TestMethod]
        public void CheckIndexesFor_0_0_0_()
        {
            Color c = Color.FromArgb(0, 0, 0);
            var output = Octree.GetIndexesForPixel(c);
            var expected = Enumerable.Repeat(0, 7).ToArray();

            for (int i = 0; i < 7; i++)
            {
                Assert.AreEqual(expected[i], output[i]);
            }
        }

        [TestMethod]
        public void Get5thIndexFor_255_255_255_()
        {
            Color c = Color.FromArgb(255, 255, 255);
            var expected = 7;
            var output = Octree.GetIndexForPixel(5, c);

            Assert.AreEqual(expected, output);
        }

        [TestMethod]
        public void Get5thIndexFor_0_0_0_()
        {
            Color c = Color.FromArgb(0, 0, 0);
            var expected = 0;
            var output = Octree.GetIndexForPixel(5, c);

            Assert.AreEqual(expected, output);
        }
    }
}
