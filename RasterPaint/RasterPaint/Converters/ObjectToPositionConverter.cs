using System;
using System.Globalization;
using System.Windows.Data;

using RasterPaint.Objects;

namespace RasterPaint.Converters
{
    class ObjectToPositionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var myObject = value as MyObject;

            if (myObject != null)
            {
                var myBoundary = myObject.MyBoundary;

                if (myObject is MyPoint)
                {
                    return "X = " + (int)(myBoundary.XMax + 5) + ", Y = " + ((int)myBoundary.YMax + 5);
                }

                return "X: <" + (int)myBoundary.XMin + ", " + (int)myBoundary.XMax + ">, Y: <" + (int)myBoundary.YMin + ", " + (int)myBoundary.YMax + ">";
            }

            return "Position unknown!";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
