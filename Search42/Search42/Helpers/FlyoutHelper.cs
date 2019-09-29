using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;

namespace Search42.Helpers
{
    public static class FlyoutHelper
    {
        public static readonly DependencyProperty IsVisibleProperty = DependencyProperty.RegisterAttached(
            "IsVisible", typeof(bool), typeof(FlyoutHelper), new PropertyMetadata(false, IsVisibleChangedCallback));

        public static void SetIsVisible(DependencyObject element, bool value)
        {
            element.SetValue(IsVisibleProperty, value);
        }

        public static bool GetIsVisible(DependencyObject element)
        {
            return (bool)element.GetValue(IsVisibleProperty);
        }

        public static readonly DependencyProperty ParentProperty = DependencyProperty.RegisterAttached(
            "Parent", typeof(FrameworkElement), typeof(FlyoutHelper), null);

        public static void SetParent(DependencyObject element, FrameworkElement value)
        {
            element.SetValue(ParentProperty, value);
        }

        public static FrameworkElement GetParent(DependencyObject element)
        {
            return (FrameworkElement)element.GetValue(ParentProperty);
        }

        private static void IsVisibleChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is FlyoutBase fb))
            {
                return;
            }

            if ((bool)e.NewValue)
            {
                fb.Closed += flyout_Closed;
                fb.ShowAt(GetParent(d));
            }
            else
            {
                fb.Closed -= flyout_Closed;
                fb.Hide();
            }
        }

        private static void flyout_Closed(object sender, object e)
        {
            // When the flyout is closed, sets its IsVisible attached property to false.
            SetIsVisible(sender as DependencyObject, false);
        }
    }
}
