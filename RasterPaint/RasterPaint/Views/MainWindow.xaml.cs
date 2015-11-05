using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Win32;
using RasterPaint.Annotations;
using RasterPaint.Objects;

namespace RasterPaint.Views
{
    public partial class MainWindow : INotifyPropertyChanged
    {
        #region Fields

        private const float Distance = 15.0F;

        private WriteableBitmap _wb;
        public ClipWindow ClipWnd { get; set; }

        #region Application Modes

        private bool _removalMode;
        private bool _drawingMode;
        private bool _moveObjectMode;
        private bool _editObjectMode;
        private bool _fillPolygonMode;
        private bool _clipPolygonMode; // application modes;

        #endregion

        #region Points and Objects

        private Point _lastPoint;
        private Point _firstPoint;
        private Point _movePoint;
        private Point _lastMovePoint;

        private MyLine _firstLine;
        private MyLine _secondLine;
        private bool ifFirst = false; // or second? -> while manipulating line;

        private MyObject _temporaryObject;
        private MyObject _objectToMove;
        private MyPolygon _objectToEdit;
        private MyPolygon _polygonToClip;
        private MyPolygon _clippingPolygon;

        private Point? _clipStartPoint;
        private Point? _clipEndPoint;

        #endregion

        #endregion

        #region Public Properties

        public List<MyObject> ObjectsList { get; }
        // public ObservableCollection<MyObject> ObjectsCollection { get; set; }

        public int GridCellValue { get; set; }
        public int LineWidthValue { get; set; }

        public Color GridColor { get; set; } = Colors.Gray;
        public Color BackgroundColor { get; set; } = Colors.LightYellow;
        public Color ObjectColor { get; set; } = Colors.DarkViolet;
        public Color HighlightColor { get; set; } = Colors.RoyalBlue;
        public Color FillColor { get; set; } = Colors.CornflowerBlue;

        public Brush ButtonBrush { get; set; } = Brushes.LightGray;
        public Brush EnabledBrush { get; set; } = Brushes.LightGreen;

        #endregion

        #region App. Logic Properties

        #region Modes

        public bool ShowGrid { get; set; }

        public bool RemovalMode
        {
            get { return _removalMode; }

            set
            {
                _removalMode = value;
                RemoveButton.Background = value ? EnabledBrush : ButtonBrush;
            }
        }

        public bool MoveObjectMode
        {
            get { return _moveObjectMode; }

            set
            {
                // RemovalMode = false;
                _moveObjectMode = value;
                MoveButton.Background = value ? EnabledBrush : ButtonBrush;

                if (ShowGrid)
                {
                    DrawGrid();
                }

                RedrawAllObjects(_wb);

                UserInformation.Text = "";
            }
        }

        public bool EditObjectMode
        {
            get { return _editObjectMode; }

            set
            {
                _editObjectMode = value;

                if (_objectToEdit != null)
                {
                    _objectToEdit.DrawObject(_wb);
                    _objectToEdit = null; // uważaj tutaj!

                    _firstLine = _secondLine = null;
                }

                EditButton.Background = value ? EnabledBrush : ButtonBrush;
            }
        }

        public bool FillPolygonMode
        {
            get { return _fillPolygonMode; }

            set
            {
                _fillPolygonMode = value;
                _objectToEdit = value ? _objectToEdit : null;
                FillButton.Background = value ? EnabledBrush : ButtonBrush;
            }
        }

        public bool ClipPolygonMode
        {
            get { return _clipPolygonMode; }

            set
            {
                _clipPolygonMode = value;
                _objectToEdit = value ? _objectToEdit : null;
                ClipButton.Background = value ? EnabledBrush : ButtonBrush;

                if (value)
                {
                    UserInformation.Text = "Right click on a polygon you want to clip and then drag the clipping polygon onto the polygon to clip.";
                }
                //else
                //{
                //    UserInformation.Text = "";
                //}
            }
        }

        #endregion

        #region Drawing

        private void DrawGrid(bool ifToErase = false)
        {
            Color color = ShowGrid ? GridColor : BackgroundColor;

            if (ifToErase)
            {
                _wb.Clear(BackgroundColor);
                color = GridColor;
            }

            for (var i = 0; i <= Math.Max(ImageGrid.ActualWidth, ImageGrid.ActualHeight); i += GridCellValue)
            {
                _wb.DrawLine(new Point(i, 0), new Point(i, ImageGrid.ActualWidth), color, 0); // 0: (default), 1 px;
                _wb.DrawLine(new Point(0, i), new Point(ImageGrid.ActualWidth, i), color, 0); // narysowanie siatki;
            }
        }

