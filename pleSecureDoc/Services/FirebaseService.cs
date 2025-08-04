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
            var url = config["Firebase:DatabaseUrl"] ?? throw new ArgumentNullException("Firebase:DatabaseUrl is not configured.");
            var secret = config["Firebase:Secret"] ?? throw new ArgumentNullException("Firebase:Secret is not configured.");
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
            if (request != null) // Add null check
            {
                request.IsApproved = true;
                request.ApprovalTime = DateTime.UtcNow;
                await LogRequest(request);
            }
            else
            {
                // Handle case where request is not found
                Console.WriteLine($"Error: Request with ID {requestId} not found for approval.");
            }
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

        // 🔹 Get Employee by Id (New method to fix CS1061 error)
        public async Task<Employee> GetEmployeeAsync(string employeeId)
        {
            return await _client
                .Child("employees")
                .Child(employeeId)
                .OnceSingleAsync<Employee>();
        }
    }
}
