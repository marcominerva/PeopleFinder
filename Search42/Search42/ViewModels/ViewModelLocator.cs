using System;

using GalaSoft.MvvmLight.Ioc;

using Search42.Common;
using Search42.Core.Services;
using Search42.Services;
using Search42.Views;

namespace Search42.ViewModels
{
    [Windows.UI.Xaml.Data.Bindable]
    public class ViewModelLocator
    {
        private static ViewModelLocator _current;

        public static ViewModelLocator Current => _current ?? (_current = new ViewModelLocator());

        static ViewModelLocator()
        {
            SimpleIoc.Default.Register<ISearchService>(() =>
            {
                var searchService = new SearchService(Constants.SearchServiceName, Constants.SearchIndexName, Constants.ApiKey);
                return searchService;
            });

            SimpleIoc.Default.Register(() => new NavigationServiceEx());

            Register<MainViewModel, MainPage>();
        }

        public NavigationServiceEx NavigationService => SimpleIoc.Default.GetInstance<NavigationServiceEx>();

        public MainViewModel MainViewModel => SimpleIoc.Default.GetInstance<MainViewModel>();

        public static void Register<VM, V>() where VM : class
        {
            SimpleIoc.Default.Register<VM>();
            SimpleIoc.Default.GetInstance<NavigationServiceEx>().Configure(typeof(VM).FullName, typeof(V));
        }
    }
}
