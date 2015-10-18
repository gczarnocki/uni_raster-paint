﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RasterPaint
{
    class MyLine : MyObject
    {
        public Point StartPoint { get; set; }
        public Point EndPoint { get; set; }

        public MyLine(Point startPoint, Point endPoint)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
        }

        public MyLine()
        {
            StartPoint = EndPoint = new Point(0, 0);
        }

        public override MyObject MoveObject(Vector v)
        {
            return new MyLine(new Point(StartPoint.X + v.X, StartPoint.Y + v.Y), new Point(EndPoint.X + v.X, EndPoint.Y + v.Y)) { Color = Color, Width = Width, MyBoundary = MyBoundary };
        }

        public override MyObject Clone()
        {
            return new MyLine(StartPoint, EndPoint) { Color = Color, Width = Width, MyBoundary = MyBoundary };
        }

        public override void UpdateBoundaries()
        {
            MyBoundary.Reset();
            MyBoundary.UpdateBoundary(StartPoint.X, StartPoint.Y);
            MyBoundary.UpdateBoundary(EndPoint.X, EndPoint.Y);
        }

        public override void DrawObject(WriteableBitmap wb)
        {
            wb.DrawLine(StartPoint, EndPoint, Color, Width);
        }

        public override void EraseObject(List<MyObject> list, WriteableBitmap wb, Color c)
        {
            wb.DrawLine(StartPoint, EndPoint, c, Width);

            if (list.Contains(this))
            {
                list.Remove(this);
            }
        }

        public override void HighlightObject(bool ifHighlight, WriteableBitmap wb, Color c)
        {
            Color color = ifHighlight ? c : Color;

            wb.DrawLine(StartPoint, EndPoint, color, Width);
        }

        public void DrawAndAddLine(WriteableBitmap wb, MyLine myLine, Color color)
        {
            Color = color;
            StartPoint = myLine.StartPoint;
            EndPoint = myLine.EndPoint;
            UpdateBoundaries();

            wb.DrawLine(myLine.StartPoint, myLine.EndPoint, color, Width);
        }
    }
}
