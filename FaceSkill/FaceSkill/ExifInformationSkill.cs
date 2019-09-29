using FaceSkill.Models.DataTypes;
using FaceSkill.Models.Response;
using ImageMagick;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace FaceSkill
{
    public static class ExifInformationSkill
    {
        [FunctionName("exif")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req, ILogger log, ExecutionContext context)
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

                var config = new ConfigurationBuilder()
                                    .SetBasePath(context.FunctionAppDirectory)
                                    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                                    .AddEnvironmentVariables()
                                    .Build();

                // Creates the response.
                var responseRecord = new WebApiResponseRecord(recordId);
                var response = new WebApiEnricherResponse(responseRecord);

                double? latitude = null, longitude = null;
                DateTime? takenAt = null;
                Address address = null;

                var client = new HttpClient();
                var uri = data["values"].First()["data"]?["uri"]?.ToString();
                var imageContent = await client.GetByteArrayAsync(uri);

                using (var image = new MagickImage(imageContent, 0, imageContent.Length))
                {
                    // Retrieve the exif information.
                    var profile = image.GetExifProfile();

                    var exifLatitude = profile?.Values.FirstOrDefault(v => v.Tag == ExifTag.GPSLatitude)?.Value as Rational[];
                    var exifLatitudeRef = profile?.Values.FirstOrDefault(v => v.Tag == ExifTag.GPSLatitudeRef)?.Value as string ?? "N";
                    var exifLongitude = profile?.Values.FirstOrDefault(v => v.Tag == ExifTag.GPSLongitude)?.Value as Rational[];
                    var exifLongitudeRef = profile?.Values.FirstOrDefault(v => v.Tag == ExifTag.GPSLongitudeRef)?.Value as string ?? "E";

                    latitude = ToDecimalDegrees(exifLatitude, exifLatitudeRef == "N" ? 1 : -1);
                    longitude = ToDecimalDegrees(exifLongitude, exifLongitudeRef == "E" ? 1 : -1);

                    // Perform a reverse geocoding of the address.
                    address = await GetAddressAsync(config, client, latitude, longitude);

                    var takenAtRef = (profile?.Values.FirstOrDefault(v => v.Tag == ExifTag.DateTime) ?? profile.Values.FirstOrDefault(v => v.Tag == ExifTag.DateTimeOriginal))?.Value as string;
                    if (!string.IsNullOrWhiteSpace(takenAtRef) && DateTime.TryParseExact(takenAtRef, "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                    {
                        takenAt = result;
                    }
                }

                responseRecord.Data.Add("location", new Location
                {
                    Position = EdmGeographyPoint.Create(latitude, longitude),
                    Address = address
                });

                responseRecord.Data.Add("takenAt", takenAt);

                return new OkObjectResult(response);
            }
        }

        private static double? ToDecimalDegrees(Rational[] number, int sign)
        {
            double? result = null;

            if (number?.Length == 3)
            {
                result = Math.Round(number[0].Numerator + (double)number[1].Numerator /
                    (60 * number[1].Denominator) + (double)number[2].Numerator / (3600 * number[2].Denominator), 6) * sign;
            }
            return result;
        }

        private static async Task<Address> GetAddressAsync(IConfiguration config, HttpClient client, double? latitude, double? longitude)
        {
            if (latitude.HasValue && longitude.HasValue)
            {
                var subscriptionKey = config.GetValue<string>("AppSettings:MapsSubscriptionKey");
                var reverseGeocodingResponse = await client.GetAsync($"https://atlas.microsoft.com/search/address/reverse/json?language=en-US&subscription-key={subscriptionKey}&api-version=1.0&query={latitude.ToString().Replace(",", ".")},{longitude.ToString().Replace(",", ".")}");
                if (reverseGeocodingResponse.IsSuccessStatusCode)
                {
                    var reverseGeocodingJson = await reverseGeocodingResponse.Content.ReadAsStringAsync();
                    var data = JToken.Parse(reverseGeocodingJson);
                    var address = JsonConvert.DeserializeObject<Address>(data["addresses"].FirstOrDefault()["address"].ToString());

                    return address;
                }
            }

            return null;
        }
    }
}
