using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Channels;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using RasterPaint.Annotations;
using RasterPaint.Utilities;
using static RasterPaint.Utilities.ColorReduction;
using RasterPaint.Objects;

namespace RasterPaint.Views
{
    /// <summary>
    /// Interaction logic for ColorReductionWindow.xaml
    /// </summary>
    
    public sealed partial class ColorReductionWindow : INotifyPropertyChanged
    {
        private WriteableBitmap _loadedBitmap;
        private WriteableBitmap _resultBitmap;
        private BackgroundWorker _backgroundWorker;
        private int _progress = 0;

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

            //_backgroundWorker.DoWork += (sender, args) =>
            //{
            //    var worker = sender as BackgroundWorker;

            //    for (int i = 0; i < 100; i++)
            //    {
            //        Thread.Sleep(100);
            //        _progress++;

            //        worker?.ReportProgress(_progress); // 0 - 100;
            //    }
            //};

            _backgroundWorker.ProgressChanged += (sender, args) =>
            {
                ProgressBar.Value = args.ProgressPercentage;
            };

            _backgroundWorker.RunWorkerCompleted += (sender, args) =>
            {
                ProgressLabel.Content = "Work completed.";
                ProgressBar.Value = 0;
            };

            //if (_backgroundWorker.IsBusy == false)
            //{
            //    ProgressLabel.Content = "Please wait for operation to complete.";

            //    _backgroundWorker.RunWorkerAsync();
            //}
        }

        private void PopularityAlgorithmDoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;

            var newBitmap = PopularityAlgorithm(LoadedBitmap, ColorsCount.Value.Value);
            SetImageSource(newBitmap);
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

            try
            {
                BitmapSource bitmapSource = new BitmapImage(new Uri(ofd.FileName, UriKind.RelativeOrAbsolute));

                LoadedBitmap = new WriteableBitmap(bitmapSource);

                SetImageSource(LoadedBitmap);
                SetDefaultImageSource(LoadedBitmap);
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

        private void PopularityAlgorithm_Click(object sender, RoutedEventArgs e)
        {
            if (ColorsCount.Value != null && BitmapIsLoaded)
            {
                var newBitmap = PopularityAlgorithm(LoadedBitmap, ColorsCount.Value.Value);
                SetImageSource(newBitmap);
            }

            //if (ColorsCount.Value != null && BitmapIsLoaded)
            //{
            //    _backgroundWorker.DoWork += PopularityAlgorithmDoWork;

            //    if (_backgroundWorker.IsBusy == false)
            //    {
            //        ProgressLabel.Content = "Please wait for operation to complete.";

            //        _backgroundWorker.RunWorkerAsync();
            //    }

            //    _backgroundWorker.DoWork -= PopularityAlgorithmDoWork;
            //}
        }

        private void OctreeAlgorithm_Click(object sender, RoutedEventArgs e)
        {
            if (ColorsCount.Value != null && BitmapIsLoaded)
            {
                var newBitmap = OctreeAlgorithm(LoadedBitmap, ColorsCount.Value.Value);
                SetImageSource(newBitmap);
            }
        }

        private void UniformQuantization_Click(object sender, RoutedEventArgs e)
        {
            if (BitmapIsLoaded)
            {
                var newBitmap = UniformQuantization(LoadedBitmap, RValue, GValue, BValue);
                SetImageSource(newBitmap);
            }
        }
    }
}