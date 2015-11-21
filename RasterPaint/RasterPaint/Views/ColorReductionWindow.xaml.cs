using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
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

    public enum Algorithms
    {
        UniformQuantizationAlgorithm,
        PopularityAlgorithm,
        OctreeAlgorithm
    };
    
    public partial class ColorReductionWindow : INotifyPropertyChanged
    {
        private WriteableBitmap _loadedBitmap;
        private BackgroundWorker _backgroundWorker;

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

        public bool BitmapIsLoaded => LoadedBitmap != null;

        public Algorithms CurrentAlgorithm { get; set; }

        public string ProgressString 
        {
            get { return ProgressLabel.Content.ToString(); }
            set { ProgressLabel.Content = value; }
        }

        public ColorReductionWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            PropertyChanged += ValueChanged;
            RValue = GValue = BValue = 0;

            _backgroundWorker = new BackgroundWorker { WorkerReportsProgress = true };
            _backgroundWorker.DoWork += PopularityAlgorithmDoWork;
            _backgroundWorker.ProgressChanged += _backgroundWorker_ProgressChanged;
        }

        private void GetColorsCountForPopularityAlgorithm(out int colorsCount)
        {
            colorsCount = ColorsCount.Value ?? 0;
        }

        void PopularityAlgorithmDoWork(object sender, DoWorkEventArgs e)
        {

        }

        void _backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressBar.Value = e.ProgressPercentage;
        }

        public WriteableBitmap LoadedBitmap
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

            BitmapSource bitmapSource = new BitmapImage(new Uri(ofd.FileName, UriKind.RelativeOrAbsolute));
            LoadedBitmap = new WriteableBitmap(bitmapSource);
            
            SetImageSource(LoadedBitmap);
        }

        private void ResetImage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var newBitmap = UniformQuantization(LoadedBitmap, 0, 0, 0);
                LoadedBitmap = newBitmap;
                SetImageSource(LoadedBitmap);

                RValue = GValue = BValue = 0;
            }
            catch (Exception)
            {
                MessageBox.Show("You haven't loaded an image!");
            }
        }

        private void SetImageSource(WriteableBitmap bitmap)
        {
            MyImage.Source = bitmap;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ValueChanged(object sender, PropertyChangedEventArgs e)
        {
            if (BitmapIsLoaded)
            {
                var newBitmap = UniformQuantization(LoadedBitmap, RValue, GValue, BValue);
                SetImageSource(newBitmap);
            }
        }

        private void AlgorithmComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var index = AlgorithmComboBox.SelectedIndex;
        }

        private void PopularityAlgorithm_Click(object sender, RoutedEventArgs e)
        {
            if (ColorsCount.Value != null && BitmapIsLoaded)
            {
                var newBitmap = ColorReduction.PopularityAlgorithm(LoadedBitmap, ColorsCount.Value.Value;
                SetImageSource(newBitmap);
            }
        }
    }
}