using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Search42.Core.Models
{
    public class CognitiveSearchResult
    {
        public string Uri { get; set; }

        public IEnumerable<string> People { get; set; }

        [JsonProperty("taken_at")]
        public DateTime TakenAt { get; set; }

        public Location Location { get; set; }
    }
}
