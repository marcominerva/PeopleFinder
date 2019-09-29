using Search42.Core.Models;
using System;
using System.Linq;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Data;

namespace Search42.Converters
{
    public class MapElementActionToItemConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is MapElementClickEventArgs clickArgs)
            {
                return (clickArgs.MapElements.FirstOrDefault(x => x is MapIcon) as MapIcon)?.Tag as CognitiveSearchResult;
            }

            if (value is MapElementPointerEnteredEventArgs enterArgs)
            {
                return (enterArgs.MapElement as MapIcon)?.Tag as CognitiveSearchResult;
            }

            if (value is MapElementPointerExitedEventArgs exitArgs)
            {
                return (exitArgs.MapElement as MapIcon)?.Tag as CognitiveSearchResult;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
