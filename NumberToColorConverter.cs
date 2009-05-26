using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;

namespace GeniusTetris
{
    class NumberToColorConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            byte bvalue = (byte)(int)value;
            if (bvalue >= (byte)'a')
            {
                //string key = String.Format("Option_{0}", (char)bvalue);
                string key = "chor";
                return System.Windows.Application.Current.Resources[key];
            }
            else
            {
                string key = String.Format("Block{0}", value);
                if (System.Windows.Application.Current.Resources.Contains(key))
                    return System.Windows.Application.Current.Resources[key];
                return Brushes.Transparent;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}
