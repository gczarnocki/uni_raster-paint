using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using RasterPaint.Annotations;
using static RasterPaint.Utilities.ColorReduction;

namespace RasterPaint.Views
{
    /// <summary>
    /// Interaction logic for ColorReductionWindow.xaml
    /// </summary>
    public partial class ColorReductionWindow : INotifyPropertyChanged
    {
        private Bitmap _loadedBitmap;

        public ColorReductionWindow()
        {
            InitializeComponent();
        }

        public Bitmap LoadedBitmap
        {
            get { return _loadedBitmap; }

            set
            {
                _loadedBitmap = value;
                OnPropertyChanged();
            }
        }

        private void LoadImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "Image Files (*.bmp, *.jpg, *.jpeg, *.png)|*.bmp;*.jpg;*.jpeg;*.png",
                Multiselect = false,
            };

            ofd.ShowDialog();

            LoadedBitmap = (Bitmap) Image.FromFile(ofd.FileName);
            SetImageSource(LoadedBitmap);
        }

        private void ResetImage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetImageSource(LoadedBitmap);
            }
            catch (Exception)
            {
                MessageBox.Show("You haven't loaded an image!");
            }
        }

        private static BitmapImage ConvertToBitmapImage(Bitmap bmp)
        {
            MemoryStream ms = new MemoryStream();
            BitmapImage bi = new BitmapImage();

            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Position = 0;

            bi.BeginInit();
            bi.StreamSource = ms;
            bi.EndInit();

            return bi;
        }

        private void SetImageSource(Bitmap bitmap)
        {
            MyImage.Source = ConvertToBitmapImage(bitmap);
        }

        #region Conversions

        private void UniformQuantization_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var newBitmap = UniformQuantization(LoadedBitmap, 4, 4, 4);
                SetImageSource(newBitmap);
            }
            catch (Exception)
            {
                MessageBox.Show("You haven't loaded an image!");
            }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}