using Microsoft.Xaml.Interactivity;
using Search42.Helpers;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Search42.Behaviors
{
    public class ScrollSelectedItemIntoViewBehavior : Behavior<ListViewBase>
    {
        /// <summary> 
        /// Get or Sets the IsScrollAnimated dependency property.  
        /// </summary> 
        public bool IsScrollAnimated
        {
            get => (bool)GetValue(IsScrollAnimatedProperty);
            set => SetValue(IsScrollAnimatedProperty, value);
        }

        /// <summary> 
        /// Identifies the IsScrollAnimated dependency property. 
        /// This enables animation, styling, binding, etc...
        /// </summary> 
        public static readonly DependencyProperty IsScrollAnimatedProperty = DependencyProperty.Register(nameof(IsScrollAnimated),
                                typeof(bool),
                                typeof(ScrollSelectedItemIntoViewBehavior),
                                new PropertyMetadata(true));

        protected override void OnAttached()
        {
            AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            AssociatedObject.SelectionChanged -= AssociatedObject_SelectionChanged;
            base.OnDetaching();
        }

        private async void AssociatedObject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = e.AddedItems.FirstOrDefault();

            if (item != null)
            {
                if (IsScrollAnimated)
                {
                    await AssociatedObject.ScrollToItemAsync(item);
                }
                else
                {
                    await AssociatedObject.ScrollIntoViewAsync(item);
                }
            }
        }
    }
}
