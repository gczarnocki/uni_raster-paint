using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RasterPaint
{
    public partial class MainWindow : Window
    {
        WriteableBitmap _wb;

        private bool _showGrid;
        private bool _removalMode;
        private bool _drawingPolygon;
        private bool _moveObjectMode;
        private bool _editObjectMode; // tryby aplikacji;

        public int GridCellSize { get; set; }

        Point _lastPoint;
        Point _firstPoint;
        MyObject _temporaryObject;
        MyObject _objectToMove;
        Point _movePoint;

        List<Point> _pointsList; // wszystkie punkty z bitmapy;
        List<MyLine> _linesList; // wszystkie linie z bitmapy;
        List<MyObject> _objectsList; // wszystkie obiekty;

        public bool ShowGrid
        {
            get
            {
                return _showGrid;
            }

            set
            {
                _showGrid = value;
                GridSize.IsEnabled = !value;
                GridColor.IsEnabled = !value;
            }
        }

        public bool DrawingPolygon
        {
            get
            {
                return _drawingPolygon;
            }

            set
            {
                _drawingPolygon = value;
                ObjectColor.IsEnabled = !value;
            }
        }

        public bool RemovalMode
        {
            get
            {
                return _removalMode;
            }

            set
            {
                // MoveObjectMode = false;
                _removalMode = value;
                removeObjectButton.Background = value ? Brushes.LightGreen : Brushes.LightGray;
            }
        }

        public bool MoveObjectMode
        {
            get
            {
                return _moveObjectMode;
            }

            set
            {
                // RemovalMode = false;
                _moveObjectMode = value;
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
                return _editObjectMode;
            }

            set
            {
                _editObjectMode = value;
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            _pointsList = new List<Point>();
            _linesList = new List<MyLine>();
            _objectsList = new List<MyObject>();

            RemovalMode = false;
            ShowGrid = false;
        }

        private void drawGridButton_Click(object sender, RoutedEventArgs e)
        {
            ShowGrid = !ShowGrid;

            DrawGrid();
            RedrawAllObjects(_wb);
        }

        private void DrawGrid()
        {
            Color color = ShowGrid ? GridColor.SelectedColor.Value : Colors.White; // wybór koloru;

            for (int i = 0; i <= Math.Max(imageGrid.ActualWidth, imageGrid.ActualHeight); i += GridCellSize)
            {
                BitmapExtensions.DrawLine(_wb, new Point(i, 0), new Point(i, imageGrid.ActualWidth), color);
                BitmapExtensions.DrawLine(_wb, new Point(0, i), new Point(imageGrid.ActualWidth, i), color); // narysowanie siatki;
            }
        }

        private void testButton_Click(object sender, RoutedEventArgs e)
        {
            Random r = new Random();

            for (int i = 0; i < 50; i++)
            {
                MyObject mo = new MyObject();
                mo.DrawAndAdd(_wb, new MyLine(new Point(r.Next(500), r.Next(500)), new Point(r.Next(500), r.Next(500))), Colors.Black);
                _objectsList.Add(mo);
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
                _temporaryObject = new MyObject();

                _firstPoint = _lastPoint = p;
            }
            else if (RemovalMode) // tryb usuwania;
            {
                // MyObject mo = new MyObject();

                foreach (var item in _objectsList)
                {
                    if (item.objectBoundary.Contains(p))
                    {
                        mo = item;

                        mo.HighlightObject(true, _wb);

                        if (MessageBox.Show("Czy chcesz usunąć podświetlony obiekt?", "Usuwanie obiektu", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                        {
                            break; // wiemy, że mamy usunąć obiekt;
                        }
                        else
                        {
                            mo.HighlightObject(false, _wb); // wyczyść podświetlenie;
                            mo = new MyObject();
                            RemovalMode = true;
                        }
                    }
                }
            }
            else if (MoveObjectMode)
            {
                _movePoint = e.GetPosition(myImage);

                foreach (var item in _objectsList)
                {
                    if (item.objectBoundary.Contains(_movePoint))
                    {
                        mo = item;
                        mo.HighlightObject(true, _wb); // podświetlenie obiektu;
                        break;
                    }
                }
            }

            _objectToMove = mo;

            if(!mo.Equals(null) && RemovalMode)
            {
                EraseObject(mo);
                if(ShowGrid) DrawGrid();
                RedrawAllObjects(_wb);
            }
        }

        private void myImage_ButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (DrawingPolygon && !RemovalMode) // wciąż rysujemy wielokąt;
            {
                Point point = e.GetPosition(myImage);
                Point snappedPoint = SnapPoint(point); // funkcja "snap", 5px;

                _pointsList.Add(_lastPoint);
                _pointsList.Add(snappedPoint); // dodajemy punkty, które tworzą linię;
                // punkty w tym przypadku powtarzają się: z każdej linii mamy ich dwa;

                if (snappedPoint.Equals(_firstPoint))
                {
                    ClosePolygon();
                }
                else
                {
                    _temporaryObject.DrawAndAdd(_wb, new MyLine(_lastPoint, point), ObjectColor.SelectedColor.Value);
                }

                _lastPoint = point;
            }
            else if (MoveObjectMode)
            {
                Point p = e.GetPosition(myImage);
                Vector v = new Vector(p.X - _movePoint.X, p.Y - _movePoint.Y);

                // MessageBox.Show(v.ToString());

                MyObject newObject = _objectToMove.MoveObject(v);

                EraseObject(_objectToMove);
                AddObjectToGlobalLists(newObject);

                DrawGrid();
                RedrawAllObjects(_wb);
            }
        }

        private void ClosePolygon()
        {
            DrawingPolygon = false; // wielokąt "zamknięty";

            _temporaryObject.DrawAndAdd(_wb, new MyLine(_lastPoint, _firstPoint), ObjectColor.SelectedColor.Value);
            AddObjectToGlobalLists(_temporaryObject.Clone());
            ClearTemporaryObject();
        }

        private void imageGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _wb = new WriteableBitmap((int)e.NewSize.Width, (int)e.NewSize.Height, 96, 96, PixelFormats.Bgra32, null);
            myImage.Source = _wb;

            DrawGrid();
            RedrawAllObjects(_wb);
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
                Point point = _firstPoint;

                this.ClosePolygon();

                _lastPoint = point;
            }

            DrawingPolygon = false;
        }

        private void ClearTemporaryObject()
        {
            _temporaryObject.linesList.RemoveAll(x => true);
            _temporaryObject.pointsList.RemoveAll(x => true);
            _temporaryObject.Color = Colors.Transparent;
        }

        private void AddObjectToGlobalLists(MyObject mo)
        {
            _objectsList.Add(mo);

            foreach(var item in mo.linesList)
            {
                AddLinesAndPointsToLists(item);
            }
        }

        private void AddLinesAndPointsToLists(MyLine ml) // dodanie informacji do globalnych list;
        {
            if(!_linesList.Contains(ml))
            {
                _linesList.Add(ml);
            }

            if(!_pointsList.Contains(ml.StartPoint))
            {
                _pointsList.Add(ml.StartPoint);
            }

            if (!_pointsList.Contains(ml.EndPoint))
            {
                _pointsList.Add(ml.EndPoint);
            }
        }

        private Point SnapPoint(Point p)
        {
            foreach(Point point in _pointsList)
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
            foreach (var item in _objectsList)
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
                BitmapExtensions.DrawLine(_wb, item.StartPoint, item.EndPoint, Colors.White);
                _linesList.Remove(item);
                _pointsList.Remove(item.StartPoint);
                _pointsList.Remove(item.EndPoint);
            }

            _objectsList.Remove(mo);
        }
    }
}