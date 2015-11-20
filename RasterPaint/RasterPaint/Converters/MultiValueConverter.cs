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
            byte v1 = (byte) values[0];
            byte v2 = (byte) values[1];
            byte v3 = (byte) values[2];


            int result = v1 * v2 * v3;

            return result > 0 ? result.ToString() : "Jedna z wartości równa jest 0.";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
