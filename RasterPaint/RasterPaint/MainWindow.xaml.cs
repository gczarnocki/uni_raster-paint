using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Diagnostics;

namespace RasterPaint
{
    public partial class MainWindow : Window
    {
        WriteableBitmap wb;

        private bool showGrid;
        private bool removalMode;
        private bool drawingPolygon;
        private bool moveObjectMode;
        private bool editObjectMode; // tryby aplikacji;

        public int GridCellSize { get; set; }

        Point lastPoint;
        Point firstPoint;
        MyObject temporaryObject;
        MyObject objectToMove;
        Point movePoint;

        List<Point> pointsList; // wszystkie punkty z bitmapy;
        List<MyLine> linesList; // wszystkie linie z bitmapy;
        List<MyObject> objectsList; // wszystkie obiekty;

        public bool ShowGrid
        {
            get
            {
                return showGrid;
            }

            set
            {
                showGrid = value;
                GridSize.IsEnabled = value ? false : true;
                GridColor.IsEnabled = value ? false : true;
            }
        }

        public bool DrawingPolygon
        {
            get
            {
                return drawingPolygon;
            }

            set
            {
                drawingPolygon = value;
                ObjectColor.IsEnabled = value ? false : true;
            }
        }

        public bool RemovalMode
        {
            get
            {
                return removalMode;
            }

            set
            {
                // MoveObjectMode = false;
                removalMode = value;
                removeObjectButton.Background = value ? Brushes.LightGreen : Brushes.LightGray;
            }
        }

        public bool MoveObjectMode
        {
            get
            {
                return moveObjectMode;
            }

            set
            {
                // RemovalMode = false;
                moveObjectMode = value;
                moveObjectButton.Background = value ? Brushes.LightGreen : Brushes.LightGray;

                if(!value)
                {
                    // objectToMove.HighlightObject(false, wb);
                    // objectToMove = new MyObject();
                }
            }
        }

