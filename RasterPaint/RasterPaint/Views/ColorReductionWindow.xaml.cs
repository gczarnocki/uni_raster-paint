using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using RasterPaint.Annotations;
using RasterPaint.Utilities;
using static RasterPaint.Utilities.ColorReduction;

namespace RasterPaint.Views
{
    /// <summary>
    /// Interaction logic for ColorReductionWindow.xaml
    /// </summary>

    internal enum Algorithms
    {
        UniformQuantization,
        Popularity,
        Octree
    };
    
    public partial class ColorReductionWindow : INotifyPropertyChanged
    {
        private WriteableBitmap _loadedBitmap;

        private byte _rValue;
        private byte _gValue;
        private byte _bValue;
        private byte _aValue;

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

        public byte AValue
        {
            get { return _aValue; }

            set
            {
                if (value == _aValue) return;
                _aValue = value;
                OnPropertyChanged();
            }
        }

        public ColorReductionWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            RValue = GValue = BValue = AValue = 4;
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
                var newBitmap = LoadedBitmap.UniformQuantization(4, 4, 4);
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