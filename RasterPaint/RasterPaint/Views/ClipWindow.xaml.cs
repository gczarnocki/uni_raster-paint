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
using RasterPaint.Objects;

namespace RasterPaint.Views
{
    /// <summary>
    /// Interaction logic for ClipWindow.xaml
    /// </summary>
    public partial class ClipWindow : Window
    {
        private WriteableBitmap _wb;
        private List<MyObject> _listOfAllObjects; 

        public ClipWindow(WriteableBitmap wb, List<MyObject> listOfAllObjects)
        {
            InitializeComponent();
            this.DataContext = this;

            _wb = wb;
            _listOfAllObjects = listOfAllObjects;
        }

        private void ImageGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            MyImage.Source = _wb;
        }
    }
}
