using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

using RasterPaint.Objects;

namespace RasterPaint.Converters
{
    class ObjectToBgColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var myObject = value as MyObject;

            if (myObject == null) return null;

            if (myObject is MyPolygon && !((MyPolygon)myObject).IfToFillWithImage)
            {
                return new SolidColorBrush((myObject as MyPolygon).FillColor);
            }

            return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
