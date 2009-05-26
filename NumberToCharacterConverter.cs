using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;

namespace GeniusTetris
{
    class NumberToCharacterConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            byte bvalue ;

            if (value is int)
                bvalue = (byte)(int)value;
            else
                bvalue = (byte)value;
            if (bvalue >= (byte)'a')
            {
                //string key = String.Format("Option_{0}", (char)bvalue);
                return (char)bvalue;
            }
            else
            {
                return string.Empty;

            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}