        public bool DrawingMode
        {
            get { return _drawingMode; }

            set
            {
                _drawingMode = value;

                if (ObjectColorPicker != null)
                {
                    ObjectColorPicker.IsEnabled = !value;
                    LineWidth.IsEnabled = !value;
                }
            }
        }

        public bool DrawingPolygon { get; set; }

        public bool DrawingLine { get; set; }

        public bool DrawingPoint { get; set; }

        #endregion

        #endregion

        public MainWindow()
        {
            // ShowSplashScreen();

            InitializeComponent();
            DataContext = this;

            ObjectsList = new List<MyObject>();
            RemovalMode = false;
            ShowGrid = false;
        }

        #region Event Handlers

        #region Buttons

        private void DrawGridButton_Click(object sender, RoutedEventArgs e)
        {
            ShowGrid = !ShowGrid;

            DrawGrid();
            RedrawAllObjects(_wb);
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            RemovalMode = !RemovalMode;
        }

        private void MoveButton_Click(object sender, RoutedEventArgs e)
        {
            MoveObjectMode = !MoveObjectMode;
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            EditObjectMode = !EditObjectMode;
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            HelpWindow hw = new HelpWindow();
            hw.ShowDialog();
        }

        private void FillButton_Click(object sender, RoutedEventArgs e)
        {
            FillPolygonMode = !FillPolygonMode;
        }

        private void ClipButton_Click(object sender, RoutedEventArgs e)
        {
            ClipPolygonMode = !ClipPolygonMode;
        }

        private void ListButton_Click(object sender, RoutedEventArgs e)
        {
            ListWindow lw = new ListWindow(ObjectsList, _wb, BackgroundColor);
            lw.Show();
        }

        private void LoadButton_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = ".xml Files (*.xml)|*.xml";
            ofd.Multiselect = false;
            ofd.FileName = "Serialized.xml";

            if (ofd.ShowDialog() == true)
            {
                XmlSerializer xs = new XmlSerializer(typeof (SerializableObject));
                StreamReader sr = new StreamReader(ofd.OpenFile());

                SerializableObject so = xs.Deserialize(sr) as SerializableObject;

                ObjectsList.RemoveAll(x => true);

                foreach (var item in so.AllLines)
                {
                    // ObjectsList.Add(item);
                    AddObjectToList(item);
                }

                foreach (var item in so.AllPolygons)
                {
                    // ObjectsList.Add(item);
                    AddObjectToList(item);
                }

                foreach (var item in so.AllPoints)
                {
                    // ObjectsList.Add(item);
                    AddObjectToList(item);
                }

                BackgroundColor = so.BackgroundColor;
                BackgroundColorPicker.SelectedColor = BackgroundColor;

                GridColor = so.GridColor;
                GridColorPicker.SelectedColor = GridColor;

                GridCellValue = so.GridSize;
                GridSize.Value = GridCellValue;
                ShowGrid = so.ShowGrid;

                DrawGrid();
                RedrawAllObjects(_wb);
            }
        }

