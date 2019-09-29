using Search42.Common;
using Search42.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;

namespace Search42.Views
{
    public sealed partial class MainPage : Page
    {
        private MainViewModel ViewModel => ViewModelLocator.Current.MainViewModel;

        public MainPage()
        {
            InitializeComponent();
        }

        private void MapControl_Loaded(object sender, RoutedEventArgs e)
        {
            map.MapServiceToken = Constants.MapServiceToken;
        }

        private void mapStyleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Protect against events that are raised before we are fully initialized.
            if (map != null)
            {
                switch (mapStyleComboBox.SelectedIndex)
                {
                    case 0:
                        map.Style = MapStyle.Road;
                        break;

                    case 1:
                        map.Style = MapStyle.Aerial;
                        break;

                    case 2:
                        map.Style = MapStyle.AerialWithRoads;
                        break;
                }
            }
        }
    }
}
