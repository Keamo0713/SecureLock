using Microsoft.AspNetCore.Mvc;
using pleSecureDoc.Models;
using pleSecureDoc.Services;
using System.IO;
using System.Threading.Tasks;
using System;

namespace pleSecureDoc.Controllers
{
    public class FaceController : Controller
    {
        private readonly FirebaseService _firebase;
        private readonly FaceRecognitionService _faceService;
        private readonly EmailService _emailService; // Added EmailService dependency

        public FaceController(FirebaseService firebase, FaceRecognitionService faceService, EmailService emailService)
        {
            _firebase = firebase;
            _faceService = faceService;
            _emailService = emailService; // Initialize EmailService
        }

        [HttpGet]
        public async Task<IActionResult> StartFaceRecognition(string requestId)
        {
            var request = await _firebase.GetRequest(requestId);
            if (request == null)
                return NotFound("Request not found.");

            return View("StartFaceRecognition", request);
        }

        [HttpGet]
        public IActionResult RegisterEmployerFace() => View();

        [HttpPost]
        public async Task<IActionResult> RegisterEmployerFaceConfirm(string base64Image)
        {
            var personId = await _faceService.RegisterEmployerFaceAsync(base64Image, "kgomotsosele80");

            // Store this personId in Firebase or elsewhere for later verification
            await _firebase.SaveEmployerPersonIdAsync("kgomotsosele80", personId);

            return View("RegistrationSuccess");
        }

        // Assuming this action is for sending a verification email after employer face registration
        // Note: The parameter name 'employeeId' here seems inconsistent with 'RegisterEmployerFaceConfirm(string base64Image)'
        // You might want to rename this action or clarify its purpose.
        [HttpGet] // Changed to HttpGet as it's likely a redirect target or initial view load
        public async Task<IActionResult> RegisterEmployerFaceConfirmationEmail(string employeeId)
        {
            var employee = await _firebase.GetEmployeeAsync(employeeId); // Assumes GetEmployeeAsync exists
            // This email is hardcoded and should be dynamic in a real application.
            var employerEmail = "kgomotsosele80@gmail.com";
            await _emailService.SendFaceVerificationEmail(employerEmail, employee.Id, employee.Email); // Assumes SendFaceVerificationEmail exists
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Confirm(string requestId)
        {
            var file = Request.Form.Files["faceImage"];
            if (file == null || file.Length == 0)
                return BadRequest("No image provided.");

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            var imageBytes = memoryStream.ToArray();
            var base64Image = Convert.ToBase64String(imageBytes);

            string employerId = "kgomotsosele80"; // Note: This should be dynamic.
            var personId = await _firebase.GetEmployerPersonIdAsync(employerId);
            if (string.IsNullOrEmpty(personId))
            {
                return NotFound("Employer's face not registered.");
            }
            
            bool isMatch = await _faceService.VerifyFaceAsync(base64Image, personId);

            if (!isMatch)
                return View("FaceVerificationFailed");

            var request = await _firebase.GetRequest(requestId);
            request.IsFaceConfirmed = true;
            await _firebase.LogRequest(request);

            return View("FaceVerificationSuccess");
        }
    }
}
