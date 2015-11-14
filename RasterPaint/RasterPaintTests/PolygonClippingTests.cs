using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RasterPaint.Objects;
using RasterPaint.Utilities;

namespace RasterPaintTests
{
    [TestClass()]
    public class PolygonClippingTests
    {
        [TestMethod()]
        public void CohenSutherlandLineClipTest1()
        {
            MyBoundary mb = new MyBoundary(100, 100, 300, 300);
            Point p0 = new Point(200, 50);
            Point p1 = new Point(200, 150);

            var list = PolygonClipping.CohenSutherlandLineClip(mb, p0, p1);
            var correct = new List<Point> { new Point(200, 100), new Point(200, 150) };

            if (list.Count() == correct.Count())
            {
                for (int i = 0; i < list.Count(); i++)
                {
                    Assert.AreEqual(list[i].X, correct[i].X);
                    Assert.AreEqual(list[i].Y, correct[i].Y);
                }
            }
        }

        [TestMethod()]
        public void CohenSutherlandLineClipTest2()
        {
            MyBoundary mb = new MyBoundary(0, 0, 100, 100);
            Point p0 = new Point(50, 50);
            Point p1 = new Point(75, 75);

            var list = PolygonClipping.CohenSutherlandLineClip(mb, p0, p1);
            var correct = new List<Point> { new Point(50, 50), new Point(75, 75) };

            if (list.Count() == correct.Count())
            {
                for (int i = 0; i < list.Count(); i++)
                {
                    Assert.AreEqual(list[i].X, correct[i].X);
                    Assert.AreEqual(list[i].Y, correct[i].Y);
                }
            }
        }

        [TestMethod()]
        public void CohenSutherlandLineClipTest3()
        {
            MyBoundary mb = new MyBoundary(0, 0, 100, 100);
            Point p0 = new Point(150, 150);
            Point p1 = new Point(151, 151);

            var list = PolygonClipping.CohenSutherlandLineClip(mb, p0, p1);
            
            Assert.IsNull(list);
        }

        [TestMethod()]
        public void CohenSutherlandLineClipTest4()
        {
            MyBoundary mb = new MyBoundary(0, 0, 250, 500);
            Point p0 = new Point(0, 0);
            Point p1 = new Point(250, 500);

            var list = PolygonClipping.CohenSutherlandLineClip(mb, p0, p1);
            var correct = new List<Point> { new Point(0, 0), new Point(250, 500) };

            for (int i = 0; i < list.Count(); i++)
            {
                Assert.AreEqual(list[i].X, correct[i].X);
                Assert.AreEqual(list[i].Y, correct[i].Y);
            }
        }

        [TestMethod()]
        public void CohenSutherlandLineClipTest5()
        {
            MyBoundary mb = new MyBoundary(100, 100, 200, 200);
            Point p0 = new Point(0, 150);
            Point p1 = new Point(150, 0);

            var list = PolygonClipping.CohenSutherlandLineClip(mb, p0, p1);

            Assert.IsNull(list);
        }

        [TestMethod()]
        public void CohenSutherlandLineClipTest6()
        {
            MyBoundary mb = new MyBoundary(100, 100, 300, 500);
            Point p0 = new Point(100, 600);
            Point p1 = new Point(200, 400);

            var list = PolygonClipping.CohenSutherlandLineClip(mb, p0, p1);
            var correct = new List<Point> { new Point(150, 500), new Point(200, 400) };

            for (int i = 0; i < list.Count(); i++)
            {
                Assert.AreEqual(list[i].X, correct[i].X);
                Assert.AreEqual(list[i].Y, correct[i].Y);
            }
        }

        [TestMethod()]
        public void CohenSutherlandLineClipTest7()
        {
            MyBoundary mb = new MyBoundary(200, 200, 400, 400);
            Point p0 = new Point(200, 500);
            Point p1 = new Point(500, 200);

            var list = PolygonClipping.CohenSutherlandLineClip(mb, p0, p1);
            var correct = new List<Point> { new Point(300, 400), new Point(400, 300) };

            for (int i = 0; i < list.Count(); i++)
            {
                Assert.AreEqual(list[i].X, correct[i].X);
                Assert.AreEqual(list[i].Y, correct[i].Y);
            }
        }

        [TestMethod()]
        public void CohenSutherlandLineClipTest8()
        {
            MyBoundary mb = new MyBoundary(200, 200, 400, 400);
            Point p0 = new Point(200, 600);
            Point p1 = new Point(500, 0);

            var list = PolygonClipping.CohenSutherlandLineClip(mb, p0, p1);
            var correct = new List<Point> { new Point(300, 400), new Point(400, 200) };

            for (int i = 0; i < list.Count(); i++)
            {
                Assert.AreEqual(list[i].X, correct[i].X);
                Assert.AreEqual(list[i].Y, correct[i].Y);
            }
        }

        [TestMethod()]
        public void CohenSutherlandLineClipTest9()
        {
            MyBoundary mb = new MyBoundary(200, 200, 400, 400);
            Point p0 = new Point(100, 100);
            Point p1 = new Point(500, 500);

            var list = PolygonClipping.CohenSutherlandLineClip(mb, p0, p1);
            var correct = new List<Point> { new Point(200, 200), new Point(400, 400) };

            for (int i = 0; i < list.Count(); i++)
            {
                Assert.AreEqual(list[i].X, correct[i].X);
                Assert.AreEqual(list[i].Y, correct[i].Y);
            }
        }

        [TestMethod()]
        public void CohenSutherlandLineClipTest10()
        {
            MyBoundary mb = new MyBoundary(200, 200, 400, 400);
            Point p0 = new Point(200, 100);
            Point p1 = new Point(200, 500);

            var list = PolygonClipping.CohenSutherlandLineClip(mb, p0, p1);
            var correct = new List<Point> { new Point(200, 200), new Point(200, 400) };

            for (int i = 0; i < list.Count(); i++)
            {
                Assert.AreEqual(list[i].X, correct[i].X);
                Assert.AreEqual(list[i].Y, correct[i].Y);
            }
        }
    }
}