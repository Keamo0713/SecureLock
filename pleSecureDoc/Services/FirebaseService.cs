using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.Extensions.Configuration;
using pleSecureDoc.Models;
using System;
using System.Threading.Tasks;

namespace pleSecureDoc.Services
{
    public class FirebaseService
    {
        private readonly FirebaseClient _client;

        public FirebaseService(IConfiguration config)
        {
            var url = config["Firebase:DatabaseUrl"];
            var secret = config["Firebase:Secret"];
            _client = new FirebaseClient(url, new FirebaseOptions
            {
                AuthTokenAsyncFactory = () => Task.FromResult(secret)
            });
        }

        public async Task LogRequest(DocumentRequest request)
        {
            await _client
                .Child("requests")
                .Child(request.RequestId)
                .PutAsync(request);
        }

        public async Task<DocumentRequest> GetRequest(string requestId)
        {
            return await _client
                .Child("requests")
                .Child(requestId)
                .OnceSingleAsync<DocumentRequest>();
        }

        public async Task ApproveRequest(string requestId)
        {
            var request = await GetRequest(requestId);
            request.IsApproved = true;
            request.ApprovalTime = DateTime.UtcNow;
            await LogRequest(request);
        }

        // 🔹 Save employer's personId
        public async Task SaveEmployerPersonIdAsync(string employerId, string personId)
        {
            await _client
                .Child("personIds")
                .Child(employerId)
                .PutAsync(personId);
        }

        // 🔹 Get employer's personId
        public async Task<string> GetEmployerPersonIdAsync(string employerId)
        {
            return await _client
                .Child("personIds")
                .Child(employerId)
                .OnceSingleAsync<string>();
        }
    }
}
