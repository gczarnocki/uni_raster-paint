using System;
using System.Collections.Generic;
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
using System.Xml.Serialization;
using Microsoft.Win32;
using RasterPaint.Annotations;
using RasterPaint.Objects;
using RasterPaint.Utilities;

namespace RasterPaint.Views
{
    public sealed partial class MainWindow : INotifyPropertyChanged
    {
        #region Fields
        private WriteableBitmap _wb;
        public ClipWindow ClipWnd { get; set; }
        public ListWindow ListWnd { get; set; }

        #region Application Modes
        private bool _removalMode;
        private bool _drawingMode;
        private bool _moveObjectMode;
        private bool _editObjectMode;
        private bool _fillPolygonMode;
        private bool _clipPolygonMode; // application modes;
        #endregion

        #region Points and Objects
        private bool _clipWndHelpWndShown = false;

        private Point _lastPoint;
        private Point _firstPoint;
        private Point _movePoint;
        private Point _lastMovePoint;
        private Point? _clipStartPoint;
        private Point? _clipEndPoint;

        private MyLine _firstLine;
        private MyLine _secondLine;
        private MyObject _objectToMove;
        private MyObject _objectToEdit;
        private MyPolygon _polygonToClip;
        private MyPolygon _clippingPolygon;
        private MyObject _temporaryObject;
        #endregion
        #endregion

        #region Color Quantization
        private byte _rValue;
        private byte _gValue;
        private byte _bValue;

        public byte RValue
        {
            get { return _rValue; }

            set
            {
                if (value == _rValue) return;
                _rValue = value;
                OnPropertyChanged();
            }
        }

        public byte GValue
        {
            get { return _gValue; }

            set
            {
                if (value == _gValue) return;
                _gValue = value;
                OnPropertyChanged();
            }
        }

        public byte BValue
        {
            get { return _bValue; }

            set
            {
                if (value == _bValue) return;
                _bValue = value;
                OnPropertyChanged();
            }
        }

        private int ColorsCount => ColorsCountUpDown.Value ?? 0;

        private void UniformQuantization_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PopularityQuantization_Click(object sender, RoutedEventArgs e)
        {

        }

        private void OctreeQuantization_Click(object sender, RoutedEventArgs e)
        {
            if (ColorsCountUpDown.Value != null)
            {
                ColorReduction.OctreeAlgorithm(ObjectsList, BackgroundColor, GridColor, ColorsCountUpDown.Value.Value);
                
            }
        }
        #endregion

        #region Public Properties
        public List<MyObject> ObjectsList { get; }

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
                _moveObjectMode = value;
                MoveButton.Background = value ? EnabledBrush : ButtonBrush;