        private void MyImage_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ClipPolygonMode)
            {
                Point p = e.GetPosition(MyImage);

                foreach (var obj in ObjectsList)
                {
                    if (obj.MyBoundary.Contains(p))
                    {
                        _polygonToClip = obj as MyPolygon;
                        _polygonToClip?.HighlightObject(true, _wb, Colors.Red);
                        break;
                    }
                }
            }
        }

        private void ClipWndButton_Click(object sender, RoutedEventArgs e)
        {
            if (ClipWnd != null)
            {
                ClipWnd.BackgroundColor = BackgroundColor;
                ClipWnd.ListOfAllObjects = ObjectsList;
                ClipWnd.OnPropertyChanged();

                ClipWnd.Show();
            }
            else
            {
                ClipWnd = new ClipWindow(ObjectsList, this);
                ClipWnd.Show();
            }
        }

        private void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            XmlSerializer xs = new XmlSerializer(typeof (SerializableObject));

            using (
                FileStream fs =
                    new FileStream(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\Serialized.xml",
                        FileMode.Create))
            {
                xs.Serialize(fs,
                    new SerializableObject(ObjectsList, BackgroundColor, GridColor, GridCellValue, ShowGrid));

                UserInformation.Text = "Serializacja przebiegła pomyślnie! Plik zapisano na Pulpicie.";
            }
        }

        private void DrawingType_Checked(object sender, RoutedEventArgs e)
        {
            var button = sender as RadioButton;

            if (button != null)
            {
                switch (button.Name)
                {
                    case "PolygonRadioButton":
                        DrawingPolygon = true;
                        DrawingLine = false;
                        DrawingPoint = false;
                        break;
                    case "LineRadioButton":
                        DrawingLine = true;
                        DrawingPolygon = false;
                        DrawingPoint = false;
                        break;
                    case "PointRadioButton":
                        DrawingPoint = true;
                        DrawingLine = false;
                        DrawingPolygon = false;
                        break;
                }
            }
        }

        #endregion

        #region WriteableBitmap

        private void MyImage_ButtonDown(object sender, MouseButtonEventArgs e) // kliknięcie na bitmapę;
        {
            Point p = e.GetPosition(MyImage);
            MyObject myObject = null;
            bool removeNow = false;

            if (!RemovalMode && !MoveObjectMode && !DrawingMode && !EditObjectMode && !FillPolygonMode && !ClipPolygonMode) // zaczynamy rysować;
            {
                if (DrawingPolygon)
                {
                    _temporaryObject = new MyPolygon();
                }
                else if (DrawingLine)
                {
                    _temporaryObject = new MyLine();
                }
                else if (DrawingPoint)
                {
                    _temporaryObject = new MyPoint();
                }

                DrawingMode = true;
                _temporaryObject.Width = LineWidthValue;
                _temporaryObject.Color = ObjectColor;
                _firstPoint = _lastPoint = p;
            }
            else if (RemovalMode) // tryb usuwania;
            {
                foreach (MyObject mo in ObjectsList)
                {
                    if (mo.MyBoundary.Contains(p) ||
                        (mo is MyPoint && DistanceBetweenPoints(p, ((MyPoint) mo).Point) <= 15.0F))
                    {
                        myObject = mo;
                        mo.HighlightObject(true, _wb, HighlightColor);

                        if (MessageBox.Show("Czy chcesz usunąć podświetlony obiekt?",
                            "Usuwanie obiektu",
                            MessageBoxButton.OKCancel,
                            MessageBoxImage.Question)
                            == MessageBoxResult.OK)
                        {
                            removeNow = true;
                            break; // wiemy, że mamy usunąć obiekt;
                        }
                    }

                    mo.HighlightObject(false, _wb, HighlightColor); // wyczyść podświetlenie;
                    RemovalMode = true;
                }
            }
            else if (MoveObjectMode)
            {
                _movePoint = e.GetPosition(MyImage);

                foreach (var mo in ObjectsList)
                {
                    if (mo.MyBoundary.Contains(_movePoint) ||
                        (mo is MyPoint && DistanceBetweenPoints(p, ((MyPoint) mo).Point) <= Distance) ||
                        mo.IfPointCloseToBoundary(_movePoint))
                    {
                        myObject = mo;
                        mo.HighlightObject(true, _wb, HighlightColor); // podświetlenie obiektu;
                        break;
                    }
                }

                _objectToMove = myObject;
            }
            else if (EditObjectMode)
            {
                foreach (var mo in ObjectsList)
                {
                    if (mo.MyBoundary.Contains(p))
                        // || (mo is MyPoint && DistanceBetweenPoints(p, ((MyPoint)mo).Point) <= Distance))
                    {
                        myObject = mo;
                        _objectToEdit = mo as MyPolygon;
                        mo.HighlightObject(true, _wb, HighlightColor); // podświetlenie obiektu;
                        break;
                    }
                } // mamy obiekt do zedytowania i dwie linie;

                if (_objectToEdit != null)
                {
                    MyPolygon myPolygon = _objectToEdit as MyPolygon;
                    Point point = SnapPoint(myPolygon, p, (int) Distance);

                    foreach (var item in myPolygon.LinesList)
                    {
                        if (point.Equals(item.EndPoint))
                        {
                            _firstLine = item;
                        }

                        if (point.Equals(item.StartPoint))
                        {
                            _secondLine = item;
                        }
                    } // mamy dwie linie;
                }
            }
            else if (FillPolygonMode)
            {
                foreach (var item in ObjectsList.Where(item => item is MyPolygon && item.MyBoundary.Contains(p)))
                {
                    _objectToEdit = item as MyPolygon;
                    break;
                }

                var objectToEdit = _objectToEdit;
                objectToEdit?.FillPolygonScanLine(true, _wb, FillColor);
            }
            else if (ClipPolygonMode)
            {
                _clipStartPoint = p;
            }

            if (ClipPolygonMode && MoveObjectMode)
            {
                if (_objectToMove is MyPolygon && ((MyPolygon)_objectToMove).PolygonIsConvex())
                {
                    _clippingPolygon = _objectToMove as MyPolygon;
                    // UserInformation.Text = "";
                }
                else
                {
                    UserInformation.Text = "The clipping polygon is not convex! Clipping aborted.";
                    ClipPolygonMode = false;
                    _polygonToClip = null;
                }
            }

            if (removeNow)
            {
                myObject.EraseObject(ObjectsList, _wb, BackgroundColor);

                if (ShowGrid)
                {
                    DrawGrid();
                }

                RedrawAllObjects(_wb);

                // removeNow = false;
            }
        }

        private void MyImage_ButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point point = e.GetPosition(MyImage);

            if (DrawingMode && !RemovalMode && !MoveObjectMode && !EditObjectMode && !ClipPolygonMode)
            {
                if (DrawingPolygon)
                {
                    Point snappedPoint = SnapPoint(_temporaryObject, point, 15); // "snap", 15px;

                    if (snappedPoint.Equals(_firstPoint) && ((MyPolygon) _temporaryObject).LinesList.Count > 1)
                    {
                        ClosePolygon();

                        EraseLine(_lastPoint, _lastMovePoint);
                        DrawGrid();
                        RedrawAllObjects(_wb);
                    }
                    else
                    {
                        if (ObjectColorPicker.SelectedColor != null)
                        {
                            ((MyPolygon) _temporaryObject).DrawAndAddLine(_wb, new MyLine(_lastPoint, point),
                                _temporaryObject.Color);
                        }
                    }

                    _lastPoint = point;
                }
                else if (DrawingLine)
                {
                    if (ObjectColorPicker.SelectedColor != null && point != _firstPoint)
                    {
                        ((MyLine) _temporaryObject).DrawAndAddLine(_wb, new MyLine(_lastPoint, point),
                            _temporaryObject.Color);

                        AddObjectToList(_temporaryObject.Clone());
                        ClearTemporaryObject();
                    }

                    DrawingMode = false;
                }
                else if (DrawingPoint)
                {
                    if (ObjectColorPicker.SelectedColor != null)
                    {
                        ((MyPoint) _temporaryObject).DrawAndAdd(_wb, _lastPoint, _temporaryObject.Color, _temporaryObject.Width);
                    }

                    AddObjectToList(_temporaryObject.Clone());
                    ClearTemporaryObject();

                    DrawingMode = false;
                }
            }
            else if (MoveObjectMode && !RemovalMode)
            {
                Point p = e.GetPosition(MyImage);
                Vector v = new Vector(p.X - _movePoint.X, p.Y - _movePoint.Y);

                if (_objectToMove != null && ObjectsList.Contains(_objectToMove))
                {
                    MyObject newObject = _objectToMove.MoveObject(v);
                    newObject.UpdateBoundaries();

                    if (_objectToMove is MyPolygon && _objectToMove.Equals(_clippingPolygon))
                    {
                        _clippingPolygon = newObject as MyPolygon;
                    }

                    _objectToMove.EraseObject(ObjectsList, _wb, BackgroundColor);
                    AddObjectToList(newObject);
                }

                MoveObjectMode = true;
                _wb.Clear(BackgroundColor);
                if (ShowGrid) DrawGrid(true);
                RedrawAllObjects(_wb);

                _objectToMove = null;
            }
            else if (EditObjectMode)
            {
                if (_firstLine != null && _secondLine != null && _objectToEdit != null)
                {
                    if (DrawingPolygon)
                    {
                        _firstLine.EndPoint = point;
                        _secondLine.StartPoint = point;
                    }

                    _objectToEdit.UpdateBoundaries();
                    ClearAndRedraw();
                    // _objectToEdit = new MyPolygon(); // uważaj tutaj! 
                    // _objectToEdit = null;
                }
            }
            else if (ClipPolygonMode)
            {
                _clipEndPoint = point;
                ClipPolygonWithRectangle();
            }

            if (MoveObjectMode && ClipPolygonMode)
            {
                if (_polygonToClip != null && _clippingPolygon != null)
                {
                    if (_clippingPolygon.PolygonIsConvex())
                    {
                        Trace.WriteLine($"ToClip: {_polygonToClip.XCenter} {_polygonToClip.YCenter}");
                        Trace.WriteLine($"Clipping: {_clippingPolygon.XCenter} {_clippingPolygon.YCenter}");

                        var newPolygonArray = CohenSutherland.GetIntersectedPolygon(_polygonToClip.GetPointsArray, _clippingPolygon.GetPointsArray);

                        Trace.WriteLine($"newPolygonArray.Count(): {newPolygonArray.Count()}");

                        if (newPolygonArray.Count() > 0)
                        {
                            MyPolygon mp = new MyPolygon
                            {
                                Color = _polygonToClip.Color,
                                FillColor = Colors.DarkOrange,
                                Width = _polygonToClip.Width
                            };

                            for (int i = 0; i < newPolygonArray.Count(); i++)
                            {
                                mp.LinesList.Add(new MyLine(newPolygonArray[i], newPolygonArray[(i + 1) % newPolygonArray.Count()]));
                            }

                            mp.UpdateBoundaries();
                            ObjectsList.Remove(_polygonToClip);
                            AddObjectToList(mp);
                            ClearAndRedraw();
                        }
                        else
                        {
                            UserInformation.Text = "Nie udało się!";
                        }

                        _polygonToClip = null;
                        _clippingPolygon = null;
                        ClipPolygonMode = false;
                        // MoveObjectMode = false;
                    }
                }  
            }
        }

        private void ClipPolygonWithRectangle()
        {
            if (_polygonToClip != null && _clipStartPoint != null && _clipEndPoint != null)
            {
                //var xmin = _clipStartPoint.Value.X <= _clipEndPoint.Value.X ? _clipStartPoint.Value.X : _clipEndPoint.Value.X;
                //var xmax = Math.Abs(_clipStartPoint.Value.X - xmin) < 0.0001 ? _clipEndPoint.Value.X : _clipStartPoint.Value.X;
                //var ymin = _clipStartPoint.Value.Y <= _clipEndPoint.Value.Y ? _clipStartPoint.Value.Y : _clipEndPoint.Value.Y;
                //var ymax = Math.Abs(_clipStartPoint.Value.Y - ymin) < 0.001 ? _clipEndPoint.Value.Y : _clipStartPoint.Value.Y;

                // MyRectangle myRect = new MyRectangle(_clipStartPoint.Value, _clipEndPoint.Value);
                var rect = new MyRectangle(_clipStartPoint.Value, _clipEndPoint.Value);
                var polygon = _polygonToClip.LinesList.Select(x => x.StartPoint).ToArray();

                MyPolygon mp = new MyPolygon
                {
                    Color = _polygonToClip.Color,
                    FillColor = _polygonToClip.FillColor,
                    Width = _polygonToClip.Width
                };

                var intersected = CohenSutherland.GetIntersectedPolygon(polygon, rect.FourPointsList().ToArray());

                if (intersected.Count() > 0)
                {
                    for (var i = 0; i < intersected.Count(); i++)
                    {
                        mp.LinesList.Add(new MyLine(intersected[i], intersected[(i + 1)%intersected.Count()]));
                    }

                    ObjectsList.Remove(_polygonToClip);
                    _polygonToClip = null;
                    mp.UpdateBoundaries();
                    // ObjectsList.Add(mp);
                    AddObjectToList(mp);
                }

            }

            _clipStartPoint = null;
            _clipEndPoint = null;
            ClipPolygonMode = false;

            ClearAndRedraw();
        }

        private bool DoubleEquals(double a, double b)
        {
            return Math.Abs(a - b) < 0.00001;
        }

        private void MyImage_MouseLeave(object sender, MouseEventArgs e)
        {
            Point leavePoint = e.GetPosition(MyImage);

            if (DrawingMode)
            {
                if (DrawingPolygon && _temporaryObject is MyPolygon && !RemovalMode)
                {
                    if (((MyPolygon) _temporaryObject).LinesList.Count > 1)
                    {
                        Point point = _firstPoint;

                        ClosePolygon();

                        _lastPoint = point;
                    }
                    else
                    {
                        foreach (var item in ((MyPolygon) _temporaryObject).LinesList)
                        {
                            _wb.DrawLine(item.StartPoint, item.EndPoint, BackgroundColor, LineWidthValue);
                        }

                        if (DrawingMode)
                        {
                            _wb.DrawLine(_lastPoint, _lastMovePoint, BackgroundColor, LineWidthValue);
                        }

                        ClearTemporaryObject();
                        if (ShowGrid) DrawGrid();
                        RedrawAllObjects(_wb);
                    }
                }
                else if (DrawingLine && _temporaryObject is MyLine)
                {
                    Point point = e.GetPosition(MyImage);

                    MyLine ml = new MyLine(_firstPoint, point)
                    {
                        Color = _temporaryObject.Color,
                        MyBoundary = _temporaryObject.MyBoundary,
                        Width = _temporaryObject.Width
                    };

                    ml.DrawAndAddLine(_wb, ml, ObjectColor);
                    AddObjectToList(ml);
                    _lastPoint = point;
                }

                DrawingMode = false;
            }
        }

        private void MyImage_OnMouseMove(object sender, MouseEventArgs e)
        {
            Point p = e.GetPosition(MyImage);

            if (e.LeftButton == MouseButtonState.Pressed && DrawingMode && !DrawingPoint && !RemovalMode && !ClipPolygonMode && !MoveObjectMode)
            {
                EraseLine(_lastPoint, _lastMovePoint);

                DrawGrid();
                RedrawObject(_temporaryObject);
                RedrawAllObjects(_wb);

                if (ObjectColorPicker.SelectedColor != null)
                {
                    _wb.DrawLine(_lastPoint, p, ObjectColorPicker.SelectedColor.Value, LineWidthValue);
                }
            }
            else if (MoveObjectMode && _objectToMove != null)
            {
                Vector v = new Vector(p.X - _movePoint.X, p.Y - _movePoint.Y);
                var newObject = _objectToMove.MoveObject(v);
                newObject.UpdateBoundaries(); ;

                ClearAndRedraw();
                newObject.DrawObject(_wb);
            }
            else if (ClipPolygonMode)
            {
                ClearAndRedraw();

                if (MoveObjectMode)
                {
                    _polygonToClip?.HighlightObject(true, _wb, Colors.Black);
                }
                

                if (_clipStartPoint != null)
                {
                    if (_clipStartPoint.Value.X > p.X && _clipStartPoint.Value.Y > p.Y) // wynika to z ograniczenia f-cji DrawRectangle();
                    {
                        _wb.DrawRectangle((int) p.X, (int) p.Y, (int)_clipStartPoint.Value.X, (int)_clipStartPoint.Value.Y, Colors.Green);
                    }
                    else
                    {
                        _wb.DrawRectangle((int)_clipStartPoint.Value.X, (int)_clipStartPoint.Value.Y, (int)p.X, (int)p.Y, Colors.Green);
                    }
                }
            }


            _lastMovePoint = p;
        }

        private void ImageGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _wb = new WriteableBitmap((int) e.NewSize.Width, (int) e.NewSize.Height, 96, 96, PixelFormats.Bgra32, null);
            MyImage.Source = _wb;

            _wb.Clear(BackgroundColor);
            DrawGrid();
            RedrawAllObjects(_wb);
        }

        #endregion

        #region Properties

        private void GridSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            GridCellValue = (int) e.NewValue;

            if (_wb != null)
            {
                if (ShowGrid)
                {
                    DrawGrid(true);
                }

                RedrawAllObjects(_wb);
            }
        }

        private void LineWidth_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            LineWidthValue = (int) e.NewValue;

            if (EditObjectMode && _objectToEdit != null)
            {
                _objectToEdit.Width = LineWidthValue;
            }

            if (_wb != null)
            {
                ClearAndRedraw();
            }
        }

        private void ClearAndRedraw()
        {
            _wb.Clear(BackgroundColor);
            /* if(ShowGrid) */
            DrawGrid();
            RedrawAllObjects(_wb);
        }

        private void BackgroundColor_OnSelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (e.NewValue != null && _wb != null)
            {
                BackgroundColor = e.NewValue.Value;
                ClearAndRedraw();
            }
        }

        private void ObjectColor_OnSelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (e.NewValue != null && _wb != null)
            {
                ObjectColor = e.NewValue.Value;
            }

            if (_objectToEdit != null)
            {
                _objectToEdit.Color = ObjectColor;
                _objectToEdit.DrawObject(_wb);
            }
        }

        private void GridColor_OnSelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (e.NewValue != null && _wb != null)
            {
                GridColor = e.NewValue.Value;
                ClearAndRedraw();
            }
        }

        private void FillColor_OnSelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (e.NewValue != null && _wb != null)
            {
                FillColor = e.NewValue.Value;
            }

            if (_objectToEdit != null)
            {
                _objectToEdit.FillColor = FillColor;
                _objectToEdit.DrawObject(_wb);
            }
        }

        #endregion

        #endregion

        #region TemporaryObject

        private void ClearTemporaryObject()
        {
            if (_temporaryObject is MyPolygon)
            {
                ((MyPolygon) _temporaryObject).LinesList.RemoveAll(x => true);
            }
            else if (_temporaryObject is MyLine)
            {
                ((MyLine) _temporaryObject).StartPoint = ((MyLine) _temporaryObject).EndPoint = new Point(0, 0);
            }
            else if (_temporaryObject is MyPoint)
            {
                ((MyPoint) _temporaryObject).Point = new Point(0, 0);
            }

            _temporaryObject.Color = Colors.Transparent;
            _temporaryObject.Width = 0;
            _temporaryObject.MyBoundary = new MyBoundary();
        }

        private static Point SnapPoint(MyObject mo, Point p, int distance)
        {
            if (distance > 0)
            {
                if (mo is MyPolygon)
                {
                    foreach (var item in ((MyPolygon) mo).LinesList)
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
                else if (mo is MyLine)
                {
                    if (DistanceBetweenPoints(p, ((MyLine) mo).StartPoint) <= distance)
                    {
                        return ((MyLine) mo).StartPoint;
                    }

                    if (DistanceBetweenPoints(p, ((MyLine) mo).EndPoint) <= distance)
                    {
                        return ((MyLine) mo).EndPoint;
                    }
                }
                else if (mo is MyPoint)
                {
                    if (DistanceBetweenPoints(((MyPoint) mo).Point, p) <= distance)
                    {
                        return ((MyPoint) mo).Point;
                    }
                }
            }

            return p;
        }

        #endregion

        #region Line

        private void EraseLine(Point startPoint, Point endPoint)
        {
            _wb.DrawLine(startPoint, endPoint, BackgroundColor, (int) Width);
        }

        #endregion

        #region Object

        private void ClosePolygon()
        {
            DrawingMode = false;

            ((MyPolygon) _temporaryObject).DrawAndAddLine(_wb, new MyLine(_lastPoint, _firstPoint),
                _temporaryObject.Color);

            AddObjectToList(_temporaryObject.Clone());
            ClearTemporaryObject();
        }

        private void RedrawObject(MyObject myObject)
        {
            if (ObjectColorPicker.SelectedColor != null)
            {
                if (myObject is MyPolygon)
                {
                    foreach (var item in ((MyPolygon) myObject).LinesList)
                    {
                        _wb.DrawLine(item.StartPoint, item.EndPoint, myObject.Color, myObject.Width);
                    }
                }
                else if (myObject is MyLine)
                {
                    _wb.DrawLine(((MyLine) myObject).StartPoint, ((MyLine) myObject).EndPoint, myObject.Color,
                        myObject.Width);
                }
                else if (myObject is MyPoint)
                {
                    _wb.DrawPoint(((MyPoint) myObject).Point, myObject.Color, myObject.Width);
                }
            }
        }

        private void AddObjectToList(MyObject mo)
        {
            if (!ObjectsList.Contains(mo))
            {
                ObjectsList.Add(mo);
            }

            ClipWnd?.OnPropertyChanged();
        }

        public void RedrawAllObjects(WriteableBitmap wb)
        {
            foreach (MyObject item in ObjectsList)
            {
                item.DrawObject(wb);
            }
        }

        #endregion

        #region Static

        private static double DistanceBetweenPoints(Point a, Point b)
        {
            return Math.Sqrt((a.X - b.X)*(a.X - b.X) + (a.Y - b.Y)*(a.Y - b.Y));
        }
        private static void ShowSplashScreen()
        {
            SplashScreen splash = new SplashScreen("SplashScreen.png");
            splash.Show(false, true);
            Thread.Sleep(1500);
            splash.Close(TimeSpan.FromSeconds(1));
            Thread.Sleep(1000);
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            ClipWnd?.Close();
        }
    }
}