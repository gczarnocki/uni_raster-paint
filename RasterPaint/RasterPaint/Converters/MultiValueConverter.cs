using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace RasterPaint.Converters
{
    class MultiValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var bytes = values.Select(x => (byte) x).Where(x => x > 0).Select(x => x);

            int result = bytes.Aggregate(1, (current, item) => current * item);

            return result != 1 ? result.ToString() : "0";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
