using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace OpenDL.Converter
{
    public class DoubleToStringPercentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double doubleValue = (double)value;
            doubleValue *= this.Scale;
            string output = string.Format("{0:F2} %", doubleValue);

            return output;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {

            return 0.0;
        }


        private double _Scale = 1;
        public double Scale
        {
            get
            {
                return _Scale;

            }

            set
            {
                _Scale = value;
            }
        }
    }
}
