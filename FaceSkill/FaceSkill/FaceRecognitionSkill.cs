using FaceSkill.Models;
using FaceSkill.Models.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace FaceSkill
{
    public class FaceRecognitionSkill
    {
        private readonly HttpClient httpClient;
        private readonly AppSettings settings;

        public FaceRecognitionSkill(IHttpClientFactory httpClientFactory, IOptions<AppSettings> appSettings)
        {
            httpClient = httpClientFactory.CreateClient();
            settings = appSettings.Value;
        }

        [FunctionName("Face")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req, ILogger log, ExecutionContext context)
        {
            using (var inputStream = new StreamReader(req.Body))
            {
                var requestBody = await inputStream.ReadToEndAsync();
                var data = JToken.Parse(requestBody);

                // Validation
                if (data == null)
                {
                    return new BadRequestObjectResult(" Could not find values array");
                }

                if (data["values"]?.FirstOrDefault() == null)
                {
                    // It could not find a record, then return empty values array.
                    return new BadRequestObjectResult(" Could not find valid records in values array");
                }

                var recordId = data["values"].First()["recordId"]?.ToString();
                if (recordId == null)
                {
                    return new BadRequestObjectResult("recordId cannot be null");
                }

                var base64image = data["values"].First()["data"]?["image"]?["data"]?.ToString();
                if (base64image == null)
                {
                    return new BadRequestObjectResult("image data cannot be null");
                }

                // Creates the response.
                var responseRecord = new WebApiResponseRecord(recordId);
                var response = new WebApiEnricherResponse(responseRecord);

                var people = new List<string>();

                var faceClient = new FaceClient(new ApiKeyServiceClientCredentials(settings.FaceSubscriptionKey), httpClient, false)
                {
                    Endpoint = $"https://{settings.Region}.api.cognitive.microsoft.com"
                };

                var buffer = System.Convert.FromBase64String(base64image);
                using (var stream = new MemoryStream(buffer))
                {
                    var faces = await faceClient.Face.DetectWithStreamAsync(stream);

                    if (faces.Any())
                    {
                        var personGroups = await faceClient.PersonGroup.ListAsync();
                        var identifyPersonGroupId = (personGroups?.FirstOrDefault(p => p.Name.ToLower() == "default" || p.UserData.ToLower() == "default") ?? personGroups?.FirstOrDefault())?.PersonGroupId;

                        if (identifyPersonGroupId != null)
                        {
                            var faceIds = faces.Select(face => face.FaceId.Value).ToList();
                            var faceIdentificationResult = await faceClient.Face.IdentifyAsync(faceIds, identifyPersonGroupId);

                            foreach (var face in faces)
                            {
                                var candidate = faceIdentificationResult?.FirstOrDefault(r => r.FaceId == face.FaceId)?.Candidates.FirstOrDefault();
                                if (candidate != null)
                                {
                                    // Gets the person name.
                                    var person = await faceClient.PersonGroupPerson.GetAsync(identifyPersonGroupId, candidate.PersonId);
                                    people.Add(person.Name);
                                }
                            }
                        }
                    }
                }

                responseRecord.Data.Add("people", people);
                return new OkObjectResult(response);
            }
        }
    }
}