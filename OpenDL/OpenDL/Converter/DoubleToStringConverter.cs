using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace OpenDL.Converter
{
    public class DoubleToStringConverters : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double convertedValue = 0;
            try
            {
                convertedValue = double.Parse((string)value);
            }
            catch (Exception e)
            {

            }
            return convertedValue;
        }


        
    }
}
