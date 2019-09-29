using Microsoft.Azure.Search.Models;
using Search42.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Search42.Core.Services
{
    public interface ISearchService
    {
        Task<DocumentSearchResult<CognitiveSearchResult>> SearchAsync(string term, string filters = null, IList<string> orderBy = null, IList<string> facets = null);

        Task<IEnumerable<string>> GetSuggestionsAsync(string searchText, string suggesterName);
    }
}