using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace pleSecureDoc.Services
{
    public class FaceRecognitionService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;
        private readonly string _personGroupId;

        public FaceRecognitionService(IConfiguration config)
        {
            _config = config;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _config["FaceApi:Key"] ?? throw new ArgumentNullException("FaceApi:Key is not configured."));
            _personGroupId = _config["FaceApi:PersonGroupId"] ?? throw new ArgumentNullException("FaceApi:PersonGroupId is not configured.");
        }

        public async Task CreatePersonGroupAsync()
        {
            var url = $"{_config["FaceApi:Endpoint"]}/face/v1.0/persongroups/{_personGroupId}";
            var body = new
            {
                name = "Employers",
                userData = "Group for verifying employer faces"
            };

            var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(url, content);
            response.EnsureSuccessStatusCode(); // Throw exception if not successful
        }

        public async Task<string> RegisterEmployerFaceAsync(string base64Image, string employerName)
        {
            // 1. Create Person
            var createPersonUrl = $"{_config["FaceApi:Endpoint"]}/face/v1.0/persongroups/{_personGroupId}/persons";
            var personPayload = new { name = employerName };
            var personContent = new StringContent(JsonConvert.SerializeObject(personPayload), Encoding.UTF8, "application/json");

            var personRes = await _httpClient.PostAsync(createPersonUrl, personContent);
            personRes.EnsureSuccessStatusCode();
            var personJson = await personRes.Content.ReadAsStringAsync();
            dynamic? personResult = JsonConvert.DeserializeObject(personJson);
            string personId = personResult?.personId?.ToString() ?? string.Empty;

            if (string.IsNullOrEmpty(personId))
            {
                throw new InvalidOperationException("Failed to create person: personId is null or empty.");
            }

            // 2. Add Face
            var addFaceUrl = $"{_config["FaceApi:Endpoint"]}/face/v1.0/persongroups/{_personGroupId}/persons/{personId}/persistedFaces";
            var imageBytes = Convert.FromBase64String(base64Image);
            var byteContent = new ByteArrayContent(imageBytes);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            var addFaceRes = await _httpClient.PostAsync(addFaceUrl, byteContent);
            addFaceRes.EnsureSuccessStatusCode();

            // 3. Train Group
            var trainUrl = $"{_config["FaceApi:Endpoint"]}/face/v1.0/persongroups/{_personGroupId}/train";
            var trainRes = await _httpClient.PostAsync(trainUrl, null);
            trainRes.EnsureSuccessStatusCode();

            return personId;
        }

        public async Task<bool> VerifyFaceAsync(string base64Image, string personId)
        {
            // 1. Detect face from uploaded image
            var detectUrl = $"{_config["FaceApi:Endpoint"]}/face/v1.0/detect?returnFaceId=true";
            var byteArray = Convert.FromBase64String(base64Image);
            var content = new ByteArrayContent(byteArray);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            var detectResponse = await _httpClient.PostAsync(detectUrl, content);
            detectResponse.EnsureSuccessStatusCode();
            var detectJson = await detectResponse.Content.ReadAsStringAsync();
            var faces = JsonConvert.DeserializeObject<List<dynamic>>(detectJson);

            if (faces == null || faces.Count == 0) return false;

            string? faceId = faces.Count > 0 ? faces[0].faceId?.ToString() : null;
            if (string.IsNullOrEmpty(faceId)) return false;


            // 2. Verify face
            var verifyUrl = $"{_config["FaceApi:Endpoint"]}/face/v1.0/verify";
            var verifyPayload = new
            {
                faceId = faceId,
                personId = personId,
                personGroupId = _personGroupId
            };
            var verifyContent = new StringContent(JsonConvert.SerializeObject(verifyPayload), Encoding.UTF8, "application/json");
            var verifyResponse = await _httpClient.PostAsync(verifyUrl, verifyContent);
            verifyResponse.EnsureSuccessStatusCode();
            var verifyJson = await verifyResponse.Content.ReadAsStringAsync();
            dynamic? verifyResult = JsonConvert.DeserializeObject(verifyJson);

            return verifyResult?.isIdentical == true && verifyResult?.confidence > 0.5;
        }
    }
}
