using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RasterPaint
{
    public partial class MainWindow
    {
        WriteableBitmap _wb;

        private bool _showGrid;
        private bool _removalMode;
        private bool _drawingPolygon;
        private bool _moveObjectMode;
        private bool _editObjectMode; // tryby aplikacji;

        public int GridCellSize { get; set; }

        private Point _lastPoint;
        private Point _firstPoint;
        private Point _movePoint;
        private Point _lastMovePoint;

        private MyPolygon _temporaryObject;
        private MyPolygon _objectToMove;

        readonly List<MyPolygon> _objectsList; // wszystkie obiekty;

        public Brush ButtonBrush { get; set; } = Brushes.LightGray;
        public Brush EnabledBrush { get; set; } = Brushes.LightGreen;
        public Color WhiteColor { get; set; } = Colors.White;

        // właściwości:

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
                _removalMode = value;
                RemoveObjectButton.Background = value ? EnabledBrush : ButtonBrush;
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
                RemovalMode = false;
                _moveObjectMode = value;
                MoveObjectButton.Background = value ? EnabledBrush : ButtonBrush;

                if (ShowGrid)
                {
                    DrawGrid();
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
                EditObjectButton.Background = value ? EnabledBrush : ButtonBrush;
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            _objectsList = new List<MyPolygon>();
            RemovalMode = false;
            ShowGrid = false;
        }

        private void DrawGridButton_Click(object sender, RoutedEventArgs e)
        {
            ShowGrid = !ShowGrid;

            DrawGrid();
            RedrawAllObjects(_wb);
        }

        private void RemoveObjectButton_Click(object sender, RoutedEventArgs e)
        {
            RemovalMode = !RemovalMode;
        }

        private void MoveObjectButton_Click(object sender, RoutedEventArgs e)
        {
            MoveObjectMode = !MoveObjectMode;
        }

        private void EditObjectButton_OnClickObjectButton_Click(object sender, RoutedEventArgs e)
        {
            EditObjectMode = !EditObjectMode;
        }

        private void MyImage_ButtonDown(object sender, MouseButtonEventArgs e) // kliknięcie na bitmapę;
        {
            Point p = e.GetPosition(MyImage);
            MyPolygon mo = new MyPolygon();

            if (!DrawingPolygon && !RemovalMode && !MoveObjectMode) // zaczynamy rysować wielokąt;
            {
                DrawingPolygon = true;
                _temporaryObject = new MyPolygon();

                _firstPoint = _lastPoint = p;
            }
            else if (RemovalMode) // tryb usuwania;
            {
                foreach (var item in _objectsList)
                {
                    if (item.ObjectMyBoundary.Contains(p))
                    {
                        mo = item;

                        mo.HighlightObject(true, _wb);

                        if (MessageBox.Show("Czy chcesz usunąć podświetlony obiekt?", "Usuwanie obiektu", MessageBoxButton.OKCancel) 
                            == MessageBoxResult.OK)
                        {
                            break; // wiemy, że mamy usunąć obiekt;
                        }

                        mo.HighlightObject(false, _wb); // wyczyść podświetlenie;
                        mo = new MyPolygon();
                        RemovalMode = true;
                    }
                }
            }
            else if (MoveObjectMode)
            {
                _movePoint = e.GetPosition(MyImage);

                foreach (var item in _objectsList)
                {
                    if (item.ObjectMyBoundary.Contains(_movePoint))
                    {
                        mo = item;
                        mo.HighlightObject(true, _wb); // podświetlenie obiektu;
                        break;
                    }
                }

                _objectToMove = mo;
            }

            if(RemovalMode && !mo.Equals(null))
            {
                EraseObject(mo);

                if (ShowGrid)
                {
                    DrawGrid();
                }

                RedrawAllObjects(_wb);
            }
        }

        private void MyImage_ButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (DrawingPolygon && !RemovalMode) // wciąż rysujemy wielokąt;
            {
                Point point = e.GetPosition(MyImage);
                Point snappedPoint = SnapTemporaryPoint(point, 15); // funkcja "snap", 15px;

                if (snappedPoint.Equals(_firstPoint))
                {
                    ClosePolygon();
                }
                else
                {
                    if (ObjectColor.SelectedColor != null)
                    {
                        _temporaryObject.DrawAndAdd(_wb, new MyLine(_lastPoint, point), ObjectColor.SelectedColor.Value);
                    }
                }

                _lastPoint = point;
            }
            else if (MoveObjectMode)
            {
                Point p = e.GetPosition(MyImage);
                Vector v = new Vector(p.X - _movePoint.X, p.Y - _movePoint.Y);

                MyPolygon newObject = _objectToMove.MoveObject(v);

                EraseObject(_objectToMove);
                AddObjectToList(newObject);

                RedrawAllObjects(_wb);
            }
        }

        private void MyImage_MouseLeave(object sender, MouseEventArgs e)
        {
            if (DrawingPolygon)
            {
                if (_temporaryObject.LinesList.Count > 1)
                {
                    Point point = _firstPoint;

                    ClosePolygon();

                    _lastPoint = point;
                }
                else
                {
                    DrawingPolygon = false;

                    foreach (var item in _temporaryObject.LinesList)
                    {
                        BitmapExtensions.DrawLine(_wb, item.StartPoint, item.EndPoint, ObjectColor.SelectedColor.Value);
                    }

                    ClearTemporaryObject();
                }
            }

            DrawingPolygon = false;
        }

        private void ImageGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _wb = new WriteableBitmap((int)e.NewSize.Width, (int)e.NewSize.Height, 96, 96, PixelFormats.Bgra32, null);
            MyImage.Source = _wb;

            DrawGrid();
            RedrawAllObjects(_wb);
        }

        private void DrawGrid()
        {
            if (GridColor.SelectedColor == null) return;

            var color = ShowGrid ? GridColor.SelectedColor.Value : WhiteColor; // wybór koloru;

            for (int i = 0; i <= Math.Max(ImageGrid.ActualWidth, ImageGrid.ActualHeight); i += GridCellSize)
            {
                BitmapExtensions.DrawLine(_wb, new Point(i, 0), new Point(i, ImageGrid.ActualWidth), color);
                BitmapExtensions.DrawLine(_wb, new Point(0, i), new Point(ImageGrid.ActualWidth, i), color); // narysowanie siatki;
            }
        }

        private void ClosePolygon()
        {
            DrawingPolygon = false; // wielokąt "zamknięty";

            _temporaryObject.DrawAndAdd(_wb, new MyLine(_lastPoint, _firstPoint), ObjectColor.SelectedColor.Value);
            AddObjectToList(_temporaryObject.Clone());
            ClearTemporaryObject();
        }

        private void ClearTemporaryObject()
        {
            _temporaryObject.LinesList.RemoveAll(x => true);
            _temporaryObject.Color = Colors.Transparent;
        }

        private void AddObjectToList(MyPolygon mo)
        {
            if (!_objectsList.Contains(mo))
            {
                _objectsList.Add(mo);
            }
        }

        private Point SnapTemporaryPoint(Point p, int distance)
        {
            if (distance > 0)
            {
                foreach (var item in _temporaryObject.LinesList)
                {
                    if (DistanceBetweenPoints(p, item.StartPoint) <= distance)
                    {
                        return item.StartPoint;
                    }

                    if (DistanceBetweenPoints(p, item.EndPoint) <= distance)
                    {
                        return item.EndPoint;
                    }
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
                foreach (var line in item.LinesList)
                {
                    BitmapExtensions.DrawLine(wb, line.StartPoint, line.EndPoint, item.Color);
                }
            }
        }

        private void EraseObject(MyPolygon mo)
        {
            foreach (var item in mo.LinesList)
            {
                BitmapExtensions.DrawLine(_wb, item.StartPoint, item.EndPoint, WhiteColor);
            }

            _objectsList.Remove(mo);
        }

        private void HelpButton_OnClick(object sender, RoutedEventArgs e)
        {
            HelpWindow hw = new HelpWindow();
            hw.ShowDialog();
        }

        private void EraseLine(Point startPoint, Point endPoint)
        {
            BitmapExtensions.DrawLine(_wb, startPoint, endPoint, WhiteColor);
        }

        private void MyImage_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && !ShowGrid && DrawingPolygon)
            {
                Point p = e.GetPosition(MyImage);

                EraseLine(_lastPoint, _lastMovePoint);

                RedrawObject(_temporaryObject);

                BitmapExtensions.DrawLine(_wb, _lastPoint, p, ObjectColor.SelectedColor.Value);

                _lastMovePoint = p;
            }
        }

        private void RedrawObject(MyPolygon myPolygon)
        {
            foreach (var item in myPolygon.LinesList)
            {
                BitmapExtensions.DrawLine(_wb, item.StartPoint, item.EndPoint, ObjectColor.SelectedColor.Value);
            }
        }

        private void GridSize_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            GridCellSize = (int)e.NewValue;
        }
    }
}