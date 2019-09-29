using System.Collections.Generic;

namespace FaceSkill.Models.Response
{
    public class WebApiEnricherResponse
    {
        public List<WebApiResponseRecord> Values { get; } = new List<WebApiResponseRecord>();

        public WebApiEnricherResponse(WebApiResponseRecord respondRecord)
        {
            Values.Add(respondRecord);
        }
    }
}
