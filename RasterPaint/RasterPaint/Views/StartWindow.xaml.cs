using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
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
        }

        private void MainProgram_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mWnd = new MainWindow();
            mWnd.Show();

            // this.Close();
        }

        private void ColorReduction_Click(object sender, RoutedEventArgs e)
        {
            ColorReductionWindow crWnd = new ColorReductionWindow();
            crWnd.Show();

            // this.Close();
        }
    }
}
