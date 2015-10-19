using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace RasterPaint
{
    /// <summary>
    /// Interaction logic for ListWindow.xaml
    /// </summary>
    public partial class ListWindow : Window
    {
        private readonly List<MyObject> _moList;
        private readonly WriteableBitmap _wb;
        private readonly Color _c;

        public ListWindow(List<MyObject> moList, WriteableBitmap wb, Color c)
        {
            InitializeComponent();
            _moList = moList;
            _wb = wb;
            _c = c;
            Objects.ItemsSource = new ObservableCollection<MyObject>(_moList);
        }

        private void RemoveAllObjectsButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (
                MessageBox.Show("Czy chcesz usunąć wszystkie obiekty ze sceny?",
                                "Usunięcie obiektów ze sceny",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Question,
                                MessageBoxResult.No,
                                MessageBoxOptions.None) == MessageBoxResult.Yes)
            {
                _moList.RemoveAll(x => true);
                Objects.ItemsSource = new ObservableCollection<MyObject>(_moList);

                _wb.Clear(_c);

                Close();
            }
        }
    }
}
