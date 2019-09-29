using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Data;

namespace Search42.Converters
{
    public class StringArrayToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is IEnumerable<string> array)
            {
                return string.Join(", ", array);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
