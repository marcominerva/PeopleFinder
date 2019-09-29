using Search42.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Data;

namespace Search42.Converters
{
    public class SearchResultToMapLayerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is IEnumerable<CognitiveSearchResult> searchResult)
            {
                // Create the layer on the map with all the positions.
                var itemsWithPosition = searchResult.Where(i => (i.Location?.Position?.Latitude.HasValue ?? false) && (i.Location?.Position?.Longitude.HasValue ?? false));
                var searchResultMapLayer = new List<MapLayer> { new MapElementsLayer
                {
                    MapElements = new List<MapElement>(itemsWithPosition.Select(i => new MapIcon
                    {
                        Location = new Geopoint(new BasicGeoposition
                        {
                            Latitude = i.Location.Position.Latitude.Value,
                            Longitude = i.Location.Position.Longitude.Value
                        }),
                        NormalizedAnchorPoint = new Point(0.5, 1.0),
                        ZIndex = 0,
                        Title = i.TakenAt.ToString("dd MMM yyyy"), //  i.Location.Address.FreeformAddress,
                        CollisionBehaviorDesired= MapElementCollisionBehavior.RemainVisible,
                        Tag = i
                    })),
                    ZIndex = 1
                }};

                return searchResultMapLayer;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
