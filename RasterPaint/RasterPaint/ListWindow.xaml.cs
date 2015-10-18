﻿using System;
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
        private List<MyObject> _moList;
        private WriteableBitmap _wb;

        public ListWindow(List<MyObject> moList, WriteableBitmap wb)
        {
            InitializeComponent();
            _moList = moList;
            _wb = wb;
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

                _wb.Clear(Colors.White);

                this.Close();
            }
        }
    }
}
