using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using RasterPaint.Objects;
using RasterPaint;
using RasterPaint.Annotations;
using RasterPaint.Utilities;

namespace RasterPaint.Views
{
    /// <summary>
    /// Interaction logic for ClipWindow.xaml
    /// </summary>
    public sealed partial class ClipWindow : Window, INotifyPropertyChanged
    {
        private WriteableBitmap _wb;
        private Color _backgroundColor;
        private readonly MainWindow _mainWindow;

        private int _xPos;
        private int _yPos;
        private int _xSize;
        private int _ySize;

        private Point _startPoint;
        private Point _endPoint;
        private Point _lastMovePoint;

        public Color BackgroundColor
        {
            get { return _backgroundColor; }

            set
            {
                if (Wb == null) return;
                _backgroundColor = value;
                Wb.Clear(BackgroundColor);
                OnPropertyChanged();
            }
        }

        public List<MyObject> ListOfAllObjects { get; set; }

        public WriteableBitmap Wb
        {
            get { return _wb; }

            set
            {
                if (value != null)
                {
                    _wb = value;
                }
            }
        }

        public int XPos
        {
            get
            {
                return _xPos;
            }

            set
            {
                _xPos = value;
                OnPropertyChanged(nameof(XPos));
            }
        }

        public int YPos
        {
            get
            {
                return _yPos;
            }

            set
            {
                _yPos = value;
                OnPropertyChanged(nameof(YPos));
            }
        }

        public int XSize
        {
            get
            {
                return _xSize;
            }

            set
            {
                _xSize = value;
                OnPropertyChanged(nameof(XSize));
            }
        }

        public int YSize
        {
            get
            {
                return _ySize;
            }

            set
            {
                _ySize = value;
                OnPropertyChanged(nameof(YSize));
            }
        }

        public ClipRectangle ClipRect { get; set; }

        public ClipWindow(List<MyObject> listOfAllObjects, MainWindow mw)
        {
            InitializeComponent();
            this.DataContext = this;

            XPos = 5;
            YPos = 5;
            XSize = 250;
            YSize = 250;

            _mainWindow = mw;
            ListOfAllObjects = listOfAllObjects;
            BackgroundColor = _mainWindow.BackgroundColor;

            if(Wb != null) Wb.Clear(BackgroundColor);

            OnPropertyChanged(nameof(XPos));
        }

        private void ImageGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Wb = new WriteableBitmap((int)e.NewSize.Width, (int)e.NewSize.Height, 96, 96, PixelFormats.Bgra32, null);
            MyImage.Source = Wb;

            Wb.Clear(BackgroundColor);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            if (propertyName != null) Trace.WriteLine(propertyName);

            if (Wb != null)
            {
                ClipRect = new ClipRectangle(XPos, YPos, XSize, YSize);

                Wb.Clear(BackgroundColor);
                Wb.DrawRectangle((int)XPos, (int)YPos, (int)(XPos + XSize), (int)(YPos + YSize), Colors.Red);

                foreach (var item in ListOfAllObjects)
                {
                    if (item is MyPolygon)
                    {
                        var array = PolygonClipping.GetIntersectedPolygon(((MyPolygon)item).LinesList.Select(x => x.StartPoint).ToArray(), ClipRect.ToArray());

                        for (int i = 0; i < array.Count(); i++)
                        {
                            Wb.DrawLine(array[i], array[(i + 1) % array.Count()], item.Color, 0);
                        }
                    }

                }
            }
        }

        private void ClipWindow_OnClosed(object sender, EventArgs e)
        {
            _mainWindow.ClipWnd = null;
        }

        private void ClipWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            OnPropertyChanged();
        }

        private void ImageGrid_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(MyImage);
            _lastMovePoint = _startPoint;
        }

        private void ImageGrid_OnMouseMove(object sender, MouseEventArgs e)
        {
            Point p = e.GetPosition(MyImage);

            if (InsideBoundary(p, -5)) // inside;
            {
                this.Cursor = Cursors.SizeAll;

                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    _endPoint = e.GetPosition(MyImage);

                    double deltaX = _endPoint.X - _lastMovePoint.X;
                    double deltaY = _endPoint.Y - _lastMovePoint.Y;

                    XPos += (int)deltaX;
                    YPos += (int)deltaY;

                    _lastMovePoint = _endPoint;

                    OnPropertyChanged();
                }
            }
            else
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        private bool InsideBoundary(Point p, int offset)
        {
            if ((p.X <= XSize + XPos + offset && p.X >= XPos - offset))
            {
                if (p.Y <= YSize + YPos + offset && p.Y >= YPos - offset)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return false;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var clipHelpWindow = new ClipHelpWindow();
            clipHelpWindow.Show();
        }
    }
}
