using System;
using System.Reflection;
using Windows.UI.Xaml.Data;

namespace Search42.Converters
{
    public class EventArgsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null)
            {
                var propertyPath = parameter as string;
                if (!string.IsNullOrWhiteSpace(propertyPath))
                {
                    //Walk the ParameterPath for nested properties.
                    var propertyPathParts = propertyPath.Split('.');
                    var propertyValue = value;
                    foreach (var propertyPathPart in propertyPathParts)
                    {
                        var propInfo = propertyValue.GetType().GetTypeInfo().GetDeclaredProperty(propertyPathPart);
                        propertyValue = propInfo.GetValue(propertyValue);
                    }

                    return propertyValue;
                }
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
