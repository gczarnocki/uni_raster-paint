using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace RasterPaint.Converters
{
    class MultiValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            //int v1 = (int)values[0];
            //int v2 = (int)values[1];
            //int v3 = (int)values[2];

            //var result = v1 * v2 * v3;

            //// Trace.WriteLine(result);
            //return result.ToString();

            return 0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
