using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using RasterPaint.Objects;

namespace RasterPaint.Views
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

        private void RemoveAllSelectedObjectsButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (
                MessageBox.Show("Czy chcesz usunąć zaznaczone obiekty ze sceny?",
                    "Usunięcie obiektów ze sceny",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question,
                    MessageBoxResult.No,
                    MessageBoxOptions.None) == MessageBoxResult.Yes)
            {
                var selectedItems = Objects.SelectedItems;

                foreach (var item in selectedItems)
                {
                    _moList.Remove((MyObject) item);
                }

                Objects.ItemsSource = new ObservableCollection<MyObject>(_moList);

                _wb.Clear(_c);

                foreach (var item in _moList)
                {
                    item.DrawObject(_wb);
                }

                Close();
            }
        }
    }
}