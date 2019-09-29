using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace Search42.Converters
{
    public class AutoSuggestionBoxTextChangeReasonToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is AutoSuggestBoxTextChangedEventArgs args)
            {
                return args.Reason == AutoSuggestionBoxTextChangeReason.UserInput;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
