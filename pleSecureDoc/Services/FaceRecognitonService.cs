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
            _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _config["FaceApi:Key"]);
            _personGroupId = _config["FaceApi:PersonGroupId"];
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
            await _httpClient.PutAsync(url, content);
        }

        public async Task<string> RegisterEmployerFaceAsync(string base64Image, string employerName)
        {
            // 1. Create Person
            var createPersonUrl = $"{_config["FaceApi:Endpoint"]}/face/v1.0/persongroups/{_personGroupId}/persons";
            var personPayload = new { name = employerName };
            var personContent = new StringContent(JsonConvert.SerializeObject(personPayload), Encoding.UTF8, "application/json");

            var personRes = await _httpClient.PostAsync(createPersonUrl, personContent);
            var personJson = await personRes.Content.ReadAsStringAsync();
            var personId = JsonConvert.DeserializeObject<dynamic>(personJson).personId.ToString();

            // 2. Add Face
            var addFaceUrl = $"{_config["FaceApi:Endpoint"]}/face/v1.0/persongroups/{_personGroupId}/persons/{personId}/persistedFaces";
            var imageBytes = Convert.FromBase64String(base64Image);
            var byteContent = new ByteArrayContent(imageBytes);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            await _httpClient.PostAsync(addFaceUrl, byteContent);

            // 3. Train Group
            var trainUrl = $"{_config["FaceApi:Endpoint"]}/face/v1.0/persongroups/{_personGroupId}/train";
            await _httpClient.PostAsync(trainUrl, null);

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
            var detectJson = await detectResponse.Content.ReadAsStringAsync();
            var faces = JsonConvert.DeserializeObject<List<dynamic>>(detectJson);

            if (faces == null || faces.Count == 0) return false;

            string faceId = faces[0].faceId;

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
            var verifyJson = await verifyResponse.Content.ReadAsStringAsync();
            dynamic verifyResult = JsonConvert.DeserializeObject(verifyJson);

            return verifyResult.isIdentical == true && verifyResult.confidence > 0.5;
        }
    }
}
