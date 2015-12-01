using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace RasterPaint.Views
{
    /// <summary>
    /// Interaction logic for ReductionOptionWindow.xaml
    /// </summary>
    public partial class ReductionOptionWindow : Window, IDisposable
    {
        public enum Algorithms
        {
            UniformQuantization,
            PopularityQuantization,
            OctreeQuantization
        };

        public byte RValue { get; set; } 

        public byte GValue { get; set; }

        public byte BValue { get; set; }

        public uint ColorsCountValue { get; set; }

        public Algorithms SelectedAlgorithm { get; set; }

        public ReductionOptionWindow()
        {
            InitializeComponent();
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AlgorithmSelector.SelectedIndex == 0)
            {
                RUpDown.IsEnabled = true;
                GUpDown.IsEnabled = true;
                BUpDown.IsEnabled = true;
                ColorsCountUpDown.IsEnabled = false;

                SelectedAlgorithm = Algorithms.UniformQuantization;
            }
            else
            {
                RUpDown.IsEnabled = false;
                GUpDown.IsEnabled = false;
                BUpDown.IsEnabled = false;
                ColorsCountUpDown.IsEnabled = true;

                SelectedAlgorithm = AlgorithmSelector.SelectedIndex == 1 ? Algorithms.PopularityQuantization : Algorithms.OctreeQuantization;
            }
        }

        private void ReductionOptionWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            AlgorithmSelector.SelectionChanged += Selector_OnSelectionChanged;
        }

        private void RUpDown_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (RUpDown.Value != null) RValue = Convert.ToByte(RUpDown.Value);
        }

        private void GUpDown_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (GUpDown.Value != null) GValue = Convert.ToByte(GUpDown.Value);
        }

        private void BUpDown_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (BUpDown.Value != null) BValue = Convert.ToByte(BUpDown.Value);
        }

        private void ColorsCountUpDown_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (ColorsCountUpDown.Value != null) GValue = Convert.ToByte(ColorsCountUpDown.Value);
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        public void Dispose()
        {
            
        }

        private void ReductionOptionWindow_OnClosing(object sender, CancelEventArgs e)
        {

        }
    }
}