                DrawGrid();
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
                    _objectToEdit = null;
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
            }
        }
        #endregion
        
        #region Drawing
        private void DrawGrid(bool ifToErase = false)
        {
            //if (ShowGrid)
            //{
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
            //}
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

            if (!_clipWndHelpWndShown)
            {
                if (MessageBox.Show(
                    "Do you want to display help window?",
                    "Display Clip Help Window?",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    var clipHelpWindow = new ClipHelpWindow();
                    clipHelpWindow.Show();
                }
            }

            _clipWndHelpWndShown = true;
        }

        private void ListButton_Click(object sender, RoutedEventArgs e)
        {
            ListWnd = new ListWindow(ObjectsList, _wb, BackgroundColor);
            ListWnd.Show();
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            ClipWnd?.Close();
            ListWnd?.Close();
        }

        private void LoadButton_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = ".xml Files (*.xml)|*.xml",
                Multiselect = false,
                FileName = "Serialized.xml"
            };

            if (ofd.ShowDialog() != true) return;

            XmlSerializer xs = new XmlSerializer(typeof (SerializableObject));
            StreamReader sr = new StreamReader(ofd.OpenFile());

            SerializableObject so = xs.Deserialize(sr) as SerializableObject;

            ObjectsList.RemoveAll(x => true);

            if (so == null) return;

            foreach (var item in so.AllLines)
            {
                AddObjectToList(item);
            }

            foreach (var item in so.AllPolygons)
            {
                AddObjectToList(item);
            }

            foreach (var item in so.AllPoints)
            {
                AddObjectToList(item);
            }

            GridCellValue = so.GridSize;
            GridSize.Value = GridCellValue;

            GridColor = so.GridColor;
            BackgroundColor = so.BackgroundColor;
            
            GridColorPicker.SelectedColor = GridColor;
            BackgroundColorPicker.SelectedColor = BackgroundColor;

            ShowGrid = so.ShowGrid;
            DrawGrid();
            RedrawAllObjects(_wb);
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
                        _polygonToClip?.HighlightObject(true, _wb, Colors.OrangeRed);
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

            using (FileStream fs = new FileStream(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\Serialized.xml", FileMode.Create))
            {
                xs.Serialize(fs, new SerializableObject(ObjectsList, BackgroundColor, GridColor, GridCellValue, ShowGrid));

                UserInformation.Text = "Serialization succedded. File was saved on Desktop.";
            }
        }

        private void SavePngButton_OnClick(object sender, RoutedEventArgs e)
        {
            var filePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\SavedImage.png";

            Static.CreateThumbnail(filePath, _wb.Clone());

            MessageBox.Show("Scene successfully saved on Desktop.");
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
                    if (mo.MyBoundary.Contains(p) || (mo is MyPoint && Static.DistanceBetweenPoints(p, ((MyPoint) mo).Point) <= 15.0F))
                    {
                        myObject = mo;
                        mo.HighlightObject(true, _wb, HighlightColor);

                        if (MessageBox.Show("Do you want to delete highlighted object?",
                            "Delete the object?",
                            MessageBoxButton.OKCancel,
                            MessageBoxImage.Question)
                            == MessageBoxResult.OK)
                        {
                            removeNow = true;
                            break;
                        }
                    }

                    mo.HighlightObject(false, _wb, HighlightColor);
                    RemovalMode = true;
                }
            }
            else if (MoveObjectMode)
            {
                _movePoint = e.GetPosition(MyImage);

                foreach (var mo in ObjectsList)
                {
                    if (mo.MyBoundary.Contains(_movePoint) || (mo is MyPoint && DistanceBetweenPoints(p, ((MyPoint) mo).Point) <= Static.Distance) || mo.IfPointCloseToBoundary(_movePoint))
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
                    if (mo.MyBoundary.Contains(p) || (mo is MyPoint && DistanceBetweenPoints(p, ((MyPoint)mo).Point) <= Static.Distance) || mo.IfPointCloseToBoundary(p))
                    {
                        myObject = mo;
                        //_objectToEdit = mo as MyPolygon;
                        _objectToEdit = myObject;
                        mo.HighlightObject(true, _wb, HighlightColor);
                        break;
                    }
                }

                if (_objectToEdit is MyPolygon)
                {
                    MyPolygon myPolygon = _objectToEdit as MyPolygon;
                    Point point = SnapPoint(myPolygon, p, (int) Static.Distance);

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
                    }
                }
            }
            else if (FillPolygonMode)
            {
                foreach (var item in ObjectsList.Where(item => item is MyPolygon && item.MyBoundary.Contains(p)))
                {
                    _objectToEdit = item as MyPolygon;
                    break;
                }

                if (_objectToEdit is MyPolygon)
                {
                    MyPolygon polygonToEdit = _objectToEdit as MyPolygon;

                    using (FillOptionWindow fow = new FillOptionWindow(polygonToEdit))
                    {
                        if (fow.ShowDialog() == true)
                        {
                            if (fow.ChosenOption == ChosenOption.ImageBrush)
                            {
                                polygonToEdit.FillImage = fow.LoadedFillBitmap;
                                polygonToEdit.DrawObject(_wb);
                            }
                            else
                            {
                                var color = fow.LoadedColor;

                                polygonToEdit.FillImage = null;
                                polygonToEdit.FillColor = color;
                                polygonToEdit.DrawObject(_wb);
                            }
                        }
                    }
                }
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
                            ((MyPolygon) _temporaryObject).DrawAndAddLine(_wb, new MyLine(_lastPoint, point), _temporaryObject.Color);
                        }
                    }

                    _lastPoint = point;
                }
                else if (DrawingLine)
                {
                    if (ObjectColorPicker.SelectedColor != null && point != _firstPoint)
                    {
                        ((MyLine) _temporaryObject).DrawAndAddLine(_wb, new MyLine(_lastPoint, point), _temporaryObject.Color);

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
                        var newPolygonArray = PolygonClipping.GetIntersectedPolygon(_polygonToClip.GetPointsArray, _clippingPolygon.GetPointsArray);

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
                            UserInformation.Text = "Program failed to clip the polygon!";
                        }

                        _polygonToClip = null;
                        _clippingPolygon = null;
                        ClipPolygonMode = false;
                    }
                }  
            }
        }

        private void MyImage_MouseLeave(object sender, MouseEventArgs e)
        {
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
                newObject.UpdateBoundaries();

                ClearAndRedraw();
                newObject.DrawObject(_wb);

                _polygonToClip?.HighlightObject(true, _wb, Colors.Black);
            }
            else if (ClipPolygonMode)
            {
                ClearAndRedraw();

                if (MoveObjectMode)
                {
                    _polygonToClip?.HighlightObject(true, _wb, Colors.Black);
                }

                _polygonToClip?.HighlightObject(true, _wb, Colors.Black);

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
                DrawGrid(true);
                // DrawGrid();
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

            if (_objectToEdit is MyPolygon)
            {
                ((MyPolygon)_objectToEdit).FillColor = FillColor;
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

            OnPropertyChanged(nameof(ObjectsList));
            ClipWnd?.OnPropertyChanged();
        }

        public void RedrawAllObjects(WriteableBitmap wb)
        {
            foreach (MyObject item in ObjectsList)
            {
                item.DrawObject(wb);
            }
        }

        private void ClipPolygonWithRectangle()
        {
            if (_polygonToClip != null && _clipStartPoint != null && _clipEndPoint != null)
            {
                var rect = new MyRectangle(_clipStartPoint.Value, _clipEndPoint.Value);
                var polygon = _polygonToClip.LinesList.Select(x => x.StartPoint).ToArray();

                MyPolygon mp = new MyPolygon
                {
                    Color = _polygonToClip.Color,
                    FillColor = _polygonToClip.FillColor,
                    Width = _polygonToClip.Width
                };

                var intersected = PolygonClipping.GetIntersectedPolygon(polygon, rect.FourPointsList().ToArray());

                if (intersected.Count() > 0)
                {
                    for (var i = 0; i < intersected.Count(); i++)
                    {
                        mp.LinesList.Add(new MyLine(intersected[i], intersected[(i + 1) % intersected.Count()]));
                    }

                    ObjectsList.Remove(_polygonToClip);
                    _polygonToClip = null;
                    mp.UpdateBoundaries();
                    AddObjectToList(mp);
                }

            }

            _clipStartPoint = null;
            _clipEndPoint = null;
            ClipPolygonMode = false;

            ClearAndRedraw();
        }
        #endregion

        #region Static
        private static double DistanceBetweenPoints(Point a, Point b)
        {
            return Math.Sqrt((a.X - b.X)*(a.X - b.X) + (a.Y - b.Y)*(a.Y - b.Y));
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            if (propertyName == "ObjectsList" && ListWnd != null)
            {
                ListWnd.Objects.ItemsSource = ObjectsList;
            }
        }
    }
}