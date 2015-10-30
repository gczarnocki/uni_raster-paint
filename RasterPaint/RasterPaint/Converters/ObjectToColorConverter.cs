using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

using RasterPaint.Objects;

namespace RasterPaint.Converters
{
    class ObjectToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var myObject = value as MyObject;

            if (myObject == null) return null;

            var color = myObject.Color;
            return new SolidColorBrush(color);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
