using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
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
    
    public sealed partial class ColorReductionWindow : INotifyPropertyChanged
    {
        private WriteableBitmap _loadedBitmap;

        private Stopwatch _stopwatch;
        private StringBuilder _fileStatistics;
        private BackgroundWorker _backgroundWorker;

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

        private bool BitmapIsLoaded => LoadedBitmap != null;

        public bool ContinuousUpdateUqEnabled { get; set; }

        public ColorReductionWindow()
        {
            InitializeComponent();
            DataContext = this;

            PropertyChanged += ValueChanged;
            RValue = GValue = BValue = 0;

            _backgroundWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };

            _stopwatch = new Stopwatch();
            _fileStatistics = new StringBuilder();

            _backgroundWorker.ProgressChanged += (sender, args) =>
            {
                ProgressBar.Value = args.ProgressPercentage;
            };

            _backgroundWorker.RunWorkerCompleted += (sender, args) =>
            {
                ProgressLabel.Content = "Work completed.";
                ProgressBar.Value = 0;
            };
        }

        private void PopularityAlgorithmDoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;

            if (ColorsCount.Value != null)
            {
                var newBitmap = PopularityAlgorithm(LoadedBitmap, ColorsCount.Value.Value);
                SetImageSource(newBitmap);
            }
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

        private void ComputeAndShowFileStatistics(OpenFileDialog ofd)
        {
            var fileInfo = new FileInfo(ofd.FileName);

            _fileStatistics.Clear();
            _fileStatistics.AppendLine($"FileName: {ofd.SafeFileName}");
            _fileStatistics.AppendLine($"Size: {fileInfo.Length} B");

            if (BitmapIsLoaded)
            {
                _fileStatistics.AppendLine($"Width: {LoadedBitmap.PixelWidth} px");
                _fileStatistics.AppendLine($"Height: {LoadedBitmap.PixelHeight} px");
                _fileStatistics.AppendLine($"DPI: [x: {LoadedBitmap.DpiX}, y: {LoadedBitmap.DpiY}]");
            }

            FileStatisticsTextBox.Text = _fileStatistics.ToString();
        }

        private void LoadImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "Image Files (*.bmp, *.jpg, *.jpeg, *.png)|*.bmp;*.jpg;*.jpeg;*.png",
                Multiselect = false,
            };

            ofd.ShowDialog();

            try
            {
                LoadBitmapFromOpenFileDialog(ofd);
                ComputeAndShowFileStatistics(ofd);
            }
            catch (NotSupportedException)
            {
                MessageBox.Show("This file is not supported.");
            }
            catch (ArgumentException)
            {
                MessageBox.Show("Empty path is not acceptable.");
            }
        }

        private void LoadBitmapFromOpenFileDialog(OpenFileDialog ofd)
        {
            BitmapSource bitmapSource = new BitmapImage(new Uri(ofd.FileName, UriKind.RelativeOrAbsolute));

            LoadedBitmap = new WriteableBitmap(bitmapSource);

            SetImageSource(LoadedBitmap);
            SetDefaultImageSource(LoadedBitmap);
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

        private void SetDefaultImageSource(WriteableBitmap bitmap)
        {
            DefaultImage.Source = bitmap;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ValueChanged(object sender, PropertyChangedEventArgs e)
        {
            if (BitmapIsLoaded && ContinuousUpdateUqEnabled)
            {
                var newBitmap = UniformQuantization(LoadedBitmap, RValue, GValue, BValue);
                SetImageSource(newBitmap);
            }
        }

        private void SetProgressText(string text)
        {
            ProgressLabel.Content = text;
        }

        private void PopularityAlgorithm_Click(object sender, RoutedEventArgs e)
        {
            StartComputation();

            if (ColorsCount.Value != null && BitmapIsLoaded)
            {
                var newBitmap = PopularityAlgorithm(LoadedBitmap, ColorsCount.Value.Value);
                SetImageSource(newBitmap);
            }

            EndComputation();
        }

        private void EndComputation()
        {
            _stopwatch.Stop();
            SetProgressText($"Computation done in {_stopwatch.Elapsed.Seconds} s {_stopwatch.Elapsed.Milliseconds} ms.");
        }

        private void StartComputation()
        {
            SetProgressText("Computation in progress, please wait...");
            _stopwatch.Reset();
            _stopwatch.Start();
        }

        private void OctreeAlgorithm_Click(object sender, RoutedEventArgs e)
        {
            StartComputation();

            if (ColorsCount.Value != null && BitmapIsLoaded)
            {
                var newBitmap = OctreeAlgorithm(LoadedBitmap, ColorsCount.Value.Value);
                SetImageSource(newBitmap);
            }

            EndComputation();
        }

        private void UniformQuantization_Click(object sender, RoutedEventArgs e)
        {
            StartComputation();

            if (BitmapIsLoaded)
            {
                var newBitmap = UniformQuantization(LoadedBitmap, RValue, GValue, BValue);
                SetImageSource(newBitmap);
            }

            EndComputation();
        }
    }
}