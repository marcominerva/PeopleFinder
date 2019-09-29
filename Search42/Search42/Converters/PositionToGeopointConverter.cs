using Search42.Core.Models;
using System;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml.Data;

namespace Search42.Converters
{
    public class PositionToGeopointConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Position location && location.Latitude.HasValue && location.Longitude.HasValue)
            {
                var snPosition = new BasicGeoposition
                {
                    Latitude = location.Latitude.Value,
                    Longitude = location.Longitude.Value
                };

                var snPoint = new Geopoint(snPosition);
                return snPoint;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
