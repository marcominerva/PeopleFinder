using Microsoft.Xaml.Interactivity;
using Search42.Core.Models;
using System.Collections.Generic;
using System.Linq;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Maps;

namespace Search42.Behaviors
{
    public class MapBoundingBoxBehavior : Behavior<MapControl>
    {
        public object ItemsSource
        {
            get => GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(nameof(ItemsSource), typeof(object),
            typeof(MapBoundingBoxBehavior),
            new PropertyMetadata(null, ItemsSourcePropertyChanged));

        private static void ItemsSourcePropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var behavior = sender as MapBoundingBoxBehavior;
            if (behavior.AssociatedObject == null || e.NewValue == null)
            {
                return;
            }

            if (behavior.AssociatedObject is MapControl map && behavior.ItemsSource is IEnumerable<CognitiveSearchResult> searchResult && searchResult.Any())
            {
                var itemsWithPosition = searchResult.Where(i => (i.Location?.Position?.Latitude.HasValue ?? false) && (i.Location?.Position?.Longitude.HasValue ?? false));
                if (itemsWithPosition.Any())
                {
                    // Calculate the bounding box for the zoom of the map in order to make all the pins visible.
                    var boundingBox = GeoboundingBox.TryCompute(itemsWithPosition.Select(i => new BasicGeoposition
                    {
                        Latitude = i.Location.Position.Latitude.Value,
                        Longitude = i.Location.Position.Longitude.Value
                    }));

                    if (boundingBox != null)
                    {
                        _ = map.TrySetViewBoundsAsync(boundingBox, new Thickness(100), MapAnimationKind.Bow);
                    }
                }
            }
        }
    }
}
