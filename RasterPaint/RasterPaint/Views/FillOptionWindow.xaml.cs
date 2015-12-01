using System;
using System.Drawing;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using RasterPaint.Objects;
using Color = System.Windows.Media.Color;

namespace RasterPaint.Views
{
    /// <summary>
    /// Interaction logic for FillOptionWindow.xaml
    /// </summary>

    public enum ChosenOption
    {
        None,
        SolidBrush,
        ImageBrush
    }

    public partial class FillOptionWindow : Window, IDisposable
    {
        public ChosenOption ChosenOption { get; private set; } = ChosenOption.None;
        public WriteableBitmap LoadedFillBitmap { get; private set; }
        public Color LoadedColor { get; set; }
        private MyPolygon PolygonToFill { get; set; }

        public FillOptionWindow(MyPolygon myPolygon)
        {
            InitializeComponent();
            PolygonToFill = myPolygon;
        }

        public void Dispose()
        {
            
        }

        private void ImageBrushChosen(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "All Graphics (*.bmp, *.jpg, *.jpeg, *.png)|*.bmp;*.jpg;*.jpeg;*.png",
                Multiselect = false,
            };

            if (ofd.ShowDialog() == true)
            {
                var bitmapSource = new BitmapImage(new Uri(ofd.FileName, UriKind.RelativeOrAbsolute));
                LoadedFillBitmap = new WriteableBitmap(bitmapSource);

                ChosenOption = ChosenOption.ImageBrush;
                DialogResult = true;
            }
            else
            {
                ChosenOption = ChosenOption.None;
                DialogResult = false;
            }
        }

        private void SolidBrushChosen(object sender, MouseButtonEventArgs e)
        {
            FillColorPicker.Visibility = Visibility.Visible;            
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (FillColorPicker.SelectedColor != null)
            {
                LoadedColor = FillColorPicker.SelectedColor.Value;
            }

            DialogResult = ChosenOption != ChosenOption.None;
            Close();
        }

        private void FillColorPicker_OnSelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            ChosenOption = ChosenOption.SolidBrush;
        }
    }
}
