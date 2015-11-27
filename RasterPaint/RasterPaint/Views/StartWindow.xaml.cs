using System;
using System.Threading;
using System.Windows;
using RasterPaint.Views;

namespace RasterPaint
{
    /// <summary>
    /// Interaction logic for StartWindow.xaml
    /// </summary>
    public partial class StartWindow : Window
    {
        public StartWindow()
        {
            InitializeComponent();
            ShowSplashScreen();
        }

        private void MainProgram_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mWnd = new MainWindow();
            mWnd.Show();
        }

        private void ColorReduction_Click(object sender, RoutedEventArgs e)
        {
            ColorReductionWindow crWnd = new ColorReductionWindow();
            crWnd.Show();
        }

        private static void ShowSplashScreen()
        {
            SplashScreen splash = new SplashScreen("Resources/SplashScreen.png");
            splash.Show(false, true);
            Thread.Sleep(1500);
            splash.Close(TimeSpan.FromSeconds(1));
            Thread.Sleep(1000);
        }
    }
}
