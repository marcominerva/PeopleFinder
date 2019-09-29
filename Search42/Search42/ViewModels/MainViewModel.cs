using GalaSoft.MvvmLight;
using Microsoft.Azure.Search.Models;
using Search42.Common;
using Search42.Core.Models;
using Search42.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Search42.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly ISearchService searchService;

        private string searchText;
        public string SearchText
        {
            get => searchText;
            set => Set(ref searchText, value, broadcast: true);
        }

        private bool isDateFromEnabled;
        public bool IsDateFromEnabled
        {
            get => isDateFromEnabled;
            set
            {
                if (Set(ref isDateFromEnabled, value))
                {
                    SearchCommand.Execute(searchText);
                }
            }
        }

        private DateTimeOffset? dateFrom;
        public DateTimeOffset? DateFrom
        {
            get => dateFrom;
            set
            {
                if (Set(ref dateFrom, value))
                {
                    SearchCommand.Execute(searchText);
                }
            }
        }

        private bool isDateToEnabled;
        public bool IsDateToEnabled
        {
            get => isDateToEnabled;
            set
            {
                if (Set(ref isDateToEnabled, value))
                {
                    SearchCommand.Execute(searchText);
                }
            }
        }

        private DateTimeOffset? dateTo;
        public DateTimeOffset? DateTo
        {
            get => dateTo;
            set
            {
                if (Set(ref dateTo, value))
                {
                    SearchCommand.Execute(searchText);
                }
            }
        }

        private DocumentSearchResult<CognitiveSearchResult> searchResult;
        public DocumentSearchResult<CognitiveSearchResult> SearchResult
        {
            get => searchResult;
            set
            {
                if (Set(ref searchResult, value))
                {
                    RaisePropertyChanged(nameof(SearchResultItems));
                }
            }
        }

        public IEnumerable<CognitiveSearchResult> SearchResultItems => searchResult?.Results.Select(r => r.Document);

        private CognitiveSearchResult selectedItem;
        public CognitiveSearchResult SelectedItem
        {
            get => selectedItem;
            set => Set(ref selectedItem, value);
        }

        private IEnumerable<Facet> facets;
        public IEnumerable<Facet> Facets
        {
            get => facets;
            set => Set(ref facets, value);
        }

        private Facet selectedFacet;
        public Facet SelectedFacet
        {
            get => selectedFacet;
            set
            {
                if (value != null && Set(ref selectedFacet, value))
                {
                    SearchCommand.Execute(searchText);
                }
            }
        }

        private IEnumerable<string> suggestions;
        public IEnumerable<string> Suggestions
        {
            get => suggestions;
            set => Set(ref suggestions, value);
        }

        private bool isBusy;
        public bool IsBusy
        {
            get => isBusy;
            set => Set(ref isBusy, value, broadcast: true);
        }

        private int zoomLevel;
        public int ZoomLevel
        {
            get => zoomLevel;
            set => Set(ref zoomLevel, value);
        }

        private Position mapCenter;
        public Position MapCenter
        {
            get => mapCenter;
            set => Set(ref mapCenter, value);
        }

        private bool isPopupOpen;
        public bool IsPopupOpen
        {
            get => isPopupOpen;
            set => Set(ref isPopupOpen, value);
        }

        public AutoRelayCommand<string> SearchCommand { get; }

        public AutoRelayCommand<bool> TextChangedCommand { get; }

        public AutoRelayCommand<CognitiveSearchResult> ItemSelectedCommand { get; }

        public AutoRelayCommand<CognitiveSearchResult> MapItemSelectedCommand { get; }

        public AutoRelayCommand<CognitiveSearchResult> MapItemEnteredCommand { get; }

        public MainViewModel(ISearchService searchService)
        {
            this.searchService = searchService;

            SearchCommand = new AutoRelayCommand<string>(async (queryText) => await SearchAsync(queryText),
                (queryText) => !IsBusy && !string.IsNullOrWhiteSpace(queryText))
                .DependsOn(nameof(IsBusy)).DependsOn(nameof(SearchText));

            TextChangedCommand = new AutoRelayCommand<bool>(async (isUserInput) => await SuggestAsync(),
                (isUserInput) => isUserInput && searchText?.Length >= 3)
                .DependsOn(nameof(SearchText));

            ItemSelectedCommand = new AutoRelayCommand<CognitiveSearchResult>(async (item) => await ShowItemInformationAsync(item));
            MapItemSelectedCommand = new AutoRelayCommand<CognitiveSearchResult>(async (item) => await ShowMapItemInformationAsync(item));
            MapItemEnteredCommand = new AutoRelayCommand<CognitiveSearchResult>(async (item) => await MapItemEnteredAsync(item));
        }

        private async Task ShowItemInformationAsync(CognitiveSearchResult item)
        {
            MapCenter = item.Location.Position;
            ZoomLevel = 14;

            await Task.Delay(1000);

            // Show the flyout with the information related to the selected item.
            IsPopupOpen = true;
        }

        private Task ShowMapItemInformationAsync(CognitiveSearchResult item)
        {
            SelectedItem = item;

            // Show the flyout with the information related to the selected item.
            IsPopupOpen = true;

            return Task.CompletedTask;
        }

        private Task MapItemEnteredAsync(CognitiveSearchResult item)
        {
            SelectedItem = item;
            return Task.CompletedTask;
        }

        private async Task SearchAsync(string queryText)
        {
            IsBusy = true;
            SearchText = queryText;

            var filters = $"taken_at ge {(isDateFromEnabled && dateFrom.HasValue ? dateFrom.Value : DateTimeOffset.MinValue).Date.ToString("yyyy-MM-dd")} and taken_at lt {(isDateToEnabled && DateTo.HasValue ? dateTo.Value.AddDays(1) : DateTimeOffset.MaxValue).Date.ToString("yyyy-MM-dd")}";
            if (selectedFacet != null)
            {
                filters += $" and location/address/Municipality eq '{selectedFacet.Key}'";
            }

            // Query the Search Index
            SearchResult = await searchService.SearchAsync(searchText,
                orderBy: new List<string> { "taken_at desc" },
                filters: filters,
                facets: new List<string> { "location/address/Municipality,count:100" });

            if (selectedFacet == null)
            {
                Facets = searchResult.Facets.FirstOrDefault().Value.Select(f => new Facet
                {
                    Key = f.Value.ToString(),
                    Count = f.Count.GetValueOrDefault()
                }).OrderBy(f => f.Key);
            }

            selectedFacet = null;
            IsBusy = false;
            Suggestions = null;
        }

        private async Task SuggestAsync()
        {
            Suggestions = await searchService.GetSuggestionsAsync(searchText, "entities");
        }
    }
}
