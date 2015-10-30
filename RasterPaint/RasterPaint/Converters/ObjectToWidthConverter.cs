using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

using RasterPaint.Objects;

namespace RasterPaint.Converters
{
    class ObjectToWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var myObject = value as MyObject;

            if (myObject != null)
            {
                if (myObject.Width > 0)
                {
                    return myObject.Width;
                }
                
                return "(default)";
            }

            return "Width unknown!";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
