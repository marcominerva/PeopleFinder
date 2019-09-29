using System.Collections.Generic;

namespace FaceSkill.Models.Response
{
    public class WebApiResponseRecord
    {
        public string RecordId { get; }

        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();

        public List<WebApiResponseError> Errors { get; set; }

        public List<WebApiResponseWarning> Warnings { get; set; }

        public WebApiResponseRecord(string recordId)
        {
            RecordId = recordId;
        }
    }
}
