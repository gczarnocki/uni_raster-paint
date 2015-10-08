using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System;

namespace RasterPaint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Point startPoint;
        Point endPoint;
        List<Point> pointsList;
        List<Tuple<Point, Point>> linesList;

        public MainWindow()
        {
            InitializeComponent();
            pointsList = new List<Point>();
            linesList = new List<Tuple<Point, Point>>();
        }

        private void DrawMyLine(Point startPoint, Point endPoint)
        {
            IEnumerable<Point> pointsCollection = GetPointsOnLine((int)startPoint.X, (int)startPoint.Y, (int)endPoint.X, (int)endPoint.Y);
            
            foreach(Point point in pointsCollection)
            {
                SetPixel(point, 1);
            }          
        }

        public static IEnumerable<Point> GetPointsOnLine(int x0, int y0, int x1, int y1)
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
            yield break;
        }

        private void SetPixel(Point p, int thickness)
        {
            if(thickness >= 1)
            {
                Ellipse myEllipse = new Ellipse
                {
                    Stroke = Brushes.Black,
                    Width = Height = 1
                };

                canvasObject.Children.Add(myEllipse);
                Canvas.SetLeft(myEllipse, p.X);
                Canvas.SetTop(myEllipse, p.Y);
            }
        }

        public static double GetDistanceBetweenPoints(Point p, Point q)
        {
            double a = p.X - q.X;
            double b = p.Y - q.Y;
            double distance = Math.Sqrt(a * a + b * b);
            return distance;
        }

        private void canvasObject_MouseUp(object sender, MouseButtonEventArgs e)
        {
            endPoint = e.GetPosition(canvasObject);
            endPoint = SnapPoint(endPoint);
            pointsList.Add(endPoint);
            linesList.Add(new Tuple<Point, Point>(startPoint, endPoint));

            if (startPoint != endPoint)
            {
                DrawMyLine(startPoint, endPoint);
            }
        }

        private void canvasObject_MouseDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(canvasObject);
            startPoint = SnapPoint(startPoint);

            pointsList.Add(startPoint);
        }

        private Point SnapPoint(Point point)
        {
            foreach(Point p in pointsList)
            {
                if(GetDistanceBetweenPoints(p, point) <= 15.0F)
                {
                    return p;
                }
            }

            return point;
        }
    }
}
