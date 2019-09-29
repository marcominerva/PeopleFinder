using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Search42.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Search42.Core.Services
{
    public class SearchService : ISearchService
    {
        private readonly ISearchIndexClient index;

        public SearchService(string serviceName, string indexName, string apiKey)
        {
            index = new SearchIndexClient(serviceName, indexName, new SearchCredentials(apiKey));
        }

        public async Task<DocumentSearchResult<CognitiveSearchResult>> SearchAsync(string term, string filters = null, IList<string> orderBy = null, IList<string> facets = null)
        {
            var searchParameters = new SearchParameters()
            {
                OrderBy = orderBy,
                Filter = filters,
                IncludeTotalResultCount = true,
                Facets = facets
            };

            var results = await index.Documents.SearchAsync<CognitiveSearchResult>($"\"{term}\"", searchParameters);
            return results;
        }

        public async Task<IEnumerable<string>> GetSuggestionsAsync(string searchText, string suggesterName)
        {
            var sp = new SuggestParameters()
            {
                UseFuzzyMatching = true,
                Top = 100
            };

            var response = await index.Documents.SuggestAsync(searchText, suggesterName, sp);

            var suggestions = response.Results.Select(x => x.Text).Distinct().OrderBy(s => s);
            return suggestions;
        }
    }
}
