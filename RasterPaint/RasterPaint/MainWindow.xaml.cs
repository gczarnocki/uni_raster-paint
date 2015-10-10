using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;

namespace RasterPaint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        WriteableBitmap wb;

        bool removalMode;

        Point startPoint;
        Point endPoint;
        List<Point> pointsList;
        List<MyLine> linesList;
        List<MyObject> objectsList;

        public MainWindow()
        {
            InitializeComponent();

            // wb = new WriteableBitmap((int)imageGrid.Width, (int)imageGrid.Height, 300, 300, PixelFormats.Bgra32, null); // size derived from myImage;
            // myImage.Source = wb;

            pointsList = new List<Point>();
            linesList = new List<MyLine>();
            objectsList = new List<MyObject>();

            removalMode = false;
        }

        private void DrawGrid(object sender, RoutedEventArgs e)
        {
            for(int i = 0; i < wb.PixelHeight * 2; i += 10)
            {
                for(int j = 0; j < wb.PixelWidth * 2; j += 10)
                {
                    BitmapExtensions.SetPixel(wb, j, i, Colors.Red);
                }
            }
        }

        private void myImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(removalMode)
            {
                Point p = e.GetPosition(myImage);
                MyObject mo = new MyObject(null);

                foreach(var item in objectsList)
                {
                    if(item.objectBoundary.Contains(p))
                    {
                        mo = item;
                        MessageBox.Show("Hit!");
                        break;
                    }
                }

                foreach(var item in mo.linesList)
                {
                    BitmapExtensions.DrawLine(wb, item.StartPoint, item.EndPoint, Colors.Red);
                }
            }
            else
            {
                startPoint = e.GetPosition(myImage);
                startPoint = SnapPoint(startPoint);
                pointsList.Add(startPoint);
            }
        }

        private void myImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if(!removalMode)
            {
                endPoint = e.GetPosition(myImage);
                endPoint = SnapPoint(endPoint);
                pointsList.Add(endPoint);

                MyLine ml = new MyLine(startPoint, endPoint);
                linesList.Add(ml);

                MyObject mo = FindObjectFromPoint(startPoint, endPoint);

                if (mo != null)
                {
                    mo.linesList.Add(ml);
                }
                else
                {
                    objectsList.Add(new MyObject(ml));
                }

                BitmapExtensions.DrawLine(wb, startPoint, endPoint, Colors.Black);
            }
        }

        private void AddToList(Point p)
        {
            if(!pointsList.Contains(p))
            {
                pointsList.Add(p);
            }
        }

        private void ClearPoints()
        {
            startPoint = new Point(0, 0);
            endPoint = new Point(0, 0);
        }

        private Point SnapPoint(Point p)
        {
            foreach(Point point in pointsList)
            {
                if(DistanceBetweenPoints(p, point) <= 5.0F && !p.Equals(point))
                {
                    return point;
                }
            }

            return p;
        }

        private double DistanceBetweenPoints(Point a, Point b)
        {
            return Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
        }

        private void removeObjectButton_Click(object sender, RoutedEventArgs e)
        {
            removalMode = !removalMode;
            removeObjectButton.Background = removalMode ? Brushes.LightGreen : Brushes.LightGray;
        }

        private MyObject FindObjectFromPoint(Point p_1, Point p_2)
        {
            foreach(var item in objectsList)
            {
                foreach(var line in item.linesList)
                {
                    if(p_1.Equals(line.StartPoint) || p_1.Equals(line.EndPoint) || p_2.Equals(line.StartPoint) || p_2.Equals(line.EndPoint))
                    {
                        return item;
                    }
                }
            }

            return null;
        }

        private void MergeObjects()
        {
            // method used when we merge objects by making a line which connects them;
        }

        private void imageGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            wb = new WriteableBitmap((int)e.NewSize.Width, (int)e.NewSize.Height, 300, 300, PixelFormats.Bgra32, null);
            myImage.Source = wb;
        }
    }

    class Boundary
    {
        double x_min = int.MaxValue;
        double x_max = int.MinValue;
        double y_min = int.MaxValue;
        double y_max = int.MinValue;

        public void UpdateBoundary(double x, double y)
        {
            if (x <= x_min) x_min = x;
            if (x >= x_max) x_max = x;
            if (y <= y_min) y_min = y;
            if (y >= y_max) y_max = y;
        }

        public bool Contains(Point p)
        {
            if(p.X <= x_max && p.X >= x_min)
            {
                if (p.Y <= y_max && p.Y >= y_min)
                {
                    return true;
                }
            }

            return false;
        }
    }

    class MyLine
    {
        public Point StartPoint { get; set; }
        public Point EndPoint { get; set; }

        public MyLine(Point startPoint, Point endPoint)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
        }
    }

    class MyObject
    {
        public List<MyLine> linesList;
        public Boundary objectBoundary;

        public MyObject(MyLine ml)
        {
            linesList = new List<MyLine>();
            objectBoundary = new Boundary();
            AddLine(ml);
        }

        public void AddLine(MyLine ml)
        {
            if(ml != null)
            {
                linesList.Add(ml);
                objectBoundary.UpdateBoundary(ml.StartPoint.X, ml.StartPoint.Y);
                objectBoundary.UpdateBoundary(ml.EndPoint.X, ml.EndPoint.Y);
            }
        }
    }
}