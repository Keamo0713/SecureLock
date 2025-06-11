using Microsoft.AspNetCore.Mvc;
using pleSecureDoc.Services;
using pleSecureDoc.Models;
using System.IO;
using System.Threading.Tasks;
using System;

namespace pleSecureDoc.Controllers
{
    public class FaceController : Controller
    {
        private readonly FirebaseService _firebase;
        private readonly FaceRecognitionService _faceService;

        public FaceController(FirebaseService firebase, FaceRecognitionService faceService)
        {
            _firebase = firebase;
            _faceService = faceService;
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
            await _firebase.SaveEmployerPersonId("kgomotsosele80", personId);

            return View("RegistrationSuccess");
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

            bool isMatch = await _faceService.VerifyFaceAsync(base64Image);

            if (!isMatch)
                return View("FaceVerificationFailed");

            var request = await _firebase.GetRequest(requestId);
            request.IsFaceConfirmed = true;
            await _firebase.LogRequest(request);

            return View("FaceVerificationSuccess");
        }
    }
}