        public bool EditObjectMode
        {
            get
            {
                return editObjectMode;
            }

            set
            {
                editObjectMode = value;
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            pointsList = new List<Point>();
            linesList = new List<MyLine>();
            objectsList = new List<MyObject>();

            RemovalMode = false;
            ShowGrid = false;
        }

        private void drawGridButton_Click(object sender, RoutedEventArgs e)
        {
            ShowGrid = !ShowGrid;

            DrawGrid();
            RedrawAllObjects(wb);
        }

        private void DrawGrid()
        {
            Color color = ShowGrid ? GridColor.SelectedColor.Value : Colors.White; // wybór koloru;

            for (int i = 0; i <= Math.Max(imageGrid.ActualWidth, imageGrid.ActualHeight); i += GridCellSize)
            {
                BitmapExtensions.DrawLine(wb, new Point(i, 0), new Point(i, imageGrid.ActualWidth), color);
                BitmapExtensions.DrawLine(wb, new Point(0, i), new Point(imageGrid.ActualWidth, i), color); // narysowanie siatki;
            }
        }

        private void testButton_Click(object sender, RoutedEventArgs e)
        {
            Random r = new Random();

            for (int i = 0; i < 50; i++)
            {
                MyObject mo = new MyObject();
                mo.DrawAndAdd(wb, new MyLine(new Point(r.Next(500), r.Next(500)), new Point(r.Next(500), r.Next(500))), Colors.Black);
                objectsList.Add(mo);
            }
        }

        private void removeObjectButton_Click(object sender, RoutedEventArgs e)
        {
            RemovalMode = !RemovalMode;
        }

        private void moveObjectButton_Click(object sender, RoutedEventArgs e)
        {
            MoveObjectMode = !MoveObjectMode;
        }

        private void myImage_ButtonDown(object sender, MouseButtonEventArgs e) // kliknięcie na bitmapę;
        {
            Point p = e.GetPosition(myImage);
            MyObject mo = new MyObject();

            if (!DrawingPolygon && !RemovalMode && !MoveObjectMode) // zaczynamy rysować wielokąt;
            {
                DrawingPolygon = true;
                temporaryObject = new MyObject();

                firstPoint = lastPoint = p;
            }
            else if (RemovalMode) // tryb usuwania;
            {
                // MyObject mo = new MyObject();

                foreach (var item in objectsList)
                {
                    if (item.objectBoundary.Contains(p))
                    {
                        mo = item;

                        mo.HighlightObject(true, wb);

                        if (MessageBox.Show("Czy chcesz usunąć podświetlony obiekt?", "Usuwanie obiektu", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                        {
                            break; // wiemy, że mamy usunąć obiekt;
                        }
                        else
                        {
                            mo.HighlightObject(false, wb); // wyczyść podświetlenie;
                            mo = new MyObject();
                            RemovalMode = true;
                        }
                    }
                }
            }
            else if (MoveObjectMode)
            {
                movePoint = e.GetPosition(myImage);

                foreach (var item in objectsList)
                {
                    if (item.objectBoundary.Contains(movePoint))
                    {
                        mo = item;
                        mo.HighlightObject(true, wb); // podświetlenie obiektu;
                        break;
                    }
                }
            }

            objectToMove = mo;

            if(!mo.Equals(null) && RemovalMode)
            {
                EraseObject(mo);
                if(ShowGrid) DrawGrid();
                RedrawAllObjects(wb);
            }
        }

        private void myImage_ButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (DrawingPolygon && !RemovalMode) // wciąż rysujemy wielokąt;
            {
                Point point = e.GetPosition(myImage);
                Point snappedPoint = SnapPoint(point); // funkcja "snap", 5px;

                pointsList.Add(lastPoint);
                pointsList.Add(snappedPoint); // dodajemy punkty, które tworzą linię;
                // punkty w tym przypadku powtarzają się: z każdej linii mamy ich dwa;

                if (snappedPoint.Equals(firstPoint))
                {
                    ClosePolygon();
                }
                else
                {
                    temporaryObject.DrawAndAdd(wb, new MyLine(lastPoint, point), ObjectColor.SelectedColor.Value);
                }

                lastPoint = point;
            }
            else if (MoveObjectMode)
            {
                Point p = e.GetPosition(myImage);
                Vector v = new Vector(p.X - movePoint.X, p.Y - movePoint.Y);

                // MessageBox.Show(v.ToString());

                MyObject newObject = objectToMove.MoveObject(v);

                EraseObject(objectToMove);
                AddObjectToGlobalLists(newObject);

                DrawGrid();
                RedrawAllObjects(wb);
            }
        }

        private void ClosePolygon()
        {
            DrawingPolygon = false; // wielokąt "zamknięty";

            temporaryObject.DrawAndAdd(wb, new MyLine(lastPoint, firstPoint), ObjectColor.SelectedColor.Value);
            AddObjectToGlobalLists(temporaryObject.Clone());
            ClearTemporaryObject();
        }

        private void imageGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            wb = new WriteableBitmap((int)e.NewSize.Width, (int)e.NewSize.Height, 96, 96, PixelFormats.Bgra32, null);
            myImage.Source = wb;

            DrawGrid();
            RedrawAllObjects(wb);
        }

        private void GridSize_TextChanged(object sender, TextChangedEventArgs e)
        {
            int result;
            int.TryParse(GridSize.Text, out result);
            GridCellSize = result > 0 ? result : 10;
        }

        private void myImage_MouseLeave(object sender, MouseEventArgs e)
        {
            if(DrawingPolygon)
            {
                Point point = firstPoint;

                this.ClosePolygon();

                lastPoint = point;
            }

            DrawingPolygon = false;
        }

        private void ClearTemporaryObject()
        {
            temporaryObject.linesList.RemoveAll(x => true);
            temporaryObject.pointsList.RemoveAll(x => true);
            temporaryObject.Color = Colors.Transparent;
        }

        private void AddObjectToGlobalLists(MyObject mo)
        {
            objectsList.Add(mo);

            foreach(var item in mo.linesList)
            {
                AddLinesAndPointsToLists(item);
            }
        }

        private void AddLinesAndPointsToLists(MyLine ml) // dodanie informacji do globalnych list;
        {
            if(!linesList.Contains(ml))
            {
                linesList.Add(ml);
            }

            if(!pointsList.Contains(ml.StartPoint))
            {
                pointsList.Add(ml.StartPoint);
            }

            if (!pointsList.Contains(ml.EndPoint))
            {
                pointsList.Add(ml.EndPoint);
            }
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

        private void RedrawAllObjects(WriteableBitmap wb)
        {
            foreach (var item in objectsList)
            {
                foreach (var line in item.linesList)
                {
                    BitmapExtensions.DrawLine(wb, line.StartPoint, line.EndPoint, item.Color);
                }
            }
        }

        private void EraseObject(MyObject mo)
        {
            foreach (var item in mo.linesList)
            {
                BitmapExtensions.DrawLine(wb, item.StartPoint, item.EndPoint, Colors.White);
                linesList.Remove(item);
                pointsList.Remove(item.StartPoint);
                pointsList.Remove(item.EndPoint);
            }

            objectsList.Remove(mo);
        }
    }
}