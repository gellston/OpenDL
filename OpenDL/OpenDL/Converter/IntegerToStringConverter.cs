using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace OpenDL.Converter
{
    public class IntegerToStringConverters : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int convertedValue = 0;
            try
            {
                convertedValue = int.Parse((string)value);
            }
            catch(Exception e)
            {

            }
            return convertedValue;
        }
    }
}
